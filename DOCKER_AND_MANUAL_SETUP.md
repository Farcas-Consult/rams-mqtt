# Docker Setup and Manual Configuration Guide

## Question 1: Running on Docker with Localhost Access

**Yes!** You can run the API on Docker and still access it via localhost. The `docker-compose.yml` already has port mapping configured:

```yaml
ports:
  - "5001:80"    # Maps container port 80 to host port 5001
  - "5002:443"   # Maps container port 443 to host port 5002
```

### To run with Docker:

```bash
# Start all services (API, SQL Server, Mosquitto)
docker-compose up

# Or run in background
docker-compose up -d
```

**Access the API:**
- Swagger UI: `http://localhost:5001`
- API endpoints: `http://localhost:5001/api/v1/...`
- HTTPS: `https://localhost:5002`

The ports are already mapped, so you can access the API on `localhost` even though it's running in Docker!

---

## Question 2: Adding Gates Manually (No UI)

Since there's no UI, you have two options:

### Option A: Using the API (Recommended)

Use cURL, Postman, or Swagger UI to create gates:

**Via Swagger UI:**
1. Start the API
2. Go to `http://localhost:5001` (or `http://localhost:5200` if using Docker)
3. Find `POST /api/v1/gates`
4. Click "Try it out"
5. Enter gate data:
```json
{
  "name": "Gate 1",
  "description": "Main entrance gate",
  "locationId": 1,
  "isActive": true
}
```
6. Click "Execute"

**Via cURL:**
```bash
curl -X POST "http://localhost:5001/api/v1/gates" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Gate 1",
    "description": "Main entrance gate",
    "locationId": 1,
    "isActive": true
  }'
```

**Via SQL (Direct Database):**
```sql
-- First, ensure you have a Location (StorageUnit) with ID 1
-- If not, create one:
INSERT INTO StorageUnits (Name, Description) 
VALUES ('Warehouse 1', 'Main Warehouse');

-- Then create the gate:
INSERT INTO Gates (Name, Description, LocationId, IsActive, CreatedAt)
VALUES ('Gate 1', 'Main entrance gate', 1, 1, GETUTCDATE());
```

### Creating Multiple Gates (Batch Script)

Create a file `create-gates.sh`:

```bash
#!/bin/bash

API_URL="http://localhost:5001"

# Gate 1
curl -X POST "${API_URL}/api/v1/gates" \
  -H "Content-Type: application/json" \
  -d '{"name":"Gate 1","description":"Main entrance","isActive":true}'

# Gate 2
curl -X POST "${API_URL}/api/v1/gates" \
  -H "Content-Type: application/json" \
  -d '{"name":"Gate 2","description":"Secondary gate","isActive":true}'

# Gate 3
curl -X POST "${API_URL}/api/v1/gates" \
  -H "Content-Type: application/json" \
  -d '{"name":"Gate 3","description":"Loading dock","isActive":true}'
```

Or use a JSON file with multiple gates (you'd need to loop through them).

---

## Question 3: CSV/Excel Import for Assets

**Current Status:** The bulk import endpoint currently accepts **JSON only**, not CSV/Excel files.

The endpoint signature is:
```csharp
POST /api/v1/assets/bulk-import
Content-Type: application/json
Body: List<BulkImportAssetDto>
```

### Current Implementation (JSON Only):

```bash
curl -X POST "http://localhost:5001/api/v1/assets/bulk-import" \
  -H "Content-Type: application/json" \
  -d '[
    {
      "equipment": "ASSET-001",
      "description": "Asset Name 1",
      "materialDescription": "Material Desc",
      "plant": "PLANT-001",
      "costCtr": "CC-001"
    },
    {
      "equipment": "ASSET-002",
      "description": "Asset Name 2",
      "plant": "PLANT-001"
    }
  ]'
```

### To Add CSV/Excel Support:

According to Task 3.3 in WORK_BREAKDOWN.md, CSV/Excel file upload support needs to be implemented. This requires:

1. **Add file upload endpoint** that accepts `multipart/form-data`
2. **Add CSV/Excel parsing library** (e.g., `CsvHelper` or `EPPlus`)
3. **Parse file and convert to `BulkImportAssetDto` list**
4. **Call existing bulk import logic**

### Quick Workaround - Convert CSV to JSON:

Until CSV/Excel support is implemented, you can:

1. **Export your CSV/Excel to JSON** using:
   - Excel: Save as JSON (if supported) or use online converter
   - Python script to convert CSV to JSON
   - Online CSV to JSON converter

2. **Use the JSON with the existing endpoint**

**Example Python script to convert CSV to JSON:**

```python
import csv
import json

csv_file = 'assets.csv'
json_file = 'assets.json'

assets = []
with open(csv_file, 'r') as f:
    reader = csv.DictReader(f)
    for row in reader:
        assets.append({
            "equipment": row.get('AssetNumber', ''),
            "description": row.get('Name', ''),
            "materialDescription": row.get('Description', ''),
            "material": row.get('MaterialId', ''),
            "plant": row.get('Plant', ''),
            "costCtr": row.get('CostCenter', ''),
            # Add other fields as needed
        })

with open(json_file, 'w') as f:
    json.dump(assets, f, indent=2)
```

Then use the JSON file with the API.

---

## Summary

1. **Docker + Localhost:** ✅ Yes, works via port mapping (currently 5200:80, can change to 5001:80)
2. **Adding Gates:** Use API endpoint `POST /api/v1/gates` via Swagger, cURL, or Postman
3. **CSV/Excel Import:** ❌ Not yet implemented - currently JSON only. Use CSV→JSON converter as workaround.

Would you like me to implement CSV/Excel file upload support now, or would you prefer to use the JSON workaround for now?

