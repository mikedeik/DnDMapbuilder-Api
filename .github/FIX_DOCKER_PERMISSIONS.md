# Fixing Docker Permission Issues on Your Server

## The Problem

You're seeing this error:
```
permission denied while trying to connect to the Docker daemon socket at unix:///var/run/docker.sock
```

This happens because your SSH user doesn't have permission to run Docker commands.

## Solution: Add User to Docker Group

SSH into your server and run these commands:

### Step 1: Add your user to the docker group
```bash
sudo usermod -aG docker $USER
```

If you're deploying with a specific user (e.g., `ubuntu`), replace `$USER`:
```bash
sudo usermod -aG docker ubuntu
```

### Step 2: Apply the group changes

You have two options:

**Option A: Log out and log back in** (Recommended)
```bash
exit
# Then SSH back in
```

**Option B: Activate the group without logging out**
```bash
newgrp docker
```

### Step 3: Verify it works
```bash
docker ps
```

If this runs without `sudo` and without errors, you're all set!

### Step 4: Test the full workflow
```bash
# Test login to GitHub Container Registry
echo "YOUR_GITHUB_PAT" | docker login ghcr.io -u YOUR_GITHUB_USERNAME --password-stdin

# Test pulling an image
docker pull hello-world

# Test running a container
docker run hello-world
```

## Alternative Solution: Use Sudo (Not Recommended)

If you can't add the user to the docker group, you can modify the workflow to use `sudo`. However, this requires configuring passwordless sudo for Docker commands.

### Configure passwordless sudo for Docker

1. SSH into your server
2. Edit the sudoers file:
```bash
sudo visudo
```

3. Add this line at the end (replace `ubuntu` with your username):
```bash
ubuntu ALL=(ALL) NOPASSWD: /usr/bin/docker
```

4. Save and exit (Ctrl+X, then Y, then Enter)

5. Update the workflow to use `sudo`:

In `.github/workflows/ci-cd.yml`, replace all `docker` commands with `sudo docker`:
```bash
sudo docker login ...
sudo docker pull ...
sudo docker stop ...
sudo docker rm ...
sudo docker run ...
```

## Checking Current Docker Group Membership

To see if your user is already in the docker group:
```bash
groups $USER
```

Or for a specific user:
```bash
groups ubuntu
```

You should see `docker` in the output.

## Understanding Docker Socket Permissions

The Docker daemon runs as root and listens on `/var/run/docker.sock`. By default, only root and members of the `docker` group can access this socket.

To check socket permissions:
```bash
ls -l /var/run/docker.sock
```

Output should look like:
```
srw-rw---- 1 root docker 0 Jan 15 21:00 /var/run/docker.sock
```

The `docker` in the group column means users in the docker group can access it.

## Security Note

Adding a user to the docker group gives them root-equivalent privileges because Docker containers can be run with root access. Only add trusted users to the docker group.

For production environments, consider:
- Using a dedicated deployment user with minimal privileges
- Implementing proper secret rotation
- Using Docker's user namespaces for additional isolation
- Auditing Docker commands via logging

## Testing Your Fix

After adding your user to the docker group and logging back in, run:

```bash
# Should work without sudo
docker ps

# Should work without sudo
docker images

# Should work without sudo
docker pull alpine
```

If all three commands work without errors, your permissions are fixed!

## Still Having Issues?

### Issue: "Cannot connect to the Docker daemon"
**Solution**: Make sure Docker is running
```bash
sudo systemctl status docker
sudo systemctl start docker
sudo systemctl enable docker  # Enable on boot
```

### Issue: "credential helper" warning
This warning is harmless but can be fixed by installing a credential helper:
```bash
# For Ubuntu/Debian
sudo apt-get install pass gnupg2

# Configure Docker to use it
docker-credential-pass
```

Or ignore it - it won't affect deployment.

### Issue: "network dnd-network not found"
Create the Docker network first:
```bash
docker network create dnd-network
```

Or update the workflow to create it automatically:
```bash
docker network create dnd-network || true
```
