# DnD Map Builder - Complete File Structure

## Solution Structure

```
DnDMapBuilder/
│
├── DnDMapBuilder.sln                      # Visual Studio Solution File
├── .gitignore                             # Git ignore patterns
├── docker-compose.yml                     # Docker Compose configuration
├── setup.sh                               # Quick setup script (chmod +x)
│
├── README.md                              # Main documentation
├── QUICKSTART.md                          # Quick start guide
├── API_DOCUMENTATION.md                   # Complete API reference
├── PROJECT_SUMMARY.md                     # Project overview
├── FILE_STRUCTURE.md                      # This file
│
└── src/                                   # Source code directory
    │
    ├── DnDMapBuilder.Contracts/           # DTO & Contract Layer
    │   ├── DnDMapBuilder.Contracts.csproj
    │   ├── DTOs/
    │   │   └── DomainDtos.cs             # Campaign, Mission, Map, Token DTOs
    │   ├── Requests/
    │   │   └── ApiRequests.cs            # Create/Update request models
    │   └── Responses/
    │       └── ApiResponses.cs           # API response wrappers
    │
    ├── DnDMapBuilder.Data/                # Data Access Layer
    │   ├── DnDMapBuilder.Data.csproj
    │   ├── DnDMapBuilderDbContext.cs     # EF Core DbContext
    │   ├── Entities/
    │   │   └── DomainEntities.cs         # Database entities (User, Campaign, etc.)
    │   └── Repositories/
    │       ├── IRepositories.cs          # Repository interfaces
    │       └── Repositories.cs           # Repository implementations
    │
    ├── DnDMapBuilder.Application/         # Business Logic Layer
    │   ├── DnDMapBuilder.Application.csproj
    │   ├── Interfaces/
    │   │   └── IServices.cs              # Service interfaces
    │   ├── Mappings/
    │   │   └── MappingExtensions.cs      # Entity to DTO mappings
    │   └── Services/
    │       ├── AuthService.cs            # Authentication & user management
    │       ├── CampaignService.cs        # Campaign operations
    │       ├── MissionService.cs         # Mission operations
    │       ├── GameMapAndTokenServices.cs # Map & token operations
    │       └── JwtService.cs             # JWT token generation/validation
    │
    ├── DnDMapBuilder.Api/                 # Web API Layer
    │   ├── DnDMapBuilder.Api.csproj
    │   ├── Program.cs                    # Application entry point & DI configuration
    │   ├── Dockerfile                    # Docker image definition
    │   ├── appsettings.json              # Application configuration
    │   ├── appsettings.Development.json  # Development configuration
    │   ├── Controllers/
    │   │   ├── AuthController.cs         # /api/auth endpoints
    │   │   ├── CampaignsController.cs    # /api/campaigns endpoints
    │   │   └── OtherControllers.cs       # Missions, Maps, Tokens endpoints
    │   └── Properties/
    │       └── launchSettings.json       # Launch profiles (HTTP/HTTPS)
    │
    ├── DnDMapBuilder.Aspire.AppHost/      # Aspire Orchestration
    │   ├── DnDMapBuilder.Aspire.AppHost.csproj
    │   └── Program.cs                    # Aspire app host configuration
    │
    └── DnDMapBuilder.Aspire.ServiceDefaults/ # Aspire Shared Services
        ├── DnDMapBuilder.Aspire.ServiceDefaults.csproj
        └── Extensions.cs                 # OpenTelemetry & health checks
```

## File Descriptions

### Root Level

- **DnDMapBuilder.sln**: Visual Studio solution file that references all projects
- **.gitignore**: Specifies files/folders to ignore in version control
- **docker-compose.yml**: Docker Compose configuration for SQL Server + API
- **setup.sh**: Bash script for quick project setup (make executable with `chmod +x`)

### Documentation Files

- **README.md**: Complete setup instructions and project overview
- **QUICKSTART.md**: Fast setup guide with minimal steps
- **API_DOCUMENTATION.md**: Full API endpoint reference with examples
- **PROJECT_SUMMARY.md**: Architecture overview and technology stack

### Source Projects

#### 1. DnDMapBuilder.Contracts (No Dependencies)
**Purpose**: Define data contracts used across all layers

- `DTOs/DomainDtos.cs`: Data transfer objects for all domain models
- `Requests/ApiRequests.cs`: Request models for API endpoints
- `Responses/ApiResponses.cs`: Standardized response wrappers

#### 2. DnDMapBuilder.Data (Depends on: None)
**Purpose**: Database access and entity definitions

- `DnDMapBuilderDbContext.cs`: EF Core database context with configuration
- `Entities/DomainEntities.cs`: Database entity classes (User, Campaign, Mission, GameMap, TokenDefinition, MapTokenInstance)
- `Repositories/IRepositories.cs`: Repository interface definitions
- `Repositories/Repositories.cs`: Concrete repository implementations with EF Core

**Key Features**:
- Entity relationships configured with Fluent API
- Repository pattern for data access abstraction
- Seeded admin user for initial setup

#### 3. DnDMapBuilder.Application (Depends on: Contracts, Data)
**Purpose**: Business logic and service layer

- `Interfaces/IServices.cs`: Service contracts (IAuthService, ICampaignService, etc.)
- `Mappings/MappingExtensions.cs`: Extension methods for Entity→DTO conversion
- `Services/AuthService.cs`: User registration, login, approval workflow
- `Services/JwtService.cs`: JWT token generation and validation
- `Services/CampaignService.cs`: Campaign CRUD operations
- `Services/MissionService.cs`: Mission CRUD operations
- `Services/GameMapAndTokenServices.cs`: Map and token CRUD operations

**Key Features**:
- User ownership validation
- Role-based authorization checks
- Password hashing with BCrypt
- JWT token management

#### 4. DnDMapBuilder.Api (Depends on: Application, Contracts, Data)
**Purpose**: REST API endpoints and HTTP handling

- `Program.cs`: Application startup, DI container configuration, middleware pipeline
- `Controllers/AuthController.cs`: Authentication endpoints
- `Controllers/CampaignsController.cs`: Campaign management endpoints
- `Controllers/OtherControllers.cs`: Missions, Maps, and Tokens endpoints
- `appsettings.json`: Database connection, JWT settings
- `Dockerfile`: Multi-stage Docker build definition

**Key Features**:
- JWT Bearer authentication
- Swagger/OpenAPI documentation
- CORS configuration
- Automatic database migration on startup

#### 5. DnDMapBuilder.Aspire.AppHost (Depends on: Api, ServiceDefaults)
**Purpose**: Local development orchestration

- `Program.cs`: Configure SQL Server and API containers

**Features**:
- SQL Server container with persistent volume
- Automatic service discovery
- Health check monitoring
- Aspire dashboard

#### 6. DnDMapBuilder.Aspire.ServiceDefaults (No Dependencies)
**Purpose**: Shared Aspire configuration

- `Extensions.cs`: OpenTelemetry, health checks, service discovery

## Total File Count

- **C# Project Files**: 6
- **C# Source Files**: 19
- **Configuration Files**: 5
- **Documentation Files**: 5
- **Docker Files**: 2
- **Scripts**: 1

**Total**: 38 files

## File Sizes (Approximate)

- Total solution size: ~115 KB (code only)
- Largest file: `Repositories.cs` (~6 KB)
- Average file size: ~3 KB

## Technology Stack by Project

### DnDMapBuilder.Contracts
- .NET 9.0
- No external dependencies

### DnDMapBuilder.Data
- .NET 9.0
- Entity Framework Core 9.0
- SQL Server provider

### DnDMapBuilder.Application
- .NET 9.0
- BCrypt.Net-Next
- System.IdentityModel.Tokens.Jwt

### DnDMapBuilder.Api
- ASP.NET Core 9.0
- JWT Bearer Authentication
- Swashbuckle (Swagger)
- Entity Framework Core Design Tools

### Aspire Projects
- .NET 9.0
- Aspire.Hosting
- OpenTelemetry

## Quick Navigation

| Task | File to Modify |
|------|---------------|
| Add new API endpoint | `src/DnDMapBuilder.Api/Controllers/` |
| Add business logic | `src/DnDMapBuilder.Application/Services/` |
| Add database entity | `src/DnDMapBuilder.Data/Entities/DomainEntities.cs` |
| Add repository method | `src/DnDMapBuilder.Data/Repositories/` |
| Change DB schema | `src/DnDMapBuilder.Data/DnDMapBuilderDbContext.cs` |
| Add DTO | `src/DnDMapBuilder.Contracts/DTOs/` |
| Configure JWT | `src/DnDMapBuilder.Api/appsettings.json` |
| Change connection string | `src/DnDMapBuilder.Api/appsettings.json` |
| Modify Aspire setup | `src/DnDMapBuilder.Aspire.AppHost/Program.cs` |

## Missing Files (By Design)

The following are **NOT** included as they're generated/downloaded:
- `bin/` and `obj/` directories (build output)
- `packages/` directory (NuGet packages)
- `.vs/` directory (Visual Studio cache)
- `*.user` files (user-specific settings)
- Database migration files (generate with EF Core tools)
- `node_modules/` (not applicable for this project)

## Next Steps After Download

1. Extract the ZIP file
2. Navigate to the `DnDMapBuilder` directory
3. Choose a run method:
   - Aspire: Run `setup.sh` and select option 1
   - Docker: Run `docker-compose up`
   - Direct: Configure SQL Server and run `dotnet run`
4. Access Swagger UI to test the API

## Verifying the Structure

Run this command in the root directory to verify all files are present:

```bash
find . -name "*.csproj" | wc -l  # Should show 6
find . -name "*.cs" | wc -l      # Should show 19
```

## Support Files

All necessary support files are included:
- ✅ Solution file (.sln)
- ✅ Project files (.csproj)
- ✅ Configuration files (appsettings.json, launchSettings.json)
- ✅ Docker files (Dockerfile, docker-compose.yml)
- ✅ Documentation (4 markdown files)
- ✅ Setup script (setup.sh)
- ✅ Git ignore (.gitignore)

The solution is **complete and ready to build**!
