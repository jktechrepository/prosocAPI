using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.DTOs.FlexPay;

namespace Prosoc.Tests.Integration.FlexPay;

public class FlexPayCallbackIntegrationTests : IClassFixture<FlexPayWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FlexPayWebApplicationFactory _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public FlexPayCallbackIntegrationTests(FlexPayWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static async Task<(bool Success, bool AlreadyProcessed, int? IdCollecte, int? IdAdhesion)>
        ParseCallbackResponseAsync(HttpResponseMessage res)
    {
        var json = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var result = json.GetProperty("result");
        return (
            result.GetProperty("success").GetBoolean(),
            result.TryGetProperty("alreadyProcessed", out var ap) && ap.GetBoolean(),
            result.TryGetProperty("idCollecte", out var ic) && ic.ValueKind != JsonValueKind.Null
                ? ic.GetInt32()
                : null,
            result.TryGetProperty("idAdhesion", out var ia) && ia.ValueKind != JsonValueKind.Null
                ? ia.GetInt32()
                : null);
    }

    [Fact]
    public async Task Callback_CodeZero_CreeCollecte()
    {
        int affilieId, agentId, fraisId, deviseId;
        string reference;
        decimal montantFlexPay;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (affilieId, agentId, fraisId, deviseId) = await FlexPayTestSeedHelper.SeedAffilieAgentFraisAsync(db);
            (_, _, reference, montantFlexPay) = await FlexPayTestSeedHelper.SeedCollecteEnAttenteAsync(
                db, affilieId, agentId, fraisId, deviseId, "ORD-CALLBACK-001");
        }

        var beforeCount = await CountCollectesAsync();
        var callback = new FlexPayCallbackDto
        {
            Code = "0",
            OrderNumber = "ORD-CALLBACK-001",
            Reference = reference,
            Amount = montantFlexPay.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Currency = "CDF",
            ProviderReference = "PRV-001",
            Channel = "ORANGE"
        };

        var res = await _client.PostAsJsonAsync("/api/FlexPay/callback", callback);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var parsed = await ParseCallbackResponseAsync(res);
        if (!parsed.Success)
        {
            var body = await res.Content.ReadAsStringAsync();
            Assert.Fail($"Callback failed: {body}");
        }
        Assert.True(parsed.Success);
        Assert.False(parsed.AlreadyProcessed);
        Assert.NotNull(parsed.IdCollecte);

        Assert.Equal(beforeCount + 1, await CountCollectesAsync());

        using var scope2 = _factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<ProsocDbContext>();
        var collecte = await db2.Collectes.FirstAsync(c => c.IdCollecte == parsed.IdCollecte);
        Assert.Equal("MOBILE_MONEY", collecte.ModePaiement);
        Assert.Equal(CollecteStatutPaiement.Valide, collecte.StatutPaiement);
        Assert.Equal("ORD-CALLBACK-001", collecte.OrderNumberFlexPay);
    }

    [Fact]
    public async Task Callback_Idempotent_DeuxiemeAppelNeDupliquePas()
    {
        decimal montantFlexPay;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var (affilieId, agentId, fraisId, deviseId) =
                await FlexPayTestSeedHelper.SeedAffilieAgentFraisAsync(db);
            (_, _, _, montantFlexPay) = await FlexPayTestSeedHelper.SeedCollecteEnAttenteAsync(
                db, affilieId, agentId, fraisId, deviseId, "ORD-IDEM-001");
        }

        var callback = new FlexPayCallbackDto
        {
            Code = "0",
            OrderNumber = "ORD-IDEM-001",
            Amount = montantFlexPay.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Currency = "CDF"
        };

        var first = await _client.PostAsJsonAsync("/api/FlexPay/callback", callback);
        var p1 = await ParseCallbackResponseAsync(first);
        Assert.True(p1.Success);
        Assert.NotNull(p1.IdCollecte);

        var countAfterFirst = await CountCollectesAsync();

        var second = await _client.PostAsJsonAsync("/api/FlexPay/callback", callback);
        var p2 = await ParseCallbackResponseAsync(second);
        Assert.True(p2.Success);
        Assert.True(p2.AlreadyProcessed);
        Assert.Equal(p1.IdCollecte, p2.IdCollecte);
        Assert.Equal(countAfterFirst, await CountCollectesAsync());
    }

    [Fact]
    public async Task Callback_CodeRefuse_NeCreePasCollecte()
    {
        decimal montantFlexPay;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var (affilieId, agentId, fraisId, deviseId) =
                await FlexPayTestSeedHelper.SeedAffilieAgentFraisAsync(db);
            (_, _, _, montantFlexPay) = await FlexPayTestSeedHelper.SeedCollecteEnAttenteAsync(
                db, affilieId, agentId, fraisId, deviseId, "ORD-FAIL-001");
        }

        var before = await CountCollectesAsync();
        var callback = new FlexPayCallbackDto
        {
            Code = "1",
            OrderNumber = "ORD-FAIL-001",
            Amount = montantFlexPay.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Currency = "CDF"
        };

        var res = await _client.PostAsJsonAsync("/api/FlexPay/callback", callback);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal(before, await CountCollectesAsync());

        using var scope2 = _factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<ProsocDbContext>();
        var enAttente = await db2.CollectesEnAttente
            .FirstAsync(c => c.OrderNumberFlexPay == "ORD-FAIL-001");
        Assert.Equal(CollecteEnAttenteStatut.Echec, enAttente.StatutEnAttente);
    }

    [Fact]
    public async Task InitiateCollecte_MobileMoney_RetourneEnAttenteSansCollecte()
    {
        int affilieId, agentId, fraisId, deviseId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (affilieId, agentId, fraisId, deviseId) = await FlexPayTestSeedHelper.SeedAffilieAgentFraisAsync(db);
        }

        decimal montantFlexPay;
        using (var scopeCalc = _factory.Services.CreateScope())
        {
            var dbCalc = scopeCalc.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var fraisEntity = await dbCalc.Frais.AsNoTracking().FirstAsync(f => f.IdFrais == fraisId);
            (_, _, montantFlexPay, _) = await FlexPayTestSeedHelper.ResolveMontantFlexPayAsync(
                dbCalc,
                (decimal)fraisEntity.Montant,
                fraisEntity.DeviseId,
                deviseId,
                DateTime.UtcNow);
        }

        var before = await CountCollectesAsync();

        var dto = new CollecteCreateDto
        {
            TypeCollecte = TypeCollecte.Frais,
            FraisId = fraisId,
            AffilieId = affilieId,
            AgentId = agentId,
            Montant = montantFlexPay,
            Mois = DateTime.UtcNow.Month,
            Annee = DateTime.UtcNow.Year,
            ModePaiement = "MOBILE_MONEY",
            DeviseId = deviseId,
            Phone = "0812345678",
            Statut = true
        };

        var res = await _client.PostAsJsonAsync("/api/Collecte", dto);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var init = await res.Content.ReadFromJsonAsync<InitiateFlexPayResponseDto>(JsonOptions);
        Assert.NotNull(init);
        Assert.True(init.FlexPayAccepted);
        Assert.False(string.IsNullOrWhiteSpace(init.OrderNumberFlexPay));
        Assert.Equal(before, await CountCollectesAsync());

        using var scope2 = _factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<ProsocDbContext>();
        Assert.True(await db2.CollectesEnAttente.AnyAsync(c => c.IdCollecteEnAttente == init.IdCollecteEnAttente));
    }

    [Fact]
    public async Task AdhesionFlexPay_InitiationPuisCallback_CreeAdhesion()
    {
        int agentId, prestationId, cotisationAffilieId, deviseId;
        decimal montantSouscription;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (prestationId, cotisationAffilieId, deviseId, montantSouscription) =
                await FlexPayTestSeedHelper.SeedPrestationCotisationAsync(db);
            agentId = await SeedAgentAsync(db);
        }

        var phone = $"08{Guid.NewGuid():N}"[..10];
        var input = BuildAdhesionFlexPayInput(
            agentId, cotisationAffilieId, prestationId, phone, deviseId, montantSouscription, "MOBILE_MONEY");

        var initRequest = new AdhesionWithAffiliePaiementElectroniqueCreateDto
        {
            Adhesion = input,
            ModePaiement = "MOBILE_MONEY",
            TelephonePaiement = phone,
            DevisePaiementId = deviseId
        };

        var initRes = await _client.PostAsJsonAsync(
            "/api/Adhesion/with-affilie-paiement-electronique", initRequest);
        if (initRes.StatusCode != HttpStatusCode.Accepted)
        {
            var body = await initRes.Content.ReadAsStringAsync();
            Assert.Fail($"Expected 202, got {(int)initRes.StatusCode}: {body}");
        }

        var init = await initRes.Content.ReadFromJsonAsync<InitiateFlexPayResponseDto>(JsonOptions);
        Assert.NotNull(init);
        Assert.False(string.IsNullOrWhiteSpace(init.OrderNumberFlexPay));

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            Assert.False(await db.Affilies.AnyAsync(a => a.Telephone == phone));
        }

        var callback = new FlexPayCallbackDto
        {
            Code = "0",
            OrderNumber = init.OrderNumberFlexPay,
            Reference = init.ReferenceFlexPay,
            Amount = init.MontantFlexPay.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Currency = init.CodeDevisePaiement
        };

        var cbRes = await _client.PostAsJsonAsync("/api/FlexPay/callback", callback);
        if (cbRes.StatusCode != HttpStatusCode.OK)
        {
            var err = await cbRes.Content.ReadAsStringAsync();
            Assert.Fail($"Callback HTTP {(int)cbRes.StatusCode}: {err}");
        }

        var parsed = await ParseCallbackResponseAsync(cbRes);
        if (!parsed.Success)
        {
            var errBody = await cbRes.Content.ReadAsStringAsync();
            Assert.Fail($"Callback adhesion failed: {errBody}");
        }

        Assert.True(parsed.Success);
        Assert.NotNull(parsed.IdAdhesion);

        using var scope3 = _factory.Services.CreateScope();
        var db3 = scope3.ServiceProvider.GetRequiredService<ProsocDbContext>();
        var adhesion = await db3.Adhesions.FirstAsync(a => a.IdAdhesion == parsed.IdAdhesion);
        var affilie = await db3.Affilies.FirstAsync(a => a.IdAffilie == adhesion.AffilieId);
        Assert.Equal(phone, affilie.Telephone);
        Assert.True(await db3.Collectes.AnyAsync(c => c.AffilieId == affilie.IdAffilie));
    }

    [Fact]
    public async Task AdhesionFlexPay_AnonymousSansUtilisateurId_FinaliseAvecAgentIdNull()
    {
        int prestationId, cotisationAffilieId, deviseId;
        decimal montantSouscription;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (prestationId, cotisationAffilieId, deviseId, montantSouscription) =
                await FlexPayTestSeedHelper.SeedPrestationCotisationAsync(db);
        }

        var phone = $"08{Guid.NewGuid():N}"[..10];
        var input = BuildAdhesionFlexPayInput(
            agentId: null, cotisationAffilieId, prestationId, phone, deviseId, montantSouscription, "MOBILE_MONEY");

        var initRequest = new AdhesionWithAffiliePaiementElectroniqueCreateDto
        {
            Adhesion = input,
            ModePaiement = "MOBILE_MONEY",
            TelephonePaiement = phone,
            DevisePaiementId = deviseId
        };

        // Init via TestAuth (toujours authentifié) ; UtilisateurId est ensuite purgé pour simuler AllowAnonymous.
        var initRes = await _client.PostAsJsonAsync(
            "/api/Adhesion/with-affilie-paiement-electronique", initRequest);
        if (initRes.StatusCode != HttpStatusCode.Accepted)
        {
            var body = await initRes.Content.ReadAsStringAsync();
            Assert.Fail($"Expected 202, got {(int)initRes.StatusCode}: {body}");
        }

        var init = await initRes.Content.ReadFromJsonAsync<InitiateFlexPayResponseDto>(JsonOptions);
        Assert.NotNull(init);

        // Simule une initiation anonyme : purge UtilisateurId (TestAuthHandler est toujours authentifié).
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var enAttente = await db.CollectesEnAttente
                .FirstAsync(e => e.ReferenceFlexPay == init.ReferenceFlexPay);

            enAttente.IdUtilisateur = null;
            enAttente.AgentId = null;

            var payload = System.Text.Json.JsonSerializer.Deserialize<AdhesionFlexPayPayload>(
                enAttente.PayloadMetierJson!)!;
            payload.UtilisateurId = null;
            payload.Input.AgentId = null;
            enAttente.PayloadMetierJson = System.Text.Json.JsonSerializer.Serialize(payload);
            await db.SaveChangesAsync();
        }

        var callback = new FlexPayCallbackDto
        {
            Code = "0",
            OrderNumber = init.OrderNumberFlexPay,
            Reference = init.ReferenceFlexPay,
            Amount = init.MontantFlexPay.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Currency = init.CodeDevisePaiement
        };

        var cbRes = await _client.PostAsJsonAsync("/api/FlexPay/callback", callback);
        if (cbRes.StatusCode != HttpStatusCode.OK)
        {
            var err = await cbRes.Content.ReadAsStringAsync();
            Assert.Fail($"Callback HTTP {(int)cbRes.StatusCode}: {err}");
        }

        var parsed = await ParseCallbackResponseAsync(cbRes);
        if (!parsed.Success)
        {
            var errBody = await cbRes.Content.ReadAsStringAsync();
            Assert.Fail($"Callback adhesion anonyme failed: {errBody}");
        }

        Assert.NotNull(parsed.IdAdhesion);

        using var scope3 = _factory.Services.CreateScope();
        var db3 = scope3.ServiceProvider.GetRequiredService<ProsocDbContext>();
        var adhesion = await db3.Adhesions.FirstAsync(a => a.IdAdhesion == parsed.IdAdhesion);
        Assert.Null(adhesion.AgentId);
        Assert.Null(adhesion.UtilisateurId);
        Assert.True(await db3.Collectes.AnyAsync(c =>
            c.AffilieId == adhesion.AffilieId && c.OperateurUtilisateurId == null));
    }

    [Fact]
    public async Task AdhesionEndpoint_WithAffilie_RejectsFlexPayModes()
    {
        int agentId, prestationId, cotisationAffilieId, deviseId;
        decimal montantSouscription;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (prestationId, cotisationAffilieId, deviseId, montantSouscription) =
                await FlexPayTestSeedHelper.SeedPrestationCotisationAsync(db);
            agentId = await SeedAgentAsync(db);
        }

        var phone = $"08{Guid.NewGuid():N}"[..10];
        var input = BuildAdhesionFlexPayInput(
            agentId, cotisationAffilieId, prestationId, phone, deviseId, montantSouscription, "MOBILE_MONEY");

        var res = await _client.PostAsJsonAsync("/api/Adhesion/with-affilie", input);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var body = await res.Content.ReadAsStringAsync();
        Assert.Contains("with-affilie-paiement-electronique", body);
    }

    [Fact]
    public async Task AdhesionFlexPayEndpoint_RequiresPhoneForMobileMoney()
    {
        int agentId, prestationId, cotisationAffilieId, deviseId;
        decimal montantSouscription;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (prestationId, cotisationAffilieId, deviseId, montantSouscription) =
                await FlexPayTestSeedHelper.SeedPrestationCotisationAsync(db);
            agentId = await SeedAgentAsync(db);
        }

        var input = BuildAdhesionFlexPayInput(
            agentId, cotisationAffilieId, prestationId, "0800001234", deviseId, montantSouscription, "MOBILE_MONEY");
        var request = new AdhesionWithAffiliePaiementElectroniqueCreateDto
        {
            Adhesion = input,
            ModePaiement = "MOBILE_MONEY",
            TelephonePaiement = null,
            DevisePaiementId = deviseId
        };

        var res = await _client.PostAsJsonAsync("/api/Adhesion/with-affilie-paiement-electronique", request);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task AdhesionCarteBancaire_InitiationPuisCallback_CreeAdhesion()
    {
        int agentId, prestationId, cotisationAffilieId, deviseId;
        decimal montantSouscription;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (prestationId, cotisationAffilieId, deviseId, montantSouscription) =
                await FlexPayTestSeedHelper.SeedPrestationCotisationAsync(db);
            agentId = await SeedAgentAsync(db);
        }

        var phone = $"08{Guid.NewGuid():N}"[..10];
        var input = BuildAdhesionFlexPayInput(
            agentId, cotisationAffilieId, prestationId, phone, deviseId, montantSouscription, "CARTE_BANCAIRE");

        var initRequest = new AdhesionWithAffiliePaiementElectroniqueCreateDto
        {
            Adhesion = input,
            ModePaiement = "CARTE_BANCAIRE",
            TelephonePaiement = null,
            DevisePaiementId = deviseId
        };

        var initRes = await _client.PostAsJsonAsync(
            "/api/Adhesion/with-affilie-paiement-electronique", initRequest);
        if (initRes.StatusCode != HttpStatusCode.Accepted)
        {
            var body = await initRes.Content.ReadAsStringAsync();
            Assert.Fail($"Expected 202, got {(int)initRes.StatusCode}: {body}");
        }

        var init = await initRes.Content.ReadFromJsonAsync<InitiateFlexPayResponseDto>(JsonOptions);
        Assert.NotNull(init);
        Assert.False(string.IsNullOrWhiteSpace(init.OrderNumberFlexPay));
        Assert.False(string.IsNullOrWhiteSpace(init.PaymentUrl));

        using (var scopeTx = _factory.Services.CreateScope())
        {
            var dbTx = scopeTx.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var tx = await dbTx.TransactionsFlexPay
                .FirstAsync(t => t.OrderNumber == init.OrderNumberFlexPay);
            Assert.Equal("2", tx.TypePaiement);
        }

        var callback = new FlexPayCallbackDto
        {
            Code = "0",
            OrderNumber = init.OrderNumberFlexPay,
            Reference = init.ReferenceFlexPay,
            Amount = init.MontantFlexPay.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Currency = init.CodeDevisePaiement
        };

        var cbRes = await _client.PostAsJsonAsync("/api/FlexPay/callback", callback);
        var parsed = await ParseCallbackResponseAsync(cbRes);
        if (!parsed.Success)
        {
            var errBody = await cbRes.Content.ReadAsStringAsync();
            Assert.Fail($"Callback adhesion carte failed: {errBody}");
        }

        Assert.True(parsed.Success);
        Assert.NotNull(parsed.IdAdhesion);

        using var scope3 = _factory.Services.CreateScope();
        var db3 = scope3.ServiceProvider.GetRequiredService<ProsocDbContext>();
        var adhesion = await db3.Adhesions.FirstAsync(a => a.IdAdhesion == parsed.IdAdhesion);
        var collecte = await db3.Collectes.FirstAsync(c => c.AffilieId == adhesion.AffilieId);
        Assert.Equal("CARTE_BANCAIRE", collecte.ModePaiement);
    }

    [Fact]
    public async Task CollectePublicFlexPay_InitiationPuisCallback_CreeCollecte()
    {
        int affilieId, agentId, fraisId, deviseId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (affilieId, agentId, fraisId, deviseId) = await FlexPayTestSeedHelper.SeedAffilieAgentFraisAsync(db);
        }

        decimal montantFlexPay;
        using (var scopeCalc = _factory.Services.CreateScope())
        {
            var dbCalc = scopeCalc.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var fraisEntity = await dbCalc.Frais.AsNoTracking().FirstAsync(f => f.IdFrais == fraisId);
            (_, _, montantFlexPay, _) = await FlexPayTestSeedHelper.ResolveMontantFlexPayAsync(
                dbCalc,
                (decimal)fraisEntity.Montant,
                fraisEntity.DeviseId,
                deviseId,
                DateTime.UtcNow);
        }

        var before = await CountCollectesAsync();
        var phone = $"08{Guid.NewGuid():N}"[..10];

        var initRequest = new CollecteWithPaiementElectroniqueCreateDto
        {
            ModePaiement = "MOBILE_MONEY",
            TelephonePaiement = phone,
            DevisePaiementId = deviseId,
            Collecte = new CollecteCreateDto
            {
                TypeCollecte = TypeCollecte.Frais,
                FraisId = fraisId,
                AffilieId = affilieId,
                AgentId = agentId,
                Montant = montantFlexPay,
                Mois = DateTime.UtcNow.Month,
                Annee = DateTime.UtcNow.Year,
                ModePaiement = "MOBILE_MONEY",
                DeviseId = deviseId,
                StatutPaiement = "EN_ATTENTE",
                Statut = true
            }
        };

        var initRes = await _client.PostAsJsonAsync(
            "/api/Collecte/with-paiement-electronique", initRequest);
        if (initRes.StatusCode != HttpStatusCode.Accepted)
        {
            var body = await initRes.Content.ReadAsStringAsync();
            Assert.Fail($"Expected 202, got {(int)initRes.StatusCode}: {body}");
        }

        var init = await initRes.Content.ReadFromJsonAsync<InitiateFlexPayResponseDto>(JsonOptions);
        Assert.NotNull(init);
        Assert.False(string.IsNullOrWhiteSpace(init.OrderNumberFlexPay));
        Assert.Equal(before, await CountCollectesAsync());

        using (var scopeCheck = _factory.Services.CreateScope())
        {
            var dbCheck = scopeCheck.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var enAttente = await dbCheck.CollectesEnAttente
                .FirstAsync(c => c.IdCollecteEnAttente == init.IdCollecteEnAttente);
            Assert.Equal(CollecteEnAttenteSourceFlux.CollectePaiementElectroniquePublic, enAttente.SourceFlux);
        }

        var callback = new FlexPayCallbackDto
        {
            Code = "0",
            OrderNumber = init.OrderNumberFlexPay,
            Reference = init.ReferenceFlexPay,
            Amount = init.MontantFlexPay.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Currency = init.CodeDevisePaiement
        };

        var cbRes = await _client.PostAsJsonAsync("/api/FlexPay/callback", callback);
        var parsed = await ParseCallbackResponseAsync(cbRes);
        if (!parsed.Success)
        {
            var errBody = await cbRes.Content.ReadAsStringAsync();
            Assert.Fail($"Callback collecte publique failed: {errBody}");
        }

        Assert.True(parsed.Success);
        Assert.NotNull(parsed.IdCollecte);
        Assert.Equal(before + 1, await CountCollectesAsync());
    }

    [Fact]
    public async Task CollectePublicFlexPayEndpoint_RequiresPhoneForMobileMoney()
    {
        int affilieId, agentId, fraisId, deviseId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (affilieId, agentId, fraisId, deviseId) = await FlexPayTestSeedHelper.SeedAffilieAgentFraisAsync(db);
        }

        var request = new CollecteWithPaiementElectroniqueCreateDto
        {
            ModePaiement = "MOBILE_MONEY",
            TelephonePaiement = null,
            DevisePaiementId = deviseId,
            Collecte = new CollecteCreateDto
            {
                TypeCollecte = TypeCollecte.Frais,
                FraisId = fraisId,
                AffilieId = affilieId,
                AgentId = agentId,
                Montant = 100,
                DeviseId = deviseId,
                ModePaiement = "MOBILE_MONEY",
                Statut = true
            }
        };

        var res = await _client.PostAsJsonAsync("/api/Collecte/with-paiement-electronique", request);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task CollectePublicCarteBancaire_InitiationRetournePaymentUrlEtCallback_CreeCollecte()
    {
        int affilieId, agentId, fraisId, deviseId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            (affilieId, agentId, fraisId, deviseId) = await FlexPayTestSeedHelper.SeedAffilieAgentFraisAsync(db);
        }

        decimal montantFlexPay;
        using (var scopeCalc = _factory.Services.CreateScope())
        {
            var dbCalc = scopeCalc.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var fraisEntity = await dbCalc.Frais.AsNoTracking().FirstAsync(f => f.IdFrais == fraisId);
            (_, _, montantFlexPay, _) = await FlexPayTestSeedHelper.ResolveMontantFlexPayAsync(
                dbCalc,
                (decimal)fraisEntity.Montant,
                fraisEntity.DeviseId,
                deviseId,
                DateTime.UtcNow);
        }

        var before = await CountCollectesAsync();

        var initRequest = new CollecteWithPaiementElectroniqueCreateDto
        {
            ModePaiement = "CARTE_BANCAIRE",
            TelephonePaiement = null,
            DevisePaiementId = deviseId,
            Collecte = new CollecteCreateDto
            {
                TypeCollecte = TypeCollecte.Frais,
                FraisId = fraisId,
                AffilieId = affilieId,
                AgentId = agentId,
                Montant = montantFlexPay,
                Mois = DateTime.UtcNow.Month,
                Annee = DateTime.UtcNow.Year,
                ModePaiement = "CARTE_BANCAIRE",
                DeviseId = deviseId,
                StatutPaiement = "EN_ATTENTE",
                Statut = true
            }
        };

        var initRes = await _client.PostAsJsonAsync(
            "/api/Collecte/with-paiement-electronique", initRequest);
        if (initRes.StatusCode != HttpStatusCode.Accepted)
        {
            var body = await initRes.Content.ReadAsStringAsync();
            Assert.Fail($"Expected 202, got {(int)initRes.StatusCode}: {body}");
        }

        var init = await initRes.Content.ReadFromJsonAsync<InitiateFlexPayResponseDto>(JsonOptions);
        Assert.NotNull(init);
        Assert.False(string.IsNullOrWhiteSpace(init.PaymentUrl));
        Assert.Equal(before, await CountCollectesAsync());

        var callback = new FlexPayCallbackDto
        {
            Code = "0",
            OrderNumber = init.OrderNumberFlexPay,
            Reference = init.ReferenceFlexPay,
            Amount = init.MontantFlexPay.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Currency = init.CodeDevisePaiement
        };

        var cbRes = await _client.PostAsJsonAsync("/api/FlexPay/callback", callback);
        var parsed = await ParseCallbackResponseAsync(cbRes);
        if (!parsed.Success)
        {
            var errBody = await cbRes.Content.ReadAsStringAsync();
            Assert.Fail($"Callback collecte carte failed: {errBody}");
        }

        Assert.NotNull(parsed.IdCollecte);

        using var scope2 = _factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<ProsocDbContext>();
        var collecte = await db2.Collectes.FirstAsync(c => c.IdCollecte == parsed.IdCollecte);
        Assert.Equal("CARTE_BANCAIRE", collecte.ModePaiement);
        Assert.Equal(before + 1, await CountCollectesAsync());
    }

    [Fact]
    public async Task CollectePublicFlexPayEndpoint_RejectsNonFlexPayMode()
    {
        var request = new CollecteWithPaiementElectroniqueCreateDto
        {
            ModePaiement = "ESPECE",
            DevisePaiementId = 1,
            Collecte = new CollecteCreateDto
            {
                TypeCollecte = TypeCollecte.Frais,
                FraisId = 1,
                AffilieId = 1,
                AgentId = 1,
                Montant = 10,
                DeviseId = 1,
                ModePaiement = "ESPECE",
                Statut = true
            }
        };

        var res = await _client.PostAsJsonAsync("/api/Collecte/with-paiement-electronique", request);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    private async Task<int> CountCollectesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
        return await db.Collectes.CountAsync();
    }

    private static async Task<int> SeedAgentAsync(ProsocDbContext db)
    {
        var zone = await db.ZonesSociales.FirstAsync();
        var agent = new Agent
        {
            NomComplet = "Agent Adh FP",
            Matricule = $"MAT-ADH-{Guid.NewGuid():N}"[..10],
            Phone = "0822222222",
            ZoneSocialeId = zone.IdZoneSociale,
            Statut = true,
            DateCreation = DateTime.UtcNow
        };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();
        return agent.IdAgent;
    }

    [Fact]
    public async Task Verifier_StatusPending_NeMarquePasEchec_EtPendingTrue()
    {
        FlexPayStubService.ResetCheckStatus();
        FlexPayStubService.CheckTransactionStatus = "1"; // non-final chez FlexPay check API

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var (affilieId, agentId, fraisId, deviseId) =
                await FlexPayTestSeedHelper.SeedAffilieAgentFraisAsync(db);
            await FlexPayTestSeedHelper.SeedCollecteEnAttenteAsync(
                db, affilieId, agentId, fraisId, deviseId, "ORD-VERIFY-PENDING");
        }

        var before = await CountCollectesAsync();
        var res = await _client.GetAsync("/api/FlexPay/verifier/ORD-VERIFY-PENDING");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var body = await res.Content.ReadFromJsonAsync<FlexPayCallbackProcessResultDto>(JsonOptions);
        Assert.NotNull(body);
        Assert.True(body!.Pending);
        Assert.False(body.Success);
        Assert.Contains("en cours", body.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(body.IdCollecte);
        Assert.Null(body.IdAdhesion);
        Assert.Equal(before, await CountCollectesAsync());

        using var scope2 = _factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<ProsocDbContext>();
        var enAttente = await db2.CollectesEnAttente
            .FirstAsync(c => c.OrderNumberFlexPay == "ORD-VERIFY-PENDING");
        Assert.Equal(CollecteEnAttenteStatut.EnAttente, enAttente.StatutEnAttente);
        Assert.Null(enAttente.IdCollecteFinalisee);

        FlexPayStubService.ResetCheckStatus();
    }

    [Fact]
    public async Task Verifier_StatusZero_FinaliseCollecte()
    {
        FlexPayStubService.ResetCheckStatus();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var (affilieId, agentId, fraisId, deviseId) =
                await FlexPayTestSeedHelper.SeedAffilieAgentFraisAsync(db);
            await FlexPayTestSeedHelper.SeedCollecteEnAttenteAsync(
                db, affilieId, agentId, fraisId, deviseId, "ORD-VERIFY-OK");
        }

        var before = await CountCollectesAsync();
        var res = await _client.GetAsync("/api/FlexPay/verifier/ORD-VERIFY-OK");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var body = await res.Content.ReadFromJsonAsync<FlexPayCallbackProcessResultDto>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(body!.Pending);
        Assert.True(body.Success);
        Assert.NotNull(body.IdCollecte);
        Assert.Equal(before + 1, await CountCollectesAsync());
    }

    [Fact]
    public async Task Verifier_PendingPuisSuccess_FinaliseSansEchecIntermediaire()
    {
        FlexPayStubService.ResetCheckStatus();
        FlexPayStubService.CheckTransactionStatus = "1";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var (affilieId, agentId, fraisId, deviseId) =
                await FlexPayTestSeedHelper.SeedAffilieAgentFraisAsync(db);
            await FlexPayTestSeedHelper.SeedCollecteEnAttenteAsync(
                db, affilieId, agentId, fraisId, deviseId, "ORD-VERIFY-THEN-OK");
        }

        var pendingRes = await _client.GetAsync("/api/FlexPay/verifier/ORD-VERIFY-THEN-OK");
        var pending = await pendingRes.Content.ReadFromJsonAsync<FlexPayCallbackProcessResultDto>(JsonOptions);
        Assert.True(pending!.Pending);
        Assert.False(pending.Success);

        FlexPayStubService.CheckTransactionStatus = "0";
        var okRes = await _client.GetAsync("/api/FlexPay/verifier/ORD-VERIFY-THEN-OK");
        var ok = await okRes.Content.ReadFromJsonAsync<FlexPayCallbackProcessResultDto>(JsonOptions);
        Assert.False(ok!.Pending);
        Assert.True(ok.Success);
        Assert.NotNull(ok.IdCollecte);

        using var scope2 = _factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<ProsocDbContext>();
        var enAttente = await db2.CollectesEnAttente
            .FirstAsync(c => c.OrderNumberFlexPay == "ORD-VERIFY-THEN-OK");
        Assert.Equal(CollecteEnAttenteStatut.Finalise, enAttente.StatutEnAttente);

        FlexPayStubService.ResetCheckStatus();
    }

    private static AdhesionWithAffilieCreateDto BuildAdhesionFlexPayInput(
        int? agentId,
        int cotisationAffilieId,
        int prestationId,
        string phone,
        int deviseId,
        decimal montantSouscription,
        string flexPayMode = "MOBILE_MONEY")
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        return new()
        {
            Nom = $"Mukendi{unique}",
            Prenom = $"Grace{unique}",
            DateNaissance = new DateTime(1992, 4, 5),
            Telephone = phone,
            ProvinceResidence = "Kinshasa",
            CommuneResidence = "Gombe",
            QuartierResidence = "Centre",
            PhotoBase64 = "cGhvdG8=",
            PhotoContentType = "image/jpeg",
            CarteIdentiteBase64 = "Y2FydGU=",
            CarteIdentiteContentType = "image/jpeg",
            AffilieStatut = true,
            StatutDossier = "EN ATTENTE",
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
                    DeviseId = deviseId,
                    ModePaiement = flexPayMode,
                    StatutPaiement = "EN_ATTENTE",
                    Statut = true,
                    Mois = DateTime.UtcNow.Month,
                    Annee = DateTime.UtcNow.Year
                },
                new()
                {
                    TypeCollecte = TypeCollecte.Souscription,
                    Montant = montantSouscription,
                    DeviseId = deviseId,
                    ModePaiement = flexPayMode,
                    StatutPaiement = "EN_ATTENTE",
                    Statut = true,
                    Mois = DateTime.UtcNow.Month,
                    Annee = DateTime.UtcNow.Year,
                    Souscription = new SouscriptionPrestationCreateDto { PrestationId = prestationId }
                }
            }
        };
    }
}
