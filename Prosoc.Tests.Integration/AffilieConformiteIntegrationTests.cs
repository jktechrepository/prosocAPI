using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Prosoc.Tests.Integration;

public class AffilieConformiteIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AffilieConformiteIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        TestAuthHandler.Roles = new[] { "Admin" };
    }

    [Fact]
    public async Task GetByAffilie_AvecArriereCotisation_RetourneHorsOrdre()
    {
        int affilieId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProsocDbContext>();
            var deviseId = await db.Devises.Where(d => d.Statut).Select(d => d.IdDevise).FirstAsync();

            var affilie = new Affilie
            {
                CodeAdhesion = $"AFF-CF-{Guid.NewGuid():N}"[..12],
                Nom = "Conformite",
                Prenom = "Test",
                NomComplet = "Conformite Test",
                DateNaissance = new DateTime(1991, 3, 3),
                Statut = true
            };
            db.Affilies.Add(affilie);
            await db.SaveChangesAsync();
            affilieId = affilie.IdAffilie;

            db.ArrieresAffilie.Add(new ArrieresAffilie
            {
                AffilieId = affilieId,
                TypeObligation = TypeCollecte.Cotisation,
                Mois = 5,
                Annee = 2026,
                DateEcheance = DateTime.UtcNow.AddDays(-15),
                MontantAttendu = 80m,
                MontantPaye = 0m,
                RestAPayer = 80m,
                DeviseId = deviseId,
                StatutPaiement = ArrieresAffilieStatuts.EnRetard,
                Periodicite = "Mensuel",
                Description = "Cotisation mai 2026",
                Statut = true
            });
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/AffilieConformite/{affilieId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<AffilieConformiteDto>();
        Assert.NotNull(result);
        Assert.Equal(AffilieConformiteStatuts.HorsOrdre, result!.StatutCotisation);
        Assert.Equal(AffilieConformiteStatuts.HorsOrdre, result.StatutGlobal);
        Assert.Equal(AffilieConformiteStatuts.EnOrdre, result.StatutPrestation);
        Assert.True(result.NombreArrieresOuverts >= 1);
    }
}
