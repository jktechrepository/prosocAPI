using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Services.Repositories;
using BCrypt.Net;

namespace ProsocAPI.Services
{
    public class AgentService : IAgentRepository
    {
        private readonly ProsocDbContext _db;
        private readonly IMatriculeGeneratorService _matriculeGenerator;
        private readonly IEmailService _emailService;
        private readonly ITerritorialEncadrementService _territorialEncadrement;
        private readonly ILogger<AgentService> _logger;

        public AgentService(
            ProsocDbContext db,
            IMatriculeGeneratorService matriculeGenerator,
            IEmailService emailService,
            ITerritorialEncadrementService territorialEncadrement,
            ILogger<AgentService> logger)
        {
            _db = db;
            _matriculeGenerator = matriculeGenerator;
            _emailService = emailService;
            _territorialEncadrement = territorialEncadrement;
            _logger = logger;
        }

        public async Task<List<Agent>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Agents
                .Include(a => a.Zone)
                .Include(a => a.CategorieAgent)
                .AsNoTracking()
                .OrderBy(x => x.Matricule)
                .ToListAsync(ct);
        }

        public async Task<Agent?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Agents
                .Include(a => a.Zone)
                .Include(a => a.CategorieAgent)
                .Include(a => a.Wallets).ThenInclude(w => w.Devise)
                .Include(a => a.WalletVirtuel)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdAgent == id, ct);
        }

        public async Task<Agent> CreateAsync(Agent entity, CancellationToken ct = default)
        {
            // Générer automatiquement le matricule si non fourni
            if (string.IsNullOrWhiteSpace(entity.Matricule))
            {
                if (!entity.CategorieAgentId.HasValue)
                    throw new ArgumentException("CategorieAgentId est requis pour générer automatiquement le matricule");

                entity.Matricule = await _matriculeGenerator.GenerateMatriculeAsync(entity.CategorieAgentId.Value, ct);
            }

            if (!string.IsNullOrWhiteSpace(entity.Phone))
                entity.Phone = PhoneNumberHelper.NormalizeForStorage(entity.Phone) ?? entity.Phone.Trim();

            entity.EmailAgent = NormalizeEmailAgent(entity.EmailAgent);
            await EnsureUniqueEmailAgentAsync(entity.EmailAgent, excludeAgentId: null, ct);

            _db.Agents.Add(entity);
            await _db.SaveChangesAsync(ct);
            
            // 🆕 CRÉATION AUTOMATIQUE DE L'UTILISATEUR ASSOCIÉ
            await CreateAssociatedUserAsync(entity, ct);
            
            // 🆕 CRÉATION AUTOMATIQUE DU WALLET ASSOCIÉ
            await CreateAssociatedWalletAsync(entity, ct);
            
            // 🆕 CRÉATION AUTOMATIQUE DU WALLET VIRTUEL ASSOCIÉ
            await CreateAssociatedWalletVirtuelAsync(entity, ct);
            
            _logger.LogInformation("Agent créé avec succès: {AgentId} - {NomComplet}", entity.IdAgent, entity.NomComplet);
            
            return entity;
        }

        private async Task CreateAssociatedUserAsync(Agent agent, CancellationToken ct)
        {
            // Vérifier si l'utilisateur n'existe pas déjà
            var existingUser = await _db.Utilisateurs
                .AnyAsync(u => u.AgentId == agent.IdAgent, ct);
            
            if (!existingUser)
            {
                _logger.LogInformation("Création du compte utilisateur pour l'agent: {AgentId}", agent.IdAgent);
                
                // 🆕 Récupérer le rôle dynamiquement basé sur RoleAgent
                var agentRole = await GetRoleForAgentAsync(agent, ct);
                
                if (agentRole != null)
                {
                    // 🎯 LOGIQUE D'EMAIL ET USERNAME
                    string emailUtilisateur = null;
                    string nomUtilisateur;
                    string defaultUsername;
                    string phoneUtilisateur = PhoneNumberHelper.NormalizeForStorage(agent.Phone) ?? agent.Phone.Trim();
                    
                    if (!string.IsNullOrWhiteSpace(agent.EmailAgent))
                    {
                        emailUtilisateur = agent.EmailAgent;
                        nomUtilisateur = agent.NomComplet;
                        defaultUsername = agent.Matricule;
                    }
                    else
                    {
                        nomUtilisateur = agent.NomComplet; 
                        defaultUsername = agent.Matricule;
                    }
                    
                    // Extraire nom et prénom du nom complet
                    var noms = agent.NomComplet.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var nom = noms.Length > 0 ? noms[0] : agent.NomComplet;
                    var prenom = noms.Length > 1 ? string.Join(" ", noms.Skip(1)) : "";

                    var utilisateur = new Utilisateur
                    {
                        NomUtilisateur = nomUtilisateur,
                        EmailUtilisateur = emailUtilisateur,
                        DefaultUsername = defaultUsername,
                        PhoneUtilisateur = phoneUtilisateur,
                        MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        AgentId = agent.IdAgent,
                        RoleId = agentRole.IdRole, // 🆕 Utilisation directe de l'ID du rôle dynamique
                        Statut = true,
                        DoitChangerMotDePasse = true, // Forcer le changement au premier login
                        DateCreation = DateTime.Now
                    };
                    
                    _db.Utilisateurs.Add(utilisateur);
                    await _db.SaveChangesAsync(ct);
                    
                    _logger.LogInformation("Utilisateur créé avec succès: {UserId} - {Username} (Rôle: {Role})", 
                        utilisateur.IdUtilisateur, utilisateur.NomUtilisateur, agentRole.Nom);
                    
                    // Attribuer le rôle à l'utilisateur
                    var userRole = new UserRole
                    {
                        UtilisateurId = utilisateur.IdUtilisateur,
                        RoleId = agentRole.IdRole,
                        IsPrimary = true,
                        DateAttribution = DateTime.Now
                    };
                    _db.UserRoles.Add(userRole);
                    await _db.SaveChangesAsync(ct);
                    
                    _logger.LogInformation("Rôle '{Role}' attribué à l'utilisateur: {UserId}", agentRole.Nom, utilisateur.IdUtilisateur);
                    
                    // 🆕 ENVOI DE L'EMAIL DE BIENVENUE
                    try
                    {
                        var phoneNumber = !string.IsNullOrWhiteSpace(agent.Phone) 
                            ? agent.Phone 
                            : "+243 000 000 000"; // Valeur par défaut si pas de téléphone
                        
                        await _emailService.SendWelcomeEmailAsync(
                            toEmail: emailUtilisateur,
                            username: nomUtilisateur,
                            password: "123456", // Mot de passe par défaut
                            phoneNumber: phoneNumber,
                            roleName: agentRole.Nom
                        );
                        
                        _logger.LogInformation("Email de bienvenue envoyé avec succès à {Email} pour l'agent {AgentId}", 
                            emailUtilisateur, agent.IdAgent);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Échec de l'envoi de l'email de bienvenue à {Email} pour l'agent {AgentId}", 
                            emailUtilisateur, agent.IdAgent);
                        // Ne pas échouer la création de l'agent si l'email échoue
                    }
                }
                else
                {
                    _logger.LogError("Aucun rôle valide trouvé pour l'agent: {AgentId} - RoleAgent: '{RoleAgent}'", 
                        agent.IdAgent, agent.RoleAgent);
                    throw new InvalidOperationException($"Impossible de créer le compte utilisateur : aucun rôle valide trouvé pour '{agent.RoleAgent}'");
                }
            }
            else
            {
                _logger.LogInformation("Un compte utilisateur existe déjà pour l'agent: {AgentId}", agent.IdAgent);
            }
        }

        /// <summary>
        /// Récupère le rôle approprié pour un agent en fonction de son RoleAgent
        /// avec gestion des erreurs et fallback
        /// </summary>
        private async Task<Role?> GetRoleForAgentAsync(Agent agent, CancellationToken ct)
        {
            // 1. Valider que RoleAgent est fourni
            if (string.IsNullOrWhiteSpace(agent.RoleAgent))
            {
                _logger.LogWarning("RoleAgent non spécifié pour l'agent {AgentId}, utilisation du rôle par défaut 'Agent'", agent.IdAgent);
                return await GetDefaultRoleAsync(ct);
            }

            // 2. Rechercher le rôle correspondant exactement
            var role = await _db.Roles
                .FirstOrDefaultAsync(r => r.Nom == agent.RoleAgent, ct);
            
            if (role != null)
            {
                _logger.LogInformation("Rôle '{RoleAgent}' trouvé pour l'agent {AgentId}", agent.RoleAgent, agent.IdAgent);
                return role;
            }

            // 3. Fallback sur le rôle par défaut si le rôle spécifié n'existe pas
            _logger.LogWarning("Rôle '{RoleAgent}' non trouvé dans la base de données, utilisation du rôle par défaut 'Agent' pour l'agent {AgentId}", 
                agent.RoleAgent, agent.IdAgent);
            
            return await GetDefaultRoleAsync(ct);
        }

        /// <summary>
        /// Récupère le rôle par défaut "Agent"
        /// </summary>
        private async Task<Role?> GetDefaultRoleAsync(CancellationToken ct)
        {
            var defaultRole = await _db.Roles
                .FirstOrDefaultAsync(r => r.Nom == "Agent", ct);

            if (defaultRole == null)
            {
                defaultRole = await _db.Roles
                    .FirstOrDefaultAsync(r => r.Nom == "Agent (AT)", ct);
            }
            
            if (defaultRole == null)
            {
                _logger.LogError("Rôle par défaut 'Agent' non trouvé dans la base de données");
            }
            
            return defaultRole;
        }

        public async Task<List<Affilie>> GetAffiliesByAgentAsync(int agentId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération des affiliés pour l'agent {AgentId}", agentId);

                // Récupérer les adhésions de l'agent avec les affiliés actifs uniquement
                var adhesions = await _db.Adhesions
                    .Include(a => a.Affilie)
                    .Include(a => a.TypeAdhesion)
                    .Where(a => a.AgentId == agentId && a.Statut == true && a.Affilie.Statut == true)
                    .OrderByDescending(a => a.DateCreation)
                    .AsNoTracking()
                    .ToListAsync(ct);

                var affilies = adhesions.Select(a => a.Affilie).ToList();

                _logger.LogInformation("Trouvé {Count} affiliés actifs pour l'agent {AgentId}", affilies.Count, agentId);
                return affilies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des affiliés pour l'agent {AgentId}", agentId);
                throw;
            }
        }

        private async Task CreateAssociatedWalletAsync(Agent agent, CancellationToken ct)
        {
            var principale = await _db.Devises
                .FirstOrDefaultAsync(d => d.EstDevisePrincipale && d.Statut, ct);

            if (principale == null)
            {
                _logger.LogWarning("Création wallet agent {AgentId} ignorée : devise principale absente.", agent.IdAgent);
                return;
            }

            var existingWallet = await _db.WalletsAgents
                .AnyAsync(w => w.AgentId == agent.IdAgent && w.DeviseId == principale.IdDevise, ct);

            if (!existingWallet)
            {
                _db.WalletsAgents.Add(new WalletAgent
                {
                    AgentId = agent.IdAgent,
                    DeviseId = principale.IdDevise,
                    RowVersion = new byte[] { 0 },
                    SoldeCourant = 0,
                    SoldeDisponible = 0,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                await _db.SaveChangesAsync(ct);
            }
        }

        private async Task CreateAssociatedWalletVirtuelAsync(Agent agent, CancellationToken ct)
        {
            var principale = await _db.Devises
                .FirstOrDefaultAsync(d => d.EstDevisePrincipale && d.Statut, ct);

            if (principale == null)
            {
                _logger.LogWarning(
                    "Création wallet virtuel agent {AgentId} ignorée : devise principale absente.",
                    agent.IdAgent);
                return;
            }

            var existingWalletVirtuel = await _db.WalletsVirtuelsAgents
                .AnyAsync(w => w.AgentId == agent.IdAgent, ct);
            
            if (!existingWalletVirtuel)
            {
                var walletVirtuel = new WalletVirtuelAgent
                {
                    AgentId = agent.IdAgent,
                    DeviseId = principale.IdDevise,
                    SoldeVirtuel = 0,
                    Statut = true,
                    DateCreation = DateTime.Now
                };
                
                _db.WalletsVirtuelsAgents.Add(walletVirtuel);
                await _db.SaveChangesAsync(ct);
            }
        }

        public async Task<Agent?> AffecterZoneSocialeAsync(int agentId, int? zoneSocialeId, CancellationToken ct = default)
        {
            var existing = await _db.Agents.FirstOrDefaultAsync(x => x.IdAgent == agentId, ct);
            if (existing == null)
                return null;

            if (zoneSocialeId.HasValue)
            {
                var zone = await _db.ZonesSociales
                    .AsNoTracking()
                    .FirstOrDefaultAsync(z => z.IdZoneSociale == zoneSocialeId.Value, ct);

                if (zone == null)
                    throw new KeyNotFoundException($"Zone sociale {zoneSocialeId.Value} introuvable.");

                if (!zone.Statut)
                    throw new ArgumentException($"La zone sociale '{zone.Nom}' est inactive.");
            }

            existing.ZoneSocialeId = zoneSocialeId;
            existing.DateModification = DateTime.Now;

            await _db.SaveChangesAsync(ct);

            return await _db.Agents
                .Include(a => a.Zone)
                .Include(a => a.CategorieAgent)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdAgent == agentId, ct);
        }

        public async Task<Agent?> UpdateAsync(int id, Agent entity, CancellationToken ct = default)
        {
            var existing = await _db.Agents.FirstOrDefaultAsync(x => x.IdAgent == id, ct);
            if (existing == null)
                return null;

            var wasActive = existing.Statut;

            var emailNormalized = NormalizeEmailAgent(entity.EmailAgent);
            await EnsureUniqueEmailAgentAsync(emailNormalized, excludeAgentId: id, ct);

            existing.NomComplet = entity.NomComplet;
            existing.Matricule = entity.Matricule;
            existing.Phone = !string.IsNullOrWhiteSpace(entity.Phone)
                ? PhoneNumberHelper.NormalizeForStorage(entity.Phone) ?? entity.Phone.Trim()
                : entity.Phone;
            existing.EmailAgent = emailNormalized;
            existing.Fonction = entity.Fonction;
            existing.RoleAgent = entity.RoleAgent;
            existing.PhotoUrl = entity.PhotoUrl;
            existing.ZoneSocialeId = entity.ZoneSocialeId;
            existing.CategorieAgentId = entity.CategorieAgentId;
            existing.Statut = entity.Statut;
            existing.DateModification = entity.DateModification;

            await _db.SaveChangesAsync(ct);

            if (wasActive && !existing.Statut)
            {
                await _territorialEncadrement.ReleaseTitularitesForAgentAsync(id, ct);
                _logger.LogInformation(
                    "Titularités territoriales libérées pour l'agent {AgentId} (désactivation).",
                    id);
            }

            return existing;
        }

        private static string? NormalizeEmailAgent(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return email.Trim().ToLowerInvariant();
        }

        private async Task EnsureUniqueEmailAgentAsync(string? emailNormalized, int? excludeAgentId, CancellationToken ct)
        {
            if (emailNormalized == null)
                return;

            var exists = await _db.Agents
                .AsNoTracking()
                .AnyAsync(a =>
                    a.EmailAgent != null
                    && a.EmailAgent.Trim().ToLower() == emailNormalized
                    && (!excludeAgentId.HasValue || a.IdAgent != excludeAgentId.Value),
                    ct);

            if (exists)
                throw new ArgumentException($"EmailAgent '{emailNormalized}' est déjà utilisé par un autre agent.");
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.Agents.FirstOrDefaultAsync(x => x.IdAgent == id, ct);
            if (existing == null)
                return false;

            await _territorialEncadrement.ReleaseTitularitesForAgentAsync(id, ct);

            _db.Agents.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
