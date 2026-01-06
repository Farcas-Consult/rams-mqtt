# Zebra IoT Connector - API Documentation

## Overview

This document describes the REST API endpoints for the Zebra IoT Connector Asset Tracking System. The API provides endpoints for managing assets, gates, movements, and generating reports.

**Base URL:** `http://localhost:5001/api/v1`  
**Swagger UI:** `http://localhost:5001`  
**API Version:** v1

### Terminology

This API uses SAP terminology where applicable:
- **StorageUnit**: A physical location or storage area (referenced as "location" in API field names for backward compatibility, e.g., `currentLocationId`, `locationId`, `fromLocationId`, `toLocationId`)

## Table of Contents

1. [Getting Started](#getting-started)
2. [Authentication](#authentication)
3. [Assets API](#assets-api)
4. [Gates API](#gates-api)
5. [Movements API](#movements-api)
6. [Reports API](#reports-api)
7. [Data Models](#data-models)
8. [Error Handling](#error-handling)
9. [Examples](#examples)

---

## Getting Started

### Prerequisites

- .NET 6.0 SDK or later
- SQL Server (local or remote)
- Docker (optional, for containerized deployment)

### Running the API

**Option 1: Direct Run**
```bash
cd ZebraIoTConnector.Backend.API
dotnet run
```

**Option 2: Docker Compose**
```bash
docker-compose up
```

The API will be available at:
- **HTTP:** `http://localhost:5001`
- **HTTPS:** `https://localhost:5002`
- **Swagger UI:** `http://localhost:5001` (root path)

### Database

The API automatically runs database migrations on startup. Ensure your connection string in `appsettings.json` is correct:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql.data,1433;Database=ZebraRFID_DockDoor;MultipleActiveResultSets=true;User ID=sa;Password=Zebra2022!"
  }
}
```

---

## Authentication

Currently, the API does not require authentication. This may be added in future versions.

---

## Assets API

Base path: `/api/v1/assets`

### Get All Assets

**GET** `/api/v1/assets`

Returns a paginated list of assets with optional filters.

**Query Parameters:**
- `assetNumber` (string, optional) - Filter by asset number
- `name` (string, optional) - Filter by asset name
- `tagIdentifier` (string, optional) - Filter by tag identifier
- `plant` (string, optional) - Filter by plant
- `costCenter` (string, optional) - Filter by cost center
- `currentLocationId` (int, optional) - Filter by current StorageUnit ID
- `daysNotSeen` (int, optional) - Filter by days not seen
- `includeDeleted` (bool, optional) - Include deleted assets (default: false)
- `pageNumber` (int, optional) - Page number (default: 1)
- `pageSize` (int, optional) - Page size (default: 10)

**Response:** 200 OK
```json
[
  {
    "id": 1,
    "assetNumber": "ASSET-001",
    "name": "Asset Name",
    "description": "Asset description",
    "plant": "PLANT-001",
    "costCenter": "CC-001",
    "tagIdentifier": "TAG123",
    "currentLocationId": 1,
    "currentLocationName": "Warehouse 1",
    "lastDiscoveredAt": "2024-01-15T10:30:00Z",
    "lastDiscoveredBy": "Gate 1",
    "createdAt": "2024-01-01T00:00:00Z",
    "isDeleted": false
  }
]
```

**Response Headers:**
- `X-Total-Count` - Total number of assets matching the filter

---

### Get Asset by ID

**GET** `/api/v1/assets/{id}`

Returns a single asset by its ID.

**Path Parameters:**
- `id` (int, required) - Asset ID

**Response:** 200 OK with AssetDto, or 404 Not Found

---

### Get Asset by Tag

**GET** `/api/v1/assets/by-tag/{tagId}`

Returns an asset by its tag identifier.

**Path Parameters:**
- `tagId` (string, required) - Tag identifier (hex format)

**Response:** 200 OK with AssetDto, or 404 Not Found

---

### Create Asset

**POST** `/api/v1/assets`

Creates a new asset.

**Request Body:**
```json
{
  "assetNumber": "ASSET-001",
  "name": "Asset Name",
  "description": "Optional description",
  "materialId": "MAT-001",
  "serialNumber": "SN123456",
  "plant": "PLANT-001",
  "costCenter": "CC-001",
  "tagIdentifier": "TAG123",
  "currentLocationId": 1
}
```

**Required Fields:**
- `assetNumber` (string)
- `name` (string)

**Response:** 201 Created with AssetDto, or 400 Bad Request

---

### Update Asset

**PUT** `/api/v1/assets/{id}`

Updates an existing asset.

**Path Parameters:**
- `id` (int, required) - Asset ID

**Request Body:** UpdateAssetDto (all fields optional)

**Response:** 200 OK with AssetDto, 404 Not Found, or 400 Bad Request

---

### Delete Asset

**DELETE** `/api/v1/assets/{id}`

Soft deletes an asset.

**Path Parameters:**
- `id` (int, required) - Asset ID

**Response:** 204 No Content, or 404 Not Found

---

### Assign Tag to Asset

**POST** `/api/v1/assets/{id}/assign-tag`

Assigns a tag identifier to an asset.

**Path Parameters:**
- `id` (int, required) - Asset ID

**Request Body:**
```json
{
  "tagIdentifier": "TAG123"
}
```

**Response:** 200 OK with AssetDto, 404 Not Found, or 400 Bad Request

---

### Unassign Tag from Asset

**POST** `/api/v1/assets/{id}/unassign-tag`

Removes the tag identifier from an asset.

**Path Parameters:**
- `id` (int, required) - Asset ID

**Response:** 200 OK with AssetDto, or 404 Not Found

---

### Bulk Import Assets

**POST** `/api/v1/assets/bulk-import`

Bulk imports multiple assets from JSON array.

**Request Body:**
```json
[
  {
    "equipment": "ASSET-001",
    "description": "Asset Name 1",
    "materialDescription": "Material description",
    "material": "MAT-001",
    "plant": "PLANT-001",
    "costCtr": "CC-001",
    "manufacturer": "Zebra",
    "purchaseDate": "2023-01-01T00:00:00Z",
    "category": "Scanner",
    "location": "Warehouse A"
  },
  {
    "equipment": "ASSET-002",
    "description": "Asset Name 2",
    "plant": "PLANT-001"
  }
]
```

**Note:** The `currentLocationId` field references a StorageUnit ID. Currently accepts JSON only. CSV/Excel file upload support is planned.

**Response:** 200 OK
```json
{
  "success": true,
  "totalRows": 2,
  "successCount": 2,
  "errorCount": 0,
  "errors": []
}
```

---

## Gates API

Base path: `/api/v1/gates`

### Get All Gates

**GET** `/api/v1/gates`

Returns a list of all gates.

**Response:** 200 OK
```json
[
  {
    "id": 1,
    "name": "Gate 1",
    "description": "Main entrance gate",
    "locationId": 1,
    "locationName": "Warehouse 1",
    "isActive": true,
    "readers": [
      {
        "id": 1,
        "name": "Reader-001",
        "description": "RFID Reader",
        "isMobile": false,
        "isOnline": true,
        "gateId": 1
      }
    ]
  }
]
```

---

### Get Gate by ID

**GET** `/api/v1/gates/{id}`

Returns a single gate by its ID.

**Path Parameters:**
- `id` (int, required) - Gate ID

**Response:** 200 OK with GateDto, or 404 Not Found

---

### Create Gate

**POST** `/api/v1/gates`

Creates a new gate.

**Request Body:**
```json
{
  "name": "Gate 1",
  "description": "Main entrance gate",
  "locationId": 1,
  "isActive": true
}
```

**Required Fields:**
- `name` (string)

**Optional Fields:**
- `description` (string)
- `locationId` (int) - Must reference existing StorageUnit
- `isActive` (bool, default: true)

**Response:** 201 Created with GateDto, or 400 Bad Request

**Error Responses:**
- 400 Bad Request - "Name is required" or "Gate with name 'X' already exists"

---

### Update Gate

**PUT** `/api/v1/gates/{id}`

Updates an existing gate.

**Path Parameters:**
- `id` (int, required) - Gate ID

**Request Body:** UpdateGateDto (all fields optional)

**Response:** 200 OK with GateDto, 404 Not Found, or 400 Bad Request

---

### Assign Reader to Gate

**POST** `/api/v1/gates/{gateId}/readers/{readerId}`

Assigns a reader (equipment) to a gate.

**Path Parameters:**
- `gateId` (int, required) - Gate ID
- `readerId` (int, required) - Reader (Equipment) ID

**Response:** 204 No Content, 404 Not Found, or 400 Bad Request

---

### Remove Reader from Gate

**DELETE** `/api/v1/gates/{gateId}/readers/{readerId}`

Removes a reader from a gate.

**Path Parameters:**
- `gateId` (int, required) - Gate ID
- `readerId` (int, required) - Reader (Equipment) ID

**Response:** 204 No Content, or 404 Not Found

---

## Movements API

Base path: `/api/v1/movements`

### Get Movements

**GET** `/api/v1/movements`

Returns a list of asset movements with optional filters.

**Query Parameters:**
- `fromDate` (DateTime, required) - Start date (default: 7 days ago)
- `toDate` (DateTime, required) - End date (default: now)
- `assetId` (int, optional) - Filter by asset ID
- `gateId` (int, optional) - Filter by gate ID
- `pageNumber` (int, optional) - Page number (default: 1)
- `pageSize` (int, optional) - Page size (default: 10)

**Response:** 200 OK
```json
[
  {
    "id": 1,
    "assetId": 1,
    "assetNumber": "ASSET-001",
    "assetName": "Asset Name",
    "fromLocationId": 1,
    "fromLocationName": "Warehouse 1",
    "toLocationId": 2,
    "toLocationName": "Warehouse 2",
    "gateId": 1,
    "gateName": "Gate 1",
    "readerId": 1,
    "readerName": "Reader-001",
    "readTimestamp": "2024-01-15T10:30:00Z"
  }
]
```

---

### Get Movement by ID

**GET** `/api/v1/movements/{id}`

Returns a single movement by its ID.

**Path Parameters:**
- `id` (int, required) - Movement ID

**Response:** 200 OK with AssetMovementDto, or 404 Not Found

---

## Reports API

Base path: `/api/v1/reports`

### Location Report

**GET** `/api/v1/reports/location`

Returns assets grouped by StorageUnit.

**Query Parameters:**
- `locationId` (int, optional) - Filter by StorageUnit ID
- `plant` (string, optional) - Filter by plant
- `costCenter` (string, optional) - Filter by cost center
- `pageNumber` (int, optional) - Page number (default: 1)
- `pageSize` (int, optional) - Page size (default: 10)

**Response:** 200 OK with list of AssetDto

---

### Movement Report

**GET** `/api/v1/reports/movement`

Returns movement history with filters.

**Query Parameters:**
- `fromDate` (DateTime, required) - Start date
- `toDate` (DateTime, required) - End date
- `assetId` (int, optional) - Filter by asset ID
- `gateId` (int, optional) - Filter by gate ID
- `pageNumber` (int, optional) - Page number (default: 1)
- `pageSize` (int, optional) - Page size (default: 10)

**Response:** 200 OK with list of AssetMovementDto

---

### Discovery Report

**GET** `/api/v1/reports/discovery?daysNotSeen=30`

Returns assets that haven't been seen in the specified number of days.

**Query Parameters:**
- `daysNotSeen` (int, optional) - Days not seen (default: 30)

**Response:** 200 OK with list of AssetDto

---

### Gate Activity Report

**GET** `/api/v1/reports/gate-activity?gateId=1&from=2024-01-01&to=2024-12-31`

Returns all movements through a specific gate within a date range.

**Query Parameters:**
- `gateId` (int, required) - Gate ID
- `from` (DateTime, required) - Start date
- `to` (DateTime, required) - End date

**Response:** 200 OK with list of AssetMovementDto, or 400 Bad Request if dates are invalid

---

### Asset Statistics

**GET** `/api/v1/reports/statistics`

Returns aggregate statistics about assets, gates, and movements.

**Response:** 200 OK
```json
{
  "totalAssets": 100,
  "activeAssets": 95,
  "decommissionedAssets": 5,
  "assetsWithTags": 80,
  "assetsWithoutTags": 20,
  "assetsNotSeen30Days": 5,
  "assetsNotSeen90Days": 2,
  "totalGates": 5,
  "activeGates": 4,
  "totalReaders": 10,
  "onlineReaders": 8,
  "totalMovementsLast24Hours": 50
}
```

---

## Data Models

### AssetDto

```typescript
{
  id: number;
  assetNumber: string;
  name: string;
  description?: string;
  materialId?: string;
  serialNumber?: string;
  technicalId?: string;
  plant?: string;
  storageLocation?: string;
  costCenter?: string;
  assetGroup?: string;
  businessArea?: string;
  objectType?: string;
  systemStatus?: string;
  userStatus?: string;
  acquisitionValue?: number;
  comments?: string;
  tagIdentifier?: string;
  lastDiscoveredAt?: DateTime;
  lastDiscoveredBy?: string;
  currentLocationId?: number;      // References StorageUnit ID
  currentLocationName?: string;    // StorageUnit name
  createdAt: DateTime;
  updatedAt?: DateTime;
  isDeleted: boolean;
}
```

### CreateAssetDto

```typescript
{
  assetNumber: string;        // Required
  name: string;               // Required
  description?: string;
  materialId?: string;
  serialNumber?: string;
  technicalId?: string;
  plant?: string;
  storageLocation?: string;
  costCenter?: string;
  assetGroup?: string;
  businessArea?: string;
  objectType?: string;
  systemStatus?: string;
  userStatus?: string;
  acquisitionValue?: number;
  comments?: string;
  tagIdentifier?: string;
  currentLocationId?: number;      // References StorageUnit ID
}
```

### GateDto

```typescript
{
  id: number;
  name: string;
  description?: string;
  locationId?: number;             // References StorageUnit ID
  locationName?: string;           // StorageUnit name
  isActive: boolean;
  readers: ReaderDto[];
}
```

### CreateGateDto

```typescript
{
  name: string;               // Required
  description?: string;
  locationId?: number;        // References StorageUnit ID
  isActive?: boolean;         // Default: true
}
```

### AssetMovementDto

```typescript
{
  id: number;
  assetId: number;
  assetNumber?: string;
  assetName?: string;
  fromLocationId?: number;         // Source StorageUnit ID
  fromLocationName?: string;       // Source StorageUnit name
  toLocationId: number;            // Destination StorageUnit ID
  toLocationName?: string;         // Destination StorageUnit name
  gateId?: number;
  gateName?: string;
  readerId?: number;
  readerName?: string;
  readerIdString?: string;
  readTimestamp: DateTime;
}
```

### AssetStatisticsDto

```typescript
{
  totalAssets: number;
  activeAssets: number;
  decommissionedAssets: number;
  assetsWithTags: number;
  assetsWithoutTags: number;
  assetsNotSeen30Days: number;
  assetsNotSeen90Days: number;
  totalGates: number;
  activeGates: number;
  totalReaders: number;
  onlineReaders: number;
  totalMovementsLast24Hours: number;
}
```

---

## Error Handling

The API uses standard HTTP status codes:

- **200 OK** - Request succeeded
- **201 Created** - Resource created successfully
- **204 No Content** - Request succeeded, no content to return
- **400 Bad Request** - Invalid request (validation errors, duplicate names, etc.)
- **404 Not Found** - Resource not found
- **500 Internal Server Error** - Server error

### Error Response Format

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "detail": "Detailed error message here"
}
```

For validation errors (400), the response may include ModelState errors:
```json
{
  "errors": {
    "name": ["Name is required"],
    "assetNumber": ["AssetNumber already exists"]
  }
}
```

---

## Examples

### Example 1: Create a Gate

```bash
curl -X POST "http://localhost:5001/api/v1/gates" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Gate 1",
    "description": "Main entrance gate",
    "isActive": true
  }'
```

### Example 2: Create an Asset

```bash
curl -X POST "http://localhost:5001/api/v1/assets" \
  -H "Content-Type: application/json" \
  -d '{
    "assetNumber": "ASSET-001",
    "name": "Laptop Computer",
    "plant": "PLANT-001",
    "costCenter": "CC-001"
  }'
```

### Example 3: Get Asset Statistics

```bash
curl -X GET "http://localhost:5001/api/v1/reports/statistics" \
  -H "accept: application/json"
```

### Example 4: Get Assets by StorageUnit

```bash
curl -X GET "http://localhost:5001/api/v1/assets?currentLocationId=1&pageNumber=1&pageSize=20" \
  -H "accept: application/json"
```

### Example 5: Assign Tag to Asset

```bash
curl -X POST "http://localhost:5001/api/v1/assets/1/assign-tag" \
  -H "Content-Type: application/json" \
  -d '{
    "tagIdentifier": "TAG123456"
  }'
```

### Example 6: Get Movement Report

```bash
curl -X GET "http://localhost:5001/api/v1/reports/movement?fromDate=2024-01-01T00:00:00Z&toDate=2024-12-31T23:59:59Z&gateId=1" \
  -H "accept: application/json"
```

---

## Swagger UI

The easiest way to explore and test the API is using Swagger UI:

1. Start the API: `dotnet run` or `docker-compose up`
2. Navigate to: `http://localhost:5001`
3. Use the "Try it out" buttons to test endpoints
4. View request/response schemas directly in the UI

---

## Notes for Developers

### Current Limitations

1. **Bulk Import:** Currently accepts JSON only. CSV/Excel file upload support is planned.
2. **Authentication:** Not yet implemented. All endpoints are publicly accessible.
3. **Pagination:** Some endpoints support pagination, but not all.

### Future Enhancements

- CSV/Excel file upload for bulk asset import
- JWT authentication
- Rate limiting
- Webhook support
- SignalR real-time updates (already implemented, needs client integration)

---

## Support

For issues or questions:
1. Check the application logs for detailed error messages
2. Review the Swagger UI for endpoint documentation
3. Refer to the troubleshooting guide: `TROUBLESHOOTING.md`

---

**Last Updated:** December 2024  
**API Version:** 1.0

