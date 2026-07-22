using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using Xunit;

namespace Prosoc.Tests.Integration;

public class DashboardPercepteurRapportPerceptionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DashboardPercepteurRapportPerceptionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        TestAuthHandler.Roles = new[] { "Admin", "Percepteur" };
    }

    [Fact]
    public async Task GetRapportPerception_RetourneAgentVaEtAffilieGuichet()
    {
        int collecteVaId;
        int collecteEspeceId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var devise = await db.Devises.FirstAsync(d => d.EstDevisePrincipale && d.Statut);
            var agent = await db.Agents.FirstAsync(a => a.Statut);
            var agentId = agent.IdAgent;

            var affilie = new Affilie
            {
                CodeAdhesion = $"AFF-RP-{Guid.NewGuid():N}"[..12],
                Nom = "Rapport",
                Prenom = "Test",
                NomComplet = "Rapport Test",
                DateNaissance = new DateTime(1991, 3, 3),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();

            var wallet = await db.WalletsVirtuelsAgents.FirstOrDefaultAsync(w => w.AgentId == agentId && w.Statut);
            if (wallet == null)
            {
                wallet = new WalletVirtuelAgent
                {
                    AgentId = agentId,
                    DeviseId = devise.IdDevise,
                    SoldeVirtuel = 1000m,
                    Statut = true
                };
                db.WalletsVirtuelsAgents.Add(wallet);
                await db.SaveChangesAsync();
            }

            var collecteVa = new Collecte
            {
                AffilieId = affilie.IdAffilie,
                AgentId = agentId,
                DeviseId = devise.IdDevise,
                Montant = 60m,
                MontantDevisePrincipale = 60m,
                TypeCollecte = TypeCollecte.Cotisation,
                ModePaiement = MethodePaiementHelper.VirtualAccount,
                StatutPaiement = CollecteStatutPaiement.Valide,
                StatutPerception = CollecteStatutPerception.NonPerçu,
                Statut = true,
                DateCollecte = DateTime.UtcNow
            };
            var collecteEspece = new Collecte
            {
                AffilieId = affilie.IdAffilie,
                DeviseId = devise.IdDevise,
                Montant = 25m,
                MontantDevisePrincipale = 25m,
                TypeCollecte = TypeCollecte.Cotisation,
                ModePaiement = MethodePaiementHelper.Espece,
                StatutPaiement = CollecteStatutPaiement.Valide,
                Statut = true,
                DateCollecte = DateTime.UtcNow
            };
            db.Collectes.AddRange(collecteVa, collecteEspece);
            await db.SaveChangesAsync();
            collecteVaId = collecteVa.IdCollecte;
            collecteEspeceId = collecteEspece.IdCollecte;

            db.WalletVirtuelMouvements.Add(new WalletVirtuelMouvement
            {
                WalletVirtuelId = wallet.IdWalletVirtuelAgent,
                Montant = 60m,
                TypeOperation = "DEBIT",
                Source = WalletVirtuelMouvementSources.CollecteCompteVirtuel,
                ReferenceExterne = collecteVaId,
                Statut = true
            });
            await db.SaveChangesAsync();
        }

        var rapport = await _client.GetFromJsonAsync<PerceptionRapportResponseDto>(
            "/api/DashboardPercepteur/rapport-perception?pageNumber=1&pageSize=50");

        Assert.NotNull(rapport);
        Assert.True(rapport!.Lignes.TotalItems >= 2);

        var ligneVa = rapport.Lignes.Data.FirstOrDefault(l => l.IdCollecte == collecteVaId);
        var ligneEspece = rapport.Lignes.Data.FirstOrDefault(l => l.IdCollecte == collecteEspeceId);

        Assert.NotNull(ligneVa);
        Assert.NotNull(ligneEspece);
        Assert.Equal(PerceptionOrigineHelper.OrigineAgent, ligneVa!.OriginePerception);
        Assert.Equal(PerceptionOrigineHelper.StatutEnAttente, ligneVa.StatutPerception);
        Assert.Equal(PerceptionOrigineHelper.OrigineAffilie, ligneEspece!.OriginePerception);
        Assert.Equal(PerceptionOrigineHelper.StatutPercu, ligneEspece.StatutPerception);
        Assert.NotNull(rapport.Synthese.DeviseCode);
    }

    [Fact]
    public async Task GetRapportPerception_Financier_Retourne200()
    {
        TestAuthHandler.Roles = new[] { "Financier" };

        var response = await _client.GetAsync(
            "/api/DashboardPercepteur/rapport-perception?pageNumber=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
