# GitHub Actions CI/CD Guide

## Overview

This guide explains how to use the GitHub Actions workflow for your DnD MapBuilder API, and how to configure secrets (similar to GitLab CI/CD variables).

## GitLab vs GitHub Actions: Key Differences

| Feature | GitLab CI/CD | GitHub Actions |
|---------|--------------|----------------|
| **Config File** | `.gitlab-ci.yml` (root) | `.github/workflows/*.yml` |
| **Variables/Secrets** | Settings > CI/CD > Variables | Settings > Secrets and variables > Actions |
| **Jobs** | Defined in stages | Defined in jobs (can run in parallel or sequence) |
| **Runners** | GitLab Runners | GitHub-hosted or self-hosted runners |
| **Docker Registry** | GitLab Container Registry | GitHub Container Registry (ghcr.io) |
| **Manual Trigger** | `when: manual` | `workflow_dispatch` |

## How the Workflow Works

The workflow has 3 jobs that run sequentially:

### 1. Build and Test (`build-and-test`)
- Triggers on: Push to `main`/`develop` or Pull Requests
- Checks out code
- Sets up .NET 10.0
- Restores dependencies
- Builds the solution
- Runs tests

### 2. Build Docker Image (`build-docker`)
- Runs only on push to `main` branch (after tests pass)
- Builds Docker image using your Dockerfile
- Pushes to GitHub Container Registry (ghcr.io)
- Tags images with branch name, commit SHA, and 'latest'

### 3. Deploy to Server (`deploy`)
- Runs only on push to `main` branch (after Docker build)
- Connects to your server via SSH
- Pulls the latest Docker image
- Stops old container
- Starts new container with environment variables
- Verifies deployment

## Setting Up Secrets (GitLab Variables Equivalent)

In GitHub, secrets are stored in: **Repository Settings > Secrets and variables > Actions**

### Required Secrets

#### Server Connection Secrets
```
SECRET NAME: SERVER_HOST
VALUE: your-server-ip-or-domain.com
DESCRIPTION: IP address or domain of your deployment server
```

```
SECRET NAME: SERVER_USERNAME
VALUE: ubuntu (or your SSH username)
DESCRIPTION: SSH username for server access
```

```
SECRET NAME: SSH_PRIVATE_KEY
VALUE: -----BEGIN OPENSSH PRIVATE KEY-----
...your private key content...
-----END OPENSSH PRIVATE KEY-----
DESCRIPTION: SSH private key for server authentication
```

```
SECRET NAME: SERVER_PORT
VALUE: 22 (optional, defaults to 22)
DESCRIPTION: SSH port if different from 22
```

#### Application Secrets
```
SECRET NAME: DB_CONNECTION_STRING
VALUE: Host=your-db-host;Database=dndmapbuilder;Username=dbuser;Password=dbpass
DESCRIPTION: PostgreSQL/SQL Server connection string
```

```
SECRET NAME: JWT_SECRET
VALUE: your-super-secret-jwt-key-at-least-32-characters-long
DESCRIPTION: Secret key for JWT token signing
```

```
SECRET NAME: JWT_ISSUER
VALUE: https://your-api-domain.com
DESCRIPTION: JWT token issuer
```

```
SECRET NAME: JWT_AUDIENCE
VALUE: https://your-api-domain.com
DESCRIPTION: JWT token audience
```

### How to Add Secrets

1. Go to your GitHub repository
2. Click **Settings** tab
3. In the left sidebar, click **Secrets and variables** > **Actions**
4. Click **New repository secret**
5. Enter the secret name and value
6. Click **Add secret**

### Secret Scopes

- **Repository secrets**: Available to all workflows in the repository (default)
- **Environment secrets**: Scoped to specific environments (production, staging)
- **Organization secrets**: Shared across multiple repositories

## Generating SSH Key Pair

If you don't have SSH keys set up:

```bash
# On your local machine
ssh-keygen -t ed25519 -C "github-actions-deploy" -f ~/.ssh/github_actions_deploy

# Copy public key to your server
ssh-copy-id -i ~/.ssh/github_actions_deploy.pub user@your-server.com

# Copy private key content to GitHub secret
cat ~/.ssh/github_actions_deploy
# Copy the entire output including BEGIN and END lines
```

## GitHub Container Registry Setup

GitHub Container Registry (ghcr.io) is free and automatically available. No extra setup needed!

The workflow uses `GITHUB_TOKEN` which is automatically provided by GitHub Actions.

### Making Your Image Public (Optional)

1. Go to your package: https://github.com/users/YOUR_USERNAME/packages/container/YOUR_REPO
2. Click **Package settings**
3. Change visibility to Public if desired

## Triggering the Workflow

### Automatic Triggers
- **Push to `main` or `develop`**: Runs build, test, Docker build (only on main), and deploy (only on main)
- **Pull Request to `main` or `develop`**: Runs build and test only

### Manual Trigger
1. Go to **Actions** tab in your repository
2. Select **CI/CD Pipeline** workflow
3. Click **Run workflow** button
4. Select branch and click **Run workflow**

## Monitoring Workflow Runs

1. Go to **Actions** tab in your repository
2. Click on a workflow run to see details
3. Click on individual jobs to see logs
4. Failed steps will be highlighted in red

## Environment Variables in Workflow

There are two types of variables:

### 1. Secrets (Sensitive Data)
```yaml
${{ secrets.DB_CONNECTION_STRING }}
```
- Encrypted and hidden in logs
- Used for passwords, keys, tokens

### 2. Environment Variables (Non-Sensitive)
```yaml
env:
  DOTNET_VERSION: '10.0.x'
```
- Visible in workflow file
- Used for versions, public URLs

### 3. GitHub Context Variables
```yaml
${{ github.repository }}  # owner/repo-name
${{ github.actor }}       # username who triggered
${{ github.ref }}         # branch reference
```

## Customizing the Workflow

### Change Deployment Branch
To deploy from `develop` instead of `main`:
```yaml
if: github.event_name == 'push' && github.ref == 'refs/heads/develop'
```

### Add Staging Environment
```yaml
deploy-staging:
  if: github.ref == 'refs/heads/develop'
  environment: staging
  # ... steps
```

### Add Database Migrations
Add before deployment:
```yaml
- name: Run migrations
  uses: appleboy/ssh-action@v1.0.0
  with:
    host: ${{ secrets.SERVER_HOST }}
    username: ${{ secrets.SERVER_USERNAME }}
    key: ${{ secrets.SSH_PRIVATE_KEY }}
    script: |
      docker exec dnd-api dotnet ef database update
```

## Caching for Faster Builds

The workflow already includes Docker layer caching:
```yaml
cache-from: type=gha
cache-to: type=gha,mode=max
```

To add .NET dependency caching:
```yaml
- uses: actions/cache@v3
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
```

## Notifications

### Slack Notifications
Add to the end of deploy job:
```yaml
- name: Notify Slack
  uses: slackapi/slack-github-action@v1
  with:
    webhook-url: ${{ secrets.SLACK_WEBHOOK_URL }}
    payload: |
      {
        "text": "Deployment completed: ${{ job.status }}"
      }
```

### Email Notifications
GitHub sends email notifications for failed workflows automatically.

## Troubleshooting

### SSH Connection Fails
- Verify `SERVER_HOST`, `SERVER_USERNAME`, and `SSH_PRIVATE_KEY` secrets
- Ensure server allows SSH from GitHub's IP ranges
- Check that public key is in `~/.ssh/authorized_keys` on server

### Docker Pull Fails
- Ensure GitHub token has package read permissions
- Make package public or use Personal Access Token with `read:packages` scope

### Container Won't Start
- Check secrets are correctly set
- Review container logs: `docker logs dnd-api`
- Verify environment variables format

## Security Best Practices

1. **Never commit secrets to code**
2. **Use environment-specific secrets** for production vs staging
3. **Rotate secrets regularly**
4. **Use least-privilege service accounts** for SSH access
5. **Enable branch protection** on `main` to require PR reviews
6. **Use environment protection rules** to require manual approval for production deploys

## Next Steps

1. Add secrets to your GitHub repository
2. Push this workflow to your `main` branch
3. Monitor the Actions tab for the first run
4. Adjust the workflow based on your specific needs
