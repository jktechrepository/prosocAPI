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

public class AdhesionControllerTests
{
    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Prosoc.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    private static AdhesionController CreateController(IPaginationService? paginationService = null)
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
            Request = { Path = "/api/Adhesion/paginated" }
        };

        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }, "Test");
        var controller = new AdhesionController(
            Mock.Of<IAdhesionRepository>(),
            Mock.Of<IAffilieRepository>(),
            db,
            Mock.Of<IEmailService>(),
            Mock.Of<INotificationService>(),
            Mock.Of<ICotisationAffilieMetierService>(),
            Mock.Of<IFlexPayAdhesionService>(),
            Mock.Of<ICollecteMultideviseService>(),
            Mock.Of<IWalletVirtuelPaymentService>(),
            Mock.Of<ITypeAdhesionDependantsValidationService>(),
            serviceProvider.GetRequiredService<ErrorService>(),
            paginationService ?? new Mock<IPaginationService>().Object,
            Options.Create(new PaginationOptions()),
            NullLogger<AdhesionController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

        return controller;
    }

    [Fact]
    public async Task GetPaginated_ErreurTechnique_RetourneErrorResponseStructure()
    {
        var pagination = new Mock<IPaginationService>();
        pagination
            .Setup(p => p.CreatePaginatedResponseAsync(
                It.IsAny<IQueryable<Adhesion>>(),
                It.IsAny<PaginationRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("échec pagination adhésions"));

        var controller = CreateController(pagination.Object);

        var result = await controller.GetPaginated(new PaginationRequest());

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);

        var errorResponse = Assert.IsType<ErrorResponse>(objectResult.Value);
        Assert.Equal(ErrorCodes.TECHNICAL_INTERNAL_ERROR, errorResponse.Error.Code);
        Assert.False(string.IsNullOrEmpty(errorResponse.CorrelationId));
        Assert.Single(errorResponse.Error.Details);
        Assert.Equal("échec pagination adhésions", errorResponse.Error.Details[0].Issue);
    }
}
