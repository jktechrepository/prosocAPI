using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prosoc.Data;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services
{
    public class DeviseConversionService : IDeviseConversionService
    {
        private readonly ProsocDbContext _db;

        public DeviseConversionService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<Devise> GetDevisePrincipaleAsync(CancellationToken ct = default)
        {
            var principale = await _db.Devises
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.EstDevisePrincipale && d.Statut, ct);

            if (principale == null)
                throw new InvalidOperationException(
                    "Aucune devise principale active configurée. Veuillez définir une devise principale (USD).");

            return principale;
        }

        public async Task<TauxChangeDevise?> GetTauxActifAsync(
            int deviseSourceId,
            int deviseCibleId,
            DateTime dateReference,
            CancellationToken ct = default)
        {
            if (deviseSourceId == deviseCibleId)
                return null;

            return await _db.TauxChangeDevises
                .AsNoTracking()
                .Where(t => t.DeviseSourceId == deviseSourceId
                            && t.DeviseCibleId == deviseCibleId
                            && t.Statut
                            && t.DateEffet <= dateReference)
                .OrderByDescending(t => t.DateEffet)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<(decimal montantConverti, decimal taux)> ConvertirAsync(
            decimal montant,
            int deviseSourceId,
            int deviseCibleId,
            DateTime dateReference,
            CancellationToken ct = default)
        {
            if (deviseSourceId == deviseCibleId)
                return (montant, 1m);

            var tauxDirect = await GetTauxActifAsync(deviseSourceId, deviseCibleId, dateReference, ct);
            if (tauxDirect != null)
            {
                var montantConverti = Math.Round(montant * tauxDirect.Taux, 2, MidpointRounding.AwayFromZero);
                return (montantConverti, tauxDirect.Taux);
            }

            var tauxInverse = await GetTauxActifAsync(deviseCibleId, deviseSourceId, dateReference, ct);
            if (tauxInverse != null && tauxInverse.Taux != 0)
            {
                var taux = 1m / tauxInverse.Taux;
                var montantConverti = Math.Round(montant * taux, 2, MidpointRounding.AwayFromZero);
                return (montantConverti, taux);
            }

            var source = await _db.Devises.AsNoTracking().FirstOrDefaultAsync(d => d.IdDevise == deviseSourceId, ct);
            var cible = await _db.Devises.AsNoTracking().FirstOrDefaultAsync(d => d.IdDevise == deviseCibleId, ct);
            throw new InvalidOperationException(
                $"Aucun taux de change actif trouvé pour {source?.Code ?? deviseSourceId.ToString()} → {cible?.Code ?? deviseCibleId.ToString()} à la date {dateReference:yyyy-MM-dd}.");
        }

        public async Task<PreviewConversionDto> PreviewConversionAsync(
            string codeDeviseSource,
            decimal montant,
            string? codeDeviseCible,
            DateTime datePaiement,
            CancellationToken ct = default)
        {
            var source = await _db.Devises
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Code == codeDeviseSource.ToUpperInvariant() && d.Statut, ct)
                ?? throw new ArgumentException($"Devise source '{codeDeviseSource}' introuvable ou inactive.");

            Devise cible;
            if (!string.IsNullOrWhiteSpace(codeDeviseCible))
            {
                cible = await _db.Devises
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Code == codeDeviseCible.ToUpperInvariant() && d.Statut, ct)
                    ?? throw new ArgumentException($"Devise cible '{codeDeviseCible}' introuvable ou inactive.");
            }
            else
            {
                cible = await GetDevisePrincipaleAsync(ct);
            }

            var principale = await GetDevisePrincipaleAsync(ct);
            var (montantConverti, taux) = await ConvertirAsync(montant, source.IdDevise, cible.IdDevise, datePaiement, ct);

            return new PreviewConversionDto
            {
                CodeDeviseSource = source.Code,
                CodeDeviseCible = cible.Code,
                CodeDevisePrincipale = principale.Code,
                DatePaiement = datePaiement,
                Taux = taux,
                MontantSource = montant,
                MontantConverti = montantConverti
            };
        }
    }

    public class CollecteMultideviseService : ICollecteMultideviseService
    {
        private readonly ProsocDbContext _db;
        private readonly IDeviseConversionService _conversion;
        private readonly ICotisationAffilieMetierService _cotisationMetier;
        private readonly MultideviseOptions _options;

        public CollecteMultideviseService(
            ProsocDbContext db,
            IDeviseConversionService conversion,
            ICotisationAffilieMetierService cotisationMetier,
            IOptions<MultideviseOptions> options)
        {
            _db = db;
            _conversion = conversion;
            _cotisationMetier = cotisationMetier;
            _options = options.Value;
        }

        public async Task<(decimal montantAttendu, int deviseTarifId)> ResolveTarifAttenduAsync(
            Collecte collecte,
            int nombreDependants,
            CancellationToken ct = default)
        {
            switch (collecte.TypeCollecte)
            {
                case TypeCollecte.Cotisation:
                {
                    if (!collecte.CotisationAffilieId.HasValue)
                        throw new ArgumentException("CotisationAffilieId requis pour résoudre le tarif.");

                    var calcul = await _cotisationMetier.CalculerMontantTotalAsync(
                        collecte.CotisationAffilieId.Value, nombreDependants, ct);
                    return (calcul.MontantTotal, calcul.DeviseId);
                }
                case TypeCollecte.Souscription:
                {
                    if (!collecte.SouscriptionPrestationId.HasValue)
                        throw new ArgumentException(
                            "Collecte SOUSCRIPTION : prestationId requis (via collectes[].souscription.prestationId).");

                    var produit = await ResolveProduitSouscriptionOuPrestationAsync(
                        collecte.SouscriptionPrestationId.Value, ct)
                        ?? throw new ArgumentException("Produit introuvable pour cette souscription.");

                    var deviseId = await ResolveProduitDeviseIdAsync(collecte.SouscriptionPrestationId.Value, ct);
                    return (produit.Montant, deviseId);
                }
                case TypeCollecte.Frais:
                {
                    if (!collecte.FraisId.HasValue)
                        throw new ArgumentException("FraisId requis pour résoudre le tarif.");

                    var frais = await _db.Frais.AsNoTracking()
                        .FirstOrDefaultAsync(f => f.IdFrais == collecte.FraisId.Value, ct)
                        ?? throw new ArgumentException($"Frais {collecte.FraisId} introuvable.");

                    return ((decimal)frais.Montant, frais.DeviseId);
                }
                default:
                    throw new ArgumentException($"Type de collecte {collecte.TypeCollecte} non supporté pour la résolution tarif.");
            }
        }

        public async Task ValidateAndApplySnapshotAsync(
            Collecte collecte,
            int nombreDependants,
            CancellationToken ct = default,
            DateTime? dateConversion = null)
        {
            var devisePaiement = await _db.Devises.AsNoTracking()
                .FirstOrDefaultAsync(d => d.IdDevise == collecte.DeviseId && d.Statut, ct);
            if (devisePaiement == null)
                throw new ArgumentException($"Devise de paiement {collecte.DeviseId} introuvable ou inactive.");

            var (montantAttendu, deviseTarifId) = await ResolveTarifAttenduAsync(collecte, nombreDependants, ct);
            var dateRef = dateConversion ?? collecte.DateCollecte;

            if (collecte.Montant <= 0 && collecte.TypeCollecte != TypeCollecte.Souscription)
                throw new ArgumentException("Le montant de la collecte doit être supérieur à zéro.");

            if (collecte.TypeCollecte == TypeCollecte.Souscription)
            {
                var produit = await ResolveProduitSouscriptionOuPrestationAsync(
                    collecte.SouscriptionPrestationId!.Value, ct);
                if (produit != null && produit.EstGratuit && collecte.Montant == 0)
                {
                    await ApplySnapshotInternalAsync(collecte, deviseTarifId, 0m, dateRef, ct);
                    return;
                }
            }

            var (montantPayeEnDeviseTarif, _) = await _conversion.ConvertirAsync(
                collecte.Montant, collecte.DeviseId, deviseTarifId, dateRef, ct);

            if (Math.Abs(montantPayeEnDeviseTarif - montantAttendu) > _options.ToleranceConversion)
            {
                var deviseTarif = await _db.Devises.AsNoTracking()
                    .FirstAsync(d => d.IdDevise == deviseTarifId, ct);
                throw new ArgumentException(
                    $"Montant incorrect. Attendu : {montantAttendu:F2} {deviseTarif.Code}, " +
                    $"reçu (converti) : {montantPayeEnDeviseTarif:F2} {deviseTarif.Code}.");
            }

            await ApplySnapshotInternalAsync(collecte, deviseTarifId, montantAttendu, dateRef, ct);
        }

        private async Task ApplySnapshotInternalAsync(
            Collecte collecte,
            int deviseTarifId,
            decimal montantTarifAttendu,
            DateTime dateRef,
            CancellationToken ct)
        {
            var principale = await _conversion.GetDevisePrincipaleAsync(ct);
            var (montantPrincipal, taux) = await _conversion.ConvertirAsync(
                collecte.Montant, collecte.DeviseId, principale.IdDevise, dateRef, ct);

            collecte.DevisePrincipaleId = principale.IdDevise;
            collecte.TauxVersDevisePrincipale = taux;
            collecte.MontantDevisePrincipale = montantPrincipal;
            collecte.DeviseTarifId = deviseTarifId;
            collecte.MontantTarifAttendu = montantTarifAttendu;
        }

        private async Task<int> ResolveProduitDeviseIdAsync(int souscriptionOuPrestationId, CancellationToken ct)
        {
            var souscription = await _db.SouscriptionsPrestations
                .AsNoTracking()
                .Include(sp => sp.Prestation)
                .FirstOrDefaultAsync(sp => sp.IdSouscriptionPrestation == souscriptionOuPrestationId, ct);

            if (souscription?.Prestation != null)
                return await ResolveDeviseIdFromPrestationAsync(souscription.Prestation, ct);

            var prestation = await _db.Prestations.AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdPrestation == souscriptionOuPrestationId, ct)
                ?? throw new ArgumentException("Prestation introuvable pour la souscription.");

            return await ResolveDeviseIdFromPrestationAsync(prestation, ct);
        }

        /// <summary>
        /// Adhésion FlexPay : <c>SouscriptionPrestationId</c> peut être un <c>PrestationId</c> avant création de la souscription.
        /// </summary>
        private async Task<ProduitBase?> ResolveProduitSouscriptionOuPrestationAsync(
            int souscriptionOuPrestationId,
            CancellationToken ct)
        {
            var produit = await ProduitSouscriptionRules.ResolveProduitFromSouscriptionAsync(
                _db, souscriptionOuPrestationId, ct);
            if (produit != null)
                return produit;

            return await ProduitSouscriptionRules.ResolveProduitFromPrestationAsync(
                _db, souscriptionOuPrestationId, ct);
        }

        private async Task<int> ResolveDeviseIdFromPrestationAsync(Prestation prestation, CancellationToken ct)
        {
            if (prestation.ProduitMutuelId.HasValue)
            {
                var pm = await _db.ProduitsMutuels.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.IdProduit == prestation.ProduitMutuelId, ct);
                if (pm != null) return pm.DeviseId;
            }

            if (prestation.ProduitAssureurId.HasValue)
            {
                var pa = await _db.ProduitsAssureurs.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.IdProduit == prestation.ProduitAssureurId, ct);
                if (pa != null) return pa.DeviseId;
            }

            return prestation.DeviseId;
        }
    }
}
