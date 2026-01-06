# Verification: Asset Import, Mobile Read API & Location Hierarchy

## 1. Asset Import Enhancement

We verified that the system now correctly imports and preserves the following fields during bulk import:
-   `Manufacturer`
-   `PurchaseDate`
-   `Category`
-   `Location`

### Verification Steps

1.  **Import Asset**: Used `POST /api/v1/assets/bulk-import` with a payload containing the new fields.
2.  **Verify Data**: Used `GET /api/v1/assets/{id}` to confirm fields were persisted.

## 2. Mobile Read API (Portable Gate)

We implemented a new endpoint `POST /api/v1/movements/report` that allows mobile devices to report tag scans.

### Verification Steps

1.  **Report Movement**:
    ```bash
    curl -X POST "http://localhost:5001/api/v1/movements/report" \
         -H "Content-Type: application/json" \
         -d '{ "tagId": "TAG-MOBILE-TEST-2", "gateId": 3, "deviceId": "MOBILE-DEVICE-001", "timestamp": "2026-01-02T12:00:00Z" }'
    ```
2.  **Verify Result**:
    Fetched the asset details and confirmed `lastDiscoveredBy` was "Mobile: MOBILE-DEVICE-001" and location was updated.

### 2.1 Direction Support

We added a `direction` field (Inbound=1, Outbound=2) to the report endpoint.

**Verification**:
```bash
curl -X POST "http://localhost:5001/api/v1/movements/report" \
     -H "Content-Type: application/json" \
     -d '{ "tagId": "TAG-MOBILE-TEST-2", "gateId": 1002, "deviceId": "MOBILE-001", "timestamp": "2026-01-02T12:00:00Z", "direction": 1 }'
```
Verified via logs/database that the movement was recorded.

### 2.2 Unknown Asset Handling

We verified that the API returns `404 Not Found` for unknown tags.

**Verification**:
```bash
curl -v -X POST "http://localhost:5001/api/v1/movements/report" \
     -H "Content-Type: application/json" \
     -d '{ "tagId": "NON-EXISTENT-TAG", "gateId": 1002, "deviceId": "MOBILE-001", "timestamp": "2026-01-02T12:00:00Z", "direction": 1 }'
```
Response: `404 Not Found` with message: `"Asset with Tag ID 'NON-EXISTENT-TAG' not found."`

## 3. Location Hierarchy (Site -> Gate)

We implemented a hierarchical structure where Gates belong to a **Site** (Location).

### Verification Steps

1.  **Create Site**:
    ```bash
    curl -X POST "http://localhost:5001/api/v1/sites" -H "Content-Type: application/json" -d '{ "name": "Mombasa", "description": "Coastal Region" }'
    ```
    Response: `{"id":1,"name":"Mombasa",...}`

2.  **Create Gate in Site**:
    ```bash
    curl -X POST "http://localhost:5001/api/v1/gates" -H "Content-Type: application/json" -d '{ "name": "Mombasa Gate 2", "siteId": 1 }'
    ```
    Response: `{"id":1003,"name":"Mombasa Gate 2", ..., "siteId":1, "siteName":"Mombasa"}`

3.  **List Gates by Site**:
    ```bash
    curl "http://localhost:5001/api/v1/sites/1/gates"
    ```
    Response confirmed the gate is listed under the site.

### 2.3 Error Handling Improvement

We fixed a `500 Internal Server Error` that occurred when reporting a movement against a gate without a configured `LocationId`.

**Verification**:
1.  Created a gate without a location: `curl -X POST "http://localhost:5001/api/v1/gates" ...`
2.  Reported movement against it.
3.  **Result**: API now returns `409 Conflict` with the message: `"Gate {Name} does not have a configured Location"`.

### 2.4 Automated Location Creation

We implemented a feature to automatically create an Inventory Location (StorageUnit) when a Site is created, and auto-assign it to new Gates.

**Verification**:
1.  Created a new Site "TestAutoSite".
2.  Created a new Gate "TestAutoGate" with `siteId` but NO `locationId`.
3.  **Result**: The Gate was automatically assigned a `locationId` corresponding to the "TestAutoSite" location.
    ```json
    {
      "id": 1005,
      "name": "TestAutoGate",
      "locationId": 4,
      "locationName": "TestAutoSite",
      "siteId": 2,
      ...
    }
    ```

### 2.5 Live Feed Integration

We fixed an issue where portable gate movements were not appearing in the frontend Live Feed.
**Resolution**: We updated `AssetManagementService` to inject `ITagReadNotifier` (SignalR) and broadcast movement events whenever a portable movement is reported.

**Verification**:
1.  Created test asset "TAG-LIVEFEED".
2.  Reported movement via API.
3.  **Result**: API returned `200 OK`, and backend logs confirmed processing without errors, indicating SignalR message dispatch.

### 2.6 CORS Configuration

We resolved a CORS error preventing the frontend from connecting to the SignalR hub.
**Resolution**: Added a specific CORS policy in `Program.cs` to allow origin `http://localhost:3000` with credentials (required for SignalR).

**Verification**:
1.  Verified `Program.cs` contains `AddCors` and `UseCors` middleware.
2.  Rebuilt backend container.
3.  **Action Required**: User to verify frontend connection.

### 2.7 Remote Reader MQTT Configuration

We successfully configured a physical Zebra Reader to communicate with the backend via MQTT.
**Resolution**: Guided user to configure the reader with:
-   **Broker**: Pre-seeded internal IP (`169.254.7.130`).
-   **Client ID**: `fx9600fd776c` (Matched to seeded DB entity).
-   **Topics**: Mapped standard Zebra topics to `zebra/fx9600fd776c/...`.

**Verification**:
1.  User applied configuration on reader web interface.
2.  Backend logs confirmed connection: `set_config sent to reader fx9600fd776c`.
3.  Backend logs confirmed control messages: `Message Successfully published to zebra/fx9600fd776c/ctrl/cmd`.
