namespace DnDMapBuilder.Application.Interfaces;

/// <summary>
/// Service interface for JWT token generation and validation.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="userId">The user ID to include in the token</param>
    /// <param name="email">The user's email to include in the token</param>
    /// <param name="role">The user's role to include in the token</param>
    /// <returns>The generated JWT token</returns>
    string GenerateToken(string userId, string email, string role);

    /// <summary>
    /// Validates a JWT token and returns the user ID if valid.
    /// </summary>
    /// <param name="token">The JWT token to validate</param>
    /// <returns>The user ID if token is valid, null if invalid or expired</returns>
    string? ValidateToken(string token);
}
