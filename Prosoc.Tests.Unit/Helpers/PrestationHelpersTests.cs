using ProsocAPI.Models.Core;
using Prosoc.Utilities;
using Xunit;

namespace Prosoc.Tests.Unit.Helpers;

public class PrestationHelpersTests
{
    [Fact]
    public void EstGratuite_ReturnsTrue_WhenProduitMutuelEstGratuit()
    {
        var prestation = new Prestation
        {
            ProduitMutuel = new ProduitMutuel { EstGratuit = true }
        };

        Assert.True(PrestationHelpers.EstGratuite(prestation));
    }

    [Fact]
    public void EstGratuite_ReturnsTrue_WhenProduitAssureurEstGratuit()
    {
        var prestation = new Prestation
        {
            ProduitAssureur = new ProduitAssureur { EstGratuit = true }
        };

        Assert.True(PrestationHelpers.EstGratuite(prestation));
    }

    [Fact]
    public void EstGratuite_ReturnsFalse_WhenAucunProduitGratuit()
    {
        var prestation = new Prestation
        {
            ProduitMutuel = new ProduitMutuel { EstGratuit = false },
            ProduitAssureur = new ProduitAssureur { EstGratuit = false }
        };

        Assert.False(PrestationHelpers.EstGratuite(prestation));
    }

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

    [Fact]
    public void ToReadDto_ExposeEstGratuit_FromProduitLie()
    {
        var prestation = new Prestation
        {
            IdPrestation = 2,
            NomPrestation = "MAASH",
            Periodicite = "Mensuel",
            Montant = 0m,
            ProduitMutuel = new ProduitMutuel { Nom = "MAASH", EstGratuit = true }
        };

        var dto = PrestationHelpers.ToReadDto(prestation);

        Assert.True(dto.EstGratuit);
    }
}
