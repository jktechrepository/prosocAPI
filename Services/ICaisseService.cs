using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;

namespace ProsocAPI.Services
{
    public interface ICaisseService
    {
        Task<SessionCaisseReadDto> OuvrirSessionAsync(int utilisateurId, SessionCaisseOuvrirDto dto, CancellationToken ct = default);
        Task<SessionCaisseReadDto> CloturerSessionAsync(int utilisateurId, int sessionId, SessionCaisseCloturerDto dto, CancellationToken ct = default);
        Task<SessionCaisseReadDto?> GetSessionCouranteAsync(int utilisateurId, CancellationToken ct = default);
        Task<SessionCaisseSoldeDto?> GetSoldeSessionAsync(int utilisateurId, int sessionId, CancellationToken ct = default);
        Task<PaginatedResponse<MouvementCaisseReadDto>> GetMouvementsAsync(
            int utilisateurId,
            int sessionId,
            PaginationRequest request,
            CancellationToken ct = default);

        Task<PaginatedResponse<SessionCaisseReadDto>> GetSessionsAsync(
            int utilisateurId,
            DateTime? dateDebut,
            DateTime? dateFin,
            string? statut,
            PaginationRequest request,
            CancellationToken ct = default);

        Task<SessionCaisse?> ResolveSessionPourOperationAsync(
            int utilisateurId,
            int? sessionCaisseId,
            bool skipSessionCheck,
            CancellationToken ct = default);

        Task<decimal> CalculerSoldeSessionAsync(int sessionId, CancellationToken ct = default);

        Task<bool> TryEnregistrerEntreeCollecteGuichetAsync(Collecte collecte, CancellationToken ct = default);

        MouvementCaisse BuildMouvementSortieRetrait(
            SessionCaisse session,
            int utilisateurId,
            JetonRetrait jeton,
            DemandeRetraitAgent demande,
            WalletMouvement walletMouvement);
    }
}
