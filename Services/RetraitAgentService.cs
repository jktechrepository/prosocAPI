using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services.Repositories;
using ProsocAPI.Utilities;

namespace ProsocAPI.Services
{
    public class RetraitAgentService : IDemandeRetraitAgentRepository, IRetraitAgentRepository
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<RetraitAgentService> _logger;
        private readonly IWalletAgentRepository _walletAgentRepository;
        private readonly IDeviseConversionService _deviseConversionService;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IParametresMetierProvider _parametresMetierProvider;
        private readonly ICaisseService _caisseService;

        public RetraitAgentService(
            ProsocDbContext db, 
            ILogger<RetraitAgentService> logger,
            IWalletAgentRepository walletAgentRepository,
            IDeviseConversionService deviseConversionService,
            IWebHostEnvironment hostEnvironment,
            IParametresMetierProvider parametresMetierProvider,
            ICaisseService caisseService)
        {
            _db = db;
            _logger = logger;
            _walletAgentRepository = walletAgentRepository;
            _deviseConversionService = deviseConversionService;
            _hostEnvironment = hostEnvironment;
            _parametresMetierProvider = parametresMetierProvider;
            _caisseService = caisseService;
        }

        // Méthodes pour DemandeRetraitAgent
        public async Task<List<DemandeRetraitAgent>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.DemandesRetraitAgents
                .Include(d => d.Agent)
                .Include(d => d.AgentValidation)
                .Include(d => d.JetonRetrait)
                .OrderByDescending(d => d.DateDemande)
                .ToListAsync(ct);
        }

        public async Task<DemandeRetraitAgent?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.DemandesRetraitAgents
                .Include(d => d.Agent)
                .Include(d => d.AgentValidation)
                .Include(d => d.JetonRetrait)
                .FirstOrDefaultAsync(d => d.IdDemande == id, ct);
        }

        public async Task<List<DemandeRetraitAgent>> GetByAgentIdAsync(int agentId, CancellationToken ct = default)
        {
            return await _db.DemandesRetraitAgents
                .Include(d => d.Agent)
                .Include(d => d.AgentValidation)
                .Include(d => d.JetonRetrait)
                .Where(d => d.AgentId == agentId)
                .OrderByDescending(d => d.DateDemande)
                .ToListAsync(ct);
        }

        public async Task<List<DemandeRetraitAgent>> GetByStatutAsync(string statut, CancellationToken ct = default)
        {
            return await _db.DemandesRetraitAgents
                .Include(d => d.Agent)
                .Include(d => d.AgentValidation)
                .Include(d => d.JetonRetrait)
                .Where(d => d.StatutDemande == statut)
                .OrderByDescending(d => d.DateDemande)
                .ToListAsync(ct);
        }

        public async Task<DemandeRetraitAgent> CreateAsync(DemandeRetraitAgent entity, CancellationToken ct = default)
        {
            _db.DemandesRetraitAgents.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<DemandeRetraitAgent?> UpdateAsync(int id, DemandeRetraitAgent entity, CancellationToken ct = default)
        {
            var snapshot = await _db.DemandesRetraitAgents
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.IdDemande == id, ct);
            if (snapshot == null) return null;

            var existing = await _db.DemandesRetraitAgents.FirstOrDefaultAsync(d => d.IdDemande == id, ct);
            if (existing == null) return null;

            var ancienStatut = snapshot.StatutDemande;
            existing.StatutDemande = entity.StatutDemande;
            existing.DateValidation = entity.DateValidation;
            existing.AgentValidationId = entity.AgentValidationId;
            existing.MotifRejet = entity.MotifRejet;
            existing.DateTraitement = entity.DateTraitement;
            existing.JetonRetraitId = entity.JetonRetraitId;
            existing.DateModification = DateTime.Now;

            if (ancienStatut != "REJETEE"
                && entity.StatutDemande == "REJETEE"
                && (ancienStatut == "EN_ATTENTE" || ancienStatut == "VALIDEE"))
            {
                await LibererSoldeReserveAsync(snapshot.AgentId, snapshot.MontantDemande, ct);
            }

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.DemandesRetraitAgents.FirstOrDefaultAsync(d => d.IdDemande == id, ct);
            if (existing == null) return false;

            if (existing.StatutDemande is "EN_ATTENTE" or "VALIDEE")
            {
                await LibererSoldeReserveAsync(existing.AgentId, existing.MontantDemande, ct);
            }

            _db.DemandesRetraitAgents.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        private async Task<WalletAgent?> GetPrincipalWalletTrackedAsync(int agentId, CancellationToken ct)
        {
            var devisePrincipale = await _deviseConversionService.GetDevisePrincipaleAsync(ct);
            return await _db.WalletsAgents
                .FirstOrDefaultAsync(w =>
                    w.AgentId == agentId
                    && w.DeviseId == devisePrincipale.IdDevise
                    && w.Statut, ct);
        }

        private async Task<bool> ReserverSoldeDisponibleAsync(int agentId, decimal montant, CancellationToken ct)
        {
            var wallet = await GetPrincipalWalletTrackedAsync(agentId, ct);
            if (wallet == null || wallet.SoldeDisponible < montant)
                return false;

            wallet.SoldeDisponible -= montant;
            wallet.DateModification = DateTime.Now;
            return true;
        }

        private async Task LibererSoldeReserveAsync(int agentId, decimal montant, CancellationToken ct)
        {
            var wallet = await GetPrincipalWalletTrackedAsync(agentId, ct);
            if (wallet == null)
                return;

            wallet.SoldeDisponible += montant;
            if (wallet.SoldeDisponible > wallet.SoldeCourant)
                wallet.SoldeDisponible = wallet.SoldeCourant;

            wallet.DateModification = DateTime.Now;
        }

        private static void DebiterSoldeApresRetrait(WalletAgent wallet, decimal montant)
        {
            wallet.SoldeCourant -= montant;
            if (wallet.SoldeDisponible > wallet.SoldeCourant)
                wallet.SoldeDisponible -= montant;

            wallet.DateModification = DateTime.Now;
        }

        private async Task TraiterJetonExpireAsync(JetonRetrait jeton, CancellationToken ct)
        {
            var demande = jeton.DemandeRetrait;
            if (demande == null || demande.StatutDemande != "VALIDEE")
                return;

            await LibererSoldeReserveAsync(jeton.AgentId, jeton.MontantRetrait, ct);
            demande.StatutDemande = "REJETEE";
            demande.MotifRejet = "Jeton de retrait expiré";
            demande.DateModification = DateTime.Now;
            jeton.EstValide = false;
            jeton.DateModification = DateTime.Now;
            await _db.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Vérifie si la période de retrait est autorisée
        /// </summary>
        public async Task<PeriodeRetraitVerificationDto> VerifierPeriodeRetraitAsync(
            DateTime date,
            CancellationToken ct = default)
        {
            var options = await _parametresMetierProvider.GetRetraitAgentAsync(ct);
            var estAutorise = RetraitAgentPeriodeHelper.EstJourAutorise(date, options);

            return new PeriodeRetraitVerificationDto
            {
                Date = date,
                EstPeriodeAutorisee = estAutorise,
                JourDuMois = date.Day,
                Message = RetraitAgentPeriodeHelper.BuildMessage(date, estAutorise, options),
                PeriodeInfo = RetraitAgentPeriodeHelper.GetPeriodeInfo(date, options)
            };
        }

        public async Task<PeriodeRetraitCouranteDto> GetPeriodeCouranteAsync(CancellationToken ct = default)
        {
            var options = await _parametresMetierProvider.GetRetraitAgentAsync(ct);
            var now = DateTime.Now;
            var estAutorise = RetraitAgentPeriodeHelper.EstJourAutorise(now, options);
            var fenetreActive = RetraitAgentPeriodeHelper.GetFenetreActive(now, options);
            var typeAutorise = RetraitAgentPeriodeHelper.GetTypeRetraitAutorise(now, options);

            return new PeriodeRetraitCouranteDto
            {
                Date = now,
                EstPeriodeAutorisee = estAutorise,
                Message = RetraitAgentPeriodeHelper.BuildMessage(now, estAutorise, options),
                JourDuMois = now.Day,
                PeriodeInfo = RetraitAgentPeriodeHelper.GetPeriodeInfo(now, options),
                Fenetre1Debut = options.Fenetre1Debut,
                Fenetre1Fin = options.Fenetre1Fin,
                Fenetre2Debut = RetraitAgentPeriodeHelper.GetFenetre2Debut(now.Year, now.Month, options),
                Fenetre2Fin = RetraitAgentPeriodeHelper.GetFenetre2Fin(now.Year, now.Month),
                FenetreActive = fenetreActive,
                TypeRetraitAutorise = typeAutorise,
                MontantMinimumPartiel = options.MontantMinimumPartiel,
                MontantDemandeRequis = typeAutorise == RetraitAgentPeriodeHelper.TypePartiel
            };
        }

        /// <summary>
        /// Vérifie si le solde est suffisant pour le retrait
        /// </summary>
        public async Task<SoldeVerificationDto> VerifierSoldeDisponible(int agentId, decimal montantDemande, CancellationToken ct = default)
        {
            var devisePrincipale = await _deviseConversionService.GetDevisePrincipaleAsync(ct);
            var wallet = await _walletAgentRepository.GetPrincipalWalletByAgentIdAsync(agentId, ct);
            var deviseLabel = FormatDeviseLabel(devisePrincipale);

            if (wallet == null)
            {
                return new SoldeVerificationDto
                {
                    AgentId = agentId,
                    MontantDemande = montantDemande,
                    SoldeDisponible = 0,
                    SoldeSuffisant = false,
                    Difference = montantDemande,
                    DeviseId = devisePrincipale.IdDevise,
                    DeviseCode = devisePrincipale.Code,
                    DeviseSymbole = devisePrincipale.Symbole,
                    Message = $"Aucun wallet en devise principale ({devisePrincipale.Code}) pour cet agent."
                };
            }

            var soldeSuffisant = wallet.SoldeDisponible >= montantDemande;

            return new SoldeVerificationDto
            {
                AgentId = agentId,
                AgentNom = $"{wallet.Agent?.NomComplet}".Trim(),
                MontantDemande = montantDemande,
                SoldeDisponible = wallet.SoldeDisponible,
                SoldeSuffisant = soldeSuffisant,
                Difference = soldeSuffisant ? wallet.SoldeDisponible - montantDemande : montantDemande - wallet.SoldeDisponible,
                DeviseId = devisePrincipale.IdDevise,
                DeviseCode = devisePrincipale.Code,
                DeviseSymbole = devisePrincipale.Symbole,
                Message = soldeSuffisant
                    ? $"Solde suffisant pour le retrait ({devisePrincipale.Code})"
                    : $"Solde insuffisant. Disponible: {wallet.SoldeDisponible:N0} {deviseLabel}, Demandé: {montantDemande:N0} {deviseLabel}"
            };
        }

        /// <summary>
        /// Crée une demande de retrait avec toutes les validations
        /// </summary>
        public async Task<RetraitWorkflowResultDto> CreerDemandeRetraitAsync(DemandeRetraitAgentCreateDto createDto, CancellationToken ct = default)
        {
            try
            {
                var dateReference = DateTime.Now;

                if (!_hostEnvironment.IsEnvironment("IntegrationTests"))
                {
                    var periodeVerification = await VerifierPeriodeRetraitAsync(dateReference, ct);
                    if (!periodeVerification.EstPeriodeAutorisee)
                    {
                        return new RetraitWorkflowResultDto
                        {
                            Succes = false,
                            Message = periodeVerification.Message
                        };
                    }
                }

                var wallet = await _walletAgentRepository.GetPrincipalWalletByAgentIdAsync(createDto.AgentId, ct);
                var soldeDisponible = wallet?.SoldeDisponible;
                var retraitOptions = await _parametresMetierProvider.GetRetraitAgentAsync(ct);

                var resolution = _hostEnvironment.IsEnvironment("IntegrationTests")
                    ? RetraitAgentDemandeResolver.ResoudreModeTest(createDto, retraitOptions)
                    : RetraitAgentDemandeResolver.Resoudre(createDto, dateReference, soldeDisponible, retraitOptions);

                if (!resolution.Succes)
                {
                    return new RetraitWorkflowResultDto
                    {
                        Succes = false,
                        Message = resolution.Message
                    };
                }

                var montantEffectif = resolution.MontantEffectif;
                var typeRetraitEffectif = resolution.TypeRetrait;

                if (!string.IsNullOrWhiteSpace(createDto.TypeRetrait)
                    && !string.Equals(createDto.TypeRetrait, typeRetraitEffectif, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation(
                        "TypeRetrait client '{TypeClient}' remplacé par '{TypeEffectif}' selon la fenêtre courante pour l'agent {AgentId}",
                        createDto.TypeRetrait,
                        typeRetraitEffectif,
                        createDto.AgentId);
                }

                var soldeVerification = await VerifierSoldeDisponible(createDto.AgentId, montantEffectif, ct);
                if (!soldeVerification.SoldeSuffisant)
                {
                    return new RetraitWorkflowResultDto
                    {
                        Succes = false,
                        Message = soldeVerification.Message
                    };
                }

                await using var transaction = await _db.Database.BeginTransactionAsync(ct);

                if (!await ReserverSoldeDisponibleAsync(createDto.AgentId, montantEffectif, ct))
                {
                    await transaction.RollbackAsync(ct);
                    return new RetraitWorkflowResultDto
                    {
                        Succes = false,
                        Message = "Solde insuffisant pour réserver le montant du retrait."
                    };
                }

                var demande = new DemandeRetraitAgent
                {
                    AgentId = createDto.AgentId,
                    MontantDemande = montantEffectif,
                    TypeRetrait = typeRetraitEffectif,
                    MotifRetrait = createDto.MotifRetrait,
                    StatutDemande = "EN_ATTENTE",
                    DateDemande = DateTime.Now,
                    DateCreation = DateTime.Now
                };

                _db.DemandesRetraitAgents.Add(demande);
                await _db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                var demandeCreee = demande;

                var devisePrincipale = await _deviseConversionService.GetDevisePrincipaleAsync(ct);
                return new RetraitWorkflowResultDto
                {
                    Succes = true,
                    Message = "Demande de retrait créée avec succès. En attente de validation.",
                    DemandeId = demandeCreee.IdDemande,
                    MontantRetrait = demandeCreee.MontantDemande,
                    TypeRetrait = demandeCreee.TypeRetrait,
                    DeviseId = devisePrincipale.IdDevise,
                    DeviseCode = devisePrincipale.Code,
                    DeviseSymbole = devisePrincipale.Symbole
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la demande de retrait pour l'agent {AgentId}", createDto.AgentId);
                return new RetraitWorkflowResultDto
                {
                    Succes = false,
                    Message = "Erreur technique lors de la création de la demande"
                };
            }
        }

        /// <summary>
        /// Valide une demande de retrait et génère le jeton
        /// </summary>
        public async Task<RetraitWorkflowResultDto> ValiderEtGenererJetonAsync(int demandeId, int agentValidationId, CancellationToken ct = default)
        {
            try
            {
                var demande = await GetByIdAsync(demandeId, ct);
                if (demande == null)
                {
                    return new RetraitWorkflowResultDto
                    {
                        Succes = false,
                        Message = "Demande de retrait non trouvée"
                    };
                }

                if (demande.StatutDemande != "EN_ATTENTE")
                {
                    return new RetraitWorkflowResultDto
                    {
                        Succes = false,
                        Message = $"Cette demande est déjà {demande.StatutDemande}"
                    };
                }

                var jeton = await GenererJetonRetraitAsync(demande.IdDemande, demande.AgentId, demande.MontantDemande, ct);

                demande.StatutDemande = "VALIDEE";
                demande.DateValidation = DateTime.Now;
                demande.AgentValidationId = agentValidationId;
                demande.JetonRetraitId = jeton.IdJeton;
                demande.DateModification = DateTime.Now;

                await _db.SaveChangesAsync(ct);

                return new RetraitWorkflowResultDto
                {
                    Succes = true,
                    Message = "Demande validée avec succès. Jeton de retrait généré.",
                    DemandeId = demande.IdDemande,
                    JetonId = jeton.IdJeton,
                    JetonCode = jeton.CodeJeton,
                    MontantRetrait = demande.MontantDemande,
                    DateEmission = jeton.DateEmission,
                    DateExpiration = jeton.DateExpiration
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation de la demande {DemandeId}", demandeId);
                return new RetraitWorkflowResultDto
                {
                    Succes = false,
                    Message = "Erreur technique lors de la validation"
                };
            }
        }

        /// <summary>
        /// Utilise un jeton de retrait (paiement caisse) et met à jour wallet + journal caisse.
        /// </summary>
        public async Task<RetraitPaiementResultDto> UtiliserJetonRetraitAsync(
            JetonRetraitUtilisationDto utilisationDto,
            int operateurUtilisateurId,
            CancellationToken ct = default)
        {
            var skipSessionCheck = _hostEnvironment.IsEnvironment("IntegrationTests")
                && !utilisationDto.SessionCaisseId.HasValue;

            try
            {
                var verifierPeriode = !skipSessionCheck && !utilisationDto.SessionCaisseId.HasValue;
                if (verifierPeriode)
                {
                    var periode = await VerifierPeriodeRetraitAsync(DateTime.Now, ct);
                    if (!periode.EstPeriodeAutorisee)
                    {
                        return Fail("HORS_PERIODE", periode.Message);
                    }
                }

                var jeton = await _db.JetonsRetraits
                    .Include(j => j.DemandeRetrait)
                    .Include(j => j.Agent)
                    .FirstOrDefaultAsync(j => j.CodeJeton == utilisationDto.CodeJeton && j.AgentId == utilisationDto.AgentId, ct);

                if (jeton == null)
                    return Fail("JETON_INTROUVABLE", "Jeton de retrait non trouvé");

                if (!jeton.EstValide)
                    return Fail("JETON_INVALIDE", "Jeton de retrait invalide");

                if (jeton.EstUtilise)
                    return Fail("JETON_DEJA_UTILISE", "Jeton de retrait déjà utilisé", conflict: true);

                if (jeton.DateExpiration < DateTime.Now)
                {
                    await TraiterJetonExpireAsync(jeton, ct);
                    return Fail("JETON_EXPIRE", "Jeton de retrait expiré");
                }

                if (jeton.DemandeRetrait == null || jeton.DemandeRetrait.StatutDemande != "VALIDEE")
                    return Fail("DEMANDE_INVALIDE", "La demande de retrait n'est pas en statut VALIDEE");

                SessionCaisse? session = null;
                try
                {
                    session = await _caisseService.ResolveSessionPourOperationAsync(
                        operateurUtilisateurId,
                        utilisationDto.SessionCaisseId,
                        skipSessionCheck,
                        ct);
                }
                catch (InvalidOperationException ex) when (ex.Message == "SESSION_CAISSIER_REQUISE")
                {
                    return Fail("SESSION_CAISSIER_REQUISE", "Aucune session de caisse ouverte. Ouvrez une session avant de payer un retrait.");
                }

                if (session != null)
                {
                    var soldeSession = await _caisseService.CalculerSoldeSessionAsync(session.IdSessionCaisse, ct);
                    if (soldeSession < jeton.MontantRetrait)
                    {
                        return Fail(
                            "SOLDE_CAISSE_INSUFFISANT",
                            $"Solde caisse insuffisant ({soldeSession:N0} disponible, {jeton.MontantRetrait:N0} requis).");
                    }
                }

                var wallet = await GetPrincipalWalletTrackedAsync(jeton.AgentId, ct);
                if (wallet == null)
                    return Fail("WALLET_INTROUVABLE", "Wallet agent introuvable en devise principale");

                await using var transaction = await _db.Database.BeginTransactionAsync(ct);

                jeton.EstUtilise = true;
                jeton.DateUtilisation = DateTime.Now;
                jeton.ObservationUtilisation = utilisationDto.ObservationUtilisation;
                jeton.OperateurUtilisateurId = operateurUtilisateurId;
                jeton.DateModification = DateTime.Now;

                var demande = jeton.DemandeRetrait;
                demande.StatutDemande = "TRAITEE";
                demande.DateTraitement = DateTime.Now;
                demande.OperateurPaiementUtilisateurId = operateurUtilisateurId;
                demande.DateModification = DateTime.Now;

                DebiterSoldeApresRetrait(wallet, jeton.MontantRetrait);

                var devisePrincipale = await _deviseConversionService.GetDevisePrincipaleAsync(ct);
                var walletMouvement = new WalletMouvement
                {
                    WalletId = wallet.IdWalletAgent,
                    DeviseId = devisePrincipale.IdDevise,
                    Montant = jeton.MontantRetrait,
                    TypeOperation = "DEBIT",
                    Source = "RETRAIT_JETON",
                    Description = $"Paiement retrait agent — jeton {jeton.CodeJeton}",
                    DateOperation = DateTime.Now,
                    DateCreation = DateTime.Now,
                    Statut = true
                };
                _db.WalletMouvements.Add(walletMouvement);
                await _db.SaveChangesAsync(ct);

                demande.WalletMouvementId = walletMouvement.IdWalletMouvement;

                MouvementCaisse? mouvementCaisse = null;
                if (session != null)
                {
                    mouvementCaisse = _caisseService.BuildMouvementSortieRetrait(
                        session, operateurUtilisateurId, jeton, demande, walletMouvement);
                    _db.MouvementsCaisses.Add(mouvementCaisse);
                    await _db.SaveChangesAsync(ct);
                }

                await _db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                decimal? soldeCaisseApres = null;
                if (session != null)
                    soldeCaisseApres = await _caisseService.CalculerSoldeSessionAsync(session.IdSessionCaisse, ct);

                _logger.LogInformation(
                    "Paiement retrait jeton {CodeJeton} — montant {Montant} par utilisateur {UtilisateurId}",
                    jeton.CodeJeton, jeton.MontantRetrait, operateurUtilisateurId);

                return new RetraitPaiementResultDto
                {
                    Succes = true,
                    Message = "Retrait payé avec succès",
                    DemandeId = demande.IdDemande,
                    JetonId = jeton.IdJeton,
                    JetonCode = jeton.CodeJeton,
                    MontantPaye = jeton.MontantRetrait,
                    SoldeWalletApres = wallet.SoldeCourant,
                    SoldeCaisseSessionApres = soldeCaisseApres,
                    WalletMouvementId = walletMouvement.IdWalletMouvement,
                    MouvementCaisseId = mouvementCaisse?.IdMouvementCaisse,
                    SessionCaisseId = session?.IdSessionCaisse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du paiement retrait jeton {CodeJeton}", utilisationDto.CodeJeton);
                return Fail("ERREUR_TECHNIQUE", "Erreur technique lors du paiement du retrait");
            }
        }

        private static RetraitPaiementResultDto Fail(string code, string message, bool conflict = false) =>
            new()
            {
                Succes = false,
                CodeErreur = code,
                Message = message
            };

        /// <summary>
        /// Récupère les statistiques de retrait pour un mois donné
        /// </summary>
        public async Task<DemandeRetraitAgentStatsDto> GetStatsAsync(DateTime date, CancellationToken ct = default)
        {
            var debutMois = new DateTime(date.Year, date.Month, 1);
            var finMois = debutMois.AddMonths(1).AddDays(-1);

            var demandes = await _db.DemandesRetraitAgents
                .Where(d => d.DateDemande >= debutMois && d.DateDemande <= finMois)
                .ToListAsync(ct);

            var totalDemandes = demandes.Count;
            var demandesEnAttente = demandes.Count(d => d.StatutDemande == "EN_ATTENTE");
            var demandesValidees = demandes.Count(d => d.StatutDemande == "VALIDEE");
            var demandesRejetees = demandes.Count(d => d.StatutDemande == "REJETEE");
            var demandesTraitees = demandes.Count(d => d.StatutDemande == "TRAITEE");

            var totalMontantDemande = demandes.Where(d => d.StatutDemande != "REJETEE").Sum(d => d.MontantDemande);
            var totalMontantTraite = demandes.Where(d => d.StatutDemande == "TRAITEE").Sum(d => d.MontantDemande);

            var tauxValidation = totalDemandes > 0 ? (decimal)demandesValidees / totalDemandes * 100 : 0;

            return new DemandeRetraitAgentStatsDto
            {
                TotalDemandes = totalDemandes,
                DemandesEnAttente = demandesEnAttente,
                DemandesValidees = demandesValidees,
                DemandesRejetees = demandesRejetees,
                DemandesTraitees = demandesTraitees,
                TotalMontantDemande = totalMontantDemande,
                TotalMontantTraite = totalMontantTraite,
                TauxValidation = Math.Round(tauxValidation, 2),
                DateStats = date
            };
        }

        private static string FormatDeviseLabel(Devise devise) =>
            !string.IsNullOrWhiteSpace(devise.Symbole) ? devise.Symbole! : devise.Code;

        /// <summary>
        /// Génère un jeton de retrait unique
        /// </summary>
        private async Task<JetonRetrait> GenererJetonRetraitAsync(int demandeId, int agentId, decimal montant, CancellationToken ct = default)
        {
            const string prefix = "JRT";
            var random = new Random();
            string codeJeton;
            int attempts = 0;
            const int maxAttempts = 10;

            do
            {
                var suffix = GenerateRandomString(8);
                codeJeton = $"{prefix}{suffix}";
                
                var exists = await _db.JetonsRetraits
                    .AnyAsync(j => j.CodeJeton == codeJeton, ct);
                
                if (!exists) break;
                attempts++;
            } while (attempts < maxAttempts);

            if (attempts >= maxAttempts)
            {
                throw new InvalidOperationException("Impossible de générer un code de jeton unique");
            }

            var jeton = new JetonRetrait
            {
                AgentId = agentId,
                DemandeRetraitId = demandeId,
                CodeJeton = codeJeton,
                MontantRetrait = montant,
                DateEmission = DateTime.Now,
                DateExpiration = DateTime.Now.AddDays(7),
                EstValide = true,
                EstUtilise = false,
                DateCreation = DateTime.Now
            };

            _db.JetonsRetraits.Add(jeton);
            await _db.SaveChangesAsync(ct);
            return jeton;
        }

        /// <summary>
        /// Génère une chaîne alphanumérique aléatoire
        /// </summary>
        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }
            return new string(result);
        }

        // Méthodes pour compatibilité avec ancienne interface RetraitAgent
        async Task<List<RetraitAgent>> IRetraitAgentRepository.GetAllAsync(CancellationToken ct = default)
        {
            return new List<RetraitAgent>();
        }

        async Task<RetraitAgent?> IRetraitAgentRepository.GetByIdAsync(int id, CancellationToken ct = default)
        {
            return null;
        }

        async Task<List<RetraitAgent>> IRetraitAgentRepository.GetByAgentAsync(int agentId, CancellationToken ct = default)
        {
            return new List<RetraitAgent>();
        }

        async Task<List<RetraitAgent>> IRetraitAgentRepository.GetNonValidesAsync(CancellationToken ct = default)
        {
            return new List<RetraitAgent>();
        }

        async Task<List<RetraitAgent>> IRetraitAgentRepository.GetValidesAsync(CancellationToken ct = default)
        {
            return new List<RetraitAgent>();
        }

        async Task<RetraitAgent> IRetraitAgentRepository.CreateAsync(RetraitAgent entity, CancellationToken ct = default)
        {
            return entity;
        }

        async Task<RetraitAgent?> IRetraitAgentRepository.UpdateAsync(int id, RetraitAgent entity, CancellationToken ct = default)
        {
            return null;
        }

        async Task<bool> IRetraitAgentRepository.DeleteAsync(int id, CancellationToken ct = default)
        {
            return false;
        }

        async Task<bool> IRetraitAgentRepository.ValiderRetraitAsync(int id, CancellationToken ct = default)
        {
            // Implémentation vide pour compatibilité
            return true;
        }
    }
}
