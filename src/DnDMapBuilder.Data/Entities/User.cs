namespace DnDMapBuilder.Data.Entities;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "user"; // admin or user
    public string Status { get; set; } = "pending"; // pending, approved, rejected
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The OAuth provider used for authentication (null for email/password users)
    /// Values: "google", "apple", null
    /// </summary>
    public string? OAuthProvider { get; set; }

    /// <summary>
    /// The unique identifier from the OAuth provider
    /// </summary>
    public string? OAuthProviderId { get; set; }

    /// <summary>
    /// URL to the user's profile picture from OAuth provider
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// Whether the email has been verified by the OAuth provider
    /// </summary>
    public bool EmailVerified { get; set; }

    // Navigation properties
    public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
    public ICollection<TokenDefinition> TokenDefinitions { get; set; } = new List<TokenDefinition>();
}
