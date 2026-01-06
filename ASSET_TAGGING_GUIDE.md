# Asset Tagging & Association Guide

This document outlines the workflow and API endpoints for the **Asset Tagging** feature, designed for mobile/handheld application integration.

## 1. Overview

The asset tagging process involves three main steps:
1.  **Search**: Find the existing asset record in the system (by Asset Number, Name, etc.).
2.  **Scan**: Use the handheld reader to scan the RFID tag (EPC).
3.  **Associate**: Bind the scanned Tag ID to the selected Asset ID.

## 2. API Endpoints

### 2.1 Search Assets
**Endpoint**: `GET /api/v1/assets`

Used to look up an asset before tagging it.

**Query Parameters:**
- `assetNumber` (string): Search by asset number (partial match supported).
- `name` (string): Search by asset name/description.
- `tagIdentifier` (string): Search by existing tag (if checking for duplicates).
- `includeDeleted` (bool): Set to `false` (default) to only show active assets.

**Example Request:**
```http
GET /api/v1/assets?assetNumber=LAPTOP&pageNumber=1&pageSize=10
```

**Example Response:**
```json
[
  {
    "id": 123,
    "assetNumber": "LAPTOP-005",
    "name": "Dell XPS 15",
    "tagIdentifier": null,  // null means untagged
    "currentLocationName": "Warehouse A"
  }
]
```

---

### 2.2 Assign Tag (Associate)
**Endpoint**: `POST /api/v1/assets/{id}/assign-tag`

Used to bind a scanned RFID tag to a specific asset.

**Path Parameters:**
- `id` (int): The unique ID of the asset (obtained from the Search step).

**Request Body:**
```json
{
  "tagIdentifier": "EPC-VIN-987654321"
}
```

**Success Response (200 OK):**
Returns the updated Asset object.
```json
{
  "id": 123,
  "assetNumber": "LAPTOP-005",
  "tagIdentifier": "EPC-VIN-987654321",
  "updatedAt": "2026-01-02T12:30:00Z"
}
```

**Error Responses:**
- `400 Bad Request`: Tag Identifier is missing.
- `404 Not Found`: Asset ID does not exist.
- `409 Conflict`: The tag is **already assigned** to another asset. The error message will clarify: `"Tag 'EPC-...' is already assigned to asset 'ASSET-009'"`.

---

### 2.3 Unassign Tag (Optional)
**Endpoint**: `POST /api/v1/assets/{id}/unassign-tag`

Used to remove a tag from an asset (e.g., if re-tagging or decommissioning).

---

## 3. Recommended Mobile Workflow

1.  **User opens "Tag Asset" screen**.
2.  **User enters search term** (e.g., Asset Number "1005").
    - App calls `GET /api/v1/assets?assetNumber=1005`.
3.  **App displays list of matching assets**.
    - User taps the correct asset (e.g., Asset ID `1005`).
4.  **App prompts to "Scan Tag"**.
    - User pulls trigger on handheld.
    - App reads RFID EPC (e.g., `E280...`).
5.  **App confirms association**.
    - App calls `POST /api/v1/assets/1005/assign-tag` with body `{ "tagIdentifier": "E280..." }`.
6.  **Success**:
    - App shows "Tag Assigned" toast/message.
    - App returns to search screen.
7.  **Error Handling**:
    - If API returns error (e.g., "Tag already assigned"), App displays alert: "This tag is already used on Asset X. Please use a new tag or unassign the old one first."

## 4. Validation Rules

- **Unique Tags**: A Tag Identifier (EPC) can only be assigned to **one** asset at a time. Trying to assign a used tag will result in an error.
- **Asset Existence**: You cannot assign a tag to a deleted or non-existent asset ID.
