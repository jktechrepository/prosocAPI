using ProsocAPI.Models.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class ProduitTarifRulesTests
{
    [Fact]
    public void ValidateTrancheAge_AgeMaxInferieurAgeMin_LeveException()
    {
        Assert.Throws<ArgumentException>(() => ProduitTarifRules.ValidateTrancheAge(18, 5));
    }

    [Theory]
    [InlineData("annuel", "Annuel")]
    [InlineData("trimestriel", "Trimestriel")]
    [InlineData("SEMESTRIEL", "Semestriel")]
    [InlineData("mensuel", "Mensuel")]
    public void NormalizePeriodicite_ValeursValides_RetourneValeurCanonique(string input, string expected)
    {
        var result = ProduitTarifRules.NormalizePeriodicite(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizePeriodicite_Invalide_LeveException()
    {
        Assert.Throws<ArgumentException>(() => ProduitTarifRules.NormalizePeriodicite("Hebdomadaire"));
    }

    [Fact]
    public void ValidateTrancheAge_ZeroA18_Accepte()
    {
        var ex = Record.Exception(() => ProduitTarifRules.ValidateTrancheAge(0, 18));
        Assert.Null(ex);
    }

    [Fact]
    public void ValidateTauxCommission_SuperieurA100_LeveException()
    {
        Assert.Throws<ArgumentException>(() => ProduitTarifRules.ValidateTauxCommission(101m, "TauxCommissionAT"));
    }

    [Fact]
    public void ValidateTauxCommission_15_Accepte()
    {
        var ex = Record.Exception(() => ProduitTarifRules.ValidateTauxCommission(15m, "TauxCommissionAT"));
        Assert.Null(ex);
    }

    [Fact]
    public void ValidateGratuitPayant_GratuitAvecMontant_LeveException()
    {
        var produit = new ProduitMutuel { EstGratuit = true, Montant = 10m };
        Assert.Throws<ArgumentException>(() => ProduitTarifRules.ValidateGratuitPayant(produit));
    }

    [Fact]
    public void ValidateGratuitPayant_Gratuit_RemetTauxAZero()
    {
        var produit = new ProduitMutuel
        {
            EstGratuit = true,
            Montant = 0m,
            TauxCommissionAT = 15m,
            TauxCommissionAA = 5m
        };
        ProduitTarifRules.ValidateGratuitPayant(produit);
        Assert.Equal(0m, produit.TauxCommissionAT);
        Assert.Equal(0m, produit.TauxCommissionAA);
    }

    [Fact]
    public void ValidateGratuitPayant_Payant_MontantZero_LeveException()
    {
        var produit = new ProduitMutuel { EstGratuit = false, Montant = 0m };
        Assert.Throws<ArgumentException>(() => ProduitTarifRules.ValidateGratuitPayant(produit));
    }
}
