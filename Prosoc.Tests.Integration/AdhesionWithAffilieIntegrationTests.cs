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

public class AdhesionWithAffilieIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AdhesionWithAffilieIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static CollecteAvecSouscriptionDto CotisationAdhesionCollecte(int cotisationAffilieId, string referenceSuffix = "cot") =>
        new()
        {
            TypeCollecte = TypeCollecte.Cotisation,
            CotisationAffilieId = cotisationAffilieId,
            Montant = 1.5m,
            DeviseId = 1,
            StatutPaiement = "PAYE",
            Statut = true,
            ReferencePaiement = $"REF-COT-{referenceSuffix}",
            ModePaiement = "ESPECE",
            Mois = DateTime.Now.Month,
            Annee = DateTime.Now.Year
        };

    private static CollecteAvecSouscriptionDto SouscriptionCollecte(int prestationId, string referenceSuffix = "001") =>
        new()
        {
            TypeCollecte = TypeCollecte.Souscription,
            Montant = 5000m,
            DeviseId = 1,
            StatutPaiement = "PAYE",
            Statut = true,
            ReferencePaiement = $"REF-TEST-{referenceSuffix}",
            ModePaiement = "ESPECE",
            Souscription = new SouscriptionPrestationCreateDto
            {
                PrestationId = prestationId,
                DateSouscription = DateTime.Now,
                Statut = true
            }
        };

    private static async Task<(int PrestationId, int CotisationAffilieId)> SeedPrestationAndCotisationAsync(ProsocDbContext db)
    {
        var devise = await db.Devises.FirstAsync(d => d.Code == "CDF");

        var cotisationAffilie = await db.CotisationsAffilie
            .FirstOrDefaultAsync(c => c.TypeAdhesionId == 1 && c.Periodicite == "Mensuel");
        if (cotisationAffilie == null)
        {
            cotisationAffilie = new CotisationAffilie
            {
                Montant = 1.5m,
                Periodicite = "Mensuel",
                TypeAdhesionId = 1,
                DeviseId = devise.IdDevise,
                Statut = true
            };
            db.CotisationsAffilie.Add(cotisationAffilie);
            await db.SaveChangesAsync();
        }

        var unique = Guid.NewGuid().ToString("N")[..6];
        var produit = new ProduitMutuel
        {
            Nom = $"Produit test {unique}",
            Montant = 5000m,
            EstGratuit = false,
            Periodicite = "Mensuel",
            AgeMin = 0,
            AgeMax = 120,
            DeviseId = devise.IdDevise,
            Statut = true
        };
        db.ProduitsMutuels.Add(produit);
        await db.SaveChangesAsync();

        var prestation = new Prestation
        {
            NomPrestation = $"Prestation test {unique}",
            Montant = 5000,
            DeviseId = devise.IdDevise,
            ProduitMutuelId = produit.IdProduit,
            Statut = true
        };
        db.Prestations.Add(prestation);
        await db.SaveChangesAsync();
        return (prestation.IdPrestation, cotisationAffilie.IdCotisationAffilie);
    }

    private static List<CollecteAvecSouscriptionDto> CollectesAdhesionCompletes(
        int cotisationAffilieId,
        int prestationId,
        string referenceSuffix) =>
        new()
        {
            CotisationAdhesionCollecte(cotisationAffilieId, $"{referenceSuffix}-cot"),
            SouscriptionCollecte(prestationId, $"{referenceSuffix}-sub")
        };

    [Fact]
    public async Task CreateWithAffilie_CreatesBothAndLinks_AndRelationEndpointsWork()
    {
        int agentId;
        int prestationId;
        int cotisationAffilieId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

            var province = new Province { Nom = "P-Test", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();

            var commune = new Commune { Nom = "C-Test", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune);
            await db.SaveChangesAsync();

            var zone = new ZoneSociale { Nom = "Z-Test", CommuneId = commune.IdCommune, Statut = true };
            db.ZonesSociales.Add(zone);
            await db.SaveChangesAsync();

            var agent = new Agent
            {
                NomComplet = "Agent Test",
                Matricule = "MAT-001",
                Phone = "0999999999",
                ZoneSocialeId = zone.IdZoneSociale,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);

            await db.SaveChangesAsync();

            agentId = agent.IdAgent;
            (prestationId, cotisationAffilieId) = await SeedPrestationAndCotisationAsync(db);
        }

        var input = new AdhesionWithAffilieCreateDto
        {
            Nom = "Doe",
            Prenom = "John",
            DateNaissance = new DateTime(1990, 1, 1),
            Telephone = "0800000000",
            ProvinceResidence = "Kinshasa",
            CommuneResidence = "Gombe",
            QuartierResidence = "Centre",
            PhotoBase64 = "cGhvdG8=",
            PhotoContentType = "image/jpeg",
            CarteIdentiteBase64 = "Y2FydGU=",
            CarteIdentiteContentType = "image/jpeg",
            AffilieStatut = true,
            StatutDossier = "A",
            TypeAdhesionId = 1,
            AgentId = agentId,
            AdhesionStatut = true,
            Collectes = CollectesAdhesionCompletes(cotisationAffilieId, prestationId, "main")
        };

        var createRes = await _client.PostAsJsonAsync("/api/Adhesion/with-affilie", input);
        if (createRes.StatusCode != HttpStatusCode.Created)
        {
            var errorBody = await createRes.Content.ReadAsStringAsync();
            Assert.Fail($"Expected 201 Created but got {(int)createRes.StatusCode} {createRes.StatusCode}. Body: {errorBody}");
        }

        var created = await createRes.Content.ReadFromJsonAsync<AdhesionWithAffilieReadDto>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.True(created.AffilieId > 0);
        Assert.Equal(agentId, created.AgentId);
        Assert.False(string.IsNullOrWhiteSpace(created.CodeAdhesion));

        var affilieRes = await _client.GetAsync($"/api/Adhesion/{created.Id}/affilie");
        Assert.Equal(HttpStatusCode.OK, affilieRes.StatusCode);
        var affilie = await affilieRes.Content.ReadFromJsonAsync<AffilieReadDto>();
        Assert.NotNull(affilie);
        Assert.Equal(created.AffilieId, affilie!.IdAffilie);
        Assert.False(string.IsNullOrWhiteSpace(affilie.CodeAdhesion));

        var adhesionRes = await _client.GetAsync($"/api/Adhesion/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, adhesionRes.StatusCode);
        var adhesion = await adhesionRes.Content.ReadFromJsonAsync<AdhesionReadDto>();
        Assert.NotNull(adhesion);
        Assert.Equal(created.Id, adhesion!.Id);
        Assert.Equal(created.AffilieId, adhesion.AffilieId);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var collectes = db.Collectes.Where(c => c.AffilieId == created.AffilieId).ToList();
            Assert.Equal(2, collectes.Count);

            var collecte = collectes.First(c => c.TypeCollecte == TypeCollecte.Cotisation);
            Assert.Equal(agentId, collecte.AgentId);
            Assert.Equal(1.5m, collecte.Montant);
            Assert.Equal(1, collecte.DeviseId);
            Assert.True(collecte.DateCollecte > DateTime.MinValue);
        }
    }

    [Fact]
    public async Task CreateWithAffilie_SansPhotoNiCarte_Reussit()
    {
        int agentId;
        int prestationId;
        int cotisationAffilieId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

            var province = new Province { Nom = "P-Test-NoPhoto", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();

            var commune = new Commune { Nom = "C-Test-NoPhoto", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune);
            await db.SaveChangesAsync();

            var zone = new ZoneSociale { Nom = "Z-Test-NoPhoto", CommuneId = commune.IdCommune, Statut = true };
            db.ZonesSociales.Add(zone);
            await db.SaveChangesAsync();

            var agent = new Agent
            {
                NomComplet = "Agent No Photo",
                Matricule = $"MAT-NP-{Guid.NewGuid():N}"[..12],
                Phone = "0888888888",
                ZoneSocialeId = zone.IdZoneSociale,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            agentId = agent.IdAgent;
            (prestationId, cotisationAffilieId) = await SeedPrestationAndCotisationAsync(db);
        }

        var input = new AdhesionWithAffilieCreateDto
        {
            Nom = "SansPhoto",
            Prenom = "Test",
            DateNaissance = new DateTime(1992, 3, 4),
            Telephone = $"07{Guid.NewGuid():N}"[..10],
            ProvinceResidence = "Kinshasa",
            CommuneResidence = "Gombe",
            QuartierResidence = "Centre",
            AffilieStatut = true,
            StatutDossier = "A",
            TypeAdhesionId = 1,
            AgentId = agentId,
            AdhesionStatut = true,
            Collectes = new List<CollecteAvecSouscriptionDto>
            {
                new()
                {
                    TypeCollecte = TypeCollecte.Cotisation,
                    CotisationAffilieId = cotisationAffilieId,
                    Montant = 1.5m,
                    DeviseId = 1,
                    StatutPaiement = "PAYE",
                    Statut = true,
                    ReferencePaiement = "REF-COT-nophoto",
                    ModePaiement = "ESPECE",
                    Mois = DateTime.Now.Month,
                    Annee = DateTime.Now.Year
                }
            }
        };

        var createRes = await _client.PostAsJsonAsync("/api/Adhesion/with-affilie", input);
        if (createRes.StatusCode != HttpStatusCode.Created)
        {
            var errorBody = await createRes.Content.ReadAsStringAsync();
            Assert.Fail($"Expected 201 Created but got {(int)createRes.StatusCode} {createRes.StatusCode}. Body: {errorBody}");
        }

        var created = await createRes.Content.ReadFromJsonAsync<AdhesionWithAffilieReadDto>();
        Assert.NotNull(created);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var affilie = await db.Affilies.FindAsync(created!.AffilieId);
            Assert.NotNull(affilie);
            Assert.True(affilie!.PhotoData == null || affilie.PhotoData.Length == 0);
            Assert.True(affilie.CarteIdentiteData == null || affilie.CarteIdentiteData.Length == 0);
        }
    }

    [Fact]
    public async Task CreateWithAffilie_WhenAdhesionAlreadyExistsForDetectedAffilie_Returns409Conflict()
    {
        int agentId;
        int prestationId;
        int cotisationAffilieId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

            var province = new Province { Nom = "P-Test-409", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();

            var commune = new Commune { Nom = "C-Test-409", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune);
            await db.SaveChangesAsync();

            var zone = new ZoneSociale { Nom = "Z-Test-409", CommuneId = commune.IdCommune, Statut = true };
            db.ZonesSociales.Add(zone);
            await db.SaveChangesAsync();

            var agent = new Agent
            {
                NomComplet = "Agent Test 409",
                Matricule = "MAT-409",
                Phone = "0900000409",
                ZoneSocialeId = zone.IdZoneSociale,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();
            agentId = agent.IdAgent;
            (prestationId, cotisationAffilieId) = await SeedPrestationAndCotisationAsync(db);
        }

        var uniqueKey = Guid.NewGuid().ToString("N")[..8];
        var input = new AdhesionWithAffilieCreateDto
        {
            Nom = $"Conflict{uniqueKey}",
            Prenom = "Affilie",
            DateNaissance = new DateTime(1991, 2, 3),
            Telephone = $"08{uniqueKey}",
            ProvinceResidence = "Kinshasa",
            CommuneResidence = "Gombe",
            QuartierResidence = "Centre",
            PhotoBase64 = "cGhvdG8=",
            PhotoContentType = "image/jpeg",
            CarteIdentiteBase64 = "Y2FydGU=",
            CarteIdentiteContentType = "image/jpeg",
            AffilieStatut = true,
            StatutDossier = "A",
            TypeAdhesionId = 1,
            AgentId = agentId,
            AdhesionStatut = true,
            Collectes = CollectesAdhesionCompletes(cotisationAffilieId, prestationId, "409-first")
        };

        var first = await _client.PostAsJsonAsync("/api/Adhesion/with-affilie", input);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        input.Collectes = CollectesAdhesionCompletes(cotisationAffilieId, prestationId, "409-second");
        input.Telephone = $"08{uniqueKey}9";
        var second = await _client.PostAsJsonAsync("/api/Adhesion/with-affilie", input);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task CreateWithAffilie_WhenAffilieExistsWithoutAdhesion_ReusesAffilie()
    {
        int agentId;
        int existingAffilieId;
        int prestationId;
        int cotisationAffilieId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

            var province = new Province { Nom = "P-Test-Reuse", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();

            var commune = new Commune { Nom = "C-Test-Reuse", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune);
            await db.SaveChangesAsync();

            var zone = new ZoneSociale { Nom = "Z-Test-Reuse", CommuneId = commune.IdCommune, Statut = true };
            db.ZonesSociales.Add(zone);
            await db.SaveChangesAsync();

            var agent = new Agent
            {
                NomComplet = "Agent Test Reuse",
                Matricule = "MAT-REUSE",
                Phone = "0900000999",
                ZoneSocialeId = zone.IdZoneSociale,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();
            agentId = agent.IdAgent;

            var affilie = new Affilie
            {
                CodeAdhesion = "TEMP-REUSE",
                Nom = "Reuse",
                Prenom = "Affilie",
                Postnom = "X",
                NomComplet = "Affilie X Reuse",
                DateNaissance = new DateTime(1992, 4, 5),
                Telephone = "0800000111",
                ProvinceResidence = "Kinshasa",
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();
            existingAffilieId = affilie.IdAffilie;
            (prestationId, cotisationAffilieId) = await SeedPrestationAndCotisationAsync(db);
        }

        var input = new AdhesionWithAffilieCreateDto
        {
            Nom = "Reuse",
            Prenom = "Affilie",
            Postnom = "Y",
            DateNaissance = new DateTime(1992, 4, 5),
            Telephone = "0800000222",
            ProvinceResidence = "Kinshasa",
            CommuneResidence = "Gombe",
            QuartierResidence = "Centre",
            PhotoBase64 = "cGhvdG8=",
            PhotoContentType = "image/jpeg",
            CarteIdentiteBase64 = "Y2FydGU=",
            CarteIdentiteContentType = "image/jpeg",
            AffilieStatut = true,
            StatutDossier = "A",
            TypeAdhesionId = 1,
            AgentId = agentId,
            AdhesionStatut = true,
            Collectes = CollectesAdhesionCompletes(cotisationAffilieId, prestationId, "reuse")
        };

        var res = await _client.PostAsJsonAsync("/api/Adhesion/with-affilie", input);
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        var created = await res.Content.ReadFromJsonAsync<AdhesionWithAffilieReadDto>();
        Assert.NotNull(created);
        Assert.Equal(existingAffilieId, created!.AffilieId);
        Assert.False(string.IsNullOrWhiteSpace(created.CodeAdhesion));

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var affilieCount = db.Affilies.Count(a => a.Nom == "Reuse" && a.Prenom == "Affilie");
            Assert.Equal(1, affilieCount);
        }
    }

    [Fact]
    public async Task CreateWithAffilie_WithMultipleSouscriptions_CreatesSouscriptions()
    {
        int agentId;
        int prestation1Id;
        int prestation2Id;
        int cotisationAffilieId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();

            var province = new Province { Nom = "P-Test-Sub", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();

            var commune = new Commune { Nom = "C-Test-Sub", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune);
            await db.SaveChangesAsync();

            var zone = new ZoneSociale { Nom = "Z-Test-Sub", CommuneId = commune.IdCommune, Statut = true };
            db.ZonesSociales.Add(zone);
            await db.SaveChangesAsync();

            var agent = new Agent
            {
                NomComplet = "Agent Test Sub",
                Matricule = "MAT-SUB",
                Phone = "0900000123",
                ZoneSocialeId = zone.IdZoneSociale,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();
            agentId = agent.IdAgent;

            (prestation1Id, cotisationAffilieId) = await SeedPrestationAndCotisationAsync(db);
            (prestation2Id, _) = await SeedPrestationAndCotisationAsync(db);
        }

        var input = new AdhesionWithAffilieCreateDto
        {
            Nom = "Subs",
            Prenom = "Multi",
            DateNaissance = new DateTime(1988, 8, 8),
            Telephone = "0800000777",
            ProvinceResidence = "Kinshasa",
            CommuneResidence = "Gombe",
            QuartierResidence = "Centre",
            PhotoBase64 = "cGhvdG8=",
            PhotoContentType = "image/jpeg",
            CarteIdentiteBase64 = "Y2FydGU=",
            CarteIdentiteContentType = "image/jpeg",
            AffilieStatut = true,
            StatutDossier = "A",
            TypeAdhesionId = 1,
            AgentId = agentId,
            AdhesionStatut = true,
            Collectes = new List<CollecteAvecSouscriptionDto>
            {
                CotisationAdhesionCollecte(cotisationAffilieId, "multi-cot"),
                SouscriptionCollecte(prestation1Id, "multi-1"),
                SouscriptionCollecte(prestation2Id, "multi-2")
            }
        };

        var res = await _client.PostAsJsonAsync("/api/Adhesion/with-affilie", input);
        if (res.StatusCode != HttpStatusCode.Created)
        {
            var body = await res.Content.ReadAsStringAsync();
            Assert.Fail($"Expected 201 but got {(int)res.StatusCode}. Body: {body}");
        }

        var created = await res.Content.ReadFromJsonAsync<AdhesionWithAffilieReadDto>();
        Assert.NotNull(created);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var subs = db.SouscriptionsPrestations.Where(s => s.AffilieId == created!.AffilieId).ToList();
            Assert.Equal(2, subs.Count);
            Assert.Contains(subs, s => s.PrestationId == prestation1Id);
            Assert.Contains(subs, s => s.PrestationId == prestation2Id);
        }
    }

    private static CollecteAvecSouscriptionDto FraisVirtualAccountCollecte(int fraisId, int deviseId, decimal montant) =>
        new()
        {
            TypeCollecte = TypeCollecte.Frais,
            FraisId = fraisId,
            Montant = montant,
            DeviseId = deviseId,
            StatutPaiement = "OK",
            Statut = true,
            ModePaiement = "VIRTUAL_ACCOUNT",
            Mois = DateTime.Now.Month,
            Annee = DateTime.Now.Year,
            MontantRecu = montant,
            MontantAttendu = montant
        };

    private static CollecteAvecSouscriptionDto SouscriptionVirtualAccountCollecte(int prestationId, int deviseId, decimal montant) =>
        new()
        {
            TypeCollecte = TypeCollecte.Souscription,
            Montant = montant,
            DeviseId = deviseId,
            StatutPaiement = "OK",
            Statut = true,
            ModePaiement = "VIRTUAL_ACCOUNT",
            Mois = DateTime.Now.Month,
            Annee = DateTime.Now.Year,
            MontantRecu = montant,
            MontantAttendu = montant,
            Souscription = new SouscriptionPrestationCreateDto
            {
                PrestationId = prestationId,
                DateSouscription = DateTime.UtcNow,
                Statut = true
            }
        };

    private async Task<(int AgentId, int WalletVirtuelId)> CreateAgentWithFundedWalletAsync()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        var categorieDto = new CreateCategorieAgentDto
        {
            Code = $"A{unique[..6]}",
            Description = "Catégorie test adhésion"
        };

        var categorieResponse = await _client.PostAsJsonAsync("/api/CategorieAgent", categorieDto);
        categorieResponse.EnsureSuccessStatusCode();
        var createdCategorie = await categorieResponse.Content.ReadFromJsonAsync<CategorieAgentDto>();
        Assert.NotNull(createdCategorie);

        var phoneSuffix = Math.Abs(unique.GetHashCode()) % 10_000;
        var agentDto = new AgentCreateDto
        {
            NomComplet = $"Agent Adh {unique}",
            Matricule = $"AD{unique.PadRight(9, '0')}"[..11],
            Phone = $"099901{phoneSuffix:D4}",
            CategorieAgentId = createdCategorie!.IdCategorieAgent,
            Statut = true
        };

        var agentResponse = await _client.PostAsJsonAsync("/api/Agent", agentDto);
        agentResponse.EnsureSuccessStatusCode();
        var createdAgent = await agentResponse.Content.ReadFromJsonAsync<AgentReadDto>();
        Assert.NotNull(createdAgent);
        Assert.NotNull(createdAgent!.WalletVirtuelId);

        var fundResponse = await _client.PutAsJsonAsync(
            $"/api/WalletVirtuelAgent/{createdAgent.WalletVirtuelId}/ajouter-solde",
            new WalletVirtuelAgentAjouterSoldeDto { Montant = 500m });
        fundResponse.EnsureSuccessStatusCode();

        return (createdAgent.Id, createdAgent.WalletVirtuelId!.Value);
    }

    /// <summary>
    /// VIRTUAL_ACCOUNT est réservé aux rôles terrain (AT, Chef d'équipe, Superviseur, Percepteur).
    /// </summary>
    private async Task<HttpResponseMessage> PostWithAffilieAsVirtualAccountCallerAsync(AdhesionWithAffilieCreateDto input)
    {
        var previousRoles = TestAuthHandler.Roles;
        try
        {
            TestAuthHandler.Roles = new[] { "Agent (AT)" };
            return await _client.PostAsJsonAsync("/api/Adhesion/with-affilie", input);
        }
        finally
        {
            TestAuthHandler.Roles = previousRoles;
        }
    }

    private static async Task<(int PrestationId, int FraisId, int DeviseId)> SeedFraisEtPrestationUsdAsync(ProsocDbContext db)
    {
        var deviseUsd = await db.Devises.FirstAsync(d => d.Code == "USD");
        var frais = await db.Frais.FirstAsync(f => f.Code == FraisCodes.FraisAdhesion && f.Statut);

        var unique = Guid.NewGuid().ToString("N")[..6];
        var produit = new ProduitMutuel
        {
            Nom = $"Produit adh USD {unique}",
            Montant = 10m,
            EstGratuit = false,
            Periodicite = "Mensuel",
            AgeMin = 0,
            AgeMax = 120,
            DeviseId = deviseUsd.IdDevise,
            Statut = true
        };
        db.ProduitsMutuels.Add(produit);
        await db.SaveChangesAsync();

        var prestation = new Prestation
        {
            NomPrestation = $"Prestation adh USD {unique}",
            Montant = 10,
            DeviseId = deviseUsd.IdDevise,
            ProduitMutuelId = produit.IdProduit,
            Statut = true
        };
        db.Prestations.Add(prestation);
        await db.SaveChangesAsync();

        return (prestation.IdPrestation, frais.IdFrais, deviseUsd.IdDevise);
    }

    [Fact]
    public async Task CreateWithAffilie_FraisSeuls_Reussit()
    {
        var (agentId, _) = await CreateAgentWithFundedWalletAsync();
        int fraisId;
        int deviseId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (_, fraisId, deviseId) = await SeedFraisEtPrestationUsdAsync(db);
        }

        var montant = 1.5m;
        var input = new AdhesionWithAffilieCreateDto
        {
            Nom = "FraisSeul",
            Prenom = "Test",
            Postnom = "Adh",
            DateNaissance = new DateTime(1991, 2, 27),
            Telephone = $"099903{Math.Abs(Guid.NewGuid().GetHashCode()) % 10_000:D4}",
            EmailAffilie = $"frais{Guid.NewGuid():N}"[..20] + "@example.com",
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
            AgentId = agentId,
            AdhesionStatut = true,
            Collectes = new List<CollecteAvecSouscriptionDto>
            {
                new()
                {
                    TypeCollecte = TypeCollecte.Frais,
                    FraisId = fraisId,
                    Montant = montant,
                    DeviseId = deviseId,
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    Statut = true,
                    ModePaiement = "ESPECE",
                    ReferencePaiement = $"REF-FRAIS-{Guid.NewGuid():N}"[..24],
                    Mois = DateTime.Now.Month,
                    Annee = DateTime.Now.Year,
                    MontantRecu = montant,
                    MontantAttendu = montant
                }
            }
        };

        var createRes = await _client.PostAsJsonAsync("/api/Adhesion/with-affilie", input);
        if (createRes.StatusCode != HttpStatusCode.Created)
        {
            var errorBody = await createRes.Content.ReadAsStringAsync();
            Assert.Fail($"Expected 201 Created but got {(int)createRes.StatusCode} {createRes.StatusCode}. Body: {errorBody}");
        }

        var created = await createRes.Content.ReadFromJsonAsync<AdhesionWithAffilieReadDto>();
        Assert.NotNull(created);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var collectes = db.Collectes.Where(c => c.AffilieId == created!.AffilieId).ToList();
            Assert.Single(collectes);
            Assert.Equal(TypeCollecte.Frais, collectes[0].TypeCollecte);
            Assert.Empty(db.SouscriptionsPrestations.Where(s => s.AffilieId == created!.AffilieId));
        }
    }

    [Fact]
    public async Task CreateWithAffilie_FraisEtSouscriptionSansCotisation_VirtualAccount_Reussit()
    {
        var (agentId, _) = await CreateAgentWithFundedWalletAsync();
        int prestationId;
        int fraisId;
        int deviseId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (prestationId, fraisId, deviseId) = await SeedFraisEtPrestationUsdAsync(db);
        }

        var input = new AdhesionWithAffilieCreateDto
        {
            Nom = "Mukobo",
            Prenom = "Nelson",
            Postnom = "Test",
            DateNaissance = new DateTime(1991, 2, 27),
            Telephone = $"099902{Math.Abs(Guid.NewGuid().GetHashCode()) % 10_000:D4}",
            EmailAffilie = $"test{Guid.NewGuid():N}"[..20] + "@example.com",
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
            AgentId = agentId,
            AdhesionStatut = true,
            Collectes = new List<CollecteAvecSouscriptionDto>
            {
                FraisVirtualAccountCollecte(fraisId, deviseId, 1.5m),
                SouscriptionVirtualAccountCollecte(prestationId, deviseId, 10m)
            }
        };

        var createRes = await PostWithAffilieAsVirtualAccountCallerAsync(input);
        if (createRes.StatusCode != HttpStatusCode.Created)
        {
            var errorBody = await createRes.Content.ReadAsStringAsync();
            Assert.Fail($"Expected 201 Created but got {(int)createRes.StatusCode} {createRes.StatusCode}. Body: {errorBody}");
        }

        var created = await createRes.Content.ReadFromJsonAsync<AdhesionWithAffilieReadDto>();
        Assert.NotNull(created);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var collectes = db.Collectes.Where(c => c.AffilieId == created!.AffilieId).ToList();
            Assert.Equal(2, collectes.Count);
            Assert.DoesNotContain(collectes, c => c.TypeCollecte == TypeCollecte.Cotisation);

            var souscriptionCollecte = collectes.Single(c => c.TypeCollecte == TypeCollecte.Souscription);
            Assert.NotNull(souscriptionCollecte.SouscriptionPrestationId);
            Assert.True(souscriptionCollecte.SouscriptionPrestationId > 0);

            var souscription = await db.SouscriptionsPrestations.FindAsync(souscriptionCollecte.SouscriptionPrestationId);
            Assert.NotNull(souscription);
            Assert.Equal(prestationId, souscription!.PrestationId);
        }
    }

    [Fact]
    public async Task CreateWithAffilie_SouscriptionSansPrestationId_RetourneErreurExplicite()
    {
        var (agentId, _) = await CreateAgentWithFundedWalletAsync();
        int fraisId;
        int deviseId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (_, fraisId, deviseId) = await SeedFraisEtPrestationUsdAsync(db);
        }

        var input = new AdhesionWithAffilieCreateDto
        {
            Nom = "Sans",
            Prenom = "Prestation",
            DateNaissance = new DateTime(1990, 5, 5),
            Telephone = $"099903{Math.Abs(Guid.NewGuid().GetHashCode()) % 10_000:D4}",
            ProvinceResidence = "Kinshasa",
            CommuneResidence = "Gombe",
            QuartierResidence = "Centre",
            PhotoBase64 = "cGhvdG8=",
            PhotoContentType = "image/png",
            CarteIdentiteBase64 = "Y2FydGU=",
            CarteIdentiteContentType = "image/png",
            AffilieStatut = true,
            StatutDossier = "COMPLET",
            TypeAdhesionId = 1,
            AgentId = agentId,
            AdhesionStatut = true,
            Collectes = new List<CollecteAvecSouscriptionDto>
            {
                FraisVirtualAccountCollecte(fraisId, deviseId, 1.5m),
                new()
                {
                    TypeCollecte = TypeCollecte.Souscription,
                    Montant = 10m,
                    DeviseId = deviseId,
                    StatutPaiement = "OK",
                    Statut = true,
                    ModePaiement = "VIRTUAL_ACCOUNT",
                    Mois = DateTime.Now.Month,
                    Annee = DateTime.Now.Year,
                    Souscription = null
                }
            }
        };

        var createRes = await _client.PostAsJsonAsync("/api/Adhesion/with-affilie", input);
        var body = await createRes.Content.ReadAsStringAsync();

        Assert.NotEqual(HttpStatusCode.Created, createRes.StatusCode);
        Assert.Contains("prestationId", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateWithAffilie_WithPersonneContact_PersistsAndReturnsInFicheEncodeur()
    {
        var (agentId, _) = await CreateAgentWithFundedWalletAsync();
        int prestationId;
        int fraisId;
        int deviseId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (prestationId, fraisId, deviseId) = await SeedFraisEtPrestationUsdAsync(db);
        }

        var input = new AdhesionWithAffilieCreateDto
        {
            Nom = "Kabila",
            Prenom = "Marie",
            DateNaissance = new DateTime(1992, 3, 15),
            Telephone = $"099904{Math.Abs(Guid.NewGuid().GetHashCode()) % 10_000:D4}",
            ProvinceResidence = "Kinshasa",
            CommuneResidence = "Selembao",
            QuartierResidence = "Sans-fil",
            PhotoBase64 = "cGhvdG8=",
            PhotoContentType = "image/png",
            CarteIdentiteBase64 = "Y2FydGU=",
            CarteIdentiteContentType = "image/png",
            AffilieStatut = true,
            StatutDossier = "EN ATTENTE",
            TypeAdhesionId = 1,
            AgentId = agentId,
            AdhesionStatut = true,
            PersonneContact = new PersonneContactCreateDto
            {
                NomComplet = "Jean Kabila",
                LienParente = "EPOUSE",
                Adresse = "Kinshasa, Selembao, av. Lukunga 12"
            },
            Collectes = new List<CollecteAvecSouscriptionDto>
            {
                FraisVirtualAccountCollecte(fraisId, deviseId, 1.5m),
                SouscriptionVirtualAccountCollecte(prestationId, deviseId, 10m)
            }
        };

        var createRes = await PostWithAffilieAsVirtualAccountCallerAsync(input);
        if (createRes.StatusCode != HttpStatusCode.Created)
        {
            var errorBody = await createRes.Content.ReadAsStringAsync();
            Assert.Fail($"Expected 201 Created but got {(int)createRes.StatusCode} {createRes.StatusCode}. Body: {errorBody}");
        }

        var created = await createRes.Content.ReadFromJsonAsync<AdhesionWithAffilieReadDto>();
        Assert.NotNull(created);
        Assert.NotNull(created!.PersonneContact);
        Assert.Equal("Jean Kabila", created.PersonneContact!.NomComplet);
        Assert.Equal("EPOUSE", created.PersonneContact.LienParente);

        var ficheRes = await _client.GetAsync($"/api/Adhesion/{created.Id}/fiche-encodeur");
        Assert.Equal(HttpStatusCode.OK, ficheRes.StatusCode);
        var fiche = await ficheRes.Content.ReadFromJsonAsync<AdhesionFicheEncodeurReadDto>();
        Assert.NotNull(fiche?.PersonneContact);
        Assert.Equal("Jean Kabila", fiche!.PersonneContact!.NomComplet);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var contact = await db.PersonnesContact
                .FirstOrDefaultAsync(p => p.AffilieId == created.AffilieId);
            Assert.NotNull(contact);
            Assert.Equal("Jean Kabila", contact!.NomComplet);
        }
    }

    [Fact]
    public async Task CreateWithAffilie_WithPartialPersonneContact_Returns400()
    {
        var (agentId, _) = await CreateAgentWithFundedWalletAsync();
        int prestationId;
        int fraisId;
        int deviseId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (prestationId, fraisId, deviseId) = await SeedFraisEtPrestationUsdAsync(db);
        }

        var input = new AdhesionWithAffilieCreateDto
        {
            Nom = "Test",
            Prenom = "Contact",
            DateNaissance = new DateTime(1990, 1, 1),
            Telephone = $"099905{Math.Abs(Guid.NewGuid().GetHashCode()) % 10_000:D4}",
            ProvinceResidence = "Kinshasa",
            CommuneResidence = "Gombe",
            QuartierResidence = "Centre",
            AffilieStatut = true,
            StatutDossier = "EN ATTENTE",
            TypeAdhesionId = 1,
            AgentId = agentId,
            AdhesionStatut = true,
            PersonneContact = new PersonneContactCreateDto
            {
                NomComplet = "Contact Incomplet"
            },
            Collectes = new List<CollecteAvecSouscriptionDto>
            {
                FraisVirtualAccountCollecte(fraisId, deviseId, 1.5m),
                SouscriptionVirtualAccountCollecte(prestationId, deviseId, 10m)
            }
        };

        var createRes = await _client.PostAsJsonAsync("/api/Adhesion/with-affilie", input);
        var body = await createRes.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, createRes.StatusCode);
        Assert.Contains("lien de parenté", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateWithAffilie_WithoutPersonneContact_NoContactInDatabase()
    {
        var (agentId, _) = await CreateAgentWithFundedWalletAsync();
        int prestationId;
        int fraisId;
        int deviseId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (prestationId, fraisId, deviseId) = await SeedFraisEtPrestationUsdAsync(db);
        }

        var input = new AdhesionWithAffilieCreateDto
        {
            Nom = "Sans",
            Prenom = "Contact",
            DateNaissance = new DateTime(1988, 6, 10),
            Telephone = $"099906{Math.Abs(Guid.NewGuid().GetHashCode()) % 10_000:D4}",
            ProvinceResidence = "Kinshasa",
            CommuneResidence = "Gombe",
            QuartierResidence = "Centre",
            AffilieStatut = true,
            StatutDossier = "EN ATTENTE",
            TypeAdhesionId = 1,
            AgentId = agentId,
            AdhesionStatut = true,
            Collectes = new List<CollecteAvecSouscriptionDto>
            {
                FraisVirtualAccountCollecte(fraisId, deviseId, 1.5m),
                SouscriptionVirtualAccountCollecte(prestationId, deviseId, 10m)
            }
        };

        var createRes = await PostWithAffilieAsVirtualAccountCallerAsync(input);
        createRes.EnsureSuccessStatusCode();

        var created = await createRes.Content.ReadFromJsonAsync<AdhesionWithAffilieReadDto>();
        Assert.NotNull(created);
        Assert.Null(created!.PersonneContact);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var hasContact = await db.PersonnesContact.AnyAsync(p => p.AffilieId == created.AffilieId);
            Assert.False(hasContact);
        }
    }

    [Fact]
    public async Task GetById_ReturnsEmailAffilieInAffilieBlock()
    {
        var (agentId, _) = await CreateAgentWithFundedWalletAsync();
        int prestationId;
        int fraisId;
        int deviseId;
        var email = $"getbyid{Guid.NewGuid():N}"[..22] + "@example.com";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (prestationId, fraisId, deviseId) = await SeedFraisEtPrestationUsdAsync(db);
        }

        var input = new AdhesionWithAffilieCreateDto
        {
            Nom = "Email",
            Prenom = "Test",
            DateNaissance = new DateTime(1985, 8, 20),
            Telephone = $"099907{Math.Abs(Guid.NewGuid().GetHashCode()) % 10_000:D4}",
            EmailAffilie = email,
            ProvinceResidence = "Kinshasa",
            CommuneResidence = "Gombe",
            QuartierResidence = "Centre",
            AffilieStatut = true,
            StatutDossier = "EN ATTENTE",
            TypeAdhesionId = 1,
            AgentId = agentId,
            AdhesionStatut = true,
            Collectes = new List<CollecteAvecSouscriptionDto>
            {
                FraisVirtualAccountCollecte(fraisId, deviseId, 1.5m),
                SouscriptionVirtualAccountCollecte(prestationId, deviseId, 10m)
            }
        };

        var createRes = await PostWithAffilieAsVirtualAccountCallerAsync(input);
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<AdhesionWithAffilieReadDto>();
        Assert.NotNull(created);

        var getRes = await _client.GetAsync($"/api/Adhesion/{created!.Id}");
        getRes.EnsureSuccessStatusCode();
        var adhesion = await getRes.Content.ReadFromJsonAsync<AdhesionWithAffilieReadDto>();
        Assert.NotNull(adhesion);
        Assert.NotNull(adhesion!.Affilie);
        Assert.Equal(email, adhesion.Affilie.EmailAffilie);

        var affilieRes = await _client.GetAsync($"/api/Adhesion/{created.Id}/affilie");
        affilieRes.EnsureSuccessStatusCode();
        var affilie = await affilieRes.Content.ReadFromJsonAsync<AffilieReadDto>();
        Assert.NotNull(affilie);
        Assert.Equal(email, affilie!.EmailAffilie);
    }

    [Fact]
    public async Task Niveau2Encodeur_ValiderSansAdresseActivite_Retourne400()
    {
        TestAuthHandler.Roles = new[] { "Admin", "SuperAdmin" };
        var (agentId, _) = await CreateAgentWithFundedWalletAsync();
        int fraisId;
        int deviseId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (_, fraisId, deviseId) = await SeedFraisEtPrestationUsdAsync(db);
        }

        var montant = 1.5m;
        var createInput = new AdhesionWithAffilieCreateDto
        {
            Nom = "N2SansAct",
            Prenom = "Test",
            DateNaissance = new DateTime(1991, 2, 27),
            Telephone = $"099907{Math.Abs(Guid.NewGuid().GetHashCode()) % 10_000:D4}",
            ProvinceResidence = "Kinshasa",
            AffilieStatut = true,
            StatutDossier = AdhesionNiveau2Regles.StatutEnAttente,
            TypeAdhesionId = 1,
            AgentId = agentId,
            AdhesionStatut = true,
            Collectes = new List<CollecteAvecSouscriptionDto>
            {
                new()
                {
                    TypeCollecte = TypeCollecte.Frais,
                    FraisId = fraisId,
                    Montant = montant,
                    DeviseId = deviseId,
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    Statut = true,
                    ModePaiement = "ESPECE",
                    ReferencePaiement = $"REF-N2A-{Guid.NewGuid():N}"[..24],
                    Mois = DateTime.Now.Month,
                    Annee = DateTime.Now.Year,
                    MontantRecu = montant,
                    MontantAttendu = montant
                }
            }
        };

        var createRes = await _client.PostAsJsonAsync("/api/Adhesion/with-affilie", createInput);
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<AdhesionWithAffilieReadDto>();
        Assert.NotNull(created);

        var n2 = new AdhesionNiveau2EncodeurDto
        {
            Valider = true,
            PersonneContact = new PersonneContactNiveau2Dto
            {
                NomComplet = "Contact N2",
                LienParente = "AMI",
                Adresse = "Kinshasa"
            },
            PhotoBase64 = "cGhvdG8=",
            PhotoContentType = "image/png",
            CarteIdentiteBase64 = "Y2FydGU=",
            CarteIdentiteContentType = "image/png"
        };

        var n2Res = await _client.PutAsJsonAsync($"/api/Adhesion/{created!.Id}/niveau-2-encodeur", n2);
        var body = await n2Res.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.BadRequest, n2Res.StatusCode);
        Assert.Contains("activité", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Niveau2Encodeur_QuatreBlocsComplets_PasseAValide()
    {
        TestAuthHandler.Roles = new[] { "Admin", "SuperAdmin" };
        var (agentId, _) = await CreateAgentWithFundedWalletAsync();
        int fraisId;
        int deviseId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (_, fraisId, deviseId) = await SeedFraisEtPrestationUsdAsync(db);
        }

        var montant = 1.5m;
        var createInput = new AdhesionWithAffilieCreateDto
        {
            Nom = "N2Complet",
            Prenom = "Ok",
            DateNaissance = new DateTime(1990, 4, 12),
            Telephone = $"099908{Math.Abs(Guid.NewGuid().GetHashCode()) % 10_000:D4}",
            ProvinceResidence = "Kinshasa",
            AffilieStatut = true,
            StatutDossier = AdhesionNiveau2Regles.StatutEnAttente,
            TypeAdhesionId = 1,
            AgentId = agentId,
            AdhesionStatut = true,
            Collectes = new List<CollecteAvecSouscriptionDto>
            {
                new()
                {
                    TypeCollecte = TypeCollecte.Frais,
                    FraisId = fraisId,
                    Montant = montant,
                    DeviseId = deviseId,
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    Statut = true,
                    ModePaiement = "ESPECE",
                    ReferencePaiement = $"REF-N2C-{Guid.NewGuid():N}"[..24],
                    Mois = DateTime.Now.Month,
                    Annee = DateTime.Now.Year,
                    MontantRecu = montant,
                    MontantAttendu = montant
                }
            }
        };

        var createRes = await _client.PostAsJsonAsync("/api/Adhesion/with-affilie", createInput);
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<AdhesionWithAffilieReadDto>();
        Assert.NotNull(created);

        var n2 = new AdhesionNiveau2EncodeurDto
        {
            Valider = true,
            PersonneContact = new PersonneContactNiveau2Dto
            {
                NomComplet = "Contact Complet",
                LienParente = "AMI",
                Adresse = "Kinshasa, Gombe"
            },
            CommuneActivite = "Gombe",
            QuartierActivite = "Centre",
            AvenueActivite = "av. Commerce",
            NumeroActivite = "10",
            PhotoBase64 = "cGhvdG8=",
            PhotoContentType = "image/png",
            CarteIdentiteBase64 = "Y2FydGU=",
            CarteIdentiteContentType = "image/png"
        };

        var n2Res = await _client.PutAsJsonAsync($"/api/Adhesion/{created!.Id}/niveau-2-encodeur", n2);
        if (n2Res.StatusCode != HttpStatusCode.OK)
        {
            var errorBody = await n2Res.Content.ReadAsStringAsync();
            Assert.Fail($"Expected 200 but got {(int)n2Res.StatusCode}. Body: {errorBody}");
        }

        var result = await n2Res.Content.ReadFromJsonAsync<AdhesionNiveau2EncodeurReadDto>();
        Assert.NotNull(result);
        Assert.Equal(AdhesionNiveau2Regles.StatutValide, result!.StatutDossier);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var affilie = await db.Affilies.FirstAsync(a => a.IdAffilie == created.AffilieId);
            Assert.Equal("Gombe", affilie.CommuneActivite);
            Assert.Equal("Centre", affilie.QuartierActivite);
        }
    }
}
