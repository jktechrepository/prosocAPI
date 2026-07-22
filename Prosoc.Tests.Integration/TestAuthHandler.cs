using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Prosoc.Tests.Integration;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    /// <summary>Id utilisateur seedé — aligné sur la base de test après SeedData.</summary>
    public static string UserId { get; set; } = "1";

    /// <summary>Rôles JWT simulés pour les tests d'autorisation.</summary>
    public static IReadOnlyList<string> Roles { get; set; } = new[] { "Admin", "Super-Admin" };

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new("uid", UserId),
            new("UserId", UserId),
            new(ClaimTypes.NameIdentifier, UserId),
            new(ClaimTypes.Name, "test-user"),
        };

        foreach (var role in Roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
