using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Application.Services;
using DnDMapBuilder.Data;
using DnDMapBuilder.Data.Repositories;
using DnDMapBuilder.Data.Repositories.Interfaces;
using DnDMapBuilder.Infrastructure.Configuration;
using DnDMapBuilder.Infrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container
var controllerBuilder = builder.Services.AddControllers();
controllerBuilder.ConfigureCacheProfiles();
builder.Services.AddEndpointsApiExplorer();

// Response Caching
builder.Services.AddResponseCachingConfiguration();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "DnD Map Builder API", Version = "v1" });
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DnDMapBuilderDbContext>(options =>
    options.UseSqlServer(connectionString));

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddSingleton<IJwtService>(sp => 
    new JwtService(
        secretKey,
        jwtSettings["Issuer"] ?? "DnDMapBuilderApi",
        jwtSettings["Audience"] ?? "DnDMapBuilderClient",
        int.Parse(jwtSettings["ExpirationMinutes"] ?? "1440")
    ));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configuration - Use Options pattern
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection(CorsSettings.SectionName));

// Rate Limiting
builder.Services.AddRateLimitingConfiguration();

// CORS - Use configured origins
var corsSettings = builder.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsSettings.SectionName, policy =>
    {
        if (corsSettings?.AllowedOrigins?.Length > 0)
        {
            policy.WithOrigins(corsSettings.AllowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            // Fallback to AllowAll if no origins configured
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<IMissionRepository, MissionRepository>();
builder.Services.AddScoped<IGameMapRepository, GameMapRepository>();
builder.Services.AddScoped<ITokenDefinitionRepository, TokenDefinitionRepository>();
builder.Services.AddScoped<IMapTokenInstanceRepository, MapTokenInstanceRepository>();

// Register Services
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<IMissionService, MissionService>();
builder.Services.AddScoped<IGameMapService, GameMapService>();
builder.Services.AddScoped<ITokenDefinitionService, TokenDefinitionService>();

// File Storage Service
var baseStoragePath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads");
var basePublicUrl = "/uploads";
builder.Services.AddSingleton<IFileStorageService>(sp =>
    new LocalFileStorageService(baseStoragePath, basePublicUrl, sp.GetRequiredService<ILogger<LocalFileStorageService>>()));

var app = builder.Build();

app.MapDefaultEndpoints();

// Apply migrations and seed admin user on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DnDMapBuilderDbContext>();
    await DbInitializer.InitializeAsync(db);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "DnD Map Builder API v1");
    });
}

app.UseHttpsRedirection();

// Add response caching middleware (should be early in pipeline)
app.UseResponseCachingConfiguration();

// Add cache control headers
app.UseCacheControlHeaders();

// Add security headers middleware
app.UseSecurityHeaders();

// Add request/response logging middleware
app.UseRequestResponseLogging();

// Add rate limiting middleware
app.UseRateLimitingConfiguration();

// Ensure wwwroot directory exists for static file serving
var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
}

// Serve static files from wwwroot at /api path
// This matches the frontend's URL construction: BASE_URL + imageUrl = /api + /uploads/...
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(wwwrootPath),
    RequestPath = "/api"
});

app.UseCors(CorsSettings.SectionName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
