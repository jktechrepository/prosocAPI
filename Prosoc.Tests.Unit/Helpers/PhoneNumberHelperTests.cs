using ProsocAPI.Helpers;

namespace Prosoc.Tests.Unit.Helpers;

public class PhoneNumberHelperTests
{
    [Theory]
    [InlineData("0812345678", "+243812345678")]
    [InlineData("+243812345678", "+243812345678")]
    [InlineData("243812345678", "+243812345678")]
    [InlineData("+243 81 234 56 78", "+243812345678")]
    [InlineData("081-234-56-78", "+243812345678")]
    [InlineData("0977123456", "+243977123456")]
    public void NormalizeForStorage_ConvertitVersFormatInternational(string input, string expected)
    {
        Assert.Equal(expected, PhoneNumberHelper.NormalizeForStorage(input));
    }

    [Theory]
    [InlineData("089111111")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    public void NormalizeForStorage_RetourneNullSiInvalide(string input)
    {
        Assert.Null(PhoneNumberHelper.NormalizeForStorage(input));
    }

    [Theory]
    [InlineData("0812345678", true)]
    [InlineData("+243812345678", true)]
    [InlineData("+243 81 234 56 78", true)]
    [InlineData("089111111", false)]
    [InlineData("admin@prosoc.cd", false)]
    public void IsValidPhone_ValideLesFormats(string input, bool expected)
    {
        Assert.Equal(expected, PhoneNumberHelper.IsValidPhone(input));
    }

    [Fact]
    public void GetLookupVariants_InclutFormatsLocalEtInternational()
    {
        var variants = PhoneNumberHelper.GetLookupVariants("0812345678");

        Assert.Contains("+243812345678", variants);
        Assert.Contains("0812345678", variants);
        Assert.Contains("243812345678", variants);
    }

    [Fact]
    public void GetLookupVariants_LieEntre0EtPlus243()
    {
        var variants = PhoneNumberHelper.GetLookupVariants("+243891111111");

        Assert.Contains("+243891111111", variants);
        Assert.Contains("0891111111", variants);
    }
}
