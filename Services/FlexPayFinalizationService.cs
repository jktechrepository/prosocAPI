using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.DTOs.FlexPay;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public interface IFlexPayFinalizationService
    {
        Task<Collecte> FinalizeCollecteAgentAsync(
            CollecteEnAttente enAttente,
            FlexPayCallbackDto callback,
            CancellationToken ct = default);

        Task<Collecte> FinalizePaiementAffilieAsync(
            CollecteEnAttente enAttente,
            FlexPayCallbackDto callback,
            CancellationToken ct = default);

        Task<AdhesionWithAffilieReadDto> FinalizeAdhesionWithAffilieAsync(
            CollecteEnAttente enAttente,
            FlexPayCallbackDto callback,
            CancellationToken ct = default);
    }

    public class FlexPayFinalizationService : IFlexPayFinalizationService
    {
        private readonly ProsocDbContext _db;
        private readonly ICollecteRepository _collecteRepository;
        private readonly IAdhesionWithAffilieExecutorService _adhesionExecutor;
        private readonly IPaiementHoldService _holdService;
        private readonly ICaisseService _caisseService;
        private readonly ILogger<FlexPayFinalizationService> _logger;

        public FlexPayFinalizationService(
            ProsocDbContext db,
            ICollecteRepository collecteRepository,
            IAdhesionWithAffilieExecutorService adhesionExecutor,
            IPaiementHoldService holdService,
            ICaisseService caisseService,
            ILogger<FlexPayFinalizationService> logger)
        {
            _db = db;
            _collecteRepository = collecteRepository;
            _adhesionExecutor = adhesionExecutor;
            _holdService = holdService;
            _caisseService = caisseService;
            _logger = logger;
        }

        public Task<Collecte> FinalizeCollecteAgentAsync(
            CollecteEnAttente enAttente,
            FlexPayCallbackDto callback,
            CancellationToken ct = default) =>
            FinalizeInternalAsync(enAttente, callback, ct);

        public Task<Collecte> FinalizePaiementAffilieAsync(
            CollecteEnAttente enAttente,
            FlexPayCallbackDto callback,
            CancellationToken ct = default) =>
            FinalizeInternalAsync(enAttente, callback, ct);

        public async Task<AdhesionWithAffilieReadDto> FinalizeAdhesionWithAffilieAsync(
            CollecteEnAttente enAttente,
            FlexPayCallbackDto callback,
            CancellationToken ct = default)
        {
            if (enAttente.IdAdhesionFinalisee.HasValue)
            {
                var existing = await _db.Adhesions.AsNoTracking()
                    .FirstOrDefaultAsync(a => a.IdAdhesion == enAttente.IdAdhesionFinalisee.Value, ct);
                if (existing != null)
                {
                    var affilie = await _db.Affilies.AsNoTracking()
                        .FirstAsync(a => a.IdAffilie == existing.AffilieId, ct);
                    return new AdhesionWithAffilieReadDto
                    {
                        Id = existing.IdAdhesion,
                        AffilieId = existing.AffilieId,
                        CodeAdhesion = affilie.CodeAdhesion ?? string.Empty
                    };
                }
            }

            var payload = JsonSerializer.Deserialize<AdhesionFlexPayPayload>(enAttente.PayloadMetierJson)
                ?? throw new InvalidOperationException("Payload adhésion FlexPay invalide.");

            var mode = MethodePaiementHelper.NormalizeForStorage(enAttente.MethodePaiement);
            // Parcours public (AllowAnonymous) : UtilisateurId peut être null.
            var utilisateurId = payload.UtilisateurId ?? enAttente.IdUtilisateur;

            var result = await _adhesionExecutor.ExecuteAsync(
                payload.Input,
                utilisateurId,
                callback,
                mode,
                ct);

            enAttente.StatutEnAttente = CollecteEnAttenteStatut.Finalise;
            enAttente.IdAdhesionFinalisee = result.Id;
            enAttente.IdCollecteFinalisee = result.Collectes.FirstOrDefault()?.IdCollecte;
            enAttente.DateModification = DateTime.UtcNow;

            var transaction = await _db.TransactionsFlexPay
                .FirstOrDefaultAsync(t => t.IdCollecteEnAttente == enAttente.IdCollecteEnAttente, ct);
            if (transaction != null)
                transaction.IdCollecte = enAttente.IdCollecteFinalisee;

            await _holdService.ReleaseHoldAsync(enAttente.IdCollecteEnAttente, ct);
            await _db.SaveChangesAsync(ct);

            foreach (var collecteDto in result.Collectes)
            {
                var collecte = await _db.Collectes
                    .FirstOrDefaultAsync(c => c.IdCollecte == collecteDto.IdCollecte, ct);
                if (collecte != null)
                    await _caisseService.TryEnregistrerEntreeCollecteGuichetAsync(collecte, ct);
            }

            _logger.LogInformation(
                "Adhésion {AdhesionId} finalisée via FlexPay (en attente {EnAttenteId})",
                result.Id, enAttente.IdCollecteEnAttente);

            return result;
        }

        private async Task<Collecte> FinalizeInternalAsync(
            CollecteEnAttente enAttente,
            FlexPayCallbackDto callback,
            CancellationToken ct)
        {
            if (enAttente.IdCollecteFinalisee.HasValue)
            {
                var existing = await _db.Collectes.FirstOrDefaultAsync(c => c.IdCollecte == enAttente.IdCollecteFinalisee.Value, ct);
                if (existing != null)
                    return existing;
            }

            var dto = System.Text.Json.JsonSerializer.Deserialize<CollecteCreateDto>(enAttente.PayloadMetierJson)
                ?? throw new InvalidOperationException("Payload métier collecte invalide.");

            var collecte = MapToCollecte(dto, enAttente, callback);
            var created = await _collecteRepository.CreateAsync(collecte, ct);

            enAttente.StatutEnAttente = CollecteEnAttenteStatut.Finalise;
            enAttente.IdCollecteFinalisee = created.IdCollecte;
            enAttente.DateModification = DateTime.UtcNow;

            var transaction = await _db.TransactionsFlexPay
                .FirstOrDefaultAsync(t => t.IdCollecteEnAttente == enAttente.IdCollecteEnAttente, ct);
            if (transaction != null)
                transaction.IdCollecte = created.IdCollecte;

            await _holdService.ReleaseHoldAsync(enAttente.IdCollecteEnAttente, ct);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Collecte {CollecteId} finalisée via FlexPay (en attente {EnAttenteId})",
                created.IdCollecte, enAttente.IdCollecteEnAttente);

            return created;
        }

        private static Collecte MapToCollecte(CollecteCreateDto dto, CollecteEnAttente enAttente, FlexPayCallbackDto callback)
        {
            return new Collecte
            {
                TypeCollecte = dto.TypeCollecte,
                FraisId = dto.FraisId,
                CotisationAffilieId = dto.CotisationAffilieId,
                AffilieId = dto.AffilieId,
                AgentId = dto.AgentId,
                OperateurUtilisateurId = enAttente.IdUtilisateur,
                Montant = dto.Montant,
                Mois = dto.Mois,
                Annee = dto.Annee,
                ReferencePaiement = callback.OrderNumber ?? enAttente.OrderNumberFlexPay,
                OrderNumberFlexPay = callback.OrderNumber ?? enAttente.OrderNumberFlexPay,
                ProviderReferenceFlexPay = callback.ProviderReference,
                ModePaiement = MethodePaiementHelper.NormalizeForStorage(dto.ModePaiement),
                Operateur = callback.Channel ?? dto.Operateur,
                StatutPaiement = CollecteStatutPaiement.Valide,
                SouscriptionPrestationId = dto.SouscriptionPrestationId,
                MontantRecu = dto.Montant,
                MontantAttendu = enAttente.MontantTarif,
                DeviseId = dto.DeviseId,
                Observation = dto.Observation,
                DateCollecte = DateTime.UtcNow,
                DateCreation = DateTime.UtcNow,
                Statut = true
            };
        }
    }
}
