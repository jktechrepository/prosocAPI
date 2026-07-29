using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Extensions;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;

namespace ProsocAPI.Services
{
    public class DemandeRechargeWalletVirtuelService : IDemandeRechargeWalletVirtuelService
    {
        private readonly ProsocDbContext _db;
        private readonly IParametresMetierProvider _parametresMetierProvider;
        private readonly IWalletVirtuelMouvementService _walletMouvementService;
        private readonly IPaginationService _paginationService;
        private readonly ILogger<DemandeRechargeWalletVirtuelService> _logger;

        public DemandeRechargeWalletVirtuelService(
            ProsocDbContext db,
            IParametresMetierProvider parametresMetierProvider,
            IWalletVirtuelMouvementService walletMouvementService,
            IPaginationService paginationService,
            ILogger<DemandeRechargeWalletVirtuelService> logger)
        {
            _db = db;
            _parametresMetierProvider = parametresMetierProvider;
            _walletMouvementService = walletMouvementService;
            _paginationService = paginationService;
            _logger = logger;
        }

        public async Task<PaginatedResponse<DemandeRechargeWalletVirtuelReadDto>> GetAllAsync(
            PaginationRequest request,
            string? statutDemande = null,
            CancellationToken ct = default)
        {
            var query = BaseQuery();
            if (!string.IsNullOrWhiteSpace(statutDemande))
                query = query.Where(d => d.StatutDemande == statutDemande.Trim().ToUpperInvariant());

            if (string.IsNullOrEmpty(request.SortBy))
                query = query.OrderByDescending(d => d.DateDemande);

            var result = await _paginationService.CreatePaginatedResponseAsync(query, request, ct);
            return new PaginatedResponse<DemandeRechargeWalletVirtuelReadDto>
            {
                Data = result.Data.Select(MapToReadDto).ToList(),
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage
            };
        }

        public async Task<List<DemandeRechargeWalletVirtuelReadDto>> GetEnAttenteAsync(CancellationToken ct = default)
        {
            var demandes = await BaseQuery()
                .Where(d => d.StatutDemande == DemandeRechargeWalletVirtuelStatuts.EnAttente && d.Statut)
                .OrderBy(d => d.DateDemande)
                .ToListAsync(ct);

            return demandes.Select(MapToReadDto).ToList();
        }

        public async Task<DemandeRechargeWalletVirtuelReadDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var demande = await BaseQuery().FirstOrDefaultAsync(d => d.IdDemande == id, ct);
            return demande == null ? null : MapToReadDto(demande);
        }

        public async Task<List<DemandeRechargeWalletVirtuelReadDto>> GetByAgentAsync(
            int agentId,
            CancellationToken ct = default)
        {
            var demandes = await BaseQuery()
                .Where(d => d.AgentId == agentId)
                .OrderByDescending(d => d.DateDemande)
                .ToListAsync(ct);

            return demandes.Select(MapToReadDto).ToList();
        }

        public async Task<DemandeRechargeWalletVirtuelOperationResultDto> CreerAsync(
            ClaimsPrincipal user,
            int demandeParUtilisateurId,
            DemandeRechargeWalletVirtuelCreateDto dto,
            CancellationToken ct = default)
        {
            if (dto.AgentId <= 0)
                return Fail("AGENT_ID_REQUIS", "AgentId est obligatoire.");

            var agent = await _db.Agents.AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdAgent == dto.AgentId && a.Statut, ct);
            if (agent == null)
                return Fail("AGENT_INTROUVABLE", "Agent introuvable ou inactif.");

            if (!await AgentQueryableExtensions.CanRechargeWalletVirtuelAsync(_db, user, dto.AgentId, ct))
            {
                return Fail(
                    "HIERARCHIE_RECHARGE_INTERDITE",
                    "Vous ne pouvez créer une demande que pour un agent de niveau hiérarchique inférieur au vôtre.",
                    forbidden: true);
            }

            var dejaEnAttente = await _db.DemandesRechargeWalletVirtuel.AnyAsync(
                d => d.AgentId == dto.AgentId
                     && d.Statut
                     && d.StatutDemande == DemandeRechargeWalletVirtuelStatuts.EnAttente,
                ct);
            if (dejaEnAttente)
            {
                return Fail(
                    "DEMANDE_EN_ATTENTE_EXISTANTE",
                    "Une demande de recharge est déjà en attente pour cet agent.",
                    conflict: true);
            }

            var wallet = await _db.WalletsVirtuelsAgents
                .FirstOrDefaultAsync(w => w.AgentId == dto.AgentId && w.Statut, ct);
            if (wallet == null)
                return Fail("WALLET_VIRTUEL_INTROUVABLE", "Wallet virtuel introuvable pour cet agent.");

            var plafond = (await _parametresMetierProvider.GetWalletVirtuelAsync(ct)).PlafondSolde;
            if (plafond <= 0)
                return Fail("PLAFOND_INVALIDE", "Le plafond wallet virtuel n'est pas configuré correctement.");

            var montant = CalculerMontantRecharge(plafond, wallet.SoldeVirtuel);
            if (montant <= 0)
            {
                return Fail(
                    "SOLDE_AU_PLAFOND",
                    $"Le solde actuel ({wallet.SoldeVirtuel}) est déjà au plafond ({plafond}).");
            }

            var demande = new DemandeRechargeWalletVirtuel
            {
                AgentId = dto.AgentId,
                MontantCalcule = montant,
                SoldeAuMomentDemande = wallet.SoldeVirtuel,
                PlafondAuMomentDemande = plafond,
                StatutDemande = DemandeRechargeWalletVirtuelStatuts.EnAttente,
                Motif = string.IsNullOrWhiteSpace(dto.Motif) ? null : dto.Motif.Trim(),
                DateDemande = DateTime.Now,
                DemandeParUtilisateurId = demandeParUtilisateurId,
                DateCreation = DateTime.Now,
                Statut = true
            };

            _db.DemandesRechargeWalletVirtuel.Add(demande);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Demande recharge wallet virtuel #{Id} créée pour agent {AgentId}: montant {Montant}",
                demande.IdDemande, dto.AgentId, montant);

            var created = await BaseQuery().FirstAsync(d => d.IdDemande == demande.IdDemande, ct);
            return Ok(MapToReadDto(created));
        }

        public async Task<DemandeRechargeWalletVirtuelOperationResultDto> ConfirmerAsync(
            ClaimsPrincipal user,
            int confirmeParUtilisateurId,
            int demandeId,
            CancellationToken ct = default)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var demande = await _db.DemandesRechargeWalletVirtuel
                    .FirstOrDefaultAsync(d => d.IdDemande == demandeId && d.Statut, ct);

                if (demande == null)
                    return Fail("DEMANDE_INTROUVABLE", "Demande introuvable.");

                if (demande.StatutDemande != DemandeRechargeWalletVirtuelStatuts.EnAttente)
                {
                    return Fail(
                        "DEMANDE_NON_EN_ATTENTE",
                        $"La demande n'est pas en attente (statut: {demande.StatutDemande}).",
                        conflict: true);
                }

                if (!await AgentQueryableExtensions.CanRechargeWalletVirtuelAsync(_db, user, demande.AgentId, ct))
                {
                    return Fail(
                        "HIERARCHIE_RECHARGE_INTERDITE",
                        "Vous ne pouvez confirmer une recharge que pour un agent de niveau hiérarchique inférieur au vôtre.",
                        forbidden: true);
                }

                var wallet = await _db.WalletsVirtuelsAgents
                    .FirstOrDefaultAsync(w => w.AgentId == demande.AgentId && w.Statut, ct);
                if (wallet == null)
                    return Fail("WALLET_VIRTUEL_INTROUVABLE", "Wallet virtuel introuvable pour cet agent.");

                var plafond = (await _parametresMetierProvider.GetWalletVirtuelAsync(ct)).PlafondSolde;
                var montant = CalculerMontantRecharge(plafond, wallet.SoldeVirtuel);
                if (montant <= 0)
                {
                    return Fail(
                        "SOLDE_AU_PLAFOND",
                        $"Le solde actuel ({wallet.SoldeVirtuel}) est déjà au plafond ({plafond}). Aucun crédit nécessaire.");
                }

                var soldeAvant = wallet.SoldeVirtuel;
                var soldeApres = soldeAvant + montant;
                wallet.SoldeVirtuel = soldeApres;
                wallet.DateModification = DateTime.Now;

                await _walletMouvementService.EnregistrerMouvementAsync(
                    wallet.IdWalletVirtuelAgent,
                    montant,
                    "CREDIT",
                    WalletVirtuelMouvementSources.RechargePlafond,
                    soldeAvant,
                    soldeApres,
                    confirmeParUtilisateurId,
                    wallet.DeviseId,
                    $"Demande recharge #{demande.IdDemande}",
                    referenceExterne: demande.IdDemande,
                    ct: ct);

                await _db.SaveChangesAsync(ct);

                var mouvementId = await _db.WalletVirtuelMouvements
                    .Where(m => m.WalletVirtuelId == wallet.IdWalletVirtuelAgent
                                && m.Source == WalletVirtuelMouvementSources.RechargePlafond
                                && m.ReferenceExterne == demande.IdDemande)
                    .OrderByDescending(m => m.IdWalletVirtuelMouvement)
                    .Select(m => m.IdWalletVirtuelMouvement)
                    .FirstAsync(ct);

                demande.MontantCalcule = montant;
                demande.MontantCredite = montant;
                demande.SoldeAvantCredit = soldeAvant;
                demande.SoldeApresCredit = soldeApres;
                demande.PlafondAuMomentDemande = plafond;
                demande.StatutDemande = DemandeRechargeWalletVirtuelStatuts.Confirmee;
                demande.DateConfirmation = DateTime.Now;
                demande.ConfirmeParUtilisateurId = confirmeParUtilisateurId;
                demande.WalletVirtuelMouvementId = mouvementId;
                demande.DateModification = DateTime.Now;

                await _db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                _logger.LogInformation(
                    "Demande recharge #{Id} confirmée: agent {AgentId}, crédit {Montant} ({Avant} → {Apres})",
                    demande.IdDemande, demande.AgentId, montant, soldeAvant, soldeApres);

                var updated = await BaseQuery().FirstAsync(d => d.IdDemande == demande.IdDemande, ct);
                return Ok(MapToReadDto(updated));
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<DemandeRechargeWalletVirtuelOperationResultDto> RejeterAsync(
            int rejeteParUtilisateurId,
            int demandeId,
            DemandeRechargeWalletVirtuelRejeterDto dto,
            CancellationToken ct = default)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Motif))
                return Fail("MOTIF_REQUIS", "Le motif de rejet est obligatoire.");

            var demande = await _db.DemandesRechargeWalletVirtuel
                .FirstOrDefaultAsync(d => d.IdDemande == demandeId && d.Statut, ct);

            if (demande == null)
                return Fail("DEMANDE_INTROUVABLE", "Demande introuvable.");

            if (demande.StatutDemande != DemandeRechargeWalletVirtuelStatuts.EnAttente)
            {
                return Fail(
                    "DEMANDE_NON_EN_ATTENTE",
                    $"La demande n'est pas en attente (statut: {demande.StatutDemande}).",
                    conflict: true);
            }

            demande.StatutDemande = DemandeRechargeWalletVirtuelStatuts.Rejetee;
            demande.MotifRejet = dto.Motif.Trim();
            demande.DateRejet = DateTime.Now;
            demande.RejeteParUtilisateurId = rejeteParUtilisateurId;
            demande.DateModification = DateTime.Now;

            await _db.SaveChangesAsync(ct);

            var updated = await BaseQuery().FirstAsync(d => d.IdDemande == demande.IdDemande, ct);
            return Ok(MapToReadDto(updated));
        }

        public static decimal CalculerMontantRecharge(decimal plafond, decimal soldeActuel) =>
            Math.Round(plafond - soldeActuel, 2, MidpointRounding.AwayFromZero);

        private IQueryable<DemandeRechargeWalletVirtuel> BaseQuery() =>
            _db.DemandesRechargeWalletVirtuel
                .AsNoTracking()
                .Include(d => d.Agent)
                .Include(d => d.DemandePar)
                .Include(d => d.ConfirmePar)
                .Include(d => d.RejetePar);

        private static DemandeRechargeWalletVirtuelReadDto MapToReadDto(DemandeRechargeWalletVirtuel d) =>
            new()
            {
                IdDemande = d.IdDemande,
                AgentId = d.AgentId,
                AgentNom = d.Agent?.NomComplet,
                AgentMatricule = d.Agent?.Matricule,
                MontantCalcule = d.MontantCalcule,
                SoldeAuMomentDemande = d.SoldeAuMomentDemande,
                PlafondAuMomentDemande = d.PlafondAuMomentDemande,
                StatutDemande = d.StatutDemande,
                Motif = d.Motif,
                MotifRejet = d.MotifRejet,
                DateDemande = d.DateDemande,
                DateConfirmation = d.DateConfirmation,
                DateRejet = d.DateRejet,
                DemandeParUtilisateurId = d.DemandeParUtilisateurId,
                DemandeParNom = d.DemandePar?.NomUtilisateur,
                ConfirmeParUtilisateurId = d.ConfirmeParUtilisateurId,
                ConfirmeParNom = d.ConfirmePar?.NomUtilisateur,
                RejeteParUtilisateurId = d.RejeteParUtilisateurId,
                RejeteParNom = d.RejetePar?.NomUtilisateur,
                WalletVirtuelMouvementId = d.WalletVirtuelMouvementId,
                MontantCredite = d.MontantCredite,
                SoldeAvantCredit = d.SoldeAvantCredit,
                SoldeApresCredit = d.SoldeApresCredit,
                DateCreation = d.DateCreation,
                DateModification = d.DateModification,
                Statut = d.Statut
            };

        private static DemandeRechargeWalletVirtuelOperationResultDto Ok(DemandeRechargeWalletVirtuelReadDto demande) =>
            new() { Success = true, Demande = demande };

        private static DemandeRechargeWalletVirtuelOperationResultDto Fail(
            string code,
            string message,
            bool conflict = false,
            bool forbidden = false) =>
            new()
            {
                Success = false,
                CodeErreur = code,
                Message = message,
                Conflict = conflict,
                Forbidden = forbidden
            };
    }
}
