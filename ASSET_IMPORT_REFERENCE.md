# Asset Import Format Reference

## Import Column Mapping

This document provides a detailed mapping between the SAP export columns and the Asset entity fields.

### Column Mapping Table

| Import Column | Asset Entity Field | Data Type | Required | Notes |
|--------------|-------------------|-----------|----------|-------|
| **Equipment** | `AssetNumber` | `string` (100) | ✅ Yes | Unique identifier, indexed |
| **Description** | `Name` | `string` (200) | ✅ Yes | Main asset name |
| **Material Description** | `Description` | `string` (500) | ❌ No | Detailed description |
| **Material** | `MaterialId` | `string` (50) | ❌ No | Material ID from SAP |
| **ManufSerialNumber** | `SerialNumber` | `string` (100) | ❌ No | Manufacturer serial number |
| **TechIdentNo.** | `TechnicalId` | `string` (100) | ❌ No | Technical identification |
| **Plnt** | `Plant` | `string` (10) | ❌ No | Plant code (e.g., KE10) |
| **SLoc** | `StorageLocation` | `string` (10) | ❌ No | Storage location code |
| **Cost Ctr** | `CostCenter` | `string` (20) | ❌ No | Cost center code |
| **AGrp** | `AssetGroup` | `string` (10) | ❌ No | Asset group code |
| **BusA** | `BusinessArea` | `string` (10) | ❌ No | Business area code |
| **ObjectType** | `ObjectType` | `string` (20) | ❌ No | Object type code |
| **SysStatus** | `SystemStatus` | `string` (20) | ❌ No | System status (e.g., ESTO) |
| **UserStatus** | `UserStatus` | `string` (50) | ❌ No | User status codes (comma-separated if multiple) |
| **AcquistnValue** | `AcquisitionValue` | `decimal(18,2)` | ❌ No | Acquisition value |
| **Comment** | `Comments` | `string` (1000) | ❌ No | Additional comments |
| - | `TagIdentifier` | `string` (100) | ❌ No | RFID tag EPC (assigned later, unique) |
| - | `LastDiscoveredAt` | `DateTime?` | ❌ No | Auto-updated on tag read |
| - | `LastDiscoveredBy` | `string` (100) | ❌ No | Gate/reader name |
| - | `CurrentLocationId` | `int?` | ❌ No | FK to StorageUnit |
| - | `CreatedAt` | `DateTime` | ✅ Yes | Auto-set on creation |
| - | `UpdatedAt` | `DateTime?` | ❌ No | Auto-set on update |
| - | `IsDeleted` | `bool` | ✅ Yes | Soft delete flag (default: false) |

### Sample Import Data

```csv
Equipment,Description,Material Description,Material,ManufSerialNumber,TechIdentNo.,Plnt,SLoc,Cost Ctr,AGrp,BusA,ObjectType,SysStatus,UserStatus,AcquistnValue,Comment
100001,NETWORK ATTACHED STORAGE,Storage:Array Disc Entry,MAT001,SN123456789,TECH001,KE10,2203,CC001,T00,P017,OT001,ESTO,"EQID,WRPR",50000.00,Production server
100002,LAPTOP DELL XPS 15,Computer:Laptop,MAT002,SN987654321,TECH002,KE10,2203,CC001,T00,P017,OT002,ESTO,EQID,1500.00,Development laptop
```

### Import Validation Rules

#### Required Fields
- ✅ **Equipment** (AssetNumber) - Must be unique, cannot be null/empty
- ✅ **Description** (Name) - Cannot be null/empty

#### Validation Rules
1. **AssetNumber (Equipment)**
   - Must be unique across all assets
   - Cannot be null or empty
   - Max length: 100 characters
   - If duplicate found, import will fail for that row

2. **Name (Description)**
   - Cannot be null or empty
   - Max length: 200 characters

3. **TagIdentifier**
   - Not imported (assigned later via UI or mobile app)
   - Must be unique when assigned
   - Format: EPC hex string (e.g., `E20034120107000000000001`)

4. **AcquisitionValue**
   - Must be a valid decimal number
   - Cannot be negative
   - If invalid, default to 0 or skip field

5. **UserStatus**
   - Can contain multiple values separated by comma
   - Max length: 50 characters per value
   - Example: `"EQID,WRPR"` → stored as `"EQID,WRPR"`

### Import Process Flow

```
1. User uploads CSV/Excel file
   ↓
2. Parse file and validate format
   ↓
3. For each row:
   a. Validate required fields
   b. Check for duplicate AssetNumber
   c. Validate data types
   d. Create AssetDto object
   ↓
4. Batch insert into database (transaction)
   ↓
5. Return import results:
   - Success count
   - Error count
   - List of errors (row number + error message)
```

### Import Error Handling

#### Common Errors

| Error | Cause | Resolution |
|-------|-------|------------|
| `AssetNumber is required` | Equipment column is empty | Fill in Equipment column |
| `AssetNumber already exists` | Duplicate asset number | Use different asset number or update existing |
| `Name is required` | Description column is empty | Fill in Description column |
| `Invalid AcquisitionValue` | Non-numeric value | Use valid decimal number |
| `File format error` | Invalid CSV/Excel format | Check file format and headers |

#### Error Response Format

```json
{
  "success": false,
  "totalRows": 100,
  "successCount": 95,
  "errorCount": 5,
  "errors": [
    {
      "rowNumber": 3,
      "assetNumber": "100003",
      "error": "AssetNumber already exists"
    },
    {
      "rowNumber": 15,
      "assetNumber": "",
      "error": "AssetNumber is required"
    }
  ]
}
```

### CSV Format Example

```csv
Equipment,Description,Material Description,Material,ManufSerialNumber,TechIdentNo.,Plnt,SLoc,Cost Ctr,AGrp,BusA,ObjectType,SysStatus,UserStatus,AcquistnValue,Comment
100001,NETWORK ATTACHED STORAGE,Storage:Array Disc Entry,MAT001,SN123456789,TECH001,KE10,2203,CC001,T00,P017,OT001,ESTO,"EQID,WRPR",50000.00,Production server
100002,LAPTOP DELL XPS 15,Computer:Laptop,MAT002,SN987654321,TECH002,KE10,2203,CC001,T00,P017,OT002,ESTO,EQID,1500.00,Development laptop
```

### Excel Format Requirements

- **Sheet Name:** Any (first sheet will be used)
- **Headers:** Must match column names exactly (case-insensitive)
- **Data Types:** 
  - Equipment, Description: Text
  - AcquisitionValue: Number
  - All other fields: Text

### Post-Import Steps

After successful import:

1. **Tag Assignment** (Manual or via Mobile App)
   - Assign RFID tags to imported assets
   - Tags can be assigned individually or in bulk

2. **Location Assignment** (Optional)
   - Set initial location for assets
   - Can be done via UI or automatically on first tag read

3. **Verification**
   - Review imported assets
   - Check for any data issues
   - Update missing information if needed

### API Endpoint

```
POST /api/v1/assets/bulk-import
Content-Type: multipart/form-data

Request:
- file: CSV/Excel file

Response:
{
  "success": true,
  "totalRows": 100,
  "successCount": 100,
  "errorCount": 0,
  "errors": []
}
```

### Notes

- **Tag Assignment:** Tags are NOT imported. They must be assigned separately via:
  - Web UI (Asset detail page)
  - Mobile app (Tagging mode)
  - API endpoint: `POST /api/v1/assets/{id}/assign-tag`

- **Owner Tracking:** Currently not tracked. Can be added later if needed.

- **Location:** Initial location can be set during import or left null (will be set on first tag read).

- **Soft Delete:** Imported assets use soft delete (`IsDeleted` flag), so they can be restored if needed.

---

**Last Updated:** [Current Date]  
**Version:** 1.0

