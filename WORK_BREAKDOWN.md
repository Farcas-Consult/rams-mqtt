# Asset Tracking System - Work Breakdown Document

## Overview

This document breaks down all work required to transform the existing RFID dock door sample into a comprehensive asset tracking system. The system will track assets, their locations, movements through gates, and provide real-time monitoring and reporting.

**Current Status:**
- ✅ Existing MQTT infrastructure (Mosquitto, MQTT client)
- ✅ Existing database structure (SQL Server, Entity Framework)
- ✅ React Native mobile app (existing)
- ✅ Next.js frontend (existing)
- ❌ Asset tracking functionality (to be built)
- ❌ Gate management (to be built)
- ❌ Real-time live feed (to be built)
- ❌ Reports/analytics (to be built)

---

## Asset Import Format Mapping

### Import Column → Asset Entity Mapping

| Import Column | Asset Entity Field | Data Type | Notes |
|--------------|-------------------|-----------|-------|
| **Equipment** | `AssetNumber` | string (unique) | Primary identifier from SAP |
| **Description** | `Name` | string | Main asset name |
| **Material Description** | `Description` | string | Detailed description |
| **Material** | `MaterialId` | string | Material ID from SAP |
| **ManufSerialNumber** | `SerialNumber` | string | Manufacturer serial number |
| **TechIdentNo.** | `TechnicalId` | string | Technical identification number |
| **Plnt** | `Plant` | string | Plant/site code (e.g., KE10) |
| **SLoc** | `StorageLocation` | string | Storage location code |
| **Cost Ctr** | `CostCenter` | string | Cost center code |
| **AGrp** | `AssetGroup` | string | Asset group code |
| **BusA** | `BusinessArea` | string | Business area code |
| **ObjectType** | `ObjectType` | string | Object type code |
| **SysStatus** | `SystemStatus` | string | System status (e.g., ESTO) |
| **UserStatus** | `UserStatus` | string | User status codes |
| **AcquistnValue** | `AcquisitionValue` | decimal | Acquisition value |
| **Comment** | `Comments` | string | Additional comments |
| **TagIdentifier** | `TagIdentifier` | string (unique) | RFID tag EPC (assigned later) |
| - | `LastDiscoveredAt` | DateTime? | Auto-updated on tag read |
| - | `LastDiscoveredBy` | string | Gate/reader name |
| - | `CurrentLocationId` | int | FK to StorageUnit |
| - | `CreatedAt` | DateTime | Auto-set on creation |
| - | `IsDeleted` | bool | Soft delete flag |

**Note:** Owner tracking is not required for now (can be added later if needed).

---

## Phase 1: Database & Backend Foundation

### Task 1.1: Create Asset Entity
**Priority:** Critical  
**Estimated Time:** 2-3 hours

**Work Items:**
1. Create `Asset.cs` entity in `ZebraIoTConnector.Persistence/Entities/`
2. Add all fields based on import mapping above
3. Add navigation properties:
   - `CurrentLocation` → `StorageUnit`
   - `Movements` → `List<AssetMovement>`
4. Add indexes:
   - `TagIdentifier` (unique)
   - `AssetNumber` (unique)
   - `LastDiscoveredAt`
   - `Plant`
   - `CostCenter`

**Files to Create:**
- `ZebraIoTConnector.Persistence/Entities/Asset.cs`

**Files to Modify:**
- `ZebraIoTConnector.Persistence/ZebraDbContext.cs` (add DbSet)

---

### Task 1.2: Create Gate Entity
**Priority:** Critical  
**Estimated Time:** 2 hours

**Work Items:**
1. Create `Gate.cs` entity
2. Add fields:
   - `Id`, `Name`, `Description`
   - `Location` → `StorageUnit` (navigation)
   - `IsActive` (bool)
   - `Readers` → `List<Equipment>` (navigation)
3. Add unique index on `Name`

**Files to Create:**
- `ZebraIoTConnector.Persistence/Entities/Gate.cs`

**Files to Modify:**
- `ZebraIoTConnector.Persistence/ZebraDbContext.cs`

---

### Task 1.3: Create AssetMovement Entity
**Priority:** Critical  
**Estimated Time:** 1-2 hours

**Work Items:**
1. Create `AssetMovement.cs` entity
2. Add fields:
   - `Id`
   - `Asset` → `Asset` (navigation)
   - `FromLocation` → `StorageUnit` (navigation, nullable)
   - `ToLocation` → `StorageUnit` (navigation)
   - `Gate` → `Gate` (navigation, nullable)
   - `Reader` → `Equipment` (navigation, nullable)
   - `ReaderIdString` (string) - for mobile devices
   - `ReadTimestamp` (DateTime)
3. Add indexes on `AssetId`, `ReadTimestamp`, `GateId`

**Files to Create:**
- `ZebraIoTConnector.Persistence/Entities/AssetMovement.cs`

**Files to Modify:**
- `ZebraIoTConnector.Persistence/ZebraDbContext.cs`

---

### Task 1.4: Update Equipment Entity
**Priority:** Critical  
**Estimated Time:** 1 hour

**Work Items:**
1. Add `Gate` navigation property to `Equipment.cs`
2. Add `GateId` foreign key (nullable int)
3. Add `IsMobile` (bool) - flag for mobile devices
4. Add `LastHeartbeat` (DateTime?)
5. Add `IsOnline` (bool)
6. Remove or keep `ReferenceStorageUnit` (for backward compatibility)

**Files to Modify:**
- `ZebraIoTConnector.Persistence/Entities/Equipment.cs`
- `ZebraIoTConnector.Persistence/ZebraDbContext.cs` (update relationships)

---

### Task 1.5: Create Database Migration
**Priority:** Critical  
**Estimated Time:** 1-2 hours

**Work Items:**
1. Create Entity Framework migration
2. Review migration SQL
3. Test migration on development database
4. Update `InitScript.sql` with sample data (optional)

**Commands:**
```bash
dotnet ef migrations add AddAssetTrackingEntities
dotnet ef database update
```

**Files to Create:**
- Migration files in `ZebraIoTConnector.Persistence/Migrations/`

---

### Task 1.6: Create Asset Repository
**Priority:** Critical  
**Estimated Time:** 3-4 hours

**Work Items:**
1. Create `IAssetRepository.cs` interface
2. Create `AssetRepository.cs` implementation
3. Implement methods:
   - `GetById(int id)`
   - `GetByAssetNumber(string assetNumber)`
   - `GetByTagIdentifier(string tagId)` - **Critical for tag processing**
   - `GetAll()` with filters (plant, cost center, location, etc.)
   - `Create(Asset asset)`
   - `Update(Asset asset)`
   - `Delete(int id)` - soft delete
   - `BulkCreate(List<Asset> assets)` - for import
   - `GetAssetsNotSeenInDays(int days)`
   - `GetAssetsByLocation(int locationId)`

**Files to Create:**
- `ZebraIoTConnector.Persistence/Repositories/IAssetRepository.cs`
- `ZebraIoTConnector.Persistence/Repositories/AssetRepository.cs`

**Files to Modify:**
- `ZebraIoTConnector.Persistence/IUnitOfWork.cs` (add property)
- `ZebraIoTConnector.Persistence/UnitOfWork.cs` (implement)

---

### Task 1.7: Create Gate Repository
**Priority:** High  
**Estimated Time:** 2 hours

**Work Items:**
1. Create `IGateRepository.cs` interface
2. Create `GateRepository.cs` implementation
3. Implement methods:
   - `GetById(int id)`
   - `GetByName(string name)`
   - `GetByReaderName(string readerName)` - **Critical for tag processing**
   - `GetAll()`
   - `Create(Gate gate)`
   - `Update(Gate gate)`
   - `Delete(int id)`

**Files to Create:**
- `ZebraIoTConnector.Persistence/Repositories/IGateRepository.cs`
- `ZebraIoTConnector.Persistence/Repositories/GateRepository.cs`

**Files to Modify:**
- `ZebraIoTConnector.Persistence/IUnitOfWork.cs`
- `ZebraIoTConnector.Persistence/UnitOfWork.cs`

---

### Task 1.8: Create AssetMovement Repository
**Priority:** High  
**Estimated Time:** 2 hours

**Work Items:**
1. Create `IAssetMovementRepository.cs` interface
2. Create `AssetMovementRepository.cs` implementation
3. Implement methods:
   - `GetById(int id)`
   - `GetByAssetId(int assetId)` - with pagination
   - `GetByGateId(int gateId, DateTime? from, DateTime? to)`
   - `GetRecent(int count)` - for live feed
   - `Create(AssetMovement movement)`
   - `GetMovementHistory(DateTime from, DateTime to, int? assetId, int? gateId)`

**Files to Create:**
- `ZebraIoTConnector.Persistence/Repositories/IAssetMovementRepository.cs`
- `ZebraIoTConnector.Persistence/Repositories/AssetMovementRepository.cs`

**Files to Modify:**
- `ZebraIoTConnector.Persistence/IUnitOfWork.cs`
- `ZebraIoTConnector.Persistence/UnitOfWork.cs`

---

### Task 1.9: Create DTOs for Asset
**Priority:** High  
**Estimated Time:** 2 hours

**Work Items:**
1. Create DTOs in `ZebraIoTConnector.DomainModel/Dto/`:
   - `AssetDto.cs` - for API responses
   - `CreateAssetDto.cs` - for API requests
   - `UpdateAssetDto.cs` - for API requests
   - `AssetMovementDto.cs` - for movement history
   - `BulkImportAssetDto.cs` - for bulk import
   - `AssetImportResultDto.cs` - for import results

**Files to Create:**
- `ZebraIoTConnector.DomainModel/Dto/AssetDto.cs`
- `ZebraIoTConnector.DomainModel/Dto/CreateAssetDto.cs`
- `ZebraIoTConnector.DomainModel/Dto/UpdateAssetDto.cs`
- `ZebraIoTConnector.DomainModel/Dto/AssetMovementDto.cs`
- `ZebraIoTConnector.DomainModel/Dto/BulkImportAssetDto.cs`
- `ZebraIoTConnector.DomainModel/Dto/AssetImportResultDto.cs`

---

## Phase 2: Business Logic Services

### Task 2.1: Create Asset Management Service
**Priority:** Critical  
**Estimated Time:** 4-5 hours

**Work Items:**
1. Create `IAssetManagementService.cs` interface
2. Create `AssetManagementService.cs` implementation
3. Implement methods:
   - `CreateAsset(CreateAssetDto dto)`
   - `UpdateAsset(int id, UpdateAssetDto dto)`
   - `GetAsset(int id)`
   - `GetAssetByTag(string tagIdentifier)`
   - `GetAssets(AssetFilterDto filter)` - with pagination
   - `DeleteAsset(int id)` - soft delete
   - `AssignTagToAsset(int assetId, string tagIdentifier)`
   - `UnassignTag(int assetId)`
   - `BulkImportAssets(List<BulkImportAssetDto> assets)` - **Critical for import**
     - Validate data
     - Check duplicates
     - Return import results with errors

**Files to Create:**
- `ZebraIoTConnector.Services/IAssetManagementService.cs`
- `ZebraIoTConnector.Services/AssetManagementService.cs`

**Files to Modify:**
- `ZebraIoTConnector.Client.MQTT.Console/DependencyRegistrar.cs` (register service)

---

### Task 2.2: Refactor MaterialMovementService
**Priority:** Critical  
**Estimated Time:** 4-5 hours

**Work Items:**
1. Rename to `AssetTrackingService` (or keep name, update logic)
2. **Critical Change:** Replace auto-create logic with asset lookup
3. Update `NewTagReaded` method:
   ```csharp
   public void NewTagReaded(string clientId, List<TagReadEvent> tagReadEvent)
   {
       // 1. Get reader/gate info
       var reader = unitOfWork.EquipmentRepository.GetEquipmentByName(clientId);
       var gate = reader?.Gate;
       
       if (gate == null)
       {
           logger.LogWarning($"Reader {clientId} not assigned to a gate");
           return;
       }
       
       // 2. Process each tag
       foreach (var tag in tagReadEvent)
       {
           // 3. Look up asset (DON'T AUTO-CREATE!)
           var asset = unitOfWork.AssetRepository.GetByTagIdentifier(tag.IdHex);
           
           if (asset == null)
           {
               logger.LogWarning($"Unregistered tag: {tag.IdHex} at gate {gate.Name}");
               // Optionally: Push to SignalR for unregistered tag alert
               continue;
           }
           
           // 4. Update asset tracking
           var previousLocation = asset.CurrentLocation;
           asset.LastDiscoveredAt = DateTime.UtcNow;
           asset.LastDiscoveredBy = gate.Name;
           asset.CurrentLocation = gate.Location;
           
           // 5. Record movement
           var movement = new AssetMovement
           {
               Asset = asset,
               FromLocation = previousLocation,
               ToLocation = gate.Location,
               Gate = gate,
               Reader = reader,
               ReaderIdString = clientId,
               ReadTimestamp = DateTime.UtcNow
           };
           
           unitOfWork.AssetMovementRepository.Add(movement);
       }
       
       // 6. Save all changes
       unitOfWork.SaveChanges();
       
       // 7. Push to SignalR (if implemented)
   }
   ```

**Files to Modify:**
- `ZebraIoTConnector.Services/MaterialMovementService.cs` (major refactor)
- `ZebraIoTConnector.Services/IMaterialMovementService.cs` (update interface if needed)

---

### Task 2.3: Create Gate Management Service
**Priority:** High  
**Estimated Time:** 3 hours

**Work Items:**
1. Create `IGateManagementService.cs` interface
2. Create `GateManagementService.cs` implementation
3. Implement methods:
   - `CreateGate(CreateGateDto dto)`
   - `UpdateGate(int id, UpdateGateDto dto)`
   - `GetGate(int id)`
   - `GetAllGates()`
   - `AssignReaderToGate(int gateId, int readerId)`
   - `RemoveReaderFromGate(int gateId, int readerId)`
   - `GetGateByReaderName(string readerName)`

**Files to Create:**
- `ZebraIoTConnector.Services/IGateManagementService.cs`
- `ZebraIoTConnector.Services/GateManagementService.cs`

---

### Task 2.4: Create Reporting Service
**Priority:** Medium  
**Estimated Time:** 4-5 hours

**Work Items:**
1. Create `IReportingService.cs` interface
2. Create `ReportingService.cs` implementation
3. Implement methods:
   - `GetLocationReport(LocationReportFilter filter)`
   - `GetMovementReport(MovementReportFilter filter)`
   - `GetDiscoveryReport(int daysNotSeen)`
   - `GetGateActivityReport(int gateId, DateTime from, DateTime to)`
   - `GetAssetStatistics()`

**Files to Create:**
- `ZebraIoTConnector.Services/IReportingService.cs`
- `ZebraIoTConnector.Services/ReportingService.cs`

---

## Phase 3: Convert Console App to Web API

### Task 3.1: Create ASP.NET Core Web API Project
**Priority:** Critical  
**Estimated Time:** 2-3 hours

**Work Items:**
1. Convert existing console app to Web API, OR
2. Create new Web API project and reference existing projects
3. Add NuGet packages:
   - `Microsoft.AspNetCore.SignalR`
   - `Swashbuckle.AspNetCore` (Swagger)
   - `Microsoft.AspNetCore.Authentication.JwtBearer` (if auth needed)
4. Configure services in `Startup.cs` or `Program.cs`
5. Keep existing MQTT subscriber as `IHostedService`

**Decision:** Recommend creating new Web API project and referencing existing projects to maintain separation.

**Files to Create:**
- `ZebraIoTConnector.Backend.API/Program.cs`
- `ZebraIoTConnector.Backend.API/Startup.cs` (if using .NET 5)
- `ZebraIoTConnector.Backend.API/appsettings.json`
- `ZebraIoTConnector.Backend.API/appsettings.Development.json`

**Files to Modify:**
- Solution file (add new project)

---

### Task 3.2: Create Asset API Controllers
**Priority:** Critical  
**Estimated Time:** 4-5 hours

**Work Items:**
1. Create `AssetsController.cs`
2. Implement endpoints:
   - `GET /api/v1/assets` - List with filters
   - `GET /api/v1/assets/{id}` - Get by ID
   - `GET /api/v1/assets/by-tag/{tagId}` - Get by tag
   - `POST /api/v1/assets` - Create
   - `PUT /api/v1/assets/{id}` - Update
   - `DELETE /api/v1/assets/{id}` - Delete
   - `POST /api/v1/assets/bulk-import` - Bulk import
   - `POST /api/v1/assets/{id}/assign-tag` - Assign tag
   - `POST /api/v1/assets/{id}/unassign-tag` - Unassign tag

3. Add validation
4. Add error handling
5. Add Swagger documentation

**Files to Create:**
- `ZebraIoTConnector.Backend.API/Controllers/AssetsController.cs`
- `ZebraIoTConnector.Backend.API/Controllers/GatesController.cs`
- `ZebraIoTConnector.Backend.API/Controllers/MovementsController.cs`
- `ZebraIoTConnector.Backend.API/Controllers/ReportsController.cs`

---

### Task 3.3: Implement Bulk Import Endpoint
**Priority:** High  
**Estimated Time:** 3-4 hours

**Work Items:**
1. Create `POST /api/v1/assets/bulk-import` endpoint
2. Accept CSV or Excel file
3. Parse file using import column mapping
4. Validate each row:
   - Required fields present
   - AssetNumber unique
   - Valid data types
5. Return import results:
   - Success count
   - Error count
   - List of errors with row numbers
6. Use transaction for atomicity

**Files to Create:**
- `ZebraIoTConnector.Backend.API/Controllers/AssetsController.cs` (add method)
- `ZebraIoTConnector.Services/AssetImportService.cs` (optional, for complex parsing)

**Libraries Needed:**
- `CsvHelper` or `EPPlus` for Excel parsing

---

### Task 3.4: Create SignalR Hub for Live Feed
**Priority:** High  
**Estimated Time:** 3-4 hours

**Work Items:**
1. Create `LiveFeedHub.cs` in `ZebraIoTConnector.Backend.API/Hubs/`
2. Implement hub methods:
   - `SubscribeToLiveFeed()` - client subscribes
   - `UnsubscribeFromLiveFeed()` - client unsubscribes
3. Update `MaterialMovementService` to push events to SignalR:
   ```csharp
   await _hubContext.Clients.All.SendAsync("TagRead", new {
       TagId = tag.IdHex,
       AssetNumber = asset.AssetNumber,
       AssetName = asset.Name,
       Gate = gate.Name,
       Location = gate.Location?.Name,
       Timestamp = DateTime.UtcNow,
       Plant = asset.Plant
   });
   ```
4. Add connection management (track connected clients)

**Files to Create:**
- `ZebraIoTConnector.Backend.API/Hubs/LiveFeedHub.cs`

**Files to Modify:**
- `ZebraIoTConnector.Services/MaterialMovementService.cs` (inject IHubContext)

---

### Task 3.5: Create MQTT Background Service
**Priority:** Critical  
**Estimated Time:** 2-3 hours

**Work Items:**
1. Create `MqttSubscriberService.cs` implementing `IHostedService`
2. Move existing MQTT subscription logic from console app
3. Subscribe to `zebra/#` topics
4. Process tag read events
5. Handle reconnection logic
6. Register as hosted service in DI

**Files to Create:**
- `ZebraIoTConnector.Backend.API/Services/MqttSubscriberService.cs`

**Files to Modify:**
- `ZebraIoTConnector.Backend.API/Program.cs` (register service)

---

## Phase 4: Next.js Frontend

### Task 4.1: Asset Management Pages
**Priority:** Critical  
**Estimated Time:** 6-8 hours

**Work Items:**
1. Create asset list page (`/assets`)
   - Table with pagination
   - Filters: Plant, Cost Center, Location, Last Seen
   - Search by Asset Number, Name, Tag
   - Actions: View, Edit, Delete
2. Create asset detail page (`/assets/[id]`)
   - Show all asset details
   - Show movement history
   - Show tag assignment
3. Create asset create/edit page (`/assets/new`, `/assets/[id]/edit`)
   - Form with all fields
   - Validation
   - Tag assignment UI
4. Create bulk import page (`/assets/import`)
   - File upload (CSV/Excel)
   - Preview import data
   - Show validation errors
   - Execute import
   - Show import results

**Files to Create:**
- `frontend/pages/assets/index.tsx`
- `frontend/pages/assets/[id].tsx`
- `frontend/pages/assets/new.tsx`
- `frontend/pages/assets/[id]/edit.tsx`
- `frontend/pages/assets/import.tsx`
- `frontend/components/assets/AssetList.tsx`
- `frontend/components/assets/AssetForm.tsx`
- `frontend/components/assets/BulkImport.tsx`

---

### Task 4.2: Live Feed Dashboard
**Priority:** High  
**Estimated Time:** 5-6 hours

**Work Items:**
1. Create live feed page (`/dashboard/live-feed`)
2. Connect to SignalR hub
3. Display real-time tag reads:
   - Table/list with auto-refresh
   - Columns: Timestamp, Tag, Asset, Gate, Location
   - Filter by gate, time range
   - Highlight unregistered tags
4. Show gate status:
   - Active/inactive readers
   - Last heartbeat
   - Reader health
5. Show recent movements (last 50)

**Files to Create:**
- `frontend/pages/dashboard/live-feed.tsx`
- `frontend/components/dashboard/LiveFeedTable.tsx`
- `frontend/components/dashboard/GateStatus.tsx`
- `frontend/hooks/useSignalR.ts` (custom hook)

**Libraries Needed:**
- `@microsoft/signalr` for SignalR client

---

### Task 4.3: Reports Pages
**Priority:** Medium  
**Estimated Time:** 6-8 hours

**Work Items:**
1. Create reports page (`/reports`)
2. Implement reports:
   - **Location Report**: Current location of all assets
     - Group by location
     - Export to Excel/PDF
   - **Movement Report**: Movement history
     - Filter by asset, gate, date range
     - Timeline view
   - **Discovery Report**: Assets not seen in X days
     - Configurable days threshold
     - Export capability
   - **Gate Activity Report**: Activity per gate
     - Reads per gate
     - Peak hours chart
3. Add date range pickers
4. Add export functionality

**Files to Create:**
- `frontend/pages/reports/index.tsx`
- `frontend/pages/reports/location.tsx`
- `frontend/pages/reports/movement.tsx`
- `frontend/pages/reports/discovery.tsx`
- `frontend/pages/reports/gate-activity.tsx`
- `frontend/components/reports/ReportFilters.tsx`

**Libraries Needed:**
- Chart library (e.g., `recharts` or `chart.js`)
- Export library (e.g., `xlsx` for Excel)

---

### Task 4.4: Gate Management Pages
**Priority:** High  
**Estimated Time:** 4-5 hours

**Work Items:**
1. Create gate list page (`/gates`)
   - List all gates
   - Show associated readers
   - Show gate status
2. Create gate create/edit page (`/gates/new`, `/gates/[id]/edit`)
   - Form for gate details
   - Assign readers to gate
   - Set gate location
3. Create reader management page (`/readers`)
   - List all readers
   - Show reader status
   - Assign readers to gates

**Files to Create:**
- `frontend/pages/gates/index.tsx`
- `frontend/pages/gates/[id].tsx`
- `frontend/pages/gates/new.tsx`
- `frontend/pages/readers/index.tsx`
- `frontend/components/gates/GateForm.tsx`

---

### Task 4.5: API Client Setup
**Priority:** Critical  
**Estimated Time:** 2-3 hours

**Work Items:**
1. Set up API client (axios or fetch wrapper)
2. Create API service files:
   - `api/assets.ts`
   - `api/gates.ts`
   - `api/movements.ts`
   - `api/reports.ts`
3. Add error handling
4. Add request/response interceptors
5. Configure base URL from environment

**Files to Create:**
- `frontend/lib/api/client.ts`
- `frontend/lib/api/assets.ts`
- `frontend/lib/api/gates.ts`
- `frontend/lib/api/movements.ts`
- `frontend/lib/api/reports.ts`

---

## Phase 5: React Native Mobile App

### Task 5.1: Asset Tagging Mode
**Priority:** High  
**Estimated Time:** 8-10 hours

**Work Items:**
1. Create tagging screen
2. Implement RFID scanning:
   - Use Zebra EMDK or DataWedge
   - Scan tag EPC
   - Look up asset by tag
3. Asset registration flow:
   - Scan tag
   - Enter asset details (or select from list if exists)
   - Assign tag to asset
   - Save locally (SQLite)
   - Sync to server
4. Tag assignment flow:
   - Scan existing asset (by asset number or tag)
   - Scan new tag
   - Assign tag to asset
5. Asset lookup:
   - Scan tag to view asset details
   - Show: Name, Location, Last Seen

**Files to Create:**
- `mobile/screens/TaggingScreen.tsx`
- `mobile/screens/AssetRegistrationScreen.tsx`
- `mobile/screens/AssetLookupScreen.tsx`
- `mobile/services/RfidService.ts`
- `mobile/services/AssetService.ts`

**Libraries Needed:**
- Zebra EMDK or DataWedge integration
- SQLite for local storage
- API client for sync

---

### Task 5.2: Portable Gate Mode
**Priority:** High  
**Estimated Time:** 6-8 hours

**Work Items:**
1. Create portable gate screen
2. Implement gate activation:
   - User enables "Portable Gate" mode
   - Device connects to MQTT broker
   - Publishes to `zebra/mobile/{deviceId}/data`
3. Continuous scanning:
   - Background RFID scanning
   - Publish tag reads in real-time
   - Show read count
   - Show recent tags
4. Gate configuration:
   - Set device ID/name
   - Configure MQTT connection
   - Set location (manual or GPS)
   - Set gate name

**Files to Create:**
- `mobile/screens/PortableGateScreen.tsx`
- `mobile/services/MqttService.ts`
- `mobile/services/GateService.ts`

**Libraries Needed:**
- MQTT client (e.g., `react-native-mqtt`)

---

### Task 5.3: Offline Support
**Priority:** Medium  
**Estimated Time:** 4-5 hours

**Work Items:**
1. Set up SQLite database
2. Create local schema (assets, movements, sync queue)
3. Implement sync service:
   - Queue operations when offline
   - Sync when connection restored
   - Conflict resolution
4. Add connection status indicator
5. Add sync status indicator

**Files to Create:**
- `mobile/database/schema.ts`
- `mobile/database/database.ts`
- `mobile/services/SyncService.ts`

**Libraries Needed:**
- `react-native-sqlite-storage` or `@react-native-async-storage/async-storage`

---

### Task 5.4: Mobile App Navigation & UI
**Priority:** Medium  
**Estimated Time:** 4-5 hours

**Work Items:**
1. Set up navigation (React Navigation)
2. Create main menu/home screen
3. Create mode selection (Tagging vs Portable Gate)
4. Design touch-friendly UI
5. Add status indicators (connection, sync)
6. Add settings screen

**Files to Create:**
- `mobile/navigation/AppNavigator.tsx`
- `mobile/screens/HomeScreen.tsx`
- `mobile/screens/SettingsScreen.tsx`

---

## Phase 6: Testing & Integration

### Task 6.1: Backend Unit Tests
**Priority:** Medium  
**Estimated Time:** 4-5 hours

**Work Items:**
1. Write unit tests for:
   - Asset repository
   - Asset management service
   - Material movement service (tag processing)
   - Gate management service
2. Use xUnit or NUnit
3. Mock dependencies

**Files to Create:**
- `ZebraIoTConnector.Services.Tests/` (test project)
- Test files for each service

---

### Task 6.2: API Integration Tests
**Priority:** Medium  
**Estimated Time:** 3-4 hours

**Work Items:**
1. Test API endpoints:
   - Asset CRUD
   - Bulk import
   - Gate management
   - Movement history
2. Test SignalR hub
3. Test error scenarios

---

### Task 6.3: End-to-End Testing
**Priority:** High  
**Estimated Time:** 4-5 hours

**Work Items:**
1. Test complete flow:
   - Import assets
   - Assign tags
   - Read tags at gate
   - Verify movement recorded
   - Verify live feed updates
2. Test mobile app:
   - Tagging mode
   - Portable gate mode
   - Offline sync
3. Test with real RFID readers (if available)

---

## Phase 7: Deployment & Documentation

### Task 7.1: Update Docker Configuration
**Priority:** High  
**Estimated Time:** 2-3 hours

**Work Items:**
1. Update `docker-compose.yml`:
   - Add backend API service
   - Configure environment variables
   - Set up networking
2. Create Dockerfile for backend API
3. Test docker-compose setup

**Files to Modify:**
- `docker-compose.yml`

**Files to Create:**
- `ZebraIoTConnector.Backend.API/Dockerfile`

---

### Task 7.2: Environment Configuration
**Priority:** High  
**Estimated Time:** 1-2 hours

**Work Items:**
1. Create environment configuration files:
   - `appsettings.Development.json`
   - `appsettings.Production.json`
2. Set up connection strings
3. Configure MQTT broker settings
4. Set up CORS for frontend

---

### Task 7.3: Documentation
**Priority:** Medium  
**Estimated Time:** 3-4 hours

**Work Items:**
1. Update README.md with:
   - Setup instructions
   - API documentation
   - Deployment guide
2. Create user guide for:
   - Asset import process
   - Tag assignment
   - Using live feed
   - Mobile app usage
3. Create admin guide for:
   - Gate configuration
   - Reader setup
   - Troubleshooting

---

## Summary of Deliverables

### Backend (.NET)
- ✅ Asset entity and repository
- ✅ Gate entity and repository
- ✅ AssetMovement entity and repository
- ✅ Refactored tag processing (no auto-create)
- ✅ Asset management service
- ✅ Gate management service
- ✅ Reporting service
- ✅ ASP.NET Core Web API
- ✅ SignalR hub for live feed
- ✅ MQTT background service
- ✅ Bulk import endpoint

### Frontend (Next.js)
- ✅ Asset management pages (CRUD, import)
- ✅ Live feed dashboard
- ✅ Reports pages
- ✅ Gate management pages
- ✅ API client setup

### Mobile (React Native)
- ✅ Tagging mode
- ✅ Portable gate mode
- ✅ Offline support
- ✅ Navigation and UI

---

## Estimated Timeline

| Phase | Tasks | Estimated Time |
|-------|-------|----------------|
| Phase 1: Database & Backend Foundation | 9 tasks | 20-25 hours |
| Phase 2: Business Logic Services | 4 tasks | 14-18 hours |
| Phase 3: Web API | 5 tasks | 14-18 hours |
| Phase 4: Next.js Frontend | 5 tasks | 23-30 hours |
| Phase 5: React Native Mobile | 4 tasks | 22-28 hours |
| Phase 6: Testing & Integration | 3 tasks | 11-14 hours |
| Phase 7: Deployment & Documentation | 3 tasks | 6-9 hours |
| **Total** | **33 tasks** | **110-142 hours** |

**Note:** This is a rough estimate. Actual time may vary based on team experience and complexity.

---

## Priority Order

### Must Have (MVP)
1. Phase 1: Database & Backend Foundation (all tasks)
2. Phase 2: Business Logic Services (Tasks 2.1, 2.2)
3. Phase 3: Web API (Tasks 3.1, 3.2, 3.3, 3.5)
4. Phase 4: Next.js Frontend (Tasks 4.1, 4.2, 4.5)
5. Phase 6: End-to-End Testing (Task 6.3)

### Should Have
- Phase 2: Task 2.3 (Gate Management Service)
- Phase 3: Task 3.4 (SignalR)
- Phase 4: Task 4.3 (Reports)
- Phase 5: Task 5.1 (Tagging Mode)

### Nice to Have
- Phase 2: Task 2.4 (Reporting Service)
- Phase 4: Task 4.4 (Gate Management UI)
- Phase 5: Tasks 5.2, 5.3 (Portable Gate, Offline)
- Phase 6: Tasks 6.1, 6.2 (Unit/Integration Tests)

---

## Next Steps

1. **Review this document** with the team
2. **Prioritize tasks** based on business needs
3. **Assign tasks** to team members
4. **Start with Phase 1, Task 1.1** (Create Asset Entity)
5. **Set up project tracking** (Jira, Trello, etc.)
6. **Schedule regular check-ins** to track progress

---

**Last Updated:** [Current Date]  
**Version:** 1.0

