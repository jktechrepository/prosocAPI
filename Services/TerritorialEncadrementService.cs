using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class TerritorialEncadrementService : ITerritorialEncadrementService
    {
        private readonly ProsocDbContext _db;
        private readonly IUtilisateurRepository _utilisateurRepository;
        private readonly ILogger<TerritorialEncadrementService> _logger;

        public TerritorialEncadrementService(
            ProsocDbContext db,
            IUtilisateurRepository utilisateurRepository,
            ILogger<TerritorialEncadrementService> logger)
        {
            _db = db;
            _utilisateurRepository = utilisateurRepository;
            _logger = logger;
        }

        public async Task<TerritorialAffectationResultDto> AssignChefEquipeAsync(
            int zoneId,
            int agentId,
            int? assignedByUserId = null,
            CancellationToken ct = default)
        {
            var zone = await _db.ZonesSociales.FirstOrDefaultAsync(z => z.IdZoneSociale == zoneId, ct)
                ?? throw new InvalidOperationException("Zone sociale introuvable.");

            var agent = await _db.Agents.FirstOrDefaultAsync(a => a.IdAgent == agentId && a.Statut, ct)
                ?? throw new InvalidOperationException("Agent introuvable ou inactif.");

            if (agent.ZoneSocialeId != zoneId)
                throw new InvalidOperationException("L'agent doit être affecté à cette zone sociale.");

            var previousAgentId = zone.ChefEquipeAgentId;
            string? previousNom = null;
            if (previousAgentId.HasValue && previousAgentId != agentId)
            {
                previousNom = await GetAgentNomAsync(previousAgentId.Value, ct);
                await RevokeChefEquipeRoleIfNotTitulaireAsync(previousAgentId.Value, ct);
            }

            var autresZones = await _db.ZonesSociales
                .Where(z => z.ChefEquipeAgentId == agentId && z.IdZoneSociale != zoneId)
                .ToListAsync(ct);
            foreach (var autre in autresZones)
                autre.ChefEquipeAgentId = null;

            zone.ChefEquipeAgentId = agentId;
            await _db.SaveChangesAsync(ct);

            await GrantRoleToAgentAsync(agentId, ChefEquipeZoneScopeHelper.RoleName, assignedByUserId, ct);

            _logger.LogInformation(
                "Chef d'équipe {AgentId} affecté à la zone {ZoneId} (précédent : {PreviousAgentId})",
                agentId, zoneId, previousAgentId);

            return new TerritorialAffectationResultDto
            {
                TerritoryId = zoneId,
                PreviousAgentId = previousAgentId == agentId ? null : previousAgentId,
                PreviousAgentNom = previousNom,
                NewAgentId = agentId,
                NewAgentNom = agent.NomComplet
            };
        }

        public async Task<TerritorialAffectationResultDto> ClearChefEquipeAsync(
            int zoneId,
            int? assignedByUserId = null,
            CancellationToken ct = default)
        {
            var zone = await _db.ZonesSociales.FirstOrDefaultAsync(z => z.IdZoneSociale == zoneId, ct)
                ?? throw new InvalidOperationException("Zone sociale introuvable.");

            var previousAgentId = zone.ChefEquipeAgentId;
            string? previousNom = null;
            if (previousAgentId.HasValue)
            {
                previousNom = await GetAgentNomAsync(previousAgentId.Value, ct);
                zone.ChefEquipeAgentId = null;
                await _db.SaveChangesAsync(ct);
                await RevokeChefEquipeRoleIfNotTitulaireAsync(previousAgentId.Value, ct);
            }

            return new TerritorialAffectationResultDto
            {
                TerritoryId = zoneId,
                PreviousAgentId = previousAgentId,
                PreviousAgentNom = previousNom,
                NewAgentId = null,
                NewAgentNom = null
            };
        }

        public async Task<TerritorialAffectationResultDto> AssignSuperviseurAsync(
            int communeId,
            int agentId,
            int? assignedByUserId = null,
            CancellationToken ct = default)
        {
            var commune = await _db.Communes.FirstOrDefaultAsync(c => c.IdCommune == communeId, ct)
                ?? throw new InvalidOperationException("Commune introuvable.");

            var agent = await (
                from a in _db.Agents
                join z in _db.ZonesSociales on a.ZoneSocialeId equals z.IdZoneSociale
                where a.IdAgent == agentId && a.Statut && z.CommuneId == communeId
                select a
            ).FirstOrDefaultAsync(ct)
                ?? throw new InvalidOperationException(
                    "Agent introuvable, inactif ou non rattaché à une zone de cette commune.");

            var previousAgentId = commune.SuperviseurAgentId;
            string? previousNom = null;
            if (previousAgentId.HasValue && previousAgentId != agentId)
            {
                previousNom = await GetAgentNomAsync(previousAgentId.Value, ct);
                await RevokeSuperviseurRoleIfNotTitulaireAsync(previousAgentId.Value, ct);
            }

            var autresCommunes = await _db.Communes
                .Where(c => c.SuperviseurAgentId == agentId && c.IdCommune != communeId)
                .ToListAsync(ct);
            foreach (var autre in autresCommunes)
                autre.SuperviseurAgentId = null;

            commune.SuperviseurAgentId = agentId;
            await _db.SaveChangesAsync(ct);

            await GrantRoleToAgentAsync(agentId, SuperviseurTerritoryScopeHelper.RoleName, assignedByUserId, ct);

            _logger.LogInformation(
                "Superviseur {AgentId} affecté à la commune {CommuneId} (précédent : {PreviousAgentId})",
                agentId, communeId, previousAgentId);

            return new TerritorialAffectationResultDto
            {
                TerritoryId = communeId,
                PreviousAgentId = previousAgentId == agentId ? null : previousAgentId,
                PreviousAgentNom = previousNom,
                NewAgentId = agentId,
                NewAgentNom = agent.NomComplet
            };
        }

        public async Task<TerritorialAffectationResultDto> ClearSuperviseurAsync(
            int communeId,
            int? assignedByUserId = null,
            CancellationToken ct = default)
        {
            var commune = await _db.Communes.FirstOrDefaultAsync(c => c.IdCommune == communeId, ct)
                ?? throw new InvalidOperationException("Commune introuvable.");

            var previousAgentId = commune.SuperviseurAgentId;
            string? previousNom = null;
            if (previousAgentId.HasValue)
            {
                previousNom = await GetAgentNomAsync(previousAgentId.Value, ct);
                commune.SuperviseurAgentId = null;
                await _db.SaveChangesAsync(ct);
                await RevokeSuperviseurRoleIfNotTitulaireAsync(previousAgentId.Value, ct);
            }

            return new TerritorialAffectationResultDto
            {
                TerritoryId = communeId,
                PreviousAgentId = previousAgentId,
                PreviousAgentNom = previousNom,
                NewAgentId = null,
                NewAgentNom = null
            };
        }

        public async Task ReleaseTitularitesForAgentAsync(int agentId, CancellationToken ct = default)
        {
            var zones = await _db.ZonesSociales
                .Where(z => z.ChefEquipeAgentId == agentId)
                .ToListAsync(ct);
            foreach (var zone in zones)
                zone.ChefEquipeAgentId = null;

            var communes = await _db.Communes
                .Where(c => c.SuperviseurAgentId == agentId)
                .ToListAsync(ct);
            foreach (var commune in communes)
                commune.SuperviseurAgentId = null;

            if (zones.Count > 0 || communes.Count > 0)
                await _db.SaveChangesAsync(ct);

            await RevokeChefEquipeRoleIfNotTitulaireAsync(agentId, ct);
            await RevokeSuperviseurRoleIfNotTitulaireAsync(agentId, ct);
        }

        private async Task GrantRoleToAgentAsync(
            int agentId,
            string roleName,
            int? assignedByUserId,
            CancellationToken ct)
        {
            var roleId = await _db.Roles.AsNoTracking()
                .Where(r => r.Nom == roleName && r.Statut)
                .Select(r => r.IdRole)
                .FirstOrDefaultAsync(ct);
            if (roleId == 0)
            {
                _logger.LogWarning("Rôle {RoleName} introuvable lors de l'affectation territoriale.", roleName);
                return;
            }

            var userId = await _db.Utilisateurs.AsNoTracking()
                .Where(u => u.AgentId == agentId && u.Statut)
                .Select(u => u.IdUtilisateur)
                .FirstOrDefaultAsync(ct);
            if (userId == 0)
            {
                _logger.LogWarning(
                    "Aucun utilisateur actif pour l'agent {AgentId} — rôle {RoleName} non synchronisé.",
                    agentId, roleName);
                return;
            }

            await _utilisateurRepository.AddRoleToUserAsync(userId, roleId, assignedByUserId, false, ct);
        }

        private async Task RevokeChefEquipeRoleIfNotTitulaireAsync(int agentId, CancellationToken ct)
        {
            var encoreTitulaire = await _db.ZonesSociales
                .AnyAsync(z => z.ChefEquipeAgentId == agentId, ct);
            if (!encoreTitulaire)
                await RevokeRoleFromAgentAsync(agentId, ChefEquipeZoneScopeHelper.RoleName, ct);
        }

        private async Task RevokeSuperviseurRoleIfNotTitulaireAsync(int agentId, CancellationToken ct)
        {
            var encoreTitulaire = await _db.Communes
                .AnyAsync(c => c.SuperviseurAgentId == agentId, ct);
            if (!encoreTitulaire)
                await RevokeRoleFromAgentAsync(agentId, SuperviseurTerritoryScopeHelper.RoleName, ct);
        }

        private async Task RevokeRoleFromAgentAsync(int agentId, string roleName, CancellationToken ct)
        {
            var roleId = await _db.Roles.AsNoTracking()
                .Where(r => r.Nom == roleName)
                .Select(r => r.IdRole)
                .FirstOrDefaultAsync(ct);
            if (roleId == 0)
                return;

            var userId = await _db.Utilisateurs.AsNoTracking()
                .Where(u => u.AgentId == agentId)
                .Select(u => u.IdUtilisateur)
                .FirstOrDefaultAsync(ct);
            if (userId == 0)
                return;

            await _utilisateurRepository.RemoveRoleFromUserAsync(userId, roleId, ct);
        }

        private async Task<string?> GetAgentNomAsync(int agentId, CancellationToken ct) =>
            await _db.Agents.AsNoTracking()
                .Where(a => a.IdAgent == agentId)
                .Select(a => a.NomComplet)
                .FirstOrDefaultAsync(ct);
    }
}
