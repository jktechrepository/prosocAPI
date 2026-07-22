using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Authentication;
using Xunit;

namespace Prosoc.Tests.Integration;

public class UtilisateurControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public UtilisateurControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ExistsById_ReturnsTrue_ForSeededUser()
    {
        var res = await _client.GetAsync("/api/Utilisateur/exists/1");
        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadFromJsonAsync<Dictionary<string, bool>>();
        Assert.NotNull(json);
        Assert.True(json!["exists"]);
    }

    [Fact]
    public async Task ExistsByEmail_ReturnsTrue_ForSeededUser()
    {
        var res = await _client.GetAsync("/api/Utilisateur/exists/email/admin@prosoc.cd");
        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadFromJsonAsync<Dictionary<string, bool>>();
        Assert.NotNull(json);
        Assert.True(json!["exists"]);
    }

    [Fact]
    public async Task GetByEmail_ReturnsUser_WhenFound()
    {
        var res = await _client.GetAsync("/api/Utilisateur/email?email=admin@prosoc.cd");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var json = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(json);
        Assert.True(json!.ContainsKey("idUtilisateur"));
    }

    [Fact]
    public async Task GetByStatut_True_ReturnsOk()
    {
        var res = await _client.GetAsync("/api/Utilisateur/statut/true");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task GetByRole_ReturnsOk()
    {
        var res = await _client.GetAsync("/api/Utilisateur/role/2");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Login_Agent_ReturnsNullIdAgentGestionnaireCompte()
    {
        var response = await _client.PostAsJsonAsync("/api/Utilisateur/login", new AuthentificationRequest
        {
            EmailOuTelephone = "admin@prosoc.cd",
            MotDePasse = "Admin"
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthentificationResponse>();

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.NotNull(result.Utilisateur);
        Assert.NotNull(result.Utilisateur!.AgentId);
        Assert.Null(result.Utilisateur.IdAgentGestionnaireCompte);
    }

    [Fact]
    public async Task Login_AffilieWithAdhesion_ReturnsIdAgentGestionnaireCompte()
    {
        const string email = "affilie.gestionnaire@test.cd";
        const string password = "Affilie-Test1";

        int gestionnaireAgentId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var role = db.Roles.First();

            var gestionnaire = new Agent
            {
                NomComplet = "Gestionnaire Test",
                Matricule = "AG-GEST-TEST",
                Phone = "0991111222",
                EmailAgent = "gestionnaire@test.cd",
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            db.Agents.Add(gestionnaire);

            var affilie = new Affilie
            {
                CodeAdhesion = "AFF-GEST-TEST",
                Nom = "Test",
                Prenom = "Affilie",
                NomComplet = "Test Affilie",
                DateNaissance = new DateTime(1992, 5, 10),
                Telephone = "0993333444",
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();

            var utilisateur = new Utilisateur
            {
                NomUtilisateur = "affilie_test",
                EmailUtilisateur = email,
                PhoneUtilisateur = "+24393333444",
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(password),
                Statut = true,
                RoleId = role.IdRole,
                AffilieId = affilie.IdAffilie,
                DateCreation = DateTime.UtcNow
            };
            db.Utilisateurs.Add(utilisateur);
            await db.SaveChangesAsync();

            db.Adhesions.Add(new Adhesion
            {
                AgentId = gestionnaire.IdAgent,
                AffilieId = affilie.IdAffilie,
                UtilisateurId = utilisateur.IdUtilisateur,
                TypeAdhesionId = 1,
                StatutDossier = "Validé",
                Statut = true,
                DateCreation = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            gestionnaireAgentId = gestionnaire.IdAgent;
        }

        var response = await _client.PostAsJsonAsync("/api/Utilisateur/login", new AuthentificationRequest
        {
            EmailOuTelephone = email,
            MotDePasse = password
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthentificationResponse>();

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.NotNull(result.Utilisateur);
        Assert.Equal(gestionnaireAgentId, result.Utilisateur!.IdAgentGestionnaireCompte);
        Assert.Equal("Gestionnaire Test", result.Utilisateur.NomAgentGestionnaireCompte);
        Assert.Equal("AG-GEST-TEST", result.Utilisateur.MatriculeAgentGestionnaireCompte);
        Assert.Null(result.Utilisateur.AgentId);
    }

    [Fact]
    public async Task Login_AffilieWithoutAdhesion_ReturnsNullIdAgentGestionnaireCompte()
    {
        const string email = "affilie.sans.adhesion@test.cd";
        const string password = "Affilie-Test2";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var role = db.Roles.First();

            var affilie = new Affilie
            {
                CodeAdhesion = "AFF-NO-ADH",
                Nom = "Sans",
                Prenom = "Adhesion",
                NomComplet = "Sans Adhesion",
                DateNaissance = new DateTime(1988, 3, 3),
                Statut = true,
                DateCreation = DateTime.UtcNow
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();

            db.Utilisateurs.Add(new Utilisateur
            {
                NomUtilisateur = "affilie_sans_adh",
                EmailUtilisateur = email,
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(password),
                Statut = true,
                RoleId = role.IdRole,
                AffilieId = affilie.IdAffilie,
                DateCreation = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var response = await _client.PostAsJsonAsync("/api/Utilisateur/login", new AuthentificationRequest
        {
            EmailOuTelephone = email,
            MotDePasse = password
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthentificationResponse>();

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.NotNull(result.Utilisateur);
        Assert.Null(result.Utilisateur!.IdAgentGestionnaireCompte);
    }
}
