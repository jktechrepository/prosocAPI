using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.DTOs.FlexPay;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;
using Prosoc.Data;
using Prosoc.Utilities;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AffilieController : BaseApiController
    {
        private readonly IAffilieRepository _repo;
        private readonly IAdhesionRepository _adhesionRepo;
        private readonly ProsocDbContext _db;
        private readonly IPaiementAffilieService _paiementAffilieService;
        private readonly IFlexPayPaiementAffilieService _flexPayPaiementAffilieService;

        public AffilieController(
            IAffilieRepository repo, 
            IAdhesionRepository adhesionRepo,
            ProsocDbContext db,
            IPaiementAffilieService paiementAffilieService,
            IFlexPayPaiementAffilieService flexPayPaiementAffilieService,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<AffilieController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _repo = repo;
            _adhesionRepo = adhesionRepo;
            _db = db;
            _paiementAffilieService = paiementAffilieService;
            _flexPayPaiementAffilieService = flexPayPaiementAffilieService;
        }

        // ENDPOINTS CRUD DE BASE (RESTAURÉS)

        /// <summary>
        /// Récupère la liste de tous les affiliés
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<AffilieReadDto>>> GetAffilies(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var deny = AffilieMemberScopeHelper.DenyListAccessForMembre(User, "des affiliés");
                if (deny != null)
                    return deny;

                if (!HasPermission("READ_AFFILIE"))
                    return ForbiddenPermission("READ_AFFILIE");

                base._logger.LogInformation("Récupération de la liste des affiliés");

                var query = AffilieQueryHelper.WithAssociations(_db.Affilies.AsNoTracking());

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);
                var dtos = result.Data.Select(AffilieDtoMapper.ToReadDto).ToList();

                return Ok(new PaginatedResponse<AffilieReadDto>
                {
                    Data = dtos,
                    CurrentPage = result.CurrentPage,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    HasNextPage = result.HasNextPage,
                    HasPreviousPage = result.HasPreviousPage
                });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la récupération des affiliés",
                    ex);
            }
        }

        /// <summary>
        /// Récupère un affilié par son ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AffilieReadDto>> GetAffilie(int id, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await AffilieMemberScopeHelper.EnsureOwnAffilieScopeAsync(User, _db, id, ct);
                if (scopeError != null)
                    return scopeError;

                if (!AffilieMemberScopeHelper.IsMembreAffilie(User) && !HasPermission("READ_AFFILIE"))
                    return ForbiddenPermission("READ_AFFILIE");

                base._logger.LogInformation("Récupération de l'affilié {AffilieId}", id);

                var affilié = await _repo.GetByIdAsync(id);
                if (affilié == null)
                    return NotFound("Affilié non trouvé");

                return Ok(AffilieDtoMapper.ToReadDto(affilié));
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la récupération de l'affilié",
                    ex);
            }
        }

        /// <summary>
        /// Récupère les antécédents d'un affilié (paginé)
        /// </summary>
        [HttpGet("{id}/antecedants")]
        public async Task<ActionResult<PaginatedResponse<AntecedentReadDto>>> GetAntecedantsByAffilie(
            int id,
            [FromQuery] PaginationRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var scopeError = await AffilieMemberScopeHelper.EnsureOwnAffilieScopeAsync(User, _db, id, ct);
                if (scopeError != null)
                    return scopeError;

                if (!AffilieMemberScopeHelper.IsMembreAffilie(User) && !HasPermission("READ_ANTECEDENT"))
                    return ForbiddenPermission("READ_ANTECEDENT");

                var affilieExists = await _db.Affilies
                    .AsNoTracking()
                    .AnyAsync(a => a.IdAffilie == id, ct);
                if (!affilieExists)
                    return NotFound("Affilié non trouvé");

                var query = _db.Antecedants
                    .AsNoTracking()
                    .Include(a => a.Affilie)
                    .Include(a => a.Dependant)
                    .Where(a => a.AffilieId == id)
                    .OrderByDescending(a => a.DateCreation);

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request, ct);
                var dtos = result.Data.Select(a => a.ToReadDto()).ToList();

                return Ok(new PaginatedResponse<AntecedentReadDto>
                {
                    Data = dtos,
                    CurrentPage = result.CurrentPage,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    HasNextPage = result.HasNextPage,
                    HasPreviousPage = result.HasPreviousPage
                });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la récupération des antécédents de l'affilié",
                    ex);
            }
        }

        /// <summary>
        /// Récupère les dépendants d'un affilié (paginé)
        /// </summary>
        [HttpGet("{id}/dependants")]
        public async Task<ActionResult<PaginatedResponse<DependantReadDto>>> GetDependantsByAffilie(
            int id,
            [FromQuery] PaginationRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var scopeError = await AffilieMemberScopeHelper.EnsureOwnAffilieScopeAsync(User, _db, id, ct);
                if (scopeError != null)
                    return scopeError;

                if (!AffilieMemberScopeHelper.IsMembreAffilie(User) && !HasPermission("READ_DEPENDANT"))
                    return ForbiddenPermission("READ_DEPENDANT");

                var affilieExists = await _db.Affilies
                    .AsNoTracking()
                    .AnyAsync(a => a.IdAffilie == id, ct);
                if (!affilieExists)
                    return NotFound("Affilié non trouvé");

                var query = DependantQueryHelper.GetByAffilieQuery(_db, id);

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request, ct);
                var dtos = result.Data.Select(DependantDtoMapper.ToReadDto).ToList();

                return Ok(new PaginatedResponse<DependantReadDto>
                {
                    Data = dtos,
                    CurrentPage = result.CurrentPage,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    HasNextPage = result.HasNextPage,
                    HasPreviousPage = result.HasPreviousPage
                });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la récupération des dépendants de l'affilié",
                    ex);
            }
        }

        /// <summary>
        /// Crée un nouvel affilié
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AffilieReadDto>> CreateAffilie([FromBody] AffilieCreateDto dto)
        {
            try
            {
                base._logger.LogInformation("Création d'un nouvel affilié");

                // Valider le DTO
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var affilié = new Affilie
                {
                    CodeAdhesion = dto.CodeAdhesion,
                    Nom = dto.Nom,
                    Postnom = dto.Postnom,
                    Prenom = dto.Prenom,
                    NomComplet = $"{dto.Nom} {dto.Postnom} {dto.Prenom}",
                    DateNaissance = dto.DateNaissance,
                    Telephone = dto.Telephone,
                    EmailAffilie = dto.EmailAffilie,
                    ProvinceResidence = dto.ProvinceResidence,
                    CommuneResidence = dto.CommuneResidence,
                    QuartierResidence = dto.QuartierResidence,
                    AvenueResidence = dto.AvenueResidence,
                    NumeroResidence = dto.NumeroResidence,
                    CommuneActivite = dto.CommuneActivite,
                    QuartierActivite = dto.QuartierActivite,
                    AvenueActivite = dto.AvenueActivite,
                    NumeroActivite = dto.NumeroActivite,
                    Statut = true,
                    DateCreation = DateTime.UtcNow
                };

                try
                {
                    AffilieFichierApplicator.AppliquerCreationAffilie(affilié, dto);
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(ex.Message);
                }

                var created = await _repo.CreateAsync(affilié);
                var createdWithAssoc = await _repo.GetByIdAsync(created.IdAffilie) ?? created;
                return CreatedAtAction(nameof(GetAffilie), new { id = created.IdAffilie }, AffilieDtoMapper.ToReadDto(createdWithAssoc));
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la création de l'affilié",
                    ex);
            }
        }

        /// <summary>
        /// Met à jour un affilié existant
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<AffilieReadDto>> UpdateAffilie(int id, [FromBody] AffilieUpdateDto dto, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await AffilieMemberScopeHelper.EnsureOwnAffilieScopeAsync(User, _db, id, ct);
                if (scopeError != null)
                    return scopeError;

                if (!HasPermission("UPDATE_AFFILIE"))
                    return ForbiddenPermission("UPDATE_AFFILIE");

                base._logger.LogInformation("Mise à jour de l'affilié {AffilieId}", id);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var affilié = await _repo.GetByIdAsync(id);
                if (affilié == null)
                    return NotFound("Affilié non trouvé");

                // Mettre à jour les propriétés
                affilié.CodeAdhesion = dto.CodeAdhesion;
                affilié.Nom = dto.Nom;
                affilié.Postnom = dto.Postnom;
                affilié.Prenom = dto.Prenom;
                affilié.NomComplet = $"{dto.Nom} {dto.Postnom} {dto.Prenom}";
                affilié.DateNaissance = dto.DateNaissance;
                affilié.Telephone = dto.Telephone;
                affilié.EmailAffilie = dto.EmailAffilie;
                affilié.ProvinceResidence = dto.ProvinceResidence;
                affilié.CommuneResidence = dto.CommuneResidence;
                affilié.QuartierResidence = dto.QuartierResidence;
                affilié.AvenueResidence = dto.AvenueResidence;
                affilié.NumeroResidence = dto.NumeroResidence;
                affilié.CommuneActivite = dto.CommuneActivite;
                affilié.QuartierActivite = dto.QuartierActivite;
                affilié.AvenueActivite = dto.AvenueActivite;
                affilié.NumeroActivite = dto.NumeroActivite;
                try
                {
                    AffilieFichierApplicator.AppliquerMiseAJourOptionnelle(affilié, dto);
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(ex.Message);
                }

                affilié.DateModification = DateTime.UtcNow;

                var updated = await _repo.UpdateAsync(affilié.IdAffilie, affilié);
                var updatedWithAssoc = updated == null
                    ? null
                    : await _repo.GetByIdAsync(updated.IdAffilie) ?? updated;
                return updatedWithAssoc == null
                    ? NotFound("Affilié non trouvé")
                    : Ok(AffilieDtoMapper.ToReadDto(updatedWithAssoc));
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la mise à jour de l'affilié",
                    ex);
            }
        }

        [HttpGet("{id}/photo")]
        public async Task<IActionResult> GetPhoto(int id, CancellationToken ct)
        {
            var scopeError = await AffilieMemberScopeHelper.EnsureOwnAffilieScopeAsync(User, _db, id, ct);
            if (scopeError != null)
                return scopeError;

            if (!AffilieMemberScopeHelper.IsMembreAffilie(User) && !HasPermission("READ_AFFILIE"))
                return ForbiddenPermission("READ_AFFILIE");

            var fichier = await _db.Affilies
                .AsNoTracking()
                .Where(a => a.IdAffilie == id)
                .Select(a => new { a.PhotoData, a.PhotoContentType })
                .FirstOrDefaultAsync(ct);

            if (fichier?.PhotoData == null || fichier.PhotoData.Length == 0)
                return NotFound();

            return File(fichier.PhotoData, fichier.PhotoContentType ?? "image/jpeg");
        }

        [HttpGet("{id}/carte-identite")]
        public async Task<IActionResult> GetCarteIdentite(int id, CancellationToken ct)
        {
            var scopeError = await AffilieMemberScopeHelper.EnsureOwnAffilieScopeAsync(User, _db, id, ct);
            if (scopeError != null)
                return scopeError;

            if (!AffilieMemberScopeHelper.IsMembreAffilie(User) && !HasPermission("READ_AFFILIE"))
                return ForbiddenPermission("READ_AFFILIE");

            var fichier = await _db.Affilies
                .AsNoTracking()
                .Where(a => a.IdAffilie == id)
                .Select(a => new { a.CarteIdentiteData, a.CarteIdentiteContentType })
                .FirstOrDefaultAsync(ct);

            if (fichier?.CarteIdentiteData == null || fichier.CarteIdentiteData.Length == 0)
                return NotFound();

            return File(
                fichier.CarteIdentiteData,
                fichier.CarteIdentiteContentType ?? "application/octet-stream");
        }

        /// <summary>
        /// Supprime un affilié
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAffilie(int id)
        {
            try
            {
                base._logger.LogInformation("Suppression de l'affilié {AffilieId}", id);

                var affilié = await _repo.GetByIdAsync(id);
                if (affilié == null)
                    return NotFound("Affilié non trouvé");

                await _repo.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la suppression de l'affilié",
                    ex);
            }
        }

        // NOUVEAUX ENDPOINTS PAIEMENT AFFILIÉ

        /// <summary>Profil de l'affilié connecté (fiche + personne de contact + adhésion).</summary>
        [HttpGet("mon-profil")]
        public async Task<ActionResult<AffilieProfilMembreDto>> GetMonProfil(CancellationToken ct = default)
        {
            try
            {
                var (affilieId, error) = await AffilieMemberScopeHelper.RequireOwnAffilieIdAsync(User, _db, ct);
                if (error != null)
                    return error;

                var affilie = await AffilieQueryHelper.WithAssociations(_db.Affilies.AsNoTracking())
                    .Include(a => a.Adhesion)
                        .ThenInclude(ad => ad!.TypeAdhesion)
                    .FirstOrDefaultAsync(a => a.IdAffilie == affilieId, ct);

                if (affilie == null)
                    return NotFound("Affilié non trouvé");

                var affilieDto = AffilieDtoMapper.ToReadDto(affilie);
                return Ok(new AffilieProfilMembreDto
                {
                    Affilie = affilieDto,
                    PersonneContact = affilieDto.PersonneContact,
                    AdhesionId = affilie.Adhesion?.IdAdhesion,
                    StatutDossier = affilie.Adhesion?.StatutDossier,
                    TypeAdhesion = affilie.Adhesion?.TypeAdhesion?.Libelle
                });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la récupération du profil",
                    ex);
            }
        }

        /// <summary>
        /// Récupérer les souscriptions payables par l'affilié connecté
        /// </summary>
        [HttpGet("souscriptions")]
        public async Task<ActionResult<List<SouscriptionPrestationReadDto>>> GetSouscriptionsPayables(CancellationToken ct = default)
        {
            try
            {
                var affilieId = await GetCurrentAffilieIdAsync(ct);
                if (affilieId == 0)
                    return Unauthorized("Utilisateur non authentifié ou non affilié");

                base._logger.LogInformation("Récupération des souscriptions payables pour l'affilié {AffilieId}", affilieId);

                var souscriptions = await _paiementAffilieService.GetSouscriptionsPayablesAsync(affilieId);

                var dtos = souscriptions.Select(sp => new SouscriptionPrestationReadDto
                {
                    Id = sp.IdSouscriptionPrestation,
                    AffilieId = sp.AffilieId,
                    PrestationId = sp.PrestationId,
                    PrestationNom = sp.Prestation?.NomPrestation ?? "Inconnue",
                    PrestationDescription = sp.Prestation?.Description ?? "",
                    DateSouscription = sp.DateSouscription,
                    DateCreation = sp.DateCreation,
                    Statut = sp.Statut,
                    NombreCollectes = 0, // Sera calculé si nécessaire
                    TotalCollectes = 0m,
                    AffilieNom = sp.Affilie?.NomComplet,
                    AffiliePrenom = sp.Affilie?.Prenom
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la récupération des souscriptions payables",
                    ex);
            }
        }

        /// <summary>
        /// Permettre à un affilié de payer sa souscription
        /// </summary>
        [HttpPost("paiement")]
        public async Task<ActionResult<object>> PayerMaSouscription([FromBody] PayerSouscriptionDto dto, CancellationToken ct = default)
        {
            try
            {
                var affilieId = await GetCurrentAffilieIdAsync(ct);
                if (affilieId == 0)
                    return Unauthorized("Utilisateur non authentifié ou non affilié");

                if (!dto.IsValid())
                    return BadRequest("Données de paiement invalides");

                base._logger.LogInformation("Paiement de souscription par l'affilié {AffilieId} - Souscription: {SouscriptionId}", 
                    affilieId, dto.SouscriptionPrestationId);

                if (MethodePaiementHelper.IsFlexPay(dto.ModePaiement))
                {
                    var flexResult = await _flexPayPaiementAffilieService.InitiateAsync(
                        affilieId, dto, dto.Phone, ct);
                    return Ok(flexResult);
                }

                MethodePaiementHelper.EnsureGuichetSyncOnly(dto.ModePaiement);

                var collecte = await _paiementAffilieService.PayerSouscriptionAsync(affilieId, dto, ct);

                // Mapper vers le DTO de réponse
                var collecteDto = new CollecteReadDto
                {
                    IdCollecte = collecte.IdCollecte,
                    TypeCollecte = collecte.TypeCollecte,
                    FraisId = collecte.FraisId,
                    FraisLibelle = collecte.Frais?.Libelle,
                    FraisMontant = collecte.Frais?.Montant,
                    AffilieId = collecte.AffilieId,
                    AffilieNom = collecte.Affilie?.NomComplet,
                    AgentId = collecte.AgentId,
                    AgentNom = collecte.Agent?.NomComplet,
                    Montant = collecte.Montant,
                    ReferencePaiement = collecte.ReferencePaiement,
                    ModePaiement = collecte.ModePaiement,
                    Operateur = collecte.Operateur,
                    StatutPaiement = collecte.StatutPaiement,
                    SouscriptionPrestationId = collecte.SouscriptionPrestationId,
                    MontantRecu = collecte.MontantRecu,
                    MontantAttendu = collecte.MontantAttendu,
                    DeviseId = collecte.DeviseId,
                    DeviseNom = collecte.Devise?.Nom,
                    DeviseCode = collecte.Devise?.Code,
                    DateCollecte = collecte.DateCollecte,
                    Observation = collecte.Observation,
                    DateCreation = collecte.DateCreation,
                    DateModification = collecte.DateModification,
                    Statut = collecte.Statut
                };

                return Ok(collecteDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                base._logger.LogWarning(ex, "Tentative de paiement non autorisée");
                return StatusCode(403, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                base._logger.LogWarning(ex, "Opération invalide lors du paiement");
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                base._logger.LogWarning(ex, "Argument invalide lors du paiement");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors du paiement de souscription",
                    ex);
            }
        }

        /// <summary>
        /// Récupérer l'historique des paiements de l'affilié connecté
        /// </summary>
        [HttpGet("paiements/historique")]
        public async Task<ActionResult<PaginatedResponse<CollecteReadDto>>> GetMesPaiements(
            [FromQuery] PaginationRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var affilieId = await GetCurrentAffilieIdAsync(ct);
                if (affilieId == 0)
                    return Unauthorized("Utilisateur non authentifié ou non affilié");

                base._logger.LogInformation("Récupération de l'historique des paiements pour l'affilié {AffilieId}", affilieId);

                var query = _paiementAffilieService
                    .GetHistoriquePaiementsQuery(affilieId)
                    .Select(c => new CollecteReadDto
                    {
                        IdCollecte = c.IdCollecte,
                        TypeCollecte = c.TypeCollecte,
                        FraisId = c.FraisId,
                        FraisLibelle = c.Frais != null ? c.Frais.Libelle : null,
                        FraisMontant = c.Frais != null ? c.Frais.Montant : null,
                        CotisationAffilieId = c.CotisationAffilieId,
                        AffilieId = c.AffilieId,
                        AffilieNom = c.Affilie != null ? c.Affilie.NomComplet : null,
                        AgentId = c.AgentId,
                        AgentNom = c.Agent != null ? c.Agent.NomComplet : null,
                        Montant = c.Montant,
                        ReferencePaiement = c.ReferencePaiement,
                        ModePaiement = c.ModePaiement,
                        Operateur = c.Operateur,
                        StatutPaiement = c.StatutPaiement,
                        SouscriptionPrestationId = c.SouscriptionPrestationId,
                        MontantRecu = c.MontantRecu,
                        MontantAttendu = c.MontantAttendu,
                        DeviseId = c.DeviseId,
                        DeviseNom = c.Devise != null ? c.Devise.Nom : null,
                        DeviseCode = c.Devise != null ? c.Devise.Code : null,
                        Mois = c.Mois,
                        Annee = c.Annee,
                        DateCollecte = c.DateCollecte,
                        Observation = c.Observation,
                        DateCreation = c.DateCreation,
                        DateModification = c.DateModification,
                        Statut = c.Statut
                    });

                var response = await _paginationService.CreatePaginatedResponseAsync(query, request, ct);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue lors de la récupération de l'historique des paiements",
                    ex);
            }
        }

        private Task<int> GetCurrentAffilieIdAsync(CancellationToken ct) =>
            CurrentUserAffilieResolver.ResolveAffilieIdAsync(User, _db, ct);
    }
}
