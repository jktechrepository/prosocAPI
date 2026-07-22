using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using Xunit;

namespace Prosoc.Tests.Integration;

public class CaisseSessionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CaisseSessionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        TestAuthHandler.Roles = new[] { "Admin", "Caissier" };
    }

    private async Task CloturerSessionOuverteSiExisteAsync()
    {
        var couranteResponse = await _client.GetAsync("/api/Caisse/session/courante");
        if (!couranteResponse.IsSuccessStatusCode)
            return;

        var session = await couranteResponse.Content.ReadFromJsonAsync<SessionCaisseReadDto>();
        if (session == null)
            return;

        await _client.PostAsJsonAsync(
            $"/api/Caisse/session/{session.IdSessionCaisse}/cloturer",
            new SessionCaisseCloturerDto { SoldeReelCloture = session.SoldeCourant });
    }

    [Fact]
    public async Task OuvrirSession_PuisCourante_RetourneSessionOuverte()
    {
        await CloturerSessionOuverteSiExisteAsync();
        var response = await _client.PostAsJsonAsync(
            "/api/Caisse/session/ouvrir",
            new SessionCaisseOuvrirDto { SoldeOuverture = 250000m });
        response.EnsureSuccessStatusCode();

        var session = await response.Content.ReadFromJsonAsync<SessionCaisseReadDto>();
        Assert.NotNull(session);
        Assert.Equal("OUVERTE", session!.Statut);
        Assert.Equal(250000m, session.SoldeOuverture);
        Assert.Equal(250000m, session.SoldeCourant);

        var courante = await _client.GetFromJsonAsync<SessionCaisseReadDto>("/api/Caisse/session/courante");
        Assert.NotNull(courante);
        Assert.Equal(session.IdSessionCaisse, courante!.IdSessionCaisse);
    }

    [Fact]
    public async Task CloturerSession_AvecSoldeReel_FermeLaSession()
    {
        await CloturerSessionOuverteSiExisteAsync();
        var openResponse = await _client.PostAsJsonAsync(
            "/api/Caisse/session/ouvrir",
            new SessionCaisseOuvrirDto { SoldeOuverture = 100000m });
        openResponse.EnsureSuccessStatusCode();
        var session = await openResponse.Content.ReadFromJsonAsync<SessionCaisseReadDto>();
        Assert.NotNull(session);

        var closeResponse = await _client.PostAsJsonAsync(
            $"/api/Caisse/session/{session!.IdSessionCaisse}/cloturer",
            new SessionCaisseCloturerDto { SoldeReelCloture = 99500m, ObservationCloture = "Test clôture" });
        closeResponse.EnsureSuccessStatusCode();

        var closed = await closeResponse.Content.ReadFromJsonAsync<SessionCaisseReadDto>();
        Assert.NotNull(closed);
        Assert.Equal("CLOTUREE", closed!.Statut);
        Assert.Equal(99500m, closed.SoldeReelCloture);

        var couranteResponse = await _client.GetAsync("/api/Caisse/session/courante");
        Assert.Equal(HttpStatusCode.NotFound, couranteResponse.StatusCode);
    }

    [Fact]
    public async Task UtiliserJeton_AvecSessionOuverte_CreeWalletMouvementEtSortieCaisse()
    {
        await CloturerSessionOuverteSiExisteAsync();
        var openResponse = await _client.PostAsJsonAsync(
            "/api/Caisse/session/ouvrir",
            new SessionCaisseOuvrirDto { SoldeOuverture = 500000m });
        openResponse.EnsureSuccessStatusCode();
        var session = await openResponse.Content.ReadFromJsonAsync<SessionCaisseReadDto>();
        Assert.NotNull(session);

        int agentId;
        int demandeId;
        string codeJeton;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var deviseId = await db.Devises.Where(d => d.EstDevisePrincipale).Select(d => d.IdDevise).FirstAsync();

            var agent = new Agent
            {
                NomComplet = "Agent Caisse",
                Matricule = "MAT-CAISSE-01",
                Phone = "0998887701",
                ZoneSocialeId = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();
            agentId = agent.IdAgent;

            db.WalletsAgents.Add(new WalletAgent
            {
                AgentId = agentId,
                DeviseId = deviseId,
                SoldeCourant = 200000m,
                SoldeDisponible = 200000m,
                DateCreation = DateTime.Now
            });

            var demande = new DemandeRetraitAgent
            {
                AgentId = agentId,
                MontantDemande = 75000m,
                TypeRetrait = "PARTIEL",
                StatutDemande = "VALIDEE",
                DateDemande = DateTime.Now,
                DateValidation = DateTime.Now
            };
            db.DemandesRetraitAgents.Add(demande);
            await db.SaveChangesAsync();
            demandeId = demande.IdDemande;

            codeJeton = "JRTCAISSE1";
            db.JetonsRetraits.Add(new JetonRetrait
            {
                AgentId = agentId,
                DemandeRetraitId = demandeId,
                CodeJeton = codeJeton,
                MontantRetrait = 75000m,
                DateEmission = DateTime.Now,
                DateExpiration = DateTime.Now.AddDays(7),
                EstValide = true
            });
            await db.SaveChangesAsync();
        }

        var payResponse = await _client.PostAsJsonAsync(
            "/api/RetraitAgent/marquer-paye",
            new JetonRetraitUtilisationDto
            {
                CodeJeton = codeJeton,
                AgentId = agentId,
                IdJeton = 0,
                SessionCaisseId = session!.IdSessionCaisse
            });
        payResponse.EnsureSuccessStatusCode();

        var result = await payResponse.Content.ReadFromJsonAsync<RetraitPaiementResultDto>();
        Assert.NotNull(result);
        Assert.True(result!.Succes);
        Assert.Equal(75000m, result.MontantPaye);
        Assert.NotNull(result.WalletMouvementId);
        Assert.NotNull(result.MouvementCaisseId);
        Assert.Equal(425000m, result.SoldeCaisseSessionApres);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ProsocDbContext>();
        var walletMouvement = await verifyDb.WalletMouvements.FindAsync(result.WalletMouvementId);
        Assert.NotNull(walletMouvement);
        Assert.Equal("RETRAIT_JETON", walletMouvement!.Source);

        var mouvementCaisse = await verifyDb.MouvementsCaisses.FindAsync(result.MouvementCaisseId);
        Assert.NotNull(mouvementCaisse);
        Assert.Equal(MouvementCaisseTypes.Sortie, mouvementCaisse!.TypeOperation);
    }

    [Fact]
    public async Task SessionCourante_SansSessionOuverte_Retourne404()
    {
        await CloturerSessionOuverteSiExisteAsync();

        var response = await _client.GetAsync("/api/Caisse/session/courante");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FluxComplet_OuvrirSession_PuisPayerJetonAvecSessionCourante()
    {
        await CloturerSessionOuverteSiExisteAsync();

        var openResponse = await _client.PostAsJsonAsync(
            "/api/Caisse/session/ouvrir",
            new SessionCaisseOuvrirDto { SoldeOuverture = 300000m });
        openResponse.EnsureSuccessStatusCode();

        var courante = await _client.GetFromJsonAsync<SessionCaisseReadDto>("/api/Caisse/session/courante");
        Assert.NotNull(courante);
        Assert.Equal("OUVERTE", courante!.Statut);

        var soldeResponse = await _client.GetFromJsonAsync<SessionCaisseSoldeDto>(
            $"/api/Caisse/session/{courante.IdSessionCaisse}/solde");
        Assert.NotNull(soldeResponse);
        Assert.True(soldeResponse!.SoldeCourant >= 50000m);

        int agentId;
        string codeJeton;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var deviseId = await db.Devises.Where(d => d.EstDevisePrincipale).Select(d => d.IdDevise).FirstAsync();

            var agent = new Agent
            {
                NomComplet = "Agent Flux Session",
                Matricule = "MAT-FLUX-01",
                Phone = "0998887702",
                ZoneSocialeId = 1,
                Statut = true,
                DateCreation = DateTime.Now
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();
            agentId = agent.IdAgent;

            db.WalletsAgents.Add(new WalletAgent
            {
                AgentId = agentId,
                DeviseId = deviseId,
                SoldeCourant = 100000m,
                SoldeDisponible = 100000m,
                DateCreation = DateTime.Now
            });

            var demande = new DemandeRetraitAgent
            {
                AgentId = agentId,
                MontantDemande = 50000m,
                TypeRetrait = "PARTIEL",
                StatutDemande = "VALIDEE",
                DateDemande = DateTime.Now,
                DateValidation = DateTime.Now
            };
            db.DemandesRetraitAgents.Add(demande);
            await db.SaveChangesAsync();

            codeJeton = "JRTFLUX01";
            db.JetonsRetraits.Add(new JetonRetrait
            {
                AgentId = agentId,
                DemandeRetraitId = demande.IdDemande,
                CodeJeton = codeJeton,
                MontantRetrait = 50000m,
                DateEmission = DateTime.Now,
                DateExpiration = DateTime.Now.AddDays(7),
                EstValide = true
            });
            await db.SaveChangesAsync();
        }

        var payResponse = await _client.PostAsJsonAsync(
            "/api/RetraitAgent/utiliser-jeton",
            new JetonRetraitUtilisationDto
            {
                CodeJeton = codeJeton,
                AgentId = agentId,
                SessionCaisseId = courante.IdSessionCaisse,
                ObservationUtilisation = "Test flux session courante"
            });
        payResponse.EnsureSuccessStatusCode();

        var result = await payResponse.Content.ReadFromJsonAsync<RetraitPaiementResultDto>();
        Assert.NotNull(result);
        Assert.True(result!.Succes);
        Assert.NotNull(result.MouvementCaisseId);
        Assert.NotNull(result.SoldeCaisseSessionApres);
    }

    [Fact]
    public async Task CloturerSession_PuisReouvrir_PermetNouvelleSessionCourante()
    {
        await CloturerSessionOuverteSiExisteAsync();

        var firstOpen = await _client.PostAsJsonAsync(
            "/api/Caisse/session/ouvrir",
            new SessionCaisseOuvrirDto { SoldeOuverture = 80000m });
        firstOpen.EnsureSuccessStatusCode();
        var first = await firstOpen.Content.ReadFromJsonAsync<SessionCaisseReadDto>();
        Assert.NotNull(first);

        var closeResponse = await _client.PostAsJsonAsync(
            $"/api/Caisse/session/{first!.IdSessionCaisse}/cloturer",
            new SessionCaisseCloturerDto { SoldeReelCloture = 80000m });
        closeResponse.EnsureSuccessStatusCode();

        var couranteAfterClose = await _client.GetAsync("/api/Caisse/session/courante");
        Assert.Equal(HttpStatusCode.NotFound, couranteAfterClose.StatusCode);

        var secondOpen = await _client.PostAsJsonAsync(
            "/api/Caisse/session/ouvrir",
            new SessionCaisseOuvrirDto { SoldeOuverture = 120000m });
        secondOpen.EnsureSuccessStatusCode();

        var second = await _client.GetFromJsonAsync<SessionCaisseReadDto>("/api/Caisse/session/courante");
        Assert.NotNull(second);
        Assert.Equal("OUVERTE", second!.Statut);
        Assert.NotEqual(first.IdSessionCaisse, second.IdSessionCaisse);
        Assert.Equal(120000m, second.SoldeOuverture);
    }

    [Fact]
    public async Task GetSessions_RetourneSessionsUtilisateurConnecte()
    {
        await CloturerSessionOuverteSiExisteAsync();
        var openResponse = await _client.PostAsJsonAsync(
            "/api/Caisse/session/ouvrir",
            new SessionCaisseOuvrirDto { SoldeOuverture = 100000m });
        openResponse.EnsureSuccessStatusCode();
        var session = await openResponse.Content.ReadFromJsonAsync<SessionCaisseReadDto>();
        Assert.NotNull(session);

        var listResponse = await _client.GetAsync("/api/Caisse/sessions?pageNumber=1&pageSize=10");
        listResponse.EnsureSuccessStatusCode();

        var json = await listResponse.Content.ReadAsStringAsync();
        Assert.Contains(session!.IdSessionCaisse.ToString(), json);
    }
}
