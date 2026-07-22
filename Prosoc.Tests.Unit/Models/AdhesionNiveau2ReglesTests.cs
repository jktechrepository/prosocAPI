using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace Prosoc.Tests.Unit.Models;

public class AdhesionNiveau2ReglesTests
{
    private static AdhesionNiveau2EncodeurDto InputMinimal(bool valider = true) => new()
    {
        Valider = valider,
        PersonneContact = new PersonneContactNiveau2Dto
        {
            NomComplet = "Contact Test",
            LienParente = "AMI",
            Adresse = "Kinshasa"
        },
        CommuneActivite = "Gombe",
        QuartierActivite = "Centre",
        PhotoBase64 = "cGhvdG8=",
        PhotoContentType = "image/jpeg",
        CarteIdentiteBase64 = "Y2FydGU=",
        CarteIdentiteContentType = "image/jpeg"
    };

    private static Affilie AffilieIdentiteComplete() => new()
    {
        Nom = "Kabila",
        Prenom = "Jean",
        DateNaissance = new DateTime(1990, 5, 1),
        NomComplet = "Kabila Jean"
    };

    [Fact]
    public void ValiderPiecesIdentitePourValidation_ValiderSansPieces_Rejette()
    {
        var affilie = new Affilie();
        var input = new AdhesionNiveau2EncodeurDto
        {
            Valider = true,
            PersonneContact = InputMinimal().PersonneContact
        };

        var errors = AdhesionNiveau2Regles.ValiderPiecesIdentitePourValidation(affilie, input);

        Assert.Contains(errors, e => e.Contains("photo", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, e => e.Contains("carte", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValiderPiecesIdentitePourValidation_SansValiderSansPieces_Accepte()
    {
        var affilie = new Affilie();
        var input = InputMinimal(valider: false);
        input.PhotoBase64 = null;
        input.CarteIdentiteBase64 = null;

        var errors = AdhesionNiveau2Regles.ValiderPiecesIdentitePourValidation(affilie, input);

        Assert.Empty(errors);
    }

    [Fact]
    public void ValiderPiecesIdentitePourValidation_PiecesDansDto_Accepte()
    {
        var affilie = new Affilie();
        var input = InputMinimal();

        var errors = AdhesionNiveau2Regles.ValiderPiecesIdentitePourValidation(affilie, input);

        Assert.Empty(errors);
    }

    [Fact]
    public void Valider_SansContactNiEnBase_Rejette()
    {
        var input = new AdhesionNiveau2EncodeurDto
        {
            PersonneContact = new PersonneContactNiveau2Dto()
        };

        var errors = AdhesionNiveau2Regles.Valider(input, new DateTime(1990, 1, 1), contactExistantEnBase: false);

        Assert.Contains(errors, e => e.Contains("personne de contact", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Valider_ContactExistantEnBaseSansBody_Accepte()
    {
        var input = new AdhesionNiveau2EncodeurDto
        {
            PersonneContact = new PersonneContactNiveau2Dto()
        };

        var errors = AdhesionNiveau2Regles.Valider(input, new DateTime(1990, 1, 1), contactExistantEnBase: true);

        Assert.DoesNotContain(errors, e => e.Contains("personne de contact", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Valider_ContactPartiel_Rejette()
    {
        var input = new AdhesionNiveau2EncodeurDto
        {
            PersonneContact = new PersonneContactNiveau2Dto
            {
                NomComplet = "Marie Kabila"
            }
        };

        var errors = AdhesionNiveau2Regles.Valider(input, new DateTime(1990, 1, 1));

        Assert.Contains(errors, e => e.Contains("lien de parenté", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, e => e.Contains("adresse", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValiderPiecesIdentitePourValidation_PiecesDejaEnBase_Accepte()
    {
        var affilie = new Affilie
        {
            PhotoData = new byte[] { 1 },
            PhotoContentType = "image/jpeg",
            CarteIdentiteData = new byte[] { 2 },
            CarteIdentiteContentType = "image/jpeg"
        };
        var input = InputMinimal();
        input.PhotoBase64 = null;
        input.CarteIdentiteBase64 = null;

        var errors = AdhesionNiveau2Regles.ValiderPiecesIdentitePourValidation(affilie, input);

        Assert.Empty(errors);
    }

    [Fact]
    public void ValiderDossierComplet_SansAdresseActivite_Rejette()
    {
        var affilie = AffilieIdentiteComplete();
        var input = InputMinimal();
        input.CommuneActivite = null;
        input.QuartierActivite = null;

        AdhesionNiveau2Regles.AppliquerIdentiteActivite(affilie, input);
        var errors = AdhesionNiveau2Regles.ValiderDossierCompletPourValidation(affilie, input, contactExistantEnBase: true);

        Assert.Contains(errors, e => e.Contains("commune d'activité", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, e => e.Contains("quartier d'activité", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValiderDossierComplet_SansIdentite_Rejette()
    {
        var affilie = new Affilie { DateNaissance = default };
        var input = InputMinimal();

        AdhesionNiveau2Regles.AppliquerIdentiteActivite(affilie, input);
        var errors = AdhesionNiveau2Regles.ValiderDossierCompletPourValidation(affilie, input, contactExistantEnBase: true);

        Assert.Contains(errors, e => e.Contains("nom est obligatoire", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, e => e.Contains("prénom est obligatoire", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, e => e.Contains("date de naissance", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValiderDossierComplet_QuatreBlocsOk_Accepte()
    {
        var affilie = AffilieIdentiteComplete();
        var input = InputMinimal();

        AdhesionNiveau2Regles.AppliquerIdentiteActivite(affilie, input);
        var errors = AdhesionNiveau2Regles.ValiderDossierCompletPourValidation(affilie, input, contactExistantEnBase: true);

        Assert.Empty(errors);
        Assert.True(AdhesionNiveau2Regles.EstDossierComplet(affilie, input, contactExistantEnBase: true));
    }

    [Fact]
    public void ValiderDossierComplet_SansValider_IgnoreControles()
    {
        var affilie = new Affilie();
        var input = InputMinimal(valider: false);
        input.CommuneActivite = null;

        var errors = AdhesionNiveau2Regles.ValiderDossierCompletPourValidation(affilie, input);

        Assert.Empty(errors);
    }

    [Fact]
    public void AppliquerIdentiteActivite_CompleteAdresseEtNomComplet()
    {
        var affilie = AffilieIdentiteComplete();
        var input = InputMinimal();
        input.Nom = "Mukebo";
        input.Prenom = "Paul";
        input.Postnom = "X";
        input.CommuneActivite = "Limete";
        input.QuartierActivite = "Kingabwa";

        AdhesionNiveau2Regles.AppliquerIdentiteActivite(affilie, input);

        Assert.Equal("Mukebo", affilie.Nom);
        Assert.Equal("Paul", affilie.Prenom);
        Assert.Equal("X", affilie.Postnom);
        Assert.Equal("Mukebo X Paul", affilie.NomComplet);
        Assert.Equal("Limete", affilie.CommuneActivite);
        Assert.Equal("Kingabwa", affilie.QuartierActivite);
    }
}
