# DnD Map Builder - Backend API

A comprehensive ASP.NET Web API for managing D&D campaigns, missions, maps, and tokens with authentication and authorization.

## Architecture

The solution follows a layered architecture with clean separation of concerns:

### Project Organization

- **DnDMapBuilder.Contracts**: DTOs, Requests, and Response models
- **DnDMapBuilder.Data**: Entity Framework Core, Entities, DbContext, and Repositories
- **DnDMapBuilder.Application**: Business logic, services, and domain validation
- **DnDMapBuilder.Infrastructure**: Cross-cutting concerns (logging, telemetry, middleware, security)
- **DnDMapBuilder.Api**: ASP.NET Core Web API with controllers and route handlers
- **DnDMapBuilder.UnitTests**: Unit tests with mocked dependencies
- **DnDMapBuilder.IntegrationTests**: Integration tests with real database
- **DnDMapBuilder.ArchitectureTests**: Architectural rule enforcement

### Layered Architecture

```
Client (Frontend)
    ↓
API Layer (Controllers, Routes)
    ↓
Application Layer (Services, Business Logic)
    ↓
Data Layer (Repositories, EF Core)
    ↓
Database (SQL Server)
```

**Separation of Concerns:**
- Controllers handle HTTP concerns only
- Services handle business logic
- Repositories handle data access
- Entities are database models (not exposed to clients)
- DTOs are used for API contracts

## Prerequisites

- .NET 10.0 SDK
- Docker Desktop (for containerized deployment and SQL Server)
- Visual Studio 2022 or Visual Studio Code
- SQL Server 2022 (local or cloud-based)
- Git for version control

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

CORS is configured via `CorsSettings` in `appsettings.json`:

```json
"CorsSettings": {
  "AllowedOrigins": [
    "http://localhost:3000",
    "https://yourdomain.com"
  ]
}
```

## Security Features

The API includes several security features by default:

### Security Headers

The following security headers are automatically included in all responses:
- `X-Content-Type-Options: nosniff` - Prevent MIME sniffing
- `X-Frame-Options: DENY` - Prevent clickjacking
- `X-XSS-Protection: 1; mode=block` - Enable XSS protection
- `Strict-Transport-Security: max-age=31536000` - Force HTTPS
- `Content-Security-Policy: default-src 'self'` - Restrict content to same-origin
- `Referrer-Policy: strict-origin-when-cross-origin` - Control referrer information
- `Permissions-Policy: geolocation=(), microphone=(), camera=()` - Disable dangerous APIs

### Rate Limiting

Rate limiting is enabled to prevent abuse:
- **Anonymous users**: 100 requests per minute (IP-based)
- **Authenticated users**: 300 requests per minute (User ID-based)
- **File uploads**: 10 requests per minute

Exceeded rate limits return HTTP 429 (Too Many Requests) with a Retry-After header.

### Request/Response Logging

All HTTP requests and responses are logged with:
- Request method, path, query string, and user identity
- Response status code and duration
- Correlation IDs for request tracing
- Sensitive headers (Authorization, Cookie) are sanitized in logs

### API Versioning

The API supports versioning via URL path. Current version is **v1.0**.

Routes follow the pattern: `/api/v{version:apiVersion}/[controller]`

Example: `/api/v1.0/auth/login`

## Testing

### Running Tests

Execute all tests:
```bash
dotnet test
```

Run only unit tests:
```bash
dotnet test --filter Category=Unit
```

Run only integration tests (requires database):
```bash
dotnet test --filter Category=Integration
```

Run only architecture tests:
```bash
dotnet test --filter Category=Architecture
```

### Test Coverage

The project includes:
- **75+ Unit Tests**: Services, repositories, entities, and utilities
- **8 Architecture Tests**: Enforce layered architecture and design principles
- **7 Integration Tests**: Database operations and API endpoints (with database)

Target coverage: >80% for core business logic

### Running Tests in CI/CD

Tests are automatically run in the GitHub Actions pipeline on every push to `main` or `develop` branches.

## Monitoring and Logging

### Application Logging

Structured logging is configured with Serilog:
- **Console**: Real-time log output in development
- **File**: Rolling file logs (daily rotation, 100MB max size)
- **Structured JSON**: Machine-readable log format

### Optional: External Monitoring

You can configure optional monitoring integrations by setting secrets in GitHub:

#### Azure Application Insights
```bash
APPLICATIONINSIGHTS_CONNECTION_STRING=<your-connection-string>
```

#### OpenTelemetry (OTEL)
```bash
OTEL_EXPORTER_OTLP_ENDPOINT=http://your-otel-collector:4317
```

#### Log Level Configuration
```bash
LOG_LEVEL=Information  # Options: Debug, Information, Warning, Error, Critical
```

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

**Core Framework:**
- .NET 10.0
- ASP.NET Core Web API
- Entity Framework Core 10.0
- SQL Server 2022 / Azure SQL Edge

**Authentication & Security:**
- JWT Bearer Token Authentication
- BCrypt.Net for secure password hashing
- Security headers middleware
- CORS policy enforcement
- Rate limiting

**API Documentation & Testing:**
- Swagger/OpenAPI with XML documentation
- xUnit testing framework
- FluentAssertions for readable test assertions
- Moq for mocking dependencies
- AutoFixture for test data generation

**Logging & Observability:**
- Serilog for structured logging
- OpenTelemetry for distributed tracing
- Health checks endpoints
- Request/response logging middleware

**Infrastructure & Deployment:**
- Docker containerization
- GitHub Actions for CI/CD
- Alpine Linux for minimal image size
- Non-root container user for security

## Development

### Running Tests

```bash
dotnet test
```

### Code Style

The project follows standard C# coding conventions with nullable reference types enabled.

## Continuous Integration & Deployment

### GitHub Actions Pipeline

The project includes automated CI/CD pipeline (`.github/workflows/main.yml`) that:

1. **Build & Test** (on every push)
   - Restores dependencies
   - Builds in Release mode
   - Runs all unit, integration, and architecture tests

2. **Docker Image Build** (on main branch only)
   - Builds multi-platform Docker image (linux/amd64, linux/arm64)
   - Pushes to GitHub Container Registry (ghcr.io)
   - Uses layer caching for faster builds

3. **Deployment** (on main branch only)
   - Creates Docker network if needed
   - Runs database migrations
   - Deploys API container with health checks
   - Verifies deployment with health endpoints
   - Cleans up old images

### Required GitHub Secrets

To enable CI/CD deployment, configure these secrets in GitHub repository settings:

| Secret | Description | Example |
|--------|-------------|---------|
| `DB_CONNECTION_STRING` | SQL Server connection string | `Server=your-server;Database=dndmapbuilder;User Id=sa;Password=***;` |
| `SERVER_HOST` | Deployment server hostname | `api.example.com` |
| `SERVER_USERNAME` | SSH username for deployment | `deploy` |
| `SSH_PRIVATE_KEY` | SSH private key for authentication | (SSH RSA private key) |
| `SERVER_PORT` | SSH port (optional) | `22` |

### Optional Monitoring Secrets

Configure these secrets if using external monitoring services:

| Secret | Description |
|--------|-------------|
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Azure Application Insights connection string |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | OpenTelemetry collector endpoint |

### Deployment Flow

```
Push to main
    ↓
Run tests in GitHub
    ↓
Build Docker image
    ↓
Push to container registry
    ↓
SSH to deployment server
    ↓
Pull latest image
    ↓
Run database migrations
    ↓
Start new API container
    ↓
Verify health checks
```

### Local Docker Deployment

To test deployment locally:

```bash
# Build Docker image
docker build -t dnd-api:latest -f src/DnDMapBuilder.Api/Dockerfile .

# Create network
docker network create dnd-network

# Run migrations
docker run --rm \
  -e ConnectionStrings__DefaultConnection="<connection-string>" \
  dnd-api:latest \
  sh -c "dotnet ef database update --project src/DnDMapBuilder.Data --startup-project src/DnDMapBuilder.Api"

# Start API container
docker run -d \
  --name dnd-api \
  -p 5000:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="<connection-string>" \
  --network dnd-network \
  dnd-api:latest

# Check health
curl http://localhost:5000/health/live
```

## License

This project is licensed under the MIT License.
