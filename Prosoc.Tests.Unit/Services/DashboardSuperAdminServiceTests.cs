using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.DashboardAdmin;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Unit.Services;

public class DashboardSuperAdminServiceTests
{
    private static async Task<(ProsocDbContext Db, DashboardSuperAdminService Service)> CreateContextAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var adminMock = new Mock<IDashboardAdminRepository>();
        adminMock.Setup(x => x.GetKpisAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DashboardAdminKpisDto { TotalAffilies = 10 });
        adminMock.Setup(x => x.GetTopAgentsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PerformanceAgentsDto>());
        adminMock.Setup(x => x.GetCollectesEnAttenteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CollecteEnAttenteDto>());

        var service = new DashboardSuperAdminService(
            db,
            adminMock.Object,
            new Mock<ILogger<DashboardSuperAdminService>>().Object);

        return (db, service);
    }

    [Fact]
    public async Task GetKpisSystemeAsync_CompteUtilisateursEtFlexPay()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            db.Utilisateurs.AddRange(
                new Utilisateur { NomUtilisateur = "actif", MotDePasseHash = "h", Statut = true },
                new Utilisateur { NomUtilisateur = "inactif", MotDePasseHash = "h", Statut = false },
                new Utilisateur { NomUtilisateur = "pwd", MotDePasseHash = "h", Statut = true, DoitChangerMotDePasse = true });

            db.InfoPaiementsMarchand.Add(new InfoPaiementMarchand
            {
                CodeMarchand = "M1",
                ApiToken = "token",
                Statut = true,
                ActifMobileMoney = true,
                ActifCarteBancaire = false
            });

            db.CollectesEnAttente.Add(new CollecteEnAttente
            {
                SourceFlux = CollecteEnAttenteSourceFlux.CollecteAgent,
                StatutEnAttente = CollecteEnAttenteStatut.EnAttente,
                TypeCollecte = TypeCollecte.Cotisation,
                MethodePaiement = "MOBILE_MONEY",
                MontantTarif = 10,
                DeviseTarifId = 1,
                MontantFlexPay = 10,
                CodeDevisePaiement = "CDF",
                PayloadMetierJson = "{}",
                DateExpiration = DateTime.UtcNow.AddHours(1)
            });

            await db.SaveChangesAsync();

            var kpis = await service.GetKpisSystemeAsync();

            Assert.Equal(2, kpis.TotalUtilisateursActifs);
            Assert.Equal(1, kpis.TotalUtilisateursInactifs);
            Assert.Equal(1, kpis.UtilisateursDoiventChangerMotDePasse);
            Assert.True(kpis.FlexPayMarchandConfigure);
            Assert.True(kpis.FlexPayMobileMoneyActif);
            Assert.False(kpis.FlexPayCarteBancaireActif);
            Assert.Equal(1, kpis.CollectesFlexPayEnAttente);
        }
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_InclutKpisAdmin()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var summary = await service.GetDashboardSummaryAsync();

            Assert.Equal(10, summary.KpisAdmin.TotalAffilies);
            Assert.NotNull(summary.KpisSysteme);
        }
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_ExposeDevisePrincipaleCode()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var adminService = new DashboardAdminService(
            db,
            new DeviseConversionService(db),
            new Mock<ILogger<DashboardAdminService>>().Object);

        db.Devises.Add(new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true });
        await db.SaveChangesAsync();

        var service = new DashboardSuperAdminService(
            db,
            adminService,
            new Mock<ILogger<DashboardSuperAdminService>>().Object);

        var summary = await service.GetDashboardSummaryAsync();

        Assert.Equal("USD", summary.KpisAdmin.DevisePrincipaleCode);
        Assert.Equal("USD", summary.DevisePrincipaleCode);
    }

    [Fact]
    public async Task GetUtilisateursParRoleAsync_CompteParRole()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var roleAdmin = new Role { Nom = "Admin", Code = "AD", Statut = true, DateCreation = DateTime.Now };
            var roleIt = new Role { Nom = "IT", Code = "IT", Statut = true, DateCreation = DateTime.Now };
            db.Roles.AddRange(roleAdmin, roleIt);

            var u1 = new Utilisateur { NomUtilisateur = "u1", MotDePasseHash = "h", Statut = true };
            var u2 = new Utilisateur { NomUtilisateur = "u2", MotDePasseHash = "h", Statut = true };
            db.Utilisateurs.AddRange(u1, u2);
            await db.SaveChangesAsync();

            db.UserRoles.AddRange(
                new UserRole { UtilisateurId = u1.IdUtilisateur, RoleId = roleAdmin.IdRole, Statut = true },
                new UserRole { UtilisateurId = u2.IdUtilisateur, RoleId = roleAdmin.IdRole, Statut = true },
                new UserRole { UtilisateurId = u2.IdUtilisateur, RoleId = roleIt.IdRole, Statut = true });
            await db.SaveChangesAsync();

            var repartition = await service.GetUtilisateursParRoleAsync();

            Assert.Equal(2, repartition.First(r => r.RoleNom == "Admin").NombreUtilisateurs);
            Assert.Equal(1, repartition.First(r => r.RoleNom == "IT").NombreUtilisateurs);
        }
    }
}
