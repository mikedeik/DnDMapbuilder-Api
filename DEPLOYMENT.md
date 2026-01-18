# Deployment Runbook

## Overview

This runbook documents the procedures, checklists, and strategies for deploying DnDMapBuilder API to production and staging environments.

## Pre-Deployment Checklist

### Code and Build
- [ ] All tests passing: `dotnet test`
- [ ] Build succeeding: `dotnet build --configuration Release`
- [ ] No compiler warnings
- [ ] Code review completed and approved
- [ ] All changes merged to main branch
- [ ] Version bumped (semantic versioning)

### Security
- [ ] No vulnerable dependencies: `dotnet list package --vulnerable`
- [ ] Secrets not in source code
- [ ] CORS origins configured for target environment
- [ ] Security headers configured
- [ ] Rate limiting policies appropriate
- [ ] Authentication/authorization tests passing

### Database
- [ ] Database migrations tested locally
- [ ] Backup created before migration
- [ ] Rollback plan documented
- [ ] Data consistency verified

### Infrastructure
- [ ] Target environment verified (staging/production)
- [ ] Database connection string verified
- [ ] Key Vault secrets configured
- [ ] Health check endpoints responding
- [ ] Monitoring and logging configured

### Documentation
- [ ] Release notes prepared
- [ ] Breaking changes documented
- [ ] Migration guide prepared (if applicable)
- [ ] Runbook reviewed

## Deployment Steps

### Phase 1: Pre-Deployment (30 minutes)

1. **Create deployment ticket**
   - Document deployment date/time
   - List changes and version
   - Assign responsible engineer

2. **Verify environment readiness**
   ```bash
   # Check target environment
   docker ps -a
   docker network ls
   
   # Verify services
   curl https://api.example.com/health
   
   # Check logs
   docker logs dnd-api
   ```

3. **Take database backup**
   ```bash
   # SQL Server backup
   docker exec -it dnd-db sqlcmd -S localhost -U sa \
     -P $SA_PASSWORD \
     -Q "BACKUP DATABASE [DnDMapBuilder] \
         TO DISK = '/var/opt/mssql/backup/pre-deployment.bak'"
   ```

4. **Notify stakeholders**
   - Post deployment notification to Slack/Teams
   - Set deployment status page to "In Progress"
   - Alert support team of potential service interruption

### Phase 2: Application Deployment (15-30 minutes)

#### Option A: Docker Compose (Single Host)

```bash
# 1. Navigate to deployment directory
cd ~/dnd-deployment

# 2. Pull latest image
docker pull ghcr.io/yourorg/dnd-api:latest

# 3. Stop current container
docker-compose down

# 4. Start new container
docker-compose up -d

# 5. Verify startup
sleep 10
docker logs dnd-api | tail -20
```

#### Option B: Kubernetes (Production)

```bash
# 1. Update image version in deployment
kubectl set image deployment/dnd-api \
  dnd-api=ghcr.io/yourorg/dnd-api:v1.2.3

# 2. Monitor rollout
kubectl rollout status deployment/dnd-api

# 3. Verify pods running
kubectl get pods -l app=dnd-api

# 4. Check logs
kubectl logs -l app=dnd-api -f
```

#### Option C: Azure App Service

```bash
# 1. Deploy using Azure CLI
az webapp deployment source config-zip \
  --resource-group dnd-rg \
  --name dnd-api-app \
  --src-path release.zip

# 2. Monitor deployment
az webapp deployment slot show \
  --resource-group dnd-rg \
  --name dnd-api-app \
  --slot staging

# 3. Swap slots when ready
az webapp deployment slot swap \
  --resource-group dnd-rg \
  --name dnd-api-app \
  --slot staging
```

### Phase 3: Database Migration (10-20 minutes)

If database schema changes exist:

```bash
# 1. Run migrations
docker exec -it dnd-api \
  dotnet ef database update --context DnDMapBuilderDbContext

# 2. Verify migration
docker exec -it dnd-api \
  dotnet ef migrations list --context DnDMapBuilderDbContext

# 3. Check database state
docker exec -it dnd-db sqlcmd -S localhost -U sa -P $SA_PASSWORD \
  -Q "SELECT TOP 5 * FROM dbo.Users"
```

### Phase 4: Post-Deployment Verification (15-20 minutes)

1. **Health Check**
   ```bash
   curl https://api.example.com/health
   curl https://api.example.com/health/ready
   curl https://api.example.com/health/live
   ```

2. **Smoke Tests**
   - Login endpoint: `curl -X POST https://api.example.com/api/v1/auth/login`
   - Get user data: `curl -H "Authorization: Bearer $TOKEN" https://api.example.com/api/v1/campaigns`
   - Create resource: Test campaign creation
   - File upload: Test image upload
   - Rate limiting: Verify 429 responses

3. **Monitor Metrics**
   ```bash
   # Check application logs
   docker logs -f dnd-api
   
   # Check error rate
   curl https://api.example.com/metrics | grep http_requests_total
   
   # Check performance
   kubectl top pod -l app=dnd-api  # Kubernetes
   ```

4. **Database Consistency**
   ```bash
   # Check record counts
   docker exec -it dnd-db sqlcmd -S localhost -U sa -P $SA_PASSWORD \
     -Q "SELECT COUNT(*) FROM Users; SELECT COUNT(*) FROM Campaigns;"
   ```

## Rollback Procedure

If issues occur during or after deployment:

### Quick Rollback (< 2 minutes)

```bash
# Docker Compose
docker-compose down
git checkout previous-tag
docker-compose up -d

# Kubernetes
kubectl rollout undo deployment/dnd-api

# Azure App Service
az webapp deployment slot swap \
  --resource-group dnd-rg \
  --name dnd-api-app \
  --slot staging
```

### Database Rollback (if needed)

```bash
# SQL Server restore
docker exec -it dnd-db sqlcmd -S localhost -U sa -P $SA_PASSWORD \
  -Q "RESTORE DATABASE [DnDMapBuilder] \
      FROM DISK = '/var/opt/mssql/backup/pre-deployment.bak' \
      WITH REPLACE"

# Verify database
docker exec -it dnd-db sqlcmd -S localhost -U sa -P $SA_PASSWORD \
  -Q "SELECT @@VERSION"
```

### Notification After Rollback

- Post incident notification to Slack/Teams
- Document root cause
- Create ticket for issue resolution
- Plan re-deployment

## Environment-Specific Procedures

### Staging Deployment

```bash
# Deploy to staging for validation
git push origin develop

# Automated deployment via GitHub Actions
# Verify health: https://staging-api.example.com/health

# Run extended smoke tests
./scripts/smoke_tests.sh https://staging-api.example.com

# If tests pass, proceed to production
```

### Production Deployment

```bash
# Merge develop to main
git push origin develop:main

# Wait for automated deployment
# Verify all post-deployment checks
# Monitor for 24 hours

# If issues arise, execute rollback
```

## Monitoring Post-Deployment

### First Hour
- Monitor error rates in real-time
- Check for exceptions in logs
- Verify performance metrics
- Monitor database queries

### First Day
- Check for memory leaks
- Verify all endpoints responding
- Monitor rate limiting effectiveness
- Check for security events

### First Week
- Review error trends
- Analyze performance metrics
- Check for regressions
- Verify telemetry collection

## Incident Response During Deployment

### If deployment fails to complete

1. **Assess severity**
   - Is service down or degraded?
   - Are users affected?
   - Can it be fixed quickly?

2. **Immediate action**
   - If critical: Execute rollback immediately
   - If minor: Continue investigation

3. **Investigation**
   - Check logs: `docker logs dnd-api`
   - Check database: Verify connection
   - Check infrastructure: Resource availability
   - Check dependencies: External services

4. **Communication**
   - Update status page
   - Notify stakeholders
   - Post incident channel updates
   - Document timeline

### If health checks fail

1. Verify network connectivity
2. Check service startup logs
3. Verify configuration files
4. Check database connectivity
5. Execute rollback if unable to resolve quickly

### If performance degrades

1. Check memory usage
2. Monitor CPU utilization
3. Analyze slow queries
4. Check rate limiting
5. Scale infrastructure if necessary

## Post-Deployment Reporting

### Deployment Report Template

```
Deployment Report - [Date]

Deployment Details:
- Version: [Version number]
- Environment: [staging/production]
- Start time: [Time]
- End time: [Time]
- Duration: [Minutes]

Changes Deployed:
- [List of features/fixes]

Pre-Deployment Checks:
- [✓] Tests passing
- [✓] Security audit passed
- [✓] Database backup created

Post-Deployment Results:
- [✓] Health checks passing
- [✓] Smoke tests passed
- [✓] Performance acceptable

Issues Encountered:
- None

Verification:
- [✓] Endpoints responding
- [✓] Authentication working
- [✓] File uploads functional
- [✓] Rate limiting active

Rollback Status:
- Not required

Signed off by: [Name]
Date: [Date]
```

## Deployment Automation

### GitHub Actions Workflow

```yaml
name: Deploy to Production

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      
      - name: Build Docker image
        run: docker build -t dnd-api:${{ github.sha }} .
      
      - name: Push to registry
        run: docker push dnd-api:${{ github.sha }}
      
      - name: Deploy to production
        run: |
          # Deploy commands
          docker pull dnd-api:${{ github.sha }}
          docker-compose up -d
      
      - name: Run smoke tests
        run: ./scripts/smoke_tests.sh
      
      - name: Notify deployment
        uses: 8398a7/action-slack@v3
        with:
          status: ${{ job.status }}
```

## Disaster Recovery

### Database Corruption

1. Restore from latest backup
2. Verify data consistency
3. Run integrity checks
4. Monitor for anomalies

### Complete Service Failure

1. Execute rollback procedure
2. Restore from backup
3. Verify service startup
4. Run full smoke test suite

### Data Loss Scenario

1. Restore from backup (ideally < 1 hour old)
2. Verify backup integrity
3. Communicate to users about data recovery
4. Monitor for issues

## References

- [ASP.NET Core Deployment](https://learn.microsoft.com/aspnet/core/host-and-deploy/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Kubernetes Deployment](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/)
- [Incident Response](https://www.atlassian.com/incident-management)
