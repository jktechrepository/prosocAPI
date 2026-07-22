using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class PenaliteAffilieServiceTests
{
  private static async Task<(ProsocDbContext db, SqliteConnection connection)> CreateDbAsync()
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

  private static PenaliteAffilieService CreateService(
      ProsocDbContext db,
      int fraisPenaliteId = 1,
      int delaiGraceJours = 3)
  {
    var penaliteOptions = new PenaliteOptions
    {
      ApplicationAutomatiqueActivee = true,
      DelaiGraceJours = delaiGraceJours,
      FraisPenaliteCode = FraisCodes.PenaliteRetardCotisation,
      RetardCotisationActive = true
    };
    var provider = new Mock<IParametresMetierProvider>();
    provider.Setup(p => p.GetPenaliteAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(penaliteOptions);
    var logger = new Mock<ILogger<PenaliteAffilieService>>();
    return new PenaliteAffilieService(db, provider.Object, logger.Object);
  }

  private static async Task<(Devise devise, Frais fraisPenalite, Affilie affilie, ArrieresAffilie arriere)> SeedArriereCotisationAsync(
      ProsocDbContext db,
      DateTime dateEcheance,
      decimal restAPayer = 100m)
  {
    var devise = new Devise { Code = "CDF", Nom = "Franc", Statut = true };
    db.Devises.Add(devise);
    await db.SaveChangesAsync();

    var fraisPenalite = new Frais
    {
      Code = FraisCodes.PenaliteRetardCotisation,
      Libelle = "Penalite Retard Cotisation",
      Montant = 5000,
      DeviseId = devise.IdDevise,
      Periodicite = "Ponctuel",
      Statut = true
    };
    db.Frais.Add(fraisPenalite);

    var affilie = new Affilie
    {
      CodeAdhesion = "ADH-PEN-001",
      Nom = "Test",
      Prenom = "Affilie",
      NomComplet = "Test Affilie",
      DateNaissance = new DateTime(1990, 1, 1),
      Statut = true
    };
    db.Affilies.Add(affilie);
    await db.SaveChangesAsync();

    var categorie = new CategorieAdhesion { Libelle = "Particulier", Statut = true };
    db.CategoriesAdhesions.Add(categorie);
    await db.SaveChangesAsync();

    var typeAdhesion = new TypeAdhesion
    {
      Libelle = "Solo",
      MaxDependants = 0,
      CategorieAdhesionId = categorie.IdCategorieAdhesion,
      Montant = 1m,
      Statut = true
    };
    db.TypeAdhesions.Add(typeAdhesion);
    await db.SaveChangesAsync();

    var cotisation = new CotisationAffilie
    {
      Montant = 100m,
      Periodicite = "Mensuel",
      TypeAdhesionId = typeAdhesion.IdTypeAdhesion,
      DeviseId = devise.IdDevise,
      Statut = true
    };
    db.CotisationsAffilie.Add(cotisation);
    await db.SaveChangesAsync();

    var arriere = new ArrieresAffilie
    {
      AffilieId = affilie.IdAffilie,
      TypeObligation = TypeCollecte.Cotisation,
      CotisationAffilieId = cotisation.IdCotisationAffilie,
      Mois = dateEcheance.Month,
      Annee = dateEcheance.Year,
      DateEcheance = dateEcheance,
      Periodicite = "Mensuel",
      MontantAttendu = 100m,
      MontantPaye = 100m - restAPayer,
      RestAPayer = restAPayer,
      DeviseId = devise.IdDevise,
      Description = "Cotisation test",
      StatutPaiement = ArrieresAffilieStatuts.EnRetard,
      Statut = true
    };
    db.ArrieresAffilie.Add(arriere);
    await db.SaveChangesAsync();

    return (devise, fraisPenalite, affilie, arriere);
  }

  [Fact]
  public async Task AppliquerPenalites_AvantDelaiGrace_NeCreeAucunePenalite()
  {
    var (db, connection) = await CreateDbAsync();
    await using (connection)
    await using (db)
    {
      var echeance = new DateTime(2026, 3, 1);
      var (_, fraisPenalite, _, _) = await SeedArriereCotisationAsync(db, echeance);
      var service = CreateService(db, fraisPenalite.IdFrais);

      var result = await service.AppliquerPenalitesRetardCotisationAsync(echeance.AddDays(2));

      Assert.Empty(result);
      Assert.Empty(await db.PenalitesAffilie.ToListAsync());
    }
  }

  [Fact]
  public async Task AppliquerPenalites_ApresDelaiGrace_CreeUnePenalite()
  {
    var (db, connection) = await CreateDbAsync();
    await using (connection)
    await using (db)
    {
      var echeance = new DateTime(2026, 3, 1);
      var (_, fraisPenalite, affilie, arriere) = await SeedArriereCotisationAsync(db, echeance);
      var service = CreateService(db, fraisPenalite.IdFrais);

      var result = await service.AppliquerPenalitesRetardCotisationAsync(echeance.AddDays(3));

      Assert.Single(result);
      var penalite = await db.PenalitesAffilie.SingleAsync();
      Assert.Equal(affilie.IdAffilie, penalite.AffilieId);
      Assert.Equal(arriere.IdArrieresAffilie, penalite.ArrieresAffilieId);
      Assert.Equal(5000m, penalite.Montant);
      Assert.Equal(PenaliteAffilieStatuts.Appliquee, penalite.Statut);
      Assert.Equal(TypePenalite.RetardCotisation, penalite.TypePenalite);
    }
  }

  [Fact]
  public async Task AppliquerPenalites_DeuxFois_NeDupliquePas()
  {
    var (db, connection) = await CreateDbAsync();
    await using (connection)
    await using (db)
    {
      var echeance = new DateTime(2026, 3, 1);
      var (_, fraisPenalite, _, _) = await SeedArriereCotisationAsync(db, echeance);
      var service = CreateService(db, fraisPenalite.IdFrais);
      var date = echeance.AddDays(10);

      await service.AppliquerPenalitesRetardCotisationAsync(date);
      await service.AppliquerPenalitesRetardCotisationAsync(date);

      Assert.Equal(1, await db.PenalitesAffilie.CountAsync());
    }
  }

  [Fact]
  public async Task ProcessCollecteForPenaliteAsync_MarquePenalitePayee()
  {
    var (db, connection) = await CreateDbAsync();
    await using (connection)
    await using (db)
    {
      var echeance = new DateTime(2026, 3, 1);
      var (_, fraisPenalite, affilie, arriere) = await SeedArriereCotisationAsync(db, echeance);
      var service = CreateService(db, fraisPenalite.IdFrais);
      await service.AppliquerPenalitesRetardCotisationAsync(echeance.AddDays(5));

      var penalite = await db.PenalitesAffilie.SingleAsync();
      var agent = new Agent
      {
        NomComplet = "Agent Penalite",
        Matricule = "AG-PEN-001",
        Phone = "0990000099",
        Statut = true
      };
      db.Agents.Add(agent);
      await db.SaveChangesAsync();

      var collecte = new Collecte
      {
        TypeCollecte = TypeCollecte.Frais,
        FraisId = fraisPenalite.IdFrais,
        PenaliteAffilieId = penalite.IdPenaliteAffilie,
        AffilieId = affilie.IdAffilie,
        AgentId = agent.IdAgent,
        Montant = 5000m,
        DeviseId = penalite.DeviseId,
        Statut = true
      };
      db.Collectes.Add(collecte);
      await db.SaveChangesAsync();

      await service.ProcessCollecteForPenaliteAsync(collecte);

      var updated = await db.PenalitesAffilie.SingleAsync();
      Assert.Equal(PenaliteAffilieStatuts.Payee, updated.Statut);
      Assert.NotNull(updated.DatePaiement);
    }
  }

  [Fact]
  public async Task AnnulerPenaliteAsync_PenaliteAppliquee_PasseEnAnnulee()
  {
    var (db, connection) = await CreateDbAsync();
    await using (connection)
    await using (db)
    {
      var echeance = new DateTime(2026, 3, 1);
      var (_, fraisPenalite, _, _) = await SeedArriereCotisationAsync(db, echeance);
      var service = CreateService(db, fraisPenalite.IdFrais);
      await service.AppliquerPenalitesRetardCotisationAsync(echeance.AddDays(4));

      var penalite = await db.PenalitesAffilie.SingleAsync();
      await service.AnnulerPenaliteAsync(penalite.IdPenaliteAffilie, "Geste commercial");

      var updated = await db.PenalitesAffilie.SingleAsync();
      Assert.Equal(PenaliteAffilieStatuts.Annulee, updated.Statut);
      Assert.Equal("Geste commercial", updated.MotifAnnulation);
    }
  }
}
