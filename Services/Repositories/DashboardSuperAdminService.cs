using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.DashboardAdmin;
using ProsocAPI.Models.DTOs.DashboardSuperAdmin;

namespace ProsocAPI.Services.Repositories
{
    public class DashboardSuperAdminService : IDashboardSuperAdminRepository
    {
        private readonly ProsocDbContext _db;
        private readonly IDashboardAdminRepository _adminDashboard;
        private readonly ILogger<DashboardSuperAdminService> _logger;

        public DashboardSuperAdminService(
            ProsocDbContext db,
            IDashboardAdminRepository adminDashboard,
            ILogger<DashboardSuperAdminService> logger)
        {
            _db = db;
            _adminDashboard = adminDashboard;
            _logger = logger;
        }

        public Task<DashboardAdminKpisDto> GetKpisAdminAsync(CancellationToken ct = default) =>
            _adminDashboard.GetKpisAsync(ct);

        public async Task<SuperAdminSystemKpisDto> GetKpisSystemeAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            var marchand = await _db.InfoPaiementsMarchand.AsNoTracking()
                .Where(m => m.Statut)
                .OrderByDescending(m => m.DateCreation)
                .FirstOrDefaultAsync(ct);

            var flexPayEnAttente = await _db.CollectesEnAttente.AsNoTracking()
                .CountAsync(c => c.StatutEnAttente == CollecteEnAttenteStatut.EnAttente, ct);

            var flexPayExpirees = await _db.CollectesEnAttente.AsNoTracking()
                .CountAsync(c =>
                    c.StatutEnAttente == CollecteEnAttenteStatut.EnAttente &&
                    c.DateExpiration < now, ct);

            var flexPayEchec = await _db.CollectesEnAttente.AsNoTracking()
                .CountAsync(c => c.StatutEnAttente == CollecteEnAttenteStatut.Echec, ct);

            return new SuperAdminSystemKpisDto
            {
                TotalUtilisateursActifs = await _db.Utilisateurs.CountAsync(u => u.Statut, ct),
                TotalUtilisateursInactifs = await _db.Utilisateurs.CountAsync(u => !u.Statut, ct),
                UtilisateursDoiventChangerMotDePasse = await _db.Utilisateurs.CountAsync(u => u.DoitChangerMotDePasse && u.Statut, ct),
                TotalRoles = await _db.Roles.CountAsync(r => r.Statut, ct),
                TotalPermissionsActives = await _db.Permissions.CountAsync(p => p.Statut, ct),
                FlexPayMarchandConfigure = marchand != null,
                FlexPayMobileMoneyActif = marchand?.ActifMobileMoney == true,
                FlexPayCarteBancaireActif = marchand?.ActifCarteBancaire == true,
                CollectesFlexPayEnAttente = flexPayEnAttente,
                CollectesFlexPayExpirees = flexPayExpirees,
                CollectesFlexPayEchec = flexPayEchec
            };
        }

        public async Task<List<UtilisateursParRoleDto>> GetUtilisateursParRoleAsync(CancellationToken ct = default)
        {
            var countsByRole = await _db.UserRoles.AsNoTracking()
                .Where(ur => ur.Statut)
                .Join(
                    _db.Utilisateurs.Where(u => u.Statut),
                    ur => ur.UtilisateurId,
                    u => u.IdUtilisateur,
                    (ur, _) => ur.RoleId)
                .GroupBy(roleId => roleId)
                .Select(g => new { RoleId = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            var roles = await _db.Roles.AsNoTracking()
                .Where(r => r.Statut)
                .OrderBy(r => r.Nom)
                .ToListAsync(ct);

            return roles
                .Select(r => new UtilisateursParRoleDto
                {
                    RoleNom = r.Nom,
                    RoleCode = r.Code ?? string.Empty,
                    NombreUtilisateurs = countsByRole.FirstOrDefault(c => c.RoleId == r.IdRole)?.Count ?? 0
                })
                .OrderByDescending(x => x.NombreUtilisateurs)
                .ToList();
        }

        public async Task<DashboardSuperAdminDto> GetDashboardSummaryAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Récupération du dashboard SuperAdmin (admin + système)");

            var kpisAdmin = await GetKpisAdminAsync(ct);

            return new DashboardSuperAdminDto
            {
                KpisAdmin = kpisAdmin,
                KpisSysteme = await GetKpisSystemeAsync(ct),
                UtilisateursParRole = await GetUtilisateursParRoleAsync(ct),
                TopAgents = await _adminDashboard.GetTopAgentsAsync(10, ct),
                CollectesEnAttenteValidation = await _adminDashboard.GetCollectesEnAttenteAsync(ct),
                DerniereMiseAJour = DateTime.Now,
                DevisePrincipaleCode = kpisAdmin.DevisePrincipaleCode
            };
        }
    }
}
