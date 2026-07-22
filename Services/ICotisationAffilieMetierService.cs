using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services
{
    public interface ITarifCotisationMetierService
    {
        /// <summary>Nombre de personnes assurées = titulaire (1) + personnes à charge.</summary>
        int CompterPersonnesAssurees(int nombreDependants);

        Task<TarifCotisationMontantCalculDto> CalculerMontantTotalAsync(
            int cotisationAffilieId,
            int nombreDependants,
            CancellationToken ct = default);

        Task ValidateCollecteCotisationAsync(
            int cotisationAffilieId,
            int typeAdhesionId,
            decimal montantCollecte,
            int nombreDependants,
            CancellationToken ct = default);

        /// <summary>Validation structurelle sans contrôle de montant (cross-devise géré ailleurs).</summary>
        Task ValidateCollecteCotisationStructureAsync(
            int cotisationAffilieId,
            int typeAdhesionId,
            int nombreDependants,
            CancellationToken ct = default);
    }

    [Obsolete("Use ITarifCotisationMetierService instead.")]
    public interface ICotisationAffilieMetierService : ITarifCotisationMetierService
    {
    }
}
