using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;

namespace Prosoc.Tests.Unit.Services;

public class DashboardAgentHopitalServiceTests
{
    private static async Task<(ProsocDbContext Db, DashboardAgentHopitalService Service)> CreateContextAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var service = new DashboardAgentHopitalService(
            db,
            new DeviseConversionService(db),
            new Mock<ILogger<DashboardAgentHopitalService>>().Object);

        return (db, service);
    }

    private static async Task<(int HopitalCibleId, int AutreHopitalId)> SeedHopitalScopeDataAsync(ProsocDbContext db)
    {
        var devise = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
        db.Devises.Add(devise);

        var hopitalCible = new HopitalPartenaire
        {
            Nom = "Hôpital A",
            CodeAcces = "HOP-A",
            Statut = true
        };
        var autreHopital = new HopitalPartenaire
        {
            Nom = "Hôpital B",
            CodeAcces = "HOP-B",
            Statut = true
        };
        db.HopitalPartenaires.AddRange(hopitalCible, autreHopital);
        await db.SaveChangesAsync();

        var prestation = new Prestation
        {
            NomPrestation = "Consultation",
            Montant = 30m,
            DeviseId = devise.IdDevise,
            Statut = true
        };
        db.Prestations.Add(prestation);

        var affilieCible = new Affilie
        {
            CodeAdhesion = "PAT-A",
            Nom = "Marie",
            Prenom = "Kaba",
            NomComplet = "Marie Kaba",
            DateNaissance = new DateTime(1990, 1, 15),
            Statut = true
        };
        var affilieAutre = new Affilie
        {
            CodeAdhesion = "PAT-B",
            Nom = "Paul",
            Prenom = "Mba",
            NomComplet = "Paul Mba",
            DateNaissance = new DateTime(1988, 6, 20),
            Statut = true
        };
        db.Affilies.AddRange(affilieCible, affilieAutre);
        await db.SaveChangesAsync();

        var jetonEnAttente = new JetonMedical
        {
            AffilieId = affilieCible.IdAffilie,
            HopitalPartenaireId = hopitalCible.IdHopital,
            CodeJeton = "JET-A-001",
            EstValide = true,
            EstUtilise = false,
            DateExpiration = DateTime.Now.AddDays(30),
            Statut = true
        };
        var jetonUtilise = new JetonMedical
        {
            AffilieId = affilieCible.IdAffilie,
            HopitalPartenaireId = hopitalCible.IdHopital,
            CodeJeton = "JET-A-002",
            EstValide = true,
            EstUtilise = true,
            DateUtilisation = DateTime.Now,
            Statut = true
        };
        var jetonAutreHopital = new JetonMedical
        {
            AffilieId = affilieAutre.IdAffilie,
            HopitalPartenaireId = autreHopital.IdHopital,
            CodeJeton = "JET-B-001",
            EstValide = true,
            EstUtilise = false,
            DateExpiration = DateTime.Now.AddDays(30),
            Statut = true
        };
        db.JetonsMedicaux.AddRange(jetonEnAttente, jetonUtilise, jetonAutreHopital);
        await db.SaveChangesAsync();

        var bon = new BonEnvoi
        {
            NumeroBon = "BON-A-001",
            AffilieId = affilieCible.IdAffilie,
            PrestationId = prestation.IdPrestation,
            EstUtilise = false,
            Statut = true
        };
        db.BonsEnvoi.Add(bon);
        await db.SaveChangesAsync();

        db.DemandesBonEnvoi.Add(new DemandeBonEnvoi
        {
            AffilieId = affilieCible.IdAffilie,
            PrestationId = prestation.IdPrestation,
            StatutDemande = "VALIDEE",
            JetonMedicalId = jetonEnAttente.IdJeton,
            BonEnvoiId = bon.IdBonEnvoi,
            Statut = true
        });

        db.Dependants.AddRange(
            new Dependant
            {
                Nom = "Enfant Marie",
                LienParente = "Enfant",
                AffilieId = affilieCible.IdAffilie,
                Statut = true
            },
            new Dependant
            {
                Nom = "Enfant Paul",
                LienParente = "Enfant",
                AffilieId = affilieAutre.IdAffilie,
                Statut = true
            });

        db.Antecedants.AddRange(
            new Antecedant
            {
                Description = "Hypertension",
                AffilieId = affilieCible.IdAffilie,
                Statut = true
            },
            new Antecedant
            {
                Description = "Asthme",
                AffilieId = affilieAutre.IdAffilie,
                Statut = true
            });

        await db.SaveChangesAsync();

        return (hopitalCible.IdHopital, autreHopital.IdHopital);
    }

    [Fact]
    public async Task GetKpisAsync_ScopeJetonsEtPatientsParHopital()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (hopitalCibleId, _) = await SeedHopitalScopeDataAsync(db);

            var kpis = await service.GetKpisAsync(hopitalCibleId);

            Assert.Equal(2, kpis.JetonsEmisTotal);
            Assert.Equal(1, kpis.JetonsValidesEnAttente);
            Assert.Equal(1, kpis.JetonsUtilisesMois);
            Assert.Equal(1, kpis.BonsLiesTotal);
            Assert.Equal(1, kpis.PatientsUniques);
            Assert.Equal(1, kpis.TotalDependants);
            Assert.Equal(1, kpis.TotalAntecedents);
        }
    }

    [Fact]
    public async Task GetPatientsAsync_RetourneCompteursDependantsEtAntecedents()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (hopitalCibleId, _) = await SeedHopitalScopeDataAsync(db);

            var patients = await service.GetPatientsAsync(hopitalCibleId);

            Assert.Single(patients);
            Assert.Equal("PAT-A", patients[0].CodeAdhesion);
            Assert.Equal(2, patients[0].NombreJetons);
            Assert.Equal(1, patients[0].NombreDependants);
            Assert.Equal(1, patients[0].NombreAntecedents);
        }
    }

    [Fact]
    public async Task GetDependantsEtAntecedentsAsync_FiltrentParHopital()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (hopitalCibleId, _) = await SeedHopitalScopeDataAsync(db);

            var dependants = await service.GetDependantsAsync(hopitalCibleId);
            var antecedents = await service.GetAntecedentsAsync(hopitalCibleId);

            Assert.Single(dependants);
            Assert.Equal("Enfant Marie", dependants[0].Nom);

            Assert.Single(antecedents);
            Assert.Equal("Hypertension", antecedents[0].Description);
        }
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_InclutJetonsEtPatients()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (hopitalCibleId, _) = await SeedHopitalScopeDataAsync(db);

            var summary = await service.GetDashboardSummaryAsync(hopitalCibleId);

            Assert.Equal("Hôpital A", summary.NomHopital);
            Assert.Single(summary.JetonsEnAttente);
            Assert.Single(summary.BonsRecents);
            Assert.Single(summary.Patients);
            Assert.Equal(1, summary.Kpis.PatientsUniques);
        }
    }

    [Fact]
    public async Task GetKpisAsync_ValeurPrestationsConsolideeEnDevisePrincipale()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (hopitalCibleId, _) = await SeedHopitalScopeDataAsync(db);

            var usd = await db.Devises.FirstAsync(d => d.Code == "USD");
            usd.EstDevisePrincipale = true;

            var cdf = new Devise { Code = "CDF", Nom = "Franc congolais", Statut = true };
            db.Devises.Add(cdf);
            await db.SaveChangesAsync();

            db.TauxChangeDevises.Add(new TauxChangeDevise
            {
                DeviseSourceId = usd.IdDevise,
                DeviseCibleId = cdf.IdDevise,
                Taux = 2850m,
                DateEffet = new DateTime(2026, 1, 1),
                Statut = true
            });

            var prestationCdf = new Prestation
            {
                NomPrestation = "Radio",
                Montant = 2850m,
                DeviseId = cdf.IdDevise,
                Statut = true
            };
            db.Prestations.Add(prestationCdf);
            await db.SaveChangesAsync();

            var affilieId = await db.Affilies.Where(a => a.CodeAdhesion == "PAT-A").Select(a => a.IdAffilie).FirstAsync();
            var jetonCdf = new JetonMedical
            {
                AffilieId = affilieId,
                HopitalPartenaireId = hopitalCibleId,
                CodeJeton = "JET-A-003",
                EstValide = true,
                EstUtilise = false,
                DateEmission = DateTime.Now,
                DateExpiration = DateTime.Now.AddDays(30),
                Statut = true
            };
            db.JetonsMedicaux.Add(jetonCdf);
            await db.SaveChangesAsync();

            db.DemandesBonEnvoi.Add(new DemandeBonEnvoi
            {
                AffilieId = affilieId,
                PrestationId = prestationCdf.IdPrestation,
                StatutDemande = "VALIDEE",
                JetonMedicalId = jetonCdf.IdJeton,
                Statut = true
            });
            await db.SaveChangesAsync();

            var kpis = await service.GetKpisAsync(hopitalCibleId);

            // 1 jeton en attente à 30 USD + 1 jeton CDF à 1 USD (2850 CDF) = 31 USD
            Assert.Equal(31m, kpis.ValeurPrestationsJetonsTotal);
            Assert.Equal(31m, kpis.ValeurPrestationsJetonsMois);
            Assert.Equal(30m, kpis.ValeurPrestationsBonsTotal);
            Assert.Equal("USD", kpis.DevisePrincipaleCode);
        }
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_ExposeDevisePrincipaleCode()
    {
        var (db, service) = await CreateContextAsync();
        await using (db)
        {
            var (hopitalCibleId, _) = await SeedHopitalScopeDataAsync(db);

            var summary = await service.GetDashboardSummaryAsync(hopitalCibleId);

            Assert.Equal("USD", summary.Kpis.DevisePrincipaleCode);
            Assert.Equal("USD", summary.DevisePrincipaleCode);
            Assert.Equal(30m, summary.BonsRecents[0].MontantPrestation);
            Assert.Equal(30m, summary.JetonsEnAttente[0].MontantPrestation);
        }
    }
}
