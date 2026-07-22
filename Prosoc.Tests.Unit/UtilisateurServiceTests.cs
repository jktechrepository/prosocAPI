using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit;

public class UtilisateurServiceTests
{
    private static ProsocDbContext CreateDbContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ProsocDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public async Task GetByStatutAsync_ReturnsOnlyMatchingUsers()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var db = CreateDbContext(connection);

        // Arrange
        db.Utilisateurs.AddRange(
            new Utilisateur { IdUtilisateur = 10, NomUtilisateur = "u10@x.com", MotDePasseHash = "x", Statut = true },
            new Utilisateur { IdUtilisateur = 11, NomUtilisateur = "u11@x.com", MotDePasseHash = "x", Statut = false }
        );
        await db.SaveChangesAsync();

        var service = new UtilisateurService(db);

        // Act
        var actives = await service.GetByStatutAsync(true);
        var inactives = await service.GetByStatutAsync(false);

        // Assert
        Assert.Contains(actives, u => u.IdUtilisateur == 10);
        Assert.DoesNotContain(actives, u => u.IdUtilisateur == 11);

        Assert.Contains(inactives, u => u.IdUtilisateur == 11);
        Assert.DoesNotContain(inactives, u => u.IdUtilisateur == 10);
    }

    [Fact]
    public async Task ExistsByIdAsync_ReturnsTrueWhenUserExists()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var db = CreateDbContext(connection);

        db.Utilisateurs.Add(new Utilisateur { IdUtilisateur = 20, NomUtilisateur = "u20@x.com", MotDePasseHash = "x", Statut = true });
        await db.SaveChangesAsync();

        var service = new UtilisateurService(db);

        Assert.True(await service.ExistsByIdAsync(20));
        Assert.False(await service.ExistsByIdAsync(9999));
    }

    [Fact]
    public async Task ExistsByEmailAsync_UsesEmailUtilisateur()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var db = CreateDbContext(connection);

        db.Utilisateurs.Add(new Utilisateur { IdUtilisateur = 30, NomUtilisateur = "admin", EmailUtilisateur = "admin@prosoc.cd", MotDePasseHash = "x", Statut = true });
        await db.SaveChangesAsync();

        var service = new UtilisateurService(db);

        Assert.True(await service.ExistsByEmailAsync("admin@prosoc.cd"));
        Assert.False(await service.ExistsByEmailAsync("missing@prosoc.cd"));
    }

    [Fact]
    public async Task GetByTelephoneAsync_TrouveUtilisateurAvecVariantesDeFormat()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var db = CreateDbContext(connection);

        db.Utilisateurs.Add(new Utilisateur
        {
            IdUtilisateur = 50,
            NomUtilisateur = "agent@test.cd",
            PhoneUtilisateur = "+243891111111",
            MotDePasseHash = "x",
            Statut = true
        });
        await db.SaveChangesAsync();

        var service = new UtilisateurService(db);

        var byInternational = await service.GetByTelephoneAsync("+243891111111");
        var byLocal = await service.GetByTelephoneAsync("0891111111");
        var bySpaced = await service.GetByTelephoneAsync("+243 89 111 11 11");

        Assert.NotNull(byInternational);
        Assert.Equal(50, byInternational!.IdUtilisateur);
        Assert.NotNull(byLocal);
        Assert.Equal(50, byLocal!.IdUtilisateur);
        Assert.NotNull(bySpaced);
        Assert.Equal(50, bySpaced!.IdUtilisateur);
    }

    [Fact]
    public async Task GetByRoleAsync_ReturnsUsersWithActiveRoleAssignment()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var db = CreateDbContext(connection);

        // Arrange
        var roleId = 2;
        db.Roles.Add(new Role { IdRole = roleId, Nom = "Role-Test", Statut = true, DateCreation = DateTime.Now });

        var userA = new Utilisateur { IdUtilisateur = 40, NomUtilisateur = "a@x.com", MotDePasseHash = "x", Statut = true };
        var userB = new Utilisateur { IdUtilisateur = 41, NomUtilisateur = "b@x.com", MotDePasseHash = "x", Statut = true };
        db.Utilisateurs.AddRange(userA, userB);

        db.UserRoles.AddRange(
            new UserRole { IdUserRole = 400, UtilisateurId = userA.IdUtilisateur, RoleId = roleId, Statut = true, IsPrimary = true, DateAttribution = DateTime.Now },
            new UserRole { IdUserRole = 401, UtilisateurId = userB.IdUtilisateur, RoleId = roleId, Statut = false, IsPrimary = false, DateAttribution = DateTime.Now }
        );

        await db.SaveChangesAsync();

        var service = new UtilisateurService(db);

        // Act
        var users = await service.GetByRoleAsync(roleId);

        // Assert
        Assert.Contains(users, u => u.IdUtilisateur == userA.IdUtilisateur);
        Assert.DoesNotContain(users, u => u.IdUtilisateur == userB.IdUtilisateur);
    }
}
