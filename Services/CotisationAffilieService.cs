using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class TarifCotisationService : ITarifCotisationRepository, ICotisationAffilieRepository
    {
        private static readonly string[] PeriodicitesValides = { "Mensuel", "Trimestriel", "Semestriel", "Annuel" };

        private readonly ProsocDbContext _db;

        public TarifCotisationService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<List<TarifCotisation>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.CotisationsAffilie
                .AsNoTracking()
                .Include(c => c.TypeAdhesion)
                .Include(c => c.Devise)
                .OrderBy(c => c.TypeAdhesionId)
                .ThenBy(c => c.Periodicite)
                .ToListAsync(ct);
        }

        public async Task<TarifCotisation?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.CotisationsAffilie
                .AsNoTracking()
                .Include(c => c.TypeAdhesion)
                .Include(c => c.Devise)
                .FirstOrDefaultAsync(c => c.IdCotisationAffilie == id, ct);
        }

        public async Task<List<TarifCotisation>> GetByTypeAdhesionIdAsync(int typeAdhesionId, CancellationToken ct = default)
        {
            return await _db.CotisationsAffilie
                .AsNoTracking()
                .Include(c => c.TypeAdhesion)
                .Include(c => c.Devise)
                .Where(c => c.TypeAdhesionId == typeAdhesionId)
                .OrderBy(c => c.Periodicite)
                .ToListAsync(ct);
        }

        public async Task<List<TarifCotisation>> GetByAffilieIdAsync(int affilieId, CancellationToken ct = default)
        {
            var affilieExists = await _db.Affilies.AnyAsync(a => a.IdAffilie == affilieId, ct);
            if (!affilieExists)
                throw new KeyNotFoundException($"Affilié avec ID {affilieId} introuvable.");

            var typeAdhesionId = await _db.Adhesions
                .AsNoTracking()
                .Where(a => a.AffilieId == affilieId && a.Statut)
                .OrderByDescending(a => a.DateCreation)
                .Select(a => (int?)a.TypeAdhesionId)
                .FirstOrDefaultAsync(ct);

            if (!typeAdhesionId.HasValue)
                return new List<TarifCotisation>();

            return await _db.CotisationsAffilie
                .AsNoTracking()
                .Include(c => c.TypeAdhesion)
                .Include(c => c.Devise)
                .Where(c => c.TypeAdhesionId == typeAdhesionId.Value && c.Statut)
                .OrderBy(c => c.Periodicite)
                .ToListAsync(ct);
        }

        public async Task<TarifCotisation> CreateAsync(TarifCotisation entity, CancellationToken ct = default)
        {
            entity.Periodicite = NormalizePeriodicite(entity.Periodicite);
            entity.LibelleTarifCotisation = NormalizeLibelleForStorage(entity.LibelleTarifCotisation);
            entity.LibelleTarifCotisationNormalized = BuildNormalizedUniqueKey(entity.LibelleTarifCotisation, entity.Statut);
            await EnsureTypeAdhesionExistsAsync(entity.TypeAdhesionId, ct);
            await EnsureDeviseExistsAsync(entity.DeviseId, ct);
            await EnsureUniqueTarifAsync(entity.TypeAdhesionId, entity.Periodicite, excludeId: null, ct);
            await EnsureUniqueActiveLibelleAsync(entity.LibelleTarifCotisationNormalized, excludeId: null, ct);

            entity.DateCreation = DateTime.Now;
            _db.CotisationsAffilie.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<TarifCotisation?> UpdateAsync(int id, TarifCotisation entity, CancellationToken ct = default)
        {
            var existing = await _db.CotisationsAffilie.FirstOrDefaultAsync(c => c.IdCotisationAffilie == id, ct);
            if (existing == null)
                return null;

            entity.Periodicite = NormalizePeriodicite(entity.Periodicite);
            entity.LibelleTarifCotisation = NormalizeLibelleForStorage(entity.LibelleTarifCotisation);
            entity.LibelleTarifCotisationNormalized = BuildNormalizedUniqueKey(entity.LibelleTarifCotisation, entity.Statut);
            await EnsureTypeAdhesionExistsAsync(entity.TypeAdhesionId, ct);
            await EnsureDeviseExistsAsync(entity.DeviseId, ct);
            await EnsureUniqueTarifAsync(entity.TypeAdhesionId, entity.Periodicite, excludeId: id, ct);
            await EnsureUniqueActiveLibelleAsync(entity.LibelleTarifCotisationNormalized, excludeId: id, ct);
            await EnsureNoBlockingArrieresForTarifChangeAsync(existing, entity, ct);

            existing.Montant = entity.Montant;
            existing.Periodicite = entity.Periodicite;
            existing.TypeAdhesionId = entity.TypeAdhesionId;
            existing.DeviseId = entity.DeviseId;
            existing.LibelleTarifCotisation = entity.LibelleTarifCotisation;
            existing.LibelleTarifCotisationNormalized = entity.LibelleTarifCotisationNormalized;
            existing.Statut = entity.Statut;
            existing.DateModification = DateTime.Now;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.CotisationsAffilie.FirstOrDefaultAsync(c => c.IdCotisationAffilie == id, ct);
            if (existing == null)
                return false;

            _db.CotisationsAffilie.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public static string NormalizePeriodicite(string periodicite)
        {
            if (string.IsNullOrWhiteSpace(periodicite))
                throw new ArgumentException("La périodicité est obligatoire.");

            var normalized = periodicite.Trim();
            if (normalized.Equals("mensuel", StringComparison.OrdinalIgnoreCase))
                return "Mensuel";
            if (normalized.Equals("trimestriel", StringComparison.OrdinalIgnoreCase))
                return "Trimestriel";
            if (normalized.Equals("semestriel", StringComparison.OrdinalIgnoreCase))
                return "Semestriel";
            if (normalized.Equals("annuel", StringComparison.OrdinalIgnoreCase))
                return "Annuel";

            throw new ArgumentException(
                $"Périodicité invalide : '{periodicite}'. Valeurs acceptées : {string.Join(", ", PeriodicitesValides)}.");
        }

        private async Task EnsureTypeAdhesionExistsAsync(int typeAdhesionId, CancellationToken ct)
        {
            var exists = await _db.TypeAdhesions.AnyAsync(t => t.IdTypeAdhesion == typeAdhesionId, ct);
            if (!exists)
                throw new ArgumentException($"TypeAdhesion avec ID {typeAdhesionId} introuvable.");
        }

        private async Task EnsureDeviseExistsAsync(int deviseId, CancellationToken ct)
        {
            var exists = await _db.Devises.AnyAsync(d => d.IdDevise == deviseId && d.Statut, ct);
            if (!exists)
                throw new ArgumentException($"Devise avec ID {deviseId} introuvable ou inactive.");
        }

        private async Task EnsureUniqueTarifAsync(int typeAdhesionId, string periodicite, int? excludeId, CancellationToken ct)
        {
            var duplicate = await _db.CotisationsAffilie.AnyAsync(c =>
                c.TypeAdhesionId == typeAdhesionId
                && c.Periodicite == periodicite
                && (excludeId == null || c.IdCotisationAffilie != excludeId), ct);

            if (duplicate)
                throw new InvalidOperationException(
                    $"Une cotisation {periodicite} existe déjà pour le type d'adhésion {typeAdhesionId}.");
        }

        private async Task EnsureUniqueActiveLibelleAsync(string? normalizedLibelle, int? excludeId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(normalizedLibelle))
                return;

            var duplicate = await _db.CotisationsAffilie.AnyAsync(c =>
                c.LibelleTarifCotisationNormalized == normalizedLibelle
                && (excludeId == null || c.IdCotisationAffilie != excludeId), ct);

            if (duplicate)
            {
                throw new InvalidOperationException(
                    $"Le libellé de tarif de cotisation '{normalizedLibelle}' existe déjà pour un tarif actif.");
            }
        }

        private static string? NormalizeLibelleForStorage(string? libelle)
        {
            if (string.IsNullOrWhiteSpace(libelle))
                return null;
            return libelle.Trim();
        }

        private static string? BuildNormalizedUniqueKey(string? libelle, bool statut)
        {
            if (!statut || string.IsNullOrWhiteSpace(libelle))
                return null;
            return libelle.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Empêche la modification d'un tarif lorsqu'il existe déjà des arriérés ouverts basés sur ce tarif.
        /// Cela évite de changer rétroactivement la base de calcul des dettes en cours.
        /// </summary>
        private async Task EnsureNoBlockingArrieresForTarifChangeAsync(
            TarifCotisation existing,
            TarifCotisation incoming,
            CancellationToken ct)
        {
            var changeImpactingTarif =
                existing.Montant != incoming.Montant
                || !string.Equals(existing.Periodicite, incoming.Periodicite, StringComparison.OrdinalIgnoreCase)
                || existing.TypeAdhesionId != incoming.TypeAdhesionId
                || existing.DeviseId != incoming.DeviseId;

            if (!changeImpactingTarif)
                return;

            var hasOpenArrieres = await _db.ArrieresAffilie.AnyAsync(a =>
                a.CotisationAffilieId == existing.IdCotisationAffilie
                && a.Statut
                && a.RestAPayer > 0, ct);

            if (hasOpenArrieres)
            {
                throw new InvalidOperationException(
                    "Modification bloquée : des arriérés non soldés existent pour ce tarif de cotisation. " +
                    "Créez un nouveau tarif (ou désactivez l'ancien) pour préserver l'historique.");
            }
        }
    }

    [Obsolete("Use TarifCotisationService instead.")]
    public class CotisationAffilieService : TarifCotisationService, ICotisationAffilieRepository
    {
        public CotisationAffilieService(ProsocDbContext db) : base(db)
        {
        }
    }
}
