using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserDeviceController : ControllerBase
    {
        private readonly IUserDeviceRepository _userDeviceRepository;

        public UserDeviceController(IUserDeviceRepository userDeviceRepository)
        {
            _userDeviceRepository = userDeviceRepository;
        }

        [HttpGet]
        public async Task<ActionResult> GetMyDevices(CancellationToken ct)
        {
            // Récupérer l'ID utilisateur depuis le token JWT
            var userIdClaim = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Utilisateur non identifié");
            }

            var devices = await _userDeviceRepository.GetByUserIdAsync(userId, ct);
            return Ok(devices);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetDevice(int id, CancellationToken ct)
        {
            var device = await _userDeviceRepository.GetByIdAsync(id, ct);
            if (device == null)
                return NotFound();

            // Vérifier que le device appartient à l'utilisateur connecté
            var userIdClaim = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Utilisateur non identifié");
            }

            if (device.UtilisateurId != userId)
                return Forbid();

            return Ok(device);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDevice(int id, CancellationToken ct)
        {
            var device = await _userDeviceRepository.GetByIdAsync(id, ct);
            if (device == null)
                return NotFound();

            // Vérifier que le device appartient à l'utilisateur connecté
            var userIdClaim = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Utilisateur non identifié");
            }

            if (device.UtilisateurId != userId)
                return Forbid();

            var result = await _userDeviceRepository.DeleteAsync(id, ct);
            if (result)
                return NoContent();

            return BadRequest("Erreur lors de la suppression du device");
        }
    }
}
