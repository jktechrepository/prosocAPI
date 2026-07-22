using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace Prosoc.Tests.Unit.Models;

public class AdhesionNiveau1ReglesTests
{
    [Fact]
    public void ValiderChampsObligatoires_FlexPaySansStatutConfirme_Accepte()
    {
        var input = new AdhesionWithAffilieCreateDto
        {
            Nom = "KABONGO",
            Prenom = "Jean",
            PhotoBase64 = "data",
            PhotoContentType = "image/jpeg",
            CarteIdentiteBase64 = "data",
            CarteIdentiteContentType = "image/jpeg",
            ProvinceResidence = "Kinshasa",
            CommuneResidence = "Gombe",
            QuartierResidence = "Centre",
            Collectes = new List<CollecteAvecSouscriptionDto>
            {
                new()
                {
                    TypeCollecte = TypeCollecte.Souscription,
                    Montant = 100m,
                    DeviseId = 1,
                    ModePaiement = MethodePaiementHelper.MobileMoney,
                    StatutPaiement = "EN_ATTENTE",
                    Souscription = new SouscriptionPrestationCreateDto { PrestationId = 1 }
                }
            }
        };

        var errors = AdhesionNiveau1Regles.ValiderChampsObligatoires(input);
        Assert.DoesNotContain(errors, e => e.Contains("statutPaiement confirmé"));
    }

    [Fact]
    public void ValiderChampsObligatoires_SansPhotoNiCarte_Accepte()
    {
        var input = new AdhesionWithAffilieCreateDto
        {
            Nom = "KABONGO",
            Prenom = "Jean",
            ProvinceResidence = "Kinshasa",
            CommuneResidence = "Gombe",
            QuartierResidence = "Centre",
            Collectes = new List<CollecteAvecSouscriptionDto>
            {
                new()
                {
                    TypeCollecte = TypeCollecte.Souscription,
                    Montant = 100m,
                    DeviseId = 1,
                    StatutPaiement = "PAYE",
                    Souscription = new SouscriptionPrestationCreateDto { PrestationId = 1 }
                }
            }
        };

        var errors = AdhesionNiveau1Regles.ValiderChampsObligatoires(input);

        Assert.DoesNotContain(errors, e => e.Contains("photo", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(errors, e => e.Contains("carte", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValiderChampsObligatoires_PhotoSansContentType_Rejette()
    {
        var input = new AdhesionWithAffilieCreateDto
        {
            Nom = "KABONGO",
            Prenom = "Jean",
            PhotoBase64 = "cGhvdG8=",
            ProvinceResidence = "Kinshasa",
            CommuneResidence = "Gombe",
            QuartierResidence = "Centre",
            Collectes = new List<CollecteAvecSouscriptionDto>
            {
                new()
                {
                    TypeCollecte = TypeCollecte.Souscription,
                    Montant = 100m,
                    DeviseId = 1,
                    StatutPaiement = "PAYE",
                    Souscription = new SouscriptionPrestationCreateDto { PrestationId = 1 }
                }
            }
        };

        var errors = AdhesionNiveau1Regles.ValiderChampsObligatoires(input);

        Assert.Contains(errors, e => e.Contains("photoContentType"));
    }

    [Fact]
    public void ValiderChampsObligatoires_SansPersonneContact_Accepte()
    {
        var input = InputMinimalAvecCollecte();

        var errors = AdhesionNiveau1Regles.ValiderChampsObligatoires(input);

        Assert.DoesNotContain(errors, e => e.Contains("personne de contact", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValiderChampsObligatoires_PersonneContactValide_Accepte()
    {
        var input = InputMinimalAvecCollecte();
        input.PersonneContact = new PersonneContactCreateDto
        {
            NomComplet = "Marie Kabila",
            LienParente = "EPOUSE",
            Adresse = "Kinshasa, Gombe"
        };

        var errors = AdhesionNiveau1Regles.ValiderChampsObligatoires(input);

        Assert.DoesNotContain(errors, e => e.Contains("personne de contact", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValiderChampsObligatoires_PersonneContactPartiel_Rejette()
    {
        var input = InputMinimalAvecCollecte();
        input.PersonneContact = new PersonneContactCreateDto
        {
            NomComplet = "Marie Kabila"
        };

        var errors = AdhesionNiveau1Regles.ValiderChampsObligatoires(input);

        Assert.Contains(errors, e => e.Contains("lien de parenté", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValiderChampsObligatoires_SansCommuneNiQuartier_AccepteSiProvinceEtCollectes()
    {
        var input = new AdhesionWithAffilieCreateDto
        {
            Nom = "KABONGO",
            Prenom = "Jean",
            ProvinceResidence = "Kinshasa",
            Collectes = new List<CollecteAvecSouscriptionDto>
            {
                new()
                {
                    TypeCollecte = TypeCollecte.Souscription,
                    Montant = 100m,
                    DeviseId = 1,
                    StatutPaiement = "PAYE",
                    Souscription = new SouscriptionPrestationCreateDto { PrestationId = 1 }
                }
            }
        };

        var errors = AdhesionNiveau1Regles.ValiderChampsObligatoires(input);

        Assert.Empty(errors);
    }

    [Fact]
    public void ValiderChampsObligatoires_SansProvince_Rejette()
    {
        var input = InputMinimalAvecCollecte();
        input.ProvinceResidence = "";

        var errors = AdhesionNiveau1Regles.ValiderChampsObligatoires(input);

        Assert.Contains(errors, e => e.Contains("province de résidence", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValiderChampsObligatoires_FraisSeuls_Accepte()
    {
        var input = new AdhesionWithAffilieCreateDto
        {
            Nom = "KABONGO",
            Prenom = "Jean",
            ProvinceResidence = "Kinshasa",
            Collectes = new List<CollecteAvecSouscriptionDto>
            {
                new()
                {
                    TypeCollecte = TypeCollecte.Frais,
                    FraisId = 1,
                    Montant = 1.5m,
                    DeviseId = 1,
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    ModePaiement = "ESPECE"
                }
            }
        };

        var errors = AdhesionNiveau1Regles.ValiderChampsObligatoires(input);

        Assert.Empty(errors);
    }

    [Fact]
    public void ValiderChampsObligatoires_SansCollecte_Rejette()
    {
        var input = InputMinimalAvecCollecte();
        input.Collectes = new List<CollecteAvecSouscriptionDto>();

        var errors = AdhesionNiveau1Regles.ValiderChampsObligatoires(input);

        Assert.Contains(errors, e => e.Contains("Au moins une collecte est requise"));
        Assert.DoesNotContain(errors, e => e.Contains("Souscription ou Cotisation"));
    }

    [Fact]
    public void ValiderChampsObligatoires_SouscriptionSansPrestationId_Rejette()
    {
        var input = InputMinimalAvecCollecte();
        input.Collectes = new List<CollecteAvecSouscriptionDto>
        {
            new()
            {
                TypeCollecte = TypeCollecte.Souscription,
                Montant = 100m,
                DeviseId = 1,
                StatutPaiement = "PAYE",
                Souscription = null
            }
        };

        var errors = AdhesionNiveau1Regles.ValiderChampsObligatoires(input);

        Assert.Contains(errors, e => e.Contains("souscription.prestationId"));
    }

    private static AdhesionWithAffilieCreateDto InputMinimalAvecCollecte() => new()
    {
        Nom = "KABONGO",
        Prenom = "Jean",
        ProvinceResidence = "Kinshasa",
        Collectes = new List<CollecteAvecSouscriptionDto>
        {
            new()
            {
                TypeCollecte = TypeCollecte.Souscription,
                Montant = 100m,
                DeviseId = 1,
                StatutPaiement = "PAYE",
                Souscription = new SouscriptionPrestationCreateDto { PrestationId = 1 }
            }
        }
    };
}
