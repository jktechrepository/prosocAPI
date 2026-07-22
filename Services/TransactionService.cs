using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services
{
    public interface ITransactionService
    {
        Task<TransactionResponseDto> ProcessCollecte(int agentId, CollecteDto dto);
        Task<TransactionResponseDto> ProcessRetrait(int agentId, RetraitDto dto);
        Task<TransactionResponseDto> ProcessBonus(int agentId, BonusDto dto);
        Task<TransactionResponseDto> ProcessCommission(int agentId, CommissionDto dto);
        Task<decimal> GetSoldeActuel(int agentId);
    }

    public class TransactionService : ITransactionService
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(ProsocDbContext db, ILogger<TransactionService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<TransactionResponseDto> ProcessCollecte(int agentId, CollecteDto dto)
        {
            return await ProcessTransaction(agentId, dto.Montant, "COLLECTE", dto.Description);
        }

        public async Task<TransactionResponseDto> ProcessRetrait(int agentId, RetraitDto dto)
        {
            return await ProcessTransaction(agentId, dto.Montant, "RETRAIT", $"{dto.Description} - {dto.MotifRetrait}");
        }

        public async Task<TransactionResponseDto> ProcessBonus(int agentId, BonusDto dto)
        {
            return await ProcessTransaction(agentId, dto.Montant, "BONUS", $"{dto.Description} - Source: {dto.SourceBonus}");
        }

        public async Task<TransactionResponseDto> ProcessCommission(int agentId, CommissionDto dto)
        {
            return await ProcessTransaction(agentId, dto.Montant, "COMMISSION", $"{dto.Description} - Taux: {dto.TauxCommission}%");
        }

        private async Task<TransactionResponseDto> ProcessTransaction(int agentId, decimal montant, string source, string description)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            
            try
            {
                // 1. Récupérer le wallet de l'agent
                var wallet = await _db.WalletsAgents
                    .FirstOrDefaultAsync(w => w.AgentId == agentId);
                
                if (wallet == null)
                {
                    await transaction.RollbackAsync();
                    return new TransactionResponseDto
                    {
                        Success = false,
                        Message = "Wallet non trouvé pour cet agent"
                    };
                }

                // 2. Validation du solde pour les retraits
                if (source == "RETRAIT" && wallet.SoldeCourant < montant)
                {
                    await transaction.RollbackAsync();
                    return new TransactionResponseDto
                    {
                        Success = false,
                        Message = $"Solde insuffisant. Solde actuel: {wallet.SoldeCourant}, Montant demandé: {montant}"
                    };
                }

                // 3. Créer le mouvement
                var mouvement = new WalletMouvement
                {
                    WalletId = wallet.IdWalletAgent,
                    DeviseId = wallet.DeviseId,
                    Montant = montant,
                    TypeOperation = source == "RETRAIT" ? "DEBIT" : "CREDIT",
                    Source = source,
                    DateOperation = DateTime.Now,
                    Description = description
                };

                // 4. Mettre à jour le solde du wallet
                if (source == "RETRAIT")
                    wallet.SoldeCourant -= montant;
                else
                    wallet.SoldeCourant += montant;

                wallet.DateModification = DateTime.Now;

                // 5. Sauvegarder les changements
                _db.WalletMouvements.Add(mouvement);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Transaction {Source} réussie pour Agent {AgentId}: Montant {Montant}, Nouveau solde {NouveauSolde}", 
                    source, agentId, montant, wallet.SoldeCourant);

                return new TransactionResponseDto
                {
                    Success = true,
                    Message = "Transaction effectuée avec succès",
                    NouveauSolde = wallet.SoldeCourant,
                    MouvementId = mouvement.IdWalletMouvement
                };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "Concurrence détectée lors de la transaction pour l'agent {AgentId}", agentId);
                return new TransactionResponseDto
                {
                    Success = false,
                    Message = "Transaction simultanée détectée, veuillez réessayer"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erreur lors de la transaction pour l'agent {AgentId}", agentId);
                return new TransactionResponseDto
                {
                    Success = false,
                    Message = "Erreur lors de la transaction"
                };
            }
        }

        public async Task<decimal> GetSoldeActuel(int agentId)
        {
            var wallet = await _db.WalletsAgents
                .FirstOrDefaultAsync(w => w.AgentId == agentId);
            
            return wallet?.SoldeCourant ?? 0;
        }
    }
}
