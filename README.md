# DnD Map Builder - Backend API

A comprehensive ASP.NET Web API for managing D&D campaigns, missions, maps, and tokens with authentication and authorization.

## Architecture

The solution is organized into the following projects:

- **DnDMapBuilder.Contracts**: DTOs, Requests, and Response models
- **DnDMapBuilder.Data**: Entity Framework Core, Entities, DbContext, and Repositories
- **DnDMapBuilder.Application**: Business logic and services
- **DnDMapBuilder.Api**: ASP.NET Core Web API with controllers
- **DnDMapBuilder.Aspire.AppHost**: .NET Aspire orchestration for local development
- **DnDMapBuilder.Aspire.ServiceDefaults**: Shared service defaults for Aspire

## Prerequisites

- .NET 9.0 SDK
- Docker Desktop (for local development with Aspire)
- Visual Studio 2022 or Visual Studio Code
- SQL Server (or use Docker container via Aspire)

## Getting Started

### Option 1: Run with .NET Aspire (Recommended)

.NET Aspire orchestrates the SQL Server database and API application in Docker containers.

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd DnDMapBuilder
   ```

2. **Set SQL Server password**
   
   When running for the first time, Aspire will prompt for a SQL Server password. You can also set it via:
   ```bash
   dotnet user-secrets set Parameters:sql-password "YourStrong@Passw0rd" --project src/DnDMapBuilder.Aspire.AppHost
   ```

3. **Run the Aspire AppHost**
   ```bash
   cd src/DnDMapBuilder.Aspire.AppHost
   dotnet run
   ```

4. **Access the application**
   - Aspire Dashboard: https://localhost:17001 (or check console output)
   - API: https://localhost:7001 (check Aspire dashboard for actual port)
   - Swagger UI: https://localhost:7001/swagger

### Option 2: Run API Directly

1. **Set up SQL Server**
   
   Update the connection string in `src/DnDMapBuilder.Api/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=DnDMapBuilder;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
   }
   ```

2. **Run migrations**
   ```bash
   cd src/DnDMapBuilder.Api
   dotnet ef database update
   ```

3. **Run the API**
   ```bash
   dotnet run
   ```

4. **Access Swagger UI**
   - Navigate to: https://localhost:5001/swagger

## Database Migrations

To create a new migration:

```bash
cd src/DnDMapBuilder.Api
dotnet ef migrations add <MigrationName> --project ../DnDMapBuilder.Data
```

To update the database:

```bash
dotnet ef database update --project ../DnDMapBuilder.Data
```

## Default Admin Account

A default admin account is seeded automatically:

- **Email**: admin@dndmapbuilder.com
- **Password**: Admin123!
- **Role**: admin
- **Status**: approved

## API Endpoints

### Authentication

- `POST /api/auth/register` - Register a new user (requires admin approval)
- `POST /api/auth/login` - Login and receive JWT token
- `GET /api/auth/pending-users` - Get pending user registrations (Admin only)
- `POST /api/auth/approve-user` - Approve/reject user registration (Admin only)

### Campaigns

- `GET /api/campaigns` - Get all campaigns for the authenticated user
- `GET /api/campaigns/{id}` - Get a specific campaign
- `POST /api/campaigns` - Create a new campaign
- `PUT /api/campaigns/{id}` - Update a campaign
- `DELETE /api/campaigns/{id}` - Delete a campaign

### Missions

- `GET /api/missions/{id}` - Get a specific mission
- `GET /api/missions/campaign/{campaignId}` - Get all missions for a campaign
- `POST /api/missions` - Create a new mission
- `PUT /api/missions/{id}` - Update a mission
- `DELETE /api/missions/{id}` - Delete a mission

### Maps

- `GET /api/maps/{id}` - Get a specific map
- `GET /api/maps/mission/{missionId}` - Get all maps for a mission
- `POST /api/maps` - Create a new map
- `PUT /api/maps/{id}` - Update a map (including tokens)
- `DELETE /api/maps/{id}` - Delete a map

### Tokens

- `GET /api/tokens` - Get all tokens for the authenticated user
- `GET /api/tokens/{id}` - Get a specific token
- `POST /api/tokens` - Create a new token
- `PUT /api/tokens/{id}` - Update a token
- `DELETE /api/tokens/{id}` - Delete a token

## Authentication

The API uses JWT Bearer token authentication. To authenticate:

1. Register or login via `/api/auth/register` or `/api/auth/login`
2. Copy the token from the response
3. In Swagger UI, click "Authorize" and enter: `Bearer <your-token>`
4. Or include the header in your requests: `Authorization: Bearer <your-token>`

## Configuration

### JWT Settings

Update in `appsettings.json`:

```json
"JwtSettings": {
  "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
  "Issuer": "DnDMapBuilderApi",
  "Audience": "DnDMapBuilderClient",
  "ExpirationMinutes": "1440"
}
```

### CORS

CORS is configured to allow all origins in development. Update the CORS policy in `Program.cs` for production.

## Docker Support

The Aspire AppHost automatically manages Docker containers for:

- SQL Server 2022 (with persistent data volume)
- The API application

Container orchestration is handled automatically by .NET Aspire.

## Project Structure

```
DnDMapBuilder/
├── src/
│   ├── DnDMapBuilder.Api/           # Web API Controllers
│   ├── DnDMapBuilder.Application/   # Business Logic & Services
│   ├── DnDMapBuilder.Contracts/     # DTOs & Request/Response Models
│   ├── DnDMapBuilder.Data/          # EF Core, Entities, Repositories
│   ├── DnDMapBuilder.Aspire.AppHost/         # Aspire Orchestration
│   └── DnDMapBuilder.Aspire.ServiceDefaults/ # Shared Aspire Config
└── DnDMapBuilder.sln
```

## Technologies Used

- .NET 9.0
- ASP.NET Core Web API
- Entity Framework Core 9.0
- SQL Server 2022
- JWT Authentication
- BCrypt.Net for password hashing
- .NET Aspire for orchestration
- Swagger/OpenAPI
- Docker

## Development

### Running Tests

```bash
dotnet test
```

### Code Style

The project follows standard C# coding conventions with nullable reference types enabled.

## License

This project is licensed under the MIT License.
