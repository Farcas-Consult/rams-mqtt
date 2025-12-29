# Asset Tracking System - Technical Specification

## 1. Executive Summary

### 1.1 Purpose
Specification for an RFID-based asset tracking system that tracks organizational assets, ownership, location, and movement through fixed and mobile gates.

### 1.2 Scope
- Web application for asset management, live monitoring, and analytics
- Mobile application for Zebra handheld devices (tagging and portable gate)
- MQTT backend infrastructure for real-time tag processing
- Integration with fixed RFID readers at gates and mobile handheld scanners

### 1.3 Key Requirements
- Track asset ownership (who has it)
- Track last discovery timestamp
- Record gate movements (fixed and mobile)
- Asset registration and tag assignment
- Real-time live feed of tag reads
- Basic analytics and reporting

---

## 2. System Architecture

### 2.1 High-Level Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   Web App       │     │   Mobile App    │     │  Fixed Readers   │
│   (React/Vue)   │     │  (Zebra Device) │     │  (FX7500/9600)  │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │
         │  REST API             │  MQTT                 │  MQTT
         │  WebSocket            │                       │
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │   MQTT Broker           │
                    │   (Mosquitto)           │
                    └────────────┬────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │   Backend API           │
                    │   (.NET Core/ASP.NET)   │
                    │   - MQTT Subscriber     │
                    │   - REST API            │
                    │   - WebSocket Server    │
                    └────────────┬────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │   SQL Server Database   │
                    │   (Asset, Movement,     │
                    │    Gate, Reader data)   │
                    └─────────────────────────┘
```

### 2.2 Component Overview

| Component | Technology | Purpose |
|-----------|-----------|---------|
| **Web App** | React/Vue.js + ASP.NET Core API | Asset management, live feed, reports |
| **Mobile App** | Xamarin/MAUI or Native (Android) | Tagging, portable gate functionality |
| **Backend API** | ASP.NET Core Web API | Business logic, MQTT processing, REST API |
| **MQTT Broker** | Mosquitto | Message broker for RFID events |
| **Database** | SQL Server | Persistent storage |
| **MQTT Client** | MQTTnet (existing) | Tag read event processing |

---

## 3. Component Specifications

### 3.1 Web Application

#### 3.1.1 Technology Stack
- Frontend: React.js or Vue.js
- Backend API: ASP.NET Core Web API (can extend existing console app)
- Real-time: SignalR (WebSocket) for live feed
- UI Framework: Material-UI or Ant Design

#### 3.1.2 Functional Requirements

**3.1.2.1 Asset Management**
- Asset CRUD
  - Create: Asset Number, Name, Description, Tag EPC, Owner, Department
  - Read: List with filters (owner, department, location, last seen)
  - Update: Edit asset details, reassign tag, change owner
  - Delete: Soft delete with audit trail
- Bulk import (CSV/Excel)
  - Format: AssetNumber, Name, TagEPC, Owner, Department
  - Validation and error reporting
- Tag assignment
  - Assign new tag to asset
  - Reassign tag from one asset to another
  - Unassign tag

**3.1.2.2 Live Feed Dashboard**
- Real-time tag read events
  - Stream via SignalR
  - Display: Timestamp, Tag EPC, Asset Name, Gate/Reader, Location, Owner
  - Filter by gate, time range, asset
- Gate status
  - Active/inactive readers
  - Last heartbeat per reader
  - Reader health indicators
- Recent movements
  - Last 50 movements with details
  - Auto-refresh

**3.1.2.3 Reports & Analytics**
- Asset location report
  - Current location of all assets
  - Group by location, owner, department
  - Export to PDF/Excel
- Movement history
  - Filter by asset, gate, date range, owner
  - Timeline view
  - Export capability
- Discovery report
  - Assets not seen in X days
  - Last seen by asset
  - Missing assets alert
- Ownership report
  - Assets by owner/department
  - Transfer history
- Gate activity
  - Reads per gate/reader
  - Peak hours
  - Activity trends

**3.1.2.4 Gate Management**
- Fixed gate configuration
  - Create/edit gates
  - Assign readers to gates
  - Set gate location
- Reader management
  - Register readers
  - View reader status
  - Configure reader settings

**3.1.2.5 User Management**
- Authentication (JWT)
- Role-based access (Admin, Manager, Viewer)
- User management

#### 3.1.3 API Endpoints (REST)

```
# Asset Management
GET    /api/assets                    # List assets (with filters)
GET    /api/assets/{id}              # Get asset details
POST   /api/assets                    # Create asset
PUT    /api/assets/{id}               # Update asset
DELETE /api/assets/{id}               # Delete asset
POST   /api/assets/bulk-import        # Bulk import
POST   /api/assets/{id}/assign-tag    # Assign tag to asset
GET    /api/assets/by-tag/{tagId}     # Get asset by tag EPC

# Gate Management
GET    /api/gates                     # List gates
GET    /api/gates/{id}                # Get gate details
POST   /api/gates                     # Create gate
PUT    /api/gates/{id}                # Update gate
DELETE /api/gates/{id}                # Delete gate
GET    /api/gates/{id}/readers        # Get readers for gate

# Reader Management
GET    /api/readers                   # List readers
GET    /api/readers/{id}              # Get reader details
POST   /api/readers                   # Register reader
PUT    /api/readers/{id}              # Update reader
GET    /api/readers/{id}/status      # Get reader status

# Movement History
GET    /api/movements                 # List movements (with filters)
GET    /api/movements/{id}            # Get movement details
GET    /api/movements/asset/{assetId} # Get movements for asset

# Reports
GET    /api/reports/location          # Location report
GET    /api/reports/movement          # Movement report
GET    /api/reports/discovery         # Discovery report
GET    /api/reports/ownership         # Ownership report
GET    /api/reports/gate-activity     # Gate activity report

# Real-time
GET    /api/hub/livefeed              # SignalR hub endpoint
```

#### 3.1.4 SignalR Hub

```csharp
public class LiveFeedHub : Hub
{
    // Client subscribes to receive real-time tag read events
    // Server pushes: TagReadEvent, GateStatusUpdate, AssetMovement
}
```

---

### 3.2 Mobile Application (Zebra Handheld)

#### 3.2.1 Technology Stack
- Framework: Xamarin.Forms / .NET MAUI (recommended) or Native Android
- RFID SDK: Zebra DataWedge or EMDK for Android
- MQTT Client: MQTTnet for portable gate mode
- Target Devices: Zebra TC series, MC series

#### 3.2.2 Functional Requirements

**3.2.2.1 Tagging Mode**
- Asset registration
  - Scan RFID tag
  - Enter asset details (Number, Name, Description)
  - Assign owner/department
  - Save to local DB (sync when online)
- Tag assignment
  - Scan existing asset
  - Scan new tag
  - Assign tag to asset
  - Confirm assignment
- Asset lookup
  - Scan tag to view asset details
  - Show: Name, Owner, Last Location, Last Seen
  - Quick actions: Update owner, Update location

**3.2.2.2 Portable Gate Mode**
- Gate activation
  - User selects "Portable Gate" mode
  - Device acts as mobile reader
  - Connects to MQTT broker
  - Publishes tag reads to: `zebra/mobile/{deviceId}/data`
- Continuous scanning
  - Background RFID scanning
  - Publish tag reads in real-time
  - Show read count and recent tags
- Gate configuration
  - Set device name/ID
  - Configure MQTT connection
  - Set location (manual or GPS)
  - Set gate name

**3.2.2.3 Offline Support**
- Local SQLite database
- Queue tag reads when offline
- Sync when connection restored
- Conflict resolution

**3.2.2.4 User Interface**
- Simple, touch-friendly UI
- Large buttons for common actions
- Barcode/QR scanning support (for asset lookup)
- Status indicators (connection, sync status)

#### 3.2.3 Mobile App Architecture

```
┌─────────────────────────────────────┐
│      Mobile App (Xamarin/MAUI)      │
├─────────────────────────────────────┤
│  ┌──────────┐      ┌──────────┐   │
│  │ Tagging  │      │ Portable │   │
│  │  Mode    │      │   Gate   │   │
│  └────┬─────┘      └────┬─────┘   │
│       │                 │          │
│  ┌────▼─────────────────▼─────┐   │
│  │   RFID Service Layer       │   │
│  │   (Zebra EMDK/DataWedge)    │   │
│  └────┬─────────────────┬─────┘   │
│       │                 │          │
│  ┌────▼─────────────────▼─────┐   │
│  │   Local SQLite DB          │   │
│  │   (Offline storage)        │   │
│  └────┬─────────────────┬─────┘   │
│       │                 │          │
│  ┌────▼─────────────────▼─────┐   │
│  │   Sync Service             │   │
│  │   (REST API + MQTT)        │   │
│  └────────────────────────────┘   │
└─────────────────────────────────────┘
```

---

### 3.3 MQTT Backend Infrastructure

#### 3.3.1 Architecture Decision
Recommendation: Integrate MQTT processing into the Web API backend (ASP.NET Core).

Rationale:
- Single deployment unit
- Shared database access
- Easier maintenance
- Real-time events can push directly to SignalR
- Reuse existing MQTT infrastructure

#### 3.3.2 Technology Stack
- Framework: ASP.NET Core (extend existing console app to Web API)
- MQTT Client: MQTTnet (already in use)
- Background Service: IHostedService for MQTT subscriber
- Database: Entity Framework Core (existing)

#### 3.3.3 Functional Requirements

**3.3.3.1 MQTT Topic Structure**

```
# Fixed Readers
zebra/{readerName}/data          # Tag read events
zebra/{readerName}/events         # Management events (heartbeat)
zebra/{readerName}/ctrl/cmd       # Control commands (from server)
zebra/{readerName}/ctrl/res       # Control responses

# Mobile Devices (Portable Gates)
zebra/mobile/{deviceId}/data      # Tag read events from mobile
zebra/mobile/{deviceId}/status    # Mobile device status
zebra/mobile/{deviceId}/cmd       # Commands to mobile device
```

**3.3.3.2 Tag Read Processing**

```csharp
// Pseudo-code for tag processing
public class TagReadProcessor : IHostedService
{
    public void ProcessTagRead(string clientId, TagReadEvent tagEvent)
    {
        // 1. Determine if fixed reader or mobile device
        var isMobile = clientId.StartsWith("mobile/");
        
        // 2. Get gate/reader info
        var gate = GetGateByReader(clientId);
        
        // 3. Look up asset by tag EPC (don't auto-create!)
        var asset = assetRepository.GetByTagIdentifier(tagEvent.IdHex);
        
        if (asset == null)
        {
            // Unregistered tag - log and optionally notify
            logger.LogWarning($"Unregistered tag: {tagEvent.IdHex} at {gate.Name}");
            return;
        }
        
        // 4. Update asset tracking
        asset.LastDiscoveredAt = DateTime.UtcNow;
        asset.LastDiscoveredBy = gate.Name;
        asset.CurrentLocation = gate.Location;
        
        // 5. Record movement
        var movement = new AssetMovement
        {
            Asset = asset,
            FromLocation = asset.PreviousLocation,
            ToLocation = gate.Location,
            Gate = gate,
            ReadTimestamp = DateTime.UtcNow,
            ReaderId = clientId
        };
        
        // 6. Save to database
        unitOfWork.AssetMovementRepository.Add(movement);
        unitOfWork.SaveChanges();
        
        // 7. Push to SignalR for live feed
        await signalRHub.Clients.All.SendAsync("TagRead", new {
            TagId = tagEvent.IdHex,
            AssetName = asset.Name,
            Gate = gate.Name,
            Timestamp = DateTime.UtcNow,
            Owner = asset.Owner
        });
    }
}
```

**3.3.3.3 Reader Management**
- Auto-register readers on heartbeat
- Track reader status (online/offline)
- Configure readers on startup
- Health monitoring

**3.3.3.4 Background Services**
- MQTT Subscriber Service (IHostedService)
- SignalR Hub for real-time updates
- Scheduled tasks (cleanup, reports)

---

## 4. Data Models

### 4.1 Core Entities

```csharp
// Asset Entity
public class Asset
{
    public int Id { get; set; }
    public string AssetNumber { get; set; }        // Unique asset identifier
    public string Name { get; set; }
    public string Description { get; set; }
    public string TagIdentifier { get; set; }      // RFID EPC (unique, indexed)
    public string Owner { get; set; }               // Person/department
    public string Department { get; set; }
    public DateTime? LastDiscoveredAt { get; set; }
    public string LastDiscoveredBy { get; set; }    // Gate/Reader name
    public StorageUnit CurrentLocation { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }             // Soft delete
}

// Gate Entity
public class Gate
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public StorageUnit Location { get; set; }
    public List<Equipment> Readers { get; set; }    // Multiple readers per gate
    public bool IsActive { get; set; }
}

// Equipment (Reader) Entity
public class Equipment
{
    public int Id { get; set; }
    public string Name { get; set; }                // Reader name (clientId)
    public string Description { get; set; }
    public Gate Gate { get; set; }                  // Link to gate
    public int? GateId { get; set; }
    public bool IsMobile { get; set; }              // Mobile device flag
    public DateTime? LastHeartbeat { get; set; }
    public bool IsOnline { get; set; }
}

// Asset Movement Entity
public class AssetMovement
{
    public int Id { get; set; }
    public Asset Asset { get; set; }
    public StorageUnit FromLocation { get; set; }
    public StorageUnit ToLocation { get; set; }
    public Gate Gate { get; set; }
    public Equipment Reader { get; set; }
    public DateTime ReadTimestamp { get; set; }
    public string ReaderId { get; set; }            // Reader name/device ID
}

// Storage Unit (Location) Entity
public class StorageUnit
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Direction Direction { get; set; }       // Inbound/Outbound/None
}
```

### 4.2 Database Schema

```sql
-- Assets Table
CREATE TABLE Assets (
    Id INT PRIMARY KEY IDENTITY(1,1),
    AssetNumber NVARCHAR(100) UNIQUE NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    TagIdentifier NVARCHAR(100) UNIQUE NOT NULL,  -- Indexed
    Owner NVARCHAR(100),
    Department NVARCHAR(100),
    LastDiscoveredAt DATETIME2,
    LastDiscoveredBy NVARCHAR(100),
    CurrentLocationId INT,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (CurrentLocationId) REFERENCES StorageUnits(Id)
);

-- Gates Table
CREATE TABLE Gates (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) UNIQUE NOT NULL,
    Description NVARCHAR(500),
    LocationId INT,
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (LocationId) REFERENCES StorageUnits(Id)
);

-- Equipments Table (Updated)
CREATE TABLE Equipments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) UNIQUE NOT NULL,
    Description NVARCHAR(500),
    GateId INT,
    IsMobile BIT DEFAULT 0,
    LastHeartbeat DATETIME2,
    IsOnline BIT DEFAULT 0,
    FOREIGN KEY (GateId) REFERENCES Gates(Id)
);

-- AssetMovements Table
CREATE TABLE AssetMovements (
    Id INT PRIMARY KEY IDENTITY(1,1),
    AssetId INT NOT NULL,
    FromLocationId INT,
    ToLocationId INT NOT NULL,
    GateId INT,
    ReaderId INT,
    ReaderIdString NVARCHAR(100),  -- For mobile devices
    ReadTimestamp DATETIME2 NOT NULL,
    FOREIGN KEY (AssetId) REFERENCES Assets(Id),
    FOREIGN KEY (FromLocationId) REFERENCES StorageUnits(Id),
    FOREIGN KEY (ToLocationId) REFERENCES StorageUnits(Id),
    FOREIGN KEY (GateId) REFERENCES Gates(Id),
    FOREIGN KEY (ReaderId) REFERENCES Equipments(Id)
);

-- Indexes
CREATE INDEX IX_Assets_TagIdentifier ON Assets(TagIdentifier);
CREATE INDEX IX_Assets_Owner ON Assets(Owner);
CREATE INDEX IX_Assets_LastDiscoveredAt ON Assets(LastDiscoveredAt);
CREATE INDEX IX_AssetMovements_AssetId ON AssetMovements(AssetId);
CREATE INDEX IX_AssetMovements_ReadTimestamp ON AssetMovements(ReadTimestamp);
```

---

## 5. Integration Points

### 5.1 MQTT Integration
- Fixed readers publish to: `zebra/{readerName}/data`
- Mobile devices publish to: `zebra/mobile/{deviceId}/data`
- Backend subscribes to: `zebra/#` (all topics)
- Backend publishes commands to: `zebra/{readerName}/ctrl/cmd`

### 5.2 REST API Integration
- Web app consumes REST API
- Mobile app syncs via REST API
- Authentication: JWT Bearer tokens
- API versioning: `/api/v1/...`

### 5.3 Real-time Integration
- SignalR hub for web app live feed
- WebSocket connection for real-time updates
- Push notifications for mobile app (optional)

---

## 6. Technical Requirements

### 6.1 Performance Requirements
- Tag read processing: < 100ms latency
- API response time: < 500ms (95th percentile)
- Support 100+ concurrent tag reads/second
- Database queries optimized with indexes

### 6.2 Scalability Requirements
- Support 5-10 gates with 2-4 readers each
- Support 50+ mobile devices
- Support 10,000+ assets
- Horizontal scaling capability (future)

### 6.3 Security Requirements
- JWT authentication for API
- Role-based access control (RBAC)
- MQTT broker authentication (optional but recommended)
- HTTPS for web app
- Data encryption at rest (database)

### 6.4 Reliability Requirements
- 99.9% uptime
- Automatic reconnection for MQTT
- Database backup strategy
- Offline support for mobile app

---

## 7. Deployment Architecture

### 7.1 Recommended Deployment

```yaml
# docker-compose.yml (Extended)
version: '3.4'

services:
  # Backend API + MQTT Processor
  backend-api:
    build: ./ZebraIoTConnector.Backend.API
    ports:
      - "5000:80"      # HTTP
      - "5001:443"     # HTTPS
    environment:
      - ConnectionStrings__DefaultConnection=...
    depends_on:
      - sql.data
      - mosquitto
    networks:
      - app_net

  # Web App (Frontend)
  web-app:
    build: ./WebApp
    ports:
      - "3000:80"
    environment:
      - REACT_APP_API_URL=http://backend-api
    depends_on:
      - backend-api
    networks:
      - app_net

  # Database
  sql.data:
    image: mcr.microsoft.com/mssql/server:2017-latest
    environment:
      - SA_PASSWORD=...
      - ACCEPT_EULA=Y
    volumes:
      - sql_data:/var/opt/mssql
    ports:
      - "1433:1433"
    networks:
      - app_net

  # MQTT Broker
  mosquitto:
    image: eclipse-mosquitto:latest
    ports:
      - "1883:1883"    # MQTT
      - "9001:9001"    # WebSocket
    volumes:
      - ./mosquitto:/mosquitto/config
      - mosquitto_data:/mosquitto/data
    networks:
      - app_net

volumes:
  sql_data:
  mosquitto_data:

networks:
  app_net:
    driver: bridge
```

### 7.2 Alternative: Combined Backend
- Single ASP.NET Core app (Web API + MQTT subscriber)
- Simpler deployment
- Single process for API and MQTT processing
- Recommended for initial implementation

---

## 8. Development Phases

### Phase 1: Backend Foundation (Weeks 1-2)
- Refactor existing codebase
- Implement Asset entity and repository
- Implement Gate entity
- Update MQTT processing logic
- Create REST API endpoints (basic CRUD)

### Phase 2: Web Application (Weeks 3-5)
- Set up frontend project
- Implement asset management UI
- Implement live feed dashboard
- Implement basic reports
- Integrate SignalR for real-time updates

### Phase 3: Mobile Application (Weeks 6-8)
- Set up mobile project
- Implement RFID scanning
- Implement tagging mode
- Implement portable gate mode
- Implement offline sync

### Phase 4: Integration & Testing (Weeks 9-10)
- End-to-end testing
- Performance optimization
- Security hardening
- User acceptance testing

### Phase 5: Deployment & Documentation (Week 11)
- Production deployment
- User documentation
- Admin documentation
- Training materials

---

## 9. API Specification Examples

### 9.1 Create Asset

```http
POST /api/v1/assets
Content-Type: application/json
Authorization: Bearer {token}

{
  "assetNumber": "AST-001",
  "name": "Laptop Dell XPS 15",
  "description": "Development laptop",
  "tagIdentifier": "E20034120107000000000001",
  "owner": "John Doe",
  "department": "IT"
}

Response: 201 Created
{
  "id": 1,
  "assetNumber": "AST-001",
  "name": "Laptop Dell XPS 15",
  ...
}
```

### 9.2 Get Live Feed (SignalR)

```javascript
// Client connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/api/hub/livefeed")
    .build();

connection.on("TagRead", (data) => {
    console.log("Tag read:", data);
    // Update UI
});

connection.start();
```

---

## 10. Success Criteria

- All assets can be registered and tracked
- Tag reads are processed in real-time (< 100ms)
- Live feed shows tag reads within 1 second
- Mobile app works offline and syncs when online
- Reports generate in < 5 seconds
- System handles 100+ tag reads/second
- 99.9% uptime achieved

---

## 11. Appendices

### 11.1 Technology Recommendations
- Frontend: React.js with TypeScript
- Backend: ASP.NET Core 6+ (extend existing)
- Mobile: .NET MAUI (cross-platform)
- Database: SQL Server 2019+
- MQTT: Mosquitto 2.0+

### 11.2 Third-Party Libraries
- MQTTnet (MQTT client)
- SignalR (real-time)
- Entity Framework Core (ORM)
- AutoMapper (DTO mapping)
- Serilog (logging)

---

End of Specification

---

This specification provides a roadmap for building the asset tracking system. Should I expand any section or provide implementation details for specific components?