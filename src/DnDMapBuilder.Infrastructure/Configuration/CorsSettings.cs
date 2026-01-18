namespace DnDMapBuilder.Infrastructure.Configuration;

/// <summary>
/// CORS configuration settings.
/// </summary>
public class CorsSettings
{
    /// <summary>
    /// Configuration section name for CORS settings.
    /// </summary>
    public const string SectionName = "CorsSettings";

    /// <summary>
    /// Gets or sets the allowed origins for CORS requests.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}
