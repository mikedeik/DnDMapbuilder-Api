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

# Stage 3: Runtime stage using minimal runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final

# Install curl for health checks
RUN apk add --no-cache curl

# Create non-root user for security
RUN addgroup -g 1001 -S dotnetuser && \
    adduser -u 1001 -S dotnetuser -G dotnetuser

WORKDIR /app

# Copy published application from publish stage
COPY --from=publish --chown=dotnetuser:dotnetuser /app/publish .

# Create uploads directory with proper permissions
RUN mkdir -p /app/wwwroot/uploads && \
    chown -R dotnetuser:dotnetuser /app

# Switch to non-root user
USER dotnetuser

# Expose ports
EXPOSE 8080
EXPOSE 8443

# Health check configuration
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

# Set environment for .NET to use the non-root user
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_HTTP_PORT=8080
ENV ASPNETCORE_HTTPS_PORT=8443

# Entry point
ENTRYPOINT ["dotnet", "DnDMapBuilder.Api.dll"]
