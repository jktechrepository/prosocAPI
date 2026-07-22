using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Unit.Services;

public class TerritorialEncadrementServiceTests
{
    private sealed class FakeUtilisateurRepository : IUtilisateurRepository
    {
        public List<(int UserId, int RoleId)> AddedRoles { get; } = new();
        public List<(int UserId, int RoleId)> RemovedRoles { get; } = new();

        public Task<bool> AddRoleToUserAsync(int userId, int roleId, int? assignedByUserId = null, bool isPrimary = false, CancellationToken ct = default)
        {
            AddedRoles.Add((userId, roleId));
            return Task.FromResult(true);
        }

        public Task<bool> RemoveRoleFromUserAsync(int userId, int roleId, CancellationToken ct = default)
        {
            RemovedRoles.Add((userId, roleId));
            return Task.FromResult(true);
        }

        public Task<Utilisateur> CreateAsync(Utilisateur entity, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(int id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<bool> ExistsByIdAsync(int id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Utilisateur>> GetAllAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Utilisateur?> GetByDefaultUsernameAsync(string defaultUsername, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Utilisateur?> GetByEmailAsync(string email, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Utilisateur?> GetByIdAsync(int id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Utilisateur?> GetByNomUtilisateurAsync(string nomUtilisateur, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Utilisateur?> GetByTelephoneAsync(string telephone, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Utilisateur>> GetByRoleAsync(int roleId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Utilisateur>> GetByStatutAsync(bool statut, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Permission>> GetUserPermissionsAsync(int userId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<UserRole>> GetUserRolesAsync(int userId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Role>> GetUserRolesEntitiesAsync(int userId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<bool> SetPrimaryRoleAsync(int userId, int roleId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Utilisateur?> UpdateAsync(int id, Utilisateur entity, CancellationToken ct = default) => throw new NotImplementedException();
    }

    private static async Task<(ProsocDbContext Db, SqliteConnection Connection, FakeUtilisateurRepository Users)> CreateFixtureAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return (db, connection, new FakeUtilisateurRepository());
    }

    [Fact]
    public async Task AssignChefEquipeAsync_AffecteTitulaireEtSyncRole()
    {
        var (db, connection, users) = await CreateFixtureAsync();
        await using (connection)
        await using (db)
        {
            var province = new Province { Nom = "Kinshasa", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();
            var commune = new Commune { Nom = "Gombe", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune);
            await db.SaveChangesAsync();
            var zone = new ZoneSociale { Nom = "Zone A", CommuneId = commune.IdCommune, Statut = true };
            db.ZonesSociales.Add(zone);
            var roleCe = new Role { Nom = ChefEquipeZoneScopeHelper.RoleName, Code = "CE", Statut = true };
            db.Roles.Add(roleCe);
            await db.SaveChangesAsync();

            var chef = new Agent { NomComplet = "Chef", Matricule = "CE-01", Phone = "0991000001", ZoneSocialeId = zone.IdZoneSociale, Statut = true };
            db.Agents.Add(chef);
            await db.SaveChangesAsync();

            var utilisateur = new Utilisateur { NomUtilisateur = "chef", MotDePasseHash = "x", AgentId = chef.IdAgent, Statut = true };
            db.Utilisateurs.Add(utilisateur);
            await db.SaveChangesAsync();

            var service = new TerritorialEncadrementService(db, users, NullLogger<TerritorialEncadrementService>.Instance);
            var result = await service.AssignChefEquipeAsync(zone.IdZoneSociale, chef.IdAgent);

            var zoneDb = await db.ZonesSociales.SingleAsync(z => z.IdZoneSociale == zone.IdZoneSociale);
            Assert.Equal(chef.IdAgent, zoneDb.ChefEquipeAgentId);
            Assert.Equal(chef.IdAgent, result.NewAgentId);
            Assert.Contains((utilisateur.IdUtilisateur, roleCe.IdRole), users.AddedRoles);
        }
    }

    [Fact]
    public async Task AssignChefEquipeAsync_RefuseAgentHorsZone()
    {
        var (db, connection, users) = await CreateFixtureAsync();
        await using (connection)
        await using (db)
        {
            var province = new Province { Nom = "Kinshasa", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();
            var commune = new Commune { Nom = "Gombe", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune);
            await db.SaveChangesAsync();
            var zoneA = new ZoneSociale { Nom = "Zone A", CommuneId = commune.IdCommune, Statut = true };
            var zoneB = new ZoneSociale { Nom = "Zone B", CommuneId = commune.IdCommune, Statut = true };
            db.ZonesSociales.AddRange(zoneA, zoneB);
            await db.SaveChangesAsync();

            var agent = new Agent { NomComplet = "Hors zone", Matricule = "HZ-01", Phone = "0991000002", ZoneSocialeId = zoneB.IdZoneSociale, Statut = true };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            var service = new TerritorialEncadrementService(db, users, NullLogger<TerritorialEncadrementService>.Instance);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AssignChefEquipeAsync(zoneA.IdZoneSociale, agent.IdAgent));
        }
    }

    [Fact]
    public async Task AssignSuperviseurAsync_RemplaceTitulairePrecedent()
    {
        var (db, connection, users) = await CreateFixtureAsync();
        await using (connection)
        await using (db)
        {
            var province = new Province { Nom = "Kinshasa", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();
            var commune = new Commune { Nom = "Gombe", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune);
            await db.SaveChangesAsync();
            var zone = new ZoneSociale { Nom = "Zone A", CommuneId = commune.IdCommune, Statut = true };
            db.ZonesSociales.Add(zone);
            var roleSp = new Role { Nom = SuperviseurTerritoryScopeHelper.RoleName, Code = "SP", Statut = true };
            db.Roles.Add(roleSp);
            await db.SaveChangesAsync();

            var ancien = new Agent { NomComplet = "Ancien SP", Matricule = "SP-OLD", Phone = "0992000001", ZoneSocialeId = zone.IdZoneSociale, Statut = true };
            var nouveau = new Agent { NomComplet = "Nouveau SP", Matricule = "SP-NEW", Phone = "0992000002", ZoneSocialeId = zone.IdZoneSociale, Statut = true };
            db.Agents.AddRange(ancien, nouveau);
            await db.SaveChangesAsync();

            commune.SuperviseurAgentId = ancien.IdAgent;
            await db.SaveChangesAsync();

            db.Utilisateurs.AddRange(
                new Utilisateur { NomUtilisateur = "ancien-sp", MotDePasseHash = "x", AgentId = ancien.IdAgent, Statut = true },
                new Utilisateur { NomUtilisateur = "nouveau-sp", MotDePasseHash = "x", AgentId = nouveau.IdAgent, Statut = true });
            await db.SaveChangesAsync();

            var service = new TerritorialEncadrementService(db, users, NullLogger<TerritorialEncadrementService>.Instance);
            var result = await service.AssignSuperviseurAsync(commune.IdCommune, nouveau.IdAgent);

            var communeDb = await db.Communes.SingleAsync(c => c.IdCommune == commune.IdCommune);
            Assert.Equal(nouveau.IdAgent, communeDb.SuperviseurAgentId);
            Assert.Equal(ancien.IdAgent, result.PreviousAgentId);
            Assert.Equal(nouveau.IdAgent, result.NewAgentId);
        }
    }

    [Fact]
    public async Task ReleaseTitularitesForAgentAsync_LibereFkEtRetireRoles()
    {
        var (db, connection, users) = await CreateFixtureAsync();
        await using (connection)
        await using (db)
        {
            var province = new Province { Nom = "Kinshasa", Statut = true };
            db.Provinces.Add(province);
            await db.SaveChangesAsync();
            var commune = new Commune { Nom = "Gombe", ProvinceId = province.IdProvince, Statut = true };
            db.Communes.Add(commune);
            await db.SaveChangesAsync();
            var zone = new ZoneSociale { Nom = "Zone A", CommuneId = commune.IdCommune, Statut = true };
            db.ZonesSociales.Add(zone);
            var roleCe = new Role { Nom = ChefEquipeZoneScopeHelper.RoleName, Code = "CE", Statut = true };
            var roleSp = new Role { Nom = SuperviseurTerritoryScopeHelper.RoleName, Code = "SP", Statut = true };
            db.Roles.AddRange(roleCe, roleSp);
            await db.SaveChangesAsync();

            var agent = new Agent { NomComplet = "Titulaire", Matricule = "TIT-01", Phone = "0995000001", ZoneSocialeId = zone.IdZoneSociale, Statut = true };
            db.Agents.Add(agent);
            await db.SaveChangesAsync();

            zone.ChefEquipeAgentId = agent.IdAgent;
            commune.SuperviseurAgentId = agent.IdAgent;
            await db.SaveChangesAsync();

            var utilisateur = new Utilisateur { NomUtilisateur = "titulaire", MotDePasseHash = "x", AgentId = agent.IdAgent, Statut = true };
            db.Utilisateurs.Add(utilisateur);
            await db.SaveChangesAsync();

            var service = new TerritorialEncadrementService(db, users, NullLogger<TerritorialEncadrementService>.Instance);
            await service.ReleaseTitularitesForAgentAsync(agent.IdAgent);

            var zoneDb = await db.ZonesSociales.SingleAsync(z => z.IdZoneSociale == zone.IdZoneSociale);
            var communeDb = await db.Communes.SingleAsync(c => c.IdCommune == commune.IdCommune);
            Assert.Null(zoneDb.ChefEquipeAgentId);
            Assert.Null(communeDb.SuperviseurAgentId);
            Assert.Contains((utilisateur.IdUtilisateur, roleCe.IdRole), users.RemovedRoles);
            Assert.Contains((utilisateur.IdUtilisateur, roleSp.IdRole), users.RemovedRoles);
        }
    }
}
