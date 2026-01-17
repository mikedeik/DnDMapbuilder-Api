#!/bin/bash

# Database Migration Script
# This script runs database migrations as part of the deployment pipeline.
# Can be executed from CI/CD pipelines or manually for maintenance.

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
DOTNET_VERSION=${DOTNET_VERSION:-"10.0"}
PROJECT_PATH=${PROJECT_PATH:-"src/DnDMapBuilder.Api"}
CONNECTION_STRING=${ConnectionStrings__DefaultConnection}
MIGRATION_TIMEOUT=${MIGRATION_TIMEOUT:-120}

# Helper functions
print_info() {
    echo -e "${BLUE}ℹ ${1}${NC}"
}

print_success() {
    echo -e "${GREEN}✓ ${1}${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ ${1}${NC}"
}

print_error() {
    echo -e "${RED}✗ ${1}${NC}"
}

# Verify environment
print_info "Verifying environment..."

if [ -z "$CONNECTION_STRING" ]; then
    print_error "ConnectionStrings__DefaultConnection environment variable is not set"
    exit 1
fi

if [ ! -f "$PROJECT_PATH/$PROJECT_PATH.csproj" ] && [ ! -f "src/DnDMapBuilder.Api/DnDMapBuilder.Api.csproj" ]; then
    print_error "Project file not found"
    exit 1
fi

print_success "Environment verified"

# Check if .NET is available
print_info "Checking .NET version..."
if ! command -v dotnet &> /dev/null; then
    print_error ".NET CLI is not installed"
    exit 1
fi

INSTALLED_VERSION=$(dotnet --version)
print_success ".NET $INSTALLED_VERSION is installed"

# Run migrations
print_info "Running database migrations..."
print_info "Connection String: ${CONNECTION_STRING:0:50}..."

cd "$(dirname "$0")/.."

# Use timeout to prevent hanging if migration takes too long
timeout $MIGRATION_TIMEOUT dotnet ef database update \
    --project src/DnDMapBuilder.Data/DnDMapBuilder.Data.csproj \
    --startup-project src/DnDMapBuilder.Api/DnDMapBuilder.Api.csproj \
    --context DnDMapBuilderDbContext \
    --verbose \
    || {
        MIGRATION_RESULT=$?
        if [ $MIGRATION_RESULT -eq 124 ]; then
            print_error "Migration timeout after ${MIGRATION_TIMEOUT} seconds"
            exit 1
        else
            print_error "Migration failed with exit code $MIGRATION_RESULT"
            exit 1
        fi
    }

print_success "Database migrations completed successfully"

# Verify database connectivity
print_info "Verifying database connectivity..."

# Extract server and database from connection string
SERVER=$(echo "$CONNECTION_STRING" | grep -oP 'Server=\K[^;]+')
DATABASE=$(echo "$CONNECTION_STRING" | grep -oP 'Database=\K[^;]+')

print_info "Server: $SERVER"
print_info "Database: $DATABASE"

# If using SQL Server, verify with sqlcmd
if command -v sqlcmd &> /dev/null; then
    print_info "Verifying database with sqlcmd..."

    if sqlcmd -S "$SERVER" -d "$DATABASE" -Q "SELECT DB_NAME();" &>/dev/null; then
        print_success "Database connectivity verified"
    else
        print_warning "Could not verify database with sqlcmd (this might be normal if not using SQL Server)"
    fi
else
    print_warning "sqlcmd not available, skipping verification"
fi

print_success "Migration process completed successfully"
exit 0
