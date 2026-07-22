using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Utilities;

namespace ProsocAPI.Services
{
    public interface IAgentMaashRetenueService
    {
        Task<AgentMaashCouvertureReadDto> GetCouvertureAsync(int agentId, CancellationToken ct = default);
        Task<AgentMaashRetenueReadDto> AppliquerRetenueMensuelleAsync(
            int agentId,
            AgentMaashRetenueRequestDto? request,
            CancellationToken ct = default);
        Task<AgentMaashBatchResultDto> ExecuterRetenueAutomatiqueAsync(CancellationToken ct = default);
        Task<bool> DoitExecuterRetenueAutomatiqueAsync(CancellationToken ct = default);
    }

    public class AgentMaashRetenueService : IAgentMaashRetenueService
    {
        private readonly ProsocDbContext _db;
        private readonly IParametresMetierProvider _parametresMetierProvider;
        private readonly ILogger<AgentMaashRetenueService> _logger;

        public AgentMaashRetenueService(
            ProsocDbContext db,
            IParametresMetierProvider parametresMetierProvider,
            ILogger<AgentMaashRetenueService> logger)
        {
            _db = db;
            _parametresMetierProvider = parametresMetierProvider;
            _logger = logger;
        }

        public async Task<AgentMaashCouvertureReadDto> GetCouvertureAsync(int agentId, CancellationToken ct = default)
        {
            var options = await _parametresMetierProvider.GetAgentMaashAsync(ct);
            var agent = await _db.Agents
                .AsNoTracking()
                .Include(a => a.CategorieAgent)
                .FirstOrDefaultAsync(a => a.IdAgent == agentId, ct);

            if (agent == null)
                throw new ArgumentException("Agent introuvable.");

            var (annee, mois) = GetPeriodeCourante();
            var eligible = EstCategorieEligible(CategorieAgentLibelleHelper.ResolveCode(agent.CategorieAgent), options);

            var retenue = await _db.RetenuesMaashAgents
                .AsNoTracking()
                .Where(r => r.AgentId == agentId && r.Annee == annee && r.Mois == mois && r.Statut)
                .FirstOrDefaultAsync(ct);

            var beneficiaires = await _db.AgentBeneficiairesMaash
                .AsNoTracking()
                .Where(b => b.AgentId == agentId && b.Statut)
                .Select(b => new AgentBeneficiaireMaashReadDto
                {
                    IdAgentBeneficiaireMaash = b.IdAgentBeneficiaireMaash,
                    AgentId = b.AgentId,
                    NomComplet = b.NomComplet,
                    LienParente = b.LienParente,
                    Adresse = b.Adresse,
                    Statut = b.Statut
                })
                .ToListAsync(ct);

            var produit = await ResolveProduitMaashAsync(options, ct);

            return new AgentMaashCouvertureReadDto
            {
                AgentId = agentId,
                NomCompletAgent = agent.NomComplet,
                EstEligible = eligible,
                CotisationMaashPayeePourPeriodeCourante = retenue != null,
                MontantRetenueMensuelle = options.MontantRetenueUsd,
                DeviseId = options.DeviseId,
                PeriodeCourante = $"{annee}-{mois:D2}",
                DateDerniereRetenue = retenue?.DatePaiement,
                ProduitMaashId = produit?.IdProduit,
                ProduitMaashNom = produit?.Nom,
                BeneficiairesFamille = beneficiaires
            };
        }

        public async Task<AgentMaashRetenueReadDto> AppliquerRetenueMensuelleAsync(
            int agentId,
            AgentMaashRetenueRequestDto? request,
            CancellationToken ct = default)
        {
            var options = await _parametresMetierProvider.GetAgentMaashAsync(ct);
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                var agent = await _db.Agents
                    .Include(a => a.CategorieAgent)
                    .FirstOrDefaultAsync(a => a.IdAgent == agentId, ct);

                if (agent == null)
                    throw new ArgumentException("Agent introuvable.");

                if (!EstCategorieEligible(CategorieAgentLibelleHelper.ResolveCode(agent.CategorieAgent), options))
                {
                    throw new ArgumentException(
                        "Cet agent n'est pas éligible à la retenue MAASH (catégorie non concernée).");
                }

                var annee = request?.Annee ?? GetPeriodeCourante().Annee;
                var mois = request?.Mois ?? GetPeriodeCourante().Mois;

                var dejaPaye = await _db.RetenuesMaashAgents.AnyAsync(
                    r => r.AgentId == agentId && r.Annee == annee && r.Mois == mois && r.Statut, ct);

                if (dejaPaye)
                {
                    throw new ArgumentException(
                        $"La retenue MAASH pour {annee}-{mois:D2} est déjà réglée pour cet agent.");
                }

                var wallet = await _db.WalletsAgents
                    .FirstOrDefaultAsync(w => w.AgentId == agentId && w.Statut, ct);

                if (wallet == null)
                    throw new ArgumentException("Wallet agent introuvable.");

                var montant = options.MontantRetenueUsd;

                if (wallet.SoldeCourant < montant)
                {
                    throw new ArgumentException(
                        $"Solde wallet insuffisant pour la retenue MAASH ({montant} USD). Solde actuel : {wallet.SoldeCourant}.");
                }

                if (wallet.SoldeDisponible < montant)
                {
                    throw new ArgumentException(
                        $"Solde disponible insuffisant pour la retenue MAASH ({montant} USD). Disponible : {wallet.SoldeDisponible}.");
                }

                wallet.SoldeCourant -= montant;
                wallet.SoldeDisponible -= montant;
                wallet.DateModification = DateTime.Now;

                var mouvement = new WalletMouvement
                {
                    WalletId = wallet.IdWalletAgent,
                    DeviseId = wallet.DeviseId > 0 ? wallet.DeviseId : options.DeviseId,
                    Montant = montant,
                    TypeOperation = "DEBIT",
                    Source = "RETENUE_MAASH",
                    Description = $"Retenue à la source MAASH {annee}-{mois:D2} — couverture agent et famille",
                    DateOperation = DateTime.Now,
                    Statut = true,
                    DateCreation = DateTime.Now
                };

                _db.WalletMouvements.Add(mouvement);
                await _db.SaveChangesAsync(ct);

                var retenue = new RetenueMaashAgent
                {
                    AgentId = agentId,
                    Annee = annee,
                    Mois = mois,
                    Montant = montant,
                    DeviseId = options.DeviseId,
                    WalletMouvementId = mouvement.IdWalletMouvement,
                    DatePaiement = DateTime.Now,
                    Statut = true
                };

                _db.RetenuesMaashAgents.Add(retenue);

                if (request?.BeneficiairesFamille != null)
                    await RemplacerBeneficiairesAsync(agentId, request.BeneficiairesFamille, ct);

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                var beneficiaires = await _db.AgentBeneficiairesMaash
                    .AsNoTracking()
                    .Where(b => b.AgentId == agentId && b.Statut)
                    .Select(b => new AgentBeneficiaireMaashReadDto
                    {
                        IdAgentBeneficiaireMaash = b.IdAgentBeneficiaireMaash,
                        AgentId = b.AgentId,
                        NomComplet = b.NomComplet,
                        LienParente = b.LienParente,
                        Adresse = b.Adresse,
                        Statut = b.Statut
                    })
                    .ToListAsync(ct);

                return new AgentMaashRetenueReadDto
                {
                    IdRetenueMaashAgent = retenue.IdRetenueMaashAgent,
                    AgentId = agentId,
                    Annee = annee,
                    Mois = mois,
                    Montant = montant,
                    DeviseId = options.DeviseId,
                    WalletMouvementId = mouvement.IdWalletMouvement,
                    NouveauSoldeWallet = wallet.SoldeCourant,
                    DatePaiement = retenue.DatePaiement,
                    BeneficiairesFamille = beneficiaires
                };
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        private async Task RemplacerBeneficiairesAsync(
            int agentId,
            List<AgentBeneficiaireMaashDto> beneficiaires,
            CancellationToken ct)
        {
            foreach (var b in beneficiaires)
            {
                if (string.IsNullOrWhiteSpace(b.NomComplet))
                    throw new ArgumentException("Nom complet obligatoire pour chaque bénéficiaire MAASH.");

                if (!LienParenteRegles.EstValide(b.LienParente))
                    throw new ArgumentException($"Lien de parenté invalide : {b.LienParente}");

                if (string.IsNullOrWhiteSpace(b.Adresse))
                    throw new ArgumentException("Adresse obligatoire pour chaque bénéficiaire MAASH.");
            }

            var existants = await _db.AgentBeneficiairesMaash
                .Where(x => x.AgentId == agentId)
                .ToListAsync(ct);

            _db.AgentBeneficiairesMaash.RemoveRange(existants);

            _db.AgentBeneficiairesMaash.AddRange(beneficiaires.Select(b => new AgentBeneficiaireMaash
            {
                AgentId = agentId,
                NomComplet = b.NomComplet.Trim(),
                LienParente = LienParenteRegles.Normaliser(b.LienParente),
                Adresse = b.Adresse.Trim(),
                DateCreation = DateTime.Now,
                Statut = true
            }));
        }

        private async Task<ProduitMutuel?> ResolveProduitMaashAsync(AgentMaashOptions options, CancellationToken ct)
        {
            var nom = options.NomProduitMaash.Trim();
            return await _db.ProduitsMutuels
                .AsNoTracking()
                .Where(p => p.Statut &&
                            (p.Nom.ToUpper() == nom.ToUpper() || p.Nom.Contains(nom)))
                .OrderBy(p => p.IdProduit)
                .FirstOrDefaultAsync(ct);
        }

        private static bool EstCategorieEligible(string? codeCategorie, AgentMaashOptions options)
        {
            if (string.IsNullOrWhiteSpace(codeCategorie))
                return false;

            return options.CodesCategoriesEligibles.Contains(
                codeCategorie.Trim().ToUpperInvariant());
        }

        public async Task<bool> DoitExecuterRetenueAutomatiqueAsync(CancellationToken ct = default)
        {
            var options = await _parametresMetierProvider.GetAgentMaashAsync(ct);
            if (!options.RetenueAutomatiqueActivee)
                return false;

            var now = DateTime.Now;
            if (now.Day < options.JourExecution || now.Hour < options.HeureExecution)
                return false;

            if (!options.RetenterEchecsQuotidiennement && now.Day > options.JourExecution)
                return false;

            var (annee, mois) = GetPeriodeCourante();
            return await CompterEligiblesSansRetenueAsync(annee, mois, options, ct) > 0;
        }

        public async Task<AgentMaashBatchResultDto> ExecuterRetenueAutomatiqueAsync(CancellationToken ct = default)
        {
            var options = await _parametresMetierProvider.GetAgentMaashAsync(ct);
            var (annee, mois) = GetPeriodeCourante();
            var agents = await ObtenirAgentsEligiblesAsync(options, ct);
            var result = new AgentMaashBatchResultDto
            {
                Annee = annee,
                Mois = mois,
                NbAgentsEligibles = agents.Count,
                DateExecution = DateTime.Now
            };

            _logger.LogInformation(
                "Retenue MAASH automatique — période {Annee}-{Mois:D2}, {Count} agent(s) éligible(s)",
                annee, mois, agents.Count);

            foreach (var agent in agents)
            {
                try
                {
                    await AppliquerRetenueMensuelleAsync(agent.IdAgent, null, ct);
                    result.NbSucces++;
                }
                catch (ArgumentException ex) when (ex.Message.Contains("déjà réglée"))
                {
                    result.NbDejaPaye++;
                }
                catch (ArgumentException ex)
                {
                    result.NbEchec++;
                    result.Echecs.Add(new AgentMaashBatchEchecDto
                    {
                        AgentId = agent.IdAgent,
                        NomComplet = agent.NomComplet,
                        Message = ex.Message
                    });
                    _logger.LogWarning(
                        "Retenue MAASH échouée pour l'agent {AgentId} ({Nom}) : {Message}",
                        agent.IdAgent, agent.NomComplet, ex.Message);
                }
                catch (Exception ex)
                {
                    result.NbEchec++;
                    result.Echecs.Add(new AgentMaashBatchEchecDto
                    {
                        AgentId = agent.IdAgent,
                        NomComplet = agent.NomComplet,
                        Message = ex.Message
                    });
                    _logger.LogError(ex,
                        "Erreur inattendue retenue MAASH agent {AgentId}", agent.IdAgent);
                }
            }

            _logger.LogInformation(
                "Retenue MAASH terminée — succès: {Ok}, déjà payé: {Skip}, échecs: {Ko}",
                result.NbSucces, result.NbDejaPaye, result.NbEchec);

            return result;
        }

        private async Task<List<Agent>> ObtenirAgentsEligiblesAsync(AgentMaashOptions options, CancellationToken ct)
        {
            var codes = options.CodesCategoriesEligibles
                .Select(c => c.Trim().ToUpperInvariant())
                .ToList();

            return await _db.Agents
                .AsNoTracking()
                .Include(a => a.CategorieAgent)
                .Where(a => a.Statut
                    && a.CategorieAgent != null
                    && codes.Contains(a.CategorieAgent.Code.ToUpper()))
                .Where(a => _db.WalletsAgents.Any(w => w.AgentId == a.IdAgent && w.Statut))
                .OrderBy(a => a.IdAgent)
                .ToListAsync(ct);
        }

        private async Task<int> CompterEligiblesSansRetenueAsync(
            int annee,
            int mois,
            AgentMaashOptions options,
            CancellationToken ct)
        {
            var codes = options.CodesCategoriesEligibles
                .Select(c => c.Trim().ToUpperInvariant())
                .ToList();

            return await _db.Agents
                .Where(a => a.Statut
                    && a.CategorieAgent != null
                    && codes.Contains(a.CategorieAgent.Code.ToUpper())
                    && _db.WalletsAgents.Any(w => w.AgentId == a.IdAgent && w.Statut)
                    && !_db.RetenuesMaashAgents.Any(r =>
                        r.AgentId == a.IdAgent && r.Annee == annee && r.Mois == mois && r.Statut))
                .CountAsync(ct);
        }

        private static (int Annee, int Mois) GetPeriodeCourante()
        {
            var now = DateTime.Now;
            return (now.Year, now.Month);
        }
    }
}
