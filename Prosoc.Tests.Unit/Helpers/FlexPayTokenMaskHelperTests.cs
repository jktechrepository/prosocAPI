using ProsocAPI.Helpers;

namespace Prosoc.Tests.Unit.Helpers;

public class FlexPayTokenMaskHelperTests
{
    [Fact]
    public void Mask_RetourneVideSiNull()
    {
        Assert.Equal(string.Empty, FlexPayTokenMaskHelper.Mask(null));
    }

    [Fact]
    public void Mask_MasqueTokenLong()
    {
        var masked = FlexPayTokenMaskHelper.Mask("abcdefghijklmnop");
        Assert.EndsWith("mnop", masked);
        Assert.Contains('*', masked);
        Assert.DoesNotContain("abcdef", masked);
    }

    [Fact]
    public void Mask_TokenCourt_RetourneEtoiles()
    {
        Assert.Equal("****", FlexPayTokenMaskHelper.Mask("abc"));
    }
}
