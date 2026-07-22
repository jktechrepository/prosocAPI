using System.Security.Claims;
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
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Unit.Controllers;

public class CollecteControllerTests
{
    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Prosoc.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    private static CollecteController CreateController(
        IPaginationService? paginationService = null,
        string requestPath = "/api/Collecte")
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new ProsocDbContext(options);
        db.Database.EnsureCreated();

        var pagination = paginationService ?? new Mock<IPaginationService>().Object;
        var collecteRepo = new Mock<ICollecteRepository>().Object;
        var flexPay = new Mock<IFlexPayCollecteService>().Object;

        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment { EnvironmentName = Environments.Development });
        services.Configure<ErrorHandlingOptions>(o => o.ExposeExceptionDetails = true);
        services.AddHttpContextAccessor();
        services.AddLogging();
        services.AddScoped<ErrorService>();
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Request = { Path = requestPath }
        };

        var controller = new CollecteController(
            collecteRepo,
            flexPay,
            db,
            pagination,
            Options.Create(new PaginationOptions()),
            NullLogger<CollecteController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        return controller;
    }

    [Fact]
    public async Task GetAll_ErreurTechnique_RetourneErrorResponseStructure()
    {
        var pagination = new Mock<IPaginationService>();
        pagination
            .Setup(p => p.CreatePaginatedResponseAsync(
                It.IsAny<IQueryable<Collecte>>(),
                It.IsAny<PaginationRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("échec pagination collectes"));

        var controller = CreateController(pagination.Object);

        var result = await controller.GetAll(new PaginationRequest());

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);

        var errorResponse = Assert.IsType<ErrorResponse>(objectResult.Value);
        Assert.Equal(ErrorCodes.TECHNICAL_INTERNAL_ERROR, errorResponse.Error.Code);
        Assert.False(string.IsNullOrEmpty(errorResponse.CorrelationId));
        Assert.Single(errorResponse.Error.Details);
        Assert.Equal("échec pagination collectes", errorResponse.Error.Details[0].Issue);
    }

    [Fact]
    public async Task GetByType_SansCollecte_RetourneListeVide()
    {
        var controller = CreateController(requestPath: "/api/Collecte/by-type/Frais");

        var result = await controller.GetByType(TypeCollecte.Frais);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<List<CollecteReadDto>>(ok.Value);
        Assert.Empty(list);
    }

    [Fact]
    public async Task Update_SansPermissionUpdateCollecte_Retourne403()
    {
        var controller = CreateController();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("permission", "READ_COLLECTE"),
            new Claim("permission", "CREATE_COLLECTE")
        }, "Test");
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

        var dto = new CollecteUpdateDto
        {
            TypeCollecte = TypeCollecte.Cotisation,
            AffilieId = 1,
            AgentId = 1,
            Montant = 10m,
            ModePaiement = "ESPECE",
            StatutPaiement = "OK"
        };

        var result = await controller.Update(1, dto);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, objectResult.StatusCode);
    }
}
