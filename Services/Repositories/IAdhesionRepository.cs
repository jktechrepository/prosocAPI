using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IAdhesionRepository
    {
        Task<List<Adhesion>> GetAllAsync(CancellationToken ct = default);
        Task<Adhesion?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Adhesion?> GetByAffilieIdAsync(int affilieId, CancellationToken ct = default);
        Task<Adhesion> CreateAsync(Adhesion entity, CancellationToken ct = default);
        Task<Adhesion> CreateWithAffilieAsync(
            Affilie affilie,
            Adhesion adhesion,
            IEnumerable<SouscriptionPrestation> souscriptions,
            IEnumerable<Collecte> collectes,
            int nombreDependants = 0,
            CancellationToken ct = default);
        
        // 🆕 Méthode pour gérer les dépendants
        Task<List<Dependant>> CreateDependantsAsync(int affilieId, IEnumerable<Dependant> dependants, CancellationToken ct = default);
        Task<List<Dependant>> GetDependantsByAffilieIdAsync(int affilieId, CancellationToken ct = default);
        Task<bool> DeleteDependantsAsync(int affilieId, CancellationToken ct = default);
        Task<Adhesion?> UpdateAsync(int id, Adhesion entity, CancellationToken ct = default);
        
        // 🆕 Méthode UpdateWithAffilieAsync
        Task<Adhesion> UpdateWithAffilieAsync(
            int adhesionId, 
            Affilie updatedAffilie, 
            Adhesion updatedAdhesion, 
            IEnumerable<SouscriptionPrestation> updatedSouscriptions,
            IEnumerable<Dependant> updatedDependants,
            CancellationToken ct = default);

        Task<PersonneContact> CreateOrUpdatePersonneContactAsync(
            int affilieId,
            PersonneContact personneContact,
            CancellationToken ct = default);

        Task<Adhesion> CompleteNiveau2EncodeurAsync(
            int adhesionId,
            IEnumerable<Dependant> dependants,
            PersonneContact? personneContact,
            AdhesionNiveau2EncodeurDto input,
            CancellationToken ct = default);
        
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);

        Task<AgentAffecterAffiliesResultDto?> AffecterAffiliesToAgentAsync(
            int agentId,
            IReadOnlyList<int> affilieIds,
            int? sourceAgentId = null,
            CancellationToken ct = default);
    }
}
