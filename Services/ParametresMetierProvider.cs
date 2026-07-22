using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prosoc.Data;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Utilities;

namespace ProsocAPI.Services
{
    public class ParametresMetierProvider : IParametresMetierProvider
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        private readonly ProsocDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly IOptions<RetraitAgentOptions> _retraitAgentDefaults;
        private readonly IOptions<AgentMaashOptions> _agentMaashDefaults;
        private readonly IOptions<ArrieresOptions> _arrieresDefaults;
        private readonly IOptions<PenaliteOptions> _penaliteDefaults;
        private readonly ILogger<ParametresMetierProvider> _logger;

        public ParametresMetierProvider(
            ProsocDbContext db,
            IMemoryCache cache,
            IOptions<RetraitAgentOptions> retraitAgentDefaults,
            IOptions<AgentMaashOptions> agentMaashDefaults,
            IOptions<ArrieresOptions> arrieresDefaults,
            IOptions<PenaliteOptions> penaliteDefaults,
            ILogger<ParametresMetierProvider> logger)
        {
            _db = db;
            _cache = cache;
            _retraitAgentDefaults = retraitAgentDefaults;
            _agentMaashDefaults = agentMaashDefaults;
            _arrieresDefaults = arrieresDefaults;
            _penaliteDefaults = penaliteDefaults;
            _logger = logger;
        }

        public void InvalidateCache(string code) => _cache.Remove(GetCacheKey(code));

        public Task<RetraitAgentOptions> GetRetraitAgentAsync(CancellationToken ct = default) =>
            GetOrLoadAsync(ParametreMetierCodes.RetraitAgent, _retraitAgentDefaults.Value, ct);

        public async Task<RetraitAgentParametresReadDto> GetRetraitAgentReadAsync(CancellationToken ct = default)
        {
            var entity = await GetEntityWithAuditAsync(ParametreMetierCodes.RetraitAgent, ct);
            var options = entity == null
                ? await GetRetraitAgentAsync(ct)
                : Deserialize<RetraitAgentOptions>(entity.ValeurJson) ?? _retraitAgentDefaults.Value;
            return MapRetraitAgentRead(options, entity);
        }

        public async Task<RetraitAgentParametresReadDto> UpdateRetraitAgentAsync(
            RetraitAgentParametresUpdateDto dto,
            int utilisateurId,
            CancellationToken ct = default)
        {
            var validationError = RetraitAgentParametresValidator.Validate(dto);
            if (validationError != null)
                throw new ArgumentException(validationError);

            var options = new RetraitAgentOptions
            {
                Fenetre1Debut = dto.Fenetre1Debut,
                Fenetre1Fin = dto.Fenetre1Fin,
                Fenetre2DerniersJours = dto.Fenetre2DerniersJours,
                MontantMinimumPartiel = dto.MontantMinimumPartiel
            };

            var entity = await UpsertAsync(ParametreMetierCodes.RetraitAgent, options, utilisateurId, ct);
            InvalidateCache(ParametreMetierCodes.RetraitAgent);
            _logger.LogInformation(
                "Paramètres RetraitAgent mis à jour par utilisateur {UserId}", utilisateurId);

            return MapRetraitAgentRead(options, entity);
        }

        public Task<AgentMaashOptions> GetAgentMaashAsync(CancellationToken ct = default) =>
            GetOrLoadAsync(ParametreMetierCodes.AgentMaash, _agentMaashDefaults.Value, ct);

        public async Task<AgentMaashParametresReadDto> GetAgentMaashReadAsync(CancellationToken ct = default)
        {
            var entity = await GetEntityWithAuditAsync(ParametreMetierCodes.AgentMaash, ct);
            var options = entity == null
                ? await GetAgentMaashAsync(ct)
                : Deserialize<AgentMaashOptions>(entity.ValeurJson) ?? _agentMaashDefaults.Value;
            return MapAgentMaashRead(options, entity);
        }

        public async Task<AgentMaashParametresReadDto> UpdateAgentMaashAsync(
            AgentMaashParametresUpdateDto dto,
            int utilisateurId,
            CancellationToken ct = default)
        {
            var validationError = AgentMaashParametresValidator.Validate(dto);
            if (validationError != null)
                throw new ArgumentException(validationError);

            var deviseExists = await _db.Devises.AnyAsync(d => d.IdDevise == dto.DeviseId && d.Statut, ct);
            validationError = AgentMaashParametresValidator.ValidateDeviseExists(deviseExists);
            if (validationError != null)
                throw new ArgumentException(validationError);

            var codes = dto.CodesCategoriesEligibles
                .Select(c => c.Trim().ToUpperInvariant())
                .Distinct()
                .ToArray();

            var categoriesCount = await _db.CategoriesAgents
                .CountAsync(c => codes.Contains(c.Code!) && c.Statut, ct);
            validationError = AgentMaashParametresValidator.ValidateCategoriesExist(categoriesCount == codes.Length);
            if (validationError != null)
                throw new ArgumentException(validationError);

            var options = new AgentMaashOptions
            {
                MontantRetenueUsd = dto.MontantRetenueUsd,
                DeviseId = dto.DeviseId,
                CodesCategoriesEligibles = codes,
                NomProduitMaash = dto.NomProduitMaash.Trim(),
                RetenueAutomatiqueActivee = dto.RetenueAutomatiqueActivee,
                JourExecution = dto.JourExecution,
                HeureExecution = dto.HeureExecution,
                IntervalleControleMinutes = dto.IntervalleControleMinutes,
                RetenterEchecsQuotidiennement = dto.RetenterEchecsQuotidiennement
            };

            var entity = await UpsertAsync(ParametreMetierCodes.AgentMaash, options, utilisateurId, ct);
            InvalidateCache(ParametreMetierCodes.AgentMaash);
            return MapAgentMaashRead(options, entity);
        }

        public Task<ArrieresOptions> GetArrieresAsync(CancellationToken ct = default) =>
            GetOrLoadAsync(ParametreMetierCodes.Arrieres, _arrieresDefaults.Value, ct);

        public async Task<ArrieresParametresReadDto> GetArrieresReadAsync(CancellationToken ct = default)
        {
            var entity = await GetEntityWithAuditAsync(ParametreMetierCodes.Arrieres, ct);
            var options = entity == null
                ? await GetArrieresAsync(ct)
                : Deserialize<ArrieresOptions>(entity.ValeurJson) ?? _arrieresDefaults.Value;
            return MapArrieresRead(options, entity);
        }

        public async Task<ArrieresParametresReadDto> UpdateArrieresAsync(
            ArrieresParametresUpdateDto dto,
            int utilisateurId,
            CancellationToken ct = default)
        {
            var validationError = ArrieresParametresValidator.Validate(dto);
            if (validationError != null)
                throw new ArgumentException(validationError);

            var options = new ArrieresOptions
            {
                GenerationAutomatiqueActivee = dto.GenerationAutomatiqueActivee,
                HeureExecution = dto.HeureExecution,
                MinuteExecution = dto.MinuteExecution,
                IntervalleControleMinutes = dto.IntervalleControleMinutes,
                JourEcheanceMensuelle = dto.JourEcheanceMensuelle
            };

            var entity = await UpsertAsync(ParametreMetierCodes.Arrieres, options, utilisateurId, ct);
            InvalidateCache(ParametreMetierCodes.Arrieres);
            return MapArrieresRead(options, entity);
        }

        public Task<PenaliteOptions> GetPenaliteAsync(CancellationToken ct = default) =>
            GetOrLoadAsync(ParametreMetierCodes.Penalite, _penaliteDefaults.Value, ct);

        public async Task<PenaliteParametresReadDto> GetPenaliteReadAsync(CancellationToken ct = default)
        {
            var entity = await GetEntityWithAuditAsync(ParametreMetierCodes.Penalite, ct);
            var options = entity == null
                ? await GetPenaliteAsync(ct)
                : Deserialize<PenaliteOptions>(entity.ValeurJson) ?? _penaliteDefaults.Value;
            return MapPenaliteRead(options, entity);
        }

        public async Task<PenaliteParametresReadDto> UpdatePenaliteAsync(
            PenaliteParametresUpdateDto dto,
            int utilisateurId,
            CancellationToken ct = default)
        {
            var validationError = PenaliteParametresValidator.Validate(dto);
            if (validationError != null)
                throw new ArgumentException(validationError);

            var fraisCode = dto.FraisPenaliteCode.Trim().ToUpperInvariant();
            var fraisExists = await _db.Frais.AnyAsync(
                f => f.Code == fraisCode && f.Statut && !f.EstSupprime, ct);
            validationError = PenaliteParametresValidator.ValidateFraisExists(fraisExists);
            if (validationError != null)
                throw new ArgumentException(validationError);

            var options = new PenaliteOptions
            {
                ApplicationAutomatiqueActivee = dto.ApplicationAutomatiqueActivee,
                DelaiGraceJours = dto.DelaiGraceJours,
                FraisPenaliteCode = fraisCode,
                RetardCotisationActive = dto.RetardCotisationActive
            };

            var entity = await UpsertAsync(ParametreMetierCodes.Penalite, options, utilisateurId, ct);
            InvalidateCache(ParametreMetierCodes.Penalite);
            return MapPenaliteRead(options, entity);
        }

        private async Task<TOptions> GetOrLoadAsync<TOptions>(
            string code,
            TOptions defaults,
            CancellationToken ct) where TOptions : class, new()
        {
            var cacheKey = GetCacheKey(code);
            if (_cache.TryGetValue(cacheKey, out TOptions? cached) && cached != null)
                return cached;

            var entity = await _db.ParametresMetier
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == code, ct);

            TOptions options;
            if (entity == null)
            {
                options = Clone(defaults);
                await SeedAsync(code, options, ct);
            }
            else
            {
                options = Deserialize<TOptions>(entity.ValeurJson) ?? Clone(defaults);
            }

            _cache.Set(cacheKey, options, CacheDuration);
            return options;
        }

        private async Task SeedAsync<TOptions>(string code, TOptions options, CancellationToken ct)
        {
            if (await _db.ParametresMetier.AnyAsync(p => p.Code == code, ct))
                return;

            _db.ParametresMetier.Add(new ParametreMetier
            {
                Code = code,
                ValeurJson = Serialize(options),
                DateCreation = DateTime.Now
            });
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Paramètre métier {Code} initialisé depuis appsettings.", code);
        }

        private async Task<ParametreMetier> UpsertAsync<TOptions>(
            string code,
            TOptions options,
            int utilisateurId,
            CancellationToken ct)
        {
            var entity = await _db.ParametresMetier.FirstOrDefaultAsync(p => p.Code == code, ct);
            var json = Serialize(options);
            var now = DateTime.Now;

            if (entity == null)
            {
                entity = new ParametreMetier
                {
                    Code = code,
                    ValeurJson = json,
                    DateCreation = now,
                    DateModification = now,
                    ModifieParUtilisateurId = utilisateurId > 0 ? utilisateurId : null
                };
                _db.ParametresMetier.Add(entity);
            }
            else
            {
                entity.ValeurJson = json;
                entity.DateModification = now;
                entity.ModifieParUtilisateurId = utilisateurId > 0 ? utilisateurId : null;
            }

            await _db.SaveChangesAsync(ct);

            await _db.Entry(entity)
                .Reference(e => e.ModifiePar)
                .LoadAsync(ct);

            return entity;
        }

        private Task<ParametreMetier?> GetEntityWithAuditAsync(string code, CancellationToken ct) =>
            _db.ParametresMetier
                .AsNoTracking()
                .Include(p => p.ModifiePar)
                .FirstOrDefaultAsync(p => p.Code == code, ct);

        private static string GetCacheKey(string code) => $"ParametreMetier:{code}";

        private static string Serialize<T>(T value) =>
            JsonSerializer.Serialize(value, JsonOptions);

        private static T? Deserialize<T>(string json) where T : class =>
            JsonSerializer.Deserialize<T>(json, JsonOptions);

        private static T Clone<T>(T source) where T : class, new()
        {
            var json = Serialize(source);
            return Deserialize<T>(json) ?? new T();
        }

        private static RetraitAgentParametresReadDto MapRetraitAgentRead(
            RetraitAgentOptions options,
            ParametreMetier? entity) =>
            new()
            {
                Fenetre1Debut = options.Fenetre1Debut,
                Fenetre1Fin = options.Fenetre1Fin,
                Fenetre2DerniersJours = options.Fenetre2DerniersJours,
                MontantMinimumPartiel = options.MontantMinimumPartiel,
                DateModification = entity?.DateModification,
                ModifieParUtilisateurId = entity?.ModifieParUtilisateurId,
                ModifieParNom = entity?.ModifiePar?.NomUtilisateur
            };

        private static AgentMaashParametresReadDto MapAgentMaashRead(
            AgentMaashOptions options,
            ParametreMetier? entity) =>
            new()
            {
                MontantRetenueUsd = options.MontantRetenueUsd,
                DeviseId = options.DeviseId,
                CodesCategoriesEligibles = options.CodesCategoriesEligibles,
                NomProduitMaash = options.NomProduitMaash,
                RetenueAutomatiqueActivee = options.RetenueAutomatiqueActivee,
                JourExecution = options.JourExecution,
                HeureExecution = options.HeureExecution,
                IntervalleControleMinutes = options.IntervalleControleMinutes,
                RetenterEchecsQuotidiennement = options.RetenterEchecsQuotidiennement,
                DateModification = entity?.DateModification,
                ModifieParUtilisateurId = entity?.ModifieParUtilisateurId,
                ModifieParNom = entity?.ModifiePar?.NomUtilisateur
            };

        private static ArrieresParametresReadDto MapArrieresRead(
            ArrieresOptions options,
            ParametreMetier? entity) =>
            new()
            {
                GenerationAutomatiqueActivee = options.GenerationAutomatiqueActivee,
                HeureExecution = options.HeureExecution,
                MinuteExecution = options.MinuteExecution,
                IntervalleControleMinutes = options.IntervalleControleMinutes,
                JourEcheanceMensuelle = options.JourEcheanceMensuelle,
                DateModification = entity?.DateModification,
                ModifieParUtilisateurId = entity?.ModifieParUtilisateurId,
                ModifieParNom = entity?.ModifiePar?.NomUtilisateur
            };

        private static PenaliteParametresReadDto MapPenaliteRead(
            PenaliteOptions options,
            ParametreMetier? entity) =>
            new()
            {
                ApplicationAutomatiqueActivee = options.ApplicationAutomatiqueActivee,
                DelaiGraceJours = options.DelaiGraceJours,
                FraisPenaliteCode = options.FraisPenaliteCode,
                RetardCotisationActive = options.RetardCotisationActive,
                DateModification = entity?.DateModification,
                ModifieParUtilisateurId = entity?.ModifieParUtilisateurId,
                ModifieParNom = entity?.ModifiePar?.NomUtilisateur
            };
    }
}
