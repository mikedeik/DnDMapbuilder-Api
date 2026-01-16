# Security Configuration

## Admin User Setup

The application seeds a default admin user on first run. This user is required to approve new user registrations.

### Environment Variables

Configure these environment variables to set secure admin credentials:

| Variable | Description | Required | Example |
|----------|-------------|----------|---------|
| `ADMIN_DEFAULT_PASSWORD` | Admin user password | Yes (Production) | Use a strong password |
| `ADMIN_EMAIL` | Admin user email | Yes (Production) | `your-email@domain.com` |

### GitHub Secrets Configuration

Add these secrets to your backend repository (`Settings → Secrets and variables → Actions → Secrets`):

```
ADMIN_DEFAULT_PASSWORD: YourVerySecurePassword123!@#
ADMIN_EMAIL: your-admin@yourdomain.com
```

### Security Best Practices

#### 1. Never Use Default Credentials in Production

**Bad (Development Only):**
```bash
ADMIN_DEFAULT_PASSWORD=Admin123!
ADMIN_EMAIL=admin@dndmapbuilder.com
```

**Good (Production):**
```bash
ADMIN_DEFAULT_PASSWORD=X9$mK#pL2@qR8vN4zT7hW!eA6fG5yU
ADMIN_EMAIL=your-real-email@yourdomain.com
```

#### 2. Generate Strong Passwords

Use a password generator with at least:
- 20+ characters
- Mix of uppercase, lowercase, numbers, and symbols
- No dictionary words

Example generation:
```bash
# On Linux/Mac
openssl rand -base64 32

# Or use a password manager like:
# - 1Password
# - Bitwarden
# - LastPass
```

#### 3. Rotate Credentials Regularly

- Change admin password every 90 days
- Update the GitHub secret and redeploy
- The new password takes effect on next deployment

#### 4. Database Migrations

When you first deploy or change credentials:

**Development:**
```bash
# Use default credentials for local testing
dotnet ef database update
```

**Production:**
```bash
# Credentials are read from environment variables during container startup
# The seed data runs automatically on first database creation
```

#### 5. Initial Login

After deployment, login with your configured credentials:

**Email:** The value you set in `ADMIN_EMAIL`
**Password:** The value you set in `ADMIN_DEFAULT_PASSWORD`

**Important:** Change the password through the application UI immediately after first login (when this feature is implemented).

## Additional Security Measures

### JWT Configuration

Ensure your JWT secrets are secure:

```bash
# Generate a secure JWT secret:
openssl rand -base64 64
```

Add to GitHub Secrets:
- `JWT_SECRET`: Your generated secret
- `JWT_ISSUER`: Your domain (e.g., `dndmaps-api.hostname.gr`)
- `JWT_AUDIENCE`: Your frontend domain (e.g., `dndmaps.hostname.gr`)

### Database Connection

Use secure database credentials:
- Strong database password
- Restrict database access to your application server IP only
- Use SSL/TLS for database connections if possible

### Deployment Checklist

Before deploying to production, verify:

- [ ] `ADMIN_DEFAULT_PASSWORD` is set to a strong, unique password
- [ ] `ADMIN_EMAIL` is set to a real email you control
- [ ] `JWT_SECRET` is a secure random string (64+ characters)
- [ ] `DB_CONNECTION_STRING` uses a strong database password
- [ ] All secrets are stored in GitHub Secrets, not in code
- [ ] The repository `.env` files are in `.gitignore`
- [ ] No default credentials ("Admin123!") are used in production

## Troubleshooting

### Can't login with admin credentials

1. Check the container logs:
```bash
docker logs dnd-api --tail 100
```

2. Verify environment variables are set:
```bash
docker exec dnd-api printenv | grep ADMIN
```

3. Check the database has the admin user:
```bash
# Connect to your database and check Users table
SELECT Username, Email, Role, Status FROM Users WHERE Role = 'admin';
```

### Need to reset admin password

1. Update the `ADMIN_DEFAULT_PASSWORD` secret in GitHub
2. Drop and recreate the database (this will lose all data):
```bash
# SSH into your server
docker exec dnd-api dotnet ef database drop --force
docker restart dnd-api
```

Or manually update the password hash in the database:
```bash
# Generate new hash locally using BCrypt
# Then update the database
UPDATE Users SET PasswordHash = 'your-new-hash' WHERE Role = 'admin';
```

## Contact

For security issues, please contact the repository maintainer directly. Do not open public issues for security vulnerabilities.
