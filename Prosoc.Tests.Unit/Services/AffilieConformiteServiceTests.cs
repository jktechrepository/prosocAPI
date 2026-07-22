using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class AffilieConformiteServiceTests
{
    [Fact]
    public async Task GetConformiteAffilie_AvecArriereCotisation_RetourneHorsOrdre()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>().UseSqlite(connection).Options;
        await using var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var affilie = new Affilie
        {
            CodeAdhesion = "AFF-CF-1",
            Nom = "Test",
            Prenom = "Conformite",
            NomComplet = "Test Conformite",
            DateNaissance = new DateTime(1990, 1, 1),
            Statut = true
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        var devise = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
        db.Devises.Add(devise);
        await db.SaveChangesAsync();

        db.ArrieresAffilie.Add(new ArrieresAffilie
        {
            AffilieId = affilie.IdAffilie,
            TypeObligation = TypeCollecte.Cotisation,
            Mois = 5,
            Annee = 2026,
            DateEcheance = DateTime.Today.AddDays(-10),
            MontantAttendu = 100m,
            MontantPaye = 0m,
            RestAPayer = 100m,
            DeviseId = devise.IdDevise,
            StatutPaiement = ArrieresAffilieStatuts.EnRetard,
            Periodicite = "Mensuel",
            Statut = true
        });
        await db.SaveChangesAsync();

        var arrieresProvider = new Mock<IParametresMetierProvider>();
        arrieresProvider.Setup(p => p.GetArrieresAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProsocAPI.Models.Configuration.ArrieresOptions());

        var arrieresService = new ArrieresAffilieService(
            db,
            Mock.Of<ICotisationAffilieMetierService>(),
            arrieresProvider.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ArrieresAffilieService>>());

        var service = new AffilieConformiteService(db, arrieresService);
        var result = await service.GetConformiteAffilieAsync(affilie.IdAffilie);

        Assert.NotNull(result);
        Assert.Equal(AffilieConformiteStatuts.HorsOrdre, result!.StatutCotisation);
        Assert.Equal(AffilieConformiteStatuts.HorsOrdre, result.StatutGlobal);
        Assert.Equal(AffilieConformiteStatuts.EnOrdre, result.StatutPrestation);
        Assert.Equal(1, result.NombreArrieresOuverts);
        Assert.Equal(100m, result.MontantRestantDu);
    }
}
