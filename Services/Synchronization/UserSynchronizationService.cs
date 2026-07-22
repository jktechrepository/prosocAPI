using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Synchronization
{
    public interface IUserSynchronizationService
    {
        Task SynchronizeFromAgentAsync(int agentId, CancellationToken ct = default);
        Task SynchronizeFromAffilieAsync(int affilieId, CancellationToken ct = default);
        Task SynchronizeFromUtilisateurAsync(int utilisateurId, CancellationToken ct = default);
        Task<List<SynchronizationConflict>> DetectConflictsAsync(CancellationToken ct = default);
        Task<SynchronizationMetrics> GetSynchronizationMetricsAsync(CancellationToken ct = default);
    }

    public class UserSynchronizationService : IUserSynchronizationService
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<UserSynchronizationService> _logger;

        public UserSynchronizationService(ProsocDbContext db, ILogger<UserSynchronizationService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // 🔄 SYNCHRONISATION AGENT → UTILISATEUR
        public async Task SynchronizeFromAgentAsync(int agentId, CancellationToken ct = default)
        {
            _logger.LogInformation("Début synchronisation Agent → Utilisateur pour AgentId: {AgentId}", agentId);

            try
            {
                // Récupérer l'agent et son utilisateur associé
                var agent = await _db.Agents
                    .FirstOrDefaultAsync(a => a.IdAgent == agentId, ct);

                if (agent == null)
                {
                    _logger.LogWarning("Agent {AgentId} non trouvé", agentId);
                    return;
                }

                var utilisateur = await _db.Utilisateurs
                    .FirstOrDefaultAsync(u => u.AgentId == agentId, ct);

                if (utilisateur == null)
                {
                    _logger.LogWarning("Aucun utilisateur trouvé pour l'agent {AgentId}", agentId);
                    return;
                }

                // 🔄 GESTION DES CONFLITS : Last Write Wins
                // Utiliser DateCreation de l'utilisateur comme référence
                if (agent.DateModification.HasValue && 
                    agent.DateModification.Value <= utilisateur.DateCreation)
                {
                    _logger.LogInformation("Agent {AgentId} non synchronisé : modification plus ancienne que l'utilisateur", agentId);
                    return;
                }

                // 🔄 SYNCHRONISATION DES CHAMPS
                var hasChanges = false;

                if (utilisateur.NomUtilisateur != agent.NomComplet)
                {
                    utilisateur.NomUtilisateur = agent.NomComplet;
                    hasChanges = true;
                }

                if (utilisateur.EmailUtilisateur != agent.EmailAgent)
                {
                    utilisateur.EmailUtilisateur = agent.EmailAgent;
                    hasChanges = true;
                }

                var agentPhone = PhoneNumberHelper.NormalizeForStorage(agent.Phone) ?? agent.Phone?.Trim();
                if (utilisateur.PhoneUtilisateur != agentPhone)
                {
                    utilisateur.PhoneUtilisateur = agentPhone;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    await _db.SaveChangesAsync(ct);
                    _logger.LogInformation("Synchronisation Agent → Utilisateur réussie pour AgentId: {AgentId}", agentId);
                }
                else
                {
                    _logger.LogInformation("Aucune modification détectée pour l'agent {AgentId}", agentId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la synchronisation Agent → Utilisateur pour AgentId: {AgentId}", agentId);
                throw;
            }
        }

        // 🔄 SYNCHRONISATION AFFILIÉ → UTILISATEUR
        public async Task SynchronizeFromAffilieAsync(int affilieId, CancellationToken ct = default)
        {
            _logger.LogInformation("Début synchronisation Affilie → Utilisateur pour AffilieId: {AffilieId}", affilieId);

            try
            {
                // Récupérer l'affilié et son utilisateur associé
                var affilie = await _db.Affilies
                    .FirstOrDefaultAsync(a => a.IdAffilie == affilieId, ct);

                if (affilie == null)
                {
                    _logger.LogWarning("Affilie {AffilieId} non trouvé", affilieId);
                    return;
                }

                var utilisateur = await _db.Utilisateurs
                    .FirstOrDefaultAsync(u => u.AffilieId == affilieId, ct);

                if (utilisateur == null)
                {
                    _logger.LogWarning("Aucun utilisateur trouvé pour l'affilié {AffilieId}", affilieId);
                    return;
                }

                // 🔄 GESTION DES CONFLITS : Last Write Wins
                // Utiliser DateCreation de l'utilisateur comme référence
                if (affilie.DateModification.HasValue && 
                    affilie.DateModification.Value <= utilisateur.DateCreation)
                {
                    _logger.LogInformation("Affilie {AffilieId} non synchronisé : modification plus ancienne que l'utilisateur", affilieId);
                    return;
                }

                // 🔄 SYNCHRONISATION DES CHAMPS
                var hasChanges = false;

                if (utilisateur.NomUtilisateur != affilie.NomComplet)
                {
                    utilisateur.NomUtilisateur = affilie.NomComplet;
                    hasChanges = true;
                }

                if (utilisateur.EmailUtilisateur != affilie.EmailAffilie)
                {
                    utilisateur.EmailUtilisateur = affilie.EmailAffilie;
                    hasChanges = true;
                }

                var affiliePhone = PhoneNumberHelper.NormalizeForStorage(affilie.Telephone)
                    ?? affilie.Telephone?.Trim();
                if (utilisateur.PhoneUtilisateur != affiliePhone)
                {
                    utilisateur.PhoneUtilisateur = affiliePhone;
                    hasChanges = true;
                }

                if (utilisateur.DefaultUsername != affilie.CodeAdhesion)
                {
                    utilisateur.DefaultUsername = affilie.CodeAdhesion;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    await _db.SaveChangesAsync(ct);
                    _logger.LogInformation("Synchronisation Affilie → Utilisateur réussie pour AffilieId: {AffilieId}", affilieId);
                }
                else
                {
                    _logger.LogInformation("Aucune modification détectée pour l'affilié {AffilieId}", affilieId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la synchronisation Affilie → Utilisateur pour AffilieId: {AffilieId}", affilieId);
                throw;
            }
        }

        // 🔄 SYNCHRONISATION UTILISATEUR → AGENT/AFFILIÉ
        public async Task SynchronizeFromUtilisateurAsync(int utilisateurId, CancellationToken ct = default)
        {
            _logger.LogInformation("Début synchronisation Utilisateur → Agent/Affilie pour UtilisateurId: {UtilisateurId}", utilisateurId);

            try
            {
                var utilisateur = await _db.Utilisateurs
                    .FirstOrDefaultAsync(u => u.IdUtilisateur == utilisateurId, ct);

                if (utilisateur == null)
                {
                    _logger.LogWarning("Utilisateur {UtilisateurId} non trouvé", utilisateurId);
                    return;
                }

                var hasChanges = false;

                // 🔄 SYNCHRONISATION VERS AGENT
                if (utilisateur.AgentId.HasValue)
                {
                    var agent = await _db.Agents
                        .FirstOrDefaultAsync(a => a.IdAgent == utilisateur.AgentId.Value, ct);
                    
                    if (agent != null)
                    {
                        // 🔄 GESTION DES CONFLITS : Last Write Wins
                        // Utiliser DateCreation de l'utilisateur comme référence
                        if (!agent.DateModification.HasValue || 
                            agent.DateModification.Value <= utilisateur.DateCreation)
                        {
                            if (agent.NomComplet != utilisateur.NomUtilisateur)
                            {
                                agent.NomComplet = utilisateur.NomUtilisateur;
                                hasChanges = true;
                            }

                            if (agent.EmailAgent != utilisateur.EmailUtilisateur)
                            {
                                agent.EmailAgent = utilisateur.EmailUtilisateur;
                                hasChanges = true;
                            }

                            if (agent.Phone != utilisateur.PhoneUtilisateur)
                            {
                                agent.Phone = utilisateur.PhoneUtilisateur;
                                hasChanges = true;
                            }

                            if (hasChanges)
                            {
                                agent.DateModification = DateTime.Now;
                            }
                        }
                    }
                }

                // 🔄 SYNCHRONISATION VERS AFFILIÉ
                if (utilisateur.AffilieId.HasValue)
                {
                    var affilie = await _db.Affilies
                        .FirstOrDefaultAsync(a => a.IdAffilie == utilisateur.AffilieId.Value, ct);
                    
                    if (affilie != null)
                    {
                        // 🔄 GESTION DES CONFLITS : Last Write Wins
                        // Utiliser DateCreation de l'utilisateur comme référence
                        if (!affilie.DateModification.HasValue || 
                            affilie.DateModification.Value <= utilisateur.DateCreation)
                        {
                            if (affilie.NomComplet != utilisateur.NomUtilisateur)
                            {
                                affilie.NomComplet = utilisateur.NomUtilisateur;
                                hasChanges = true;
                            }

                            if (affilie.EmailAffilie != utilisateur.EmailUtilisateur)
                            {
                                affilie.EmailAffilie = utilisateur.EmailUtilisateur;
                                hasChanges = true;
                            }

                            if (affilie.Telephone != utilisateur.PhoneUtilisateur)
                            {
                                affilie.Telephone = utilisateur.PhoneUtilisateur;
                                hasChanges = true;
                            }

                            if (hasChanges)
                            {
                                affilie.DateModification = DateTime.Now;
                            }
                        }
                    }
                }

                if (hasChanges)
                {
                    await _db.SaveChangesAsync(ct);
                    _logger.LogInformation("Synchronisation Utilisateur → Agent/Affilie réussie pour UtilisateurId: {UtilisateurId}", utilisateurId);
                }
                else
                {
                    _logger.LogInformation("Aucune modification détectée pour l'utilisateur {UtilisateurId}", utilisateurId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la synchronisation Utilisateur → Agent/Affilie pour UtilisateurId: {UtilisateurId}", utilisateurId);
                throw;
            }
        }

        // 🔍 DÉTECTION DES CONFLITS
        public async Task<List<SynchronizationConflict>> DetectConflictsAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Détection des conflits de synchronisation");

            var conflicts = new List<SynchronizationConflict>();

            // 🔄 CONFLITS AGENT ↔ UTILISATEUR
            var agentUsers = await (from u in _db.Utilisateurs
                                 join a in _db.Agents on u.AgentId equals a.IdAgent into ua
                                 from a in ua.DefaultIfEmpty()
                                 where u.AgentId.HasValue
                                 select new { u, a })
                .ToListAsync(ct);

            foreach (var pair in agentUsers)
            {
                if (pair.a == null) continue;
                
                var conflictsList = new List<string>();

                if (pair.u.NomUtilisateur != pair.a.NomComplet)
                    conflictsList.Add($"Nom: Utilisateur='{pair.u.NomUtilisateur}' vs Agent='{pair.a.NomComplet}'");

                if (pair.u.EmailUtilisateur != pair.a.EmailAgent)
                    conflictsList.Add($"Email: Utilisateur='{pair.u.EmailUtilisateur}' vs Agent='{pair.a.EmailAgent}'");

                if (pair.u.PhoneUtilisateur != pair.a.Phone)
                    conflictsList.Add($"Phone: Utilisateur='{pair.u.PhoneUtilisateur}' vs Agent='{pair.a.Phone}'");

                if (conflictsList.Any())
                {
                    conflicts.Add(new SynchronizationConflict
                    {
                        Type = "Agent-Utilisateur",
                        EntityId = pair.a.IdAgent,
                        EntityType = "Agent",
                        Conflicts = conflictsList,
                        LastModificationAgent = pair.a.DateModification,
                        LastModificationUtilisateur = pair.u.DateCreation
                    });
                }
            }

            // 🔄 CONFLITS AFFILIÉ ↔ UTILISATEUR
            var affilieUsers = await (from u in _db.Utilisateurs
                                   join a in _db.Affilies on u.AffilieId equals a.IdAffilie into ua
                                   from a in ua.DefaultIfEmpty()
                                   where u.AffilieId.HasValue
                                   select new { u, a })
                .ToListAsync(ct);

            foreach (var pair in affilieUsers)
            {
                if (pair.a == null) continue;
                
                var conflictsList = new List<string>();

                if (pair.u.NomUtilisateur != pair.a.NomComplet)
                    conflictsList.Add($"Nom: Utilisateur='{pair.u.NomUtilisateur}' vs Affilie='{pair.a.NomComplet}'");

                if (pair.u.EmailUtilisateur != pair.a.EmailAffilie)
                    conflictsList.Add($"Email: Utilisateur='{pair.u.EmailUtilisateur}' vs Affilie='{pair.a.EmailAffilie}'");

                if (pair.u.PhoneUtilisateur != pair.a.Telephone)
                    conflictsList.Add($"Phone: Utilisateur='{pair.u.PhoneUtilisateur}' vs Affilie='{pair.a.Telephone}'");

                if (conflictsList.Any())
                {
                    conflicts.Add(new SynchronizationConflict
                    {
                        Type = "Affilie-Utilisateur",
                        EntityId = pair.a.IdAffilie,
                        EntityType = "Affilie",
                        Conflicts = conflictsList,
                        LastModificationAffilie = pair.a.DateModification,
                        LastModificationUtilisateur = pair.u.DateCreation
                    });
                }
            }

            _logger.LogInformation("Détection des conflits terminée : {Count} conflits trouvés", conflicts.Count);
            return conflicts;
        }

        // 📊 MÉTRIQUES DE SYNCHRONISATION
        public async Task<SynchronizationMetrics> GetSynchronizationMetricsAsync(CancellationToken ct = default)
        {
            var metrics = new SynchronizationMetrics();

            // 📊 STATISTIQUES AGENTS
            metrics.TotalAgents = await _db.Agents.CountAsync(ct);
            metrics.AgentsWithUser = await _db.Utilisateurs
                .Where(u => u.AgentId.HasValue)
                .CountAsync(ct);
            // Simplifié : pas de champ de synchronisation spécifique
            metrics.AgentsSynchronized = metrics.AgentsWithUser;

            // 📊 STATISTIQUES AFFILIÉS
            metrics.TotalAffilies = await _db.Affilies.CountAsync(ct);
            metrics.AffiliesWithUser = await _db.Utilisateurs
                .Where(u => u.AffilieId.HasValue)
                .CountAsync(ct);
            // Simplifié : pas de champ de synchronisation spécifique
            metrics.AffiliesSynchronized = metrics.AffiliesWithUser;

            // 📊 STATISTIQUES UTILISATEURS
            metrics.TotalUsers = await _db.Utilisateurs.CountAsync(ct);
            metrics.AgentUsers = await _db.Utilisateurs
                .Where(u => u.AgentId.HasValue)
                .CountAsync(ct);
            metrics.AffilieUsers = await _db.Utilisateurs
                .Where(u => u.AffilieId.HasValue)
                .CountAsync(ct);

            // 📊 DERNIÈRES SYNCHRONISATIONS
            // Simplifié : utiliser DateModification comme référence
            metrics.LastSynchronizationAgent = await _db.Agents
                .Where(a => a.DateModification.HasValue)
                .MaxAsync(a => a.DateModification, ct);

            metrics.LastSynchronizationAffilie = await _db.Affilies
                .Where(a => a.DateModification.HasValue)
                .MaxAsync(a => a.DateModification, ct);

            return metrics;
        }
    }

    // 📊 MODÈLES DE SUPPORT
    public class SynchronizationConflict
    {
        public string Type { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public List<string> Conflicts { get; set; } = new();
        public DateTime? LastModificationAgent { get; set; }
        public DateTime? LastModificationAffilie { get; set; }
        public DateTime? LastModificationUtilisateur { get; set; }
    }

    public class SynchronizationMetrics
    {
        // 📊 STATISTIQUES AGENTS
        public int TotalAgents { get; set; }
        public int AgentsWithUser { get; set; }
        public int AgentsSynchronized { get; set; }
        public double AgentSynchronizationRate => TotalAgents > 0 ? (double)AgentsSynchronized / TotalAgents * 100 : 0;

        // 📊 STATISTIQUES AFFILIÉS
        public int TotalAffilies { get; set; }
        public int AffiliesWithUser { get; set; }
        public int AffiliesSynchronized { get; set; }
        public double AffilieSynchronizationRate => TotalAffilies > 0 ? (double)AffiliesSynchronized / TotalAffilies * 100 : 0;

        // 📊 STATISTIQUES UTILISATEURS
        public int TotalUsers { get; set; }
        public int AgentUsers { get; set; }
        public int AffilieUsers { get; set; }

        // 📊 DERNIÈRES SYNCHRONISATIONS
        public DateTime? LastSynchronizationAgent { get; set; }
        public DateTime? LastSynchronizationAffilie { get; set; }
    }
}
