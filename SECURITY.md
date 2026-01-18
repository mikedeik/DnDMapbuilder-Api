# Security Policy and Procedures

## Overview

This document outlines the security measures, best practices, and incident response procedures for the DnDMapBuilder backend API.

## Security Features

### Authentication

- **JWT (JSON Web Tokens)** for stateless authentication
  - Configurable secret key (via environment or Key Vault)
  - HS256 signing algorithm
  - Configurable expiration (default: 24 hours)
  - Token validation includes issuer, audience, and signature checks
  - Location: `src/DnDMapBuilder.Infrastructure/Security/JwtService.cs`

### Authorization

- **Role-Based Access Control (RBAC)**
  - User role: Standard user with personal resource access
  - Admin role: Administrative operations (user approval, etc.)
  - Authorization enforced via `[Authorize]` and `[Authorize(Roles = "admin")]` attributes
  - Per-resource ownership verification for multi-tenant scenarios

### Password Security

- **BCrypt hashing** with configurable work factor
- Passwords are hashed before storage
- Password verification without storing plaintext
- Unit tests verify hash security and uniqueness
- Service: `src/DnDMapBuilder.Application/Services/PasswordService.cs`

## Data Protection

### HTTPS/TLS

- HTTPS enforcement via middleware: `app.UseHttpsRedirection()`
- All API communications encrypted in transit
- TLS 1.2+ enforced by ASP.NET Core defaults

### Secrets Management

#### Development Environment
```bash
# Set up local secrets (not in source control)
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "your-dev-secret-key"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-dev-connection-string"
```

#### Production Environment
- **Recommended:** Azure Key Vault or HashiCorp Vault
- Environment variables for secrets:
  - `JwtSettings__SecretKey`
  - `ConnectionStrings__DefaultConnection`
  - `Database__ConnectionString`

### SQL Injection Prevention

- **Entity Framework Core** with parameterized queries
- All LINQ queries use parameter placeholders
- No string concatenation or raw SQL for user-provided data

### XSS Protection

- **Security Headers Middleware** configured:
  - X-Content-Type-Options: nosniff
  - X-Frame-Options: DENY
  - X-XSS-Protection: 1; mode=block
  - Strict-Transport-Security for HTTPS enforcement
  - Content-Security-Policy: default-src 'self'

### CORS (Cross-Origin Resource Sharing)

- Whitelist-based CORS configuration (not AllowAll)
- Configurable allowed origins
- Credentials support enabled for authenticated requests

## API Security

### Rate Limiting

- Anonymous users: 100 requests/minute
- Authenticated users: 300 requests/minute
- File uploads: 10 requests/minute
- Returns 429 (Too Many Requests) with Retry-After header

### File Upload Security

- File size limits: Maps (5MB), Tokens (2MB), Default (10MB)
- MIME type validation: image/png, image/jpeg, image/webp
- Storage isolation and directory traversal protection
- Comprehensive validation service with unit tests

## Logging and Monitoring

### Structured Logging

- Serilog for structured, machine-readable logs
- Correlation IDs for request tracing
- Sensitive data filtering to prevent leakage
- Environment-specific log levels (Debug in dev, Warning in prod)

### Request/Response Logging

- RequestResponseLoggingMiddleware logs requests, responses, and duration
- User identity tracking for authenticated requests
- Correlation ID propagation
- Excludes health check endpoints

### OpenTelemetry Tracing

- Distributed tracing for request flow
- Custom metrics for security events
- OTLP exporter for centralized collection

## OWASP Top 10 Compliance

| Vulnerability | Status | Details |
|---|---|---|
| A01: Broken Access Control | ✓ | Authorization checks; resource ownership verified |
| A02: Cryptographic Failures | ✓ | HTTPS enforced; BCrypt hashing; no hardcoded secrets |
| A03: Injection | ✓ | EF Core parameterized queries; no command injection |
| A04: Insecure Design | ✓ | Security headers; rate limiting; authentication required |
| A05: Security Misconfiguration | ✓ | Security headers; proper CORS; reduced SQL logging |
| A06: Vulnerable Components | ✓ | No vulnerable packages; .NET 10.0 latest |
| A07: Authentication Failures | ✓ | JWT; BCrypt; user status verification |
| A08: Data Integrity | ✓ | Secure CI/CD; code review required |
| A09: Logging & Monitoring | ✓ | Serilog; request logging; OpenTelemetry tracing |
| A10: SSRF | ✓ | No user-controlled URLs; no untrusted outbound requests |

## Dependency Vulnerability Status

**Result:** No vulnerable packages identified
- All NuGet packages up-to-date
- Regular automated scanning recommended
- .NET 10.0 runtime (latest stable)

## Best Practices for Deployment

### Pre-Deployment Checklist

- [ ] All tests passing (unit, integration, architecture)
- [ ] No vulnerable dependencies (`dotnet list package --vulnerable`)
- [ ] Secrets configured in Key Vault (not in code)
- [ ] CORS origins configured for production domain
- [ ] Security headers configured and tested
- [ ] Database migrations tested
- [ ] Health checks verified
- [ ] Monitoring and alerting configured

### Security Update Frequency

- Critical: Immediate (within 24 hours)
- High: Within 1 week
- Medium: Within 2 weeks
- Low: Next scheduled release

## Incident Response

### Reporting Security Issues

DO NOT create public GitHub issues for security vulnerabilities.

Contact project maintainers directly with:
- Detailed vulnerability description
- Reproduction steps
- Severity assessment
- Allow 30 days for patch development

### Response Process

1. Acknowledge receipt within 24 hours
2. Assess severity and impact
3. Develop and test patch
4. Release security update with advisory
5. Notify affected users

## Regular Security Activities

- **Daily:** Monitor logs for errors; check health endpoints
- **Weekly:** Review auth logs; check dependency notifications
- **Monthly:** Security log analysis; vulnerability scanning
- **Quarterly:** Full security audit; penetration testing prep
- **Annually:** Third-party pen testing; compliance verification

## References

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security](https://learn.microsoft.com/aspnet/core/security)
- [JWT Best Practices](https://tools.ietf.org/html/rfc7519)
