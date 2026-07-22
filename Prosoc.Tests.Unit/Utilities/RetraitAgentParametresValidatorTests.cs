using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Utilities;
using Xunit;

namespace Prosoc.Tests.Unit.Utilities;

public class RetraitAgentParametresValidatorTests
{
    [Fact]
    public void Validate_ValidConfig_ReturnsNull()
    {
        var dto = new RetraitAgentParametresUpdateDto
        {
            Fenetre1Debut = 15,
            Fenetre1Fin = 20,
            Fenetre2DerniersJours = 7,
            MontantMinimumPartiel = 5
        };

        Assert.Null(RetraitAgentParametresValidator.Validate(dto));
    }

    [Fact]
    public void Validate_OverlappingWindows_ReturnsError()
    {
        var dto = new RetraitAgentParametresUpdateDto
        {
            Fenetre1Debut = 15,
            Fenetre1Fin = 28,
            Fenetre2DerniersJours = 15,
            MontantMinimumPartiel = 5
        };

        Assert.NotNull(RetraitAgentParametresValidator.Validate(dto));
    }

    [Fact]
    public void Validate_NonPositiveMontant_ReturnsError()
    {
        var dto = new RetraitAgentParametresUpdateDto
        {
            Fenetre1Debut = 15,
            Fenetre1Fin = 20,
            Fenetre2DerniersJours = 7,
            MontantMinimumPartiel = 0
        };

        Assert.Contains("MontantMinimumPartiel", RetraitAgentParametresValidator.Validate(dto)!);
    }
}
