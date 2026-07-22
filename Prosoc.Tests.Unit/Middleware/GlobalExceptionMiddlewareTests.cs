using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using ProsocAPI.Middleware;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Middleware;

public class GlobalExceptionMiddlewareTests
{
    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "Prosoc.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    [Fact]
    public async Task InvokeAsync_ExceptionNonGeree_Renvoie500StructureSansDetailsEnProduction()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/crash-test";

        var hostEnvironment = new TestHostEnvironment { EnvironmentName = Environments.Production };

        var errorService = new ErrorService(
            new HttpContextAccessor { HttpContext = context },
            NullLogger<ErrorService>.Instance,
            Options.Create(new ErrorHandlingOptions { ExposeExceptionDetails = false }),
            hostEnvironment);

        RequestDelegate next = _ => throw new InvalidOperationException("secret interne");

        var middleware = new GlobalExceptionMiddleware(next, NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context, errorService);

        Assert.Equal(500, context.Response.StatusCode);
        Assert.StartsWith("application/json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);
        var root = doc.RootElement;

        Assert.Equal("Une erreur technique est survenue", root.GetProperty("error").GetProperty("message").GetString());
        Assert.False(string.IsNullOrEmpty(root.GetProperty("correlationId").GetString()));
        Assert.Equal(0, root.GetProperty("error").GetProperty("details").GetArrayLength());
    }
}
