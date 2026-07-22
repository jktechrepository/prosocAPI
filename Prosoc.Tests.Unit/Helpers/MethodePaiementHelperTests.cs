using ProsocAPI.Helpers;

namespace Prosoc.Tests.Unit.Helpers;

public class MethodePaiementHelperTests
{
    [Theory]
    [InlineData("MOBILE_MONEY", true)]
    [InlineData("mobile_money", true)]
    [InlineData("ORANGE_MONEY", true)]
    [InlineData("CARTE_BANCAIRE", true)]
    [InlineData("CARTE", true)]
    [InlineData("ESPECE", false)]
    [InlineData("VIRTUAL_ACCOUNT", false)]
    public void IsFlexPay_DetecteLesModes(string mode, bool expected)
    {
        Assert.Equal(expected, MethodePaiementHelper.IsFlexPay(mode));
    }

    [Theory]
    [InlineData("ESPECE", true)]
    [InlineData("CHEQUE", true)]
    [InlineData("VIREMENT_BANCAIRE", true)]
    [InlineData("VIRTUAL_ACCOUNT", true)]
    [InlineData("Compte Virtuel", true)]
    [InlineData("MOBILE_MONEY", false)]
    public void IsGuichetSync_DetecteLesModes(string mode, bool expected)
    {
        Assert.Equal(expected, MethodePaiementHelper.IsGuichetSync(mode));
    }

    [Theory]
    [InlineData("ESPECE", true)]
    [InlineData("MOBILE_MONEY", true)]
    [InlineData("CARTE_BANCAIRE", true)]
    [InlineData("CHEQUE", false)]
    [InlineData("VIREMENT_BANCAIRE", false)]
    [InlineData("VIRTUAL_ACCOUNT", false)]
    public void IsEntreeCaisseEligible_DetecteEspeceEtElectronique(string mode, bool expected)
    {
        Assert.Equal(expected, MethodePaiementHelper.IsEntreeCaisseEligible(mode));
    }

    [Theory]
    [InlineData("ESPECE", "COLLECTE_ESPECE")]
    [InlineData("MOBILE_MONEY", "COLLECTE_ELECTRONIQUE")]
    [InlineData("CARTE", "COLLECTE_ELECTRONIQUE")]
    public void ResolveMouvementCaisseSource_RetourneLaSourceAttendue(string mode, string expected)
    {
        Assert.Equal(expected, MethodePaiementHelper.ResolveMouvementCaisseSource(mode));
    }

    [Fact]
    public void EnsureFlexPayOnly_AccepteMobileMoney()
    {
        var ex = Record.Exception(() => MethodePaiementHelper.EnsureFlexPayOnly("MOBILE_MONEY"));
        Assert.Null(ex);
    }

    [Fact]
    public void EnsureGuichetSyncOnly_RejetteMobileMoney()
    {
        Assert.Throws<InvalidOperationException>(() =>
            MethodePaiementHelper.EnsureGuichetSyncOnly("MOBILE_MONEY"));
    }

    [Theory]
    [InlineData("ORANGE_MONEY", "MOBILE_MONEY")]
    [InlineData("Compte Virtuel", "VIRTUAL_ACCOUNT")]
    [InlineData("CARTE", "CARTE_BANCAIRE")]
    public void NormalizeForStorage_ConvertitVersCodeCanonique(string input, string expected)
    {
        Assert.Equal(expected, MethodePaiementHelper.NormalizeForStorage(input));
    }
}
