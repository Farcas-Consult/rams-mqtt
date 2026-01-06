# API Testing Guide

## Quick Start

### 1. Run the API

```bash
cd ZebraIoTConnector.Backend.API
dotnet run
```

The API will start on:
- HTTP: `http://localhost:5001`
- HTTPS: `https://localhost:5002`
- Swagger UI: `http://localhost:5001` (root URL, since RoutePrefix is empty)

### 2. Access Swagger UI

Open your browser and navigate to:
```
http://localhost:5001
```

You'll see the Swagger UI with all available endpoints.

## Available Endpoints

### Assets API (`/api/v1/assets`)

- `GET /api/v1/assets` - List all assets (with filters)
- `GET /api/v1/assets/{id}` - Get asset by ID
- `GET /api/v1/assets/by-tag/{tagId}` - Get asset by tag identifier
- `POST /api/v1/assets` - Create new asset
- `PUT /api/v1/assets/{id}` - Update asset
- `DELETE /api/v1/assets/{id}` - Delete asset (soft delete)
- `POST /api/v1/assets/{id}/assign-tag` - Assign tag to asset
- `POST /api/v1/assets/{id}/unassign-tag` - Unassign tag
- `POST /api/v1/assets/bulk-import` - Bulk import assets

### Gates API (`/api/v1/gates`)

- `GET /api/v1/gates` - List all gates
- `GET /api/v1/gates/{id}` - Get gate by ID
- `POST /api/v1/gates` - Create new gate
- `PUT /api/v1/gates/{id}` - Update gate
- `POST /api/v1/gates/{gateId}/readers/{readerId}` - Assign reader to gate
- `DELETE /api/v1/gates/{gateId}/readers/{readerId}` - Remove reader from gate

### Movements API (`/api/v1/movements`)

- `GET /api/v1/movements` - List movements (with filters)
- `GET /api/v1/movements/{id}` - Get movement by ID

### Reports API (`/api/v1/reports`)

- `GET /api/v1/reports/location` - Location report
- `GET /api/v1/reports/movement` - Movement report
- `GET /api/v1/reports/discovery?daysNotSeen=30` - Discovery report
- `GET /api/v1/reports/gate-activity?gateId=1&from=2024-01-01&to=2024-12-31` - Gate activity report
- `GET /api/v1/reports/statistics` - Asset statistics

## Testing with cURL

### 1. Get All Assets
```bash
curl -X GET "http://localhost:5001/api/v1/assets" \
  -H "accept: application/json"
```

### 2. Get Asset Statistics
```bash
curl -X GET "http://localhost:5001/api/v1/reports/statistics" \
  -H "accept: application/json"
```

### 3. Create a New Asset
```bash
curl -X POST "http://localhost:5001/api/v1/assets" \
  -H "accept: application/json" \
  -H "Content-Type: application/json" \
  -d '{
    "assetNumber": "ASSET-001",
    "name": "Test Asset",
    "description": "Test description",
    "plant": "PLANT-001",
    "costCenter": "CC-001"
  }'
```

### 4. Get All Gates
```bash
curl -X GET "http://localhost:5001/api/v1/gates" \
  -H "accept: application/json"
```

### 5. Create a New Gate
```bash
curl -X POST "http://localhost:5001/api/v1/gates" \
  -H "accept: application/json" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Gate 1",
    "description": "Main entrance gate",
    "isActive": true
  }'
```

### 6. Get Discovery Report (assets not seen in 30 days)
```bash
curl -X GET "http://localhost:5001/api/v1/reports/discovery?daysNotSeen=30" \
  -H "accept: application/json"
```

## Testing with Postman

1. Import the API:
   - Open Postman
   - Click "Import"
   - Select "Link" tab
   - Enter: `http://localhost:5001/swagger/v1/swagger.json`
   - Click "Continue" and "Import"

2. All endpoints will be available in Postman with proper request formats.

## Testing with HTTP Client (VS Code)

Create a file `test-api.http`:

```http
### Get Statistics
GET http://localhost:5001/api/v1/reports/statistics
Accept: application/json

### Get All Assets
GET http://localhost:5001/api/v1/assets
Accept: application/json

### Create Asset
POST http://localhost:5001/api/v1/assets
Content-Type: application/json
Accept: application/json

{
  "assetNumber": "ASSET-001",
  "name": "Test Asset",
  "plant": "PLANT-001"
}

### Get All Gates
GET http://localhost:5001/api/v1/gates
Accept: application/json
```

## Prerequisites

Before testing, ensure:

1. **Database is running** (SQL Server):
   - Connection string in `appsettings.json` should point to your database
   - Migrations will run automatically on startup

2. **MQTT Broker (optional for API testing)**:
   - The API will start even if MQTT broker is not available
   - MQTT features (live feed, tag reading) won't work without it
   - For API endpoint testing, MQTT is not required

## Common Test Scenarios

### Scenario 1: Create Asset and View It
1. POST `/api/v1/assets` - Create asset
2. GET `/api/v1/assets/{id}` - View created asset
3. GET `/api/v1/assets` - List all assets

### Scenario 2: Create Gate and Assign Reader
1. POST `/api/v1/gates` - Create gate
2. POST `/api/v1/gates/{gateId}/readers/{readerId}` - Assign reader
3. GET `/api/v1/gates/{id}` - View gate with readers

### Scenario 3: View Reports
1. GET `/api/v1/reports/statistics` - Get overview
2. GET `/api/v1/reports/discovery?daysNotSeen=30` - Find missing assets
3. GET `/api/v1/reports/location` - View location report

## Troubleshooting

### API won't start
- Check if port 5001/5002 is already in use
- Check database connection string in `appsettings.json`
- Check logs for errors

### 404 Not Found
- Ensure you're using the correct base path: `/api/v1/...`
- Check Swagger UI to verify endpoint paths

### 500 Internal Server Error
- Check application logs
- Verify database connection
- Ensure database migrations have run successfully

### MQTT Connection Warnings
- These are expected if MQTT broker is not running
- API endpoints will still work without MQTT
- To test MQTT features, start the mosquitto broker

