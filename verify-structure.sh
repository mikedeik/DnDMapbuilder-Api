#!/bin/bash

# DnD Map Builder - Structure Verification Script
# This script verifies that all necessary files are present

echo "=========================================="
echo "DnD Map Builder - Structure Verification"
echo "=========================================="
echo ""

ERRORS=0

# Function to check if file exists
check_file() {
    if [ -f "$1" ]; then
        echo "✅ $1"
    else
        echo "❌ MISSING: $1"
        ERRORS=$((ERRORS + 1))
    fi
}

# Function to check if directory exists
check_dir() {
    if [ -d "$1" ]; then
        echo "✅ $1/"
    else
        echo "❌ MISSING: $1/"
        ERRORS=$((ERRORS + 1))
    fi
}

echo "Checking root files..."
check_file "DnDMapBuilder.sln"
check_file ".gitignore"
check_file "docker-compose.yml"
check_file "setup.sh"
check_file "README.md"
check_file "QUICKSTART.md"
check_file "API_DOCUMENTATION.md"
check_file "PROJECT_SUMMARY.md"
check_file "FILE_STRUCTURE.md"
echo ""

echo "Checking project directories..."
check_dir "src/DnDMapBuilder.Contracts"
check_dir "src/DnDMapBuilder.Data"
check_dir "src/DnDMapBuilder.Application"
check_dir "src/DnDMapBuilder.Api"
check_dir "src/DnDMapBuilder.Aspire.AppHost"
check_dir "src/DnDMapBuilder.Aspire.ServiceDefaults"
echo ""

echo "Checking Contracts project..."
check_file "src/DnDMapBuilder.Contracts/DnDMapBuilder.Contracts.csproj"
check_file "src/DnDMapBuilder.Contracts/DTOs/DomainDtos.cs"
check_file "src/DnDMapBuilder.Contracts/Requests/ApiRequests.cs"
check_file "src/DnDMapBuilder.Contracts/Responses/ApiResponses.cs"
echo ""

echo "Checking Data project..."
check_file "src/DnDMapBuilder.Data/DnDMapBuilder.Data.csproj"
check_file "src/DnDMapBuilder.Data/DnDMapBuilderDbContext.cs"
check_file "src/DnDMapBuilder.Data/Entities/DomainEntities.cs"
check_file "src/DnDMapBuilder.Data/Repositories/IRepositories.cs"
check_file "src/DnDMapBuilder.Data/Repositories/Repositories.cs"
echo ""

echo "Checking Application project..."
check_file "src/DnDMapBuilder.Application/DnDMapBuilder.Application.csproj"
check_file "src/DnDMapBuilder.Application/Interfaces/IServices.cs"
check_file "src/DnDMapBuilder.Application/Mappings/MappingExtensions.cs"
check_file "src/DnDMapBuilder.Application/Services/AuthService.cs"
check_file "src/DnDMapBuilder.Application/Services/JwtService.cs"
check_file "src/DnDMapBuilder.Application/Services/CampaignService.cs"
check_file "src/DnDMapBuilder.Application/Services/MissionService.cs"
check_file "src/DnDMapBuilder.Application/Services/GameMapAndTokenServices.cs"
echo ""

echo "Checking API project..."
check_file "src/DnDMapBuilder.Api/DnDMapBuilder.Api.csproj"
check_file "src/DnDMapBuilder.Api/Program.cs"
check_file "src/DnDMapBuilder.Api/Dockerfile"
check_file "src/DnDMapBuilder.Api/appsettings.json"
check_file "src/DnDMapBuilder.Api/appsettings.Development.json"
check_file "src/DnDMapBuilder.Api/Properties/launchSettings.json"
check_file "src/DnDMapBuilder.Api/Controllers/AuthController.cs"
check_file "src/DnDMapBuilder.Api/Controllers/CampaignsController.cs"
check_file "src/DnDMapBuilder.Api/Controllers/OtherControllers.cs"
echo ""

echo "Checking Aspire projects..."
check_file "src/DnDMapBuilder.Aspire.AppHost/DnDMapBuilder.Aspire.AppHost.csproj"
check_file "src/DnDMapBuilder.Aspire.AppHost/Program.cs"
check_file "src/DnDMapBuilder.Aspire.ServiceDefaults/DnDMapBuilder.Aspire.ServiceDefaults.csproj"
check_file "src/DnDMapBuilder.Aspire.ServiceDefaults/Extensions.cs"
echo ""

echo "=========================================="
if [ $ERRORS -eq 0 ]; then
    echo "✅ All files present! Structure is correct."
    echo ""
    echo "File counts:"
    echo "  - C# Project files: $(find . -name "*.csproj" | wc -l)"
    echo "  - C# Source files: $(find . -name "*.cs" | wc -l)"
    echo "  - Documentation files: $(find . -maxdepth 1 -name "*.md" | wc -l)"
    echo ""
    echo "You can now:"
    echo "  1. Run './setup.sh' for quick setup"
    echo "  2. Or run 'docker-compose up' for Docker deployment"
    echo "  3. Or read README.md for detailed instructions"
else
    echo "❌ Found $ERRORS missing file(s)!"
    echo "Please check the structure and ensure all files are present."
fi
echo "=========================================="
