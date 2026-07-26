using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using Prosoc.Models.DTOs.CategorieAgent;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Prosoc.Tests.Integration
{
    public class WalletVirtuelAgentAjouterSoldeIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private static int _phoneSequence;
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public WalletVirtuelAgentAjouterSoldeIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            TestAuthHandler.Roles = new[] { "Admin", "SuperAdmin" };
        }

        private async Task LinkAgentRoleAsync(int agentId, string roleNom)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var role = await db.Roles.FirstAsync(r => r.Nom == roleNom);
            var user = await db.Utilisateurs.FirstOrDefaultAsync(u => u.AgentId == agentId);
            if (user == null)
            {
                user = new Utilisateur
                {
                    NomUtilisateur = $"u-wv-{agentId}-{Guid.NewGuid():N}"[..20],
                    MotDePasseHash = "x",
                    AgentId = agentId,
                    Statut = true
                };
                db.Utilisateurs.Add(user);
                await db.SaveChangesAsync();
            }

            var exists = await db.UserRoles.AnyAsync(ur =>
                ur.UtilisateurId == user.IdUtilisateur && ur.RoleId == role.IdRole);
            if (!exists)
            {
                db.UserRoles.Add(new UserRole
                {
                    UtilisateurId = user.IdUtilisateur,
                    RoleId = role.IdRole,
                    Statut = true,
                    IsPrimary = true
                });
                await db.SaveChangesAsync();
            }
        }

        private async Task<(int AgentId, int WalletVirtuelId)> CreateAgentWithWalletVirtuelAsync(
            string roleNom = "Agent (AT)")
        {
            var unique = Guid.NewGuid().ToString("N")[..8];
            var categorieDto = new CreateCategorieAgentDto
            {
                Code = $"W{unique[..6]}",
                Description = "Catégorie test wallet virtuel"
            };

            var categorieResponse = await _client.PostAsJsonAsync("/api/CategorieAgent", categorieDto);
            categorieResponse.EnsureSuccessStatusCode();
            var createdCategorie = await categorieResponse.Content.ReadFromJsonAsync<CategorieAgentDto>();
            Assert.NotNull(createdCategorie);

            var phoneSuffix = Interlocked.Increment(ref _phoneSequence) % 10_000;
            var agentDto = new AgentCreateDto
            {
                NomComplet = $"Agent WV {unique}",
                Matricule = $"WV{unique.PadRight(9, '0')}"[..11],
                Phone = $"099789{phoneSuffix:D4}",
                CategorieAgentId = createdCategorie!.IdCategorieAgent,
                Statut = true
            };

            var agentResponse = await _client.PostAsJsonAsync("/api/Agent", agentDto);
            agentResponse.EnsureSuccessStatusCode();
            var createdAgent = await agentResponse.Content.ReadFromJsonAsync<AgentReadDto>();
            Assert.NotNull(createdAgent);
            Assert.True(createdAgent.WalletVirtuelCree);
            Assert.NotNull(createdAgent.WalletVirtuelId);

            await LinkAgentRoleAsync(createdAgent.Id, roleNom);

            return (createdAgent.Id, createdAgent.WalletVirtuelId!.Value);
        }

        [Fact]
        public async Task AjouterSolde_AvecMontantValide_AjouteAuSoldeExistant()
        {
            TestAuthHandler.Roles = new[] { "Admin", "SuperAdmin" };
            var (agentId, walletId) = await CreateAgentWithWalletVirtuelAsync();

            var soldeInitialResponse = await _client.GetAsync($"/api/WalletVirtuelAgent/solde/{agentId}");
            soldeInitialResponse.EnsureSuccessStatusCode();
            var soldeInitial = await soldeInitialResponse.Content.ReadFromJsonAsync<decimal>();

            var response = await _client.PutAsJsonAsync(
                $"/api/WalletVirtuelAgent/{walletId}/ajouter-solde",
                new WalletVirtuelAgentAjouterSoldeDto { Montant = 100m, Observation = "Recharge test intégration" });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<WalletVirtuelAgentAjouterSoldeResultDto>();
            Assert.NotNull(result);
            Assert.Equal(soldeInitial, result.AncienSolde);
            Assert.Equal(100m, result.MontantAjoute);
            Assert.Equal(soldeInitial + 100m, result.NouveauSolde);
            Assert.Equal(result.NouveauSolde, result.Wallet.SoldeVirtuel);

            var soldeApresResponse = await _client.GetAsync($"/api/WalletVirtuelAgent/solde/{agentId}");
            soldeApresResponse.EnsureSuccessStatusCode();
            var soldeApres = await soldeApresResponse.Content.ReadFromJsonAsync<decimal>();
            Assert.Equal(soldeInitial + 100m, soldeApres);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
                var mouvement = await db.WalletVirtuelMouvements
                    .OrderByDescending(m => m.IdWalletVirtuelMouvement)
                    .FirstAsync(m => m.WalletVirtuelId == walletId);
                Assert.Equal(WalletVirtuelMouvementSources.AjoutSolde, mouvement.Source);
                Assert.Equal("CREDIT", mouvement.TypeOperation);
                Assert.Equal(soldeInitial, mouvement.SoldeAvant);
                Assert.Equal(soldeInitial + 100m, mouvement.SoldeApres);
                Assert.Equal("Recharge test intégration", mouvement.Description);
                Assert.NotNull(mouvement.DeviseId);
            }
        }

        [Fact]
        public async Task AjouterSolde_AtVersSuperviseur_Retourne403()
        {
            var previousRoles = TestAuthHandler.Roles;
            var previousPermissions = TestAuthHandler.Permissions;
            try
            {
                TestAuthHandler.Roles = new[] { "Admin", "SuperAdmin" };
                TestAuthHandler.Permissions = Array.Empty<string>();
                var (_, walletId) = await CreateAgentWithWalletVirtuelAsync("Superviseur");

                TestAuthHandler.Roles = new[] { "Agent (AT)" };
                TestAuthHandler.Permissions = new[] { "UPDATE_WALLET_VIRTUEL" };
                var response = await _client.PutAsJsonAsync(
                    $"/api/WalletVirtuelAgent/{walletId}/ajouter-solde",
                    new WalletVirtuelAgentAjouterSoldeDto { Montant = 50m, Observation = "Tentative AT→SP" });

                Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
                var body = await response.Content.ReadAsStringAsync();
                Assert.Contains("HIERARCHIE_RECHARGE_INTERDITE", body, StringComparison.Ordinal);
            }
            finally
            {
                TestAuthHandler.Roles = previousRoles;
                TestAuthHandler.Permissions = previousPermissions;
            }
        }

        [Fact]
        public async Task AjouterSolde_WalletInexistant_RetourneNotFound()
        {
            TestAuthHandler.Roles = new[] { "Admin", "SuperAdmin" };
            var response = await _client.PutAsJsonAsync(
                "/api/WalletVirtuelAgent/999999999/ajouter-solde",
                new WalletVirtuelAgentAjouterSoldeDto { Montant = 50m });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AjouterSolde_MontantZero_RetourneBadRequest()
        {
            TestAuthHandler.Roles = new[] { "Admin", "SuperAdmin" };
            var (_, walletId) = await CreateAgentWithWalletVirtuelAsync();

            var response = await _client.PutAsJsonAsync(
                $"/api/WalletVirtuelAgent/{walletId}/ajouter-solde",
                new WalletVirtuelAgentAjouterSoldeDto { Montant = 0m });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AjouterSolde_WalletInactif_RetourneBadRequest()
        {
            TestAuthHandler.Roles = new[] { "Admin", "SuperAdmin" };
            var (_, walletId) = await CreateAgentWithWalletVirtuelAsync();

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
                var wallet = await db.WalletsVirtuelsAgents.FindAsync(walletId);
                Assert.NotNull(wallet);
                wallet.Statut = false;
                await db.SaveChangesAsync();
            }

            var response = await _client.PutAsJsonAsync(
                $"/api/WalletVirtuelAgent/{walletId}/ajouter-solde",
                new WalletVirtuelAgentAjouterSoldeDto { Montant = 50m });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("inactif", body, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AjouterSolde_FinancierSansUpdateWalletVirtuel_Retourne403()
        {
            var previousRoles = TestAuthHandler.Roles;
            var previousPermissions = TestAuthHandler.Permissions;
            try
            {
                TestAuthHandler.Roles = new[] { "Admin", "SuperAdmin" };
                TestAuthHandler.Permissions = Array.Empty<string>();
                var (_, walletId) = await CreateAgentWithWalletVirtuelAsync();

                TestAuthHandler.Roles = new[] { "Financier" };
                TestAuthHandler.Permissions = Array.Empty<string>();

                var response = await _client.PutAsJsonAsync(
                    $"/api/WalletVirtuelAgent/{walletId}/ajouter-solde",
                    new WalletVirtuelAgentAjouterSoldeDto { Montant = 50m, Observation = "Financier interdit" });

                Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
                var body = await response.Content.ReadAsStringAsync();
                Assert.Contains("UPDATE_WALLET_VIRTUEL", body, StringComparison.Ordinal);
            }
            finally
            {
                TestAuthHandler.Roles = previousRoles;
                TestAuthHandler.Permissions = previousPermissions;
            }
        }
    }
}
