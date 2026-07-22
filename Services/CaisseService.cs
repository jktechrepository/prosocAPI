using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class CaisseService : ICaisseService
    {
        private readonly ProsocDbContext _db;
        private readonly IDeviseConversionService _deviseConversionService;
        private readonly IPaginationService _paginationService;
        private readonly ILogger<CaisseService> _logger;

        public CaisseService(
            ProsocDbContext db,
            IDeviseConversionService deviseConversionService,
            IPaginationService paginationService,
            ILogger<CaisseService> logger)
        {
            _db = db;
            _deviseConversionService = deviseConversionService;
            _paginationService = paginationService;
            _logger = logger;
        }

        public async Task<SessionCaisseReadDto> OuvrirSessionAsync(
            int utilisateurId,
            SessionCaisseOuvrirDto dto,
            CancellationToken ct = default)
        {
            var sessionOuverte = await GetSessionOuverteTrackedAsync(utilisateurId, ct);
            if (sessionOuverte != null)
                throw new ArgumentException("Une session de caisse est déjà ouverte pour cet utilisateur.");

            var devisePrincipale = await _deviseConversionService.GetDevisePrincipaleAsync(ct);
            var session = new SessionCaisse
            {
                UtilisateurId = utilisateurId,
                SoldeOuverture = dto.SoldeOuverture,
                DeviseId = devisePrincipale.IdDevise,
                Statut = SessionCaisseStatut.Ouverte,
                DateOuverture = DateTime.Now,
                DateCreation = DateTime.Now
            };

            _db.SessionsCaisses.Add(session);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Session caisse {SessionId} ouverte pour utilisateur {UtilisateurId}", session.IdSessionCaisse, utilisateurId);
            return await MapSessionReadAsync(session, ct);
        }

        public async Task<SessionCaisseReadDto> CloturerSessionAsync(
            int utilisateurId,
            int sessionId,
            SessionCaisseCloturerDto dto,
            CancellationToken ct = default)
        {
            var session = await _db.SessionsCaisses
                .FirstOrDefaultAsync(s => s.IdSessionCaisse == sessionId && s.UtilisateurId == utilisateurId, ct);

            if (session == null)
                throw new ArgumentException("Session de caisse introuvable.");

            if (session.Statut == SessionCaisseStatut.Cloturee)
                throw new ArgumentException("Cette session de caisse est déjà clôturée.");

            var soldeTheorique = await CalculerSoldeSessionAsync(sessionId, ct);
            session.SoldeTheoriqueCloture = soldeTheorique;
            session.SoldeReelCloture = dto.SoldeReelCloture;
            session.ObservationCloture = dto.ObservationCloture;
            session.Statut = SessionCaisseStatut.Cloturee;
            session.DateCloture = DateTime.Now;
            session.DateModification = DateTime.Now;

            await _db.SaveChangesAsync(ct);
            return await MapSessionReadAsync(session, ct);
        }

        public async Task<SessionCaisseReadDto?> GetSessionCouranteAsync(int utilisateurId, CancellationToken ct = default)
        {
            var session = await _db.SessionsCaisses
                .AsNoTracking()
                .Include(s => s.Devise)
                .FirstOrDefaultAsync(s => s.UtilisateurId == utilisateurId && s.Statut == SessionCaisseStatut.Ouverte, ct);

            return session == null ? null : await MapSessionReadAsync(session, ct);
        }

        public async Task<SessionCaisseSoldeDto?> GetSoldeSessionAsync(
            int utilisateurId,
            int sessionId,
            CancellationToken ct = default)
        {
            var session = await _db.SessionsCaisses
                .AsNoTracking()
                .Include(s => s.Devise)
                .FirstOrDefaultAsync(s => s.IdSessionCaisse == sessionId && s.UtilisateurId == utilisateurId, ct);

            if (session == null)
                return null;

            return await BuildSoldeDtoAsync(session, ct);
        }

        public async Task<PaginatedResponse<MouvementCaisseReadDto>> GetMouvementsAsync(
            int utilisateurId,
            int sessionId,
            PaginationRequest request,
            CancellationToken ct = default)
        {
            var sessionExists = await _db.SessionsCaisses
                .AnyAsync(s => s.IdSessionCaisse == sessionId && s.UtilisateurId == utilisateurId, ct);

            if (!sessionExists)
                throw new ArgumentException("Session de caisse introuvable.");

            var query = _db.MouvementsCaisses
                .AsNoTracking()
                .Include(m => m.Devise)
                .Where(m => m.SessionCaisseId == sessionId)
                .OrderByDescending(m => m.DateOperation);

            var paginated = await _paginationService.CreatePaginatedResponseAsync(query, request, ct);
            var dtos = paginated.Data.Select(MapMouvement).ToList();

            return new PaginatedResponse<MouvementCaisseReadDto>
            {
                Data = dtos,
                CurrentPage = paginated.CurrentPage,
                PageSize = paginated.PageSize,
                TotalItems = paginated.TotalItems,
                TotalPages = paginated.TotalPages,
                HasNextPage = paginated.HasNextPage,
                HasPreviousPage = paginated.HasPreviousPage
            };
        }

        public async Task<PaginatedResponse<SessionCaisseReadDto>> GetSessionsAsync(
            int utilisateurId,
            DateTime? dateDebut,
            DateTime? dateFin,
            string? statut,
            PaginationRequest request,
            CancellationToken ct = default)
        {
            var query = _db.SessionsCaisses
                .AsNoTracking()
                .Include(s => s.Devise)
                .Where(s => s.UtilisateurId == utilisateurId);

            if (dateDebut.HasValue)
                query = query.Where(s => s.DateOuverture >= dateDebut.Value);
            if (dateFin.HasValue)
                query = query.Where(s => s.DateOuverture <= dateFin.Value);
            if (!string.IsNullOrWhiteSpace(statut))
            {
                var statutNorm = statut.Trim().ToUpperInvariant();
                query = query.Where(s => s.Statut != null && s.Statut.ToUpper() == statutNorm);
            }

            query = query.OrderByDescending(s => s.DateOuverture);

            var paginated = await _paginationService.CreatePaginatedResponseAsync(query, request, ct);
            var dtos = new List<SessionCaisseReadDto>();
            foreach (var session in paginated.Data)
                dtos.Add(await MapSessionReadAsync(session, ct));

            return new PaginatedResponse<SessionCaisseReadDto>
            {
                Data = dtos,
                CurrentPage = paginated.CurrentPage,
                PageSize = paginated.PageSize,
                TotalItems = paginated.TotalItems,
                TotalPages = paginated.TotalPages,
                HasNextPage = paginated.HasNextPage,
                HasPreviousPage = paginated.HasPreviousPage
            };
        }

        public async Task<SessionCaisse?> ResolveSessionPourOperationAsync(
            int utilisateurId,
            int? sessionCaisseId,
            bool skipSessionCheck,
            CancellationToken ct = default)
        {
            if (skipSessionCheck)
                return null;

            if (sessionCaisseId.HasValue)
            {
                var session = await _db.SessionsCaisses
                    .FirstOrDefaultAsync(s =>
                        s.IdSessionCaisse == sessionCaisseId.Value
                        && s.UtilisateurId == utilisateurId
                        && s.Statut == SessionCaisseStatut.Ouverte, ct);

                if (session == null)
                    throw new InvalidOperationException("SESSION_CAISSIER_REQUISE");

                return session;
            }

            var sessionCourante = await GetSessionOuverteTrackedAsync(utilisateurId, ct);
            if (sessionCourante == null)
                throw new InvalidOperationException("SESSION_CAISSIER_REQUISE");

            return sessionCourante;
        }

        public async Task<decimal> CalculerSoldeSessionAsync(int sessionId, CancellationToken ct = default)
        {
            var session = await _db.SessionsCaisses.AsNoTracking()
                .FirstOrDefaultAsync(s => s.IdSessionCaisse == sessionId, ct);

            if (session == null)
                return 0;

            var mouvements = await _db.MouvementsCaisses
                .AsNoTracking()
                .Where(m => m.SessionCaisseId == sessionId && m.Statut)
                .ToListAsync(ct);

            var entrees = mouvements
                .Where(m => m.TypeOperation == MouvementCaisseTypes.Entree)
                .Sum(m => m.Montant);

            var sorties = mouvements
                .Where(m => m.TypeOperation == MouvementCaisseTypes.Sortie)
                .Sum(m => m.Montant);

            return session.SoldeOuverture + entrees - sorties;
        }

        public async Task<bool> TryEnregistrerEntreeCollecteGuichetAsync(Collecte collecte, CancellationToken ct = default)
        {
            if (!collecte.OperateurUtilisateurId.HasValue)
                return false;

            if (!CollecteStatutPaiementRegles.EstValide(collecte.StatutPaiement))
                return false;

            if (!MethodePaiementHelper.IsEntreeCaisseEligible(collecte.ModePaiement))
                return false;

            if (await _db.MouvementsCaisses.AnyAsync(
                    m => m.CollecteId == collecte.IdCollecte && m.Statut, ct))
            {
                return true;
            }

            var session = await GetSessionOuverteTrackedAsync(collecte.OperateurUtilisateurId.Value, ct);
            if (session == null)
            {
                _logger.LogWarning(
                    "Collecte {CollecteId} ({ModePaiement}) sans session caisse ouverte pour utilisateur {UtilisateurId}",
                    collecte.IdCollecte,
                    collecte.ModePaiement,
                    collecte.OperateurUtilisateurId);
                return false;
            }

            var source = MethodePaiementHelper.ResolveMouvementCaisseSource(collecte.ModePaiement);
            var montant = collecte.MontantDevisePrincipale ?? collecte.Montant;
            var mouvement = new MouvementCaisse
            {
                SessionCaisseId = session.IdSessionCaisse,
                UtilisateurId = collecte.OperateurUtilisateurId.Value,
                TypeOperation = MouvementCaisseTypes.Entree,
                Source = source,
                Montant = montant,
                DeviseId = session.DeviseId,
                CollecteId = collecte.IdCollecte,
                Description = source == MouvementCaisseSources.CollecteElectronique
                    ? $"Encaissement électronique collecte #{collecte.IdCollecte}"
                    : $"Encaissement espèces collecte #{collecte.IdCollecte}",
                DateOperation = DateTime.Now,
                DateCreation = DateTime.Now,
                Statut = true
            };

            _db.MouvementsCaisses.Add(mouvement);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public MouvementCaisse BuildMouvementSortieRetrait(
            SessionCaisse session,
            int utilisateurId,
            JetonRetrait jeton,
            DemandeRetraitAgent demande,
            WalletMouvement walletMouvement) =>
            CreerMouvementSortieRetrait(session, utilisateurId, jeton, demande, walletMouvement);

        internal MouvementCaisse CreerMouvementSortieRetrait(
            SessionCaisse session,
            int utilisateurId,
            JetonRetrait jeton,
            DemandeRetraitAgent demande,
            WalletMouvement walletMouvement)
        {
            return new MouvementCaisse
            {
                SessionCaisseId = session.IdSessionCaisse,
                UtilisateurId = utilisateurId,
                TypeOperation = MouvementCaisseTypes.Sortie,
                Source = MouvementCaisseSources.RetraitAgent,
                Montant = jeton.MontantRetrait,
                DeviseId = session.DeviseId,
                DemandeRetraitId = demande.IdDemande,
                JetonRetraitId = jeton.IdJeton,
                WalletMouvementId = walletMouvement.IdWalletMouvement,
                Description = $"Paiement retrait agent — jeton {jeton.CodeJeton}",
                DateOperation = DateTime.Now,
                DateCreation = DateTime.Now,
                Statut = true
            };
        }

        private async Task<SessionCaisse?> GetSessionOuverteTrackedAsync(int utilisateurId, CancellationToken ct) =>
            await _db.SessionsCaisses
                .FirstOrDefaultAsync(s => s.UtilisateurId == utilisateurId && s.Statut == SessionCaisseStatut.Ouverte, ct);

        private async Task<SessionCaisseReadDto> MapSessionReadAsync(SessionCaisse session, CancellationToken ct)
        {
            var devise = session.Devise ?? await _db.Devises.AsNoTracking()
                .FirstAsync(d => d.IdDevise == session.DeviseId, ct);

            var soldeCourant = await CalculerSoldeSessionAsync(session.IdSessionCaisse, ct);
            return new SessionCaisseReadDto
            {
                IdSessionCaisse = session.IdSessionCaisse,
                UtilisateurId = session.UtilisateurId,
                SoldeOuverture = session.SoldeOuverture,
                SoldeCourant = soldeCourant,
                DeviseId = session.DeviseId,
                DeviseCode = devise.Code,
                Statut = session.Statut,
                DateOuverture = session.DateOuverture,
                DateCloture = session.DateCloture,
                SoldeTheoriqueCloture = session.SoldeTheoriqueCloture,
                SoldeReelCloture = session.SoldeReelCloture,
                ObservationCloture = session.ObservationCloture
            };
        }

        private async Task<SessionCaisseSoldeDto> BuildSoldeDtoAsync(SessionCaisse session, CancellationToken ct)
        {
            var mouvements = await _db.MouvementsCaisses
                .AsNoTracking()
                .Where(m => m.SessionCaisseId == session.IdSessionCaisse && m.Statut)
                .ToListAsync(ct);

            var entrees = mouvements
                .Where(m => m.TypeOperation == MouvementCaisseTypes.Entree)
                .Sum(m => m.Montant);

            var sorties = mouvements
                .Where(m => m.TypeOperation == MouvementCaisseTypes.Sortie)
                .Sum(m => m.Montant);

            return new SessionCaisseSoldeDto
            {
                IdSessionCaisse = session.IdSessionCaisse,
                SoldeOuverture = session.SoldeOuverture,
                TotalEntrees = entrees,
                TotalSorties = sorties,
                SoldeCourant = session.SoldeOuverture + entrees - sorties,
                DeviseCode = session.Devise?.Code
            };
        }

        private static MouvementCaisseReadDto MapMouvement(MouvementCaisse m) => new()
        {
            IdMouvementCaisse = m.IdMouvementCaisse,
            SessionCaisseId = m.SessionCaisseId,
            TypeOperation = m.TypeOperation,
            Source = m.Source,
            Montant = m.Montant,
            DeviseId = m.DeviseId,
            DeviseCode = m.Devise?.Code,
            DateOperation = m.DateOperation,
            CollecteId = m.CollecteId,
            DemandeRetraitId = m.DemandeRetraitId,
            JetonRetraitId = m.JetonRetraitId,
            Description = m.Description
        };
    }
}
