using System.Net.Http.Json;
using ProsocAPI.Models.DTOs.Core;
using Xunit;

namespace Prosoc.Tests.Integration;

public class ReferentielLiensParenteIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ReferentielLiensParenteIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetLiensParente_Retourne23CodesEtGroupesMetier()
    {
        var referentiel = await _client.GetFromJsonAsync<LienParenteReferentielDto>(
            "/api/referentiel/liens-parente");

        Assert.NotNull(referentiel);
        Assert.Equal(23, referentiel!.Liens.Count);
        Assert.Equal(3, referentiel.LiensEnfant.Length);
        Assert.Equal(6, referentiel.LiensConjoint.Length);
        Assert.Contains(referentiel.Liens, l => l.Code == "CONJOINT" && l.Categorie == "CONJOINT");
    }
}
