using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using Prosoc.Models.DTOs.CategorieAgent;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.DTOs.FlexPay;
using Xunit;

namespace Prosoc.Tests.Integration.FlexPay;

public class SouscriptionPrestationPaiementElectroniqueIntegrationTests : IClassFixture<FlexPayWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FlexPayWebApplicationFactory _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SouscriptionPrestationPaiementElectroniqueIntegrationTests(FlexPayWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        TestAuthHandler.Roles = new[] { "Admin" };
    }

    [Fact]
    public async Task PostSync_AvecMobileMoney_Retourne400()
    {
        var (agentId, affilieId, prestationId, deviseId) = await SeedEligibleAchatAsync();

        var input = new SouscriptionPrestationAchatCreateDto
        {
            PrestationId = prestationId,
            Statut = true,
            Collecte = new SouscriptionPrestationCollecteCreateDto
            {
                AgentId = agentId,
                Montant = 5000m,
                DeviseId = deviseId,
                ModePaiement = "MOBILE_MONEY",
                Mois = DateTime.Today.Month,
                Annee = DateTime.Today.Year
            }
        };

        var res = await _client.PostAsJsonAsync($"/api/SouscriptionPrestation?affilieId={affilieId}", input);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var body = await res.Content.ReadAsStringAsync();
        Assert.Contains("paiement-electronique", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PaiementElectronique_InitiatePuisCallback_CreeSouscriptionEtCollecte()
    {
        var (agentId, affilieId, prestationId, deviseId) = await SeedEligibleAchatAsync();

        var initiateBody = BuildElectroniqueDto(affilieId, agentId, prestationId, deviseId, "0823333444");
        var initRes = await _client.PostAsJsonAsync(
            "/api/SouscriptionPrestation/paiement-electronique", initiateBody);

        if (initRes.StatusCode != HttpStatusCode.Accepted)
        {
            var err = await initRes.Content.ReadAsStringAsync();
            Assert.Fail($"Expected 202, got {(int)initRes.StatusCode}: {err}");
        }

        var initiated = await initRes.Content.ReadFromJsonAsync<InitiateFlexPayResponseDto>(JsonOptions);
        Assert.NotNull(initiated);
        Assert.True(initiated!.FlexPayAccepted);
        Assert.False(string.IsNullOrWhiteSpace(initiated.OrderNumberFlexPay));
        Assert.StartsWith("SP-", initiated.ReferenceFlexPay);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            Assert.Equal(0, await db.SouscriptionsPrestations.CountAsync(s =>
                s.AffilieId == affilieId && s.PrestationId == prestationId));
        }

        var callback = new FlexPayCallbackDto
        {
            Code = "0",
            OrderNumber = initiated.OrderNumberFlexPay,
            Reference = initiated.ReferenceFlexPay,
            Amount = initiated.MontantFlexPay.ToString(CultureInfo.InvariantCulture),
            Currency = initiated.CodeDevisePaiement,
            ProviderReference = "PRV-SUB-001",
            Channel = "AIRTEL"
        };

        var cbRes = await _client.PostAsJsonAsync("/api/FlexPay/callback", callback);
        Assert.Equal(HttpStatusCode.OK, cbRes.StatusCode);

        var json = await cbRes.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var result = json.GetProperty("result");
        Assert.True(result.GetProperty("success").GetBoolean());
        Assert.True(result.TryGetProperty("idCollecte", out var idCollecteEl)
                    && idCollecteEl.ValueKind != JsonValueKind.Null);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var sub = await db.SouscriptionsPrestations
                .SingleAsync(s => s.AffilieId == affilieId && s.PrestationId == prestationId && s.Statut);
            var collecte = await db.Collectes.SingleAsync(c =>
                c.IdCollecte == idCollecteEl.GetInt32());
            Assert.Equal(sub.IdSouscriptionPrestation, collecte.SouscriptionPrestationId);
            Assert.Equal(TypeCollecte.Souscription, collecte.TypeCollecte);
            Assert.Equal(CollecteStatutPaiement.Valide, collecte.StatutPaiement);
            Assert.Equal("MOBILE_MONEY", collecte.ModePaiement);
        }

        // Idempotence
        var cb2 = await _client.PostAsJsonAsync("/api/FlexPay/callback", callback);
        Assert.Equal(HttpStatusCode.OK, cb2.StatusCode);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            Assert.Equal(1, await db.SouscriptionsPrestations.CountAsync(s =>
                s.AffilieId == affilieId && s.PrestationId == prestationId));
            Assert.Equal(1, await db.Collectes.CountAsync(c =>
                c.SouscriptionPrestationId != null
                && c.TypeCollecte == TypeCollecte.Souscription
                && c.AffilieId == affilieId));
        }
    }

    [Fact]
    public async Task PaiementElectronique_DejaSouscrit_Retourne409()
    {
        var (agentId, affilieId, prestationId, deviseId) = await SeedEligibleAchatAsync();

        // Première souscription sync
        var sync = new SouscriptionPrestationAchatCreateDto
        {
            PrestationId = prestationId,
            Statut = true,
            Collecte = new SouscriptionPrestationCollecteCreateDto
            {
                AgentId = agentId,
                Montant = 5000m,
                DeviseId = deviseId,
                ModePaiement = "VIRTUAL_ACCOUNT",
                Mois = DateTime.Today.Month,
                Annee = DateTime.Today.Year
            }
        };
        var syncRes = await _client.PostAsJsonAsync($"/api/SouscriptionPrestation?affilieId={affilieId}", sync);
        if (syncRes.StatusCode != HttpStatusCode.Created)
        {
            var body = await syncRes.Content.ReadAsStringAsync();
            Assert.Fail($"Setup sync failed: {(int)syncRes.StatusCode} {body}");
        }

        var electronique = BuildElectroniqueDto(affilieId, agentId, prestationId, deviseId, "0825555666");
        var res = await _client.PostAsJsonAsync("/api/SouscriptionPrestation/paiement-electronique", electronique);
        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
    }

    private static SouscriptionPrestationPaiementElectroniqueCreateDto BuildElectroniqueDto(
        int affilieId, int agentId, int prestationId, int deviseId, string phone) => new()
    {
        AffilieId = affilieId,
        ModePaiement = "MOBILE_MONEY",
        TelephonePaiement = phone,
        DevisePaiementId = deviseId,
        Achat = new SouscriptionPrestationAchatCreateDto
        {
            PrestationId = prestationId,
            Statut = true,
            Collecte = new SouscriptionPrestationCollecteCreateDto
            {
                AgentId = agentId,
                Montant = 5000m,
                DeviseId = deviseId,
                ModePaiement = "MOBILE_MONEY",
                Mois = DateTime.Today.Month,
                Annee = DateTime.Today.Year
            }
        }
    };

    private async Task<(int AgentId, int AffilieId, int PrestationId, int DeviseId)> SeedEligibleAchatAsync()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        var categorieDto = new CreateCategorieAgentDto
        {
            Code = $"E{unique[..6]}",
            Description = "Cat test souscription électro"
        };
        var categorieRes = await _client.PostAsJsonAsync("/api/CategorieAgent", categorieDto);
        categorieRes.EnsureSuccessStatusCode();
        var categorie = await categorieRes.Content.ReadFromJsonAsync<CategorieAgentDto>(JsonOptions);

        var phoneSuffix = Math.Abs(unique.GetHashCode()) % 10_000;
        var agentDto = new AgentCreateDto
        {
            NomComplet = $"Agent Electro {unique}",
            Matricule = $"SE{unique.PadRight(9, '0')}"[..11],
            Phone = $"099905{phoneSuffix:D4}",
            CategorieAgentId = categorie!.IdCategorieAgent,
            Statut = true
        };
        var agentRes = await _client.PostAsJsonAsync("/api/Agent", agentDto);
        agentRes.EnsureSuccessStatusCode();
        var agent = await agentRes.Content.ReadFromJsonAsync<AgentReadDto>(JsonOptions);
        Assert.NotNull(agent?.WalletVirtuelId);

        await _client.PutAsJsonAsync(
            $"/api/WalletVirtuelAgent/{agent!.WalletVirtuelId}/ajouter-solde",
            new WalletVirtuelAgentAjouterSoldeDto { Montant = 500m });

        int cotisationAffilieId;
        int prestationId;
        int deviseId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            await FlexPayTestSeedHelper.EnsureMarchandActifAsync(db);

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
            (prestationId, deviseId) = await SeedPrestationCdfAsync(db);
        }

        var adhesionInput = new AdhesionWithAffilieCreateDto
        {
            Nom = $"Ele{unique}",
            Prenom = "Test",
            DateNaissance = new DateTime(1991, 2, 27),
            Telephone = $"+2439093{phoneSuffix:D5}"[..13],
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
                    DeviseId = deviseId,
                    Mois = DateTime.Today.Month,
                    Annee = DateTime.Today.Year,
                    ModePaiement = "VIRTUAL_ACCOUNT",
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

        var adhesion = await adhesionRes.Content.ReadFromJsonAsync<AdhesionWithAffilieReadDto>(JsonOptions);
        return (agent.Id, adhesion!.AffilieId, prestationId, deviseId);
    }

    private static async Task<(int PrestationId, int DeviseCdfId)> SeedPrestationCdfAsync(ProsocDbContext db)
    {
        var cdf = await db.Devises.FirstAsync(d => d.Code == "CDF");
        var unique = Guid.NewGuid().ToString("N")[..6];

        var produit = new ProduitMutuel
        {
            Nom = $"Produit elec {unique}",
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
            NomPrestation = $"Prestation elec {unique}",
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
