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

public class AffilieControllerTests
{
    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Prosoc.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    private static AffilieController CreateController(IPaginationService? paginationService = null)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new ProsocDbContext(options);
        db.Database.EnsureCreated();

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
            Request = { Path = "/api/Affilie" }
        };

        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }, "Test");
        var controller = new AffilieController(
            Mock.Of<IAffilieRepository>(),
            Mock.Of<IAdhesionRepository>(),
            db,
            Mock.Of<IPaiementAffilieService>(),
            Mock.Of<IFlexPayPaiementAffilieService>(),
            paginationService ?? new Mock<IPaginationService>().Object,
            Options.Create(new PaginationOptions()),
            NullLogger<AffilieController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

        return controller;
    }

    [Fact]
    public async Task GetAffilies_ErreurTechnique_RetourneErrorResponseStructure()
    {
        var pagination = new Mock<IPaginationService>();
        pagination
            .Setup(p => p.CreatePaginatedResponseAsync(
                It.IsAny<IQueryable<Affilie>>(),
                It.IsAny<PaginationRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("échec pagination affiliés"));

        var controller = CreateController(pagination.Object);

        var result = await controller.GetAffilies(new PaginationRequest());

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);

        var errorResponse = Assert.IsType<ErrorResponse>(objectResult.Value);
        Assert.Equal(ErrorCodes.TECHNICAL_INTERNAL_ERROR, errorResponse.Error.Code);
        Assert.False(string.IsNullOrEmpty(errorResponse.CorrelationId));
        Assert.Single(errorResponse.Error.Details);
        Assert.Equal("échec pagination affiliés", errorResponse.Error.Details[0].Issue);
    }

    private static AffilieController CreateController(IAffilieRepository repo, IPaginationService? paginationService = null)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var db = new ProsocDbContext(new DbContextOptionsBuilder<ProsocDbContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();

        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment());
        services.Configure<ErrorHandlingOptions>(o => o.ExposeExceptionDetails = true);
        services.AddHttpContextAccessor();
        services.AddLogging();
        services.AddScoped<ErrorService>();
        var sp = services.BuildServiceProvider();

        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }, "Test");
        var controller = new AffilieController(
            repo,
            Mock.Of<IAdhesionRepository>(),
            db,
            Mock.Of<IPaiementAffilieService>(),
            Mock.Of<IFlexPayPaiementAffilieService>(),
            paginationService ?? Mock.Of<IPaginationService>(),
            Options.Create(new PaginationOptions()),
            NullLogger<AffilieController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = sp }
            }
        };
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);
        return controller;
    }

    [Fact]
    public async Task GetAffilie_Inexistant_Retourne404SansChangerLeContrat()
    {
        var repo = new Mock<IAffilieRepository>();
        repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Affilie?)null);

        var controller = CreateController(repo.Object);

        var result = await controller.GetAffilie(999);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Affilié non trouvé", notFound.Value);
    }
}
