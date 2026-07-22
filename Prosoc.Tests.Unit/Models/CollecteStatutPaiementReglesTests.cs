using ProsocAPI.Models.Core;

namespace Prosoc.Tests.Unit.Models;

public class CollecteStatutPaiementReglesTests
{
    [Theory]
    [InlineData("VALIDE")]
    [InlineData("valide")]
    [InlineData("Validé")]
    [InlineData("OK")]
    [InlineData("PAYE")]
    [InlineData("Payé")]
    [InlineData("CONFIRME")]
    public void EstValide_ReconnaitCanoniqueEtLegacy(string statut)
    {
        Assert.True(CollecteStatutPaiementRegles.EstValide(statut));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("EN_ATTENTE")]
    [InlineData("ECHEC")]
    public void EstValide_RejetteNonPaye(string? statut)
    {
        Assert.False(CollecteStatutPaiementRegles.EstValide(statut));
    }

    [Theory]
    [InlineData("EN_ATTENTE")]
    [InlineData("en_attente")]
    public void EstEnAttente_ReconnaitFlexPayEnCours(string statut)
    {
        Assert.True(CollecteStatutPaiementRegles.EstEnAttente(statut));
    }

    [Fact]
    public void NormaliserPourEcriture_GuichetVersValide()
    {
        Assert.Equal(CollecteStatutPaiement.Valide, CollecteStatutPaiementRegles.NormaliserPourEcriture("PAYE"));
        Assert.Equal(CollecteStatutPaiement.Valide, CollecteStatutPaiementRegles.NormaliserPourEcriture(null));
        Assert.Equal(CollecteStatutPaiement.EnAttente, CollecteStatutPaiementRegles.NormaliserPourEcriture("EN_ATTENTE"));
    }

    [Fact]
    public void EstEnAttente_NeConfondPasAvecValide()
    {
        Assert.False(CollecteStatutPaiementRegles.EstEnAttente("VALIDE"));
        Assert.False(CollecteStatutPaiementRegles.EstEnAttente("PAYE"));
    }
}
