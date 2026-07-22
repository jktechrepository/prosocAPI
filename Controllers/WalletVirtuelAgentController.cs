using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProsocAPI.Extensions;
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
    public class WalletVirtuelAgentController : BaseApiController
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<WalletVirtuelAgentController> _logger;
        private readonly IWalletVirtuelMouvementService _walletVirtuelMouvementService;

        public WalletVirtuelAgentController(
            ProsocDbContext db,
            ILogger<WalletVirtuelAgentController> logger,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            IWalletVirtuelMouvementService walletVirtuelMouvementService)
            : base(paginationService, paginationOptions, logger)
        {
            _db = db;
            _logger = logger;
            _walletVirtuelMouvementService = walletVirtuelMouvementService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<WalletVirtuelAgentReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.WalletsVirtuelsAgents
                    .Include(w => w.Agent)
                    .Include(w => w.Devise)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                var dtos = result.Data.Select(WalletVirtuelAgentHelpers.ToReadDto).ToList();

                var paginatedDtos = new PaginatedResponse<WalletVirtuelAgentReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des wallets virtuels d'agents paginés",
                    ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WalletVirtuelAgentReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var wallet = await _db.WalletsVirtuelsAgents
                .Include(w => w.Agent)
                .Include(w => w.Devise)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.IdWalletVirtuelAgent == id, ct);

            if (wallet == null)
                return NotFound();

            return Ok(WalletVirtuelAgentHelpers.ToReadDto(wallet));
        }

        [HttpGet("by-agent/{agentId}")]
        public async Task<ActionResult<WalletVirtuelAgentReadDto>> GetByAgent(int agentId, CancellationToken ct = default)
        {
            var wallet = await _db.WalletsVirtuelsAgents
                .Include(w => w.Agent)
                .Include(w => w.Devise)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.AgentId == agentId, ct);

            if (wallet == null)
                return NotFound();

            return Ok(WalletVirtuelAgentHelpers.ToReadDto(wallet));
        }

        [HttpGet("by-agent/{agentId}/mouvements")]
        public async Task<ActionResult<List<WalletVirtuelMouvementReadDto>>> GetMouvementsByAgent(
            int agentId,
            [FromQuery] WalletVirtuelMouvementFiltreDto filtres,
            CancellationToken ct = default)
        {
            var query = WalletVirtuelMouvementHelpers.MouvementsQuery(_db)
                .Where(m => m.WalletVirtuel!.AgentId == agentId);
            query = WalletVirtuelMouvementHelpers.ApplyFiltres(query, filtres);

            var mouvements = await query
                .OrderByDescending(m => m.DateOperation)
                .ToListAsync(ct);

            var dtos = await WalletVirtuelMouvementHelpers.ToReadDtosAsync(_db, mouvements, ct);
            return Ok(dtos);
        }

        [HttpGet("by-agent/{agentId}/mouvements/paginated")]
        public async Task<ActionResult<PaginatedResponse<WalletVirtuelMouvementReadDto>>> GetMouvementsByAgentPaginated(
            int agentId,
            [FromQuery] PaginationRequest request,
            [FromQuery] WalletVirtuelMouvementFiltreDto filtres,
            CancellationToken ct = default)
        {
            var query = WalletVirtuelMouvementHelpers.MouvementsQuery(_db)
                .Where(m => m.WalletVirtuel!.AgentId == agentId);
            query = WalletVirtuelMouvementHelpers.ApplyFiltres(query, filtres);

            if (string.IsNullOrEmpty(request.SortBy))
                query = query.OrderByDescending(m => m.DateOperation);

            var result = await _paginationService.CreatePaginatedResponseAsync(query, request, ct);
            return Ok(await MapMouvementsPaginatedResponseAsync(result, _db, ct));
        }

        [HttpGet("solde/{agentId}")]
        public async Task<ActionResult<decimal>> GetSoldeVirtuel(int agentId, CancellationToken ct = default)
        {
            var wallet = await _db.WalletsVirtuelsAgents
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.AgentId == agentId, ct);

            if (wallet == null)
                return NotFound();

            return Ok(wallet.SoldeVirtuel);
        }

        /// <summary>
        /// Crédite le wallet virtuel en ajoutant un montant au solde existant (sans remplacement du solde).
        /// </summary>
        [HttpPut("{id:int}/ajouter-solde")]
        public async Task<ActionResult<WalletVirtuelAgentAjouterSoldeResultDto>> AjouterSolde(
            int id,
            [FromBody] WalletVirtuelAgentAjouterSoldeDto dto,
            CancellationToken ct = default)
        {
            if (dto == null)
                return BadRequest("Le corps de la requête est obligatoire.");

            if (dto.Montant <= 0)
                return BadRequest("Le montant doit être supérieur à 0.");

            var wallet = await _db.WalletsVirtuelsAgents
                .FirstOrDefaultAsync(w => w.IdWalletVirtuelAgent == id, ct);

            if (wallet == null)
                return NotFound();

            if (!wallet.Statut)
                return BadRequest("Le wallet virtuel est inactif.");

            if (!await AgentQueryableExtensions.CanRechargeWalletVirtuelAsync(_db, User, wallet.AgentId, ct))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    codeErreur = "HIERARCHIE_RECHARGE_INTERDITE",
                    message = "Vous ne pouvez recharger que le wallet virtuel d'un agent de niveau hiérarchique inférieur au vôtre."
                });
            }

            var ancienSolde = wallet.SoldeVirtuel;
            var nouveauSolde = ancienSolde + dto.Montant;
            wallet.SoldeVirtuel = nouveauSolde;
            wallet.DateModification = DateTime.Now;

            var operateurId = CurrentUserResolver.TryGetCurrentUtilisateurId(User);

            await _walletVirtuelMouvementService.EnregistrerMouvementAsync(
                wallet.IdWalletVirtuelAgent,
                dto.Montant,
                "CREDIT",
                WalletVirtuelMouvementSources.AjoutSolde,
                ancienSolde,
                nouveauSolde,
                operateurId,
                wallet.DeviseId,
                dto.Observation,
                ct: ct);

            await _db.SaveChangesAsync(ct);

            var updatedWallet = await _db.WalletsVirtuelsAgents
                .Include(w => w.Agent)
                .Include(w => w.Devise)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.IdWalletVirtuelAgent == id, ct);

            var result = new WalletVirtuelAgentAjouterSoldeResultDto
            {
                AncienSolde = ancienSolde,
                MontantAjoute = dto.Montant,
                NouveauSolde = updatedWallet!.SoldeVirtuel,
                Wallet = WalletVirtuelAgentHelpers.ToReadDto(updatedWallet!)
            };

            _logger.LogInformation(
                "Crédit wallet virtuel {WalletId}: {AncienSolde} + {Montant} = {NouveauSolde}",
                id, ancienSolde, dto.Montant, result.NouveauSolde);

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<WalletVirtuelAgentReadDto>> Create([FromBody] WalletVirtuelAgentCreateDto createDto, CancellationToken ct = default)
        {
            // Vérifier si un wallet virtuel existe déjà pour cet agent
            var existingWallet = await _db.WalletsVirtuelsAgents
                .FirstOrDefaultAsync(w => w.AgentId == createDto.AgentId, ct);
            
            if (existingWallet != null)
            {
                return BadRequest(new { Message = $"Un wallet virtuel existe déjà pour l'agent ID {createDto.AgentId}" });
            }

            if (createDto.SoldeInitial > 0
                && !await AgentQueryableExtensions.CanRechargeWalletVirtuelAsync(_db, User, createDto.AgentId, ct))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    codeErreur = "HIERARCHIE_RECHARGE_INTERDITE",
                    message = "Vous ne pouvez créditer le wallet virtuel que pour un agent de niveau hiérarchique inférieur au vôtre."
                });
            }

            int deviseId;
            try
            {
                deviseId = await WalletVirtuelAgentHelpers.ResolveDeviseIdAsync(_db, createDto.DeviseId, ct);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            var wallet = new WalletVirtuelAgent
            {
                AgentId = createDto.AgentId,
                DeviseId = deviseId,
                SoldeVirtuel = createDto.SoldeInitial,
                DateCreation = DateTime.Now,
                Statut = createDto.Statut
            };

            _db.WalletsVirtuelsAgents.Add(wallet);
            await _db.SaveChangesAsync(ct);

            if (createDto.SoldeInitial > 0)
            {
                var operateurId = CurrentUserResolver.TryGetCurrentUtilisateurId(User);
                await _walletVirtuelMouvementService.EnregistrerMouvementAsync(
                    wallet.IdWalletVirtuelAgent,
                    createDto.SoldeInitial,
                    "CREDIT",
                    WalletVirtuelMouvementSources.Creation,
                    0m,
                    createDto.SoldeInitial,
                    operateurId,
                    deviseId,
                    "Solde initial à la création",
                    ct: ct);
                await _db.SaveChangesAsync(ct);
            }

            // Récupérer les informations de l'agent
            var walletWithAgent = await _db.WalletsVirtuelsAgents
                .Include(w => w.Agent)
                .Include(w => w.Devise)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.IdWalletVirtuelAgent == wallet.IdWalletVirtuelAgent, ct);

            return CreatedAtAction(
                nameof(GetById),
                new { id = wallet.IdWalletVirtuelAgent },
                WalletVirtuelAgentHelpers.ToReadDto(walletWithAgent!));
        }

        /// <summary>
        /// PUT modifier-solde-wallet-agents — remplace le solde virtuel de plusieurs agents (par agentId). Ne modifie pas le Statut.
        /// Pour créditer sans remplacer le solde, utiliser PUT /{id}/ajouter-solde.
        /// </summary>
        [HttpPut("modifier-solde-wallet-agents")]
        public async Task<ActionResult<WalletVirtuelAgentModifierResultDto>> ModifierSoldeWalletAgents(
            [FromBody] List<WalletVirtuelAgentModifierItemDto> items,
            CancellationToken ct = default)
        {
            if (items == null || items.Count == 0)
                return BadRequest("La liste des modifications est obligatoire.");

            var lastItemByAgent = new Dictionary<int, WalletVirtuelAgentModifierItemDto>();
            var agentOrder = new List<int>();
            foreach (var item in items)
            {
                if (lastItemByAgent.ContainsKey(item.AgentId))
                    agentOrder.Remove(item.AgentId);
                lastItemByAgent[item.AgentId] = item;
                agentOrder.Add(item.AgentId);
            }

            var resultats = new List<WalletVirtuelAgentModifierItemResultDto>();
            var aModifier = new List<(WalletVirtuelAgent Wallet, decimal NouveauSolde, decimal AncienSolde)>();

            foreach (var agentId in agentOrder)
            {
                var item = lastItemByAgent[agentId];
                var resultItem = new WalletVirtuelAgentModifierItemResultDto { AgentId = agentId };

                if (item.AgentId <= 0)
                {
                    resultItem.Message = "Identifiant agent invalide.";
                    resultats.Add(resultItem);
                    continue;
                }

                if (item.SoldeVirtuel < 0)
                {
                    resultItem.Message = "Le solde virtuel ne peut pas être négatif.";
                    resultats.Add(resultItem);
                    continue;
                }

                var wallet = await _db.WalletsVirtuelsAgents
                    .FirstOrDefaultAsync(w => w.AgentId == agentId, ct);

                if (wallet == null)
                {
                    resultItem.Message = "Aucun wallet virtuel pour cet agent.";
                    resultats.Add(resultItem);
                    continue;
                }

                if (!wallet.Statut)
                {
                    resultItem.Message = "Le wallet virtuel est inactif.";
                    resultats.Add(resultItem);
                    continue;
                }

                if (!await AgentQueryableExtensions.CanRechargeWalletVirtuelAsync(_db, User, agentId, ct))
                {
                    resultItem.Message = "HIERARCHIE_RECHARGE_INTERDITE : niveau hiérarchique insuffisant pour ajuster ce wallet.";
                    resultats.Add(resultItem);
                    continue;
                }

                var ancienSolde = wallet.SoldeVirtuel;
                if (wallet.SoldeVirtuel == item.SoldeVirtuel)
                {
                    resultItem.Succes = true;
                    resultItem.IdWalletVirtuelAgent = wallet.IdWalletVirtuelAgent;
                    resultItem.AncienSolde = ancienSolde;
                    resultItem.NouveauSolde = item.SoldeVirtuel;
                    resultItem.Message = "Solde déjà à la valeur demandée.";
                    resultats.Add(resultItem);
                    continue;
                }

                aModifier.Add((wallet, item.SoldeVirtuel, ancienSolde));
            }

            if (aModifier.Count > 0)
            {
                var operateurId = CurrentUserResolver.TryGetCurrentUtilisateurId(User);
                await using var tx = await _db.Database.BeginTransactionAsync(ct);
                try
                {
                    var now = DateTime.Now;
                    foreach (var (wallet, nouveauSolde, ancienSolde) in aModifier)
                    {
                        await _walletVirtuelMouvementService.EnregistrerDeltaSoldeAsync(
                            wallet.IdWalletVirtuelAgent,
                            ancienSolde,
                            nouveauSolde,
                            WalletVirtuelMouvementSources.AjustementSolde,
                            operateurId,
                            wallet.DeviseId,
                            $"Ajustement solde agent {wallet.AgentId}",
                            ct: ct);

                        wallet.SoldeVirtuel = nouveauSolde;
                        wallet.DateModification = now;

                        resultats.Add(new WalletVirtuelAgentModifierItemResultDto
                        {
                            AgentId = wallet.AgentId,
                            Succes = true,
                            IdWalletVirtuelAgent = wallet.IdWalletVirtuelAgent,
                            AncienSolde = ancienSolde,
                            NouveauSolde = nouveauSolde,
                            Message = "Solde modifié avec succès."
                        });
                    }

                    await _db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            }

            var resultByAgent = resultats.ToDictionary(r => r.AgentId);
            var orderedResultats = agentOrder
                .Where(id => resultByAgent.ContainsKey(id))
                .Select(id => resultByAgent[id])
                .ToList();

            var totalReussites = orderedResultats.Count(r => r.Succes);
            var response = new WalletVirtuelAgentModifierResultDto
            {
                TotalDemandes = agentOrder.Count,
                TotalReussites = totalReussites,
                TotalEchecs = agentOrder.Count - totalReussites,
                Resultats = orderedResultats
            };

            if (totalReussites == 0)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Déprécié — préférer PUT modifier-solde-wallet-agents pour modifier par agentId en batch.
        /// Met à jour le solde et le statut d'un wallet par IdWalletVirtuelAgent.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<WalletVirtuelAgentReadDto>> Update(int id, [FromBody] WalletVirtuelAgentUpdateDto updateDto, CancellationToken ct = default)
        {
            var wallet = await _db.WalletsVirtuelsAgents
                .FirstOrDefaultAsync(w => w.IdWalletVirtuelAgent == id, ct);

            if (wallet == null)
                return NotFound();

            if (!await AgentQueryableExtensions.CanRechargeWalletVirtuelAsync(_db, User, wallet.AgentId, ct))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    codeErreur = "HIERARCHIE_RECHARGE_INTERDITE",
                    message = "Vous ne pouvez ajuster que le wallet virtuel d'un agent de niveau hiérarchique inférieur au vôtre."
                });
            }

            var ancienSolde = wallet.SoldeVirtuel;
            wallet.SoldeVirtuel = updateDto.SoldeVirtuel;
            wallet.Statut = updateDto.Statut;
            wallet.DateModification = DateTime.Now;

            var operateurId = CurrentUserResolver.TryGetCurrentUtilisateurId(User);
            await _walletVirtuelMouvementService.EnregistrerDeltaSoldeAsync(
                wallet.IdWalletVirtuelAgent,
                ancienSolde,
                updateDto.SoldeVirtuel,
                WalletVirtuelMouvementSources.AjustementSolde,
                operateurId,
                wallet.DeviseId,
                $"Mise à jour wallet virtuel {id}",
                ct: ct);

            await _db.SaveChangesAsync(ct);

            var updatedWallet = await _db.WalletsVirtuelsAgents
                .Include(w => w.Agent)
                .Include(w => w.Devise)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.IdWalletVirtuelAgent == wallet.IdWalletVirtuelAgent, ct);

            return Ok(WalletVirtuelAgentHelpers.ToReadDto(updatedWallet!));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            var wallet = await _db.WalletsVirtuelsAgents
                .FirstOrDefaultAsync(w => w.IdWalletVirtuelAgent == id, ct);

            if (wallet == null)
                return NotFound();

            _db.WalletsVirtuelsAgents.Remove(wallet);
            await _db.SaveChangesAsync(ct);

            return NoContent();
        }

        private static async Task<PaginatedResponse<WalletVirtuelMouvementReadDto>> MapMouvementsPaginatedResponseAsync(
            PaginatedResponse<WalletVirtuelMouvement> result,
            ProsocDbContext db,
            CancellationToken ct) =>
            new()
            {
                Data = await WalletVirtuelMouvementHelpers.ToReadDtosAsync(db, result.Data, ct),
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage
            };
    }
}
