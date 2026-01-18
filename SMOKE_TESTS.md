# Smoke Testing Guide

## Overview

Smoke tests are quick, automated tests that verify critical application functionality immediately after deployment. They're the first validation that the application is operational.

## Smoke Test Objectives

✓ Verify application startup and health endpoints
✓ Validate core authentication flow
✓ Ensure database connectivity
✓ Test CRUD operations for each entity
✓ Verify file upload functionality
✓ Confirm telemetry collection
✓ Validate security headers

## Test Environment

### Required Setup

```bash
# Environment variables
export API_URL="http://localhost:5000"
export API_USERNAME="smoketest@example.com"
export API_PASSWORD="SmokeTestPassword123!"
export TEST_ADMIN_USERNAME="admin@example.com"
export TEST_ADMIN_PASSWORD="AdminPassword123!"
```

## Smoke Test Scenarios

### 1. Application Health Check

**Test:** Verify application is running and healthy

```bash
#!/bin/bash

echo "Testing health endpoints..."

# Health endpoint (no auth required)
echo "1. GET /health"
curl -s -w "\nStatus: %{http_code}\n" http://localhost:5000/health | jq .

# Ready endpoint
echo "2. GET /health/ready"
curl -s -w "\nStatus: %{http_code}\n" http://localhost:5000/health/ready | jq .

# Live endpoint
echo "3. GET /health/live"
curl -s -w "\nStatus: %{http_code}\n" http://localhost:5000/health/live | jq .

echo "✓ Health checks complete"
```

### 2. Authentication Flow

**Test:** Verify user registration and login

```bash
#!/bin/bash

echo "Testing authentication flow..."

# Register new user
echo "1. POST /auth/register"
REGISTER_RESPONSE=$(curl -s -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "smoketest_'$(date +%s)'",
    "email": "smoketest_'$(date +%s)'@example.com",
    "password": "SmokeTest123!"
  }')

echo $REGISTER_RESPONSE | jq .

USER_ID=$(echo $REGISTER_RESPONSE | jq -r '.data.id')
echo "Created user: $USER_ID"

# Login as admin and approve user
echo "2. POST /auth/login (Admin)"
LOGIN_RESPONSE=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "'$TEST_ADMIN_USERNAME'",
    "password": "'$TEST_ADMIN_PASSWORD'"
  }')

ADMIN_TOKEN=$(echo $LOGIN_RESPONSE | jq -r '.data.token')
echo "Admin token: ${ADMIN_TOKEN:0:20}..."

# Approve user
echo "3. POST /auth/approve-user"
curl -s -X POST http://localhost:5000/api/v1/auth/approve-user \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"userId": "'$USER_ID'", "approved": true}' | jq .

# Login as new user
echo "4. POST /auth/login (New User)"
LOGIN_RESPONSE=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "smoketest_'$(date +%s)'@example.com",
    "password": "SmokeTest123!"
  }')

TOKEN=$(echo $LOGIN_RESPONSE | jq -r '.data.token')
echo "User token: ${TOKEN:0:20}..."

echo "✓ Authentication flow complete"
```

### 3. Campaign CRUD Operations

**Test:** Create, read, update, and delete campaigns

```bash
#!/bin/bash

echo "Testing Campaign CRUD..."

# CREATE
echo "1. POST /campaigns (Create)"
CREATE_RESPONSE=$(curl -s -X POST http://localhost:5000/api/v1/campaigns \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Smoke Test Campaign",
    "description": "Campaign for smoke testing"
  }')

CAMPAIGN_ID=$(echo $CREATE_RESPONSE | jq -r '.data.id')
echo "Created campaign: $CAMPAIGN_ID"

# READ
echo "2. GET /campaigns/{id} (Read)"
curl -s -H "Authorization: Bearer $TOKEN" \
  http://localhost:5000/api/v1/campaigns/$CAMPAIGN_ID | jq .

# UPDATE
echo "3. PUT /campaigns/{id} (Update)"
curl -s -X PUT http://localhost:5000/api/v1/campaigns/$CAMPAIGN_ID \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Updated Campaign Name",
    "description": "Updated description"
  }' | jq .

# LIST
echo "4. GET /campaigns (List)"
curl -s -H "Authorization: Bearer $TOKEN" \
  http://localhost:5000/api/v1/campaigns | jq .

# DELETE
echo "5. DELETE /campaigns/{id} (Delete)"
curl -s -X DELETE http://localhost:5000/api/v1/campaigns/$CAMPAIGN_ID \
  -H "Authorization: Bearer $TOKEN" | jq .

echo "✓ Campaign CRUD complete"
```

### 4. Mission Operations

**Test:** Create mission and verify relationships

```bash
#!/bin/bash

echo "Testing Mission operations..."

# Create campaign first
CAMPAIGN=$(curl -s -X POST http://localhost:5000/api/v1/campaigns \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name": "Mission Test", "description": "Test"}')

CAMPAIGN_ID=$(echo $CAMPAIGN | jq -r '.data.id')

# Create mission
echo "1. POST /missions (Create)"
MISSION=$(curl -s -X POST http://localhost:5000/api/v1/missions \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "campaignId": "'$CAMPAIGN_ID'",
    "name": "Test Mission",
    "description": "Mission for smoke testing"
  }')

MISSION_ID=$(echo $MISSION | jq -r '.data.id')
echo "Created mission: $MISSION_ID"

# Get mission
echo "2. GET /missions/{id}"
curl -s -H "Authorization: Bearer $TOKEN" \
  http://localhost:5000/api/v1/missions/$MISSION_ID | jq .

echo "✓ Mission operations complete"
```

### 5. Authorization Verification

**Test:** Verify authorization enforcement

```bash
#!/bin/bash

echo "Testing authorization..."

# Create different user
ANOTHER_USER=$(curl -s -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "another_'$(date +%s)'",
    "email": "another_'$(date +%s)'@example.com",
    "password": "AnotherUser123!"
  }')

ANOTHER_USER_ID=$(echo $ANOTHER_USER | jq -r '.data.id')

# Approve other user
curl -s -X POST http://localhost:5000/api/v1/auth/approve-user \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"userId": "'$ANOTHER_USER_ID'", "approved": true}' > /dev/null

# Login as other user
ANOTHER_TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "another_'$(date +%s)'@example.com",
    "password": "AnotherUser123!"
  }' | jq -r '.data.token')

# Try to access another user's campaign (should fail)
echo "1. Attempting unauthorized access (should get 404/403)"
curl -s -w "\nStatus: %{http_code}\n" -H "Authorization: Bearer $ANOTHER_TOKEN" \
  http://localhost:5000/api/v1/campaigns/$CAMPAIGN_ID | jq .

echo "✓ Authorization verification complete"
```

### 6. Rate Limiting Test

**Test:** Verify rate limiting is active

```bash
#!/bin/bash

echo "Testing rate limiting..."

# Make rapid requests
echo "1. Making 15 rapid requests (limit is 10/min for anonymous)"
for i in {1..15}; do
  STATUS=$(curl -s -w "%{http_code}" -o /dev/null http://localhost:5000/health)
  echo "Request $i: HTTP $STATUS"
  if [ "$STATUS" = "429" ]; then
    echo "✓ Rate limit hit at request $i (expected)"
    break
  fi
done

echo "✓ Rate limiting test complete"
```

### 7. Security Headers Verification

**Test:** Verify security headers are present

```bash
#!/bin/bash

echo "Testing security headers..."

# Get response headers
curl -s -i http://localhost:5000/health | grep -i "X-" | head -10

echo "Expected headers:"
echo "- X-Content-Type-Options: nosniff"
echo "- X-Frame-Options: DENY"
echo "- X-XSS-Protection: 1; mode=block"
echo "- Strict-Transport-Security"
echo "- Content-Security-Policy"

echo "✓ Security headers verification complete"
```

### 8. Telemetry Collection

**Test:** Verify telemetry is being collected

```bash
#!/bin/bash

echo "Testing telemetry collection..."

# Check OpenTelemetry metrics endpoint (if configured)
echo "1. Checking metrics endpoint"
curl -s http://localhost:5000/metrics | head -20

# Check application logs contain request/response data
echo "2. Checking logs for structured logging"
docker logs dnd-api | grep -i "request\|response" | head -5

echo "✓ Telemetry collection verification complete"
```

## Automated Smoke Test Suite

### shell Script

Create `scripts/smoke_tests.sh`:

```bash
#!/bin/bash

set -e

API_URL="${1:-http://localhost:5000}"
FAILED=0
PASSED=0

test_endpoint() {
  local method=$1
  local endpoint=$2
  local expected_status=$3
  local data=$4
  
  echo -n "Testing $method $endpoint... "
  
  if [ -z "$data" ]; then
    STATUS=$(curl -s -w "%{http_code}" -o /dev/null -X $method $API_URL$endpoint)
  else
    STATUS=$(curl -s -w "%{http_code}" -o /dev/null -X $method $API_URL$endpoint \
      -H "Content-Type: application/json" \
      -d "$data")
  fi
  
  if [ "$STATUS" = "$expected_status" ] || [ "$STATUS" = "200" ]; then
    echo "✓ ($STATUS)"
    ((PASSED++))
  else
    echo "✗ (Expected $expected_status, got $STATUS)"
    ((FAILED++))
  fi
}

# Run tests
echo "Running Smoke Tests for $API_URL"
echo "=================================="

test_endpoint "GET" "/health" "200"
test_endpoint "GET" "/health/ready" "200"
test_endpoint "GET" "/health/live" "200"

echo "=================================="
echo "Results: $PASSED passed, $FAILED failed"

if [ $FAILED -gt 0 ]; then
  exit 1
fi
```

### k6 Script

Create `scripts/smoke_tests.js`:

```javascript
import http from 'k6/http';
import { check } from 'k6';

const API_URL = 'http://localhost:5000';

export let options = {
  vus: 1,
  duration: '1m',
  thresholds: {
    http_req_failed: ['rate<0.1'],
  },
};

export default function() {
  // Test health endpoints
  let res = http.get(`${API_URL}/health`);
  check(res, {
    'health status is 200': (r) => r.status === 200,
  });

  res = http.get(`${API_URL}/health/ready`);
  check(res, {
    'health ready status is 200': (r) => r.status === 200,
  });

  res = http.get(`${API_URL}/health/live`);
  check(res, {
    'health live status is 200': (r) => r.status === 200,
  });

  // Test auth endpoint
  res = http.post(`${API_URL}/api/v1/auth/login`, JSON.stringify({
    email: 'test@example.com',
    password: 'TestPassword123!',
  }), {
    headers: { 'Content-Type': 'application/json' },
  });

  check(res, {
    'login response received': (r) => r.status === 200 || r.status === 401,
  });
}
```

## Running Smoke Tests

```bash
# Run shell script
./scripts/smoke_tests.sh http://localhost:5000

# Run k6 script
k6 run scripts/smoke_tests.js

# Run in CI/CD
./scripts/smoke_tests.sh https://api.production.com
```

## Success Criteria

All smoke tests must pass:
- [ ] Health endpoints return 200 OK
- [ ] Authentication flow works (register → approve → login)
- [ ] CRUD operations functional
- [ ] Authorization enforced
- [ ] Rate limiting active
- [ ] Security headers present
- [ ] No 5xx errors in logs
- [ ] Telemetry collection active

## Post-Deployment Verification Checklist

- [ ] Application responding to requests
- [ ] Database connectivity verified
- [ ] Authentication working
- [ ] Authorization working
- [ ] Rate limiting active
- [ ] Security headers present
- [ ] Telemetry flowing
- [ ] No critical errors in logs
- [ ] Performance acceptable
- [ ] All smoke tests passing

## Troubleshooting

### Connection Refused
- Verify API is running: `docker ps`
- Check port: `netstat -an | grep 5000`
- Review startup logs: `docker logs dnd-api`

### Authentication Failed
- Verify user exists and is approved
- Check JWT configuration
- Review auth logs

### Health Check Failed
- Check database connection
- Verify configuration
- Review startup logs

### Rate Limiting Not Working
- Verify rate limiting middleware is configured
- Check configuration values
- Verify rate limiting policy applied
