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
    public class PerceptionVirtuelleService : IPerceptionVirtuelleService
    {
        private readonly ProsocDbContext _db;
        private readonly IPaginationService _paginationService;
        private readonly IDeviseConversionService _deviseConversion;
        private readonly ICaisseService _caisseService;
        private readonly IWalletVirtuelMouvementService _walletMouvementService;
        private readonly ILogger<PerceptionVirtuelleService> _logger;

        public PerceptionVirtuelleService(
            ProsocDbContext db,
            IPaginationService paginationService,
            IDeviseConversionService deviseConversion,
            ICaisseService caisseService,
            IWalletVirtuelMouvementService walletMouvementService,
            ILogger<PerceptionVirtuelleService> logger)
        {
            _db = db;
            _paginationService = paginationService;
            _deviseConversion = deviseConversion;
            _caisseService = caisseService;
            _walletMouvementService = walletMouvementService;
            _logger = logger;
        }

        public async Task<PaginatedResponse<CollecteVirtuelleEnAttenteDto>> GetCollectesEnAttenteAsync(
            int? agentId,
            DateTime? dateDebut,
            DateTime? dateFin,
            PaginationRequest pagination,
            CancellationToken ct = default)
        {
            var agentParAffilie = await LoadAgentParAffilieAsync(ct);
            var collecteIdsAvecDebit = await LoadCollecteIdsAvecDebitVirtuelAsync(ct);

            var query = _db.Collectes
                .AsNoTracking()
                .Include(c => c.Agent)
                .Include(c => c.Affilie)
                .Include(c => c.Devise)
                .Where(c => c.Statut
                    && c.ModePaiement != null
                    && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement!)
                    && (c.StatutPerception == null || c.StatutPerception == CollecteStatutPerception.NonPerçu));

            if (dateDebut.HasValue)
                query = query.Where(c => c.DateCollecte >= dateDebut.Value);
            if (dateFin.HasValue)
                query = query.Where(c => c.DateCollecte <= dateFin.Value);

            var collectes = await query
                .OrderByDescending(c => c.DateCollecte)
                .ToListAsync(ct);

            var eligibles = collectes
                .Where(c => MethodePaiementHelper.IsVirtualAccount(c.ModePaiement)
                    && collecteIdsAvecDebit.Contains(c.IdCollecte))
                .Select(c => MapEnAttente(c, ResolveAgentIdEffectif(c, agentParAffilie)))
                .Where(d => d.AgentIdEffectif > 0)
                .ToList();

            if (agentId.HasValue)
                eligibles = eligibles.Where(d => d.AgentIdEffectif == agentId.Value).ToList();

            return PaginateInMemory(eligibles, pagination);
        }

        public async Task<List<PerceptionVirtuelleSyntheseAgentDto>> GetSyntheseAgentsAsync(CancellationToken ct = default)
        {
            var agentParAffilie = await LoadAgentParAffilieAsync(ct);
            var collecteIdsAvecDebit = await LoadCollecteIdsAvecDebitVirtuelAsync(ct);
            var devisePrincipale = await _deviseConversion.GetDevisePrincipaleAsync(ct);

            var collectes = await _db.Collectes
                .AsNoTracking()
                .Include(c => c.Agent)
                .Where(c => c.Statut
                    && c.ModePaiement != null
                    && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement!)
                    && (c.StatutPerception == null || c.StatutPerception == CollecteStatutPerception.NonPerçu))
                .ToListAsync(ct);

            var grouped = collectes
                .Where(c => MethodePaiementHelper.IsVirtualAccount(c.ModePaiement)
                    && collecteIdsAvecDebit.Contains(c.IdCollecte))
                .GroupBy(c => ResolveAgentIdEffectif(c, agentParAffilie))
                .Where(g => g.Key > 0)
                .ToList();

            var agentIds = grouped.Select(g => g.Key).ToList();
            var agents = await _db.Agents
                .AsNoTracking()
                .Where(a => agentIds.Contains(a.IdAgent))
                .ToDictionaryAsync(a => a.IdAgent, ct);

            return grouped
                .Select(g =>
                {
                    agents.TryGetValue(g.Key, out var agent);
                    return new PerceptionVirtuelleSyntheseAgentDto
                    {
                        AgentId = g.Key,
                        AgentNom = agent?.NomComplet,
                        AgentMatricule = agent?.Matricule,
                        NombreCollectesEnAttente = g.Count(),
                        MontantEnAttente = g.Sum(c => c.MontantDevisePrincipale ?? c.Montant),
                        DeviseCode = devisePrincipale.Code
                    };
                })
                .OrderByDescending(s => s.MontantEnAttente)
                .ToList();
        }

        public async Task<PerceptionVirtuelleReadDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var perception = await _db.PerceptionsVirtuelles
                .AsNoTracking()
                .Include(p => p.Agent)
                .Include(p => p.PercepteurUtilisateur)
                .Include(p => p.AnnuleParUtilisateur)
                .Include(p => p.Devise)
                .Include(p => p.Lignes)
                    .ThenInclude(l => l.Collecte)
                        .ThenInclude(c => c.Affilie)
                .FirstOrDefaultAsync(p => p.IdPerceptionVirtuelle == id, ct);

            return perception == null ? null : MapPerception(perception);
        }

        public async Task<PaginatedResponse<PerceptionVirtuelleReadDto>> GetHistoriqueAsync(
            int percepteurUtilisateurId,
            PaginationRequest pagination,
            CancellationToken ct = default) =>
            await QueryHistoriqueAsync(
                new PerceptionVirtuelleHistoriqueFiltreDto { PercepteurUtilisateurId = percepteurUtilisateurId },
                pagination,
                ct);

        public async Task<PaginatedResponse<PerceptionVirtuelleReadDto>> GetHistoriqueGlobalAsync(
            PerceptionVirtuelleHistoriqueFiltreDto filtres,
            PaginationRequest pagination,
            CancellationToken ct = default) =>
            await QueryHistoriqueAsync(filtres, pagination, ct);

        public async Task<PerceptionReconciliationDto> GetReconciliationAsync(
            int? agentId,
            DateTime? dateDebut,
            DateTime? dateFin,
            CancellationToken ct = default)
        {
            var agentParAffilie = await LoadAgentParAffilieAsync(ct);
            var debitParCollecte = await LoadDebitVirtuelParCollecteAsync(ct);
            var devisePrincipale = await _deviseConversion.GetDevisePrincipaleAsync(ct);

            var collectes = await _db.Collectes
                .AsNoTracking()
                .Where(c => c.Statut
                    && c.ModePaiement != null
                    && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement!))
                .ToListAsync(ct);

            if (dateDebut.HasValue)
                collectes = collectes.Where(c => c.DateCollecte >= dateDebut.Value).ToList();
            if (dateFin.HasValue)
                collectes = collectes.Where(c => c.DateCollecte <= dateFin.Value).ToList();

            var collecteIds = collectes.Select(c => c.IdCollecte).ToHashSet();

            var vaCollectes = collectes
                .Where(c => MethodePaiementHelper.IsVirtualAccount(c.ModePaiement)
                    && debitParCollecte.ContainsKey(c.IdCollecte))
                .ToList();

            if (agentId.HasValue)
            {
                vaCollectes = vaCollectes
                    .Where(c => ResolveAgentIdEffectif(c, agentParAffilie) == agentId.Value)
                    .ToList();
            }

            decimal MontantPrincipal(Collecte c) => c.MontantDevisePrincipale ?? c.Montant;

            var nonPercu = vaCollectes
                .Where(c => !CollecteStatutPerception.EstPerçu(c.StatutPerception))
                .ToList();
            var percuTerrain = vaCollectes
                .Where(c => CollecteStatutPerception.EstPerçu(c.StatutPerception) && c.PerceptionVirtuelleId.HasValue)
                .ToList();

            var vaValides = collectes
                .Where(c => MethodePaiementHelper.IsVirtualAccount(c.ModePaiement)
                    && CollecteStatutPaiementRegles.EstValide(c.StatutPaiement))
                .ToList();

            if (agentId.HasValue)
            {
                vaValides = vaValides
                    .Where(c => ResolveAgentIdEffectif(c, agentParAffilie) == agentId.Value)
                    .ToList();
            }

            var debitsSansCollecte = debitParCollecte.Keys.Count(id => !collecteIds.Contains(id));
            var collectesVaSansDebit = vaValides.Count(c => !debitParCollecte.ContainsKey(c.IdCollecte));
            var percuSansJournal = vaValides.Count(c =>
                CollecteStatutPerception.EstPerçu(c.StatutPerception) && !c.PerceptionVirtuelleId.HasValue);

            var montantDebit = vaCollectes.Sum(MontantPrincipal);
            var montantVaValides = vaValides
                .Where(c => debitParCollecte.ContainsKey(c.IdCollecte))
                .Sum(MontantPrincipal);

            return new PerceptionReconciliationDto
            {
                DeviseCode = devisePrincipale.Code,
                AgentId = agentId,
                MontantDebitWallet = montantDebit,
                MontantCollectesVaValides = montantVaValides,
                MontantNonPerçu = nonPercu.Sum(MontantPrincipal),
                MontantPerçuTerrain = percuTerrain.Sum(MontantPrincipal),
                NombreNonPerçu = nonPercu.Count,
                NombrePerçu = percuTerrain.Count,
                Anomalies = new PerceptionReconciliationAnomaliesDto
                {
                    CollectesPercuSansJournal = percuSansJournal,
                    DebitsSansCollecte = debitsSansCollecte,
                    CollectesVaSansDebit = collectesVaSansDebit
                }
            };
        }

        private async Task<PaginatedResponse<PerceptionVirtuelleReadDto>> QueryHistoriqueAsync(
            PerceptionVirtuelleHistoriqueFiltreDto filtres,
            PaginationRequest pagination,
            CancellationToken ct)
        {
            var query = _db.PerceptionsVirtuelles
                .AsNoTracking()
                .Include(p => p.Agent)
                .Include(p => p.PercepteurUtilisateur)
                .Include(p => p.AnnuleParUtilisateur)
                .Include(p => p.Devise)
                .Include(p => p.Lignes)
                    .ThenInclude(l => l.Collecte)
                        .ThenInclude(c => c.Affilie)
                .Where(p => p.Statut);

            if (filtres.PercepteurUtilisateurId.HasValue)
                query = query.Where(p => p.PercepteurUtilisateurId == filtres.PercepteurUtilisateurId.Value);
            if (filtres.AgentId.HasValue)
                query = query.Where(p => p.AgentId == filtres.AgentId.Value);
            if (filtres.DateDebut.HasValue)
                query = query.Where(p => p.DatePerception >= filtres.DateDebut.Value);
            if (filtres.DateFin.HasValue)
                query = query.Where(p => p.DatePerception <= filtres.DateFin.Value);

            query = query.OrderByDescending(p => p.DatePerception);

            var paginated = await _paginationService.CreatePaginatedResponseAsync(query, pagination, ct);
            var dtos = paginated.Data.Select(MapPerception).ToList();

            return new PaginatedResponse<PerceptionVirtuelleReadDto>
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

        public async Task<PerceptionVirtuelleConfirmerResultDto> ConfirmerPerceptionAsync(
            int percepteurUtilisateurId,
            PerceptionVirtuelleConfirmerDto dto,
            CancellationToken ct = default)
        {
            if (dto.CollecteIds == null || dto.CollecteIds.Count == 0)
                return Fail("COLLECTE_IDS_REQUIS", "Au moins une collecte est requise.");

            if (dto.CollecteIds.Distinct().Count() != dto.CollecteIds.Count)
                return Fail("COLLECTE_IDS_DUPLIQUES", "La liste des collectes contient des doublons.");

            var agentParAffilie = await LoadAgentParAffilieAsync(ct);
            var collecteIdsAvecDebit = await LoadCollecteIdsAvecDebitVirtuelAsync(ct);

            var collectes = await _db.Collectes
                .Where(c => dto.CollecteIds.Contains(c.IdCollecte))
                .ToListAsync(ct);

            if (collectes.Count != dto.CollecteIds.Count)
                return Fail("COLLECTE_INTROUVABLE", "Une ou plusieurs collectes sont introuvables.");

            foreach (var collecte in collectes)
            {
                if (CollecteStatutPerception.EstPerçu(collecte.StatutPerception))
                    return Fail("COLLECTE_DEJA_PERCUE", $"La collecte {collecte.IdCollecte} a déjà été perçue.", conflict: true);

                if (!MethodePaiementHelper.IsVirtualAccount(collecte.ModePaiement))
                    return Fail("MODE_PAIEMENT_INVALIDE", $"La collecte {collecte.IdCollecte} n'est pas en compte virtuel.");

                if (!CollecteStatutPaiementRegles.EstValide(collecte.StatutPaiement))
                    return Fail("PAIEMENT_NON_VALIDE", $"La collecte {collecte.IdCollecte} n'est pas en statut VALIDE.");

                if (!collecteIdsAvecDebit.Contains(collecte.IdCollecte))
                    return Fail("DEBIT_VIRTUEL_MANQUANT", $"Aucun débit wallet virtuel trouvé pour la collecte {collecte.IdCollecte}.");

                var agentEffectif = ResolveAgentIdEffectif(collecte, agentParAffilie);
                if (agentEffectif != dto.AgentId)
                    return Fail("AGENT_INCOHERENT", $"La collecte {collecte.IdCollecte} n'appartient pas à l'agent {dto.AgentId}.");
            }

            var devisePrincipale = await _deviseConversion.GetDevisePrincipaleAsync(ct);
            var montantTotal = collectes.Sum(c => c.MontantDevisePrincipale ?? c.Montant);

            var debitsVa = await _db.WalletVirtuelMouvements
                .AsNoTracking()
                .Where(m => m.TypeOperation == "DEBIT"
                    && m.Source == WalletVirtuelMouvementSources.CollecteCompteVirtuel
                    && m.ReferenceExterne != null
                    && dto.CollecteIds.Contains(m.ReferenceExterne.Value)
                    && m.Statut)
                .ToListAsync(ct);

            var mouvementsVirtuels = debitsVa
                .GroupBy(m => m.ReferenceExterne!.Value)
                .ToDictionary(g => g.Key, g => g.First().IdWalletVirtuelMouvement);

            SessionCaisse session;
            try
            {
                session = await _caisseService.ResolveSessionPourOperationAsync(
                    percepteurUtilisateurId, null, skipSessionCheck: false, ct)
                    ?? throw new InvalidOperationException("SESSION_CAISSIER_REQUISE");
            }
            catch (InvalidOperationException ex) when (ex.Message == "SESSION_CAISSIER_REQUISE")
            {
                return Fail("SESSION_CAISSIER_REQUISE",
                    "Une session de caisse ouverte est requise pour confirmer la perception.");
            }

            var walletIds = debitsVa.Select(d => d.WalletVirtuelId).Distinct().ToList();
            var wallets = await _db.WalletsVirtuelsAgents
                .Where(w => walletIds.Contains(w.IdWalletVirtuelAgent) && w.Statut)
                .ToDictionaryAsync(w => w.IdWalletVirtuelAgent, ct);

            if (wallets.Count != walletIds.Count)
                return Fail("WALLET_VIRTUEL_INTROUVABLE", "Wallet virtuel introuvable pour une des collectes.");

            await using var transaction = await _db.Database.BeginTransactionAsync(ct);

            var perception = new PerceptionVirtuelle
            {
                AgentId = dto.AgentId,
                PercepteurUtilisateurId = percepteurUtilisateurId,
                MontantTotal = montantTotal,
                DeviseId = devisePrincipale.IdDevise,
                NombreCollectes = collectes.Count,
                DatePerception = DateTime.Now,
                Observation = dto.Observation,
                StatutMetier = PerceptionVirtuelleStatuts.Confirmee,
                DateCreation = DateTime.Now,
                Statut = true
            };

            _db.PerceptionsVirtuelles.Add(perception);
            await _db.SaveChangesAsync(ct);

            foreach (var collecte in collectes)
            {
                var montant = collecte.MontantDevisePrincipale ?? collecte.Montant;
                var agentEffectif = ResolveAgentIdEffectif(collecte, agentParAffilie);

                _db.PerceptionsVirtuellesLignes.Add(new PerceptionVirtuelleLigne
                {
                    PerceptionVirtuelleId = perception.IdPerceptionVirtuelle,
                    CollecteId = collecte.IdCollecte,
                    AgentId = agentEffectif,
                    Montant = montant,
                    WalletVirtuelMouvementId = mouvementsVirtuels.TryGetValue(collecte.IdCollecte, out var mouvementId)
                        ? mouvementId
                        : null,
                    DateCreation = DateTime.Now,
                    Statut = true
                });

                collecte.StatutPerception = CollecteStatutPerception.Perçu;
                collecte.DatePerception = DateTime.Now;
                collecte.PercepteurUtilisateurId = percepteurUtilisateurId;
                collecte.PerceptionVirtuelleId = perception.IdPerceptionVirtuelle;
                collecte.DateModification = DateTime.Now;
            }

            _db.MouvementsCaisses.Add(new MouvementCaisse
            {
                SessionCaisseId = session.IdSessionCaisse,
                UtilisateurId = percepteurUtilisateurId,
                TypeOperation = MouvementCaisseTypes.Entree,
                Source = MouvementCaisseSources.PerceptionVirtuelle,
                Montant = montantTotal,
                DeviseId = session.DeviseId,
                DateOperation = DateTime.Now,
                PerceptionVirtuelleId = perception.IdPerceptionVirtuelle,
                Description = $"Perception virtuelle #{perception.IdPerceptionVirtuelle} — agent {dto.AgentId}",
                DateCreation = DateTime.Now,
                Statut = true
            });

            foreach (var debit in debitsVa.GroupBy(d => d.ReferenceExterne!.Value).Select(g => g.First()))
            {
                if (!wallets.TryGetValue(debit.WalletVirtuelId, out var wallet))
                    return Fail("WALLET_VIRTUEL_INTROUVABLE", $"Wallet virtuel introuvable (id {debit.WalletVirtuelId}).");

                var ancienSolde = wallet.SoldeVirtuel;
                wallet.SoldeVirtuel += debit.Montant;
                wallet.DateModification = DateTime.Now;

                await _walletMouvementService.EnregistrerMouvementAsync(
                    wallet.IdWalletVirtuelAgent,
                    debit.Montant,
                    "CREDIT",
                    WalletVirtuelMouvementSources.RemisePerceptionVirtuelle,
                    ancienSolde,
                    wallet.SoldeVirtuel,
                    percepteurUtilisateurId,
                    wallet.DeviseId,
                    $"Remise perception #{perception.IdPerceptionVirtuelle} — collecte {debit.ReferenceExterne}",
                    debit.ReferenceExterne,
                    ct);
            }

            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            var soldeRestant = await CalculerSoldeRestantAgentAsync(dto.AgentId, agentParAffilie, collecteIdsAvecDebit, ct);

            _logger.LogInformation(
                "Perception virtuelle {PerceptionId} — {Nombre} collecte(s), montant {Montant} par utilisateur {UtilisateurId} (caisse+wallet)",
                perception.IdPerceptionVirtuelle, collectes.Count, montantTotal, percepteurUtilisateurId);

            return new PerceptionVirtuelleConfirmerResultDto
            {
                Succes = true,
                Message = "Perception confirmée avec succès",
                PerceptionVirtuelleId = perception.IdPerceptionVirtuelle,
                MontantTotal = montantTotal,
                NombreCollectes = collectes.Count,
                SoldeRestantAgent = soldeRestant
            };
        }

        public async Task<PerceptionVirtuelleConfirmerResultDto> AnnulerPerceptionAsync(
            int annuleParUtilisateurId,
            int perceptionId,
            PerceptionVirtuelleAnnulerDto dto,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Motif))
                return Fail("MOTIF_REQUIS", "Le motif d'annulation est obligatoire.");

            var perception = await _db.PerceptionsVirtuelles
                .Include(p => p.Lignes)
                .FirstOrDefaultAsync(p => p.IdPerceptionVirtuelle == perceptionId, ct);

            if (perception == null)
                return Fail("PERCEPTION_INTROUVABLE", "Perception introuvable.");

            if (PerceptionVirtuelleStatuts.EstAnnulee(perception.StatutMetier))
                return Fail("DEJA_ANNULEE", "Cette perception est déjà annulée.", conflict: true);

            var collecteIds = perception.Lignes.Select(l => l.CollecteId).Distinct().ToList();
            var collectes = await _db.Collectes
                .Where(c => collecteIds.Contains(c.IdCollecte))
                .ToListAsync(ct);

            var mouvementsCaisse = await _db.MouvementsCaisses
                .Where(m => m.PerceptionVirtuelleId == perceptionId && m.Statut)
                .ToListAsync(ct);

            var creditsRemise = await _db.WalletVirtuelMouvements
                .Where(m => m.TypeOperation == "CREDIT"
                    && m.Source == WalletVirtuelMouvementSources.RemisePerceptionVirtuelle
                    && m.ReferenceExterne != null
                    && collecteIds.Contains(m.ReferenceExterne.Value)
                    && m.Statut)
                .ToListAsync(ct);

            var walletIds = creditsRemise.Select(c => c.WalletVirtuelId).Distinct().ToList();
            var wallets = await _db.WalletsVirtuelsAgents
                .Where(w => walletIds.Contains(w.IdWalletVirtuelAgent))
                .ToDictionaryAsync(w => w.IdWalletVirtuelAgent, ct);

            await using var transaction = await _db.Database.BeginTransactionAsync(ct);

            perception.StatutMetier = PerceptionVirtuelleStatuts.Annulee;
            perception.MotifAnnulation = dto.Motif.Trim();
            perception.DateAnnulation = DateTime.Now;
            perception.AnnuleParUtilisateurId = annuleParUtilisateurId;
            perception.DateModification = DateTime.Now;

            foreach (var ligne in perception.Lignes)
                ligne.Statut = false;

            foreach (var collecte in collectes)
            {
                if (collecte.PerceptionVirtuelleId == perceptionId)
                {
                    collecte.StatutPerception = CollecteStatutPerception.NonPerçu;
                    collecte.DatePerception = null;
                    collecte.PercepteurUtilisateurId = null;
                    collecte.PerceptionVirtuelleId = null;
                    collecte.DateModification = DateTime.Now;
                }
            }

            foreach (var mouvement in mouvementsCaisse)
            {
                mouvement.Statut = false;
                mouvement.Description = string.IsNullOrWhiteSpace(mouvement.Description)
                    ? $"Annulé — perception #{perceptionId}"
                    : $"{mouvement.Description} | Annulé — perception #{perceptionId}";
            }

            foreach (var credit in creditsRemise.GroupBy(c => c.ReferenceExterne!.Value).Select(g => g.First()))
            {
                if (!wallets.TryGetValue(credit.WalletVirtuelId, out var wallet))
                    return Fail("WALLET_VIRTUEL_INTROUVABLE", $"Wallet virtuel introuvable (id {credit.WalletVirtuelId}).");

                if (wallet.SoldeVirtuel < credit.Montant)
                    return Fail("SOLDE_VIRTUEL_INSUFFISANT",
                        $"Solde virtuel insuffisant pour annuler la remise de la collecte {credit.ReferenceExterne}.");

                var ancienSolde = wallet.SoldeVirtuel;
                wallet.SoldeVirtuel -= credit.Montant;
                wallet.DateModification = DateTime.Now;

                await _walletMouvementService.EnregistrerMouvementAsync(
                    wallet.IdWalletVirtuelAgent,
                    credit.Montant,
                    "DEBIT",
                    WalletVirtuelMouvementSources.AnnulRemisePerceptionVirtuelle,
                    ancienSolde,
                    wallet.SoldeVirtuel,
                    annuleParUtilisateurId,
                    wallet.DeviseId,
                    $"Annulation remise perception #{perceptionId} — collecte {credit.ReferenceExterne}",
                    credit.ReferenceExterne,
                    ct);
            }

            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "Perception virtuelle {PerceptionId} annulée par utilisateur {UtilisateurId} — motif: {Motif}",
                perceptionId, annuleParUtilisateurId, perception.MotifAnnulation);

            return new PerceptionVirtuelleConfirmerResultDto
            {
                Succes = true,
                Message = "Perception annulée avec succès",
                PerceptionVirtuelleId = perception.IdPerceptionVirtuelle,
                MontantTotal = perception.MontantTotal,
                NombreCollectes = perception.NombreCollectes
            };
        }

        public async Task<(decimal Montant, int Nombre)> GetTotauxVirtuelsEnAttenteAsync(CancellationToken ct = default)
        {
            var synthese = await GetSyntheseAgentsAsync(ct);
            return (synthese.Sum(s => s.MontantEnAttente), synthese.Sum(s => s.NombreCollectesEnAttente));
        }

        private async Task<decimal> CalculerSoldeRestantAgentAsync(
            int agentId,
            Dictionary<int, int?> agentParAffilie,
            HashSet<int> collecteIdsAvecDebit,
            CancellationToken ct)
        {
            var collectes = await _db.Collectes
                .AsNoTracking()
                .Where(c => c.Statut
                    && c.ModePaiement != null
                    && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement!)
                    && (c.StatutPerception == null || c.StatutPerception == CollecteStatutPerception.NonPerçu))
                .ToListAsync(ct);

            return collectes
                .Where(c => MethodePaiementHelper.IsVirtualAccount(c.ModePaiement)
                    && collecteIdsAvecDebit.Contains(c.IdCollecte)
                    && ResolveAgentIdEffectif(c, agentParAffilie) == agentId)
                .Sum(c => c.MontantDevisePrincipale ?? c.Montant);
        }

        private async Task<Dictionary<int, int?>> LoadAgentParAffilieAsync(CancellationToken ct) =>
            await _db.Adhesions
                .AsNoTracking()
                .Where(a => a.Statut)
                .ToDictionaryAsync(a => a.AffilieId, a => a.AgentId, ct);

        private async Task<HashSet<int>> LoadCollecteIdsAvecDebitVirtuelAsync(CancellationToken ct)
        {
            var map = await LoadDebitVirtuelParCollecteAsync(ct);
            return map.Keys.ToHashSet();
        }

        private async Task<Dictionary<int, int>> LoadDebitVirtuelParCollecteAsync(CancellationToken ct)
        {
            var mouvements = await _db.WalletVirtuelMouvements
                .AsNoTracking()
                .Where(m => m.TypeOperation == "DEBIT"
                    && m.Source == WalletVirtuelMouvementSources.CollecteCompteVirtuel
                    && m.ReferenceExterne != null
                    && m.Statut)
                .Select(m => new { CollecteId = m.ReferenceExterne!.Value, m.IdWalletVirtuelMouvement })
                .ToListAsync(ct);

            return mouvements
                .GroupBy(m => m.CollecteId)
                .ToDictionary(g => g.Key, g => g.First().IdWalletVirtuelMouvement);
        }

        public static int ResolveAgentIdEffectif(Collecte collecte, Dictionary<int, int?> agentParAffilie)
        {
            if (collecte.AgentId.HasValue && collecte.AgentId.Value > 0)
                return collecte.AgentId.Value;

            if (agentParAffilie.TryGetValue(collecte.AffilieId, out var adhesionAgentId) && adhesionAgentId.HasValue)
                return adhesionAgentId.Value;

            return 0;
        }

        private static CollecteVirtuelleEnAttenteDto MapEnAttente(Collecte c, int agentIdEffectif) =>
            new()
            {
                IdCollecte = c.IdCollecte,
                AgentId = c.AgentId,
                AgentIdEffectif = agentIdEffectif,
                AgentNom = c.Agent?.NomComplet,
                AgentMatricule = c.Agent?.Matricule,
                AffilieId = c.AffilieId,
                AffilieNom = c.Affilie?.Nom,
                Montant = c.Montant,
                MontantDevisePrincipale = c.MontantDevisePrincipale ?? c.Montant,
                DeviseCode = c.Devise?.Code,
                DateCollecte = c.DateCollecte,
                TypeCollecte = c.TypeCollecte.ToString(),
                ReferencePaiement = c.ReferencePaiement,
                StatutPerception = c.StatutPerception ?? CollecteStatutPerception.NonPerçu
            };

        private static PerceptionVirtuelleReadDto MapPerception(PerceptionVirtuelle p) =>
            new()
            {
                IdPerceptionVirtuelle = p.IdPerceptionVirtuelle,
                AgentId = p.AgentId,
                AgentNom = p.Agent?.NomComplet,
                AgentMatricule = p.Agent?.Matricule,
                PercepteurUtilisateurId = p.PercepteurUtilisateurId,
                PercepteurNom = p.PercepteurUtilisateur?.NomUtilisateur,
                MontantTotal = p.MontantTotal,
                DeviseId = p.DeviseId,
                DeviseCode = p.Devise?.Code,
                NombreCollectes = p.NombreCollectes,
                DatePerception = p.DatePerception,
                Observation = p.Observation,
                StatutMetier = string.IsNullOrWhiteSpace(p.StatutMetier)
                    ? PerceptionVirtuelleStatuts.Confirmee
                    : p.StatutMetier,
                MotifAnnulation = p.MotifAnnulation,
                DateAnnulation = p.DateAnnulation,
                AnnuleParUtilisateurId = p.AnnuleParUtilisateurId,
                AnnuleParNom = p.AnnuleParUtilisateur?.NomUtilisateur,
                Lignes = p.Lignes
                    .Select(l => new PerceptionVirtuelleLigneReadDto
                    {
                        IdLigne = l.IdLigne,
                        CollecteId = l.CollecteId,
                        AgentId = l.AgentId,
                        Montant = l.Montant,
                        WalletVirtuelMouvementId = l.WalletVirtuelMouvementId,
                        AffilieNom = l.Collecte?.Affilie?.Nom,
                        DateCollecte = l.Collecte?.DateCollecte
                    })
                    .ToList()
            };

        private static PerceptionVirtuelleConfirmerResultDto Fail(string code, string message, bool conflict = false) =>
            new()
            {
                Succes = false,
                CodeErreur = code,
                Message = message
            };

        private static PaginatedResponse<CollecteVirtuelleEnAttenteDto> PaginateInMemory(
            List<CollecteVirtuelleEnAttenteDto> items,
            PaginationRequest pagination)
        {
            var page = pagination.Page <= 0 ? 1 : pagination.Page;
            var pageSize = pagination.PageSize <= 0 ? 20 : pagination.PageSize;
            var total = items.Count;
            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
            var data = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PaginatedResponse<CollecteVirtuelleEnAttenteDto>
            {
                Data = data,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };
        }
    }
}
