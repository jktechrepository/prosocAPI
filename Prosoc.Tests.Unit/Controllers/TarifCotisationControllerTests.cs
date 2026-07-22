using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Prosoc.Data;
using ProsocAPI.Controllers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Unit.Controllers;

public class TarifCotisationControllerTests
{
    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Prosoc.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    private static TarifCotisationController CreateController(
        ITarifCotisationRepository repo,
        bool exposeExceptionDetails = true)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new ProsocDbContext(options);
        db.Database.EnsureCreated();

        var paginationService = new Mock<IPaginationService>().Object;
        var paginationOptions = Options.Create(new PaginationOptions());

        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment { EnvironmentName = Environments.Development });
        services.Configure<ErrorHandlingOptions>(o => o.ExposeExceptionDetails = exposeExceptionDetails);
        services.AddHttpContextAccessor();
        services.AddLogging();
        services.AddScoped<ErrorService>();
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Request = { Path = "/api/TarifCotisation/Affilie" }
        };

        var controller = new TarifCotisationController(
            repo,
            new TarifCotisationMetierService(db),
            db,
            paginationService,
            paginationOptions,
            NullLogger<TarifCotisationController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        return controller;
    }

    [Fact]
    public async Task GetByAffilie_ErreurTechnique_RetourneErrorResponseStructure()
    {
        var repo = new Mock<ITarifCotisationRepository>();
        repo.Setup(r => r.GetByAffilieIdAsync(42, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("connexion base indisponible"));

        var controller = CreateController(repo.Object);

        var result = await controller.GetByAffilie(42);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);

        var errorResponse = Assert.IsType<ErrorResponse>(objectResult.Value);
        Assert.Equal(ErrorCodes.TECHNICAL_INTERNAL_ERROR, errorResponse.Error.Code);
        Assert.False(string.IsNullOrEmpty(errorResponse.CorrelationId));
        Assert.Single(errorResponse.Error.Details);
        Assert.Equal("connexion base indisponible", errorResponse.Error.Details[0].Issue);
    }

    [Fact]
    public async Task GetByAffilie_AffilieInexistant_Retourne404SansChangerLeContrat()
    {
        var repo = new Mock<ITarifCotisationRepository>();
        repo.Setup(r => r.GetByAffilieIdAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Affilié avec ID 999 introuvable."));

        var controller = CreateController(repo.Object);

        var result = await controller.GetByAffilie(999);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Affilié avec ID 999 introuvable.", notFound.Value);
    }

    [Fact]
    public async Task Create_MappeLibelleTarifCotisationSurEntite()
    {
        var repo = new Mock<ITarifCotisationRepository>();
        TarifCotisation? captured = null;
        repo.Setup(r => r.CreateAsync(It.IsAny<TarifCotisation>(), It.IsAny<CancellationToken>()))
            .Callback<TarifCotisation, CancellationToken>((e, _) =>
            {
                e.IdCotisationAffilie = 123;
                captured = e;
            })
            .ReturnsAsync((TarifCotisation e, CancellationToken _) => e);

        repo.Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TarifCotisation
            {
                IdCotisationAffilie = 123,
                Montant = 100m,
                Periodicite = "Mensuel",
                TypeAdhesionId = 1,
                DeviseId = 1,
                LibelleTarifCotisation = "Libelle test",
                Statut = true
            });

        var controller = CreateController(repo.Object);
        var dto = new TarifCotisationCreateDto
        {
            Montant = 100m,
            Periodicite = "Mensuel",
            TypeAdhesionId = 1,
            DeviseId = 1,
            LibelleTarifCotisation = "Libelle test",
            Statut = true
        };

        var result = await controller.Create(dto);
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var body = Assert.IsType<TarifCotisationReadDto>(created.Value);

        Assert.NotNull(captured);
        Assert.Equal("Libelle test", captured!.LibelleTarifCotisation);
        Assert.Equal("Libelle test", body.LibelleTarifCotisation);
    }
}
