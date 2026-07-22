using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using Prosoc.Utilities;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletMouvementController : BaseApiController
    {
        private readonly ProsocDbContext _db;

        public WalletMouvementController(
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<WalletMouvementController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<WalletMouvementReadDto>>> GetAll(
            [FromQuery] PaginationRequest request,
            CancellationToken ct = default)
        {
            var query = WalletMouvementsQuery();

            var result = await _paginationService.CreatePaginatedResponseAsync(query, request, ct);
            return Ok(await MapPaginatedResponseAsync(result, ct));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WalletMouvementReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var mouvement = await WalletMouvementsQuery()
                .FirstOrDefaultAsync(m => m.IdWalletMouvement == id, ct);

            if (mouvement == null)
                return NotFound();

            var dtos = await WalletMouvementHelpers.ToReadDtosAsync(_db, new[] { mouvement }, ct);
            return Ok(dtos[0]);
        }

        [HttpGet("by-wallet/{walletId}")]
        public async Task<ActionResult<List<WalletMouvementReadDto>>> GetByWallet(int walletId, CancellationToken ct = default)
        {
            var mouvements = await WalletMouvementsQuery()
                .Where(m => m.WalletId == walletId)
                .OrderByDescending(m => m.DateOperation)
                .ToListAsync(ct);

            return Ok(await WalletMouvementHelpers.ToReadDtosAsync(_db, mouvements, ct));
        }

        [HttpGet("by-agent/{agentId}")]
        public async Task<ActionResult<List<WalletMouvementReadDto>>> GetByAgent(int agentId, CancellationToken ct = default)
        {
            var scopeDenied = await ChefEquipeZoneScopeHelper.EnsureAgentDansMaZoneAsync(User, _db, agentId, ct);
            if (scopeDenied is not null)
                return scopeDenied;

            var mouvements = await WalletMouvementsQuery()
                .Where(m => m.Wallet!.AgentId == agentId)
                .OrderByDescending(m => m.DateOperation)
                .ToListAsync(ct);

            return Ok(await WalletMouvementHelpers.ToReadDtosAsync(_db, mouvements, ct));
        }

        [HttpGet("by-agent/{agentId}/paginated")]
        public async Task<ActionResult<PaginatedResponse<WalletMouvementReadDto>>> GetByAgentPaginated(
            int agentId,
            [FromQuery] PaginationRequest request,
            CancellationToken ct = default)
        {
            var query = WalletMouvementsQuery()
                .Where(m => m.Wallet!.AgentId == agentId);

            if (string.IsNullOrEmpty(request.SortBy))
                query = query.OrderByDescending(m => m.DateOperation);

            var result = await _paginationService.CreatePaginatedResponseAsync(query, request, ct);
            return Ok(await MapPaginatedResponseAsync(result, ct));
        }

        [HttpGet("by-source/{source}")]
        public async Task<ActionResult<List<WalletMouvementReadDto>>> GetBySource(string source, CancellationToken ct = default)
        {
            var mouvements = await WalletMouvementsQuery()
                .Where(m => m.Source == source)
                .OrderByDescending(m => m.DateOperation)
                .ToListAsync(ct);

            return Ok(await WalletMouvementHelpers.ToReadDtosAsync(_db, mouvements, ct));
        }

        [HttpPost]
        public async Task<ActionResult<WalletMouvementReadDto>> Create(WalletMouvementCreateDto dto, CancellationToken ct = default)
        {
            var wallet = await _db.WalletsAgents.FindAsync(new object[] { dto.WalletId }, ct);
            if (wallet == null)
                return BadRequest("Wallet introuvable");

            var mouvement = new WalletMouvement
            {
                WalletId = dto.WalletId,
                DeviseId = wallet.DeviseId,
                Montant = dto.Montant,
                TypeOperation = dto.TypeOperation,
                Source = dto.Source,
                Description = dto.Description,
                DateOperation = DateTime.Now
            };

            _db.WalletMouvements.Add(mouvement);
            await _db.SaveChangesAsync(ct);

            await _db.Entry(mouvement)
                .Reference(m => m.Wallet)
                .Query()
                .Include(w => w!.Agent)
                .LoadAsync(ct);

            await _db.Entry(mouvement)
                .Reference(m => m.Devise)
                .LoadAsync(ct);

            var dtos = await WalletMouvementHelpers.ToReadDtosAsync(_db, new[] { mouvement }, ct);
            return CreatedAtAction(nameof(GetById), new { id = mouvement.IdWalletMouvement }, dtos[0]);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, WalletMouvementUpdateDto dto)
        {
            var mouvement = await _db.WalletMouvements.FindAsync(id);
            if (mouvement == null)
                return NotFound();

            mouvement.Montant = dto.Montant;
            mouvement.TypeOperation = dto.TypeOperation;
            mouvement.Source = dto.Source;
            mouvement.Description = dto.Description;
            mouvement.DateModification = DateTime.Now;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var mouvement = await _db.WalletMouvements.FindAsync(id);
            if (mouvement == null)
                return NotFound();

            _db.WalletMouvements.Remove(mouvement);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private IQueryable<WalletMouvement> WalletMouvementsQuery() =>
            _db.WalletMouvements
                .Include(m => m.Devise)
                .Include(m => m.Wallet)
                .ThenInclude(w => w!.Agent)
                .AsNoTracking();

        private async Task<PaginatedResponse<WalletMouvementReadDto>> MapPaginatedResponseAsync(
            PaginatedResponse<WalletMouvement> result,
            CancellationToken ct)
        {
            var dtos = await WalletMouvementHelpers.ToReadDtosAsync(_db, result.Data, ct);
            return new PaginatedResponse<WalletMouvementReadDto>
            {
                Data = dtos,
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage
            };
        }
    }
}
