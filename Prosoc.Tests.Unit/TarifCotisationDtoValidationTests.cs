using System.ComponentModel.DataAnnotations;
using System.Globalization;
using ProsocAPI.Models.DTOs.Core;

namespace Prosoc.Tests.Unit;

public class TarifCotisationDtoValidationTests
{
    [Fact]
    public void CreateDto_MontantValide_sousCultureFrFR()
    {
        var previous = CultureInfo.CurrentCulture;
        var previousUi = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
            CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

            var dto = new TarifCotisationCreateDto
            {
                Montant = 10.5m,
                Periodicite = "Mensuel",
                TypeAdhesionId = 1,
                DeviseId = 2,
                Statut = true
            };

            var results = Validate(dto);

            Assert.Empty(results);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
            CultureInfo.CurrentUICulture = previousUi;
        }
    }

    private static List<ValidationResult> Validate(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void CreateDto_LibelleTarifCotisation_Null_EstValide()
    {
        var dto = new TarifCotisationCreateDto
        {
            Montant = 10.5m,
            Periodicite = "Mensuel",
            TypeAdhesionId = 1,
            DeviseId = 2,
            LibelleTarifCotisation = null,
            Statut = true
        };

        var results = Validate(dto);
        Assert.Empty(results);
    }

    [Fact]
    public void CreateDto_LibelleTarifCotisation_Depasse255_EstInvalide()
    {
        var dto = new TarifCotisationCreateDto
        {
            Montant = 10.5m,
            Periodicite = "Mensuel",
            TypeAdhesionId = 1,
            DeviseId = 2,
            LibelleTarifCotisation = new string('x', 256),
            Statut = true
        };

        var results = Validate(dto);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(TarifCotisationCreateDto.LibelleTarifCotisation)));
    }
}
