using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class BonEnvoiGetAllNullJetonMedicalTests
{
    [Fact]
    public async Task BonsEnvoiQuery_WithNullJetonMedicalId_MaterializesWithoutInvalidCast()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var devise = new Devise
        {
            Code = "USD",
            Nom = "Dollar",
            EstDevisePrincipale = true,
            Statut = true
        };
        db.Devises.Add(devise);
        await db.SaveChangesAsync();

        var affilie = new Affilie
        {
            Nom = "Test",
            Prenom = "Affilie",
            NomComplet = "Test Affilie",
            DateNaissance = new DateTime(1990, 1, 1),
            CodeAdhesion = "AFF-BON-1",
            Statut = true
        };
        db.Affilies.Add(affilie);

        var prestation = new Prestation
        {
            NomPrestation = "Consultation",
            Description = "Desc",
            Montant = 20,
            DeviseId = devise.IdDevise,
            Statut = true
        };
        db.Prestations.Add(prestation);
        await db.SaveChangesAsync();

        db.BonsEnvoi.Add(new BonEnvoi
        {
            NumeroBon = "BE-NULL-JETON",
            AffilieId = affilie.IdAffilie,
            PrestationId = prestation.IdPrestation,
            JetonMedicalId = null,
            Statut = true
        });
        await db.SaveChangesAsync();

        var paginationService = new PaginationService(
            Mock.Of<ILogger<PaginationService>>(),
            Options.Create(new PaginationOptions()));

        var query = db.BonsEnvoi
            .Include(b => b.Affilie)
            .Include(b => b.Prestation)
            .Include(b => b.JetonMedical);

        var result = await paginationService.CreatePaginatedResponseAsync(
            query,
            new PaginationRequest { Page = 1, PageSize = 10 });

        Assert.Single(result.Data);
        Assert.Null(result.Data[0].JetonMedicalId);
        Assert.Equal("BE-NULL-JETON", result.Data[0].NumeroBon);
    }
}
