using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services
{
    public interface IDeviseConversionService
    {
        Task<Devise> GetDevisePrincipaleAsync(CancellationToken ct = default);
        Task<(decimal montantConverti, decimal taux)> ConvertirAsync(
            decimal montant,
            int deviseSourceId,
            int deviseCibleId,
            DateTime dateReference,
            CancellationToken ct = default);
        Task<PreviewConversionDto> PreviewConversionAsync(
            string codeDeviseSource,
            decimal montant,
            string? codeDeviseCible,
            DateTime datePaiement,
            CancellationToken ct = default);
        Task<TauxChangeDevise?> GetTauxActifAsync(
            int deviseSourceId,
            int deviseCibleId,
            DateTime dateReference,
            CancellationToken ct = default);
    }

    public interface ICollecteMultideviseService
    {
        Task<(decimal montantAttendu, int deviseTarifId)> ResolveTarifAttenduAsync(
            Collecte collecte,
            int nombreDependants,
            CancellationToken ct = default);
        Task ValidateAndApplySnapshotAsync(
            Collecte collecte,
            int nombreDependants,
            CancellationToken ct = default,
            DateTime? dateConversion = null);
    }
}
