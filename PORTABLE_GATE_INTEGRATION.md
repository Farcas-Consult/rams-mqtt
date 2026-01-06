# Portable Gate Integration Guide (REST API)

This document outlines the simplified integration for "Portable Gates" (mobile apps) using the new REST API endpoint. This approach **does not** require registering the mobile device as "Equipment" in the database.

## Overview

## Overview

1.  **Select Site**: The app fetches available Sites (high-level locations, e.g., "Mombasa").
2.  **Select Gate**: The app fetches Gates belonging to the selected Site.
3.  **Scan & Report**: When a tag is scanned, the app reports it directly to the API with the selected Gate ID.

## 1. Fetching Locations (Sites & Gates)

The app should first allow the user to select a Site, and then a Gate within that Site.

### 1.1 Fetching Sites

**Endpoint**: `GET /api/v1/sites`

**Response**:
```json
[
  {
    "id": 1,
    "name": "Mombasa",
    "description": "Coastal Region"
  },
  ...
]
```

### 1.2 Fetching Gates by Site

**Endpoint**: `GET /api/v1/sites/{id}/gates`

**Response**:
```json
[
  {
    "id": 1002,
    "name": "Mombasa Gate 1",
    "siteId": 1,
    "siteName": "Mombasa",
    "locationId": 1, 
    "isActive": true
  },
  ...
]
```

> **Note**: You can still fetch all gates via `GET /api/v1/gates`, which will now include `siteId` and `siteName` in the response.

## 2. Reporting Tag Reads

(Same as before: uses `gateId`)

**Endpoint**: `POST /api/v1/movements/report`

**Headers**:
-   `Content-Type: application/json`

**Payload**:
```json
{
  "tagId": "E200001D6305010048184568",     // The scanned EPC
  "gateId": 1002,                          // The ID of the selected Gate
  "deviceId": "MobileApp-UserA",           // Any string identifier for the device/user
  "timestamp": "2023-10-27T10:00:00Z",     // ISO 8601 timestamp
  "direction": 1                           // 1 = Inbound, 2 = Outbound
}
```

**Response**:
-   `200 OK`: Movement recorded successfully.
-   `400 Bad Request`: Validation failed.
-   `404 Not Found`: Asset (TagId) or Gate (GateId) not found.
-   `500 Internal Server Error`: Server side error.

## 3. Tag Association (Vehicle & Container)

For Vehicle and Container tagging, multiple tags are associated with a single asset.

### 3.1. Synchronize Assets
To get the list of eligible assets for tagging:

**Request**:
`GET /api/v1/assets/sync`

**Response**:
```json
[
  {
    "id": 123,
    "assetNumber": "CONTAINER-001",
    "name": "Container 1",
    "tags": [
        { "tagId": "TAG-1", "location": "Door" }
    ]
  }
]
```

### 3.2. Associate Tags
To link tags to an asset:

**Request**:
`POST /api/v1/assets/associate-tags`

**Payload**:
```json
{
  "assetNumber": "CONTAINER-001",
  "tags": [
    { "tagId": "TAG-DOOR", "location": "Door" },
    { "tagId": "TAG-LEFT", "location": "NearLeft" },
    { "tagId": "TAG-RIGHT", "location": "FarRight" }
  ]
}
```

**Response**:
- `200 OK`: Association successful.

## 4. Workflow Summary

1.  User logs in/opens app.
2.  App calls `GET /api/v1/sites`.
3.  User selects "Mombasa" (ID: 1).
4.  App calls `GET /api/v1/sites/1/gates`.
5.  User selects "Mombasa Gate 1" (ID: 1002).
6.  User scans tag `TAG123`.
7.  App calls `POST /api/v1/movements/report` with `gateId: 1002` and `tagId: TAG123`.
8.  Server updates asset location to "Mombasa Gate 1" location.
