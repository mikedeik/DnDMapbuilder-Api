# Visual Structure

```
DnDMapBuilder/
│
├── 📄 DnDMapBuilder.sln
├── 📄 .gitignore
├── 🐳 docker-compose.yml
├── 🔧 setup.sh
├── 🔧 verify-structure.sh
│
├── 📚 Documentation
│   ├── 📖 README.md
│   ├── 📖 QUICKSTART.md
│   ├── 📖 API_DOCUMENTATION.md
│   ├── 📖 PROJECT_SUMMARY.md
│   └── 📖 FILE_STRUCTURE.md
│
└── 📁 src/
    │
    ├── 📦 DnDMapBuilder.Contracts/
    │   ├── 📄 DnDMapBuilder.Contracts.csproj
    │   ├── 📁 DTOs/
    │   │   └── 📝 DomainDtos.cs
    │   ├── 📁 Requests/
    │   │   └── 📝 ApiRequests.cs
    │   └── 📁 Responses/
    │       └── 📝 ApiResponses.cs
    │
    ├── 📦 DnDMapBuilder.Data/
    │   ├── 📄 DnDMapBuilder.Data.csproj
    │   ├── 📝 DnDMapBuilderDbContext.cs
    │   ├── 📁 Entities/
    │   │   └── 📝 DomainEntities.cs
    │   └── 📁 Repositories/
    │       ├── 📝 IRepositories.cs
    │       └── 📝 Repositories.cs
    │
    ├── 📦 DnDMapBuilder.Application/
    │   ├── 📄 DnDMapBuilder.Application.csproj
    │   ├── 📁 Interfaces/
    │   │   └── 📝 IServices.cs
    │   ├── 📁 Mappings/
    │   │   └── 📝 MappingExtensions.cs
    │   └── 📁 Services/
    │       ├── 📝 AuthService.cs
    │       ├── 📝 JwtService.cs
    │       ├── 📝 CampaignService.cs
    │       ├── 📝 MissionService.cs
    │       └── 📝 GameMapAndTokenServices.cs
    │
    ├── 📦 DnDMapBuilder.Api/
    │   ├── 📄 DnDMapBuilder.Api.csproj
    │   ├── 📝 Program.cs
    │   ├── 🐳 Dockerfile
    │   ├── ⚙️ appsettings.json
    │   ├── ⚙️ appsettings.Development.json
    │   ├── 📁 Controllers/
    │   │   ├── 📝 AuthController.cs
    │   │   ├── 📝 CampaignsController.cs
    │   │   └── 📝 OtherControllers.cs
    │   └── 📁 Properties/
    │       └── ⚙️ launchSettings.json
    │
    ├── 📦 DnDMapBuilder.Aspire.AppHost/
    │   ├── 📄 DnDMapBuilder.Aspire.AppHost.csproj
    │   └── 📝 Program.cs
    │
    └── 📦 DnDMapBuilder.Aspire.ServiceDefaults/
        ├── 📄 DnDMapBuilder.Aspire.ServiceDefaults.csproj
        └── 📝 Extensions.cs
```

## Legend

- 📄 Project/Solution Files (.csproj, .sln)
- 📝 C# Source Files (.cs)
- 📖 Documentation (.md)
- ⚙️ Configuration (.json)
- 🐳 Docker Files
- 🔧 Scripts (.sh)
- 📦 Project Folders
- 📁 Code Organization Folders
- 📚 Documentation Section

## Statistics

- **Total Projects**: 6
- **Total C# Files**: 20
- **Total Lines of Code**: ~2,500
- **Total Documentation Files**: 5
- **Total Configuration Files**: 4
- **Total Scripts**: 2

## Quick Access

| What You Need | Where to Find It |
|---------------|------------------|
| Start the app | `setup.sh` or `docker-compose.yml` |
| API endpoints | `src/DnDMapBuilder.Api/Controllers/` |
| Database setup | `src/DnDMapBuilder.Data/DnDMapBuilderDbContext.cs` |
| Business logic | `src/DnDMapBuilder.Application/Services/` |
| API contracts | `src/DnDMapBuilder.Contracts/` |
| Configuration | `src/DnDMapBuilder.Api/appsettings.json` |
| Documentation | Root `*.md` files |
