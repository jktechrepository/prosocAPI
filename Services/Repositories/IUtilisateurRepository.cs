using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Services.Repositories
{
    public interface IUtilisateurRepository
    {
        Task<List<Utilisateur>> GetAllAsync(CancellationToken ct = default);
        Task<Utilisateur?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Utilisateur?> GetByNomUtilisateurAsync(string nomUtilisateur, CancellationToken ct = default);
        Task<Utilisateur?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<Utilisateur?> GetByTelephoneAsync(string telephone, CancellationToken ct = default);
        Task<Utilisateur?> GetByDefaultUsernameAsync(string defaultUsername, CancellationToken ct = default);
        Task<List<Utilisateur>> GetByRoleAsync(int roleId, CancellationToken ct = default);
        Task<List<Utilisateur>> GetByStatutAsync(bool statut, CancellationToken ct = default);
        Task<bool> ExistsByIdAsync(int id, CancellationToken ct = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
        Task<List<UserRole>> GetUserRolesAsync(int userId, CancellationToken ct = default);
        Task<List<Permission>> GetUserPermissionsAsync(int userId, CancellationToken ct = default);
        Task<List<Role>> GetUserRolesEntitiesAsync(int userId, CancellationToken ct = default);
        Task<bool> AddRoleToUserAsync(int userId, int roleId, int? assignedByUserId = null, bool isPrimary = false, CancellationToken ct = default);
        Task<bool> RemoveRoleFromUserAsync(int userId, int roleId, CancellationToken ct = default);
        Task<bool> SetPrimaryRoleAsync(int userId, int roleId, CancellationToken ct = default);
        Task<Utilisateur> CreateAsync(Utilisateur entity, CancellationToken ct = default);
        Task<Utilisateur?> UpdateAsync(int id, Utilisateur entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
