# Backend CI/CD Workflow Setup Instructions

## Overview
This setup implements a multi-environment deployment strategy:
- **Feature branches** → PR to develop (runs tests only)
- **develop branch** → Development environment deployment
- **main branch** → Production environment deployment
- **Protected branches** → No direct pushes allowed

## Required GitHub Secrets

### Development Environment Secrets
Add these secrets to your GitHub repository (Settings → Secrets and Variables → Actions):

```
DEV_SERVER_HOST              # Development server IP/hostname
DEV_SERVER_USERNAME          # SSH username for dev server
DEV_SSH_PRIVATE_KEY          # SSH private key for dev server
DEV_SERVER_PORT              # SSH port (optional, defaults to 22)
DEV_DB_CONNECTION_STRING     # Development database connection string
```

### Production Environment Secrets
```
PROD_SERVER_HOST             # Production server IP/hostname
PROD_SERVER_USERNAME         # SSH username for prod server
PROD_SSH_PRIVATE_KEY         # SSH private key for prod server
PROD_SERVER_PORT             # SSH port (optional, defaults to 22)
PROD_DB_CONNECTION_STRING    # Production database connection string
```

## Workflow File

Replace the contents of `.github/workflows/main.yml` with the example below.

The workflow includes:
1. **build-and-test** - Runs on all PRs and pushes (no deployment)
2. **build-docker** - Builds Docker image only on pushes to develop/main
3. **deploy-dev** - Deploys to development when commits pushed to develop
4. **deploy-prod** - Deploys to production when commits pushed to main

Each deployment job:
- Uses environment-specific secrets
- Runs database migrations
- Deploys to a separate container instance
- Verifies health checks before completion

## Setup Steps

### 1. Add GitHub Secrets

Go to: Repository → Settings → Secrets and Variables → Actions → New repository secret

Add each secret listed above with values from your actual servers.

### 2. Update Workflow File

Copy the contents of `backend-main-example.yml` to `.github/workflows/main.yml`:

```bash
cp backend-main-example.yml .github/workflows/main.yml
```

### 3. Protect Main and Develop Branches

Go to: Repository → Settings → Branches → Add branch protection rule

**For `main` branch:**
- ✅ Require pull request reviews before merging
- ✅ Require status checks to pass before merging
  - Select `build-and-test`
  - Select `build-docker`
  - Select `deploy-prod` (production environment)
- ✅ Require branches to be up to date before merging
- ✅ Include administrators
- ✅ Restrict who can push to matching branches (only releases if used)

**For `develop` branch:**
- ✅ Require pull request reviews before merging
- ✅ Require status checks to pass before merging
  - Select `build-and-test`
  - Select `build-docker`
  - Select `deploy-dev` (development environment)
- ✅ Require branches to be up to date before merging
- ✅ Include administrators
- ✅ Restrict who can push to matching branches

### 4. Create .gitignore Exception (Optional)

If you want to keep the example files in version control for reference:

```bash
# .gitignore
# Keep workflow examples for reference
!backend-main-example.yml
```

## Workflow Behavior

### When developing a feature:
1. Create feature branch from `develop`
2. Push commits and create PR to `develop`
3. CI runs: build-and-test only (no deployment)
4. After review, merge to `develop`
5. CI runs: build-and-test, build-docker, deploy-dev (development deployment)

### When releasing to production:
1. Create PR from `develop` to `main`
2. CI runs: build-and-test only (no deployment)
3. After review, merge to `main`
4. CI runs: build-and-test, build-docker, deploy-prod (production deployment)

## Container Naming

- Development: `dnd-api-dev` (ports 5000-5001)
- Production: `dnd-api` (ports 5000-5001)

## Environment Variables

Docker containers are deployed with:
- **Development**: `ASPNETCORE_ENVIRONMENT=Development`
- **Production**: `ASPNETCORE_ENVIRONMENT=Production`

## Monitoring Deployments

Check workflow runs in: Repository → Actions

Each deployment shows:
- Build and test results
- Docker image build and push
- SSH deployment commands
- Health check verification
- Container logs

## Troubleshooting

**Deployment fails with "Host key verification failed":**
- Ensure the SSH private key is properly formatted (including newlines)
- The host must be in the SSH known_hosts file or the key must be added correctly

**Docker login fails:**
- Verify `GITHUB_TOKEN` has necessary permissions
- Check if `secrets.GITHUB_TOKEN` is automatically provided (it is in GitHub Actions)

**Database migrations fail:**
- Verify connection string is correct
- Ensure the database server is accessible from the deployment server
- Check Entity Framework migration files exist

**Health check timeouts:**
- May need to increase wait time if initial startup is slow
- Check container logs in the verification step

## Future Enhancement: Staging Environment

When ready to add a staging environment (release branch):

1. Create branch protection rule for `release` branch
2. Add staging secrets: `STAGING_SERVER_HOST`, `STAGING_SERVER_USERNAME`, etc.
3. Add `deploy-staging` job triggered on commits to `release` branch
4. Update branch protection rules accordingly
