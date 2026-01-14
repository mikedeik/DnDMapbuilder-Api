# DnD Map Builder - Complete Backend Solution

## 🎯 Project Overview

A full-featured ASP.NET Web API backend for the DnD Map Builder application, implementing user authentication, campaign management, mission planning, map creation, and token management.

## 📁 Solution Structure

```
DnDMapBuilder/
├── src/
│   ├── DnDMapBuilder.Contracts/          # DTOs, Requests, Responses
│   ├── DnDMapBuilder.Data/               # EF Core, Entities, Repositories
│   ├── DnDMapBuilder.Application/        # Business Logic & Services
│   ├── DnDMapBuilder.Api/                # Web API Controllers
│   ├── DnDMapBuilder.Aspire.AppHost/     # Aspire Orchestration
│   └── DnDMapBuilder.Aspire.ServiceDefaults/  # Aspire Service Defaults
├── DnDMapBuilder.sln                     # Solution file
├── docker-compose.yml                    # Docker Compose configuration
├── setup.sh                              # Quick setup script
├── README.md                             # Main documentation
├── API_DOCUMENTATION.md                  # Complete API reference
├── QUICKSTART.md                         # Quick start guide
└── .gitignore
```

## 🏗️ Architecture

### Clean Architecture Pattern

1. **Contracts Layer** (`DnDMapBuilder.Contracts`)
   - DTOs (Data Transfer Objects)
   - Request/Response models
   - API contracts
   - No dependencies on other layers

2. **Data Layer** (`DnDMapBuilder.Data`)
   - Entity Framework Core
   - Database entities
   - DbContext configuration
   - Repository pattern implementation
   - Database migrations

3. **Application Layer** (`DnDMapBuilder.Application`)
   - Business logic
   - Service implementations
   - Mapping extensions
   - JWT service
   - Depends on: Contracts, Data

4. **API Layer** (`DnDMapBuilder.Api`)
   - Controllers
   - Authentication/Authorization
   - Middleware
   - Swagger configuration
   - Depends on: Application, Contracts, Data

5. **Aspire Orchestration** (`DnDMapBuilder.Aspire.*`)
   - Container orchestration
   - Service discovery
   - Health checks
   - Telemetry

## 🔑 Key Features

### Authentication & Authorization
- JWT-based authentication
- Role-based authorization (Admin, User)
- User approval workflow
- Secure password hashing with BCrypt

### Campaign Management
- Create, read, update, delete campaigns
- User-owned campaigns
- Complete campaign hierarchy (Campaign → Mission → Map)

### Mission Management
- Organize missions within campaigns
- Mission descriptions and metadata
- Cascade delete with campaigns

### Map Builder
- Grid-based map system
- Custom map images
- Configurable grid (rows, columns, color, opacity)
- Token placement on maps

### Token Library
- User-specific token definitions
- Token types (player, enemy)
- Token sizes (1x1, 2x2, 3x3 grid squares)
- Image URL support

## 🗄️ Database Schema

### Users
- Id, Username, Email, PasswordHash
- Role (admin/user)
- Status (pending/approved/rejected)
- Timestamps

### Campaigns
- Id, Name, Description
- OwnerId → Users
- Timestamps

### Missions
- Id, Name, Description
- CampaignId → Campaigns

### GameMaps
- Id, Name, ImageUrl
- Rows, Cols, GridColor, GridOpacity
- MissionId → Missions

### TokenDefinitions
- Id, Name, ImageUrl, Size, Type
- UserId → Users

### MapTokenInstances
- Id, TokenId → TokenDefinitions
- MapId → GameMaps
- X, Y coordinates

## 🚀 Deployment Options

### Option 1: .NET Aspire (Recommended)
```bash
cd src/DnDMapBuilder.Aspire.AppHost
dotnet user-secrets set "Parameters:sql-password" "YourPassword"
dotnet run
```

**Advantages:**
- Automatic container orchestration
- Built-in service discovery
- Health monitoring dashboard
- Telemetry and logging
- Development-optimized

### Option 2: Docker Compose
```bash
docker-compose up --build
```

**Advantages:**
- Simple deployment
- Consistent environment
- Easy to share
- Production-ready

### Option 3: Direct Deployment
- Requires SQL Server instance
- Manual configuration
- More control over environment

## 🔐 Security Features

1. **JWT Authentication**
   - Secure token generation
   - Token expiration
   - Role-based claims

2. **Password Security**
   - BCrypt hashing
   - Salt per password
   - Secure password storage

3. **Authorization**
   - Role-based access control
   - User ownership validation
   - Admin-only endpoints

4. **API Security**
   - HTTPS support
   - CORS configuration
   - Input validation

## 📊 API Endpoints Summary

### Authentication (`/api/auth`)
- POST `/register` - Register new user
- POST `/login` - User login
- GET `/pending-users` - Get pending approvals (Admin)
- POST `/approve-user` - Approve/reject user (Admin)

### Campaigns (`/api/campaigns`)
- GET `/` - List user campaigns
- GET `/{id}` - Get campaign details
- POST `/` - Create campaign
- PUT `/{id}` - Update campaign
- DELETE `/{id}` - Delete campaign

### Missions (`/api/missions`)
- GET `/{id}` - Get mission
- GET `/campaign/{campaignId}` - List campaign missions
- POST `/` - Create mission
- PUT `/{id}` - Update mission
- DELETE `/{id}` - Delete mission

### Maps (`/api/maps`)
- GET `/{id}` - Get map with tokens
- GET `/mission/{missionId}` - List mission maps
- POST `/` - Create map
- PUT `/{id}` - Update map and tokens
- DELETE `/{id}` - Delete map

### Tokens (`/api/tokens`)
- GET `/` - List user tokens
- GET `/{id}` - Get token
- POST `/` - Create token
- PUT `/{id}` - Update token
- DELETE `/{id}` - Delete token

## 🛠️ Technology Stack

- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core 9.0** - ORM
- **SQL Server 2022** - Database
- **JWT Bearer** - Authentication
- **BCrypt.Net** - Password hashing
- **Swagger/OpenAPI** - API documentation
- **.NET Aspire** - Cloud-native orchestration
- **Docker** - Containerization

## 📦 NuGet Packages

### API Project
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.AspNetCore.OpenApi
- Swashbuckle.AspNetCore

### Application Project
- BCrypt.Net-Next
- System.IdentityModel.Tokens.Jwt

### Data Project
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools

### Aspire Projects
- Aspire.Hosting.AppHost
- Aspire.Hosting.SqlServer
- OpenTelemetry packages

## 🎨 Design Patterns Used

1. **Repository Pattern** - Data access abstraction
2. **Service Layer Pattern** - Business logic separation
3. **Dependency Injection** - Loose coupling
4. **DTO Pattern** - Data transfer optimization
5. **Clean Architecture** - Layer separation
6. **Factory Pattern** - Object creation
7. **Strategy Pattern** - Service implementations

## 🔄 Data Flow

```
Client Request
    ↓
API Controller (Authentication/Authorization)
    ↓
Application Service (Business Logic)
    ↓
Repository (Data Access)
    ↓
Entity Framework Core
    ↓
SQL Server Database
    ↓
Response (via DTOs)
```

## 📝 Default Credentials

**Admin Account** (Pre-seeded):
- Email: `admin@dndmapbuilder.com`
- Password: `Admin123!`
- Role: `admin`
- Status: `approved`

## 🧪 Testing the API

### Using Swagger UI
1. Navigate to `https://localhost:5001/swagger`
2. Login to get a token
3. Click "Authorize" and enter: `Bearer <token>`
4. Test endpoints interactively

### Using cURL
```bash
# Login
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@dndmapbuilder.com","password":"Admin123!"}'

# Create Campaign (replace <token>)
curl -X POST https://localhost:5001/api/campaigns \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"name":"Test","description":"Test campaign"}'
```

## 📈 Future Enhancements

Potential features to add:
- [ ] Real-time collaboration (SignalR)
- [ ] File upload for map images
- [ ] Export/import campaigns
- [ ] Campaign sharing between users
- [ ] Map templates
- [ ] Advanced token properties (HP, AC, etc.)
- [ ] Combat tracker
- [ ] Dice roller integration
- [ ] Character sheets
- [ ] Unit tests
- [ ] Integration tests
- [ ] Rate limiting
- [ ] API versioning
- [ ] GraphQL endpoint

## 📚 Documentation Files

1. **README.md** - Complete setup and overview
2. **API_DOCUMENTATION.md** - Full API reference
3. **QUICKSTART.md** - Fast setup guide
4. **setup.sh** - Automated setup script

## 🤝 Integration with Frontend

The React frontend can integrate with this API by:

1. **Authentication**
   ```typescript
   // Replace localStorage auth with API calls
   const login = async (email, password) => {
     const response = await fetch('https://api-url/api/auth/login', {
       method: 'POST',
       headers: { 'Content-Type': 'application/json' },
       body: JSON.stringify({ email, password })
     });
     const { data } = await response.json();
     localStorage.setItem('token', data.token);
   };
   ```

2. **Data Fetching**
   ```typescript
   // Fetch campaigns
   const getCampaigns = async () => {
     const token = localStorage.getItem('token');
     const response = await fetch('https://api-url/api/campaigns', {
       headers: { 'Authorization': `Bearer ${token}` }
     });
     return await response.json();
   };
   ```

3. **Replace local storage state** with API-backed state management

## ✅ Production Checklist

Before deploying to production:

- [ ] Change JWT secret key
- [ ] Update SQL Server credentials
- [ ] Configure HTTPS certificates
- [ ] Set up proper CORS policy
- [ ] Enable rate limiting
- [ ] Configure logging (Application Insights, Serilog)
- [ ] Set up CI/CD pipeline
- [ ] Database backup strategy
- [ ] Monitoring and alerting
- [ ] Load balancing (if needed)
- [ ] CDN for static assets
- [ ] Security headers
- [ ] Input validation
- [ ] Error handling
- [ ] API documentation versioning

## 🎓 Learning Resources

To understand this codebase:
1. Review the clean architecture layers
2. Follow a request from Controller → Service → Repository → Database
3. Examine the entity relationships in DbContext
4. Study the JWT authentication flow
5. Explore the Aspire orchestration setup

## 📄 License

MIT License - Feel free to use and modify

---

**Created**: January 2026  
**.NET Version**: 9.0  
**Database**: SQL Server 2022  
**Architecture**: Clean Architecture with Repository Pattern
