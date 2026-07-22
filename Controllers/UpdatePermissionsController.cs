using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProsocAPI.Data;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UpdatePermissionsController : ControllerBase
    {
        private readonly UpdatePermissionsService _updateService;
        private readonly ILogger<UpdatePermissionsController> _logger;

        public UpdatePermissionsController(
            UpdatePermissionsService updateService,
            ILogger<UpdatePermissionsController> logger)
        {
            _updateService = updateService;
            _logger = logger;
        }

        /// <summary>
        /// Met à jour les permissions DEPENDANT et ASSUREUR pour tous les rôles
        /// </summary>
        [HttpPost("sync-dependant-assureur-permissions")]
        public async Task<IActionResult> SyncDependantAssureurPermissions(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Début de la synchronisation des permissions DEPENDANT et ASSUREUR");

                var success = await _updateService.UpdateDependantAndAssureurPermissionsAsync(ct);

                if (success)
                {
                    return Ok(new { 
                        Message = "Permissions DEPENDANT et ASSUREUR synchronisées avec succès",
                        Timestamp = DateTime.Now
                    });
                }
                else
                {
                    return StatusCode(500, new { 
                        Message = "Erreur lors de la synchronisation des permissions",
                        Timestamp = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la synchronisation des permissions");
                return this.TechnicalErrorResponse("Erreur interne du serveur", ex);
            }
        }
    }
}
