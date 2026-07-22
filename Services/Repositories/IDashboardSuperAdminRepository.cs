using ProsocAPI.Models.DTOs.DashboardAdmin;
using ProsocAPI.Models.DTOs.DashboardSuperAdmin;

namespace ProsocAPI.Services.Repositories
{
    public interface IDashboardSuperAdminRepository
    {
        Task<DashboardAdminKpisDto> GetKpisAdminAsync(CancellationToken ct = default);
        Task<SuperAdminSystemKpisDto> GetKpisSystemeAsync(CancellationToken ct = default);
        Task<List<UtilisateursParRoleDto>> GetUtilisateursParRoleAsync(CancellationToken ct = default);
        Task<DashboardSuperAdminDto> GetDashboardSummaryAsync(CancellationToken ct = default);
    }
}
