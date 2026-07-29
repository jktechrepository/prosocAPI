using ProsocAPI.Services;
using Xunit;

namespace Prosoc.Tests.Unit.Services;

public class DemandeRechargeWalletVirtuelServiceTests
{
    [Theory]
    [InlineData(100, 40, 60)]
    [InlineData(100, 100, 0)]
    [InlineData(100, 120, -20)]
    [InlineData(50.5, 20.25, 30.25)]
    public void CalculerMontantRecharge_ReturnsExpected(decimal plafond, decimal solde, decimal expected)
    {
        var montant = DemandeRechargeWalletVirtuelService.CalculerMontantRecharge(plafond, solde);
        Assert.Equal(expected, montant);
    }
}
