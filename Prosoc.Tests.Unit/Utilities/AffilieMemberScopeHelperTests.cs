using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Models.Core;

namespace Prosoc.Tests.Unit.Utilities;

public class AffilieMemberScopeHelperTests
{
    private static (ProsocDbContext Db, SqliteConnection Connection) CreateDb()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new ProsocDbContext(options);
        db.Database.EnsureCreated();
        return (db, connection);
    }

    private static ClaimsPrincipal MembreAffilie(int affilieId) =>
        new(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Affilié"),
            new Claim("AffilieId", affilieId.ToString())
        }, "Test"));

    private static ClaimsPrincipal AdminUser() =>
        new(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin")
        }, "Test"));

    [Fact]
    public void DenyListAccessForMembre_Retourne403PourMembre()
    {
        var result = AffilieMemberScopeHelper.DenyListAccessForMembre(MembreAffilie(1), "des affiliés");

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status403Forbidden, ((ObjectResult)result).StatusCode);
    }

    [Fact]
    public void DenyListAccessForMembre_RetourneNullPourAdmin()
    {
        var result = AffilieMemberScopeHelper.DenyListAccessForMembre(AdminUser(), "des affiliés");
        Assert.Null(result);
    }

    [Fact]
    public async Task EnsureOwnAffilieScopeAsync_BloqueAccesAutreAffilie()
    {
        var (db, connection) = CreateDb();
        await using (connection)
        await using (db)
        {
            var result = await AffilieMemberScopeHelper.EnsureOwnAffilieScopeAsync(
                MembreAffilie(1), db, targetAffilieId: 99);

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status403Forbidden, ((ObjectResult)result).StatusCode);
        }
    }

    [Fact]
    public async Task EnsureOwnDependantScopeAsync_AutoriseDependantDuMembre()
    {
        var (db, connection) = CreateDb();
        await using (connection)
        await using (db)
        {
            db.Affilies.Add(new Affilie
            {
                IdAffilie = 5,
                CodeAdhesion = "AFF-005",
                Nom = "Test",
                Prenom = "User",
                NomComplet = "User Test",
                DateNaissance = new DateTime(1990, 1, 1)
            });
            db.Dependants.Add(new Dependant
            {
                IdDependant = 10,
                Nom = "Enfant",
                AffilieId = 5,
                LienParente = "Fils",
                Adresse = "Kin"
            });
            await db.SaveChangesAsync();

            var result = await AffilieMemberScopeHelper.EnsureOwnDependantScopeAsync(
                MembreAffilie(5), db, dependantId: 10);

            Assert.Null(result);
        }
    }
}
