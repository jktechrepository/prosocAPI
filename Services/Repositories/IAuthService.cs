using ProsocAPI.Models.DTOs.Authentication;

namespace ProsocAPI.Services.Repositories
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
    }
}
