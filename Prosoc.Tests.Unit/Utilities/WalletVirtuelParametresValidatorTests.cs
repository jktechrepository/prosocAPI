using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Utilities;
using Xunit;

namespace Prosoc.Tests.Unit.Utilities;

public class WalletVirtuelParametresValidatorTests
{
    [Fact]
    public void Validate_PlafondPositif_ReturnsNull()
    {
        var error = WalletVirtuelParametresValidator.Validate(new WalletVirtuelParametresUpdateDto
        {
            PlafondSolde = 100m
        });
        Assert.Null(error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PlafondNonPositif_ReturnsError(decimal plafond)
    {
        var error = WalletVirtuelParametresValidator.Validate(new WalletVirtuelParametresUpdateDto
        {
            PlafondSolde = plafond
        });
        Assert.NotNull(error);
        Assert.Contains("PlafondSolde", error);
    }
}
