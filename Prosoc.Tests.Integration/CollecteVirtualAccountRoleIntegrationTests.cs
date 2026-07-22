using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using Xunit;

namespace Prosoc.Tests.Integration;

public class CollecteVirtualAccountRoleIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CollecteVirtualAccountRoleIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostCollecte_VirtualAccount_Caissier_Retourne400MessageSupport()
    {
        var previousRoles = TestAuthHandler.Roles;
        try
        {
            var seed = await SeedMinimalAsync();
            TestAuthHandler.Roles = new[] { "Caissier" };

            var dto = BuildFraisCollecteDto(seed, "VIRTUAL_ACCOUNT");
            var response = await _client.PostAsJsonAsync("/api/Collecte", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            Assert.Equal(
                WalletVirtuelPaiementAutorisation.MessageNonAutorise,
                doc.RootElement.GetProperty("message").GetString());
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
        }
    }

    [Fact]
    public async Task PostCollecte_Espece_Caissier_PasBloqueParRoleVa()
    {
        var previousRoles = TestAuthHandler.Roles;
        try
        {
            var seed = await SeedMinimalAsync();
            TestAuthHandler.Roles = new[] { "Caissier" };

            var dto = BuildFraisCollecteDto(seed, "ESPECE");
            dto.ReferencePaiement = $"REF-ESP-{Guid.NewGuid():N}"[..20];

            var response = await _client.PostAsJsonAsync("/api/Collecte", dto);

            if (response.StatusCode != HttpStatusCode.Created)
            {
                var body = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Expected 201 but got {(int)response.StatusCode}. Body: {body}");
            }
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
        }
    }

    [Fact]
    public async Task PostCollecte_VirtualAccount_AgentAt_AvecSolde_Reussit()
    {
        var previousRoles = TestAuthHandler.Roles;
        try
        {
            var seed = await SeedMinimalAsync(walletSolde: 5000m);
            TestAuthHandler.Roles = new[] { "Agent (AT)" };

            var dto = BuildFraisCollecteDto(seed, "VIRTUAL_ACCOUNT");
            var response = await _client.PostAsJsonAsync("/api/Collecte", dto);

            if (response.StatusCode != HttpStatusCode.Created)
            {
                var body = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Expected 201 but got {(int)response.StatusCode}. Body: {body}");
            }
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
        }
    }

    private static CollecteCreateDto BuildFraisCollecteDto(
        (int AffilieId, int AgentId, int FraisId, int DeviseId, decimal Montant) seed,
        string modePaiement) => new()
    {
        TypeCollecte = TypeCollecte.Frais,
        AffilieId = seed.AffilieId,
        AgentId = seed.AgentId,
        FraisId = seed.FraisId,
        Montant = seed.Montant,
        DeviseId = seed.DeviseId,
        ModePaiement = modePaiement,
        StatutPaiement = CollecteStatutPaiement.Valide,
        Statut = true
    };

    private async Task<(int AffilieId, int AgentId, int FraisId, int DeviseId, decimal Montant)> SeedMinimalAsync(
        decimal walletSolde = 1000m)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

        var frais = await db.Frais.AsNoTracking().FirstAsync(f => f.Statut);
        var deviseId = frais.DeviseId > 0
            ? frais.DeviseId
            : await db.Devises.Where(d => d.Statut).Select(d => d.IdDevise).FirstAsync();
        var montant = (decimal)frais.Montant;
        var zone = await db.ZonesSociales.FirstAsync();

        var agent = new Agent
        {
            NomComplet = $"Agent VA Role {Guid.NewGuid():N}"[..28],
            Matricule = $"VA{Guid.NewGuid():N}"[..10],
            Phone = $"08{Guid.NewGuid():N}"[..10],
            ZoneSocialeId = zone.IdZoneSociale,
            Statut = true,
            DateCreation = DateTime.UtcNow
        };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();

        db.WalletsVirtuelsAgents.Add(new WalletVirtuelAgent
        {
            AgentId = agent.IdAgent,
            DeviseId = deviseId,
            SoldeVirtuel = walletSolde,
            Statut = true
        });

        var affilie = new Affilie
        {
            Nom = "VA",
            Prenom = "Role",
            NomComplet = "VA Role",
            DateNaissance = new DateTime(1990, 1, 1),
            Telephone = $"09{Guid.NewGuid():N}"[..10],
            ProvinceResidence = "Kinshasa",
            CommuneResidence = "Gombe",
            QuartierResidence = "Centre",
            Statut = true,
            DateCreation = DateTime.UtcNow
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        var userId = await db.Utilisateurs.Select(u => u.IdUtilisateur).FirstAsync();
        var typeAdhesionId = await db.TypeAdhesions.Select(t => t.IdTypeAdhesion).FirstAsync();
        db.Adhesions.Add(new Adhesion
        {
            AffilieId = affilie.IdAffilie,
            TypeAdhesionId = typeAdhesionId,
            AgentId = agent.IdAgent,
            UtilisateurId = userId,
            StatutDossier = "VALIDE",
            Statut = true,
            DateCreation = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        return (affilie.IdAffilie, agent.IdAgent, frais.IdFrais, deviseId, montant);
    }
}
