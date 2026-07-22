using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using Xunit;

namespace Prosoc.Tests.Integration;

public class PerceptionVirtuelleIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PerceptionVirtuelleIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        TestAuthHandler.Roles = new[] { "Admin", "Percepteur" };
    }

    [Fact]
    public async Task ConfirmerPerception_CollecteVirtuelle_MarquePercu()
    {
        int agentId;
        int collecteId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var devise = await db.Devises.FirstAsync(d => d.EstDevisePrincipale && d.Statut);
            var agent = await db.Agents.FirstAsync(a => a.Statut);
            agentId = agent.IdAgent;

            var affilie = new Affilie
            {
                CodeAdhesion = $"AFF-PV-{Guid.NewGuid():N}"[..12],
                Nom = "Test",
                Prenom = "Perception",
                NomComplet = "Test Perception",
                DateNaissance = new DateTime(1992, 5, 5),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();

            if (!await db.Adhesions.AnyAsync(a => a.AffilieId == affilie.IdAffilie))
            {
                var utilisateurId = await db.Utilisateurs.Where(u => u.Statut).Select(u => u.IdUtilisateur).FirstAsync();
                db.Adhesions.Add(new Adhesion
                {
                    AffilieId = affilie.IdAffilie,
                    AgentId = agentId,
                    TypeAdhesionId = await db.TypeAdhesions.Select(t => t.IdTypeAdhesion).FirstAsync(),
                    UtilisateurId = utilisateurId,
                    StatutDossier = "A",
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }

            var wallet = await db.WalletsVirtuelsAgents.FirstOrDefaultAsync(w => w.AgentId == agentId && w.Statut);
            if (wallet == null)
            {
                wallet = new WalletVirtuelAgent
                {
                    AgentId = agentId,
                    DeviseId = devise.IdDevise,
                    SoldeVirtuel = 10000m,
                    Statut = true
                };
                db.WalletsVirtuelsAgents.Add(wallet);
                await db.SaveChangesAsync();
            }

            var frais = await db.Frais.FirstAsync(f => f.Statut);
            var collecte = new Collecte
            {
                AffilieId = affilie.IdAffilie,
                AgentId = agentId,
                DeviseId = devise.IdDevise,
                Montant = 75m,
                MontantDevisePrincipale = 75m,
                TypeCollecte = TypeCollecte.Frais,
                FraisId = frais.IdFrais,
                ModePaiement = MethodePaiementHelper.VirtualAccount,
                StatutPaiement = CollecteStatutPaiement.Valide,
                StatutPerception = CollecteStatutPerception.NonPerçu,
                Statut = true,
                DateCollecte = DateTime.UtcNow
            };
            db.Collectes.Add(collecte);
            await db.SaveChangesAsync();
            collecteId = collecte.IdCollecte;

            db.WalletVirtuelMouvements.Add(new WalletVirtuelMouvement
            {
                WalletVirtuelId = wallet.IdWalletVirtuelAgent,
                Montant = 75m,
                TypeOperation = "DEBIT",
                Source = WalletVirtuelMouvementSources.CollecteCompteVirtuel,
                ReferenceExterne = collecteId,
                Statut = true
            });
            await db.SaveChangesAsync();
        }

        var enAttente = await _client.GetFromJsonAsync<PaginatedResponse<CollecteVirtuelleEnAttenteDto>>(
            $"/api/PerceptionVirtuelle/collectes-en-attente?agentId={agentId}&pagination.page=1&pagination.pageSize=50");
        Assert.NotNull(enAttente);
        Assert.Contains(enAttente!.Data, c => c.IdCollecte == collecteId);

        var confirmResponse = await _client.PostAsJsonAsync(
            "/api/PerceptionVirtuelle/confirmer",
            new PerceptionVirtuelleConfirmerDto
            {
                AgentId = agentId,
                CollecteIds = new List<int> { collecteId },
                Observation = "Test intégration"
            });
        confirmResponse.EnsureSuccessStatusCode();

        var result = await confirmResponse.Content.ReadFromJsonAsync<PerceptionVirtuelleConfirmerResultDto>();
        Assert.NotNull(result);
        Assert.True(result!.Succes);
        Assert.Equal(75m, result.MontantTotal);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var collecte = await db.Collectes.FindAsync(collecteId);
            Assert.Equal(CollecteStatutPerception.Perçu, collecte!.StatutPerception);
            Assert.NotNull(collecte.PerceptionVirtuelleId);
        }
    }

    [Fact]
    public async Task HistoriqueGlobal_Financier_Retourne200()
    {
        TestAuthHandler.Roles = new[] { "Financier" };

        var response = await _client.GetAsync(
            "/api/PerceptionVirtuelle/historique-global?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HistoriqueGlobal_PercepteurSeul_Retourne403()
    {
        TestAuthHandler.Roles = new[] { "Percepteur" };

        var response = await _client.GetAsync(
            "/api/PerceptionVirtuelle/historique-global?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Reconciliation_Financier_RetourneSynthese()
    {
        TestAuthHandler.Roles = new[] { "Financier" };

        var response = await _client.GetAsync("/api/PerceptionVirtuelle/reconciliation");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<PerceptionReconciliationDto>();
        Assert.NotNull(body);
        Assert.NotNull(body!.DeviseCode);
    }

    [Fact]
    public async Task Export_Financier_RetourneExcel()
    {
        TestAuthHandler.Roles = new[] { "Financier" };

        var response = await _client.GetAsync("/api/PerceptionVirtuelle/export?format=excel");
        response.EnsureSuccessStatusCode();

        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            response.Content.Headers.ContentType?.MediaType);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.NotEmpty(bytes);
    }
}
