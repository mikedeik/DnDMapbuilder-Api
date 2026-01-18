namespace DnDMapBuilder.Application.Interfaces;

/// <summary>
/// Service interface for password hashing and verification operations.
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hashes a plain text password using BCrypt.
    /// </summary>
    /// <param name="password">The plain text password to hash</param>
    /// <returns>The hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a plain text password against a hash.
    /// </summary>
    /// <param name="password">The plain text password to verify</param>
    /// <param name="hash">The hashed password to compare against</param>
    /// <returns>True if password matches hash, false otherwise</returns>
    bool VerifyPassword(string password, string hash);
}
