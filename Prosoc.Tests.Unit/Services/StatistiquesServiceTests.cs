using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Statistiques;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class StatistiquesServiceTests
{
    [Fact]
    public async Task GetGeneralesAsync_CalculeTotauxDeBase()
    {
        await RunAsync(async (service, db) =>
        {
            await SeedAsync(db);

            var result = await service.GetGeneralesAsync(new StatistiquesFiltresDto());

            Assert.True(result.TotalAffilies >= 1);
            Assert.True(result.TotalCollectesMois >= 50m);
            Assert.True(result.NombreCollectesMois >= 1);
        });
    }

    [Fact]
    public async Task GetConsolideesAsync_RetourneTousLesBlocs()
    {
        await RunAsync(async (service, db) =>
        {
            await SeedAsync(db);

            var result = await service.GetConsolideesAsync(new StatistiquesFiltresDto());

            Assert.NotNull(result.Generales);
            Assert.NotNull(result.Financieres);
            Assert.NotNull(result.Operationnelles);
            Assert.NotNull(result.Performance);
        });
    }

    [Fact]
    public async Task GetGeneralesAsync_FiltreZoneSocialeId_RetourneAffiliesDeLaZone()
    {
        await RunAsync(async (service, db) =>
        {
            var ids = await SeedAsync(db);

            var resultZone = await service.GetGeneralesAsync(new StatistiquesFiltresDto { ZoneSocialeId = ids.ZoneId });
            var resultAutre = await service.GetGeneralesAsync(new StatistiquesFiltresDto { ZoneSocialeId = 99999 });

            Assert.Equal(1, resultZone.TotalAffilies);
            Assert.Equal(0, resultAutre.TotalAffilies);
        });
    }

    [Fact]
    public async Task GetGeneralesAsync_FiltreCategorieAdhesionId_RetourneAffiliesDeLaCategorie()
    {
        await RunAsync(async (service, db) =>
        {
            var ids = await SeedAsync(db);

            var result = await service.GetGeneralesAsync(new StatistiquesFiltresDto { CategorieAdhesionId = ids.CategorieId });
            var resultAutre = await service.GetGeneralesAsync(new StatistiquesFiltresDto { CategorieAdhesionId = 99999 });

            Assert.Equal(1, result.TotalAffilies);
            Assert.Equal(0, resultAutre.TotalAffilies);
        });
    }

    [Fact]
    public async Task GetOperationnellesAsync_AffilieActivite_CalculeActifsEtInactifs()
    {
        await RunAsync(async (service, db) =>
        {
            var ids = await SeedAsync(db, includeInactif: true);

            var result = await service.GetOperationnellesAsync(new StatistiquesFiltresDto());

            Assert.Equal(2, result.AffilieActivite.TotalAffilies);
            Assert.Equal(1, result.AffilieActivite.NombreAffiliesActifs);
            Assert.Equal(1, result.AffilieActivite.NombreAffiliesInactifs);
            Assert.Equal(50m, result.AffilieActivite.PourcentageActifs);
            Assert.Equal(50m, result.AffilieActivite.PourcentageInactifs);
        });
    }

    private static async Task RunAsync(Func<StatistiquesService, ProsocDbContext, Task> test)
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var service = new StatistiquesService(db, new Mock<ILogger<StatistiquesService>>().Object);
        await test(service, db);
    }

    private static async Task<(int ZoneId, int CategorieId)> SeedAsync(ProsocDbContext db, bool includeInactif = false)
    {
        var devise = new Devise { Code = "USD", Nom = "Dollar", Statut = true, EstDevisePrincipale = true };
        db.Devises.Add(devise);

        var categorie = new CategorieAdhesion { Libelle = "Standard", Statut = true };
        db.CategoriesAdhesions.Add(categorie);
        await db.SaveChangesAsync();

        var type = new TypeAdhesion
        {
            Libelle = "Solo",
            MaxDependants = 0,
            Montant = 10m,
            DeviseId = devise.IdDevise,
            CategorieAdhesionId = categorie.IdCategorieAdhesion,
            Statut = true
        };
        db.TypeAdhesions.Add(type);

        var province = new Province { Nom = "Kinshasa", Statut = true };
        db.Provinces.Add(province);
        await db.SaveChangesAsync();

        var commune = new Commune { Nom = "Gombe", ProvinceId = province.IdProvince, Statut = true };
        db.Communes.Add(commune);
        await db.SaveChangesAsync();

        var zone = new ZoneSociale { Nom = "Zone A", CommuneId = commune.IdCommune, Statut = true };
        db.ZonesSociales.Add(zone);
        await db.SaveChangesAsync();

        var agent = new Agent
        {
            NomComplet = "Agent Test",
            Matricule = "AG-STATS-001",
            Phone = "0999999999",
            RoleAgent = "Caissier",
            ZoneSocialeId = zone.IdZoneSociale,
            Statut = true
        };
        db.Agents.Add(agent);

        var affilie = new Affilie
        {
            CodeAdhesion = "AFF-STATS-001",
            Nom = "Doe",
            Prenom = "Jane",
            NomComplet = "Jane Doe",
            DateNaissance = new DateTime(1990, 1, 1),
            Statut = true
        };
        db.Affilies.Add(affilie);

        if (includeInactif)
        {
            db.Affilies.Add(new Affilie
            {
                CodeAdhesion = "AFF-STATS-002",
                Nom = "Doe",
                Prenom = "John",
                NomComplet = "John Doe",
                DateNaissance = new DateTime(1990, 1, 1),
                Statut = false
            });
        }

        var user = new Utilisateur
        {
            NomUtilisateur = "stats-user",
            MotDePasseHash = "hash",
            Statut = true
        };
        db.Utilisateurs.Add(user);
        await db.SaveChangesAsync();

        db.Adhesions.Add(new Adhesion
        {
            AgentId = agent.IdAgent,
            AffilieId = affilie.IdAffilie,
            TypeAdhesionId = type.IdTypeAdhesion,
            UtilisateurId = user.IdUtilisateur,
            StatutDossier = "VALIDE",
            Statut = true,
            DateCreation = DateTime.Now
        });

        if (includeInactif)
        {
            var inactif = await db.Affilies.FirstAsync(a => a.CodeAdhesion == "AFF-STATS-002");
            db.Adhesions.Add(new Adhesion
            {
                AgentId = agent.IdAgent,
                AffilieId = inactif.IdAffilie,
                TypeAdhesionId = type.IdTypeAdhesion,
                UtilisateurId = user.IdUtilisateur,
                StatutDossier = "VALIDE",
                Statut = true,
                DateCreation = DateTime.Now
            });
        }

        db.Collectes.Add(new Collecte
        {
            AffilieId = affilie.IdAffilie,
            AgentId = agent.IdAgent,
            DeviseId = devise.IdDevise,
            TypeCollecte = TypeCollecte.Cotisation,
            Montant = 50m,
            MontantDevisePrincipale = 50m,
            Mois = DateTime.Now.Month,
            Annee = DateTime.Now.Year,
            StatutPaiement = CollecteStatutPaiement.Valide,
            ModePaiement = "ESPECE",
            Statut = true,
            DateCollecte = DateTime.Now
        });

        db.ArrieresAffilie.Add(new ArrieresAffilie
        {
            AffilieId = affilie.IdAffilie,
            TypeObligation = TypeCollecte.Cotisation,
            Mois = DateTime.Now.Month,
            Annee = DateTime.Now.Year,
            DateEcheance = DateTime.Now,
            Periodicite = "Mensuel",
            MontantAttendu = 100m,
            MontantPaye = 50m,
            RestAPayer = 50m,
            DeviseId = devise.IdDevise,
            Description = "Arriere test",
            StatutPaiement = ArrieresAffilieStatuts.PartiellementPaye,
            Statut = true
        });

        await db.SaveChangesAsync();
        return (zone.IdZoneSociale, categorie.IdCategorieAdhesion);
    }
}
