using Microsoft.EntityFrameworkCore;
using Prosoc.Data;

namespace ProsocAPI.Services
{
    public interface ITypeAdhesionDependantsValidationService
    {
        Task ValidateDependantsCountAsync(int typeAdhesionId, int nombreDependants, CancellationToken ct = default);
    }

    public class TypeAdhesionDependantsValidationService : ITypeAdhesionDependantsValidationService
    {
        private readonly ProsocDbContext _db;

        public TypeAdhesionDependantsValidationService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task ValidateDependantsCountAsync(
            int typeAdhesionId,
            int nombreDependants,
            CancellationToken ct = default)
        {
            var typeAdhesion = await _db.TypeAdhesions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.IdTypeAdhesion == typeAdhesionId, ct);

            if (typeAdhesion == null)
                throw new ArgumentException($"TypeAdhesion avec ID {typeAdhesionId} introuvable.");

            if (nombreDependants > typeAdhesion.MaxDependants)
            {
                throw new ArgumentException(
                    $"Le nombre de personnes à charge ({nombreDependants}) dépasse le maximum autorisé " +
                    $"({typeAdhesion.MaxDependants}) pour le type d'adhésion {typeAdhesion.Libelle}.");
            }
        }
    }
}
