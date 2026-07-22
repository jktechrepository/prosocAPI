using ProsocAPI.Models.Core;

namespace Prosoc.Tests.Unit.Models;

public class PeriodicitePrestationReglesTests
{
    [Theory]
    [InlineData("mensuel", "Mensuel")]
    [InlineData("Trimestriel", "Trimestriel")]
    [InlineData(" semestriel ", "Semestriel")]
    [InlineData("ANNUEL", "Annuel")]
    public void Normaliser_ValeurValide_RetourneValeurCanonique(string input, string expected)
    {
        var result = PeriodicitePrestationRegles.Normaliser(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Normaliser_NullUtiliseFallbackMensuel()
    {
        var result = PeriodicitePrestationRegles.Normaliser(null);
        Assert.Equal("Mensuel", result);
    }

    [Fact]
    public void Normaliser_ValeurInvalide_LeveException()
    {
        var ex = Assert.Throws<ArgumentException>(() => PeriodicitePrestationRegles.Normaliser("Hebdomadaire"));
        Assert.Contains("Valeurs acceptées", ex.Message);
    }
}
