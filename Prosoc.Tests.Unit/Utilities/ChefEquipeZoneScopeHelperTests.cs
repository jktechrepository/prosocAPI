using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;

namespace Prosoc.Tests.Unit.Utilities;

public class ChefEquipeZoneScopeHelperTests
{
    private static async Task<(ProsocDbContext Db, SqliteConnection Connection)> CreateDbAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return (db, connection);
    }

    private static ClaimsPrincipal ChefEquipeUser(int agentId) =>
        new(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, ChefEquipeZoneScopeHelper.RoleName),
            new Claim("AgentId", agentId.ToString())
        }, "Test"));

    [Fact]
    public async Task GetAgentIdsAtDansZoneAsync_RetourneSeulementAtDeLaZone()
    {
        var (db, connection) = await CreateDbAsync();
        await using (connection)
        await using (db)
        {
            var province = new Province { Nom = "Kinshasa", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();
            var commune1 = new Commune { Nom = "Gombe", ProvinceId = province.IdProvince, Statut = true };
            var commune2 = new Commune { Nom = "Ngaliema", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.AddRange(commune1, commune2);
            await db.SaveChangesAsync();

            var zone1 = new ZoneSociale { Nom = "Zone 1", CommuneId = commune1.IdCommune, Statut = true };
            var zone2 = new ZoneSociale { Nom = "Zone 2", CommuneId = commune2.IdCommune, Statut = true };
            db.ZonesSociales.AddRange(zone1, zone2);

            var roleAt = new Role { Nom = "Agent (AT)", Code = "AT", Statut = true };
            db.Roles.Add(roleAt);
            await db.SaveChangesAsync();

            var chef = new Agent { NomComplet = "Chef", Matricule = "CHEF-01", Phone = "0990000001", ZoneSocialeId = zone1.IdZoneSociale, Statut = true };
            var atZone = new Agent { NomComplet = "AT Zone", Matricule = "ATZ-01", Phone = "0990000002", ZoneSocialeId = zone1.IdZoneSociale, Statut = true };
            var atAutreZone = new Agent { NomComplet = "AT Hors Zone", Matricule = "ATH-01", Phone = "0990000003", ZoneSocialeId = zone2.IdZoneSociale, Statut = true };
            db.Agents.AddRange(chef, atZone, atAutreZone);
            await db.SaveChangesAsync();

            db.Utilisateurs.AddRange(
                new Utilisateur { NomUtilisateur = "at-zone", MotDePasseHash = "x", AgentId = atZone.IdAgent, Statut = true },
                new Utilisateur { NomUtilisateur = "at-hors-zone", MotDePasseHash = "x", AgentId = atAutreZone.IdAgent, Statut = true });
            await db.SaveChangesAsync();

            db.UserRoles.AddRange(
                new UserRole { UtilisateurId = db.Utilisateurs.Single(u => u.AgentId == atZone.IdAgent).IdUtilisateur, RoleId = roleAt.IdRole, Statut = true },
                new UserRole { UtilisateurId = db.Utilisateurs.Single(u => u.AgentId == atAutreZone.IdAgent).IdUtilisateur, RoleId = roleAt.IdRole, Statut = true });
            await db.SaveChangesAsync();

            var ids = await ChefEquipeZoneScopeHelper.GetAgentIdsAtDansZoneAsync(db, chef.IdAgent);

            Assert.Single(ids);
            Assert.Contains(atZone.IdAgent, ids);
            Assert.DoesNotContain(atAutreZone.IdAgent, ids);
        }
    }

    [Fact]
    public async Task EnsureAgentDansMaZoneAsync_Retourne403SiHorsZone()
    {
        var (db, connection) = await CreateDbAsync();
        await using (connection)
        await using (db)
        {
            var province = new Province { Nom = "Kinshasa", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();
            var commune1 = new Commune { Nom = "Gombe", ProvinceId = province.IdProvince, Statut = true };
            var commune2 = new Commune { Nom = "Ngaliema", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.AddRange(commune1, commune2);
            await db.SaveChangesAsync();

            var zone1 = new ZoneSociale { Nom = "Zone 1", CommuneId = commune1.IdCommune, Statut = true };
            var zone2 = new ZoneSociale { Nom = "Zone 2", CommuneId = commune2.IdCommune, Statut = true };
            db.ZonesSociales.AddRange(zone1, zone2);
            await db.SaveChangesAsync();

            var chef = new Agent { NomComplet = "Chef", Matricule = "CHEF-02", Phone = "0990000011", ZoneSocialeId = zone1.IdZoneSociale, Statut = true };
            var autre = new Agent { NomComplet = "Autre", Matricule = "AUTRE-01", Phone = "0990000012", ZoneSocialeId = zone2.IdZoneSociale, Statut = true };
            db.Agents.AddRange(chef, autre);
            await db.SaveChangesAsync();

            zone1.ChefEquipeAgentId = chef.IdAgent;
            await db.SaveChangesAsync();

            var result = await ChefEquipeZoneScopeHelper.EnsureAgentDansMaZoneAsync(
                ChefEquipeUser(chef.IdAgent), db, autre.IdAgent);

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status403Forbidden, ((ObjectResult)result!).StatusCode);
        }
    }

    [Fact]
    public async Task EnsureAgentDansMaZoneAsync_RetourneNullSiMemeZone()
    {
        var (db, connection) = await CreateDbAsync();
        await using (connection)
        await using (db)
        {
            var province = new Province { Nom = "Kinshasa", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();
            var commune1 = new Commune { Nom = "Gombe", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune1);
            await db.SaveChangesAsync();

            var zone1 = new ZoneSociale { Nom = "Zone 1", CommuneId = commune1.IdCommune, Statut = true };
            db.ZonesSociales.Add(zone1);
            await db.SaveChangesAsync();

            var chef = new Agent { NomComplet = "Chef", Matricule = "CHEF-03", Phone = "0990000021", ZoneSocialeId = zone1.IdZoneSociale, Statut = true };
            var atZone = new Agent { NomComplet = "AT Zone", Matricule = "ATZ-03", Phone = "0990000022", ZoneSocialeId = zone1.IdZoneSociale, Statut = true };
            db.Agents.AddRange(chef, atZone);
            await db.SaveChangesAsync();

            zone1.ChefEquipeAgentId = chef.IdAgent;
            await db.SaveChangesAsync();

            var result = await ChefEquipeZoneScopeHelper.EnsureAgentDansMaZoneAsync(
                ChefEquipeUser(chef.IdAgent), db, atZone.IdAgent);

            Assert.Null(result);
        }
    }
}
