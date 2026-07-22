using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.DTOs.FlexPay;

namespace ProsocAPI.Services
{
    public interface IFlexPayPaiementAffilieService
    {
        Task<InitiateFlexPayResponseDto> InitiateAsync(
            int affilieId,
            PayerSouscriptionDto dto,
            string? phone,
            CancellationToken ct = default);
    }

    public class FlexPayPaiementAffilieService : IFlexPayPaiementAffilieService
    {
        private readonly ProsocDbContext _db;
        private readonly IFlexPayCollecteService _flexPayCollecte;

        public FlexPayPaiementAffilieService(ProsocDbContext db, IFlexPayCollecteService flexPayCollecte)
        {
            _db = db;
            _flexPayCollecte = flexPayCollecte;
        }

        public async Task<InitiateFlexPayResponseDto> InitiateAsync(
            int affilieId,
            PayerSouscriptionDto dto,
            string? phone,
            CancellationToken ct = default)
        {
            MethodePaiementHelper.EnsureFlexPayOnly(dto.ModePaiement);

            var souscription = await _db.SouscriptionsPrestations
                .AsNoTracking()
                .FirstOrDefaultAsync(sp => sp.IdSouscriptionPrestation == dto.SouscriptionPrestationId, ct)
                ?? throw new ArgumentException("Souscription introuvable.");

            if (souscription.AffilieId != affilieId)
                throw new UnauthorizedAccessException("Cette souscription ne vous appartient pas.");

            var mois = dto.Mois > 0 ? dto.Mois : DateTime.UtcNow.Month;
            var annee = dto.Annee > 0 ? dto.Annee : DateTime.UtcNow.Year;

            var adhesion = await _db.Adhesions.AsNoTracking()
                .FirstOrDefaultAsync(a => a.AffilieId == affilieId, ct);

            var collecteDto = new CollecteCreateDto
            {
                TypeCollecte = TypeCollecte.Souscription,
                SouscriptionPrestationId = dto.SouscriptionPrestationId,
                AffilieId = affilieId,
                AgentId = adhesion?.AgentId ?? 0,
                Montant = dto.Montant,
                Mois = mois,
                Annee = annee,
                ReferencePaiement = dto.ReferencePaiement,
                ModePaiement = dto.ModePaiement,
                Operateur = "AUTO_PAIEMENT_AFFILIE_FLEXPAY",
                DeviseId = dto.DeviseId,
                Observation = dto.Observation,
                Phone = phone
            };

            return await _flexPayCollecte.InitiateAgentCollecteAsync(
                collecteDto, phone, CollecteEnAttenteSourceFlux.PaiementAffilie, ct);
        }
    }
}
