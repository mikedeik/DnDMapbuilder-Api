# Implementation Plan: Real-time DM Live Map Display via SignalR WebSocket

## Overview
This implementation adds real-time map viewing capabilities for Dungeon Masters (DMs) to display game maps on a separate screen (TV/projector) during tabletop RPG sessions. The feature uses SignalR for WebSocket-based real-time bidirectional communication between the DM's editing interface and a passive live display view. Only DMs are authenticated - no player accounts or separate authentication is required.

**Core Architecture**: The implementation follows a one-authenticated-user, two-view pattern where the DM uses their existing JWT token to access both the main editing interface and the live display endpoint (`/gamemaps/{mapId}/live`). Map state changes (token movements, grid updates, map switches) are broadcast via SignalR Hub to all connected clients subscribing to that map.

## Architecture Decisions

### 1. Clean Architecture Alignment
- **Domain Layer** (`DnDMapBuilder.Data.Entities`): Add `PublicationStatus` enum to `GameMap` entity
- **Application Layer** (`DnDMapBuilder.Application`): Create `ILiveMapService` and `LiveMapService` for business logic
- **API Layer** (`DnDMapBuilder.Api`): Add `LiveMapsController` for REST endpoints and `GameMapHub` for SignalR
- **Contracts Layer** (`DnDMapBuilder.Contracts`): Add DTOs for live map events and publication status

### 2. SignalR Hub Design Pattern
- **Hub-based Architecture**: Use SignalR's Hub pattern for managing WebSocket connections
- **Map-scoped Groups**: Clients join SignalR groups based on `mapId` for targeted broadcasting
- **Authentication**: Hub connections authenticated via JWT in query string or headers
- **Connection Lifecycle**: Manage connection/disconnection, automatic group cleanup

### 3. Publication Status Pattern
- Add `PublicationStatus` enum: `Draft`, `Live`
- Draft maps are editable but not broadcast to live views
- Setting status to `Live` enables real-time broadcasting
- Status changes trigger SignalR events to live view clients

### 4. Event-Driven Updates
- Map updates in main app trigger `ILiveMapService.BroadcastMapUpdateAsync()`
- Service publishes events to SignalR Hub only if map status is `Live`
- Events: `MapUpdated`, `TokenMoved`, `TokenAdded`, `TokenRemoved`, `MapStatusChanged`

### 5. Testing Strategy
- **Unit Tests**: Service logic, Hub methods, authorization policies (using xUnit + Moq pattern)
- **Integration Tests**: SignalR connection lifecycle, JWT authentication, end-to-end message flow
- **Test Doubles**: Mock `IHubContext<GameMapHub>` for service tests

### 6. Existing Pattern Consistency
- JWT Bearer authentication (already configured in `Program.cs`)
- Repository pattern with interfaces (`IGameMapRepository`)
- Service layer separation (`IGameMapService`, `ILiveMapService`)
- Controller-based REST endpoints with `ApiResponse<T>` wrapper
- Entity Framework Core for persistence
- Moq + FluentAssertions for testing

## Implementation Steps

### Step 1: Add PublicationStatus to GameMap Entity
- **uniqueId**: `step-live-map-001`
- **status**: `done`
- **description**: Extend the `GameMap` entity with a `PublicationStatus` property to track whether a map is in Draft or Live state. This foundational change enables DMs to control when maps are broadcast to live views.
- **TDD Approach**:
  - **Test Cases**:
    - Unit test: Create `GameMap` with default `PublicationStatus` of `Draft`
    - Unit test: Update `GameMap.PublicationStatus` to `Live` and verify persistence
    - Integration test: Query maps filtered by `PublicationStatus`
  - **Implementation Guidelines**:
    1. Create `PublicationStatus` enum in `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Data/Entities/PublicationStatus.cs` with values: `Draft = 0`, `Live = 1`
    2. Add property to `GameMap.cs`: `public PublicationStatus PublicationStatus { get; set; } = PublicationStatus.Draft;`
    3. Create EF Core migration: `dotnet ef migrations add AddPublicationStatusToGameMap --project src/DnDMapBuilder.Data --startup-project src/DnDMapBuilder.Api`
    4. Update `GameMapDto` record in `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Contracts/DTOs/GameMapDto.cs` to include `PublicationStatus PublicationStatus`
    5. Update mapping extension in `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Application/Mappings/GameMapMappings.cs` (create if not exists) to map `PublicationStatus`
  - **Acceptance Criteria**:
    - Migration applies successfully without errors
    - Existing maps default to `Draft` status
    - `GameMapDto` includes `PublicationStatus` in serialization
    - No breaking changes to existing API responses
- **Dependencies**: None
- **Estimated Scope**: Small

### Step 2: Add SignalR NuGet Package and Configure Hub Infrastructure
- **uniqueId**: `step-live-map-002`
- **status**: `done`
- **description**: Install SignalR package, configure middleware in `Program.cs`, and create base `GameMapHub` class with authentication. Establish SignalR pipeline before implementing business logic.
- **TDD Approach**:
  - **Test Cases**:
    - Integration test: Verify SignalR endpoint is registered at `/hubs/gamemap`
    - Integration test: Unauthenticated connection is rejected with 401
    - Integration test: Authenticated connection with valid JWT succeeds
    - Unit test: Hub `OnConnectedAsync` logs connection with user ID from claims
  - **Implementation Guidelines**:
    1. Add package reference to `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Api/DnDMapBuilder.Api.csproj`: `<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.1.0" />`
    2. Create `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Api/Hubs/GameMapHub.cs`:
       ```csharp
       [Authorize]
       public class GameMapHub : Hub
       {
           private readonly ILogger<GameMapHub> _logger;
           public GameMapHub(ILogger<GameMapHub> logger) { _logger = logger; }

           public override Task OnConnectedAsync()
           {
               var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
               _logger.LogInformation("User {UserId} connected to GameMapHub", userId);
               return base.OnConnectedAsync();
           }
       }
       ```
    3. In `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Api/Program.cs`, add after line 35: `builder.Services.AddSignalR();`
    4. In `Program.cs`, add before `app.Run()`: `app.MapHub<GameMapHub>("/hubs/gamemap");`
    5. Configure JWT authentication for SignalR by adding to JWT options (line 81-93):
       ```csharp
       options.Events = new JwtBearerEvents
       {
           OnMessageReceived = context =>
           {
               var accessToken = context.Request.Query["access_token"];
               var path = context.HttpContext.Request.Path;
               if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
               {
                   context.Token = accessToken;
               }
               return Task.CompletedTask;
           }
       };
       ```
  - **Acceptance Criteria**:
    - SignalR services registered in DI container
    - Hub endpoint accessible at `/hubs/gamemap`
    - JWT token from query string authenticates SignalR connections
    - Hub rejects connections without valid JWT
- **Dependencies**: None
- **Estimated Scope**: Small

### Step 3: Implement Map Group Management in GameMapHub
- **uniqueId**: `step-live-map-003`
- **status**: `done`
- **description**: Add SignalR group management methods to allow clients to subscribe/unsubscribe to specific map updates. Clients join a group named `map_{mapId}` to receive targeted broadcasts.
- **TDD Approach**:
  - **Test Cases**:
    - Unit test: `JoinMapGroup(mapId)` adds connection to correct group
    - Unit test: `LeaveMapGroup(mapId)` removes connection from group
    - Unit test: Multiple clients can join same map group
    - Integration test: Client receives messages only for subscribed map groups
    - Unit test: Verify user has access to map before joining group (check ownership via campaign)
  - **Implementation Guidelines**:
    1. Inject `IGameMapService` into `GameMapHub` constructor for authorization checks
    2. Add method to `GameMapHub.cs`:
       ```csharp
       public async Task JoinMapGroup(string mapId)
       {
           var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
           if (userId == null) throw new HubException("Unauthorized");

           // Verify user has access to this map
           var map = await _gameMapService.GetByIdAsync(mapId, userId);
           if (map == null) throw new HubException("Map not found or access denied");

           await Groups.AddToGroupAsync(Context.ConnectionId, $"map_{mapId}");
           _logger.LogInformation("User {UserId} joined map group {MapId}", userId, mapId);
       }

       public async Task LeaveMapGroup(string mapId)
       {
           await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"map_{mapId}");
           _logger.LogInformation("Connection {ConnectionId} left map group {MapId}", Context.ConnectionId, mapId);
       }
       ```
    3. Override `OnDisconnectedAsync` to log disconnections
  - **Acceptance Criteria**:
    - Clients can join/leave map groups dynamically
    - Authorization check prevents unauthorized map access
    - Connection is automatically cleaned up on disconnect
    - Group names follow consistent pattern: `map_{mapId}`
- **Dependencies**: `step-live-map-002`
- **Estimated Scope**: Small

### Step 4: Create Live Map Event DTOs and Contracts
- **uniqueId**: `step-live-map-004`
- **status**: `done`
- **description**: Define strongly-typed DTOs for SignalR events (map updates, token movements, status changes). These contracts ensure type safety between server and client.
- **TDD Approach**:
  - **Test Cases**:
    - Unit test: Serialize `MapUpdatedEvent` to JSON and verify all properties present
    - Unit test: Deserialize JSON to `TokenMovedEvent` and validate data integrity
    - Unit test: Validate that event DTOs include timestamp for client-side ordering
  - **Implementation Guidelines**:
    1. Create `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Contracts/Events/LiveMapEvents.cs`:
       ```csharp
       namespace DnDMapBuilder.Contracts.Events;

       public record MapUpdatedEvent(
           string MapId,
           string Name,
           int Rows,
           int Cols,
           string GridColor,
           double GridOpacity,
           string? ImageUrl,
           DateTime Timestamp
       );

       public record TokenMovedEvent(
           string MapId,
           string TokenInstanceId,
           int X,
           int Y,
           DateTime Timestamp
       );

       public record TokenAddedEvent(
           string MapId,
           string TokenInstanceId,
           string TokenId,
           int X,
           int Y,
           DateTime Timestamp
       );

       public record TokenRemovedEvent(
           string MapId,
           string TokenInstanceId,
           DateTime Timestamp
       );

       public record MapStatusChangedEvent(
           string MapId,
           PublicationStatus NewStatus,
           DateTime Timestamp
       );

       public record MapStateSnapshot(
           GameMapDto Map,
           DateTime Timestamp
       );
       ```
    2. Ensure `PublicationStatus` enum is in Contracts project for sharing
  - **Acceptance Criteria**:
    - All event records are immutable (using `record` keyword)
    - Events include timestamp for ordering/debugging
    - DTOs serialize/deserialize correctly in JSON
    - No circular references in navigation properties
- **Dependencies**: `step-live-map-001`
- **Estimated Scope**: Small

### Step 5: Create ILiveMapService Interface and Implementation
- **uniqueId**: `step-live-map-005`
- **status**: `done`
- **description**: Implement service layer to encapsulate live map business logic including broadcasting events, managing publication status, and providing map state snapshots for new connections.
- **TDD Approach**:
  - **Test Cases**:
    - Unit test: `BroadcastMapUpdateAsync()` calls Hub's `SendAsync` with correct event to correct group
    - Unit test: `BroadcastMapUpdateAsync()` does NOT broadcast if map status is `Draft`
    - Unit test: `BroadcastMapUpdateAsync()` DOES broadcast if map status is `Live`
    - Unit test: `BroadcastTokenMovedAsync()` constructs correct `TokenMovedEvent`
    - Unit test: `SetMapPublicationStatusAsync()` updates entity and broadcasts `MapStatusChangedEvent`
    - Unit test: `GetMapStateSnapshotAsync()` returns full map with tokens for Live maps only
    - Unit test: Service throws `UnauthorizedAccessException` if user doesn't own campaign
  - **Implementation Guidelines**:
    1. Create `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Application/Interfaces/ILiveMapService.cs`:
       ```csharp
       public interface ILiveMapService
       {
           Task BroadcastMapUpdateAsync(string mapId, CancellationToken cancellationToken = default);
           Task BroadcastTokenMovedAsync(string mapId, string tokenInstanceId, int x, int y, CancellationToken cancellationToken = default);
           Task BroadcastTokenAddedAsync(string mapId, string tokenInstanceId, CancellationToken cancellationToken = default);
           Task BroadcastTokenRemovedAsync(string mapId, string tokenInstanceId, CancellationToken cancellationToken = default);
           Task SetMapPublicationStatusAsync(string mapId, PublicationStatus status, string userId, CancellationToken cancellationToken = default);
           Task<MapStateSnapshot?> GetMapStateSnapshotAsync(string mapId, string userId, CancellationToken cancellationToken = default);
       }
       ```
    2. Create `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Application/Services/LiveMapService.cs`:
       ```csharp
       public class LiveMapService : ILiveMapService
       {
           private readonly IGameMapRepository _mapRepository;
           private readonly IHubContext<GameMapHub> _hubContext;
           private readonly IGameMapService _gameMapService;
           private readonly ILogger<LiveMapService> _logger;

           // Constructor with DI

           public async Task BroadcastMapUpdateAsync(string mapId, CancellationToken cancellationToken = default)
           {
               var map = await _mapRepository.GetWithTokensAsync(mapId, cancellationToken);
               if (map == null || map.PublicationStatus != PublicationStatus.Live) return;

               var evt = new MapUpdatedEvent(map.Id, map.Name, map.Rows, map.Cols, map.GridColor, map.GridOpacity, map.ImageUrl, DateTime.UtcNow);
               await _hubContext.Clients.Group($"map_{mapId}").SendAsync("MapUpdated", evt, cancellationToken);
           }

           // Implement other methods similarly...
       }
       ```
    3. Register in `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Api/Program.cs` (after line 145): `builder.Services.AddScoped<ILiveMapService, LiveMapService>();`
  - **Acceptance Criteria**:
    - Service only broadcasts if map status is `Live`
    - All broadcasts target correct SignalR group: `map_{mapId}`
    - Service validates user authorization before state changes
    - Exceptions are logged appropriately
    - All async methods use cancellation tokens
- **Dependencies**: `step-live-map-003`, `step-live-map-004`
- **Estimated Scope**: Medium

### Step 6: Integrate LiveMapService Broadcasts into GameMapService
- **uniqueId**: `step-live-map-006`
- **status**: `done`
- **description**: Modify existing `GameMapService.UpdateAsync()` method to trigger real-time broadcasts via `ILiveMapService` when maps or tokens are updated.
- **TDD Approach**:
  - **Test Cases**:
    - Unit test: `UpdateAsync()` calls `BroadcastMapUpdateAsync()` after successful update
    - Unit test: Token position update triggers `BroadcastTokenMovedAsync()`
    - Unit test: Broadcast is NOT called if update fails
    - Unit test: Broadcast is NOT called if map status is `Draft`
    - Integration test: End-to-end map update broadcasts event to SignalR clients
  - **Implementation Guidelines**:
    1. Inject `ILiveMapService` into `GameMapService` constructor in `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Application/Services/GameMapService.cs`
    2. In `UpdateAsync()` method (after line 155), add:
       ```csharp
       // Broadcast to live views if map is Live
       await _liveMapService.BroadcastMapUpdateAsync(id, cancellationToken);
       ```
    3. Consider broadcasting individual token movements if updating only tokens (optimize for performance)
    4. Update unit tests in `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.UnitTests/Services/GameMapServiceTests.cs` (create if not exists) to verify broadcast calls
  - **Acceptance Criteria**:
    - Map updates automatically broadcast to SignalR clients
    - No performance degradation for Draft maps
    - Service remains testable with mocked `ILiveMapService`
    - Broadcasts are fire-and-forget (don't block update response)
- **Dependencies**: `step-live-map-005`
- **Estimated Scope**: Small

### Step 7: Create LiveMapsController with Publication Status Endpoints
- **uniqueId**: `step-live-map-007`
- **status**: `done`
- **description**: Add REST API controller for managing map publication status (Draft/Live) and retrieving live map state snapshots. Provides endpoints for DM to control which maps are broadcast.
- **TDD Approach**:
  - **Test Cases**:
    - Unit test: `PUT /api/v1/livemaps/{mapId}/status` returns 200 with updated map
    - Unit test: Endpoint rejects unauthorized user with 403
    - Unit test: `GET /api/v1/livemaps/{mapId}/snapshot` returns full map state for Live maps
    - Unit test: `GET /api/v1/livemaps/{mapId}/snapshot` returns 404 for Draft maps
    - Integration test: Status change triggers SignalR broadcast
  - **Implementation Guidelines**:
    1. Create `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Api/Controllers/LiveMapsController.cs`:
       ```csharp
       [ApiVersion("1.0")]
       [Authorize]
       [ApiController]
       [Route("api/v{version:apiVersion}/[controller]")]
       public class LiveMapsController : ControllerBase
       {
           private readonly ILiveMapService _liveMapService;

           [HttpPut("{mapId}/status")]
           [ResponseCache(CacheProfileName = "NoCache")]
           public async Task<ActionResult<ApiResponse<bool>>> SetPublicationStatus(
               string mapId,
               [FromBody] SetPublicationStatusRequest request,
               CancellationToken cancellationToken)
           {
               var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
               await _liveMapService.SetMapPublicationStatusAsync(mapId, request.Status, userId, cancellationToken);
               return Ok(new ApiResponse<bool>(true, true, "Publication status updated."));
           }

           [HttpGet("{mapId}/snapshot")]
           [ResponseCache(CacheProfileName = "NoCache")]
           public async Task<ActionResult<ApiResponse<MapStateSnapshot>>> GetSnapshot(
               string mapId,
               CancellationToken cancellationToken)
           {
               var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
               var snapshot = await _liveMapService.GetMapStateSnapshotAsync(mapId, userId, cancellationToken);

               if (snapshot == null)
                   return NotFound(new ApiResponse<MapStateSnapshot>(false, null, "Map not found or not live."));

               return Ok(new ApiResponse<MapStateSnapshot>(true, snapshot));
           }
       }
       ```
    2. Create `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Contracts/Requests/SetPublicationStatusRequest.cs`:
       ```csharp
       public record SetPublicationStatusRequest(PublicationStatus Status);
       ```
  - **Acceptance Criteria**:
    - Endpoints follow existing API conventions (versioning, authorization, response wrapping)
    - Status changes are audited in logs
    - Snapshot endpoint returns 404 for Draft maps
    - All endpoints require valid JWT authentication
- **Dependencies**: `step-live-map-005`
- **Estimated Scope**: Small

### Step 8: Add Live View Endpoint for Frontend Display
- **uniqueId**: `step-live-map-008`
- **status**: `pending`
- **description**: Create a dedicated MVC endpoint `/gamemaps/{mapId}/live` that serves an HTML page for the live display view. This page authenticates using the DM's token from browser storage and connects to SignalR.
- **TDD Approach**:
  - **Test Cases**:
    - Integration test: `GET /gamemaps/{mapId}/live` returns 200 with HTML content
    - Integration test: Endpoint requires authentication (redirects to login if no token)
    - Integration test: Page includes SignalR client library script reference
    - Integration test: Page renders initial map state from snapshot API
  - **Implementation Guidelines**:
    1. Create `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Api/Controllers/LiveViewController.cs`:
       ```csharp
       [Authorize]
       [Route("gamemaps")]
       public class LiveViewController : Controller
       {
           [HttpGet("{mapId}/live")]
           public IActionResult LiveView(string mapId)
           {
               ViewData["MapId"] = mapId;
               return View("LiveView");
           }
       }
       ```
    2. Create `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Api/Views/Shared/_Layout.cshtml` (basic layout with SignalR CDN)
    3. Create `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Api/Views/LiveView.cshtml`:
       - Include SignalR client: `<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@latest/dist/browser/signalr.min.js"></script>`
       - JavaScript to establish SignalR connection with JWT from localStorage/cookie
       - Canvas/grid rendering logic for map display
       - Event listeners for `MapUpdated`, `TokenMoved`, etc.
    4. Enable MVC in `Program.cs` (line 33): Change `AddControllers()` to `AddControllersWithViews()` and add `app.MapControllers().MapDefaultControllerRoute();`
  - **Acceptance Criteria**:
    - Live view page is accessible at `/gamemaps/{mapId}/live`
    - Page authenticates using DM's existing JWT token
    - SignalR connection established automatically on page load
    - Page renders initial map state from snapshot API
    - Real-time updates display without page refresh
- **Dependencies**: `step-live-map-003`, `step-live-map-007`
- **Estimated Scope**: Medium (includes frontend JavaScript)

### Step 9: Write Comprehensive Unit Tests for LiveMapService
- **uniqueId**: `step-live-map-009`
- **status**: `pending`
- **description**: Create full unit test suite for `LiveMapService` covering all broadcast methods, authorization checks, and edge cases. Follow existing test patterns with Moq and FluentAssertions.
- **TDD Approach**:
  - **Test Cases**:
    - Test: `BroadcastMapUpdateAsync()` with Live map calls Hub SendAsync
    - Test: `BroadcastMapUpdateAsync()` with Draft map does NOT call Hub SendAsync
    - Test: `BroadcastMapUpdateAsync()` with null map logs warning and returns
    - Test: `SetMapPublicationStatusAsync()` with unauthorized user throws exception
    - Test: `SetMapPublicationStatusAsync()` broadcasts status change event
    - Test: `GetMapStateSnapshotAsync()` returns null for Draft maps
    - Test: `GetMapStateSnapshotAsync()` returns full snapshot for Live maps
    - Test: All methods handle cancellation tokens correctly
    - Test: Broadcast methods log events at appropriate levels
  - **Implementation Guidelines**:
    1. Create `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.UnitTests/Services/LiveMapServiceTests.cs`:
       ```csharp
       public class LiveMapServiceTests
       {
           private readonly Mock<IGameMapRepository> _mockMapRepository;
           private readonly Mock<IHubContext<GameMapHub>> _mockHubContext;
           private readonly Mock<IGameMapService> _mockGameMapService;
           private readonly Mock<ILogger<LiveMapService>> _mockLogger;
           private readonly LiveMapService _service;

           public LiveMapServiceTests()
           {
               _mockMapRepository = new Mock<IGameMapRepository>();
               _mockHubContext = new Mock<IHubContext<GameMapHub>>();
               _mockGameMapService = new Mock<IGameMapService>();
               _mockLogger = new Mock<ILogger<LiveMapService>>();

               // Mock IHubContext setup
               var mockClients = new Mock<IHubClients>();
               var mockGroup = new Mock<IClientProxy>();
               _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
               mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockGroup.Object);

               _service = new LiveMapService(_mockMapRepository.Object, _mockHubContext.Object, _mockGameMapService.Object, _mockLogger.Object);
           }

           [Fact]
           public async Task BroadcastMapUpdateAsync_WithLiveMap_CallsHubSendAsync()
           {
               // Arrange
               var map = new GameMap { Id = "map1", PublicationStatus = PublicationStatus.Live, Name = "Test Map" };
               _mockMapRepository.Setup(r => r.GetWithTokensAsync("map1", default)).ReturnsAsync(map);

               // Act
               await _service.BroadcastMapUpdateAsync("map1");

               // Assert
               _mockHubContext.Verify(h => h.Clients.Group("map_map1").SendAsync("MapUpdated", It.IsAny<MapUpdatedEvent>(), default), Times.Once);
           }

           // Additional tests...
       }
       ```
    2. Follow existing test structure from `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.UnitTests/Services/AuthServiceTests.cs`
  - **Acceptance Criteria**:
    - All public methods have corresponding unit tests
    - Test coverage > 90% for `LiveMapService`
    - Tests use Arrange-Act-Assert pattern
    - Mock verifications confirm Hub interactions
    - FluentAssertions used for readable assertions
- **Dependencies**: `step-live-map-005`
- **Estimated Scope**: Medium

### Step 10: Write Integration Tests for SignalR Hub and Live Endpoints
- **uniqueId**: `step-live-map-010`
- **status**: `pending`
- **description**: Create integration tests validating end-to-end SignalR connection lifecycle, JWT authentication flow, and live endpoint behavior using WebApplicationFactory pattern.
- **TDD Approach**:
  - **Test Cases**:
    - Test: SignalR connection with valid JWT succeeds
    - Test: SignalR connection without JWT returns 401
    - Test: Client can join map group after authentication
    - Test: Client receives MapUpdated event after joining group
    - Test: Client does NOT receive events for other map groups
    - Test: `PUT /api/v1/livemaps/{mapId}/status` triggers SignalR broadcast
    - Test: `GET /api/v1/livemaps/{mapId}/snapshot` returns correct data for Live map
    - Test: Multiple clients receive same broadcast event
  - **Implementation Guidelines**:
    1. Create `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.IntegrationTests/Hubs/GameMapHubIntegrationTests.cs`:
       ```csharp
       public class GameMapHubIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
       {
           private readonly WebApplicationFactory<Program> _factory;

           [Fact]
           public async Task SignalRConnection_WithValidJwt_Succeeds()
           {
               // Arrange
               var client = _factory.CreateClient();
               var token = await GetAuthTokenAsync(client);

               // Act
               var hubConnection = new HubConnectionBuilder()
                   .WithUrl($"{_factory.Server.BaseAddress}hubs/gamemap?access_token={token}")
                   .Build();

               await hubConnection.StartAsync();

               // Assert
               hubConnection.State.Should().Be(HubConnectionState.Connected);

               // Cleanup
               await hubConnection.StopAsync();
           }

           // Additional tests...
       }
       ```
    2. Follow pattern from existing integration tests in `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.IntegrationTests/Controllers/HealthCheckIntegrationTests.cs`
    3. Add `Microsoft.AspNetCore.SignalR.Client` package to IntegrationTests project for SignalR client
  - **Acceptance Criteria**:
    - All integration tests pass in isolated test database
    - Tests clean up connections and resources
    - JWT token generation helper method reusable across tests
    - Tests validate both success and failure paths
- **Dependencies**: `step-live-map-007`, `step-live-map-003`
- **Estimated Scope**: Large

### Step 11: Update Existing GameMapController to Support Publication Status
- **uniqueId**: `step-live-map-011`
- **status**: `pending`
- **description**: Modify `GameMapsController` to include `PublicationStatus` in responses and allow setting status during map creation/updates.
- **TDD Approach**:
  - **Test Cases**:
    - Unit test: `CreateMap()` accepts optional `PublicationStatus` parameter (defaults to Draft)
    - Unit test: `UpdateMap()` can change `PublicationStatus`
    - Unit test: `GetMap()` includes `PublicationStatus` in response
    - Integration test: End-to-end map creation with `PublicationStatus.Live`
  - **Implementation Guidelines**:
    1. Update `CreateMapRequest` in `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Contracts/Requests/CreateMapRequest.cs` to include optional `PublicationStatus PublicationStatus = PublicationStatus.Draft`
    2. Update `UpdateMapRequest` in `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Contracts/Requests/UpdateMapRequest.cs` to include `PublicationStatus PublicationStatus`
    3. Update `GameMapService.CreateAsync()` and `UpdateAsync()` to handle `PublicationStatus`
    4. Update controller tests to verify new field handling
  - **Acceptance Criteria**:
    - Existing API consumers not affected (backwards compatible)
    - `PublicationStatus` serialized correctly in JSON responses
    - Map creation defaults to `Draft` if not specified
    - Update allows changing status (also broadcasts if changed to Live)
- **Dependencies**: `step-live-map-001`
- **Estimated Scope**: Small

### Step 12: Add Logging and Telemetry for Live Map Operations
- **uniqueId**: `step-live-map-012`
- **status**: `pending`
- **description**: Instrument `LiveMapService` and `GameMapHub` with structured logging and telemetry for monitoring connection counts, broadcast performance, and error rates.
- **TDD Approach**:
  - **Test Cases**:
    - Unit test: Verify log messages emitted at correct levels (Info, Warning, Error)
    - Unit test: Verify telemetry counters increment on events (map updates, connections)
    - Integration test: Logs written to configured sink (can verify via test output)
  - **Implementation Guidelines**:
    1. Add structured logging to `LiveMapService` methods:
       ```csharp
       _logger.LogInformation("Broadcasting map update for map {MapId}, status {Status}", mapId, map.PublicationStatus);
       _logger.LogWarning("Attempted broadcast for Draft map {MapId}, skipping", mapId);
       ```
    2. Add logging to `GameMapHub` connection lifecycle:
       ```csharp
       _logger.LogInformation("User {UserId} joined map group {MapId} (ConnectionId: {ConnectionId})", userId, mapId, Context.ConnectionId);
       _logger.LogError(ex, "Error in GameMapHub.JoinMapGroup for map {MapId}", mapId);
       ```
    3. Consider adding custom metrics/counters using existing telemetry infrastructure in `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Infrastructure/Telemetry/TelemetryService.cs`
  - **Acceptance Criteria**:
    - All significant operations logged with context (userId, mapId, connectionId)
    - Errors logged with exception details
    - Logs follow structured format for easy querying
    - No sensitive data (tokens, passwords) in logs
- **Dependencies**: `step-live-map-005`, `step-live-map-003`
- **Estimated Scope**: Small

### Step 13: Performance Optimization - Implement Broadcast Throttling
- **uniqueId**: `step-live-map-013`
- **status**: `pending`
- **description**: Add throttling/debouncing to prevent excessive SignalR broadcasts when DM makes rapid consecutive updates (e.g., dragging multiple tokens). Use sliding window to batch updates.
- **TDD Approach**:
  - **Test Cases**:
    - Unit test: Rapid consecutive calls to `BroadcastTokenMovedAsync()` result in single broadcast
    - Unit test: Broadcasts after throttle window trigger immediately
    - Unit test: Different maps don't interfere with each other's throttle windows
    - Integration test: Token drag with 10 position updates results in ~2-3 broadcasts (not 10)
  - **Implementation Guidelines**:
    1. Use `System.Threading.Channels` or `System.Reactive` for buffering events
    2. Add configuration for throttle window: `"LiveMap:ThrottleWindowMs": 100` in appsettings.json
    3. Implement per-map throttle state in `LiveMapService`:
       ```csharp
       private readonly ConcurrentDictionary<string, SemaphoreSlim> _throttleLocks = new();

       public async Task BroadcastTokenMovedAsync(string mapId, string tokenInstanceId, int x, int y, CancellationToken cancellationToken)
       {
           var throttleLock = _throttleLocks.GetOrAdd(mapId, _ => new SemaphoreSlim(1, 1));
           if (!await throttleLock.WaitAsync(0)) return; // Skip if already broadcasting

           try
           {
               // Broadcast logic
               await Task.Delay(100, cancellationToken); // Throttle window
           }
           finally
           {
               throttleLock.Release();
           }
       }
       ```
    4. Document throttling behavior for frontend team
  - **Acceptance Criteria**:
    - Rapid updates don't overwhelm SignalR connection
    - Latency remains under 200ms for typical operations
    - Memory usage stable under load (no leak from throttle state)
    - Throttle window configurable via appsettings
- **Dependencies**: `step-live-map-005`
- **Estimated Scope**: Medium

### Step 14: Create Frontend API Documentation
- **uniqueId**: `step-live-map-014`
- **status**: `pending`
- **description**: Generate comprehensive markdown documentation for frontend developers covering REST endpoints, SignalR hub methods, DTOs, authentication flow, and example code snippets.
- **TDD Approach**:
  - **Test Cases**: N/A (documentation task)
  - **Implementation Guidelines**:
    1. Create `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/docs/LIVE_MAP_API.md` with following sections:
       - **Overview**: Feature description and architecture
       - **Authentication**: JWT token usage in REST and SignalR
       - **REST Endpoints**:
         - `PUT /api/v1/livemaps/{mapId}/status` - Set publication status
         - `GET /api/v1/livemaps/{mapId}/snapshot` - Get current map state
       - **SignalR Hub**: `/hubs/gamemap`
         - Connection setup with JWT in query string
         - Hub methods: `JoinMapGroup(mapId)`, `LeaveMapGroup(mapId)`
         - Server-to-client events: `MapUpdated`, `TokenMoved`, `TokenAdded`, `TokenRemoved`, `MapStatusChanged`
       - **DTOs and Data Structures**:
         - `GameMapDto` schema
         - `MapUpdatedEvent` schema
         - `TokenMovedEvent` schema
         - `MapStateSnapshot` schema
         - `PublicationStatus` enum values
       - **Code Examples**:
         - JavaScript: Establishing SignalR connection
         - JavaScript: Subscribing to map updates
         - JavaScript: Fetching map snapshot on initial load
         - cURL: Setting map publication status
       - **Error Handling**: Common error codes and messages
       - **Performance Notes**: Throttling behavior, recommended update frequencies
    2. Include OpenAPI/Swagger annotations in controllers for auto-generated docs
    3. Add Mermaid diagram showing sequence flow: DM edits map → Service broadcasts → SignalR Hub → Live view receives update
  - **Acceptance Criteria**:
    - Documentation is complete, accurate, and matches implementation
    - All endpoints documented with request/response examples
    - SignalR connection setup clearly explained with code
    - DTOs include property types and descriptions
    - Example code is copy-paste ready
    - Mermaid diagrams render correctly in markdown viewers
- **Dependencies**: `step-live-map-001` through `step-live-map-013`
- **Estimated Scope**: Small

## Testing Strategy

### Unit Testing
- **Framework**: xUnit with Moq for mocking, FluentAssertions for assertions
- **Target Coverage**: > 85% code coverage for new services and controllers
- **Key Focus Areas**:
  - Service layer business logic (`LiveMapService`, updated `GameMapService`)
  - Authorization checks (ownership validation)
  - Hub method behavior (group management, disconnection handling)
  - Event construction and serialization

### Integration Testing
- **Framework**: xUnit with `WebApplicationFactory<Program>`
- **Test Database**: EF Core In-Memory provider for isolated tests
- **Key Scenarios**:
  - End-to-end SignalR connection with JWT authentication
  - REST API calls triggering SignalR broadcasts
  - Multiple clients receiving broadcasts in correct groups
  - Database persistence of `PublicationStatus` changes

### Manual Testing Checklist
- DM can set map to Live and see it broadcast
- Live view page displays map on separate screen/browser tab
- Token movements in main app update live view in real-time (< 500ms latency)
- Multiple maps can be Live simultaneously without cross-contamination
- Disconnecting live view doesn't affect main editing session
- Draft maps don't broadcast to any live views

## Risk Mitigation

### Risk 1: SignalR Connection Stability
- **Mitigation**: Implement automatic reconnection logic in client JavaScript with exponential backoff
- **Monitoring**: Log connection/disconnection events with userId and mapId for troubleshooting
- **Fallback**: If SignalR fails, frontend can poll snapshot endpoint every 2-3 seconds

### Risk 2: Excessive Broadcast Traffic
- **Mitigation**: Implement throttling (Step 13) to limit broadcasts to ~10-20 per second per map
- **Monitoring**: Add telemetry to track broadcast frequency and payload sizes
- **Optimization**: Only broadcast changed properties (delta updates) instead of full map state

### Risk 3: Memory Leaks from SignalR Groups
- **Mitigation**: Ensure `OnDisconnectedAsync` properly removes connections from groups
- **Testing**: Integration test verifying group cleanup after 100+ connect/disconnect cycles
- **Monitoring**: Track active connection count metric in production

### Risk 4: JWT Token Expiration During Long Sessions
- **Mitigation**: Frontend should refresh JWT before expiration and reconnect SignalR with new token
- **Implementation**: Add `RefreshToken` endpoint or extend token expiration for live sessions
- **UX**: Show warning to DM when token is about to expire (e.g., 5 minutes before)

### Risk 5: Race Conditions with Rapid Updates
- **Mitigation**: Use CancellationToken throughout async pipeline to handle overlapping requests
- **Testing**: Unit tests with concurrent calls to broadcast methods
- **Database**: Use optimistic concurrency in EF Core for `GameMap` updates (add `RowVersion` column)

### Risk 6: Cross-Browser Compatibility
- **Mitigation**: Use SignalR's built-in fallback transports (WebSockets → Server-Sent Events → Long Polling)
- **Testing**: Test live view in Chrome, Firefox, Safari, Edge
- **Documentation**: Document minimum browser versions required for WebSocket support

### Risk 7: Authorization Bypass
- **Mitigation**: Double-check authorization at both REST endpoint and SignalR Hub method level
- **Testing**: Security-focused tests attempting to access maps without ownership
- **Code Review**: Ensure all Hub methods call authorization service before group operations

## Database Migration Notes

### Migration: AddPublicationStatusToGameMap
- **Backwards Compatibility**: Existing maps will default to `PublicationStatus.Draft` (enum value 0)
- **Rollback Plan**: If migration causes issues, add `Down()` method to remove column
- **Data Migration**: No data transformation needed; default value is safe for existing records
- **Production Deployment**: Apply migration during low-traffic window; zero downtime expected

## Configuration Requirements

Add to `/Users/mikedeiktakis/RiderProjects/DnDMapbuilder-Api/src/DnDMapBuilder.Api/appsettings.json`:

```json
{
  "LiveMap": {
    "ThrottleWindowMs": 100,
    "MaxConnectionsPerMap": 50,
    "EnableDetailedErrors": false
  },
  "CorsSettings": {
    "AllowedOrigins": ["http://localhost:3000", "https://yourdomain.com"]
  }
}
```

## Deployment Checklist

1. Apply EF Core migration: `AddPublicationStatusToGameMap`
2. Verify SignalR WebSocket support on hosting environment (Azure App Service, IIS, etc.)
3. Configure CORS to allow frontend origin for SignalR connections
4. Update firewall/load balancer to support WebSocket upgrade requests
5. Test SignalR sticky sessions if using multiple server instances
6. Configure logging/telemetry sink for production monitoring
7. Set up health check endpoint to validate SignalR Hub registration

## Future Enhancements (Out of Scope)

1. **Fog of War**: Add visibility masks to hide map areas from players
2. **Player Cursors**: Show multiple DM cursors if co-DMing
3. **Audio/Video Integration**: Sync music/sound effects with map events
4. **Mobile Live View**: Optimize live view UI for tablet displays
5. **Map History/Replay**: Record map state changes for session replay
6. **Performance Metrics Dashboard**: Real-time dashboard showing connection count, broadcast latency
7. **Redis Backplane**: Scale SignalR across multiple servers using Redis for message distribution
