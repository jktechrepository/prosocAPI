using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProsocAPI.Controllers;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Controllers;

public class BaseApiControllerTests
{
    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Prosoc.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    private sealed class TestPaginationController : BaseApiController
    {
        public TestPaginationController(IPaginationService paginationService)
            : base(paginationService, Microsoft.Extensions.Options.Options.Create(new PaginationOptions()), LoggerFactory.Create(b => b.AddDebug()).CreateLogger<BaseApiController>())
        {
        }

        public Task<ActionResult<PaginatedResponse<string>>> RunPaginated()
        {
            return CreatePaginatedResponseAsync(new[] { "a", "b" }.AsQueryable());
        }
    }

    [Fact]
    public async Task CreatePaginatedResponseAsync_ErreurTechnique_RetourneErrorResponse()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment());
        services.Configure<ErrorHandlingOptions>(o => o.ExposeExceptionDetails = true);
        services.AddHttpContextAccessor();
        services.AddLogging();
        services.AddScoped<ErrorService>();
        var sp = services.BuildServiceProvider();

        var pagination = new Moq.Mock<IPaginationService>();
        pagination
            .Setup(p => p.CreatePaginatedResponseAsync(
                It.IsAny<IQueryable<string>>(),
                It.IsAny<PaginationRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("pagination base"));

        var controller = new TestPaginationController(pagination.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = sp,
                    Request = { Path = "/api/test" }
                }
            }
        };

        var result = await controller.RunPaginated();

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(objectResult.Value);
        Assert.Equal(ErrorCodes.TECHNICAL_INTERNAL_ERROR, errorResponse.Error.Code);
        Assert.False(string.IsNullOrEmpty(errorResponse.CorrelationId));
    }
}
