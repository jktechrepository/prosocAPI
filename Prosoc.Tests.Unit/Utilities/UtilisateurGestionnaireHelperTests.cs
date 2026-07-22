using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Authentication;
using ProsocAPI.Utilities;
using Xunit;

namespace Prosoc.Tests.Unit.Utilities;

public class UtilisateurGestionnaireHelperTests
{
    private static async Task<ProsocDbContext> CreateDbContextAsync(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    [Fact]
    public async Task EnrichGestionnaireAffilieAsync_WithAdhesion_SetsGestionnaireFields()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);

        db.CategoriesAdhesions.Add(new CategorieAdhesion
        {
            Libelle = "Particulier",
            Description = "Test",
            Statut = true,
            DateCreation = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var categorie = db.CategoriesAdhesions.First();
        db.TypeAdhesions.Add(new TypeAdhesion
        {
            Libelle = "Solo",
            CategorieAdhesionId = categorie.IdCategorieAdhesion,
            MaxDependants = 0,
            Description = "Test",
            Montant = 1m,
            Statut = true,
            DateCreation = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        var typeAdhesionId = db.TypeAdhesions.First().IdTypeAdhesion;

        var agent = new Agent
        {
            NomComplet = "Agent Gestionnaire",
            Matricule = "AG000000099",
            Phone = "0990000099",
            Statut = true
        };
        db.Agents.Add(agent);

        var affilie = new Affilie
        {
            CodeAdhesion = "AFF-TEST-001",
            Nom = "Dupont",
            Prenom = "Marie",
            NomComplet = "Dupont Marie",
            DateNaissance = new DateTime(1990, 1, 1),
            Statut = true
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        var utilisateur = new ProsocAPI.Models.Authentication.Utilisateur
        {
            NomUtilisateur = "user_test",
            MotDePasseHash = "hash",
            Statut = true,
            AffilieId = affilie.IdAffilie,
            DateCreation = DateTime.UtcNow
        };
        db.Utilisateurs.Add(utilisateur);
        await db.SaveChangesAsync();

        db.Adhesions.Add(new Adhesion
        {
            AgentId = agent.IdAgent,
            AffilieId = affilie.IdAffilie,
            UtilisateurId = utilisateur.IdUtilisateur,
            TypeAdhesionId = typeAdhesionId,
            StatutDossier = "Validé",
            Statut = true
        });
        await db.SaveChangesAsync();

        var dto = new UtilisateurDto { AffilieId = affilie.IdAffilie };
        await UtilisateurGestionnaireHelper.EnrichGestionnaireAffilieAsync(dto, db, affilie.IdAffilie);

        Assert.Equal(agent.IdAgent, dto.IdAgentGestionnaireCompte);
        Assert.Equal("Agent Gestionnaire", dto.NomAgentGestionnaireCompte);
        Assert.Equal("AG000000099", dto.MatriculeAgentGestionnaireCompte);
    }

    [Fact]
    public async Task EnrichGestionnaireAffilieAsync_WithoutAdhesion_LeavesFieldsNull()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);

        var affilie = new Affilie
        {
            CodeAdhesion = "AFF-TEST-002",
            Nom = "Sans",
            Prenom = "Adhesion",
            NomComplet = "Sans Adhesion",
            DateNaissance = new DateTime(1990, 1, 1),
            Statut = true
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        var dto = new UtilisateurDto { AffilieId = affilie.IdAffilie };
        await UtilisateurGestionnaireHelper.EnrichGestionnaireAffilieAsync(dto, db, affilie.IdAffilie);

        Assert.Null(dto.IdAgentGestionnaireCompte);
        Assert.Null(dto.NomAgentGestionnaireCompte);
        Assert.Null(dto.MatriculeAgentGestionnaireCompte);
    }

    [Fact]
    public async Task EnrichGestionnaireAffilieAsync_NullAffilieId_LeavesFieldsNull()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateDbContextAsync(connection);

        var dto = new UtilisateurDto();
        await UtilisateurGestionnaireHelper.EnrichGestionnaireAffilieAsync(dto, db, null);

        Assert.Null(dto.IdAgentGestionnaireCompte);
    }
}
