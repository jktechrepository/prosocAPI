using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using Xunit;

namespace Prosoc.Tests.Integration;

public class PrestationGratuitesIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PrestationGratuitesIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetGratuites_ReturnsOnlyPrestationsLieesAProduitGratuitActif()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        var nomGratuit = $"Prestation gratuite {unique}";
        var nomPayant = $"Prestation payante {unique}";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var deviseId = db.Devises.Select(d => d.IdDevise).First();

            var produitGratuit = new ProduitMutuel
            {
                Nom = $"Produit gratuit {unique}",
                Montant = 0,
                Periodicite = "Mensuel",
                EstGratuit = true,
                Statut = true,
                DeviseId = deviseId
            };
            var produitPayant = new ProduitMutuel
            {
                Nom = $"Produit payant {unique}",
                Montant = 50,
                Periodicite = "Mensuel",
                EstGratuit = false,
                Statut = true,
                DeviseId = deviseId
            };
            db.ProduitsMutuels.AddRange(produitGratuit, produitPayant);
            await db.SaveChangesAsync();

            db.Prestations.AddRange(
                new Prestation
                {
                    NomPrestation = nomGratuit,
                    Periodicite = "Mensuel",
                    Montant = 0,
                    DeviseId = deviseId,
                    Statut = true,
                    ProduitMutuelId = produitGratuit.IdProduit
                },
                new Prestation
                {
                    NomPrestation = nomPayant,
                    Periodicite = "Mensuel",
                    Montant = 50,
                    DeviseId = deviseId,
                    Statut = true,
                    ProduitMutuelId = produitPayant.IdProduit
                });
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/Prestation/gratuites?pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<PrestationReadDto>>();
        Assert.NotNull(result);

        var noms = result.Data.Select(p => p.NomPrestation).ToList();
        Assert.Contains(nomGratuit, noms);
        Assert.DoesNotContain(nomPayant, noms);
        Assert.All(result.Data.Where(p => p.NomPrestation == nomGratuit), p => Assert.True(p.EstGratuit));
    }
}
