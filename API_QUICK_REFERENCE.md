# API Quick Reference Guide

**Base URL:** `http://localhost:5001/api/v1`  
**Swagger UI:** `http://localhost:5001`

## Assets

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/assets` | List all assets (with filters) |
| GET | `/assets/{id}` | Get asset by ID |
| GET | `/assets/by-tag/{tagId}` | Get asset by tag identifier |
| POST | `/assets` | Create new asset |
| PUT | `/assets/{id}` | Update asset |
| DELETE | `/assets/{id}` | Delete asset (soft) |
| POST | `/assets/{id}/assign-tag` | Assign tag to asset |
| POST | `/assets/{id}/unassign-tag` | Unassign tag |
| POST | `/assets/bulk-import` | Bulk import assets (JSON) |

## Gates

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/gates` | List all gates |
| GET | `/gates/{id}` | Get gate by ID |
| POST | `/gates` | Create new gate |
| PUT | `/gates/{id}` | Update gate |
| POST | `/gates/{gateId}/readers/{readerId}` | Assign reader to gate |
| DELETE | `/gates/{gateId}/readers/{readerId}` | Remove reader from gate |

## Movements

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/movements` | List movements (with filters) |
| GET | `/movements/{id}` | Get movement by ID |

## Reports

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/reports/location` | Location report |
| GET | `/reports/movement` | Movement report |
| GET | `/reports/discovery?daysNotSeen=30` | Discovery report |
| GET | `/reports/gate-activity?gateId=1&from=...&to=...` | Gate activity report |
| GET | `/reports/statistics` | Asset statistics |

## Common Request Examples

### Create Gate
```json
POST /api/v1/gates
{
  "name": "Gate 1",
  "description": "Main entrance",
  "isActive": true
}
```

### Create Asset
```json
POST /api/v1/assets
{
  "assetNumber": "ASSET-001",
  "name": "Asset Name",
  "plant": "PLANT-001"
}
```

### Get Statistics
```
GET /api/v1/reports/statistics
```

## Response Codes

- **200** - Success
- **201** - Created
- **204** - No Content
- **400** - Bad Request (validation error)
- **404** - Not Found
- **500** - Server Error

## Required Fields

**Create Gate:**
- `name` (string)

**Create Asset:**
- `assetNumber` (string)
- `name` (string)

All other fields are optional.

