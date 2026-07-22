using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProsocAPI.Extensions;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;
using Prosoc.Utilities;
using ProsocAPI.Services.Synchronization;
using Prosoc.Data;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AgentController : BaseApiController
    {
        private readonly IAgentRepository _agentRepository;
        private readonly IAdhesionRepository _adhesionRepository;
        private readonly ProsocDbContext _db;
        private readonly IUserSynchronizationService _synchronizationService;

        public AgentController(
            IAgentRepository agentRepository,
            IAdhesionRepository adhesionRepository,
            ProsocDbContext db,
            IUserSynchronizationService synchronizationService,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<AgentController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _agentRepository = agentRepository;
            _adhesionRepository = adhesionRepository;
            _db = db;
            _synchronizationService = synchronizationService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<AgentReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var callerNiveau = await AgentQueryableExtensions.ResolveCallerMinNiveauAsync(_db, User);
                var query = _db.Agents
                    .Include(a => a.Zone)
                    .Include(a => a.CategorieAgent)
                    .Include(a => a.Wallets).ThenInclude(w => w.Devise)
                    .Include(a => a.WalletVirtuel)
                    .Include(a => a.AdhesionsCrees)
                    .AsQueryable()
                    .ApplyRoleNiveauVisibility(_db, callerNiveau);

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);
                var dtos = result.Data.Select(AgentDtoMapper.ToReadDto).ToList();

                var paginatedDtos = new PaginatedResponse<AgentReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des agents paginés",
                    ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AgentReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var callerNiveau = await AgentQueryableExtensions.ResolveCallerMinNiveauAsync(_db, User, ct);
            if (!await AgentQueryableExtensions.IsAgentVisibleAsync(_db, id, callerNiveau, ct))
                return NotFound();

            var agent = await _agentRepository.GetByIdAsync(id, ct);
            if (agent == null)
                return NotFound();

            return Ok(AgentDtoMapper.ToReadDto(agent));
        }

        [HttpPost]
        public async Task<ActionResult<AgentReadDto>> Create([FromBody] AgentCreateDto createDto, CancellationToken ct = default)
        {
            var agent = new Agent
            {
                NomComplet = createDto.NomComplet,
                Matricule = createDto.Matricule ?? string.Empty,
                Phone = createDto.Phone,
                EmailAgent = createDto.EmailAgent,
                Fonction = createDto.Fonction,
                RoleAgent = createDto.RoleAgent ?? "Agent", // 🆕 Valeur par défaut
                PhotoUrl = createDto.PhotoUrl,
                ZoneSocialeId = createDto.ZoneSocialeId,
                CategorieAgentId = createDto.CategorieAgentId,
                Statut = createDto.Statut,
                DateCreation = DateTime.Now
            };

            if (createDto.Matricule == null)
                agent.Matricule = string.Empty;
            else
                agent.Matricule = createDto.Matricule;

            try
            {
                var created = await _agentRepository.CreateAsync(agent, ct);
                var createdWithNav = await _agentRepository.GetByIdAsync(created.IdAgent, ct) ?? created;

                var utilisateur = await _db.Utilisateurs
                    .FirstOrDefaultAsync(u => u.AgentId == created.IdAgent, ct);

                var dto = AgentDtoMapper.ToReadDto(createdWithNav);
                dto.UtilisateurId = utilisateur?.IdUtilisateur;
                dto.NomUtilisateur = utilisateur?.NomUtilisateur;
                dto.UtilisateurCree = utilisateur != null;

                return CreatedAtAction(nameof(GetById), new { id = created.IdAgent }, dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AgentReadDto>> Update(int id, [FromBody] AgentUpdateDto updateDto, CancellationToken ct = default)
        {
            var agent = new Agent
            {
                NomComplet = updateDto.NomComplet,
                Matricule = updateDto.Matricule ?? string.Empty,
                Phone = updateDto.Phone,
                EmailAgent = updateDto.EmailAgent,
                Fonction = updateDto.Fonction,
                RoleAgent = updateDto.RoleAgent,
                PhotoUrl = updateDto.PhotoUrl,
                ZoneSocialeId = updateDto.ZoneSocialeId,
                CategorieAgentId = updateDto.CategorieAgentId,
                Statut = updateDto.Statut,
                DateModification = DateTime.Now
            };

            Agent? updated;
            try
            {
                updated = await _agentRepository.UpdateAsync(id, agent, ct);
                if (updated == null)
                    return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            // 🆕 SYNCHRONISATION AUTOMATIQUE VERS L'UTILISATEUR
            try
            {
                await _synchronizationService.SynchronizeFromAgentAsync(id, ct);
                _logger.LogInformation("Synchronisation automatique déclenchée pour l'agent {AgentId}", id);
            }
            catch (Exception syncEx)
            {
                _logger.LogWarning(syncEx, "Erreur lors de la synchronisation automatique de l'agent {AgentId}", id);
                // Ne pas bloquer l'opération principale en cas d'erreur de synchronisation
            }

            var refreshed = await _agentRepository.GetByIdAsync(id, ct) ?? updated;
            return Ok(AgentDtoMapper.ToReadDto(refreshed));
        }

        /// <summary>
        /// Affecte ou désaffecte la zone sociale d'un agent (zoneSocialeId null = désaffectation).
        /// </summary>
        [HttpPut("{agentId:int}/affecter-zone-sociale")]
        public async Task<ActionResult<AgentReadDto>> AffecterZoneSociale(
            int agentId,
            [FromBody] AgentAffecterZoneSocialeDto dto,
            CancellationToken ct = default)
        {
            if (dto == null)
                return BadRequest("Le corps de la requête est obligatoire.");

            try
            {
                var updated = await _agentRepository.AffecterZoneSocialeAsync(agentId, dto.ZoneSocialeId, ct);
                if (updated == null)
                    return NotFound($"Agent {agentId} introuvable.");

                return Ok(AgentDtoMapper.ToReadDto(updated));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Affecte un ou plusieurs affiliés à un agent via mise à jour des adhésions (transfert si déjà assigné ailleurs).
        /// Mode massif : fournir sourceAgentId avec affilieIds vide pour transférer tout le portefeuille actif.
        /// </summary>
        [HttpPut("{agentId:int}/affecter-affilies")]
        [Authorize(Roles = "Admin,Superviseur")]
        public async Task<ActionResult<AgentAffecterAffiliesResultDto>> AffecterAffilies(
            int agentId,
            [FromBody] AgentAffecterAffiliesDto dto,
            CancellationToken ct = default)
        {
            if (dto == null)
                return BadRequest("Le corps de la requête est obligatoire.");

            var hasAffilieList = dto.AffilieIds != null && dto.AffilieIds.Count > 0;
            if (!hasAffilieList && !dto.SourceAgentId.HasValue)
                return BadRequest("Fournissez une liste d'affiliés ou un agent source.");

            if (dto.SourceAgentId.HasValue && dto.SourceAgentId.Value == agentId)
                return BadRequest("L'agent source et l'agent cible doivent être différents.");

            if (dto.SourceAgentId.HasValue)
            {
                var sourceExists = await _db.Agents.AnyAsync(a => a.IdAgent == dto.SourceAgentId.Value, ct);
                if (!sourceExists)
                    return NotFound($"Agent source {dto.SourceAgentId.Value} introuvable.");
            }

            var affilieIds = dto.AffilieIds ?? new List<int>();
            var result = await _adhesionRepository.AffecterAffiliesToAgentAsync(
                agentId, affilieIds, dto.SourceAgentId, ct);
            if (result == null)
                return NotFound($"Agent {agentId} introuvable.");

            if (result.TotalReussites == 0)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            var success = await _agentRepository.DeleteAsync(id, ct);
            if (!success)
                return NotFound();
            
            return NoContent();
        }

        [HttpGet("{agentId}/affilies")]
        public async Task<ActionResult<PaginatedResponse<AgentAffilieReadDto>>> GetAffiliesByAgent(
            int agentId, 
            [FromQuery] PaginationRequest request,
            CancellationToken ct = default)
        {
            try
            {
                // Construire la requête de base pour les adhésions
                var adhesionsQuery = _db.Adhesions
                    .Include(a => a.Affilie)
                    .Include(a => a.TypeAdhesion)
                    .Where(a => a.AgentId == agentId && a.Statut == true && a.Affilie.Statut == true)
                    .OrderByDescending(a => a.DateCreation)
                    .AsNoTracking();

                // Appliquer la pagination sur les adhésions
                var paginatedAdhesions = await _paginationService.CreatePaginatedResponseAsync(adhesionsQuery, request, ct);

                // Préparer les DTOs
                var dtos = new List<AgentAffilieReadDto>();

                foreach (var adhesion in paginatedAdhesions.Data)
                {
                    // Récupérer les statistiques de collecte pour cet affilié
                    var collectes = await _db.Collectes
                        .Where(c => c.AffilieId == adhesion.AffilieId)
                        .Include(c => c.SouscriptionPrestationRef)
                        .AsNoTracking()
                        .ToListAsync(ct);

                    var dto = new AgentAffilieReadDto
                    {
                        IdAffilie = adhesion.Affilie.IdAffilie,
                        Nom = adhesion.Affilie.Nom ?? string.Empty,
                        Prenom = adhesion.Affilie.Prenom ?? string.Empty,
                        Phone = adhesion.Affilie.Telephone ?? string.Empty,
                        Email = string.Empty, // Affilie n'a pas de propriété Email
                        Matricule = adhesion.Affilie.CodeAdhesion ?? string.Empty,
                        DateAdhesion = adhesion.DateCreation,
                        DateCreationAffilie = adhesion.Affilie.DateCreation,
                        StatutAffilie = adhesion.Affilie.Statut,
                        StatutAdhesion = adhesion.Statut,
                        StatutDossier = adhesion.StatutDossier,
                        TypeAdhesion = adhesion.TypeAdhesion?.Libelle ?? string.Empty,
                        NombreCollectes = collectes.Count,
                        TotalCollectes = collectes.Sum(c => c.Montant),
                        TotalCommissions = collectes.Sum(c => c.Montant * 0.25m), // 25% de commission
                        DerniereCollecte = collectes.OrderByDescending(c => c.DateCollecte).FirstOrDefault()?.DateCollecte
                    };

                    dtos.Add(dto);
                }

                // Créer la réponse paginée
                var paginatedDtos = new PaginatedResponse<AgentAffilieReadDto>
                {
                    Data = dtos,
                    CurrentPage = paginatedAdhesions.CurrentPage,
                    PageSize = paginatedAdhesions.PageSize,
                    TotalItems = paginatedAdhesions.TotalItems,
                    TotalPages = paginatedAdhesions.TotalPages,
                    HasNextPage = paginatedAdhesions.HasNextPage,
                    HasPreviousPage = paginatedAdhesions.HasPreviousPage
                };

                return Ok(paginatedDtos);
            }
            catch (Exception ex)
            {
                base._logger.LogError(ex, "Erreur lors de la récupération des affiliés pour l'agent {AgentId}", agentId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des affiliés", ex);
            }
        }

    /// <summary>
    /// Récupère les agents paginés
    /// </summary>
    [HttpGet("paginated")]
    public async Task<ActionResult<PaginatedResponse<AgentReadDto>>> GetPaginated(
        [FromQuery] PaginationRequest request)
    {
        try
        {
            var callerNiveau = await AgentQueryableExtensions.ResolveCallerMinNiveauAsync(_db, User);
            var query = _db.Agents
                .Include(a => a.Zone)
                .Include(a => a.CategorieAgent)
                .Include(a => a.Wallets).ThenInclude(w => w.Devise)
                .Include(a => a.WalletVirtuel)
                .AsQueryable()
                .ApplyRoleNiveauVisibility(_db, callerNiveau);

            var result = await _paginationService.CreatePaginatedResponseAsync(query, request);
            var dtos = result.Data.Select(AgentDtoMapper.ToReadDto).ToList();

            var paginatedDtos = new PaginatedResponse<AgentReadDto>
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
                    "Une erreur technique est survenue lors de la récupération des agents paginés",
                    ex);
            }
    }

    /// <summary>
    /// Récupère les agents avec filtres avancés
    /// </summary>
    [HttpPost("advanced")]
    public async Task<ActionResult<ExtendedPaginatedResponse<AgentReadDto>>> GetAgentsAdvanced(
        [FromBody] AdvancedPaginationRequest request)
    {
        try
        {
            // Construire la requête de base
            var callerNiveau = await AgentQueryableExtensions.ResolveCallerMinNiveauAsync(_db, User);
            var query = _db.Agents
                .Include(a => a.Zone)
                .Include(a => a.CategorieAgent)
                .Include(a => a.Wallets).ThenInclude(w => w.Devise)
                .Include(a => a.WalletVirtuel)
                .AsQueryable()
                .ApplyRoleNiveauVisibility(_db, callerNiveau);

            // Appliquer les filtres de base
            if (request.FilterList != null && request.FilterList.Any())
            {
                foreach (var filter in request.FilterList)
                {
                    switch (filter.Field.ToLower())
                    {
                        case "statut":
                            if (filter.Operator == "eq")
                                query = query.Where(a => a.Statut.ToString() == filter.Value);
                            break;
                        case "nomcomplet":
                            if (filter.Operator == "contains")
                                query = query.Where(a => a.NomComplet.Contains(filter.Value));
                            else if (filter.Operator == "eq")
                                query = query.Where(a => a.NomComplet == filter.Value);
                            break;
                        case "matricule":
                            if (filter.Operator == "contains")
                                query = query.Where(a => a.Matricule.Contains(filter.Value));
                            else if (filter.Operator == "eq")
                                query = query.Where(a => a.Matricule == filter.Value);
                            break;
                        case "emailagent":
                            if (filter.Operator == "contains")
                                query = query.Where(a => a.EmailAgent.Contains(filter.Value));
                            break;
                        case "fonction":
                            if (filter.Operator == "contains")
                                query = query.Where(a => a.Fonction.Contains(filter.Value));
                            else if (filter.Operator == "eq")
                                query = query.Where(a => a.Fonction == filter.Value);
                            break;
                        case "roleagent":
                            if (filter.Operator == "eq")
                                query = query.Where(a => a.RoleAgent == filter.Value);
                            break;
                        case "zonesocialeid":
                            if (filter.Operator == "eq")
                                query = query.Where(a => a.ZoneSocialeId == int.Parse(filter.Value));
                            break;
                        case "walletsolde":
                            if (filter.Operator == "gt")
                                query = query.Where(a => a.Wallets.Any(w => w.SoldeCourant > decimal.Parse(filter.Value)));
                            else if (filter.Operator == "lt")
                                query = query.Where(a => a.Wallets.Any(w => w.SoldeCourant < decimal.Parse(filter.Value)));
                            break;
                    }
                }
            }

            // Appliquer la pagination
            var response = await _paginationService.CreateExtendedPaginatedResponseAsync(query, request);

            // Mapper les entités vers les DTOs
            var agentDtos = response.Data.Select(AgentDtoMapper.ToReadDto).ToList();
            
            // Créer une nouvelle réponse avec les DTOs
            var dtoResponse = new ExtendedPaginatedResponse<AgentReadDto>
            {
                Data = agentDtos,
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
                    "Une erreur technique est survenue lors de la récupération des agents avancés",
                    ex);
            }
    }
}
}
