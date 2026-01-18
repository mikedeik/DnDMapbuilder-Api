# Stage 1: Build stage using SDK image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files to optimize layer caching
COPY ["DnDMapBuilder.sln", "./"]
COPY ["src/DnDMapBuilder.Api/DnDMapBuilder.Api.csproj", "src/DnDMapBuilder.Api/"]
COPY ["src/DnDMapBuilder.Application/DnDMapBuilder.Application.csproj", "src/DnDMapBuilder.Application/"]
COPY ["src/DnDMapBuilder.Aspire.ServiceDefaults/DnDMapBuilder.Aspire.ServiceDefaults.csproj", "src/DnDMapBuilder.Aspire.ServiceDefaults/"]
COPY ["src/DnDMapBuilder.Contracts/DnDMapBuilder.Contracts.csproj", "src/DnDMapBuilder.Contracts/"]
COPY ["src/DnDMapBuilder.Data/DnDMapBuilder.Data.csproj", "src/DnDMapBuilder.Data/"]
COPY ["src/DnDMapBuilder.Infrastructure/DnDMapBuilder.Infrastructure.csproj", "src/DnDMapBuilder.Infrastructure/"]

# Restore NuGet dependencies (separate layer for better caching)
RUN dotnet restore "src/DnDMapBuilder.Api/DnDMapBuilder.Api.csproj"

# Copy remaining source code
COPY . .

# Build the application in Release mode
WORKDIR "/src/src/DnDMapBuilder.Api"
RUN dotnet build "DnDMapBuilder.Api.csproj" -c Release -o /app/build --no-restore

# Stage 2: Publish stage
FROM build AS publish
RUN dotnet publish "DnDMapBuilder.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishReadyToRun=true

# Stage 3: Runtime stage (using standard image for SQL Server compatibility)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# Install curl for health checks
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -g 1001 dotnetuser && \
    useradd -u 1001 -g dotnetuser dotnetuser

WORKDIR /app

# Copy published application from publish stage
COPY --from=publish --chown=dotnetuser:dotnetuser /app/publish .

# Create uploads directory with proper permissions
RUN mkdir -p /app/wwwroot/uploads && \
    chown -R dotnetuser:dotnetuser /app

# Switch to non-root user
USER dotnetuser

# Expose HTTP port only (nginx handles SSL termination)
EXPOSE 8080

# Health check configuration
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

# Set environment for .NET
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_HTTP_PORT=8080
# Enable globalization for SQL Server compatibility
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Entry point
ENTRYPOINT ["dotnet", "DnDMapBuilder.Api.dll"]
