using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class ErrorServiceTests
{
    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "Prosoc.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    private static ErrorService CreateService(
        bool exposeDetailsOption,
        string environmentName)
    {
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext
        {
            Request = { Path = "/api/test" }
        });

        var hostEnvironment = new TestHostEnvironment { EnvironmentName = environmentName };

        var options = Options.Create(new ErrorHandlingOptions
        {
            ExposeExceptionDetails = exposeDetailsOption
        });

        return new ErrorService(
            httpContextAccessor.Object,
            NullLogger<ErrorService>.Instance,
            options,
            hostEnvironment);
    }

    [Fact]
    public void CreateTechnicalError_Production_MasqueExceptionMessage()
    {
        var service = CreateService(exposeDetailsOption: false, environmentName: Environments.Production);
        var ex = new InvalidOperationException("détail sensible base de données");

        var response = service.CreateTechnicalError(
            ErrorCodes.TECHNICAL_INTERNAL_ERROR,
            "Erreur technique",
            ex);

        Assert.Equal("Erreur technique", response.Error.Message);
        Assert.False(string.IsNullOrEmpty(response.CorrelationId));
        Assert.Empty(response.Error.Details);
        Assert.False(service.ShouldExposeExceptionDetails());
    }

    [Fact]
    public void CreateTechnicalError_Development_ExposeExceptionMessage()
    {
        var service = CreateService(exposeDetailsOption: false, environmentName: Environments.Development);
        var ex = new InvalidOperationException("détail pour le développeur");

        var response = service.CreateTechnicalError(
            ErrorCodes.TECHNICAL_INTERNAL_ERROR,
            "Erreur technique",
            ex);

        Assert.Single(response.Error.Details);
        Assert.Equal("Exception", response.Error.Details[0].Field);
        Assert.Equal("détail pour le développeur", response.Error.Details[0].Issue);
        Assert.True(service.ShouldExposeExceptionDetails());
    }

    [Fact]
    public void CreateTechnicalError_OptionExplicitTrue_ExposeMemeEnProduction()
    {
        var service = CreateService(exposeDetailsOption: true, environmentName: Environments.Production);
        var ex = new InvalidOperationException("staging debug");

        var response = service.CreateTechnicalError(
            ErrorCodes.TECHNICAL_INTERNAL_ERROR,
            "Erreur technique",
            ex);

        Assert.Single(response.Error.Details);
        Assert.Equal("staging debug", response.Error.Details[0].Issue);
    }

    [Fact]
    public void CreateTechnicalError_ToujoursCorrelationIdEtPath()
    {
        var service = CreateService(exposeDetailsOption: false, environmentName: Environments.Production);

        var response = service.CreateTechnicalError(
            ErrorCodes.TECHNICAL_DATABASE_ERROR,
            "Erreur base",
            new Exception("hidden"));

        Assert.False(string.IsNullOrEmpty(response.CorrelationId));
        Assert.Equal("/api/test", response.Path);
        Assert.Equal(ErrorType.Technical, response.Error.Type);
    }
}
