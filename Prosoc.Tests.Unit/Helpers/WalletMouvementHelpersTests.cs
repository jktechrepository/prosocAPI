using ProsocAPI.Models.Core;
using Prosoc.Utilities;
using Xunit;

namespace Prosoc.Tests.Unit.Helpers;

public class WalletMouvementHelpersTests
{
    [Fact]
    public void ToReadDto_ExposeInfosDevise()
    {
        var devise = new Devise { IdDevise = 2, Code = "USD", Nom = "Dollar", Symbole = "$" };
        var mouvement = new WalletMouvement
        {
            IdWalletMouvement = 45,
            WalletId = 1,
            DeviseId = devise.IdDevise,
            Devise = devise,
            Montant = 2500m,
            TypeOperation = "CREDIT",
            Source = "COMMISSION",
            DateOperation = DateTime.UtcNow
        };

        var dto = WalletMouvementHelpers.ToReadDto(mouvement);

        Assert.Equal(2, dto.DeviseId);
        Assert.Equal("USD", dto.DeviseCode);
        Assert.Equal("Dollar", dto.DeviseNom);
        Assert.Equal("$", dto.DeviseSymbole);
    }

    [Fact]
    public void ToReadDto_EnrichitDescriptionCommissionCollecte()
    {
        var collecte = new Collecte
        {
            IdCollecte = 5,
            AffilieId = 78,
            Affilie = new Affilie { NomComplet = "Marie Kabila" }
        };
        var mouvement = new WalletMouvement
        {
            WalletId = 1,
            DeviseId = 1,
            Montant = 25m,
            TypeOperation = "CREDIT",
            Source = WalletMouvementSources.CommissionCollecte,
            Description = "Commission collecte #5 - Affilie 78",
            DateOperation = DateTime.UtcNow,
            Statut = true
        };

        var dto = WalletMouvementHelpers.ToReadDto(mouvement, collecte);

        Assert.Equal("Commission collecte — Marie Kabila (n° 5)", dto.Description);
        Assert.DoesNotContain("Affilie", dto.Description, StringComparison.OrdinalIgnoreCase);
    }
}
