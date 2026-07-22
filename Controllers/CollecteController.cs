using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.DTOs.FlexPay;
using ProsocAPI.Models.Pagination;
using Prosoc.Utilities;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;
using Prosoc.Data;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CollecteController : BaseApiController
    {
        private readonly ICollecteRepository _collecteRepository;
        private readonly IFlexPayCollecteService _flexPayCollecteService;
        private readonly ProsocDbContext _db;

        public CollecteController(
            ICollecteRepository collecteRepository,
            IFlexPayCollecteService flexPayCollecteService,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<CollecteController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _collecteRepository = collecteRepository;
            _flexPayCollecteService = flexPayCollecteService;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<CollecteReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.Collectes
                    .Include(c => c.Affilie)
                    .Include(c => c.Agent)
                    .Include(c => c.Devise)
                    .Include(c => c.Frais) // NOUVEAU
                    .Include(c => c.CotisationAffilie)
                    .Include(c => c.SouscriptionPrestationRef)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                var dtos = result.Data.Select(MapToCollecteReadDto).ToList();

                var paginatedDtos = new PaginatedResponse<CollecteReadDto>
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
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la récupération des collectes paginées",
                    ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CollecteReadDto>> GetById(int id, CancellationToken ct = default)
        {
            var collecte = await _collecteRepository.GetByIdAsync(id, ct);
            if (collecte == null)
                return NotFound();

            return Ok(MapToCollecteReadDto(collecte));
        }

        [HttpGet("by-affilie/{affilieId}/simple")]
        public async Task<ActionResult<List<CollecteReadDto>>> GetByAffilie(int affilieId, CancellationToken ct = default)
        {
            var collectes = await _collecteRepository.GetByAffilieAsync(affilieId, ct);
            var dtos = collectes.Select(c => new CollecteReadDto
            {
                IdCollecte = c.IdCollecte,
                    // NOUVEAU : Type de collecte
                    TypeCollecte = c.TypeCollecte,
                    // NOUVEAU : Relation avec Frais
                    FraisId = c.FraisId,
                    FraisLibelle = c.Frais?.Libelle,
                    FraisMontant = c.Frais?.Montant,
                CotisationAffilieLibelle = c.CotisationAffilie?.LibelleTarifCotisation,
                AffilieId = c.AffilieId,
                AffilieNom = $"{c.Affilie?.Nom} {c.Affilie?.Prenom}".Trim(),
                AgentId = c.AgentId,
                AgentNom = c.Agent?.NomComplet,
                Montant = c.Montant,
                ReferencePaiement = c.ReferencePaiement,
                ModePaiement = c.ModePaiement,
                Operateur = c.Operateur,
                StatutPaiement = c.StatutPaiement,
                SouscriptionPrestationId = c.SouscriptionPrestationRef?.IdSouscriptionPrestation,
                MontantRecu = c.MontantRecu,
                MontantAttendu = c.MontantAttendu,
                DeviseId = c.DeviseId,
                DeviseNom = c.Devise?.Nom,
                DeviseCode = c.Devise?.Code,
                DateCollecte = c.DateCollecte,
                Observation = c.Observation,
                DateCreation = c.DateCreation,
                DateModification = c.DateModification,
                Statut = c.Statut
            }).ToList();
            return Ok(dtos);
        }

        [HttpGet("by-agent/{agentId}")]
        public async Task<ActionResult<List<CollecteReadDto>>> GetByAgent(int agentId, CancellationToken ct = default)
        {
            var scopeDenied = await ChefEquipeZoneScopeHelper.EnsureAgentDansMaZoneAsync(User, _db, agentId, ct);
            if (scopeDenied is not null)
                return scopeDenied;

            var collectes = await _collecteRepository.GetByAgentAsync(agentId, ct);
            var dtos = collectes.Select(c => new CollecteReadDto
            {
                IdCollecte = c.IdCollecte,
                    // NOUVEAU : Type de collecte
                    TypeCollecte = c.TypeCollecte,
                    // NOUVEAU : Relation avec Frais
                    FraisId = c.FraisId,
                    FraisLibelle = c.Frais?.Libelle,
                    FraisMontant = c.Frais?.Montant,
                CotisationAffilieLibelle = c.CotisationAffilie?.LibelleTarifCotisation,
                AffilieId = c.AffilieId,
                AffilieNom = $"{c.Affilie?.Nom} {c.Affilie?.Prenom}".Trim(),
                AgentId = c.AgentId,
                AgentNom = c.Agent?.NomComplet,
                Montant = c.Montant,
                ReferencePaiement = c.ReferencePaiement,
                ModePaiement = c.ModePaiement,
                Operateur = c.Operateur,
                StatutPaiement = c.StatutPaiement,
                SouscriptionPrestationId = c.SouscriptionPrestationRef?.IdSouscriptionPrestation,
                MontantRecu = c.MontantRecu,
                MontantAttendu = c.MontantAttendu,
                DeviseId = c.DeviseId,
                DeviseNom = c.Devise?.Nom,
                DeviseCode = c.Devise?.Code,
                DateCollecte = c.DateCollecte,
                Observation = c.Observation,
                DateCreation = c.DateCreation,
                DateModification = c.DateModification,
                Statut = c.Statut
            }).ToList();
            return Ok(dtos);
        }

        [HttpGet("by-devise/{deviseId}")]
        public async Task<ActionResult<List<CollecteReadDto>>> GetByDevise(int deviseId, CancellationToken ct = default)
        {
            var collectes = await _collecteRepository.GetByDeviseAsync(deviseId, ct);
            var dtos = collectes.Select(c => new CollecteReadDto
            {
                IdCollecte = c.IdCollecte,
                    // NOUVEAU : Type de collecte
                    TypeCollecte = c.TypeCollecte,
                    // NOUVEAU : Relation avec Frais
                    FraisId = c.FraisId,
                    FraisLibelle = c.Frais?.Libelle,
                    FraisMontant = c.Frais?.Montant,
                CotisationAffilieLibelle = c.CotisationAffilie?.LibelleTarifCotisation,
                AffilieId = c.AffilieId,
                AffilieNom = $"{c.Affilie?.Nom} {c.Affilie?.Prenom}".Trim(),
                AgentId = c.AgentId,
                AgentNom = c.Agent?.NomComplet,
                Montant = c.Montant,
                ReferencePaiement = c.ReferencePaiement,
                ModePaiement = c.ModePaiement,
                Operateur = c.Operateur,
                StatutPaiement = c.StatutPaiement,
                SouscriptionPrestationId = c.SouscriptionPrestationRef?.IdSouscriptionPrestation,
                MontantRecu = c.MontantRecu,
                MontantAttendu = c.MontantAttendu,
                DeviseId = c.DeviseId,
                DeviseNom = c.Devise?.Nom,
                DeviseCode = c.Devise?.Code,
                DateCollecte = c.DateCollecte,
                Observation = c.Observation,
                DateCreation = c.DateCreation,
                DateModification = c.DateModification,
                Statut = c.Statut
            }).ToList();
            return Ok(dtos);
        }

        [HttpGet("by-date-range")]
        public async Task<ActionResult<List<CollecteReadDto>>> GetByDateRange([FromQuery] DateTime debut, [FromQuery] DateTime fin, CancellationToken ct = default)
        {
            var collectes = await _collecteRepository.GetByDateRangeAsync(debut, fin, ct);
            var dtos = collectes.Select(c => new CollecteReadDto
            {
                IdCollecte = c.IdCollecte,
                    // NOUVEAU : Type de collecte
                    TypeCollecte = c.TypeCollecte,
                    // NOUVEAU : Relation avec Frais
                    FraisId = c.FraisId,
                    FraisLibelle = c.Frais?.Libelle,
                    FraisMontant = c.Frais?.Montant,
                CotisationAffilieLibelle = c.CotisationAffilie?.LibelleTarifCotisation,
                AffilieId = c.AffilieId,
                AffilieNom = $"{c.Affilie?.Nom} {c.Affilie?.Prenom}".Trim(),
                AgentId = c.AgentId,
                AgentNom = c.Agent?.NomComplet,
                Montant = c.Montant,
                ReferencePaiement = c.ReferencePaiement,
                ModePaiement = c.ModePaiement,
                Operateur = c.Operateur,
                StatutPaiement = c.StatutPaiement,
                SouscriptionPrestationId = c.SouscriptionPrestationRef?.IdSouscriptionPrestation,
                MontantRecu = c.MontantRecu,
                MontantAttendu = c.MontantAttendu,
                DeviseId = c.DeviseId,
                DeviseNom = c.Devise?.Nom,
                DeviseCode = c.Devise?.Code,
                DateCollecte = c.DateCollecte,
                Observation = c.Observation,
                DateCreation = c.DateCreation,
                DateModification = c.DateModification,
                Statut = c.Statut
            }).ToList();
            return Ok(dtos);
        }

        [HttpGet("stats")]
        public async Task<ActionResult<CollecteStatsDto>> GetStats(CancellationToken ct = default)
        {
            var stats = await _collecteRepository.GetStatsAsync(ct);
            return Ok(stats);
        }

        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] CollecteCreateDto createDto, CancellationToken ct = default)
        {
            if (MethodePaiementHelper.IsFlexPay(createDto.ModePaiement))
            {
                try
                {
                    var flexResult = await _flexPayCollecteService.InitiateAgentCollecteAsync(
                        createDto, createDto.Phone, CollecteEnAttenteSourceFlux.CollecteAgent, ct);
                    return Ok(flexResult);
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            MethodePaiementHelper.EnsureGuichetSyncOnly(createDto.ModePaiement);

            try
            {
                WalletVirtuelPaiementAutorisation.EnsureSiVirtualAccount(createDto.ModePaiement, User);

                var collecte = new Collecte
                {
                    // ✅ CORRECTION : Ajout du TypeCollecte manquant
                    TypeCollecte = createDto.TypeCollecte,
                    FraisId = createDto.FraisId,
                    CotisationAffilieId = createDto.CotisationAffilieId,
                    AffilieId = createDto.AffilieId,
                    AgentId = createDto.AgentId,
                    OperateurUtilisateurId = CurrentUserResolver.TryGetCurrentUtilisateurId(User),
                    Montant = createDto.Montant,
                    ReferencePaiement = createDto.ReferencePaiement,
                    ModePaiement = createDto.ModePaiement,
                    Operateur = createDto.Operateur,
                    StatutPaiement = CollecteStatutPaiementRegles.NormaliserPourEcriture(createDto.StatutPaiement),
                    SouscriptionPrestationId = createDto.SouscriptionPrestationId,
                    MontantRecu = createDto.MontantRecu,
                    MontantAttendu = createDto.MontantAttendu,
                    DeviseId = createDto.DeviseId,
                    DateCollecte = DateTime.Now,
                    Observation = createDto.Observation,
                    Statut = createDto.Statut,
                    DateCreation = DateTime.Now
                };

                var created = await _collecteRepository.CreateAsync(collecte, ct);
                var reloaded = await _collecteRepository.GetByIdAsync(created.IdCollecte, ct);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = created.IdCollecte },
                    MapToCollecteReadDto(reloaded ?? created));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Initie une collecte FlexPay (public) : la collecte n'est créée qu'au callback succès.
        /// </summary>
        [HttpPost("with-paiement-electronique")]
        [AllowAnonymous]
        public async Task<ActionResult<InitiateFlexPayResponseDto>> CreateWithPaiementElectronique(
            [FromBody] CollecteWithPaiementElectroniqueCreateDto request,
            CancellationToken ct = default)
        {
            if (request?.Collecte == null)
                return BadRequest("Le payload de collecte est obligatoire.");

            var modeNormalized = MethodePaiementHelper.NormalizeForStorage(request.ModePaiement);
            if (!MethodePaiementHelper.IsFlexPay(modeNormalized))
            {
                return BadRequest(
                    "ModePaiement invalide pour cet endpoint. Valeurs autorisées : MOBILE_MONEY, CARTE_BANCAIRE.");
            }

            if (modeNormalized == MethodePaiementHelper.MobileMoney &&
                string.IsNullOrWhiteSpace(request.TelephonePaiement))
            {
                return BadRequest("TelephonePaiement est obligatoire pour MOBILE_MONEY.");
            }

            if (request.Collecte.DeviseId != request.DevisePaiementId)
            {
                return BadRequest(
                    "DevisePaiementId doit correspondre à la devise utilisée dans la collecte.");
            }

            request.Collecte.ModePaiement = modeNormalized;
            request.Collecte.DeviseId = request.DevisePaiementId;
            request.Collecte.Phone = request.TelephonePaiement?.Trim();

            if (string.IsNullOrWhiteSpace(request.Collecte.StatutPaiement))
                request.Collecte.StatutPaiement = CollecteStatutPaiement.EnAttente;
            else
                request.Collecte.StatutPaiement = CollecteStatutPaiementRegles.NormaliserPourEcriture(request.Collecte.StatutPaiement);

            try
            {
                var response = await _flexPayCollecteService.InitiateAgentCollecteAsync(
                    request.Collecte,
                    request.TelephonePaiement,
                    CollecteEnAttenteSourceFlux.CollectePaiementElectroniquePublic,
                    ct);
                return Accepted(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CollecteReadDto>> Update(int id, [FromBody] CollecteUpdateDto updateDto, CancellationToken ct = default)
        {
            if (!HasExplicitPermission("UPDATE_COLLECTE"))
                return ForbiddenPermission("UPDATE_COLLECTE");

            var collecte = new Collecte
            {
                // ✅ CORRECTION : Ajout du TypeCollecte manquant
                TypeCollecte = updateDto.TypeCollecte,
                FraisId = updateDto.FraisId,
                CotisationAffilieId = updateDto.CotisationAffilieId,
                AffilieId = updateDto.AffilieId,
                AgentId = updateDto.AgentId,
                Montant = updateDto.Montant,
                ReferencePaiement = updateDto.ReferencePaiement,
                ModePaiement = updateDto.ModePaiement,
                Operateur = updateDto.Operateur,
                StatutPaiement = updateDto.StatutPaiement,
                SouscriptionPrestationId = updateDto.SouscriptionPrestationId,
                MontantRecu = updateDto.MontantRecu,
                MontantAttendu = updateDto.MontantAttendu,
                DeviseId = updateDto.DeviseId,
                Observation = updateDto.Observation,
                Statut = updateDto.Statut,
                DateModification = DateTime.Now
            };

            var updated = await _collecteRepository.UpdateAsync(id, collecte, ct);
            if (updated == null)
                return NotFound();

            var reloaded = await _collecteRepository.GetByIdAsync(id, ct);
            return Ok(MapToCollecteReadDto(reloaded ?? updated));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            var success = await _collecteRepository.DeleteAsync(id, ct);
            if (!success)
                return NotFound();
            
            return NoContent();
        }

        /// <summary>
        /// Récupère les collectes avec filtres avancés
        /// </summary>
        [HttpPost("advanced")]
        public async Task<ActionResult<ExtendedPaginatedResponse<CollecteReadDto>>> GetCollectesAdvanced(
            [FromBody] AdvancedPaginationRequest request)
        {
            try
            {
                // Construire la requête de base
                var query = _db.Collectes
                    .Include(c => c.Affilie)
                    .Include(c => c.Agent)
                    .Include(c => c.Devise)
                    .Include(c => c.CotisationAffilie)
                    .Include(c => c.SouscriptionPrestationRef)
                    .AsQueryable();

                // Appliquer les filtres de base
                if (request.FilterList != null && request.FilterList.Any())
                {
                    foreach (var filter in request.FilterList)
                    {
                        switch (filter.Field.ToLower())
                        {
                            case "affilieid":
                                if (filter.Operator == "eq")
                                    query = query.Where(c => c.AffilieId == int.Parse(filter.Value));
                                break;
                            case "agentid":
                                if (filter.Operator == "eq")
                                    query = query.Where(c => c.AgentId == int.Parse(filter.Value));
                                break;
                            case "montant":
                                if (filter.Operator == "eq")
                                    query = query.Where(c => c.Montant == decimal.Parse(filter.Value));
                                else if (filter.Operator == "gt")
                                    query = query.Where(c => c.Montant > decimal.Parse(filter.Value));
                                else if (filter.Operator == "lt")
                                    query = query.Where(c => c.Montant < decimal.Parse(filter.Value));
                                break;
                            case "modepaiement":
                                if (filter.Operator == "eq")
                                    query = query.Where(c => c.ModePaiement == filter.Value);
                                else if (filter.Operator == "contains")
                                    query = query.Where(c => c.ModePaiement.Contains(filter.Value));
                                break;
                            case "statutpaiement":
                                if (filter.Operator == "eq")
                                    query = query.Where(c => c.StatutPaiement == filter.Value);
                                break;
                            case "operateur":
                                if (filter.Operator == "contains")
                                    query = query.Where(c => c.Operateur.Contains(filter.Value));
                                break;
                            case "affilienom":
                                if (filter.Operator == "contains")
                                    query = query.Where(c => c.Affilie != null && 
                                        (c.Affilie.Nom.Contains(filter.Value) || c.Affilie.Prenom.Contains(filter.Value)));
                                break;
                            case "agentnom":
                                if (filter.Operator == "contains")
                                    query = query.Where(c => c.Agent != null && c.Agent.NomComplet.Contains(filter.Value));
                                break;
                            case "datecollecte":
                                if (filter.Operator == "eq")
                                    query = query.Where(c => c.DateCollecte.Date == DateTime.Parse(filter.Value).Date);
                                else if (filter.Operator == "gt")
                                    query = query.Where(c => c.DateCollecte > DateTime.Parse(filter.Value));
                                else if (filter.Operator == "lt")
                                    query = query.Where(c => c.DateCollecte < DateTime.Parse(filter.Value));
                                break;
                        }
                    }
                }

                // Appliquer la pagination
                var response = await _paginationService.CreateExtendedPaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs
                var collecteDtos = response.Data.Select(c => new CollecteReadDto
                {
                    IdCollecte = c.IdCollecte,
                    // NOUVEAU : Type de collecte
                    TypeCollecte = c.TypeCollecte,
                    // NOUVEAU : Relation avec Frais
                    FraisId = c.FraisId,
                    FraisLibelle = c.Frais?.Libelle,
                    FraisMontant = c.Frais?.Montant,
                    CotisationAffilieLibelle = c.CotisationAffilie?.LibelleTarifCotisation,
                    AffilieId = c.AffilieId,
                    AffilieNom = $"{c.Affilie?.Nom} {c.Affilie?.Prenom}".Trim(),
                    AgentId = c.AgentId,
                    AgentNom = c.Agent?.NomComplet,
                    Montant = c.Montant,
                    ReferencePaiement = c.ReferencePaiement,
                    ModePaiement = c.ModePaiement,
                    Operateur = c.Operateur,
                    StatutPaiement = c.StatutPaiement,
                    SouscriptionPrestationId = c.SouscriptionPrestationRef?.IdSouscriptionPrestation,
                    MontantRecu = c.MontantRecu,
                    MontantAttendu = c.MontantAttendu,
                    DeviseId = c.DeviseId,
                    DeviseNom = c.Devise?.Nom,
                    DeviseCode = c.Devise?.Code,
                    DateCollecte = c.DateCollecte,
                    Observation = c.Observation,
                    DateCreation = c.DateCreation,
                    DateModification = c.DateModification,
                    Statut = c.Statut
                }).ToList();
                
                // Créer une nouvelle réponse avec les DTOs
                var dtoResponse = new ExtendedPaginatedResponse<CollecteReadDto>
                {
                    Data = collecteDtos,
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
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la récupération des collectes avancées",
                    ex);
            }
        }

        /// <summary>
        /// Récupère les collectes par affilié avec pagination
        /// </summary>
        [HttpGet("by-affilie/{affilieId}/paginated")]
        public async Task<ActionResult<PaginatedResponse<CollecteReadDto>>> GetByAffilie(
            int affilieId, 
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.Collectes
                    .Include(c => c.Affilie)
                    .Include(c => c.Agent)
                    .Include(c => c.Devise)
                    .Include(c => c.CotisationAffilie)
                    .Include(c => c.SouscriptionPrestationRef)
                    .Where(c => c.AffilieId == affilieId)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(c => new CollecteReadDto
                {
                    IdCollecte = c.IdCollecte,
                    // NOUVEAU : Type de collecte
                    TypeCollecte = c.TypeCollecte,
                    // NOUVEAU : Relation avec Frais
                    FraisId = c.FraisId,
                    FraisLibelle = c.Frais?.Libelle,
                    FraisMontant = c.Frais?.Montant,
                    CotisationAffilieLibelle = c.CotisationAffilie?.LibelleTarifCotisation,
                    AffilieId = c.AffilieId,
                    AffilieNom = $"{c.Affilie?.Nom} {c.Affilie?.Prenom}".Trim(),
                    AgentId = c.AgentId,
                    AgentNom = c.Agent?.NomComplet,
                    Montant = c.Montant,
                    ReferencePaiement = c.ReferencePaiement,
                    ModePaiement = c.ModePaiement,
                    Operateur = c.Operateur,
                    StatutPaiement = c.StatutPaiement,
                    SouscriptionPrestationId = c.SouscriptionPrestationRef?.IdSouscriptionPrestation,
                    MontantRecu = c.MontantRecu,
                    MontantAttendu = c.MontantAttendu,
                    DeviseId = c.DeviseId,
                    DeviseNom = c.Devise?.Nom,
                    DeviseCode = c.Devise?.Code,
                    DateCollecte = c.DateCollecte,
                    Observation = c.Observation,
                    DateCreation = c.DateCreation,
                    DateModification = c.DateModification,
                    Statut = c.Statut
                }).ToList();

                var paginatedDtos = new PaginatedResponse<CollecteReadDto>
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
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la récupération des collectes pour l'affilié",
                    ex);
            }
        }
        
        // NOUVEAUX ENDPOINTS POUR LE TYPECOLLECTE
        
        /// <summary>
        /// Récupérer les collectes par type (Frais ou Souscription)
        /// </summary>
        [HttpGet("by-type/{typeCollecte}")]
        public async Task<ActionResult<List<CollecteReadDto>>> GetByType(TypeCollecte typeCollecte, CancellationToken ct = default)
        {
            try
            {
                var collectes = await _db.Collectes
                    .Include(c => c.Affilie)
                    .Include(c => c.Agent)
                    .Include(c => c.Devise)
                    .Include(c => c.Frais)
                    .Include(c => c.CotisationAffilie)
                        .ThenInclude(ca => ca!.TypeAdhesion)
                    .Include(c => c.SouscriptionPrestationRef)
                    .Where(c => c.TypeCollecte == typeCollecte)
                    .OrderByDescending(c => c.DateCreation)
                    .ToListAsync(ct);
                
                var dtos = collectes.Select(c => new CollecteReadDto
                {
                    IdCollecte = c.IdCollecte,
                    TypeCollecte = c.TypeCollecte,
                    FraisId = c.FraisId,
                    FraisLibelle = c.Frais?.Libelle,
                    FraisMontant = c.Frais?.Montant,
                    CotisationAffilieLibelle = c.CotisationAffilie?.LibelleTarifCotisation,
                    AffilieId = c.AffilieId,
                    AffilieNom = $"{c.Affilie?.Nom} {c.Affilie?.Prenom}".Trim(),
                    AgentId = c.AgentId,
                    AgentNom = c.Agent?.NomComplet,
                    Montant = c.Montant,
                    ReferencePaiement = c.ReferencePaiement,
                    ModePaiement = c.ModePaiement,
                    Operateur = c.Operateur,
                    StatutPaiement = c.StatutPaiement,
                    SouscriptionPrestationId = c.SouscriptionPrestationRef?.IdSouscriptionPrestation,
                    MontantRecu = c.MontantRecu,
                    MontantAttendu = c.MontantAttendu,
                    DeviseId = c.DeviseId,
                    DeviseNom = c.Devise?.Nom,
                    DeviseCode = c.Devise?.Code,
                    DateCollecte = c.DateCollecte,
                    Observation = c.Observation,
                    DateCreation = c.DateCreation,
                    DateModification = c.DateModification,
                    Statut = c.Statut
                }).ToList();
                
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la récupération des collectes par type",
                    ex);
            }
        }
        
        /// <summary>
        /// Récupérer les collectes pour un frais spécifique
        /// </summary>
        [HttpGet("by-frais/{fraisId}")]
        public async Task<ActionResult<List<CollecteReadDto>>> GetByFrais(int fraisId, CancellationToken ct = default)
        {
            try
            {
                var collectes = await _db.Collectes
                    .Include(c => c.Affilie)
                    .Include(c => c.Agent)
                    .Include(c => c.Devise)
                    .Include(c => c.Frais)
                    .Include(c => c.CotisationAffilie)
                    .Where(c => c.FraisId == fraisId && c.TypeCollecte == TypeCollecte.Frais)
                    .OrderByDescending(c => c.DateCreation)
                    .ToListAsync(ct);
                
                var dtos = collectes.Select(c => new CollecteReadDto
                {
                    IdCollecte = c.IdCollecte,
                    TypeCollecte = c.TypeCollecte,
                    FraisId = c.FraisId,
                    FraisLibelle = c.Frais?.Libelle,
                    FraisMontant = c.Frais?.Montant,
                    CotisationAffilieLibelle = c.CotisationAffilie?.LibelleTarifCotisation,
                    AffilieId = c.AffilieId,
                    AffilieNom = $"{c.Affilie?.Nom} {c.Affilie?.Prenom}".Trim(),
                    AgentId = c.AgentId,
                    AgentNom = c.Agent?.NomComplet,
                    Montant = c.Montant,
                    ReferencePaiement = c.ReferencePaiement,
                    ModePaiement = c.ModePaiement,
                    Operateur = c.Operateur,
                    StatutPaiement = c.StatutPaiement,
                    SouscriptionPrestationId = c.SouscriptionPrestationRef?.IdSouscriptionPrestation,
                    MontantRecu = c.MontantRecu,
                    MontantAttendu = c.MontantAttendu,
                    DeviseId = c.DeviseId,
                    DeviseNom = c.Devise?.Nom,
                    DeviseCode = c.Devise?.Code,
                    DateCollecte = c.DateCollecte,
                    Observation = c.Observation,
                    DateCreation = c.DateCreation,
                    DateModification = c.DateModification,
                    Statut = c.Statut
                }).ToList();
                
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la récupération des collectes pour les frais",
                    ex);
            }
        }
        
        /// <summary>
        /// Récupérer les collectes pour une souscription spécifique
        /// </summary>
        [HttpGet("by-souscription/{souscriptionPrestationId}")]
        public async Task<ActionResult<List<CollecteReadDto>>> GetBySouscription(int souscriptionPrestationId, CancellationToken ct = default)
        {
            try
            {
                var collectes = await _db.Collectes
                    .Include(c => c.Affilie)
                    .Include(c => c.Agent)
                    .Include(c => c.Devise)
                    .Include(c => c.CotisationAffilie)
                    .Include(c => c.SouscriptionPrestationRef)
                    .Where(c => c.SouscriptionPrestationId == souscriptionPrestationId && c.TypeCollecte == TypeCollecte.Souscription)
                    .OrderByDescending(c => c.DateCreation)
                    .ToListAsync(ct);
                
                var dtos = collectes.Select(c => new CollecteReadDto
                {
                    IdCollecte = c.IdCollecte,
                    TypeCollecte = c.TypeCollecte,
                    FraisId = c.FraisId,
                    FraisLibelle = c.Frais?.Libelle,
                    FraisMontant = c.Frais?.Montant,
                    CotisationAffilieLibelle = c.CotisationAffilie?.LibelleTarifCotisation,
                    AffilieId = c.AffilieId,
                    AffilieNom = $"{c.Affilie?.Nom} {c.Affilie?.Prenom}".Trim(),
                    AgentId = c.AgentId,
                    AgentNom = c.Agent?.NomComplet,
                    Montant = c.Montant,
                    ReferencePaiement = c.ReferencePaiement,
                    ModePaiement = c.ModePaiement,
                    Operateur = c.Operateur,
                    StatutPaiement = c.StatutPaiement,
                    SouscriptionPrestationId = c.SouscriptionPrestationRef?.IdSouscriptionPrestation,
                    MontantRecu = c.MontantRecu,
                    MontantAttendu = c.MontantAttendu,
                    DeviseId = c.DeviseId,
                    DeviseNom = c.Devise?.Nom,
                    DeviseCode = c.Devise?.Code,
                    DateCollecte = c.DateCollecte,
                    Observation = c.Observation,
                    DateCreation = c.DateCreation,
                    DateModification = c.DateModification,
                    Statut = c.Statut
                }).ToList();
                
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la récupération des collectes pour la souscription",
                    ex);
            }
        }

        /// <summary>
        /// Récupérer les collectes pour une cotisation affilié spécifique
        /// </summary>
        [HttpGet("by-cotisation-affilie/{cotisationAffilieId}")]
        public async Task<ActionResult<List<CollecteReadDto>>> GetByCotisationAffilie(int cotisationAffilieId, CancellationToken ct = default)
        {
            try
            {
                var collectes = await _db.Collectes
                    .Include(c => c.Affilie)
                    .Include(c => c.Agent)
                    .Include(c => c.Devise)
                    .Include(c => c.CotisationAffilie)
                        .ThenInclude(ca => ca!.TypeAdhesion)
                    .Where(c => c.CotisationAffilieId == cotisationAffilieId && c.TypeCollecte == TypeCollecte.Cotisation)
                    .OrderByDescending(c => c.DateCreation)
                    .ToListAsync(ct);

                var dtos = collectes.Select(c => new CollecteReadDto
                {
                    IdCollecte = c.IdCollecte,
                    TypeCollecte = c.TypeCollecte,
                    CotisationAffilieId = c.CotisationAffilieId,
                    CotisationAffilieLibelle = c.CotisationAffilie?.LibelleTarifCotisation,
                    CotisationPeriodicite = c.CotisationAffilie?.Periodicite,
                    CotisationMontantReference = c.CotisationAffilie?.Montant,
                    CotisationTypeAdhesionId = c.CotisationAffilie?.TypeAdhesionId,
                    CotisationTypeAdhesionLibelle = c.CotisationAffilie?.TypeAdhesion?.Libelle,
                    AffilieId = c.AffilieId,
                    AffilieNom = $"{c.Affilie?.Nom} {c.Affilie?.Prenom}".Trim(),
                    AgentId = c.AgentId,
                    AgentNom = c.Agent?.NomComplet,
                    Montant = c.Montant,
                    ReferencePaiement = c.ReferencePaiement,
                    ModePaiement = c.ModePaiement,
                    Operateur = c.Operateur,
                    StatutPaiement = c.StatutPaiement,
                    DeviseId = c.DeviseId,
                    DeviseNom = c.Devise?.Nom,
                    DeviseCode = c.Devise?.Code,
                    Mois = c.Mois,
                    Annee = c.Annee,
                    DateCollecte = c.DateCollecte,
                    Observation = c.Observation,
                    DateCreation = c.DateCreation,
                    DateModification = c.DateModification,
                    Statut = c.Statut
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la récupération des collectes pour la cotisation affilié",
                    ex);
            }
        }

        private static CollecteReadDto MapToCollecteReadDto(Collecte c) => new()
        {
            IdCollecte = c.IdCollecte,
            TypeCollecte = c.TypeCollecte,
            FraisId = c.FraisId,
            FraisLibelle = c.Frais?.Libelle,
            FraisMontant = c.Frais?.Montant,
            CotisationAffilieId = c.CotisationAffilieId,
            CotisationAffilieLibelle = c.CotisationAffilie?.LibelleTarifCotisation,
            AffilieId = c.AffilieId,
            AffilieNom = $"{c.Affilie?.Nom} {c.Affilie?.Prenom}".Trim(),
            AgentId = c.AgentId,
            AgentNom = c.Agent?.NomComplet,
            Montant = c.Montant,
            ReferencePaiement = c.ReferencePaiement,
            ModePaiement = c.ModePaiement,
            Operateur = c.Operateur,
            StatutPaiement = c.StatutPaiement,
            SouscriptionPrestationId = c.SouscriptionPrestationRef?.IdSouscriptionPrestation ?? c.SouscriptionPrestationId,
            MontantRecu = c.MontantRecu,
            MontantAttendu = c.MontantAttendu,
            DeviseId = c.DeviseId,
            DeviseNom = c.Devise?.Nom,
            DeviseCode = c.Devise?.Code,
            Mois = c.Mois,
            Annee = c.Annee,
            DateCollecte = c.DateCollecte,
            Observation = c.Observation,
            DateCreation = c.DateCreation,
            DateModification = c.DateModification,
            Statut = c.Statut
        };
    }
}
