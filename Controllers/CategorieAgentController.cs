using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prosoc.Models.DTOs.CategorieAgent;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;
using ProsocAPI.Utilities;
using Prosoc.Data;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategorieAgentController : BaseApiController
    {
        private readonly ICategorieAgentRepository _categorieAgentRepository;
        private readonly ProsocDbContext _db;

        public CategorieAgentController(
            ICategorieAgentRepository categorieAgentRepository,
            ProsocDbContext db,
            ILogger<CategorieAgentController> logger,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions)
            : base(paginationService, paginationOptions, logger)
        {
            _categorieAgentRepository = categorieAgentRepository;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<CategorieAgentDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.CategoriesAgents
                    .Include(c => c.Agents)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                var paginatedDtos = new PaginatedResponse<CategorieAgentDto>
                {
                    Data = result.Data.Select(MapToDto).ToList(),
                    CurrentPage = result.CurrentPage,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    HasNextPage = result.HasNextPage,
                    HasPreviousPage = result.HasPreviousPage
                };

                return Ok(paginatedDtos);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des catégories d'agents paginées",
                    ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategorieAgentDto>> GetById(int id)
        {
            try
            {
                var categorie = await _categorieAgentRepository.GetByIdAsync(id);
                if (categorie == null)
                    return NotFound(new { message = "Catégorie d'agent non trouvée" });

                return Ok(MapToDto(categorie));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la catégorie d'agent {Id}", id);
                return this.TechnicalErrorResponse("Une erreur interne est survenue", ex);
            }
        }

        [HttpGet("actives")]
        public async Task<ActionResult<IEnumerable<CategorieAgentSummaryDto>>> GetActives()
        {
            try
            {
                var categories = await _categorieAgentRepository.GetByStatutAsync(true);
                return Ok(categories.Select(MapToSummaryDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des catégories d'agents actives");
                return this.TechnicalErrorResponse("Une erreur interne est survenue", ex);
            }
        }

        [HttpGet("inactives")]
        public async Task<ActionResult<IEnumerable<CategorieAgentSummaryDto>>> GetInactives()
        {
            try
            {
                var categories = await _categorieAgentRepository.GetByStatutAsync(false);
                return Ok(categories.Select(MapToSummaryDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des catégories d'agents inactives");
                return this.TechnicalErrorResponse("Une erreur interne est survenue", ex);
            }
        }

        [HttpPost]
        public async Task<ActionResult<CategorieAgentDto>> Create([FromBody] CreateCategorieAgentDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Données invalides", errors = ModelState });

                if (await _categorieAgentRepository.ExistsByCodeAsync(createDto.Code))
                    return BadRequest(new { message = "Une catégorie avec ce code existe déjà" });

                var (code, description, libelle) = CategorieAgentLibelleHelper.Normalize(
                    createDto.Code,
                    createDto.Description,
                    createDto.LibelleCategorie);

                if (await _categorieAgentRepository.ExistsByLibelleAsync(libelle))
                    return BadRequest(new { message = "Une catégorie avec ce libellé existe déjà" });

                var categorie = new CategorieAgent
                {
                    Code = code,
                    LibelleCategorie = libelle,
                    Description = description,
                    Statut = createDto.Statut,
                    DateCreation = DateTime.Now
                };

                var createdCategorie = await _categorieAgentRepository.CreateAsync(categorie);
                return CreatedAtAction(nameof(GetById), new { id = createdCategorie.IdCategorieAgent }, MapToDto(createdCategorie));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la catégorie d'agent");
                return this.TechnicalErrorResponse("Une erreur interne est survenue", ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CategorieAgentDto>> Update(int id, [FromBody] UpdateCategorieAgentDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Données invalides", errors = ModelState });

                if (id != updateDto.IdCategorieAgent)
                    return BadRequest(new { message = "ID de la catégorie non correspondant" });

                var existingCategorie = await _categorieAgentRepository.GetByIdAsync(id);
                if (existingCategorie == null)
                    return NotFound(new { message = "Catégorie d'agent non trouvée" });

                var categorieWithSameCode = await _categorieAgentRepository.GetByCodeAsync(updateDto.Code);
                if (categorieWithSameCode != null && categorieWithSameCode.IdCategorieAgent != id)
                    return BadRequest(new { message = "Une catégorie avec ce code existe déjà" });

                var (code, description, libelle) = CategorieAgentLibelleHelper.Normalize(
                    updateDto.Code,
                    updateDto.Description,
                    updateDto.LibelleCategorie);

                var categorieWithSameLibelle = await _categorieAgentRepository.GetByLibelleAsync(libelle);
                if (categorieWithSameLibelle != null && categorieWithSameLibelle.IdCategorieAgent != id)
                    return BadRequest(new { message = "Une catégorie avec ce libellé existe déjà" });

                existingCategorie.Code = code;
                existingCategorie.LibelleCategorie = libelle;
                existingCategorie.Description = description;
                existingCategorie.Statut = updateDto.Statut;
                existingCategorie.DateModification = DateTime.Now;

                var updated = await _categorieAgentRepository.UpdateAsync(existingCategorie);
                if (!updated)
                    return BadRequest(new { message = "Échec de la mise à jour" });

                return Ok(MapToDto(existingCategorie));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la catégorie d'agent {Id}", id);
                return this.TechnicalErrorResponse("Une erreur interne est survenue", ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var exists = await _categorieAgentRepository.ExistsByIdAsync(id);
                if (!exists)
                    return NotFound(new { message = "Catégorie d'agent non trouvée" });

                var deleted = await _categorieAgentRepository.DeleteAsync(id);
                if (!deleted)
                    return BadRequest(new { message = "Impossible de supprimer cette catégorie car elle est associée à des agents" });

                return Ok(new { message = "Catégorie d'agent supprimée avec succès" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la catégorie d'agent {Id}", id);
                return this.TechnicalErrorResponse("Une erreur interne est survenue", ex);
            }
        }

        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> ExistsById(int id)
        {
            try
            {
                var exists = await _categorieAgentRepository.ExistsByIdAsync(id);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'existence de la catégorie d'agent {Id}", id);
                return this.TechnicalErrorResponse("Une erreur interne est survenue", ex);
            }
        }

        [HttpGet("exists/libelle/{libelle}")]
        public async Task<ActionResult<bool>> ExistsByLibelle(string libelle)
        {
            try
            {
                var exists = await _categorieAgentRepository.ExistsByLibelleAsync(libelle);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'existence de la catégorie d'agent par libellé {Libelle}", libelle);
                return this.TechnicalErrorResponse("Une erreur interne est survenue", ex);
            }
        }

        [HttpGet("exists/code/{code}")]
        public async Task<ActionResult<bool>> ExistsByCode(string code)
        {
            try
            {
                var exists = await _categorieAgentRepository.ExistsByCodeAsync(code);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'existence de la catégorie d'agent par code {Code}", code);
                return this.TechnicalErrorResponse("Une erreur interne est survenue", ex);
            }
        }

        private static CategorieAgentDto MapToDto(CategorieAgent c) => new()
        {
            IdCategorieAgent = c.IdCategorieAgent,
            Code = CategorieAgentLibelleHelper.ResolveCode(c),
            LibelleCategorie = c.LibelleCategorie,
            Description = c.Description,
            Statut = c.Statut,
            DateCreation = c.DateCreation,
            DateModification = c.DateModification,
            NombreAgents = c.Agents?.Count ?? 0
        };

        private static CategorieAgentSummaryDto MapToSummaryDto(CategorieAgent c) => new()
        {
            IdCategorieAgent = c.IdCategorieAgent,
            Code = CategorieAgentLibelleHelper.ResolveCode(c),
            LibelleCategorie = c.LibelleCategorie,
            Statut = c.Statut,
            DateCreation = c.DateCreation,
            DateModification = c.DateModification
        };
    }
}
