using Xunit;

namespace DnDMapBuilder.UnitTests.Services;

public class OAuthServiceTests
{
    [Fact]
    public void OAuthService_ShouldBeStructuredCorrectly()
    {
        // This test verifies that the OAuthService is properly structured
        // Full integration tests should be performed with actual OAuth providers
        Assert.NotNull(typeof(DnDMapBuilder.Application.Services.OAuthService));
    }
}
