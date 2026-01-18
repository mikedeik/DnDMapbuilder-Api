# Load Testing and Performance Validation Guide

## Overview

This guide provides load testing strategies and performance benchmarks for the DnDMapBuilder API. Load testing helps identify bottlenecks, validate rate limiting, and ensure the API can handle expected and peak loads.

## Load Testing Tools

### Recommended Tools

1. **k6** (JavaScript-based, cloud-native)
   - Easy to write tests in JavaScript
   - Real-time results
   - Cloud execution for distributed load
   - Excellent metrics and visualization

2. **Apache JMeter** (Java-based, GUI)
   - Enterprise standard
   - Good for complex scenarios
   - Built-in reporting
   - Large community

3. **Apache Bench (ab)** (Simple, CLI)
   - Quick and simple
   - Good for baseline testing
   - Limited features

### Installation

#### k6
```bash
# macOS
brew install k6

# Linux
sudo apt-get install k6

# Windows (via Chocolatey)
choco install k6
```

#### JMeter
```bash
# macOS
brew install jmeter

# Download from https://jmeter.apache.org/download_jmeter.html
```

## Test Scenarios

### Scenario 1: Authentication Load Test

**Objective:** Validate login endpoint under sustained load

**Test Profile:**
- Duration: 2 minutes
- Ramp-up: 30 seconds to 100 concurrent users
- Steady state: 100 users for 1.5 minutes
- Ramp-down: 30 seconds

**Expected Metrics:**
- Response time p95: < 500ms
- Response time p99: < 1000ms
- Error rate: < 0.1%
- Throughput: > 50 requests/sec

**k6 Script:**
```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '30s', target: 100 },
    { duration: '1m30s', target: 100 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500', 'p(99)<1000'],
    http_req_failed: ['rate<0.01'],
  },
};

const BASE_URL = 'http://localhost:5000/api/v1';

export default function() {
  const loginPayload = JSON.stringify({
    email: `user${Math.floor(Math.random() * 1000)}@example.com`,
    password: 'TestPassword123!',
  });

  const response = http.post(`${BASE_URL}/auth/login`, loginPayload, {
    headers: { 'Content-Type': 'application/json' },
  });

  check(response, {
    'login status is 200 or 401': (r) => r.status === 200 || r.status === 401,
  });

  sleep(1);
}
```

### Scenario 2: CRUD Operations Load Test

**Objective:** Validate campaign CRUD operations under load

**Test Profile:**
- Duration: 3 minutes
- Ramp-up: 1 minute to 50 concurrent users
- Steady state: 50 users for 1.5 minutes
- Ramp-down: 30 seconds
- Mix: 40% GET, 30% POST, 20% PUT, 10% DELETE

**Expected Metrics:**
- Response time p95: < 300ms
- Response time p99: < 700ms
- Error rate: < 0.5%
- Throughput: > 30 requests/sec

### Scenario 3: File Upload Load Test

**Objective:** Validate file upload performance under concurrent uploads

**Test Profile:**
- Duration: 2 minutes
- Concurrent users: 10 (limited by file upload rate limit)
- File size: 1-3MB images
- Expected: Hits rate limit at ~10 requests/minute

**Expected Metrics:**
- Response time p95: < 2 seconds (includes file I/O)
- Error rate: < 1% (429 status acceptable)
- Rate limit compliance: 100% at > 10 req/min

### Scenario 4: Sustained Load Test

**Objective:** Validate API stability under 8-hour sustained load

**Test Profile:**
- Duration: 8 hours
- Concurrent users: 50 (constant)
- Think time: 1-2 seconds between requests
- Request mix: 60% GET, 20% POST, 15% PUT, 5% DELETE

**Expected Metrics:**
- No memory leaks (stable memory usage)
- Response time degradation: < 10% over 8 hours
- Error rate: < 0.1%
- Database connection pool stable

### Scenario 5: Spike Load Test

**Objective:** Validate API behavior during sudden traffic spikes

**Test Profile:**
- Baseline: 10 concurrent users
- Spike: Increase to 500 concurrent users in 10 seconds
- Hold: Maintain for 2 minutes
- Return to baseline

**Expected Metrics:**
- Response time p99 during spike: < 2000ms
- Error rate during spike: < 5%
- Recovery: Return to normal response times within 1 minute

## Performance Benchmarks

### Target Metrics

| Metric | Target | Acceptable | Warning |
|--------|--------|-----------|---------|
| Response Time (p50) | < 100ms | < 200ms | > 200ms |
| Response Time (p95) | < 300ms | < 500ms | > 500ms |
| Response Time (p99) | < 500ms | < 1000ms | > 1000ms |
| Error Rate | < 0.1% | < 0.5% | > 0.5% |
| Throughput | > 100 req/s | > 50 req/s | < 50 req/s |
| CPU Utilization | < 60% | < 80% | > 80% |
| Memory Utilization | < 60% | < 80% | > 80% |
| Database Connection Pool | < 80% utilized | < 90% utilized | > 90% utilized |

## Running Load Tests

### Using k6

```bash
# Basic run
k6 run load_test.js

# With output file
k6 run load_test.js --out csv=results.csv

# Cloud execution (requires k6 Cloud account)
k6 cloud load_test.js

# With custom environment
k6 run -e BASE_URL=https://api.production.com load_test.js
```

### Using Apache JMeter

```bash
# Interactive GUI
jmeter -t load_test.jmx

# Headless (command line)
jmeter -n -t load_test.jmx -l results.jtl -j jmeter.log

# Generate HTML report
jmeter -g results.jtl -o report/
```

## Performance Optimization Recommendations

### Based on Load Test Results

**If response times > target:**
1. Check database query performance (use query analyzer)
2. Add caching for frequently accessed data
3. Implement pagination for large result sets
4. Consider read replicas for database scaling
5. Review middleware performance (remove unnecessary middleware)
6. Profile application code for hotspots

**If error rate > acceptable:**
1. Review error logs for specific failure modes
2. Increase database connection pool size
3. Add circuit breaker for external dependencies
4. Implement request queuing/backpressure
5. Scale horizontally (add more instances)

**If memory usage increases:**
1. Profile for memory leaks
2. Review logging configuration (excessive log data)
3. Check for unbounded collections
4. Verify connection pool cleanup
5. Monitor garbage collection frequency

**If CPU usage high:**
1. Profile CPU-intensive operations
2. Review encryption/hashing operations
3. Check for tight loops
4. Consider async operations where applicable
5. Profile LINQ query compilation

## Continuous Load Testing

### CI/CD Integration

Add to GitHub Actions workflow:

```yaml
name: Load Testing

on:
  schedule:
    - cron: '0 2 * * 0'  # Weekly at 2 AM UTC

jobs:
  load-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      
      - name: Install k6
        run: sudo apt-get install k6
      
      - name: Start API
        run: |
          docker-compose up -d
          sleep 10
      
      - name: Run Load Tests
        run: k6 run load_tests/auth_load_test.js
      
      - name: Upload Results
        uses: actions/upload-artifact@v2
        with:
          name: load-test-results
          path: results/
```

## Monitoring During Load Tests

### Metrics to Monitor

**Application Level:**
- CPU and Memory usage
- Request rate (requests/sec)
- Response times (p50, p95, p99)
- Error rate
- Active connections

**Database Level:**
- Query execution time
- Connection pool utilization
- Deadlock occurrences
- I/O operations

**Infrastructure Level:**
- Network bandwidth
- Disk I/O
- System load
- Available resources

### Tools

- **Grafana:** Real-time metrics visualization
- **Prometheus:** Metrics collection
- **Application Insights:** Azure integrated monitoring
- **DataDog:** Enterprise monitoring
- **New Relic:** APM and monitoring

## Load Test Results Documentation

### Standard Report Includes

1. **Executive Summary**
   - Test date and duration
   - Peak concurrent users
   - Overall pass/fail status

2. **Key Metrics**
   - Response time statistics (min, max, avg, p50, p95, p99)
   - Throughput (requests/sec)
   - Error rate and error types
   - Resource utilization (CPU, Memory, Disk)

3. **Scenario Results**
   - Per-scenario performance
   - Any threshold violations
   - Anomalies or unexpected behavior

4. **Analysis**
   - Bottleneck identification
   - Performance trends
   - Comparison to previous tests

5. **Recommendations**
   - Optimization opportunities
   - Configuration adjustments
   - Infrastructure scaling needs

## Baseline Metrics

### Initial Deployment (Single Instance)

Record baseline metrics for comparison:

```
Configuration: 
- 1 API instance (2 vCPU, 4GB RAM)
- 1 SQL Server instance (2 vCPU, 8GB RAM)

Scenario: 100 concurrent users, 2 minute duration
- Response Time p50: 89ms
- Response Time p95: 234ms
- Response Time p99: 456ms
- Error Rate: 0.02%
- Throughput: 85 requests/sec
- CPU (API): 42%
- Memory (API): 512MB
- Database Connections: 12/50
```

## Maintenance and Review

- **Monthly:** Review baselines against current performance
- **Quarterly:** Full regression load testing
- **Before Major Release:** Comprehensive load testing
- **After Infrastructure Changes:** Re-baseline metrics
- **Performance Regression:** Immediate investigation

## References

- [k6 Documentation](https://k6.io/docs/)
- [Apache JMeter](https://jmeter.apache.org/usermanual/)
- [Performance Testing Guide](https://www.perfmatrix.com/)
- [Load Testing Best Practices](https://www.thoughtworks.com/insights/blog/load-testing)
