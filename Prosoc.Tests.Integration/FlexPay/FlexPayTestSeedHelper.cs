using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.DTOs.FlexPay;

namespace Prosoc.Tests.Integration.FlexPay;

internal static class FlexPayTestSeedHelper
{
    public static async Task EnsureMarchandActifAsync(ProsocDbContext db)
    {
        if (await db.InfoPaiementsMarchand.AnyAsync(m => m.Statut))
            return;

        db.InfoPaiementsMarchand.Add(new InfoPaiementMarchand
        {
            CodeMarchand = "TEST-MERCHANT",
            ApiToken = "test-token-flexpay",
            ActifMobileMoney = true,
            ActifCarteBancaire = true,
            Statut = true,
            DateCreation = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public static async Task<(int AffilieId, int AgentId, int FraisId, int DeviseId)> SeedAffilieAgentFraisAsync(
        ProsocDbContext db)
    {
        var devise = await db.Devises.FirstAsync(d => d.Code == "CDF");
        var frais = await db.Frais.FirstAsync(f => f.Statut);

        var zone = await db.ZonesSociales.FirstAsync();
        var agent = new Agent
        {
            NomComplet = "Agent FlexPay Test",
            Matricule = $"MAT-FP-{Guid.NewGuid():N}"[..10],
            Phone = "0811111111",
            ZoneSocialeId = zone.IdZoneSociale,
            Statut = true,
            DateCreation = DateTime.UtcNow
        };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();

        var affilie = new Affilie
        {
            Nom = "FlexPay",
            Prenom = "Test",
            NomComplet = "Test FlexPay",
            DateNaissance = new DateTime(1990, 1, 1),
            Telephone = $"08{Guid.NewGuid():N}"[..10],
            ProvinceResidence = "Kinshasa",
            CommuneResidence = "Gombe",
            QuartierResidence = "Centre",
            Statut = true,
            DateCreation = DateTime.UtcNow
        };
        db.Affilies.Add(affilie);
        await db.SaveChangesAsync();

        var userId = await db.Utilisateurs.Select(u => u.IdUtilisateur).FirstAsync();
        db.Adhesions.Add(new Adhesion
        {
            AffilieId = affilie.IdAffilie,
            TypeAdhesionId = 1,
            AgentId = agent.IdAgent,
            UtilisateurId = userId,
            StatutDossier = "VALIDE",
            Statut = true,
            DateCreation = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        return (affilie.IdAffilie, agent.IdAgent, frais.IdFrais, devise.IdDevise);
    }

    /// <summary>Convertit un montant tarif vers la devise de paiement (même logique que FlexPayCollecteService).</summary>
    public static async Task<(decimal MontantTarif, int DeviseTarifId, decimal MontantFlexPay, decimal Taux)>
        ResolveMontantFlexPayAsync(
            ProsocDbContext db,
            decimal montantTarif,
            int deviseTarifId,
            int devisePaiementId,
            DateTime dateRef)
    {
        if (deviseTarifId == devisePaiementId)
            return (montantTarif, deviseTarifId, montantTarif, 1m);

        var tauxDirect = await db.TauxChangeDevises.AsNoTracking()
            .Where(t => t.Statut && t.DeviseSourceId == deviseTarifId && t.DeviseCibleId == devisePaiementId
                && t.DateEffet <= dateRef)
            .OrderByDescending(t => t.DateEffet)
            .FirstOrDefaultAsync();

        if (tauxDirect != null)
        {
            var montant = Math.Round(montantTarif * tauxDirect.Taux, 2, MidpointRounding.AwayFromZero);
            var paiement = await db.Devises.AsNoTracking().FirstAsync(d => d.IdDevise == devisePaiementId);
            if (paiement.Code == "CDF")
                montant = Math.Round(montant, 0, MidpointRounding.AwayFromZero);
            return (montantTarif, deviseTarifId, montant, tauxDirect.Taux);
        }

        var tauxInverse = await db.TauxChangeDevises.AsNoTracking()
            .Where(t => t.Statut && t.DeviseSourceId == devisePaiementId && t.DeviseCibleId == deviseTarifId
                && t.DateEffet <= dateRef)
            .OrderByDescending(t => t.DateEffet)
            .FirstOrDefaultAsync();

        if (tauxInverse != null && tauxInverse.Taux != 0)
        {
            var taux = 1m / tauxInverse.Taux;
            var montant = Math.Round(montantTarif * taux, 2, MidpointRounding.AwayFromZero);
            var paiement = await db.Devises.AsNoTracking().FirstAsync(d => d.IdDevise == devisePaiementId);
            if (paiement.Code == "CDF")
                montant = Math.Round(montant, 0, MidpointRounding.AwayFromZero);
            return (montantTarif, deviseTarifId, montant, taux);
        }

        throw new InvalidOperationException(
            $"Aucun taux actif pour conversion devise {deviseTarifId} → {devisePaiementId}.");
    }

    public static async Task<(Guid EnAttenteId, string OrderNumber, string Reference, decimal MontantFlexPay)>
        SeedCollecteEnAttenteAsync(
            ProsocDbContext db,
            int affilieId,
            int agentId,
            int fraisId,
            int devisePaiementId,
            string orderNumber)
    {
        var frais = await db.Frais.AsNoTracking().FirstAsync(f => f.IdFrais == fraisId);
        var montantTarif = (decimal)frais.Montant;
        var deviseTarifId = frais.DeviseId;
        var dateRef = DateTime.UtcNow;

        var (_, _, montantFlexPay, taux) = await ResolveMontantFlexPayAsync(
            db, montantTarif, deviseTarifId, devisePaiementId, dateRef);

        var devisePaiement = await db.Devises.AsNoTracking().FirstAsync(d => d.IdDevise == devisePaiementId);
        var codeDevisePaiement = devisePaiement.Code.ToUpperInvariant();

        var id = Guid.NewGuid();
        var reference = $"PS-{id:N}"[..20];

        var dto = new CollecteCreateDto
        {
            TypeCollecte = TypeCollecte.Frais,
            FraisId = fraisId,
            AffilieId = affilieId,
            AgentId = agentId,
            Montant = montantFlexPay,
            Mois = DateTime.UtcNow.Month,
            Annee = DateTime.UtcNow.Year,
            ModePaiement = "MOBILE_MONEY",
            DeviseId = devisePaiementId,
            Statut = true
        };

        var enAttente = new CollecteEnAttente
        {
            IdCollecteEnAttente = id,
            SourceFlux = CollecteEnAttenteSourceFlux.CollecteAgent,
            AffilieId = affilieId,
            AgentId = agentId,
            TypeCollecte = TypeCollecte.Frais,
            FraisId = fraisId,
            Mois = dto.Mois,
            Annee = dto.Annee,
            MethodePaiement = "MOBILE_MONEY",
            MontantTarif = montantTarif,
            DeviseTarifId = deviseTarifId,
            MontantFlexPay = montantFlexPay,
            CodeDevisePaiement = codeDevisePaiement,
            TauxVersDevisePaiement = taux,
            ReferenceFlexPay = reference,
            OrderNumberFlexPay = orderNumber,
            PayloadMetierJson = JsonSerializer.Serialize(dto),
            DateExpiration = DateTime.UtcNow.AddMinutes(15),
            StatutEnAttente = CollecteEnAttenteStatut.EnAttente
        };

        db.CollectesEnAttente.Add(enAttente);
        db.TransactionsFlexPay.Add(new TransactionFlexPay
        {
            OrderNumber = orderNumber,
            Reference = reference,
            Amount = montantFlexPay,
            Currency = codeDevisePaiement,
            IdCollecteEnAttente = id,
            SourceFlux = CollecteEnAttenteSourceFlux.CollecteAgent
        });
        await db.SaveChangesAsync();

        return (id, orderNumber, reference, montantFlexPay);
    }

    public static async Task<(int PrestationId, int CotisationAffilieId, int DeviseId, decimal MontantSouscription)>
        SeedPrestationCotisationAsync(ProsocDbContext db)
    {
        var devise = await db.Devises.FirstAsync(d => d.Code == "USD");

        var cotisationAffilie = await db.CotisationsAffilie
            .FirstOrDefaultAsync(c => c.TypeAdhesionId == 1 && c.Periodicite == "Mensuel");
        if (cotisationAffilie == null)
        {
            cotisationAffilie = new CotisationAffilie
            {
                Montant = 1.5m,
                Periodicite = "Mensuel",
                TypeAdhesionId = 1,
                DeviseId = devise.IdDevise,
                Statut = true
            };
            db.CotisationsAffilie.Add(cotisationAffilie);
            await db.SaveChangesAsync();
        }

        const decimal montantSouscription = 50m;
        var produit = new ProduitMutuel
        {
            Nom = $"FP-{Guid.NewGuid():N}"[..8],
            Montant = montantSouscription,
            EstGratuit = false,
            Periodicite = "Mensuel",
            AgeMin = 0,
            AgeMax = 120,
            DeviseId = devise.IdDevise,
            Statut = true
        };
        db.ProduitsMutuels.Add(produit);
        await db.SaveChangesAsync();

        var prestation = new Prestation
        {
            NomPrestation = "Prest FP",
            Montant = montantSouscription,
            DeviseId = devise.IdDevise,
            ProduitMutuelId = produit.IdProduit,
            Statut = true
        };
        db.Prestations.Add(prestation);
        await db.SaveChangesAsync();
        return (prestation.IdPrestation, cotisationAffilie.IdCotisationAffilie, devise.IdDevise, montantSouscription);
    }
}
