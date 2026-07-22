using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services
{
    public class TarifCotisationMetierService : ITarifCotisationMetierService, ICotisationAffilieMetierService
    {
        private const decimal MontantTolerance = 0.01m;

        private readonly ProsocDbContext _db;

        public TarifCotisationMetierService(ProsocDbContext db)
        {
            _db = db;
        }

        public int CompterPersonnesAssurees(int nombreDependants)
        {
            if (nombreDependants < 0)
                throw new ArgumentException("Le nombre de personnes à charge ne peut pas être négatif.");

            return 1 + nombreDependants;
        }

        public async Task<TarifCotisationMontantCalculDto> CalculerMontantTotalAsync(
            int cotisationAffilieId,
            int nombreDependants,
            CancellationToken ct = default)
        {
            var cotisation = await LoadCotisationAsync(cotisationAffilieId, ct);
            var nombrePersonnes = CompterPersonnesAssurees(nombreDependants);

            return new TarifCotisationMontantCalculDto
            {
                CotisationAffilieId = cotisation.IdCotisationAffilie,
                TypeAdhesionId = cotisation.TypeAdhesionId,
                TypeAdhesionLibelle = cotisation.TypeAdhesion.Libelle,
                Periodicite = cotisation.Periodicite,
                MontantUnitaire = cotisation.Montant,
                NombreDependants = nombreDependants,
                NombrePersonnes = nombrePersonnes,
                MontantTotal = cotisation.Montant * nombrePersonnes,
                DeviseId = cotisation.DeviseId,
                DeviseCode = cotisation.Devise?.Code ?? string.Empty
            };
        }

        public async Task ValidateCollecteCotisationAsync(
            int cotisationAffilieId,
            int typeAdhesionId,
            decimal montantCollecte,
            int nombreDependants,
            CancellationToken ct = default)
        {
            var calcul = await CalculerMontantTotalAsync(cotisationAffilieId, nombreDependants, ct);
            await ValidateCollecteCotisationStructureAsync(cotisationAffilieId, typeAdhesionId, nombreDependants, ct);

            if (montantCollecte <= 0)
                throw new ArgumentException("Le montant de la collecte cotisation doit être supérieur à zéro.");

            if (Math.Abs(montantCollecte - calcul.MontantTotal) > MontantTolerance)
            {
                throw new ArgumentException(
                    $"Montant de cotisation invalide. Attendu : {calcul.MontantTotal:F2} " +
                    $"({calcul.MontantUnitaire:F2} × {calcul.NombrePersonnes} personne(s)), " +
                    $"reçu : {montantCollecte:F2}.");
            }
        }

        public async Task ValidateCollecteCotisationStructureAsync(
            int cotisationAffilieId,
            int typeAdhesionId,
            int nombreDependants,
            CancellationToken ct = default)
        {
            var cotisation = await LoadCotisationAsync(cotisationAffilieId, ct);

            if (!cotisation.Statut)
                throw new ArgumentException($"La cotisation affilié {cotisationAffilieId} est inactive.");

            if (cotisation.TypeAdhesionId != typeAdhesionId)
            {
                throw new ArgumentException(
                    $"La cotisation affilié {cotisationAffilieId} (type {cotisation.TypeAdhesion.Libelle}) " +
                    $"n'est pas compatible avec le type d'adhésion demandé (id {typeAdhesionId}).");
            }

            if (nombreDependants > cotisation.TypeAdhesion.MaxDependants)
            {
                throw new ArgumentException(
                    $"Le nombre de personnes à charge ({nombreDependants}) dépasse le maximum autorisé " +
                    $"({cotisation.TypeAdhesion.MaxDependants}) pour le type d'adhésion {cotisation.TypeAdhesion.Libelle}.");
            }
        }

        private async Task<Models.Core.TarifCotisation> LoadCotisationAsync(int cotisationAffilieId, CancellationToken ct)
        {
            var cotisation = await _db.CotisationsAffilie
                .AsNoTracking()
                .Include(c => c.TypeAdhesion)
                .Include(c => c.Devise)
                .FirstOrDefaultAsync(c => c.IdCotisationAffilie == cotisationAffilieId, ct);

            if (cotisation == null)
                throw new ArgumentException($"Cotisation affilié avec ID {cotisationAffilieId} introuvable.");

            return cotisation;
        }
    }

    [Obsolete("Use TarifCotisationMetierService instead.")]
    public class CotisationAffilieMetierService : TarifCotisationMetierService, ICotisationAffilieMetierService
    {
        public CotisationAffilieMetierService(ProsocDbContext db) : base(db)
        {
        }
    }
}
