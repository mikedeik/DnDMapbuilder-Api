# Multi-Platform Docker Builds

## The Problem

You encountered this error:
```
no matching manifest for linux/arm64/v8 in the manifest list entries
```

This means your Docker image was built for **AMD64 (x86_64)** architecture, but your server runs on **ARM64** architecture.

## Architecture Types

- **AMD64 (x86_64)**: Standard Intel/AMD processors (most cloud servers, desktops, laptops)
- **ARM64 (aarch64)**: ARM-based processors (Raspberry Pi, Apple Silicon M1/M2, AWS Graviton, Oracle ARM instances)

## How to Check Your Server Architecture

SSH into your server and run:
```bash
uname -m
```

Output examples:
- `x86_64` = AMD64 architecture
- `aarch64` or `arm64` = ARM64 architecture

Or check with:
```bash
dpkg --print-architecture
```

## The Solution

Build Docker images for **multiple platforms** so they work on any server type.

### What Changed in the Workflow

In `.github/workflows/main.yml`, I added the `platforms` parameter:

```yaml
- name: Build and push Docker image
  uses: docker/build-push-action@v5
  with:
    context: .
    file: ./src/DnDMapBuilder.Api/Dockerfile
    platforms: linux/amd64,linux/arm64  # ← Added this line
    push: true
    tags: ${{ steps.meta.outputs.tags }}
    labels: ${{ steps.meta.outputs.labels }}
```

This tells Docker to build images for both:
- `linux/amd64` - Standard x86_64 servers
- `linux/arm64` - ARM-based servers

## How Multi-Platform Images Work

1. **Build Time**: GitHub Actions builds the image twice (once for each architecture)
2. **Push Time**: Both images are pushed to the registry with the same tag
3. **Pull Time**: Docker automatically selects the correct architecture for your server

When you run `docker pull ghcr.io/mikedeik/dndmapbuilder-api:latest`, Docker will:
- Pull the ARM64 version on ARM servers
- Pull the AMD64 version on x86_64 servers

## Verifying Multi-Platform Support

After the next build, check your image on GitHub Container Registry:

1. Go to: https://github.com/users/mikedeik/packages/container/dndmapbuilder-api
2. Click on a tag (e.g., `latest`)
3. Look for "OS/Arch" - you should see both:
   - `linux/amd64`
   - `linux/arm64`

Or check via CLI:
```bash
docker manifest inspect ghcr.io/mikedeik/dndmapbuilder-api:latest
```

You should see two manifests listed.

## Build Time Impact

Multi-platform builds take longer because:
- Each platform is built separately
- ARM builds on AMD64 runners use QEMU emulation (slower)

Typical build times:
- Single platform (amd64): 2-5 minutes
- Multi-platform (amd64 + arm64): 5-15 minutes

## Optimizations

### Option 1: Build Only for Your Server Architecture

If you only deploy to ARM64 servers, change to:
```yaml
platforms: linux/arm64
```

If you only deploy to AMD64 servers, change to:
```yaml
platforms: linux/amd64
```

Or remove the `platforms` line entirely (defaults to AMD64).

### Option 2: Use Native Runners

For faster ARM64 builds, use native ARM64 runners:
```yaml
build-docker:
  strategy:
    matrix:
      include:
        - platform: linux/amd64
          runner: ubuntu-latest
        - platform: linux/arm64
          runner: ubuntu-24.04-arm  # GitHub's ARM runners
```

Note: GitHub's ARM runners are currently in beta and may require a paid plan.

### Option 3: Conditional Builds

Build for multiple platforms only on releases:
```yaml
- name: Set platforms
  id: platforms
  run: |
    if [[ "${{ github.ref }}" == "refs/tags/v"* ]]; then
      echo "platforms=linux/amd64,linux/arm64" >> $GITHUB_OUTPUT
    else
      echo "platforms=linux/amd64" >> $GITHUB_OUTPUT
    fi

- name: Build and push Docker image
  uses: docker/build-push-action@v5
  with:
    platforms: ${{ steps.platforms.outputs.platforms }}
    # ... rest of config
```

## Troubleshooting

### Build fails with "exec format error"

This means you're trying to run an image built for a different architecture. Solution:
- Ensure multi-platform build is enabled
- Pull the latest image after the new build completes

### "Cannot connect to Docker daemon during build"

QEMU might not be set up properly. The workflow already includes `docker/setup-buildx-action@v3` which handles this automatically.

### Slow builds

ARM64 builds on AMD64 runners are slow due to QEMU emulation. This is normal. Consider:
- Building only for your target architecture
- Using native ARM runners (if available)
- Caching layers aggressively

## Common Server Types

| Provider | Service | Architecture |
|----------|---------|--------------|
| AWS | EC2 (t3, m5, c5) | AMD64 |
| AWS | EC2 (t4g, m6g, c6g) Graviton | ARM64 |
| DigitalOcean | Standard Droplets | AMD64 |
| Oracle Cloud | VM.Standard.E2.1.Micro (Free Tier) | AMD64 |
| Oracle Cloud | VM.Standard.A1.Flex (Free Tier) | ARM64 |
| Raspberry Pi | All models | ARM64 |
| Apple Silicon | M1/M2 Macs | ARM64 |
| Google Cloud | N1, N2, E2 | AMD64 |
| Google Cloud | T2A (Tau) | ARM64 |

## Next Steps

1. Commit and push the updated workflow
2. Let the build complete (will take longer this time)
3. Retry deployment - it should now work on your ARM64 server
