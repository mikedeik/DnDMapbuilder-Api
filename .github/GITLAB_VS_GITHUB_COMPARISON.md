# GitLab CI/CD vs GitHub Actions: Side-by-Side Comparison

## Configuration File Structure

### GitLab CI/CD (.gitlab-ci.yml)
```yaml
stages:
  - build
  - test
  - deploy

variables:
  DOTNET_VERSION: "10.0"

build_job:
  stage: build
  image: mcr.microsoft.com/dotnet/sdk:10.0
  script:
    - dotnet restore
    - dotnet build
  only:
    - main
    - develop

deploy_job:
  stage: deploy
  script:
    - ssh user@server "docker pull image"
  only:
    - main
  when: manual
```

### GitHub Actions (.github/workflows/ci-cd.yml)
```yaml
on:
  push:
    branches: [ main, develop ]

env:
  DOTNET_VERSION: '10.0.x'

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Build
        run: |
          dotnet restore
          dotnet build

  deploy:
    needs: build
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    steps:
      - name: Deploy
        run: ssh user@server "docker pull image"
    # workflow_dispatch enables manual trigger
```

## Variables and Secrets

### GitLab: CI/CD Variables
**Location**: Settings > CI/CD > Variables

```yaml
# In .gitlab-ci.yml
script:
  - echo $DB_CONNECTION_STRING
  - echo $CI_COMMIT_SHA  # GitLab predefined variable
```

**Variable Types**:
- Regular variables
- Protected variables (only for protected branches)
- Masked variables (hidden in logs)
- File variables (content saved to temp file)

### GitHub: Secrets and Variables
**Location**: Settings > Secrets and variables > Actions

```yaml
# In workflow file
run: |
  echo ${{ secrets.DB_CONNECTION_STRING }}
  echo ${{ github.sha }}  # GitHub context variable
```

**Types**:
- Secrets (encrypted, hidden in logs)
- Variables (plain text, visible)
- Environment secrets (scoped to environments)

## Key Concept Mapping

| GitLab Concept | GitHub Actions Equivalent | Notes |
|----------------|---------------------------|-------|
| `.gitlab-ci.yml` | `.github/workflows/*.yml` | GitHub allows multiple workflow files |
| `stages:` | `jobs:` with `needs:` | GitHub jobs run in parallel by default |
| `stage: build` | `jobs: build:` | Define job dependencies with `needs:` |
| `script:` | `steps:` with `run:` | GitHub uses steps within jobs |
| `image:` | `runs-on:` or `container:` | GitHub has hosted runners |
| `only:` / `except:` | `if:` conditions | GitHub uses expressions |
| `when: manual` | `workflow_dispatch` event | Different trigger mechanism |
| `artifacts:` | `actions/upload-artifact` | GitHub uses actions |
| `cache:` | `actions/cache` | GitHub uses actions |
| `before_script:` | Common step at start | No built-in concept |
| `after_script:` | Add step with `if: always()` | No built-in concept |
| `variables:` | `env:` | GitHub also has `secrets` |
| `$CI_*` variables | `${{ github.* }}` context | Different syntax and names |
| `extends:` | Reusable workflows | Different mechanism |
| `rules:` | `if:` conditions | GitHub uses expressions |
| `environment:` | `environment:` | Similar concept |
| `services:` | `services:` in job | Similar concept |
| `retry:` | `uses: nick-invision/retry@v2` | Needs action |

## Common Variables/Context Mapping

| GitLab Variable | GitHub Context | Value |
|-----------------|----------------|-------|
| `$CI_COMMIT_SHA` | `${{ github.sha }}` | Commit SHA |
| `$CI_COMMIT_REF_NAME` | `${{ github.ref_name }}` | Branch name |
| `$CI_PIPELINE_ID` | `${{ github.run_id }}` | Pipeline/Run ID |
| `$CI_JOB_ID` | `${{ github.job }}` | Job ID |
| `$CI_PROJECT_NAME` | `${{ github.repository }}` | Project name |
| `$CI_COMMIT_MESSAGE` | `${{ github.event.head_commit.message }}` | Commit message |
| `$CI_COMMIT_BRANCH` | `${{ github.ref_name }}` | Branch name |
| `$GITLAB_USER_LOGIN` | `${{ github.actor }}` | Username |
| `$CI_REGISTRY` | `ghcr.io` | Container registry |
| `$CI_REGISTRY_USER` | `${{ github.actor }}` | Registry username |
| `$CI_REGISTRY_PASSWORD` | `${{ secrets.GITHUB_TOKEN }}` | Registry password |

## SSH Deployment Comparison

### GitLab CI/CD
```yaml
deploy:
  stage: deploy
  before_script:
    - eval $(ssh-agent -s)
    - echo "$SSH_PRIVATE_KEY" | tr -d '\r' | ssh-add -
    - mkdir -p ~/.ssh
    - chmod 700 ~/.ssh
    - ssh-keyscan $SERVER_HOST >> ~/.ssh/known_hosts
  script:
    - ssh $SERVER_USER@$SERVER_HOST "
        docker pull $CI_REGISTRY_IMAGE:latest &&
        docker stop app || true &&
        docker run -d --name app $CI_REGISTRY_IMAGE:latest
      "
  only:
    - main
```

**GitLab Variables to Set**:
- `SSH_PRIVATE_KEY` (Type: File, Protected, Masked)
- `SERVER_HOST`
- `SERVER_USER`

### GitHub Actions
```yaml
deploy:
  runs-on: ubuntu-latest
  if: github.ref == 'refs/heads/main'
  steps:
    - uses: appleboy/ssh-action@v1.0.0
      with:
        host: ${{ secrets.SERVER_HOST }}
        username: ${{ secrets.SERVER_USERNAME }}
        key: ${{ secrets.SSH_PRIVATE_KEY }}
        script: |
          docker pull ghcr.io/${{ github.repository }}:latest
          docker stop app || true
          docker run -d --name app ghcr.io/${{ github.repository }}:latest
```

**GitHub Secrets to Set**:
- `SSH_PRIVATE_KEY`
- `SERVER_HOST`
- `SERVER_USERNAME`

## Docker Build & Push Comparison

### GitLab CI/CD
```yaml
docker-build:
  stage: build
  image: docker:latest
  services:
    - docker:dind
  before_script:
    - docker login -u $CI_REGISTRY_USER -p $CI_REGISTRY_PASSWORD $CI_REGISTRY
  script:
    - docker build -t $CI_REGISTRY_IMAGE:$CI_COMMIT_SHA .
    - docker tag $CI_REGISTRY_IMAGE:$CI_COMMIT_SHA $CI_REGISTRY_IMAGE:latest
    - docker push $CI_REGISTRY_IMAGE:$CI_COMMIT_SHA
    - docker push $CI_REGISTRY_IMAGE:latest
```

**No extra variables needed** - GitLab provides `$CI_REGISTRY_*` automatically

### GitHub Actions
```yaml
docker-build:
  runs-on: ubuntu-latest
  steps:
    - uses: actions/checkout@v4

    - uses: docker/login-action@v3
      with:
        registry: ghcr.io
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}

    - uses: docker/build-push-action@v5
      with:
        context: .
        push: true
        tags: |
          ghcr.io/${{ github.repository }}:${{ github.sha }}
          ghcr.io/${{ github.repository }}:latest
```

**No extra secrets needed** - GitHub provides `GITHUB_TOKEN` automatically

## Environment-Specific Deployment

### GitLab CI/CD
```yaml
deploy_staging:
  stage: deploy
  script:
    - echo "Deploying to staging"
  environment:
    name: staging
    url: https://staging.example.com
  only:
    - develop

deploy_production:
  stage: deploy
  script:
    - echo "Deploying to production"
  environment:
    name: production
    url: https://example.com
  only:
    - main
  when: manual
```

### GitHub Actions
```yaml
deploy-staging:
  if: github.ref == 'refs/heads/develop'
  environment:
    name: staging
    url: https://staging.example.com
  runs-on: ubuntu-latest
  steps:
    - run: echo "Deploying to staging"

deploy-production:
  if: github.ref == 'refs/heads/main'
  environment:
    name: production
    url: https://example.com
  runs-on: ubuntu-latest
  steps:
    - run: echo "Deploying to production"
```

**Environment Protection Rules** (Settings > Environments):
- Required reviewers (similar to `when: manual`)
- Wait timer
- Deployment branches

## Conditional Execution

### GitLab CI/CD
```yaml
job:
  script:
    - echo "Running"
  rules:
    - if: '$CI_COMMIT_BRANCH == "main"'
    - if: '$CI_PIPELINE_SOURCE == "merge_request_event"'
    - changes:
        - src/**/*
  # OR using legacy syntax
  only:
    - main
    - merge_requests
  except:
    - tags
```

### GitHub Actions
```yaml
job:
  if: |
    github.ref == 'refs/heads/main' ||
    github.event_name == 'pull_request'
  runs-on: ubuntu-latest
  steps:
    - uses: actions/checkout@v4
    - run: echo "Running"
```

For path filtering:
```yaml
on:
  push:
    branches: [ main ]
    paths:
      - 'src/**'
```

## Matrix Builds

### GitLab CI/CD
```yaml
test:
  parallel:
    matrix:
      - DOTNET_VERSION: ['8.0', '9.0', '10.0']
  script:
    - dotnet test --framework net$DOTNET_VERSION
```

### GitHub Actions
```yaml
test:
  strategy:
    matrix:
      dotnet-version: ['8.0', '9.0', '10.0']
  runs-on: ubuntu-latest
  steps:
    - uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ matrix.dotnet-version }}
    - run: dotnet test
```

## Migration Checklist

- [ ] Move `.gitlab-ci.yml` logic to `.github/workflows/`
- [ ] Convert GitLab variables to GitHub secrets
- [ ] Update `$CI_*` variables to `${{ github.* }}`
- [ ] Change `script:` to `steps:` with `run:`
- [ ] Convert `only:`/`except:` to `if:` conditions
- [ ] Replace `extends:` with reusable workflows if needed
- [ ] Update Docker registry from GitLab to GitHub (`ghcr.io`)
- [ ] Configure environment protection rules
- [ ] Set up branch protection rules
- [ ] Test SSH access from GitHub Actions runners
- [ ] Update documentation with new workflow triggers

## Quick Reference Card

```
# Access secrets
GitLab:  $MY_SECRET  or  ${{ env.MY_SECRET }}
GitHub:  ${{ secrets.MY_SECRET }}

# Access commit SHA
GitLab:  $CI_COMMIT_SHA
GitHub:  ${{ github.sha }}

# Access branch name
GitLab:  $CI_COMMIT_REF_NAME
GitHub:  ${{ github.ref_name }}

# Run on specific branch
GitLab:  only: [ main ]
GitHub:  if: github.ref == 'refs/heads/main'

# Manual trigger
GitLab:  when: manual
GitHub:  workflow_dispatch (in 'on:' section)

# Job dependencies
GitLab:  stage: deploy (stages run in order)
GitHub:  needs: [build, test]

# Docker registry
GitLab:  $CI_REGISTRY/group/project
GitHub:  ghcr.io/username/repository
```
