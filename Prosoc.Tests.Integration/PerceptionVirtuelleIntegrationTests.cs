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
        TestAuthHandler.Permissions = new[]
        {
            "READ_PERCEPTION_VIRTUAL",
            "CONFIRM_PERCEPTION_VIRTUAL"
        };
    }

    private async Task EnsureSessionCaisseOuverteAsync()
    {
        var courante = await _client.GetAsync("/api/Caisse/session/courante");
        if (courante.IsSuccessStatusCode)
            return;

        var open = await _client.PostAsJsonAsync(
            "/api/Caisse/session/ouvrir",
            new SessionCaisseOuvrirDto { SoldeOuverture = 500000m });
        open.EnsureSuccessStatusCode();
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

        await EnsureSessionCaisseOuverteAsync();

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

            Assert.True(await db.MouvementsCaisses.AnyAsync(m =>
                m.PerceptionVirtuelleId == collecte.PerceptionVirtuelleId && m.Statut
                && m.Source == MouvementCaisseSources.PerceptionVirtuelle));
            Assert.True(await db.WalletVirtuelMouvements.AnyAsync(m =>
                m.ReferenceExterne == collecteId
                && m.Source == WalletVirtuelMouvementSources.RemisePerceptionVirtuelle
                && m.TypeOperation == "CREDIT"
                && m.Statut));
        }
    }

    [Fact]
    public async Task AnnulerPuisReconfirmer_CollecteRedevenueNonPercu()
    {
        TestAuthHandler.Roles = new[] { "Admin", "Financier" };
        TestAuthHandler.Permissions = new[]
        {
            "READ_PERCEPTION_VIRTUAL",
            "CONFIRM_PERCEPTION_VIRTUAL"
        };

        int agentId;
        int collecteId;
        int perceptionId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var devise = await db.Devises.FirstAsync(d => d.EstDevisePrincipale && d.Statut);
            var agent = await db.Agents.FirstAsync(a => a.Statut);
            agentId = agent.IdAgent;

            var affilie = new Affilie
            {
                CodeAdhesion = $"AFF-AN-{Guid.NewGuid():N}"[..12],
                Nom = "Annul",
                Prenom = "Test",
                NomComplet = "Annul Test",
                DateNaissance = new DateTime(1991, 1, 1),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();

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
                Montant = 40m,
                MontantDevisePrincipale = 40m,
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
                Montant = 40m,
                TypeOperation = "DEBIT",
                Source = WalletVirtuelMouvementSources.CollecteCompteVirtuel,
                ReferenceExterne = collecteId,
                Statut = true
            });
            await db.SaveChangesAsync();
        }

        await EnsureSessionCaisseOuverteAsync();

        var confirmResponse = await _client.PostAsJsonAsync(
            "/api/PerceptionVirtuelle/confirmer",
            new PerceptionVirtuelleConfirmerDto
            {
                AgentId = agentId,
                CollecteIds = new List<int> { collecteId }
            });
        confirmResponse.EnsureSuccessStatusCode();
        var confirm = await confirmResponse.Content.ReadFromJsonAsync<PerceptionVirtuelleConfirmerResultDto>();
        perceptionId = confirm!.PerceptionVirtuelleId!.Value;

        var annulResponse = await _client.PostAsJsonAsync(
            $"/api/PerceptionVirtuelle/{perceptionId}/annuler",
            new PerceptionVirtuelleAnnulerDto { Motif = "Correction test intégration" });
        annulResponse.EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var collecte = await db.Collectes.FindAsync(collecteId);
            Assert.Equal(CollecteStatutPerception.NonPerçu, collecte!.StatutPerception);
            Assert.Null(collecte.PerceptionVirtuelleId);

            var perception = await db.PerceptionsVirtuelles.FindAsync(perceptionId);
            Assert.Equal(PerceptionVirtuelleStatuts.Annulee, perception!.StatutMetier);

            Assert.False(await db.MouvementsCaisses.AnyAsync(m =>
                m.PerceptionVirtuelleId == perceptionId && m.Statut));
        }

        await EnsureSessionCaisseOuverteAsync();

        var reconfirm = await _client.PostAsJsonAsync(
            "/api/PerceptionVirtuelle/confirmer",
            new PerceptionVirtuelleConfirmerDto
            {
                AgentId = agentId,
                CollecteIds = new List<int> { collecteId }
            });
        reconfirm.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Confirmer_PercepteurSansPermission_Retourne403()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Percepteur" };
            TestAuthHandler.Permissions = Array.Empty<string>();

            var response = await _client.PostAsJsonAsync(
                "/api/PerceptionVirtuelle/confirmer",
                new PerceptionVirtuelleConfirmerDto
                {
                    AgentId = 1,
                    CollecteIds = new List<int> { 1 }
                });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task HistoriqueGlobal_Financier_Retourne200()
    {
        TestAuthHandler.Roles = new[] { "Financier" };
        TestAuthHandler.Permissions = new[] { "READ_PERCEPTION_VIRTUAL" };

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
        TestAuthHandler.Permissions = new[] { "READ_PERCEPTION_VIRTUAL" };

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
        TestAuthHandler.Permissions = new[] { "READ_PERCEPTION_VIRTUAL" };

        var response = await _client.GetAsync("/api/PerceptionVirtuelle/export?format=excel");
        response.EnsureSuccessStatusCode();

        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            response.Content.Headers.ContentType?.MediaType);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.NotEmpty(bytes);
    }
}
