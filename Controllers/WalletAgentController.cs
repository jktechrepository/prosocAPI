using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using Prosoc.Data;
using Prosoc.Utilities;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WalletAgentController : BaseApiController
    {
        private readonly ProsocDbContext _db;

        public WalletAgentController(
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<WalletAgentController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<WalletAgentReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.WalletsAgents
                    .Include(w => w.Agent)
                    .Include(w => w.Devise)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                var dtos = result.Data.Select(WalletAgentHelpers.ToReadDto).ToList();

                var paginatedDtos = new PaginatedResponse<WalletAgentReadDto>
                {
                    Data = dtos,
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
                    "Une erreur technique est survenue lors de la récupération des wallets agents paginés",
                    ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WalletAgentReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var wallet = await _db.WalletsAgents
                .Include(w => w.Agent)
                .Include(w => w.Devise)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.IdWalletAgent == id, ct);

            if (wallet == null)
                return NotFound();

            return Ok(WalletAgentHelpers.ToReadDto(wallet));
        }

        /// <summary>Wallet d'un agent pour une devise (paramètres explicites).</summary>
        [HttpGet("Agent")]
        public async Task<ActionResult<WalletAgentReadDto>> GetByAgentAndDevise(
            [FromQuery] int agentId,
            [FromQuery] int deviseId,
            CancellationToken ct = default)
        {
            if (agentId <= 0 || deviseId <= 0)
                return BadRequest("Les paramètres agentId et deviseId sont obligatoires et doivent être > 0.");

            var wallet = await _db.WalletsAgents
                .Include(w => w.Agent)
                .Include(w => w.Devise)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.AgentId == agentId && w.DeviseId == deviseId, ct);

            return wallet == null ? NotFound() : Ok(WalletAgentHelpers.ToReadDto(wallet));
        }

        [HttpGet("by-agent/{agentId}")]
        public async Task<ActionResult<WalletAgentReadDto>> GetByAgent(
            int agentId,
            [FromQuery] int? deviseId,
            CancellationToken ct = default)
        {
            var query = _db.WalletsAgents
                .Include(w => w.Agent)
                .Include(w => w.Devise)
                .AsNoTracking()
                .Where(w => w.AgentId == agentId);

            if (deviseId is > 0)
                query = query.Where(w => w.DeviseId == deviseId.Value);

            var wallet = await query
                .OrderByDescending(w => w.Devise!.EstDevisePrincipale)
                .FirstOrDefaultAsync(ct);

            return wallet == null ? NotFound() : Ok(WalletAgentHelpers.ToReadDto(wallet));
        }

        /// <summary>Tous les wallets d'un agent (une entrée par devise).</summary>
        [HttpGet("by-agent/{agentId}/all")]
        public async Task<ActionResult<List<WalletAgentReadDto>>> GetAllByAgent(int agentId, CancellationToken ct = default)
        {
            var wallets = await _db.WalletsAgents
                .Include(w => w.Agent)
                .Include(w => w.Devise)
                .AsNoTracking()
                .Where(w => w.AgentId == agentId)
                .OrderBy(w => w.Devise!.Code)
                .ToListAsync(ct);

            return Ok(wallets.Select(WalletAgentHelpers.ToReadDto).ToList());
        }

        [HttpGet("solde")]
        public async Task<ActionResult<decimal>> GetSolde(
            [FromQuery] int agentId,
            [FromQuery] int deviseId,
            CancellationToken ct = default)
        {
            if (agentId <= 0 || deviseId <= 0)
                return BadRequest("Les paramètres agentId et deviseId sont obligatoires et doivent être > 0.");

            var wallet = await _db.WalletsAgents
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.AgentId == agentId && w.DeviseId == deviseId, ct);

            return wallet == null ? NotFound() : Ok(wallet.SoldeCourant);
        }

        [HttpPost]
        public async Task<ActionResult<WalletAgentReadDto>> Create([FromBody] WalletAgentCreateDto createDto, CancellationToken ct = default)
        {
            if (!await _db.Devises.AnyAsync(d => d.IdDevise == createDto.DeviseId && d.Statut, ct))
                return BadRequest($"Devise avec ID {createDto.DeviseId} introuvable ou inactive.");

            var existingWallet = await _db.WalletsAgents
                .AnyAsync(w => w.AgentId == createDto.AgentId && w.DeviseId == createDto.DeviseId, ct);

            if (existingWallet)
            {
                return BadRequest(new
                {
                    Message = $"Un wallet existe déjà pour l'agent {createDto.AgentId} et la devise {createDto.DeviseId}"
                });
            }

            var wallet = new WalletAgent
            {
                AgentId = createDto.AgentId,
                DeviseId = createDto.DeviseId,
                SoldeCourant = createDto.SoldeInitial,
                SoldeDisponible = createDto.SoldeInitial,
                DateCreation = DateTime.Now,
                Statut = createDto.Statut
            };

            _db.WalletsAgents.Add(wallet);
            await _db.SaveChangesAsync(ct);

            // Récupérer les informations de l'agent
            var walletWithAgent = await _db.WalletsAgents
                .Include(w => w.Agent)
                .Include(w => w.Devise)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.IdWalletAgent == wallet.IdWalletAgent, ct);

            return CreatedAtAction(nameof(GetById), new { id = wallet.IdWalletAgent }, WalletAgentHelpers.ToReadDto(walletWithAgent!));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<WalletAgentReadDto>> Update(int id, [FromBody] WalletAgentUpdateDto updateDto, CancellationToken ct = default)
        {
            var wallet = await _db.WalletsAgents
                .FirstOrDefaultAsync(w => w.IdWalletAgent == id, ct);

            if (wallet == null)
                return NotFound();

            if (updateDto.DeviseId is > 0 && updateDto.DeviseId != wallet.DeviseId)
            {
                var duplicate = await _db.WalletsAgents.AnyAsync(
                    w => w.AgentId == wallet.AgentId && w.DeviseId == updateDto.DeviseId && w.IdWalletAgent != id, ct);
                if (duplicate)
                    return Conflict("Un wallet existe déjà pour cette paire agent/devise.");
                wallet.DeviseId = updateDto.DeviseId.Value;
            }

            wallet.SoldeCourant = updateDto.SoldeCourant;
            wallet.SoldeDisponible = Math.Min(wallet.SoldeDisponible, updateDto.SoldeCourant);
            wallet.Statut = updateDto.Statut;
            wallet.DateModification = DateTime.Now;

            await _db.SaveChangesAsync(ct);

            var updatedWallet = await _db.WalletsAgents
                .Include(w => w.Agent)
                .Include(w => w.Devise)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.IdWalletAgent == wallet.IdWalletAgent, ct);

            return Ok(WalletAgentHelpers.ToReadDto(updatedWallet!));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            var wallet = await _db.WalletsAgents
                .FirstOrDefaultAsync(w => w.IdWalletAgent == id, ct);

            if (wallet == null)
                return NotFound();

            _db.WalletsAgents.Remove(wallet);
            await _db.SaveChangesAsync(ct);

            return NoContent();
        }

        /// <summary>
        /// Récupère les wallets agents avec filtres avancés
        /// </summary>
        [HttpPost("advanced")]
        public async Task<ActionResult<ExtendedPaginatedResponse<WalletAgentReadDto>>> GetWalletsAdvanced(
            [FromBody] AdvancedPaginationRequest request)
        {
            try
            {
                // Construire la requête de base
                var query = _db.WalletsAgents
                    .Include(w => w.Agent)
                    .Include(w => w.Devise)
                    .AsQueryable();

                // Appliquer les filtres de base
                if (request.FilterList != null && request.FilterList.Any())
                {
                    foreach (var filter in request.FilterList)
                    {
                        switch (filter.Field.ToLower())
                        {
                            case "agentid":
                                if (filter.Operator == "eq")
                                    query = query.Where(w => w.AgentId == int.Parse(filter.Value));
                                break;
                            case "soldecourant":
                                if (filter.Operator == "eq")
                                    query = query.Where(w => w.SoldeCourant == decimal.Parse(filter.Value));
                                else if (filter.Operator == "gt")
                                    query = query.Where(w => w.SoldeCourant > decimal.Parse(filter.Value));
                                else if (filter.Operator == "lt")
                                    query = query.Where(w => w.SoldeCourant < decimal.Parse(filter.Value));
                                break;
                            case "statut":
                                if (filter.Operator == "eq")
                                    query = query.Where(w => w.Statut == bool.Parse(filter.Value));
                                break;
                            case "agentnom":
                                if (filter.Operator == "contains")
                                    query = query.Where(w => w.Agent != null && w.Agent.NomComplet.Contains(filter.Value));
                                break;
                            case "agentmatricule":
                                if (filter.Operator == "contains")
                                    query = query.Where(w => w.Agent != null && w.Agent.Matricule.Contains(filter.Value));
                                break;
                            case "datecreation":
                                if (filter.Operator == "eq")
                                    query = query.Where(w => w.DateCreation.Date == DateTime.Parse(filter.Value).Date);
                                else if (filter.Operator == "gt")
                                    query = query.Where(w => w.DateCreation > DateTime.Parse(filter.Value));
                                else if (filter.Operator == "lt")
                                    query = query.Where(w => w.DateCreation < DateTime.Parse(filter.Value));
                                break;
                        }
                    }
                }

                // Appliquer la pagination
                var response = await _paginationService.CreateExtendedPaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs
                var walletDtos = response.Data.Select(WalletAgentHelpers.ToReadDto).ToList();
                
                // Créer une nouvelle réponse avec les DTOs
                var dtoResponse = new ExtendedPaginatedResponse<WalletAgentReadDto>
                {
                    Data = walletDtos,
                    CurrentPage = response.CurrentPage,
                    PageSize = response.PageSize,
                    TotalItems = response.TotalItems,
                    TotalPages = response.TotalPages,
                    HasNextPage = response.HasNextPage,
                    HasPreviousPage = response.HasPreviousPage,
                    AppliedFilters = request.FilterList?.Select(f => $"{f.Field} {f.Operator} {f.Value}").ToList() ?? new(),
                    AppliedSorting = $"{request.SortBy} {request.SortDirection}"
                };

                return Ok(dtoResponse);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des wallets agents avancés",
                    ex);
            }
        }

        /// <summary>
        /// Récupère les wallets par agent avec pagination
        /// </summary>
        [HttpGet("by-agent/{agentId}/paginated")]
        public async Task<ActionResult<PaginatedResponse<WalletAgentReadDto>>> GetByAgent(
            int agentId, 
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.WalletsAgents
                    .Include(w => w.Agent)
                    .Where(w => w.AgentId == agentId)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(WalletAgentHelpers.ToReadDto).ToList();

                var paginatedDtos = new PaginatedResponse<WalletAgentReadDto>
                {
                    Data = dtos,
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
                    "Une erreur technique est survenue lors de la récupération des wallets pour l'agent ",
                    ex);
            }
        }
    }
}
