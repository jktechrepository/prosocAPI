using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class UtilisateurService : IUtilisateurRepository
    {
        private readonly ProsocDbContext _db;

        public UtilisateurService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<List<Utilisateur>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Utilisateurs.AsNoTracking().OrderBy(x => x.IdUtilisateur).ToListAsync(ct);
        }

        public async Task<Utilisateur?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Utilisateurs.AsNoTracking().FirstOrDefaultAsync(x => x.IdUtilisateur == id, ct);
        }

        public async Task<Utilisateur?> GetByNomUtilisateurAsync(string nomUtilisateur, CancellationToken ct = default)
        {
            return await _db.Utilisateurs
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(x => x.NomUtilisateur == nomUtilisateur, ct);
        }

        public async Task<Utilisateur> CreateAsync(Utilisateur entity, CancellationToken ct = default)
        {
            entity.PhoneUtilisateur = NormalizePhoneOrKeep(entity.PhoneUtilisateur);
            _db.Utilisateurs.Add(entity);
            await _db.SaveChangesAsync(ct);
            
            // Attribution automatique du rôle si RoleId est spécifié
            if (entity.RoleId.HasValue)
            {
                await AddRoleToUserAsync(entity.IdUtilisateur, entity.RoleId.Value, null, true, ct);
            }
            
            return entity;
        }

        public async Task<Utilisateur?> UpdateAsync(int id, Utilisateur entity, CancellationToken ct = default)
        {
            var existing = await _db.Utilisateurs.FirstOrDefaultAsync(x => x.IdUtilisateur == id, ct);
            if (existing == null)
                return null;

            existing.NomUtilisateur = entity.NomUtilisateur;
            existing.EmailUtilisateur = entity.EmailUtilisateur;
            existing.PhoneUtilisateur = NormalizePhoneOrKeep(entity.PhoneUtilisateur);
            existing.Statut = entity.Statut;
            existing.RoleId = entity.RoleId;
            existing.AgentId = entity.AgentId;
            existing.AffilieId = entity.AffilieId;
            existing.HopitalPartenaireId = entity.HopitalPartenaireId;
            existing.DoitChangerMotDePasse = entity.DoitChangerMotDePasse;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.Utilisateurs.FirstOrDefaultAsync(x => x.IdUtilisateur == id, ct);
            if (existing == null)
                return false;

            _db.Utilisateurs.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        // ═══════════════════════════════════════════════════════════════════════════════════
        // 🔧 MÉTHODES POUR L'AUTHENTIFICATION MULTI-CANAL
        // ═══════════════════════════════════════════════════════════════════════════════════

        public async Task<Utilisateur?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _db.Utilisateurs
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(x => x.EmailUtilisateur == email, ct);
        }

        public async Task<Utilisateur?> GetByTelephoneAsync(string telephone, CancellationToken ct = default)
        {
            var variants = PhoneNumberHelper.GetLookupVariants(telephone);
            if (variants.Count == 0)
                return null;

            return await _db.Utilisateurs
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(
                    x => x.PhoneUtilisateur != null && variants.Contains(x.PhoneUtilisateur),
                    ct);
        }

        private static string? NormalizePhoneOrKeep(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return phone;

            return PhoneNumberHelper.NormalizeForStorage(phone) ?? phone.Trim();
        }

        public async Task<Utilisateur?> GetByDefaultUsernameAsync(string defaultUsername, CancellationToken ct = default)
        {
            return await _db.Utilisateurs
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(x => x.DefaultUsername == defaultUsername, ct);
        }

        public async Task<List<Utilisateur>> GetByRoleAsync(int roleId, CancellationToken ct = default)
        {
            // Filtre sur la table de jointure UserRoles pour inclure les utilisateurs ayant ce rôle actif
            return await _db.UserRoles
                .AsNoTracking()
                .Where(ur => ur.RoleId == roleId && ur.Statut)
                .Select(ur => ur.Utilisateur)
                .Distinct()
                .OrderBy(u => u.IdUtilisateur)
                .ToListAsync(ct);
        }

        public async Task<List<Utilisateur>> GetByStatutAsync(bool statut, CancellationToken ct = default)
        {
            return await _db.Utilisateurs
                .AsNoTracking()
                .Where(u => u.Statut == statut)
                .OrderBy(u => u.IdUtilisateur)
                .ToListAsync(ct);
        }

        public async Task<bool> ExistsByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Utilisateurs
                .AsNoTracking()
                .AnyAsync(u => u.IdUtilisateur == id, ct);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _db.Utilisateurs
                .AsNoTracking()
                .AnyAsync(u => u.EmailUtilisateur == email, ct);
        }

        public async Task<List<UserRole>> GetUserRolesAsync(int userId, CancellationToken ct = default)
        {
            var userRoles = await _db.UserRoles
                .AsNoTracking()
                .Include(ur => ur.Role)
                .Where(ur => ur.UtilisateurId == userId && ur.Statut)
                .ToListAsync(ct);

            var primaryRoleId = await _db.Utilisateurs
                .AsNoTracking()
                .Where(u => u.IdUtilisateur == userId)
                .Select(u => u.RoleId)
                .FirstOrDefaultAsync(ct);

            if (primaryRoleId.HasValue && userRoles.All(ur => ur.RoleId != primaryRoleId.Value))
            {
                var role = await _db.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.IdRole == primaryRoleId.Value, ct);
                if (role != null)
                {
                    userRoles.Add(new UserRole
                    {
                        UtilisateurId = userId,
                        RoleId = role.IdRole,
                        Role = role,
                        IsPrimary = true,
                        Statut = true,
                    });
                }
            }

            return userRoles;
        }

        public async Task<List<Permission>> GetUserPermissionsAsync(int userId, CancellationToken ct = default)
        {
            var userRoles = await GetUserRolesAsync(userId, ct);
            var roleIds = userRoles.Select(ur => ur.RoleId).Distinct().ToList();

            return await _db.RolePermissions
                .AsNoTracking()
                .Include(rp => rp.Permission)
                .Where(rp => roleIds.Contains(rp.RoleId)) // Statut n'existe pas dans RolePermission
                .Select(rp => rp.Permission)
                .Distinct()
                .ToListAsync(ct);
        }

        public async Task<List<Role>> GetUserRolesEntitiesAsync(int userId, CancellationToken ct = default)
        {
            return await _db.UserRoles
                .AsNoTracking()
                .Include(ur => ur.Role)
                .Where(ur => ur.UtilisateurId == userId && ur.Statut)
                .OrderByDescending(ur => ur.IsPrimary)
                .ThenBy(ur => ur.Role.Niveau ?? 999)
                .Select(ur => ur.Role)
                .ToListAsync(ct);
        }

        public async Task<bool> AddRoleToUserAsync(int userId, int roleId, int? assignedByUserId = null, bool isPrimary = false, CancellationToken ct = default)
        {
            var userExists = await _db.Utilisateurs.AsNoTracking().AnyAsync(u => u.IdUtilisateur == userId, ct);
            if (!userExists)
                return false;

            var roleExists = await _db.Roles.AsNoTracking().AnyAsync(r => r.IdRole == roleId, ct);
            if (!roleExists)
                return false;

            var existing = await _db.UserRoles.FirstOrDefaultAsync(ur => ur.UtilisateurId == userId && ur.RoleId == roleId, ct);
            if (existing != null)
            {
                if (existing.Statut)
                {
                    // Déjà attribué et actif
                    return true;
                }

                // Réactivation
                existing.Statut = true;
                existing.DateAttribution = DateTime.Now;
                existing.IdUtilisateurAttribution = assignedByUserId;
            }
            else
            {
                _db.UserRoles.Add(new UserRole
                {
                    UtilisateurId = userId,
                    RoleId = roleId,
                    IsPrimary = false,
                    Statut = true,
                    DateAttribution = DateTime.Now,
                    IdUtilisateurAttribution = assignedByUserId
                });
            }

            if (isPrimary)
            {
                // Un seul primary
                var actives = await _db.UserRoles
                    .Where(ur => ur.UtilisateurId == userId && ur.Statut)
                    .ToListAsync(ct);

                foreach (var ur in actives)
                    ur.IsPrimary = ur.RoleId == roleId;
            }

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId, CancellationToken ct = default)
        {
            var userRole = await _db.UserRoles.FirstOrDefaultAsync(ur => ur.UtilisateurId == userId && ur.RoleId == roleId && ur.Statut, ct);
            if (userRole == null)
                return false;

            var activeCount = await _db.UserRoles.CountAsync(ur => ur.UtilisateurId == userId && ur.Statut, ct);
            if (activeCount <= 1)
            {
                // On refuse de retirer le dernier rôle actif
                return false;
            }

            var wasPrimary = userRole.IsPrimary;
            userRole.Statut = false;
            userRole.IsPrimary = false;
            await _db.SaveChangesAsync(ct);

            if (wasPrimary)
            {
                // Choisir un nouveau primary: niveau le plus petit
                var newPrimary = await _db.UserRoles
                    .Include(ur => ur.Role)
                    .Where(ur => ur.UtilisateurId == userId && ur.Statut)
                    .OrderBy(ur => ur.Role.Niveau ?? 999)
                    .FirstOrDefaultAsync(ct);

                if (newPrimary != null)
                {
                    newPrimary.IsPrimary = true;
                    await _db.SaveChangesAsync(ct);
                }
            }

            return true;
        }

        public async Task<bool> SetPrimaryRoleAsync(int userId, int roleId, CancellationToken ct = default)
        {
            var target = await _db.UserRoles.FirstOrDefaultAsync(ur => ur.UtilisateurId == userId && ur.RoleId == roleId && ur.Statut, ct);
            if (target == null)
                return false;

            var actives = await _db.UserRoles
                .Where(ur => ur.UtilisateurId == userId && ur.Statut)
                .ToListAsync(ct);

            foreach (var ur in actives)
                ur.IsPrimary = ur.RoleId == roleId;

            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
