using ProsocAPI.Models.Core;

namespace Prosoc.Tests.Unit.Models;

public class AdhesionStatutDossierReglesTests
{
    [Theory]
    [InlineData(null, AdhesionStatutDossierRegles.EnAttente)]
    [InlineData("", AdhesionStatutDossierRegles.EnAttente)]
    [InlineData("  ", AdhesionStatutDossierRegles.EnAttente)]
    [InlineData("EN ATTENTE", AdhesionStatutDossierRegles.EnAttente)]
    [InlineData("En Attente", AdhesionStatutDossierRegles.EnAttente)]
    [InlineData("en attente", AdhesionStatutDossierRegles.EnAttente)]
    [InlineData("A", AdhesionStatutDossierRegles.EnAttente)]
    [InlineData("B", AdhesionStatutDossierRegles.EnAttente)]
    [InlineData("INCONNU", AdhesionStatutDossierRegles.EnAttente)]
    [InlineData("VALIDÉ", AdhesionStatutDossierRegles.Valide)]
    [InlineData("VALIDE", AdhesionStatutDossierRegles.Valide)]
    [InlineData("Validé", AdhesionStatutDossierRegles.Valide)]
    [InlineData("COMPLET", AdhesionStatutDossierRegles.Valide)]
    [InlineData("Complet", AdhesionStatutDossierRegles.Valide)]
    [InlineData("complet", AdhesionStatutDossierRegles.Valide)]
    public void Normaliser_MappeVersCanons(string? input, string expected)
    {
        Assert.Equal(expected, AdhesionStatutDossierRegles.Normaliser(input));
    }

    [Fact]
    public void EstEnAttente_AccepteVariantesCasse()
    {
        Assert.True(AdhesionStatutDossierRegles.EstEnAttente("En Attente"));
        Assert.False(AdhesionStatutDossierRegles.EstEnAttente("COMPLET"));
    }

    [Fact]
    public void EstValide_AccepteLegacyCompletEtValide()
    {
        Assert.True(AdhesionStatutDossierRegles.EstValide("COMPLET"));
        Assert.True(AdhesionStatutDossierRegles.EstValide("VALIDE"));
        Assert.True(AdhesionStatutDossierRegles.EstValide("VALIDÉ"));
        Assert.False(AdhesionStatutDossierRegles.EstValide("EN ATTENTE"));
    }
}
