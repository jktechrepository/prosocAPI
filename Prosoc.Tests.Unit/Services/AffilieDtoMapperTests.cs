using Prosoc.Utilities;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class AffilieDtoMapperTests
{
    [Fact]
    public void ToReadDto_ExposePhotoBase64PhotoUrlEtCarteIdentiteBase64()
    {
        var photo = new byte[] { 1, 2, 3 };
        var carte = new byte[] { 9, 8, 7 };
        var affilie = new Affilie
        {
            IdAffilie = 1,
            CodeAdhesion = "CODE-1",
            Nom = "Test",
            Prenom = "Aff",
            NomComplet = "Aff Test",
            DateNaissance = new DateTime(1990, 1, 1),
            PhotoData = photo,
            PhotoContentType = "image/png",
            CarteIdentiteData = carte,
            CarteIdentiteContentType = "image/jpeg",
            Statut = true
        };

        var dto = AffilieDtoMapper.ToReadDto(affilie);

        var expectedPhoto = Convert.ToBase64String(photo);
        Assert.True(dto.HasPhoto);
        Assert.True(dto.HasCarteIdentite);
        Assert.Equal(expectedPhoto, dto.PhotoBase64);
        Assert.Equal(expectedPhoto, dto.PhotoUrl);
        Assert.Equal(Convert.ToBase64String(carte), dto.CarteIdentiteBase64);
        Assert.Equal("image/png", dto.PhotoContentType);
        Assert.Equal("image/jpeg", dto.CarteIdentiteContentType);
    }

    [Fact]
    public void ToReadDto_SansFichiers_ChampsBase64Null()
    {
        var affilie = new Affilie
        {
            IdAffilie = 2,
            CodeAdhesion = "CODE-2",
            Nom = "Sans",
            Prenom = "Photo",
            NomComplet = "Sans Photo",
            DateNaissance = new DateTime(1991, 2, 2),
            Statut = true
        };

        var dto = AffilieDtoMapper.ToReadDto(affilie);

        Assert.False(dto.HasPhoto);
        Assert.False(dto.HasCarteIdentite);
        Assert.Null(dto.PhotoBase64);
        Assert.Null(dto.PhotoUrl);
        Assert.Null(dto.CarteIdentiteBase64);
        Assert.Null(AffilieFichierHelper.VersBase64(null));
        Assert.Empty(dto.Dependants);
        Assert.Empty(dto.Antecedants);
        Assert.Null(dto.PersonneContact);
    }

    [Fact]
    public void ToReadDto_ExposeDependantsAntecedantsEtPersonneContact()
    {
        var certificat = new byte[] { 4, 5, 6 };
        var affilie = new Affilie
        {
            IdAffilie = 10,
            CodeAdhesion = "CODE-10",
            Nom = "Parent",
            Prenom = "Assoc",
            NomComplet = "Parent Assoc",
            DateNaissance = new DateTime(1985, 3, 3),
            Statut = true,
            PersonneContact = new PersonneContact
            {
                IdPersonneContact = 1,
                AffilieId = 10,
                NomComplet = "Contact Test",
                LienParente = "EPOUX",
                Adresse = "Kinshasa",
                Statut = true
            },
            Dependants =
            {
                new Dependant
                {
                    IdDependant = 5,
                    AffilieId = 10,
                    Nom = "Enfant",
                    LienParente = "FILS",
                    Adresse = "Gombe",
                    DateNaissance = new DateTime(2010, 1, 1),
                    CertificatScolariteData = certificat,
                    CertificatScolariteContentType = "application/pdf",
                    Statut = true,
                    Antecedants =
                    {
                        new Antecedant
                        {
                            IdAntecedant = 2,
                            AffilieId = 10,
                            DependantId = 5,
                            Description = "Asthme",
                            Statut = true,
                            DateCreation = DateTime.UtcNow
                        }
                    }
                }
            },
            Antecedants =
            {
                new Antecedant
                {
                    IdAntecedant = 3,
                    AffilieId = 10,
                    Description = "Hypertension",
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                }
            }
        };

        var dto = AffilieDtoMapper.ToReadDto(affilie);

        Assert.NotNull(dto.PersonneContact);
        Assert.Equal("Contact Test", dto.PersonneContact!.NomComplet);
        Assert.Equal("EPOUX", dto.PersonneContact.LienParente);

        var dependant = Assert.Single(dto.Dependants);
        Assert.Equal("Enfant", dependant.Nom);
        Assert.Equal(Convert.ToBase64String(certificat), dependant.CertificatScolariteBase64);
        Assert.Equal("application/pdf", dependant.CertificatScolariteContentType);
        Assert.True(dependant.PossedeCertificatScolarite);
        Assert.Single(dependant.Antecedants);

        var antecedent = Assert.Single(dto.Antecedants);
        Assert.Equal("Hypertension", antecedent.Description);
        Assert.Equal(10, antecedent.AffilieId);
        Assert.True(antecedent.Statut);
    }
}
