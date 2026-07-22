using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProsocAPI.Services
{
    public class CategorieAgentService : ICategorieAgentRepository
    {
        private readonly ProsocDbContext _db;

        public CategorieAgentService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<CategorieAgent>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.CategoriesAgents
                .AsNoTracking()
                .OrderBy(c => c.LibelleCategorie)
                .ToListAsync(ct);
        }

        public async Task<CategorieAgent?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.CategoriesAgents
                .AsNoTracking()
                .Include(c => c.Agents)
                .FirstOrDefaultAsync(c => c.IdCategorieAgent == id, ct);
        }

        public async Task<CategorieAgent?> GetByLibelleAsync(string libelle, CancellationToken ct = default)
        {
            return await _db.CategoriesAgents
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.LibelleCategorie.ToLower() == libelle.ToLower(), ct);
        }

        public async Task<CategorieAgent?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            var normalized = code.Trim().ToUpperInvariant();
            return await _db.CategoriesAgents
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code.ToUpper() == normalized, ct);
        }

        public async Task<IEnumerable<CategorieAgent>> GetByStatutAsync(bool statut, CancellationToken ct = default)
        {
            return await _db.CategoriesAgents
                .AsNoTracking()
                .Where(c => c.Statut == statut)
                .OrderBy(c => c.LibelleCategorie)
                .ToListAsync(ct);
        }

        public async Task<CategorieAgent> CreateAsync(CategorieAgent categorieAgent, CancellationToken ct = default)
        {
            _db.CategoriesAgents.Add(categorieAgent);
            await _db.SaveChangesAsync(ct);
            return categorieAgent;
        }

        public async Task<bool> UpdateAsync(CategorieAgent categorieAgent, CancellationToken ct = default)
        {
            _db.CategoriesAgents.Update(categorieAgent);
            var result = await _db.SaveChangesAsync(ct);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var categorieAgent = await _db.CategoriesAgents
                .Include(c => c.Agents)
                .FirstOrDefaultAsync(c => c.IdCategorieAgent == id, ct);

            if (categorieAgent == null)
                return false;

            // Vérifier si des agents sont associés à cette catégorie
            if (categorieAgent.Agents.Any())
            {
                // Optionnel: vous pouvez soit empêcher la suppression, soit dissocier les agents
                // Pour l'instant, nous allons empêcher la suppression
                return false;
            }

            _db.CategoriesAgents.Remove(categorieAgent);
            var result = await _db.SaveChangesAsync(ct);
            return result > 0;
        }

        public async Task<bool> ExistsByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.CategoriesAgents
                .AsNoTracking()
                .AnyAsync(c => c.IdCategorieAgent == id, ct);
        }

        public async Task<bool> ExistsByLibelleAsync(string libelle, CancellationToken ct = default)
        {
            return await _db.CategoriesAgents
                .AsNoTracking()
                .AnyAsync(c => c.LibelleCategorie.ToLower() == libelle.ToLower(), ct);
        }

        public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
        {
            var normalized = code.Trim().ToUpperInvariant();
            return await _db.CategoriesAgents
                .AsNoTracking()
                .AnyAsync(c => c.Code.ToUpper() == normalized, ct);
        }
    }
}
