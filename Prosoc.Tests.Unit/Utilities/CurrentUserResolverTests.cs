using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Prosoc.Utilities;

namespace Prosoc.Tests.Unit.Utilities;

public class CurrentUserResolverTests
{
    [Fact]
    public void TryGetCurrentUtilisateurId_ResolveDepuisClaimSub()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "42")
        }));

        Assert.Equal(42, CurrentUserResolver.TryGetCurrentUtilisateurId(principal));
    }

    [Fact]
    public void TryGetCurrentUtilisateurId_ResolveDepuisClaimUserId()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("UserId", "7")
        }));

        Assert.Equal(7, CurrentUserResolver.TryGetCurrentUtilisateurId(principal));
    }

    [Fact]
    public void GetCurrentUtilisateurId_LeveSiAbsent()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        Assert.Throws<UnauthorizedAccessException>(() =>
            CurrentUserResolver.GetCurrentUtilisateurId(principal));
    }
}
