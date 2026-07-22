using ProsocAPI.Models.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class ProduitSouscriptionRulesTests
{
    [Fact]
    public void ValidateMontantCollecte_Gratuit_MontantNonZero_LeveException()
    {
        var produit = new ProduitMutuel { EstGratuit = true, Montant = 0m };
        Assert.Throws<ArgumentException>(() =>
            ProduitSouscriptionRules.ValidateMontantCollecteSouscription(50m, produit));
    }

    [Fact]
    public void ValidateMontantCollecte_Gratuit_MontantZero_Accepte()
    {
        var produit = new ProduitMutuel { EstGratuit = true, Montant = 0m };
        var ex = Record.Exception(() =>
            ProduitSouscriptionRules.ValidateMontantCollecteSouscription(0m, produit));
        Assert.Null(ex);
    }

    [Fact]
    public void ValidateMontantCollecte_Payant_MauvaisMontant_LeveException()
    {
        var produit = new ProduitMutuel { EstGratuit = false, Montant = 100m };
        Assert.Throws<ArgumentException>(() =>
            ProduitSouscriptionRules.ValidateMontantCollecteSouscription(50m, produit));
    }

    [Fact]
    public void ValidateMontantCollecte_Payant_MontantExact_Accepte()
    {
        var produit = new ProduitAssureur { EstGratuit = false, Montant = 200m };
        var ex = Record.Exception(() =>
            ProduitSouscriptionRules.ValidateMontantCollecteSouscription(200m, produit));
        Assert.Null(ex);
    }
}
