# Quick Start Guide

## TL;DR - Fastest Way to Run

### Using .NET Aspire (Recommended)

```bash
# 1. Navigate to the project
cd DnDMapBuilder

# 2. Set SQL password
cd src/DnDMapBuilder.Aspire.AppHost
dotnet user-secrets set "Parameters:sql-password" "YourStrong@Passw0rd"

# 3. Run
dotnet run

# 4. Open the Aspire dashboard URL shown in console
# The API will be available at the URL shown for 'api' service
```

### Using Docker Compose

```bash
# From project root
docker-compose up --build
```

API available at: `http://localhost:5000`

### Using Direct API (Requires SQL Server)

```bash
# 1. Update connection string in src/DnDMapBuilder.Api/appsettings.json

# 2. Run migrations
cd src/DnDMapBuilder.Api
dotnet ef database update --project ../DnDMapBuilder.Data

# 3. Run API
dotnet run
```

## First Steps After Running

### 1. Access Swagger UI

Navigate to: `https://localhost:<port>/swagger`

### 2. Login as Admin

**Default Admin Credentials:**
- Email: `admin@dndmapbuilder.com`
- Password: `Admin123!`

### 3. Test the API

```bash
# Login
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@dndmapbuilder.com","password":"Admin123!"}'

# Copy the token from response

# Create a campaign
curl -X POST https://localhost:5001/api/campaigns \
  -H "Authorization: Bearer <your-token>" \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Campaign","description":"My first campaign"}'
```

## Common Issues

### Issue: Port already in use

**Solution:** Change the port in `launchSettings.json` or stop the service using that port.

### Issue: SQL Server connection failed

**Aspire:** Wait for SQL Server container to be healthy (check Aspire dashboard)

**Docker Compose:** Check `docker-compose logs sqlserver`

**Direct:** Verify SQL Server is running: `sqlcmd -S localhost -U sa -P <password> -Q "SELECT @@VERSION"`

### Issue: Database migrations not applied

```bash
cd src/DnDMapBuilder.Api
dotnet ef database update --project ../DnDMapBuilder.Data
```

## Project Structure Quick Reference

```
src/
├── DnDMapBuilder.Api/              ← API Controllers & Startup
│   ├── Controllers/                ← REST endpoints
│   ├── Program.cs                  ← Application entry point
│   └── appsettings.json           ← Configuration
├── DnDMapBuilder.Application/      ← Business logic
│   ├── Services/                   ← Service implementations
│   └── Interfaces/                 ← Service contracts
├── DnDMapBuilder.Contracts/        ← DTOs & Models
│   ├── DTOs/                       ← Data transfer objects
│   ├── Requests/                   ← Request models
│   └── Responses/                  ← Response models
├── DnDMapBuilder.Data/             ← Data access
│   ├── Entities/                   ← Database entities
│   ├── Repositories/               ← Data repositories
│   └── DnDMapBuilderDbContext.cs  ← EF Core context
└── DnDMapBuilder.Aspire.AppHost/   ← Orchestration
    └── Program.cs                  ← Aspire configuration
```

## Next Steps

1. **Register a new user** via `/api/auth/register`
2. **Approve the user** as admin via `/api/auth/approve-user`
3. **Create campaigns, missions, and maps** using the authenticated user
4. **Define custom tokens** for your maps
5. **Build maps** and place tokens

## Useful Commands

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run migrations
dotnet ef migrations add <name> --project src/DnDMapBuilder.Data

# Update database
dotnet ef database update --project src/DnDMapBuilder.Data

# Clean build artifacts
dotnet clean

# Watch for changes and auto-rebuild
dotnet watch run --project src/DnDMapBuilder.Api
```

## Environment Variables

When not using Aspire, you can override settings with environment variables:

```bash
export ConnectionStrings__DefaultConnection="Server=...;Database=...;"
export JwtSettings__SecretKey="your-secret-key"
export ASPNETCORE_ENVIRONMENT="Development"
```

## Testing with Postman

1. Import the API into Postman using the Swagger JSON: `https://localhost:5001/swagger/v1/swagger.json`
2. Set up an environment variable for the token
3. Use `{{token}}` in the Authorization header

## Production Checklist

Before deploying to production:

- [ ] Change JWT SecretKey in appsettings.json
- [ ] Update SQL Server password
- [ ] Configure proper CORS policy
- [ ] Enable HTTPS
- [ ] Set up proper logging
- [ ] Configure rate limiting
- [ ] Review and update default admin credentials
- [ ] Set up database backups
- [ ] Configure monitoring
- [ ] Review security headers

## Support

For issues or questions:
1. Check the full README.md
2. Review API_DOCUMENTATION.md
3. Check application logs
4. Review Aspire dashboard for container health
