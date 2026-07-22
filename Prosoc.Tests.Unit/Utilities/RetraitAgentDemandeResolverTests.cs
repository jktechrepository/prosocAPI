using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Utilities;
using Xunit;

namespace Prosoc.Tests.Unit.Utilities;

public class RetraitAgentDemandeResolverTests
{
    private static readonly RetraitAgentOptions DefaultOptions = new();

    [Fact]
    public void Resoudre_Fenetre1_RetournePartiel()
    {
        var dto = new DemandeRetraitAgentCreateDto { AgentId = 1, MontantDemande = 5_000m };
        var result = RetraitAgentDemandeResolver.Resoudre(
            dto,
            new DateTime(2026, 3, 16),
            soldeDisponible: 50_000m,
            DefaultOptions);

        Assert.True(result.Succes);
        Assert.Equal("PARTIEL", result.TypeRetrait);
    }

    [Fact]
    public void Resoudre_Fenetre2_RetourneTotal()
    {
        var dto = new DemandeRetraitAgentCreateDto { AgentId = 1 };
        var result = RetraitAgentDemandeResolver.Resoudre(
            dto,
            new DateTime(2026, 3, 30),
            soldeDisponible: 50_000m,
            DefaultOptions);

        Assert.True(result.Succes);
        Assert.Equal("TOTAL", result.TypeRetrait);
    }

    [Fact]
    public void Resoudre_Partiel_SansMontant_Echoue()
    {
        var dto = new DemandeRetraitAgentCreateDto { AgentId = 1 };
        var result = RetraitAgentDemandeResolver.Resoudre(
            dto,
            new DateTime(2026, 3, 16),
            soldeDisponible: 50_000m,
            DefaultOptions);

        Assert.False(result.Succes);
        Assert.Contains("obligatoire", result.Message);
    }

    [Fact]
    public void Resoudre_Partiel_MontantSousMinimum_Echoue()
    {
        var dto = new DemandeRetraitAgentCreateDto { AgentId = 1, MontantDemande = 500m };
        var result = RetraitAgentDemandeResolver.Resoudre(
            dto,
            new DateTime(2026, 3, 16),
            soldeDisponible: 50_000m,
            DefaultOptions);

        Assert.False(result.Succes);
        Assert.Contains("minimum", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Resoudre_Partiel_MontantValide_RetourneMontantDemande()
    {
        var dto = new DemandeRetraitAgentCreateDto { AgentId = 1, MontantDemande = 5_000m };
        var result = RetraitAgentDemandeResolver.Resoudre(
            dto,
            new DateTime(2026, 3, 16),
            soldeDisponible: 50_000m,
            DefaultOptions);

        Assert.True(result.Succes);
        Assert.Equal("PARTIEL", result.TypeRetrait);
        Assert.Equal(5_000m, result.MontantEffectif);
    }

    [Fact]
    public void Resoudre_Total_SansMontantDemande_UtiliseSoldeDisponible()
    {
        var dto = new DemandeRetraitAgentCreateDto { AgentId = 1 };
        var result = RetraitAgentDemandeResolver.Resoudre(
            dto,
            new DateTime(2026, 3, 30),
            soldeDisponible: 50_000m,
            DefaultOptions);

        Assert.True(result.Succes);
        Assert.Equal("TOTAL", result.TypeRetrait);
        Assert.Equal(50_000m, result.MontantEffectif);
    }

    [Fact]
    public void Resoudre_Total_IgnoreMontantDemandeClient()
    {
        var dto = new DemandeRetraitAgentCreateDto { AgentId = 1, MontantDemande = 1_000m };
        var result = RetraitAgentDemandeResolver.Resoudre(
            dto,
            new DateTime(2026, 3, 30),
            soldeDisponible: 50_000m,
            DefaultOptions);

        Assert.True(result.Succes);
        Assert.Equal(50_000m, result.MontantEffectif);
    }

    [Fact]
    public void Resoudre_Total_TypePartielEnvoye_RemplaceParTotal()
    {
        var dto = new DemandeRetraitAgentCreateDto { AgentId = 1, TypeRetrait = "PARTIEL" };
        var result = RetraitAgentDemandeResolver.Resoudre(
            dto,
            new DateTime(2026, 3, 30),
            soldeDisponible: 50_000m,
            DefaultOptions);

        Assert.True(result.Succes);
        Assert.Equal("TOTAL", result.TypeRetrait);
        Assert.Equal(50_000m, result.MontantEffectif);
    }

    [Fact]
    public void Resoudre_Partiel_TypeTotalEnvoye_RemplaceParPartiel()
    {
        var dto = new DemandeRetraitAgentCreateDto
        {
            AgentId = 1,
            TypeRetrait = "TOTAL",
            MontantDemande = 5_000m
        };
        var result = RetraitAgentDemandeResolver.Resoudre(
            dto,
            new DateTime(2026, 3, 16),
            soldeDisponible: 50_000m,
            DefaultOptions);

        Assert.True(result.Succes);
        Assert.Equal("PARTIEL", result.TypeRetrait);
        Assert.Equal(5_000m, result.MontantEffectif);
    }

    [Fact]
    public void Resoudre_Total_SoldeSousMinimum_Echoue()
    {
        var dto = new DemandeRetraitAgentCreateDto { AgentId = 1 };
        var result = RetraitAgentDemandeResolver.Resoudre(
            dto,
            new DateTime(2026, 3, 30),
            soldeDisponible: 500m,
            DefaultOptions);

        Assert.False(result.Succes);
        Assert.Contains("insuffisant", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ResoudreModeTest_TypeInvalide_RemplaceParPartiel()
    {
        var dto = new DemandeRetraitAgentCreateDto
        {
            AgentId = 1,
            MontantDemande = 5_000m,
            TypeRetrait = "INVALIDE"
        };
        var result = RetraitAgentDemandeResolver.ResoudreModeTest(dto, DefaultOptions);

        Assert.True(result.Succes);
        Assert.Equal("PARTIEL", result.TypeRetrait);
    }

    [Fact]
    public void Resoudre_HorsPeriode_Echoue()
    {
        var dto = new DemandeRetraitAgentCreateDto { AgentId = 1, MontantDemande = 5_000m };
        var result = RetraitAgentDemandeResolver.Resoudre(
            dto,
            new DateTime(2026, 3, 10),
            soldeDisponible: 50_000m,
            DefaultOptions);

        Assert.False(result.Succes);
    }
}
