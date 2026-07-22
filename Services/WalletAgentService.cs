using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class WalletAgentService : IWalletAgentRepository
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<WalletAgentService> _logger;

        public WalletAgentService(ProsocDbContext db, ILogger<WalletAgentService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<WalletAgent>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.WalletsAgents
                .Include(w => w.Agent)
                .Include(w => w.Devise)
                .OrderByDescending(w => w.DateCreation)
                .ToListAsync(ct);
        }

        public async Task<WalletAgent?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.WalletsAgents
                .Include(w => w.Agent)
                .Include(w => w.Devise)
                .FirstOrDefaultAsync(w => w.IdWalletAgent == id, ct);
        }

        public async Task<WalletAgent?> GetByAgentIdAsync(int agentId, CancellationToken ct = default)
        {
            return await _db.WalletsAgents
                .Include(w => w.Agent)
                .Include(w => w.Devise)
                .Where(w => w.AgentId == agentId)
                .OrderByDescending(w => w.Devise!.EstDevisePrincipale)
                .ThenBy(w => w.DeviseId)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<WalletAgent?> GetPrincipalWalletByAgentIdAsync(int agentId, CancellationToken ct = default)
        {
            return await _db.WalletsAgents
                .Include(w => w.Agent)
                .Include(w => w.Devise)
                .Where(w => w.AgentId == agentId && w.Statut && w.Devise!.EstDevisePrincipale)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<WalletAgent?> GetByAgentAndDeviseAsync(int agentId, int deviseId, CancellationToken ct = default)
        {
            return await _db.WalletsAgents
                .Include(w => w.Agent)
                .Include(w => w.Devise)
                .FirstOrDefaultAsync(w => w.AgentId == agentId && w.DeviseId == deviseId, ct);
        }

        public async Task<List<WalletAgent>> GetByAgentIdAllAsync(int agentId, CancellationToken ct = default)
        {
            return await _db.WalletsAgents
                .Include(w => w.Agent)
                .Include(w => w.Devise)
                .Where(w => w.AgentId == agentId)
                .OrderBy(w => w.Devise!.Code)
                .ToListAsync(ct);
        }

        public async Task<WalletAgent> GetOrCreateForAgentAndDeviseAsync(int agentId, int deviseId, CancellationToken ct = default)
        {
            var existing = await GetByAgentAndDeviseAsync(agentId, deviseId, ct);
            if (existing != null)
                return existing;

            var deviseExists = await _db.Devises.AnyAsync(d => d.IdDevise == deviseId && d.Statut, ct);
            if (!deviseExists)
                throw new ArgumentException($"Devise avec ID {deviseId} introuvable ou inactive.");

            var agentExists = await _db.Agents.AnyAsync(a => a.IdAgent == agentId, ct);
            if (!agentExists)
                throw new ArgumentException($"Agent avec ID {agentId} introuvable.");

            var wallet = new WalletAgent
            {
                AgentId = agentId,
                DeviseId = deviseId,
                SoldeCourant = 0,
                SoldeDisponible = 0,
                Statut = true,
                DateCreation = DateTime.Now
            };

            _db.WalletsAgents.Add(wallet);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Wallet agent créé : AgentId={AgentId}, DeviseId={DeviseId}, IdWallet={Id}",
                agentId, deviseId, wallet.IdWalletAgent);

            return await GetByIdAsync(wallet.IdWalletAgent, ct) ?? wallet;
        }

        public async Task<WalletAgent> CreateAsync(WalletAgent entity, CancellationToken ct = default)
        {
            var duplicate = await _db.WalletsAgents.AnyAsync(
                w => w.AgentId == entity.AgentId && w.DeviseId == entity.DeviseId, ct);
            if (duplicate)
                throw new InvalidOperationException(
                    $"Un wallet existe déjà pour l'agent {entity.AgentId} et la devise {entity.DeviseId}.");

            entity.DateCreation = DateTime.Now;
            _db.WalletsAgents.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<WalletAgent?> UpdateAsync(int id, WalletAgent entity, CancellationToken ct = default)
        {
            var existing = await _db.WalletsAgents.FirstOrDefaultAsync(w => w.IdWalletAgent == id, ct);
            if (existing == null)
                return null;

            if (entity.DeviseId > 0 && entity.DeviseId != existing.DeviseId)
            {
                var duplicate = await _db.WalletsAgents.AnyAsync(
                    w => w.AgentId == existing.AgentId && w.DeviseId == entity.DeviseId && w.IdWalletAgent != id, ct);
                if (duplicate)
                    throw new InvalidOperationException(
                        $"Un wallet existe déjà pour l'agent {existing.AgentId} et la devise {entity.DeviseId}.");
                existing.DeviseId = entity.DeviseId;
            }

            existing.SoldeCourant = entity.SoldeCourant;
            existing.SoldeDisponible = Math.Min(entity.SoldeDisponible, entity.SoldeCourant);
            existing.Statut = entity.Statut;
            existing.DateModification = DateTime.Now;

            await _db.SaveChangesAsync(ct);
            return await GetByIdAsync(id, ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.WalletsAgents.FirstOrDefaultAsync(w => w.IdWalletAgent == id, ct);
            if (existing == null)
                return false;

            _db.WalletsAgents.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
