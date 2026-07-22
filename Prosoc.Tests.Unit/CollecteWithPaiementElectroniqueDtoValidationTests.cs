using System.ComponentModel.DataAnnotations;
using System.Globalization;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace Prosoc.Tests.Unit;

public class CollecteWithPaiementElectroniqueDtoValidationTests
{
    [Fact]
    public void CreateDto_Valide_sousCultureFrFR()
    {
        var previous = CultureInfo.CurrentCulture;
        var previousUi = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
            CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

            var dto = new CollecteWithPaiementElectroniqueCreateDto
            {
                ModePaiement = "MOBILE_MONEY",
                TelephonePaiement = "0822222222",
                DevisePaiementId = 1,
                Collecte = new CollecteCreateDto
                {
                    TypeCollecte = TypeCollecte.Frais,
                    FraisId = 1,
                    AffilieId = 1,
                    AgentId = 1,
                    Montant = 10.5m,
                    DeviseId = 1,
                    ModePaiement = "MOBILE_MONEY",
                    Statut = true
                }
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
}
