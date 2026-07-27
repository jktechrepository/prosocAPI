using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IDemandeRetraitAgentRepository
    {
        Task<List<DemandeRetraitAgent>> GetAllAsync(CancellationToken ct = default);
        Task<DemandeRetraitAgent?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<DemandeRetraitAgent>> GetByAgentIdAsync(int agentId, CancellationToken ct = default);
        Task<List<DemandeRetraitAgent>> GetByStatutAsync(string statut, CancellationToken ct = default);
        Task<DemandeRetraitAgent> CreateAsync(DemandeRetraitAgent entity, CancellationToken ct = default);
        Task<DemandeRetraitAgent?> UpdateAsync(int id, DemandeRetraitAgent entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        
        // Méthodes métier spécifiques
        Task<PeriodeRetraitVerificationDto> VerifierPeriodeRetraitAsync(DateTime date, CancellationToken ct = default);
        Task<PeriodeRetraitCouranteDto> GetPeriodeCouranteAsync(CancellationToken ct = default);
        Task<SoldeVerificationDto> VerifierSoldeDisponible(int agentId, decimal montantDemande, CancellationToken ct = default);
        Task<RetraitWorkflowResultDto> CreerDemandeRetraitAsync(DemandeRetraitAgentCreateDto createDto, CancellationToken ct = default);
        Task<RetraitWorkflowResultDto> ValiderEtGenererJetonAsync(int demandeId, int agentValidationId, CancellationToken ct = default);
        Task<RetraitPaiementResultDto> UtiliserJetonRetraitAsync(
            JetonRetraitUtilisationDto utilisationDto,
            int operateurUtilisateurId,
            CancellationToken ct = default);
        /// <summary>Expire les jetons périmés non utilisés (libération solde + rejet demande).</summary>
        Task<int> ExpireJetonsExpiresAsync(CancellationToken ct = default);
        Task<DemandeRetraitAgentStatsDto> GetStatsAsync(DateTime date, CancellationToken ct = default);
    }
}
