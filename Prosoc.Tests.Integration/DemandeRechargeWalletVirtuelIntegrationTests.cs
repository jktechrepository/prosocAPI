using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using Prosoc.Models.DTOs.CategorieAgent;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using Xunit;

namespace Prosoc.Tests.Integration;

public class DemandeRechargeWalletVirtuelIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private static int _phoneSequence;
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DemandeRechargeWalletVirtuelIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        TestAuthHandler.Roles = new[] { "Admin", "SuperAdmin" };
        TestAuthHandler.Permissions = Array.Empty<string>();
    }

    private async Task EnsurePlafondAsync(decimal plafond = 100m)
    {
        TestAuthHandler.Roles = new[] { "Admin" };
        var response = await _client.PutAsJsonAsync(
            "/api/parametres-metier/plafond-wallet-virtuel",
            new WalletVirtuelParametresUpdateDto { PlafondSolde = plafond });
        response.EnsureSuccessStatusCode();
    }

    private async Task<(int AgentId, int WalletId)> CreateAgentWithWalletAsync(
        string roleNom = "Agent (AT)",
        decimal soldeInitial = 0m)
    {
        TestAuthHandler.Roles = new[] { "Admin", "SuperAdmin" };
        var unique = Guid.NewGuid().ToString("N")[..8];
        var categorieDto = new CreateCategorieAgentDto
        {
            Code = $"R{unique[..6]}",
            Description = "Catégorie recharge test"
        };

        var categorieResponse = await _client.PostAsJsonAsync("/api/CategorieAgent", categorieDto);
        categorieResponse.EnsureSuccessStatusCode();
        var createdCategorie = await categorieResponse.Content.ReadFromJsonAsync<CategorieAgentDto>();
        Assert.NotNull(createdCategorie);

        var phoneSuffix = Interlocked.Increment(ref _phoneSequence) % 10_000;
        var agentDto = new AgentCreateDto
        {
            NomComplet = $"Agent Recharge {unique}",
            Matricule = $"RC{unique.PadRight(9, '0')}"[..11],
            Phone = $"099788{phoneSuffix:D4}",
            CategorieAgentId = createdCategorie!.IdCategorieAgent,
            Statut = true
        };

        var agentResponse = await _client.PostAsJsonAsync("/api/Agent", agentDto);
        agentResponse.EnsureSuccessStatusCode();
        var createdAgent = await agentResponse.Content.ReadFromJsonAsync<AgentReadDto>();
        Assert.NotNull(createdAgent);
        Assert.NotNull(createdAgent.WalletVirtuelId);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var role = await db.Roles.FirstAsync(r => r.Nom == roleNom);
            var user = await db.Utilisateurs.FirstOrDefaultAsync(u => u.AgentId == createdAgent.Id);
            if (user == null)
            {
                user = new Utilisateur
                {
                    NomUtilisateur = $"u-rc-{createdAgent.Id}-{Guid.NewGuid():N}"[..20],
                    MotDePasseHash = "x",
                    AgentId = createdAgent.Id,
                    Statut = true
                };
                db.Utilisateurs.Add(user);
                await db.SaveChangesAsync();
            }

            if (!await db.UserRoles.AnyAsync(ur =>
                    ur.UtilisateurId == user.IdUtilisateur && ur.RoleId == role.IdRole))
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

            if (soldeInitial > 0)
            {
                var wallet = await db.WalletsVirtuelsAgents
                    .FirstAsync(w => w.IdWalletVirtuelAgent == createdAgent.WalletVirtuelId);
                wallet.SoldeVirtuel = soldeInitial;
                await db.SaveChangesAsync();
            }
        }

        return (createdAgent.Id, createdAgent.WalletVirtuelId!.Value);
    }

    [Fact]
    public async Task GetPutPlafond_AsAdmin_Works()
    {
        TestAuthHandler.Roles = new[] { "Admin" };

        var put = await _client.PutAsJsonAsync(
            "/api/parametres-metier/plafond-wallet-virtuel",
            new WalletVirtuelParametresUpdateDto { PlafondSolde = 250m });
        put.EnsureSuccessStatusCode();

        var get = await _client.GetAsync("/api/parametres-metier/plafond-wallet-virtuel");
        get.EnsureSuccessStatusCode();
        var result = await get.Content.ReadFromJsonAsync<WalletVirtuelParametresReadDto>();
        Assert.NotNull(result);
        Assert.Equal(250m, result.PlafondSolde);
    }

    [Fact]
    public async Task CreerEtConfirmer_CrediteJusquAuPlafond()
    {
        await EnsurePlafondAsync(100m);
        var (agentId, _) = await CreateAgentWithWalletAsync(soldeInitial: 30m);

        TestAuthHandler.Roles = new[] { "Admin" };
        TestAuthHandler.Permissions = Array.Empty<string>();

        var createResponse = await _client.PostAsJsonAsync(
            "/api/DemandeRechargeWalletVirtuel",
            new DemandeRechargeWalletVirtuelCreateDto { AgentId = agentId, Motif = "Besoin terrain" });
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<DemandeRechargeWalletVirtuelReadDto>();
        Assert.NotNull(created);
        Assert.Equal(70m, created.MontantCalcule);
        Assert.Equal("EN_ATTENTE", created.StatutDemande);

        var confirmResponse = await _client.PostAsync(
            $"/api/DemandeRechargeWalletVirtuel/{created.IdDemande}/confirmer",
            null);
        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
        var confirmed = await confirmResponse.Content.ReadFromJsonAsync<DemandeRechargeWalletVirtuelReadDto>();
        Assert.NotNull(confirmed);
        Assert.Equal("CONFIRMEE", confirmed.StatutDemande);
        Assert.Equal(70m, confirmed.MontantCredite);
        Assert.Equal(100m, confirmed.SoldeApresCredit);

        var soldeResponse = await _client.GetAsync($"/api/WalletVirtuelAgent/solde/{agentId}");
        soldeResponse.EnsureSuccessStatusCode();
        var solde = await soldeResponse.Content.ReadFromJsonAsync<decimal>();
        Assert.Equal(100m, solde);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
        var mouvement = await db.WalletVirtuelMouvements
            .OrderByDescending(m => m.IdWalletVirtuelMouvement)
            .FirstAsync(m => m.ReferenceExterne == created.IdDemande);
        Assert.Equal(WalletVirtuelMouvementSources.RechargePlafond, mouvement.Source);
        Assert.Equal("CREDIT", mouvement.TypeOperation);
        Assert.Equal(70m, mouvement.Montant);
    }

    [Fact]
    public async Task Creer_SoldeAuPlafond_ReturnsBadRequest()
    {
        await EnsurePlafondAsync(100m);
        var (agentId, _) = await CreateAgentWithWalletAsync(soldeInitial: 100m);

        TestAuthHandler.Roles = new[] { "Admin" };
        var response = await _client.PostAsJsonAsync(
            "/api/DemandeRechargeWalletVirtuel",
            new DemandeRechargeWalletVirtuelCreateDto { AgentId = agentId });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("SOLDE_AU_PLAFOND", body);
    }

    [Fact]
    public async Task GetEnAttente_WithoutPermission_ReturnsForbidden()
    {
        var previousRoles = TestAuthHandler.Roles;
        var previousPermissions = TestAuthHandler.Permissions;
        try
        {
            TestAuthHandler.Roles = new[] { "Agent (AT)" };
            TestAuthHandler.Permissions = Array.Empty<string>();

            var response = await _client.GetAsync("/api/DemandeRechargeWalletVirtuel/en-attente");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
            TestAuthHandler.Permissions = previousPermissions;
        }
    }

    [Fact]
    public async Task Rejeter_PasseEnRejetee()
    {
        await EnsurePlafondAsync(100m);
        var (agentId, _) = await CreateAgentWithWalletAsync(soldeInitial: 10m);

        TestAuthHandler.Roles = new[] { "Admin" };
        var createResponse = await _client.PostAsJsonAsync(
            "/api/DemandeRechargeWalletVirtuel",
            new DemandeRechargeWalletVirtuelCreateDto { AgentId = agentId });
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<DemandeRechargeWalletVirtuelReadDto>();
        Assert.NotNull(created);

        var rejectResponse = await _client.PostAsJsonAsync(
            $"/api/DemandeRechargeWalletVirtuel/{created.IdDemande}/rejeter",
            new DemandeRechargeWalletVirtuelRejeterDto { Motif = "Dossier incomplet" });
        Assert.Equal(HttpStatusCode.OK, rejectResponse.StatusCode);
        var rejected = await rejectResponse.Content.ReadFromJsonAsync<DemandeRechargeWalletVirtuelReadDto>();
        Assert.NotNull(rejected);
        Assert.Equal("REJETEE", rejected.StatutDemande);
        Assert.Equal("Dossier incomplet", rejected.MotifRejet);
    }
}
