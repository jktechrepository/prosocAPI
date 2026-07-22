using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Unit.Services;

public class DemandeBonEnvoiServiceLinkingTests
{
    private static async Task<(DemandeBonEnvoiService Service, ProsocDbContext Db)> CreateServiceAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var affilieRepo = new Mock<IAffilieRepository>();
        var adhesionRepo = new Mock<IAdhesionRepository>();
        var bonRepo = new Mock<IBonEnvoiRepository>();
        var jetonRepo = new Mock<IJetonMedicalRepository>();
        var qrService = new Mock<IBonEnvoiQrCodeService>();
        qrService
            .Setup(q => q.ApplyQrToBonAsync(It.IsAny<BonEnvoi>(), It.IsAny<CancellationToken>()))
            .Returns<BonEnvoi, CancellationToken>((b, _) =>
            {
                b.QrCodePayload = "payload";
                b.QrCodeImageBase64 = "image";
                return Task.CompletedTask;
            });

        var service = new DemandeBonEnvoiService(
            db,
            Mock.Of<ILogger<DemandeBonEnvoiService>>(),
            jetonRepo.Object,
            bonRepo.Object,
            affilieRepo.Object,
            adhesionRepo.Object,
            qrService.Object);

        var affilie = new Affilie
        {
            Nom = "A",
            Prenom = "B",
            NomComplet = "A B",
            DateNaissance = new DateTime(1990, 1, 1),
            CodeAdhesion = "AFF-DBE-1",
            Statut = true
        };
        db.Affilies.Add(affilie);

        var devise = new Devise
        {
            Code = "USD",
            Nom = "Dollar",
            EstDevisePrincipale = true,
            Statut = true
        };
        db.Devises.Add(devise);
        await db.SaveChangesAsync();

        var prestation = new Prestation
        {
            NomPrestation = "Consultation",
            Description = "Desc",
            Montant = 20,
            DeviseId = devise.IdDevise,
            Statut = true
        };
        db.Prestations.Add(prestation);

        var categorieAdhesion = new CategorieAdhesion
        {
            Libelle = "Cat",
            Statut = true
        };
        db.CategoriesAdhesions.Add(categorieAdhesion);
        await db.SaveChangesAsync();

        var hopital = new HopitalPartenaire
        {
            Nom = "H1",
            Adresse = "Adr",
            Telephone = "0991111111",
            Statut = true
        };
        db.HopitalPartenaires.Add(hopital);

        var agent = new Agent
        {
            NomComplet = "Agent 1",
            Matricule = "MAT-DBE-1",
            Phone = "0990000001",
            Statut = true
        };
        var utilisateur = new Utilisateur
        {
            NomUtilisateur = "u-dbe",
            MotDePasseHash = "hash",
            Statut = true
        };
        db.Agents.Add(agent);
        db.Utilisateurs.Add(utilisateur);
        await db.SaveChangesAsync();

        var typeAdhesion = new TypeAdhesion
        {
            Libelle = "Standard",
            CategorieAdhesionId = categorieAdhesion.IdCategorieAdhesion,
            DeviseId = devise.IdDevise,
            Statut = true
        };
        db.TypeAdhesions.Add(typeAdhesion);
        await db.SaveChangesAsync();

        var demande = new DemandeBonEnvoi
        {
            AffilieId = affilie.IdAffilie,
            PrestationId = prestation.IdPrestation,
            MotifDemande = "Motif",
            DateDemande = DateTime.Now,
            DateCreation = DateTime.Now,
            StatutDemande = "EN_ATTENTE",
            Statut = true
        };
        db.DemandesBonEnvoi.Add(demande);

        db.SouscriptionsPrestations.Add(new SouscriptionPrestation
        {
            AffilieId = affilie.IdAffilie,
            PrestationId = prestation.IdPrestation,
            DateSouscription = DateTime.Now.AddMonths(-1),
            Statut = true
        });

        db.Adhesions.Add(new Adhesion
        {
            AffilieId = affilie.IdAffilie,
            AgentId = agent.IdAgent,
            TypeAdhesionId = typeAdhesion.IdTypeAdhesion,
            UtilisateurId = utilisateur.IdUtilisateur,
            StatutDossier = "Complet",
            Statut = true,
            DateCreation = DateTime.Now
        });

        db.Collectes.AddRange(
            new Collecte
            {
                AffilieId = affilie.IdAffilie,
                AgentId = agent.IdAgent,
                DeviseId = devise.IdDevise,
                Montant = 10,
                TypeCollecte = TypeCollecte.Cotisation,
                StatutPaiement = CollecteStatutPaiement.Valide,
                ModePaiement = "ESPECE",
                DateCollecte = DateTime.Now.AddDays(-20),
                Statut = true
            },
            new Collecte
            {
                AffilieId = affilie.IdAffilie,
                AgentId = agent.IdAgent,
                DeviseId = devise.IdDevise,
                Montant = 10,
                TypeCollecte = TypeCollecte.Cotisation,
                StatutPaiement = CollecteStatutPaiement.Valide,
                ModePaiement = "ESPECE",
                DateCollecte = DateTime.Now.AddDays(-40),
                Statut = true
            },
            new Collecte
            {
                AffilieId = affilie.IdAffilie,
                AgentId = agent.IdAgent,
                DeviseId = devise.IdDevise,
                Montant = 10,
                TypeCollecte = TypeCollecte.Cotisation,
                StatutPaiement = CollecteStatutPaiement.Valide,
                ModePaiement = "ESPECE",
                DateCollecte = DateTime.Now.AddDays(-60),
                Statut = true
            });

        await db.SaveChangesAsync();

        affilieRepo.Setup(r => r.GetByIdAsync(affilie.IdAffilie, It.IsAny<CancellationToken>()))
            .ReturnsAsync(affilie);
        adhesionRepo.Setup(r => r.GetByAffilieIdAsync(affilie.IdAffilie, It.IsAny<CancellationToken>()))
            .ReturnsAsync(db.Adhesions.First(a => a.AffilieId == affilie.IdAffilie));

        return (service, db);
    }

    [Fact]
    public async Task ConfirmerDemandeAsync_CreeCoupleBonJetonLie()
    {
        var (service, db) = await CreateServiceAsync();
        var demande = await db.DemandesBonEnvoi.FirstAsync();
        var agent = await db.Agents.FirstAsync();
        var hopital = await db.HopitalPartenaires.FirstAsync();

        var result = await service.ConfirmerDemandeAsync(
            demande.IdDemande,
            new DemandeBonEnvoiConfirmerDto
            {
                AgentId = agent.IdAgent,
                Accepter = true,
                HopitalPartenaireId = hopital.IdHopital
            });

        Assert.True(result.Succes);
        Assert.NotNull(result.BonEnvoiId);
        Assert.NotNull(result.JetonMedicalId);

        var bon = await db.BonsEnvoi.FirstAsync(b => b.IdBonEnvoi == result.BonEnvoiId);
        Assert.Equal(result.JetonMedicalId, bon.JetonMedicalId);
        Assert.False(string.IsNullOrWhiteSpace(bon.QrCodePayload));

        var demandeUpdated = await db.DemandesBonEnvoi.FirstAsync(d => d.IdDemande == demande.IdDemande);
        Assert.Equal("VALIDEE", demandeUpdated.StatutDemande);
        Assert.Equal(result.BonEnvoiId, demandeUpdated.BonEnvoiId);
        Assert.Equal(result.JetonMedicalId, demandeUpdated.JetonMedicalId);
    }
}
