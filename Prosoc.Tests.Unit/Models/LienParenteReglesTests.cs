using ProsocAPI.Models.Core;

namespace Prosoc.Tests.Unit.Models;

public class LienParenteReglesTests
{
    [Theory]
    [InlineData("CONJOINT", "CONJOINT")]
    [InlineData("conjoint", "CONJOINT")]
    [InlineData("Conjoint(e)", "CONJOINT")]
    [InlineData("Conjointe", "CONJOINT")]
    [InlineData("Épouse", "EPOUSE")]
    [InlineData("Epouse", "EPOUSE")]
    [InlineData("Enfant", "ENFANT")]
    [InlineData("Frère", "FRERE")]
    [InlineData("Frere", "FRERE")]
    [InlineData("Sœur", "SOEUR")]
    [InlineData("Soeur", "SOEUR")]
    [InlineData("Oncle", "ONCLE")]
    [InlineData("Tante", "TANTE")]
    [InlineData("Cousin(e)", "COUSIN")]
    [InlineData("Cousine", "COUSINE")]
    [InlineData("Grand-père", "GRAND_PERE")]
    [InlineData("Grand-mère", "GRAND_MERE")]
    [InlineData("Collègue", "COLLEGUE")]
    public void Normaliser_LibelleFrancais_RetourneCode(string input, string expected)
    {
        Assert.Equal(expected, LienParenteRegles.Normaliser(input));
        Assert.True(LienParenteRegles.EstValide(input));
    }

    [Theory]
    [InlineData("jjk")]
    [InlineData("Inconnu")]
    [InlineData("")]
    [InlineData("   ")]
    public void EstValide_ValeurInconnue_RetourneFalse(string? input)
    {
        Assert.False(LienParenteRegles.EstValide(input));
    }

    [Fact]
    public void EstValide_TousLesCodesTechniques_SontAcceptes()
    {
        foreach (var code in LienParenteRegles.ValeursValides)
        {
            Assert.True(LienParenteRegles.EstValide(code));
            Assert.Equal(code, LienParenteRegles.Normaliser(code));
        }
    }

    [Fact]
    public void GetReferentiel_Contient23CodesAvecLibelles()
    {
        var referentiel = LienParenteRegles.GetReferentiel();

        Assert.Equal(23, referentiel.Count);
        Assert.Equal(LienParenteRegles.ValeursValides.Length, referentiel.Count);
        Assert.All(referentiel, e => Assert.False(string.IsNullOrWhiteSpace(e.Libelle)));
        Assert.Contains(referentiel, e => e.Code == "EPOUSE" && e.Libelle == "Épouse");
        Assert.Contains(referentiel, e => e.Code == "FILS" && e.Categorie == "ENFANT");
    }

    [Fact]
    public void EstLienEnfant_EtConjoint_UtilisentCodesNormalises()
    {
        Assert.True(LienParenteRegles.EstLienEnfant("Fils"));
        Assert.True(LienParenteRegles.EstLienConjoint("Épouse"));
        Assert.False(LienParenteRegles.EstLienEnfant("PERE"));
    }
}
