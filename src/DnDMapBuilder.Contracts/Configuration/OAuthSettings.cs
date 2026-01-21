namespace DnDMapBuilder.Contracts.Configuration;

public class OAuthSettings
{
    public GoogleSettings Google { get; set; } = new();
    public AppleSettings Apple { get; set; } = new();
    public string RedirectUri { get; set; } = string.Empty;
    public string FrontendRedirectUri { get; set; } = string.Empty;
}

public class GoogleSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class AppleSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string TeamId { get; set; } = string.Empty;
    public string KeyId { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
}
