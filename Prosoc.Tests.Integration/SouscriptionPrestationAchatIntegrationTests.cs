using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using Prosoc.Models.DTOs.CategorieAgent;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using Xunit;

namespace Prosoc.Tests.Integration;

public class SouscriptionPrestationAchatIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public SouscriptionPrestationAchatIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        TestAuthHandler.Roles = new[] { "Admin", "SuperAdmin" };
    }

    [Fact]
    public async Task CreateSouscriptionPrestation_WithCollecteVirtualAccount_PeriodMars2026_Reussit()
    {
        var (agentId, affilieId) = await CreateAffilieWithCotisationPayeeAsync();
        int prestationId;
        int deviseCdfId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (prestationId, deviseCdfId) = await SeedPrestationCdfAsync(db);
        }

        var input = new SouscriptionPrestationAchatCreateDto
        {
            PrestationId = prestationId,
            DateSouscription = DateTime.UtcNow,
            Statut = true,
            Collecte = new SouscriptionPrestationCollecteCreateDto
            {
                AgentId = agentId,
                Montant = 5000m,
                DeviseId = deviseCdfId,
                ModePaiement = "VIRTUAL_ACCOUNT",
                Mois = 3,
                Annee = 2026,
                Observation = "Première période"
            }
        };

        var previousRoles = TestAuthHandler.Roles;
        try
        {
            TestAuthHandler.Roles = new[] { "Agent (AT)" };

            var res = await _client.PostAsJsonAsync(
                $"/api/SouscriptionPrestation?affilieId={affilieId}", input);

            if (res.StatusCode != HttpStatusCode.Created)
            {
                var body = await res.Content.ReadAsStringAsync();
                Assert.Fail($"Expected 201 but got {(int)res.StatusCode}. Body: {body}");
            }

            var created = await res.Content.ReadFromJsonAsync<SouscriptionPrestationAchatReadDto>();
            Assert.NotNull(created);
            Assert.NotNull(created!.Souscription);
            Assert.NotNull(created.Collecte);
            Assert.Equal(prestationId, created.Souscription.PrestationId);
            Assert.Equal(1, created.Souscription.NombreCollectes);
            Assert.Equal(TypeCollecte.Souscription, created.Collecte.TypeCollecte);
            Assert.Equal(3, created.Collecte.Mois);
            Assert.Equal(2026, created.Collecte.Annee);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var subs = await db.SouscriptionsPrestations
                .Where(s => s.AffilieId == affilieId && s.PrestationId == prestationId)
                .ToListAsync();
            Assert.Single(subs);

            var collectes = await db.Collectes
                .Where(c => c.SouscriptionPrestationId == subs[0].IdSouscriptionPrestation)
                .ToListAsync();
            Assert.Single(collectes);
        }
    }

    private async Task<(int AgentId, int AffilieId)> CreateAffilieWithCotisationPayeeAsync()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        var categorieDto = new CreateCategorieAgentDto
        {
            Code = $"S{unique[..6]}",
            Description = "Cat test souscription"
        };
        var categorieRes = await _client.PostAsJsonAsync("/api/CategorieAgent", categorieDto);
        categorieRes.EnsureSuccessStatusCode();
        var categorie = await categorieRes.Content.ReadFromJsonAsync<CategorieAgentDto>();

        var phoneSuffix = Math.Abs(unique.GetHashCode()) % 10_000;
        var agentDto = new AgentCreateDto
        {
            NomComplet = $"Agent Sub {unique}",
            Matricule = $"SB{unique.PadRight(9, '0')}"[..11],
            Phone = $"099904{phoneSuffix:D4}",
            CategorieAgentId = categorie!.IdCategorieAgent,
            Statut = true
        };
        var agentRes = await _client.PostAsJsonAsync("/api/Agent", agentDto);
        agentRes.EnsureSuccessStatusCode();
        var agent = await agentRes.Content.ReadFromJsonAsync<AgentReadDto>();
        Assert.NotNull(agent?.WalletVirtuelId);

        await _client.PutAsJsonAsync(
            $"/api/WalletVirtuelAgent/{agent!.WalletVirtuelId}/ajouter-solde",
            new WalletVirtuelAgentAjouterSoldeDto { Montant = 500m });

        int cotisationAffilieId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var cotisation = await db.CotisationsAffilie
                .FirstOrDefaultAsync(c => c.TypeAdhesionId == 1 && c.Periodicite == "Mensuel" && c.Statut);

            if (cotisation == null)
            {
                var cdf = await db.Devises.FirstAsync(d => d.Code == "CDF");
                cotisation = new CotisationAffilie
                {
                    Montant = 1.5m,
                    Periodicite = "Mensuel",
                    TypeAdhesionId = 1,
                    DeviseId = cdf.IdDevise,
                    Statut = true
                };
                db.CotisationsAffilie.Add(cotisation);
                await db.SaveChangesAsync();
            }

            cotisationAffilieId = cotisation.IdCotisationAffilie;
        }

        var adhesionInput = new AdhesionWithAffilieCreateDto
        {
            Nom = $"Sub{unique}",
            Prenom = "Test",
            DateNaissance = new DateTime(1991, 2, 27),
            Telephone = $"+2439092{phoneSuffix:D5}"[..13],
            ProvinceResidence = "Kinshasa",
            CommuneResidence = "Masina",
            QuartierResidence = "Sans-fil",
            PhotoBase64 = "cGhvdG8=",
            PhotoContentType = "image/png",
            CarteIdentiteBase64 = "Y2FydGU=",
            CarteIdentiteContentType = "image/png",
            AffilieStatut = true,
            StatutDossier = "COMPLET",
            TypeAdhesionId = 1,
            AgentId = agent.Id,
            AdhesionStatut = true,
            Collectes = new List<CollecteAvecSouscriptionDto>
            {
                new()
                {
                    TypeCollecte = TypeCollecte.Cotisation,
                    CotisationAffilieId = cotisationAffilieId,
                    Montant = 1.5m,
                    DeviseId = 1,
                    Mois = DateTime.Today.Month,
                    Annee = DateTime.Today.Year,
                    ModePaiement = "ESPECE",
                    ReferencePaiement = $"REF-SUB-{unique}",
                    StatutPaiement = "PAYE",
                    MontantRecu = 1.5m,
                    MontantAttendu = 1.5m,
                    Statut = true
                }
            }
        };

        var adhesionRes = await _client.PostAsJsonAsync("/api/Adhesion/with-affilie", adhesionInput);
        if (adhesionRes.StatusCode != HttpStatusCode.Created)
        {
            var body = await adhesionRes.Content.ReadAsStringAsync();
            Assert.Fail($"Adhesion setup failed: {(int)adhesionRes.StatusCode} {body}");
        }

        var adhesion = await adhesionRes.Content.ReadFromJsonAsync<AdhesionWithAffilieReadDto>();
        return (agent.Id, adhesion!.AffilieId);
    }

    private static async Task<(int PrestationId, int DeviseCdfId)> SeedPrestationCdfAsync(ProsocDbContext db)
    {
        var cdf = await db.Devises.FirstAsync(d => d.Code == "CDF");
        var unique = Guid.NewGuid().ToString("N")[..6];

        var produit = new ProduitMutuel
        {
            Nom = $"Produit sub {unique}",
            Montant = 5000m,
            EstGratuit = false,
            Periodicite = "Mensuel",
            AgeMin = 0,
            AgeMax = 120,
            DeviseId = cdf.IdDevise,
            Statut = true
        };
        db.ProduitsMutuels.Add(produit);
        await db.SaveChangesAsync();

        var prestation = new Prestation
        {
            NomPrestation = $"Prestation sub {unique}",
            Montant = 5000,
            DeviseId = cdf.IdDevise,
            ProduitMutuelId = produit.IdProduit,
            Statut = true
        };
        db.Prestations.Add(prestation);
        await db.SaveChangesAsync();

        return (prestation.IdPrestation, cdf.IdDevise);
    }
}
