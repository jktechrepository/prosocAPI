using ProsocAPI.Helpers;

namespace Prosoc.Tests.Unit.Helpers;

public class FlexPayUrlHelperTests
{
    [Fact]
    public void DeriveRedirectUrl_RemplaceCallbackParAction()
    {
        var url = FlexPayUrlHelper.DeriveRedirectUrl(
            "https://api.example.com/api/FlexPay/callback", "approve");
        Assert.Equal("https://api.example.com/api/FlexPay/approve", url);
    }

    [Fact]
    public void ResolveCallbackUrl_UtiliseConfigEnLocalhost()
    {
        var url = FlexPayUrlHelper.ResolveCallbackUrl(
            null,
            "https://prod.example.com/api/FlexPay/callback",
            forceProductionCallbackInDev: false);
        Assert.Equal("https://prod.example.com/api/FlexPay/callback", url);
    }
}
