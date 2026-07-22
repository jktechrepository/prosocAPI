using ProsocAPI.Models.Core;
using Prosoc.Utilities;
using Xunit;

namespace Prosoc.Tests.Unit.Helpers;

public class PrestationHelpersTests
{
    [Fact]
    public void ToReadDto_ExposeDeviseCode()
    {
        var prestation = new Prestation
        {
            IdPrestation = 1,
            NomPrestation = "Consultation",
            Periodicite = "Annuel",
            Montant = 28500m,
            DeviseId = 2,
            Devise = new Devise { IdDevise = 2, Code = "CDF", Nom = "Franc congolais" }
        };

        var dto = PrestationHelpers.ToReadDto(prestation);

        Assert.Equal("CDF", dto.DeviseCode);
        Assert.Equal(2, dto.DeviseId);
        Assert.Equal("Annuel", dto.Periodicite);
    }
}
