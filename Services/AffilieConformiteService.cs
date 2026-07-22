using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;

namespace ProsocAPI.Services
{
    public class AffilieConformiteService : IAffilieConformiteService
    {
        private const int MaxArrieresOuvertsAffichage = 5;
        private readonly ProsocDbContext _db;
        private readonly IArrieresAffilieService _arrieresService;

        public AffilieConformiteService(ProsocDbContext db, IArrieresAffilieService arrieresService)
        {
            _db = db;
            _arrieresService = arrieresService;
        }

        public async Task<AffilieConformiteDto?> GetConformiteAffilieAsync(int affilieId, CancellationToken ct = default)
        {
            var map = await GetConformiteParAffiliesAsync(new[] { affilieId }, ct);
            return map.TryGetValue(affilieId, out var dto) ? dto : null;
        }

        public async Task<Dictionary<int, AffilieConformiteDto>> GetConformiteParAffiliesAsync(
            IEnumerable<int> affilieIds,
            CancellationToken ct = default)
        {
            var ids = affilieIds.Distinct().Where(id => id > 0).ToList();
            if (ids.Count == 0)
                return new Dictionary<int, AffilieConformiteDto>();

            await _arrieresService.UpdateStatutsRetardAsync(ct);

            var affilies = await _db.Affilies
                .AsNoTracking()
                .Where(a => ids.Contains(a.IdAffilie))
                .Select(a => new { a.IdAffilie, a.CodeAdhesion, a.NomComplet })
                .ToDictionaryAsync(a => a.IdAffilie, ct);

            var agentParAffilie = await _db.Adhesions
                .AsNoTracking()
                .Where(a => ids.Contains(a.AffilieId) && a.Statut && a.AgentId != null)
                .GroupBy(a => a.AffilieId)
                .Select(g => new { AffilieId = g.Key, AgentId = g.OrderByDescending(a => a.DateCreation).First().AgentId })
                .ToDictionaryAsync(x => x.AffilieId, x => x.AgentId, ct);

            var arrieres = await _db.ArrieresAffilie
                .AsNoTracking()
                .Include(a => a.CotisationAffilie)
                .Include(a => a.SouscriptionPrestation)!.ThenInclude(sp => sp!.Prestation)
                .Include(a => a.Frais)
                .Where(a => ids.Contains(a.AffilieId) && a.Statut)
                .ToListAsync(ct);

            var grouped = arrieres.GroupBy(a => a.AffilieId).ToDictionary(g => g.Key, g => g.ToList());
            var today = DateTime.Today;
            var result = new Dictionary<int, AffilieConformiteDto>();

            foreach (var affilieId in ids)
            {
                grouped.TryGetValue(affilieId, out var lignes);
                lignes ??= new List<ArrieresAffilie>();

                affilies.TryGetValue(affilieId, out var affilie);
                agentParAffilie.TryGetValue(affilieId, out var agentId);

                result[affilieId] = BuildConformite(
                    affilieId,
                    lignes,
                    today,
                    affilie?.CodeAdhesion,
                    affilie?.NomComplet,
                    agentId);
            }

            return result;
        }

        public async Task<PaginatedResponse<AffilieConformiteDto>> GetConformiteListeAsync(
            AffilieConformiteFiltreDto filtres,
            PaginationRequest pagination,
            CancellationToken ct = default)
        {
            filtres ??= new AffilieConformiteFiltreDto();

            var query = _db.Adhesions
                .AsNoTracking()
                .Include(a => a.Affilie)
                .Where(a => a.Statut && a.Affilie.Statut);

            if (filtres.AgentId.HasValue)
                query = query.Where(a => a.AgentId == filtres.AgentId.Value);

            if (!string.IsNullOrWhiteSpace(filtres.Search))
            {
                var search = filtres.Search.Trim().ToLower();
                query = query.Where(a =>
                    a.Affilie.NomComplet.ToLower().Contains(search)
                    || a.Affilie.CodeAdhesion.ToLower().Contains(search));
            }

            var affilieIds = await query
                .Select(a => a.AffilieId)
                .Distinct()
                .ToListAsync(ct);

            var conformites = await GetConformiteParAffiliesAsync(affilieIds, ct);
            var liste = conformites.Values.ToList();

            if (!string.IsNullOrWhiteSpace(filtres.StatutGlobal))
                liste = liste.Where(c => c.StatutGlobal == filtres.StatutGlobal).ToList();

            if (!string.IsNullOrWhiteSpace(filtres.StatutCotisation))
                liste = liste.Where(c => c.StatutCotisation == filtres.StatutCotisation).ToList();

            if (!string.IsNullOrWhiteSpace(filtres.StatutPrestation))
                liste = liste.Where(c => c.StatutPrestation == filtres.StatutPrestation).ToList();

            liste = liste
                .OrderBy(c => c.StatutGlobal == AffilieConformiteStatuts.HorsOrdre ? 0 : 1)
                .ThenBy(c => c.NomComplet)
                .ToList();

            var page = pagination.Page > 0 ? pagination.Page : 1;
            var pageSize = pagination.PageSize > 0 ? pagination.PageSize : 20;
            var totalItems = liste.Count;
            var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;
            var data = liste.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PaginatedResponse<AffilieConformiteDto>
            {
                Data = data,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };
        }

        private static AffilieConformiteDto BuildConformite(
            int affilieId,
            List<ArrieresAffilie> lignes,
            DateTime today,
            string? codeAdhesion,
            string? nomComplet,
            int? agentId)
        {
            var cotisationOk = AffilieConformiteRules.EstEnOrdrePourType(lignes, TypeCollecte.Cotisation, today);
            var prestationOk = AffilieConformiteRules.EstEnOrdrePourType(lignes, TypeCollecte.Souscription, today);
            var fraisOk = AffilieConformiteRules.EstEnOrdrePourType(lignes, TypeCollecte.Frais, today);
            var globalOk = cotisationOk && prestationOk && fraisOk;

            var lignesOuvertes = lignes
                .Where(l => AffilieConformiteRules.EstLigneEnSouffrance(l, today))
                .OrderBy(l => l.DateEcheance)
                .ToList();

            return new AffilieConformiteDto
            {
                AffilieId = affilieId,
                CodeAdhesion = codeAdhesion,
                NomComplet = nomComplet,
                AgentId = agentId,
                StatutCotisation = AffilieConformiteRules.ToStatut(cotisationOk),
                StatutPrestation = AffilieConformiteRules.ToStatut(prestationOk),
                StatutGlobal = AffilieConformiteRules.ToStatut(globalOk),
                NombreArrieresOuverts = lignesOuvertes.Count,
                MontantRestantDu = lignesOuvertes.Sum(l => l.RestAPayer),
                ArrieresOuverts = lignesOuvertes
                    .Take(MaxArrieresOuvertsAffichage)
                    .Select(ToArriereOuvertDto)
                    .ToList(),
                DateCalcul = DateTime.Now
            };
        }

        private static ArriereOuvertDto ToArriereOuvertDto(ArrieresAffilie arriere) =>
            new()
            {
                IdArrieresAffilie = arriere.IdArrieresAffilie,
                TypeObligation = arriere.TypeObligation.ToString().ToUpperInvariant(),
                Periode = arriere.Periode,
                MontantRestant = arriere.RestAPayer,
                StatutPaiement = arriere.StatutPaiement,
                Libelle = ResolveLibelle(arriere),
                DateEcheance = arriere.DateEcheance
            };

        private static string? ResolveLibelle(ArrieresAffilie arriere) =>
            arriere.TypeObligation switch
            {
                TypeCollecte.Cotisation => !string.IsNullOrWhiteSpace(arriere.Description)
                    ? arriere.Description
                    : $"Cotisation {arriere.Periodicite} {arriere.Periode}",
                TypeCollecte.Souscription => arriere.SouscriptionPrestation?.Prestation?.NomPrestation ?? arriere.Description,
                TypeCollecte.Frais => arriere.Frais?.Libelle ?? arriere.Description,
                _ => arriere.Description
            };
    }
}
