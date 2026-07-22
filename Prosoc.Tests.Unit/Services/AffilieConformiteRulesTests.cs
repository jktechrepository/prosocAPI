using ProsocAPI.Models.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class AffilieConformiteRulesTests
{
    private static readonly DateTime Today = new(2026, 6, 15);

    [Fact]
    public void EstEnOrdre_ToutPaye_RetourneTrue()
    {
        var lignes = new[]
        {
            CreateArriere(TypeCollecte.Cotisation, ArrieresAffilieStatuts.Paye, 0m, Today.AddDays(-10)),
            CreateArriere(TypeCollecte.Souscription, ArrieresAffilieStatuts.Paye, 0m, Today.AddDays(-5))
        };

        Assert.True(AffilieConformiteRules.EstEnOrdre(lignes, Today));
    }

    [Fact]
    public void EstEnOrdre_EnRetard_RetourneFalse()
    {
        var lignes = new[]
        {
            CreateArriere(TypeCollecte.Cotisation, ArrieresAffilieStatuts.EnRetard, 50m, Today.AddDays(-30))
        };

        Assert.False(AffilieConformiteRules.EstEnOrdre(lignes, Today));
    }

    [Fact]
    public void EstEnOrdre_EnAttenteEchu_RetourneFalse()
    {
        var lignes = new[]
        {
            CreateArriere(TypeCollecte.Cotisation, ArrieresAffilieStatuts.EnAttente, 100m, Today.AddDays(-1))
        };

        Assert.False(AffilieConformiteRules.EstEnOrdre(lignes, Today));
    }

    [Fact]
    public void EstEnOrdrePourType_CotisationOkPrestationKo()
    {
        var lignes = new[]
        {
            CreateArriere(TypeCollecte.Cotisation, ArrieresAffilieStatuts.Paye, 0m, Today.AddDays(-10)),
            CreateArriere(TypeCollecte.Souscription, ArrieresAffilieStatuts.PartiellementPaye, 25m, Today.AddDays(-20))
        };

        Assert.True(AffilieConformiteRules.EstEnOrdrePourType(lignes, TypeCollecte.Cotisation, Today));
        Assert.False(AffilieConformiteRules.EstEnOrdrePourType(lignes, TypeCollecte.Souscription, Today));
    }

    [Fact]
    public void EstEnOrdrePourType_SansObligation_RetourneTrue()
    {
        var lignes = new[]
        {
            CreateArriere(TypeCollecte.Cotisation, ArrieresAffilieStatuts.Paye, 0m, Today)
        };

        Assert.True(AffilieConformiteRules.EstEnOrdrePourType(lignes, TypeCollecte.Souscription, Today));
    }

    private static ArrieresAffilie CreateArriere(
        TypeCollecte type,
        string statut,
        decimal reste,
        DateTime echeance) =>
        new()
        {
            TypeObligation = type,
            StatutPaiement = statut,
            RestAPayer = reste,
            MontantAttendu = reste > 0 ? reste : 100m,
            DateEcheance = echeance,
            Mois = echeance.Month,
            Annee = echeance.Year,
            Statut = true
        };
}
