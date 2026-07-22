using System.Security.Claims;
using ProsocAPI.Helpers;

namespace Prosoc.Tests.Unit.Helpers;

public class WalletVirtuelPaiementAutorisationTests
{
    [Theory]
    [InlineData("Agent (AT)")]
    [InlineData("Chef d'équipe")]
    [InlineData("Superviseur")]
    [InlineData("Percepteur")]
    public void CallerPeutPayerEnWalletVirtuel_RolesWhitelist_True(string role)
    {
        Assert.True(WalletVirtuelPaiementAutorisation.CallerPeutPayerEnWalletVirtuel(
            PrincipalWithRoles(role)));
    }

    [Theory]
    [InlineData("Caissier")]
    [InlineData("Admin")]
    [InlineData("Financier")]
    [InlineData("Agent (AA)")]
    [InlineData("Affilié")]
    [InlineData("SuperAdmin")]
    public void CallerPeutPayerEnWalletVirtuel_RolesHorsWhitelist_False(string role)
    {
        Assert.False(WalletVirtuelPaiementAutorisation.CallerPeutPayerEnWalletVirtuel(
            PrincipalWithRoles(role)));
    }

    [Fact]
    public void CallerPeutPayerEnWalletVirtuel_MultiRoles_PercepteurEtAdmin_True()
    {
        Assert.True(WalletVirtuelPaiementAutorisation.CallerPeutPayerEnWalletVirtuel(
            PrincipalWithRoles("Percepteur", "Admin")));
    }

    [Fact]
    public void CallerPeutPayerEnWalletVirtuel_NullOuNonAuth_False()
    {
        Assert.False(WalletVirtuelPaiementAutorisation.CallerPeutPayerEnWalletVirtuel(null));
        Assert.False(WalletVirtuelPaiementAutorisation.CallerPeutPayerEnWalletVirtuel(
            new ClaimsPrincipal(new ClaimsIdentity())));
    }

    [Fact]
    public void EnsureCallerPeutPayerEnWalletVirtuel_Caissier_LeveMessageSupport()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            WalletVirtuelPaiementAutorisation.EnsureCallerPeutPayerEnWalletVirtuel(
                PrincipalWithRoles("Caissier")));

        Assert.Equal(WalletVirtuelPaiementAutorisation.MessageNonAutorise, ex.Message);
    }

    [Fact]
    public void EnsureSiVirtualAccount_Espece_IgnoreRole()
    {
        var ex = Record.Exception(() =>
            WalletVirtuelPaiementAutorisation.EnsureSiVirtualAccount(
                "ESPECE", PrincipalWithRoles("Caissier")));

        Assert.Null(ex);
    }

    [Fact]
    public void EnsureSiVirtualAccount_VirtualAccount_Caissier_Refuse()
    {
        Assert.Throws<ArgumentException>(() =>
            WalletVirtuelPaiementAutorisation.EnsureSiVirtualAccount(
                "VIRTUAL_ACCOUNT", PrincipalWithRoles("Caissier")));
    }

    private static ClaimsPrincipal PrincipalWithRoles(params string[] roles)
    {
        var claims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
