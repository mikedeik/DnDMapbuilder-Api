#!/bin/bash

# DnD Map Builder - Quick Setup Script

echo "=========================================="
echo "DnD Map Builder - Backend Setup"
echo "=========================================="
echo ""

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK is not installed. Please install .NET 9.0 SDK first."
    echo "   Download from: https://dotnet.microsoft.com/download"
    exit 1
fi

echo "✅ .NET SDK found: $(dotnet --version)"
echo ""

# Check if Docker is running (optional, for Aspire)
if command -v docker &> /dev/null; then
    if docker info &> /dev/null; then
        echo "✅ Docker is running"
        DOCKER_AVAILABLE=true
    else
        echo "⚠️  Docker is installed but not running"
        DOCKER_AVAILABLE=false
    fi
else
    echo "⚠️  Docker is not installed (optional for Aspire)"
    DOCKER_AVAILABLE=false
fi
echo ""

# Ask user how they want to run the application
echo "How would you like to run the application?"
echo "1. Using .NET Aspire (Recommended - requires Docker)"
echo "2. Using Docker Compose"
echo "3. Run API directly (requires SQL Server)"
echo ""
read -p "Enter your choice (1-3): " choice

case $choice in
    1)
        if [ "$DOCKER_AVAILABLE" = false ]; then
            echo "❌ Docker is required for Aspire. Please start Docker or choose another option."
            exit 1
        fi
        
        echo ""
        echo "Setting up .NET Aspire..."
        
        # Set SQL password
        read -sp "Enter SQL Server password (default: YourStrong@Passw0rd): " sql_password
        echo ""
        if [ -z "$sql_password" ]; then
            sql_password="YourStrong@Passw0rd"
        fi
        
        cd src/DnDMapBuilder.Aspire.AppHost
        dotnet user-secrets set "Parameters:sql-password" "$sql_password"
        
        echo ""
        echo "✅ Setup complete!"
        echo ""
        echo "Starting .NET Aspire..."
        dotnet run
        ;;
        
    2)
        if [ "$DOCKER_AVAILABLE" = false ]; then
            echo "❌ Docker is required for Docker Compose. Please start Docker or choose another option."
            exit 1
        fi
        
        echo ""
        echo "Starting with Docker Compose..."
        docker-compose up --build
        ;;
        
    3)
        echo ""
        echo "⚠️  Make sure SQL Server is running and accessible."
        echo ""
        read -p "Enter SQL Server host (default: localhost): " sql_host
        sql_host=${sql_host:-localhost}
        
        read -p "Enter SQL Server port (default: 1433): " sql_port
        sql_port=${sql_port:-1433}
        
        read -p "Enter SQL Server username (default: sa): " sql_user
        sql_user=${sql_user:-sa}
        
        read -sp "Enter SQL Server password: " sql_password
        echo ""
        
        # Update appsettings.json
        connection_string="Server=${sql_host},${sql_port};Database=DnDMapBuilder;User Id=${sql_user};Password=${sql_password};TrustServerCertificate=True;"
        
        cd src/DnDMapBuilder.Api
        
        # Create a temporary appsettings.Development.json with the connection string
        cat > appsettings.Development.json << EOF
{
  "ConnectionStrings": {
    "DefaultConnection": "${connection_string}"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  }
}
EOF
        
        echo ""
        echo "Running database migrations..."
        dotnet ef database update --project ../DnDMapBuilder.Data
        
        echo ""
        echo "✅ Setup complete!"
        echo ""
        echo "Starting API..."
        dotnet run
        ;;
        
    *)
        echo "❌ Invalid choice. Please run the script again."
        exit 1
        ;;
esac
