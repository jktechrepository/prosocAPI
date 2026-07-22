using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services.Repositories;
using ProsocAPI.Utilities;
using ProsocAPI.Exceptions;
using System.Collections.Generic;

namespace ProsocAPI.Services
{
    public class AdhesionService : IAdhesionRepository
    {
        private readonly ProsocDbContext _db;
        private readonly ICodeAdhesionGeneratorService _codeAdhesionGenerator;
        private readonly ILogger<AdhesionService> _logger;
        private readonly ICommissionService _commissionService;
        private readonly ICollecteMultideviseService _multidevise;

        public AdhesionService(
            ProsocDbContext db,
            ICodeAdhesionGeneratorService codeAdhesionGenerator,
            ILogger<AdhesionService> logger,
            ICommissionService commissionService,
            ICollecteMultideviseService multidevise)
        {
            _db = db;
            _codeAdhesionGenerator = codeAdhesionGenerator;
            _logger = logger;
            _commissionService = commissionService;
            _multidevise = multidevise;
        }

        public async Task<List<Adhesion>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Adhesions.AsNoTracking().OrderBy(x => x.IdAdhesion).ToListAsync(ct);
        }

        public async Task<Adhesion?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Adhesions.AsNoTracking().FirstOrDefaultAsync(x => x.IdAdhesion == id, ct);
        }

        public async Task<Adhesion?> GetByAffilieIdAsync(int affilieId, CancellationToken ct = default)
        {
            return await _db.Adhesions.AsNoTracking().FirstOrDefaultAsync(x => x.AffilieId == affilieId, ct);
        }

        public async Task<Adhesion> CreateAsync(Adhesion entity, CancellationToken ct = default)
        {
            _db.Adhesions.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<Adhesion> CreateWithAffilieAsync(
            Affilie affilie, 
            Adhesion adhesion, 
            IEnumerable<SouscriptionPrestation> souscriptions,
            IEnumerable<Collecte> collectes,
            int nombreDependants = 0,
            CancellationToken ct = default)
        {
            // 🆕 SUPPRIMÉ : La transaction est maintenant gérée par le controller
            // await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                var existingAffilie = await FindExistingAffilieAsync(affilie, ct);

                if (existingAffilie != null)
                {
                    var existingAdhesion = await _db.Adhesions
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.AffilieId == existingAffilie.IdAffilie, ct);

                    if (existingAdhesion != null)
                        throw new AdhesionAlreadyExistsException(existingAffilie.IdAffilie);

                    // ✅ OPTIMISATION : Préparer les modifications sans SaveChanges()
                    existingAffilie.Statut = true;
                    existingAffilie.DateModification = DateTime.Now;
                    existingAffilie.NomComplet = existingAffilie.Nom + " " + existingAffilie.Postnom + " " + existingAffilie.Prenom;
                    _db.Affilies.Update(existingAffilie);

                    adhesion.AffilieId = existingAffilie.IdAffilie;
                    _db.Adhesions.Add(adhesion);

                    foreach (var s in souscriptions)
                    {
                        s.AffilieId = existingAffilie.IdAffilie;
                        _db.SouscriptionsPrestations.Add(s);
                        _logger.LogInformation("Ajout de la souscription PrestationId={PrestationId} pour l'affilié {AffilieId}", s.PrestationId, existingAffilie.IdAffilie);
                    }

                    if (souscriptions.Any())
                        await _db.SaveChangesAsync(ct);

                    var existingAffilieSouscriptionsMap = souscriptions
                        .ToDictionary(s => s.PrestationId, s => s.IdSouscriptionPrestation);

                    foreach (var c in collectes)
                    {
                        c.AffilieId = existingAffilie.IdAffilie;
                        c.AgentId = AdhesionAgentIdHelper.ResolveCollecteAgentId(adhesion.AgentId);
                        c.DateCollecte = ResolveDateCollecte(c);
                        c.DateCreation = DateTime.Now;

                        if (c.TypeCollecte == TypeCollecte.Souscription && c.SouscriptionPrestationId.HasValue)
                        {
                            var prestationId = c.SouscriptionPrestationId.Value;
                            if (existingAffilieSouscriptionsMap.TryGetValue(prestationId, out var realSouscriptionId))
                                c.SouscriptionPrestationId = realSouscriptionId;
                            else
                                throw new ArgumentException(
                                    $"Aucune souscription trouvée pour prestationId {prestationId}. " +
                                    "Vérifiez collectes[].souscription.prestationId.");
                        }

                        var dateConversion = CollecteAdhesionHelper.ResolveDateConversionPaiement(
                            c.ModePaiement, c.DateCollecte);
                        await _multidevise.ValidateAndApplySnapshotAsync(
                            c, nombreDependants, ct, dateConversion);
                        _db.Collectes.Add(c);
                        
                        _logger.LogInformation("Ajout de la collecte: Type={TypeCollecte}, Montant={Montant}, ModePaiement={ModePaiement}", 
                            c.TypeCollecte, c.Montant, c.ModePaiement);
                    }

                    await _db.SaveChangesAsync(ct);

                    foreach (var c in collectes)
                    {
                        await ProcessCommissionForCollecteAsync(c, ct);
                    }

                    return adhesion;
                }

            affilie.CodeAdhesion = await GenerateCodeAdhesionForAffilieAsync(adhesion.TypeAdhesionId, affilie.ProvinceResidence, ct);
            
            affilie.NomComplet = affilie.Nom + " " + affilie.Postnom + " " + affilie.Prenom;
            _db.Affilies.Add(affilie);

            // ✅ SAUVEGARDER d'abord l'affilié pour obtenir l'ID
            await _db.SaveChangesAsync(ct);

            // 🆕 Création du compte utilisateur pour l'affilié (après sauvegarde)
            await CreateAffilieUserAsync(affilie, ct);

            adhesion.AffilieId = affilie.IdAffilie;
            _db.Adhesions.Add(adhesion);

            // ✅ OPTIMISATION : VALIDATION 2 - Éviter les doublons et mapping correct
            var souscriptionsAcreer = new List<SouscriptionPrestation>();
            var souscriptionsExistantesMap = new Dictionary<int, SouscriptionPrestation>(); // prestationId -> souscription existante

            foreach (var s in souscriptions)
            {
                var existingSouscription = await _db.SouscriptionsPrestations
                    .FirstOrDefaultAsync(sp => sp.AffilieId == affilie.IdAffilie && sp.PrestationId == s.PrestationId, ct);
                
                if (existingSouscription != null)
                {
                    _logger.LogInformation("Souscription déjà existante pour AffilieId={AffilieId}, PrestationId={PrestationId}", 
                        affilie.IdAffilie, s.PrestationId);
                    souscriptionsExistantesMap[s.PrestationId] = existingSouscription;
                }
                else
                {
                    s.AffilieId = affilie.IdAffilie;
                    souscriptionsAcreer.Add(s);
                }
            }

            // ✅ OPTIMISATION : Ajouter uniquement les nouvelles souscriptions
            foreach (var s in souscriptionsAcreer)
            {
                _db.SouscriptionsPrestations.Add(s);
            }

            if (souscriptionsAcreer.Count > 0)
                await _db.SaveChangesAsync(ct);

            // prestationId -> IdSouscriptionPrestation (existantes + nouvellement créées)
            var allSouscriptionsMap = new Dictionary<int, int>();
            foreach (var kvp in souscriptionsExistantesMap)
                allSouscriptionsMap[kvp.Key] = kvp.Value.IdSouscriptionPrestation;
            foreach (var s in souscriptionsAcreer)
                allSouscriptionsMap[s.PrestationId] = s.IdSouscriptionPrestation;

            foreach (var c in collectes)
            {
                c.AffilieId = affilie.IdAffilie;
                c.AgentId = AdhesionAgentIdHelper.ResolveCollecteAgentId(adhesion.AgentId);
                c.DateCollecte = ResolveDateCollecte(c);
                c.DateCreation = DateTime.Now;

                if (c.TypeCollecte == TypeCollecte.Souscription && c.SouscriptionPrestationId.HasValue)
                {
                    var prestationId = c.SouscriptionPrestationId.Value;
                    if (allSouscriptionsMap.TryGetValue(prestationId, out var realSouscriptionId))
                    {
                        c.SouscriptionPrestationId = realSouscriptionId;
                    }
                    else
                    {
                        throw new ArgumentException(
                            $"Aucune souscription trouvée pour prestationId {prestationId}. " +
                            "Vérifiez collectes[].souscription.prestationId.");
                    }
                }

                var dateConversion = CollecteAdhesionHelper.ResolveDateConversionPaiement(
                    c.ModePaiement, c.DateCollecte);
                await _multidevise.ValidateAndApplySnapshotAsync(
                    c, nombreDependants, ct, dateConversion);
                _db.Collectes.Add(c);
                
                _logger.LogInformation("Ajout de la collecte: Type={TypeCollecte}, Montant={Montant}, ModePaiement={ModePaiement}, SouscriptionId={SouscriptionId}", 
                    c.TypeCollecte, c.Montant, c.ModePaiement, c.SouscriptionPrestationId);
            }

            await _db.SaveChangesAsync(ct);

            // ✅ OPTIMISATION : Commissionnement dans la transaction
            foreach (var c in collectes)
            {
                await ProcessCommissionForCollecteAsync(c, ct);
            }

            // 🆕 SUPPRIMÉ : Le commit est maintenant géré par le controller
            // await tx.CommitAsync(ct);
            return adhesion;
            }
            catch (Exception)
            {
                // 🆕 SUPPRIMÉ : Le rollback est maintenant géré par le controller
                // await tx.RollbackAsync(ct);
                throw;
            }
        }

        private async Task<Affilie?> FindExistingAffilieAsync(Affilie input, CancellationToken ct)
        {
            var nom = NormalizeForMatch(input.Nom);
            var prenom = NormalizeForMatch(input.Prenom);
            var date = input.DateNaissance.Date;

            return await _db.Affilies
                .FirstOrDefaultAsync(a =>
                    a.DateNaissance.Date == date &&
                    ((a.Nom ?? string.Empty).Trim().ToUpper()) == nom &&
                    ((a.Prenom ?? string.Empty).Trim().ToUpper()) == prenom,
                    ct);
        }

        private static string NormalizeForMatch(string? value)
        {
            return (value ?? string.Empty).Trim().ToUpperInvariant();
        }

        private async Task<string> GenerateCodeAdhesionForAffilieAsync(int typeAdhesionId, string? provinceResidence, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(provinceResidence))
                throw new ArgumentException("ProvinceResidence est requis pour générer le CodeAdhesion");

            var type = await _db.TypeAdhesions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.IdTypeAdhesion == typeAdhesionId, ct);

            if (type == null)
                throw new ArgumentException($"TypeAdhesion avec ID {typeAdhesionId} introuvable");

            var typePrefix = type.Libelle.Length >= 2
                ? type.Libelle.Substring(0, 2).ToUpperInvariant()
                : type.Libelle.ToUpperInvariant().PadRight(2, 'X');

            var year2 = (DateTime.Now.Year % 100).ToString("00");

            var prov = provinceResidence.Trim();
            prov = prov.Length >= 3 ? prov.Substring(0, 3).ToUpperInvariant() : prov.ToUpperInvariant().PadRight(3, 'X');

            var prefix = $"{typePrefix}-{year2}-{prov}-";
            return await _codeAdhesionGenerator.GenerateCodeAdhesionAsync(prefix, ct);
        }

        public async Task<Adhesion?> UpdateAsync(int id, Adhesion entity, CancellationToken ct = default)
        {
            var existing = await _db.Adhesions.FirstOrDefaultAsync(x => x.IdAdhesion == id, ct);
            if (existing == null)
                return null;

            existing.StatutDossier = entity.StatutDossier;
            existing.AffilieId = entity.AffilieId;
            existing.TypeAdhesionId = entity.TypeAdhesionId;
            existing.AgentId = entity.AgentId;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.Adhesions.FirstOrDefaultAsync(x => x.IdAdhesion == id, ct);
            if (existing == null)
                return false;

            _db.Adhesions.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<AgentAffecterAffiliesResultDto?> AffecterAffiliesToAgentAsync(
            int agentId,
            IReadOnlyList<int> affilieIds,
            int? sourceAgentId = null,
            CancellationToken ct = default)
        {
            var agentExists = await _db.Agents.AnyAsync(a => a.IdAgent == agentId, ct);
            if (!agentExists)
                return null;

            var distinctAffilieIds = affilieIds.Distinct().ToList();

            if (distinctAffilieIds.Count == 0 && sourceAgentId.HasValue)
            {
                distinctAffilieIds = await _db.Adhesions
                    .Where(a => a.AgentId == sourceAgentId.Value && a.Statut && a.Affilie.Statut)
                    .Select(a => a.AffilieId)
                    .Distinct()
                    .ToListAsync(ct);

                _logger.LogInformation(
                    "Transfert massif affiliés : agent source {SourceAgentId} → agent cible {TargetAgentId}, {Count} affilié(s) résolu(s)",
                    sourceAgentId.Value, agentId, distinctAffilieIds.Count);
            }
            var resultats = new List<AgentAffilieAffectationItemDto>();
            var aTraiter = new List<(int AffilieId, Adhesion Adhesion, int? AncienAgentId)>();

            foreach (var affilieId in distinctAffilieIds)
            {
                var item = new AgentAffilieAffectationItemDto { AffilieId = affilieId };

                var affilie = await _db.Affilies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.IdAffilie == affilieId, ct);

                if (affilie == null)
                {
                    item.Message = "Affilié introuvable.";
                    resultats.Add(item);
                    continue;
                }

                if (!affilie.Statut)
                {
                    item.Message = "L'affilié est inactif.";
                    resultats.Add(item);
                    continue;
                }

                var adhesion = await _db.Adhesions
                    .FirstOrDefaultAsync(a => a.AffilieId == affilieId, ct);

                if (adhesion == null)
                {
                    item.Message = "Aucune adhésion pour cet affilié.";
                    resultats.Add(item);
                    continue;
                }

                if (!adhesion.Statut)
                {
                    item.Message = "L'adhésion est inactive.";
                    resultats.Add(item);
                    continue;
                }

                if (sourceAgentId.HasValue && adhesion.AgentId != sourceAgentId.Value)
                {
                    item.Message = "Affilié non rattaché à l'agent source.";
                    resultats.Add(item);
                    continue;
                }

                var ancienAgentId = adhesion.AgentId;
                if (adhesion.AgentId == agentId)
                {
                    item.Succes = true;
                    item.AdhesionId = adhesion.IdAdhesion;
                    item.AncienAgentId = ancienAgentId;
                    item.Message = "Déjà affecté à cet agent.";
                    resultats.Add(item);
                    continue;
                }

                aTraiter.Add((affilieId, adhesion, ancienAgentId));
            }

            if (aTraiter.Count > 0)
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);
                try
                {
                    var now = DateTime.Now;

                    foreach (var (affilieId, adhesion, ancienAgentId) in aTraiter)
                    {
                        adhesion.AgentId = agentId;
                        adhesion.DateModification = now;

                        var collectes = await _db.Collectes
                            .Where(c => c.AffilieId == affilieId)
                            .ToListAsync(ct);

                        foreach (var collecte in collectes)
                            collecte.AgentId = agentId;

                        resultats.Add(new AgentAffilieAffectationItemDto
                        {
                            AffilieId = affilieId,
                            Succes = true,
                            AdhesionId = adhesion.IdAdhesion,
                            AncienAgentId = ancienAgentId,
                            Message = ancienAgentId == agentId
                                ? "Déjà affecté à cet agent."
                                : "Affectation réussie."
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

            var totalReussites = resultats.Count(r => r.Succes);
            var resultByAffilieId = resultats.ToDictionary(r => r.AffilieId);
            return new AgentAffecterAffiliesResultDto
            {
                AgentId = agentId,
                TotalDemandes = distinctAffilieIds.Count,
                TotalReussites = totalReussites,
                TotalEchecs = distinctAffilieIds.Count - totalReussites,
                Resultats = distinctAffilieIds.Select(id => resultByAffilieId[id]).ToList()
            };
        }

        // 🆕 MÉTHODE UPDATE WITH AFFILIE
        public async Task<Adhesion> UpdateWithAffilieAsync(
            int adhesionId, 
            Affilie updatedAffilie, 
            Adhesion updatedAdhesion, 
            IEnumerable<SouscriptionPrestation> updatedSouscriptions,
            IEnumerable<Dependant> updatedDependants,
            CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                // 1. Vérifier que l'adhésion existe et est en attente
                var existingAdhesion = await _db.Adhesions
                    .Include(a => a.Affilie)
                    .FirstOrDefaultAsync(a => a.IdAdhesion == adhesionId, ct);

                if (existingAdhesion == null)
                    throw new AdhesionNotFoundException(adhesionId);

                if (existingAdhesion.StatutDossier != "EN ATTENTE")
                    throw new AdhesionNotInWaitingStateException(adhesionId);

                // 2. Valider que l'affilié a toutes les informations d'adresse requises
                ValidateAffilieAdresse(updatedAffilie);

                // 3. Vérifier les doublons d'affiliés
                await DetectAffilieDuplicatesAsync(updatedAffilie, existingAdhesion.AffilieId, ct);

                // 4. Mettre à jour l'affilié
                await UpdateAffilieAsync(existingAdhesion.Affilie, updatedAffilie, ct);

                // 5. Mettre à jour l'adhésion
                await UpdateAdhesionAsync(existingAdhesion, updatedAdhesion, ct);

                // 6. Gérer les souscriptions
                await ManageSouscriptionsAsync(adhesionId, updatedSouscriptions, ct);

                // 7. Gérer les dépendants
                await ManageDependantsAsync(existingAdhesion.AffilieId, updatedDependants, ct);

                // 8. Sauvegarder
                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return existingAdhesion;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        private void ValidateAffilieAdresse(Affilie affilie)
        {
            if (string.IsNullOrWhiteSpace(affilie.ProvinceResidence))
            {
                throw new AdresseAffilieIncompleteException(
                    "Informations d'adresse incomplètes. Champs requis: Province de résidence");
            }
        }

        private async Task DetectAffilieDuplicatesAsync(Affilie updatedAffilie, int currentAffilieId, CancellationToken ct)
        {
            var duplicates = await _db.Affilies
                .Where(a => a.IdAffilie != currentAffilieId)
                .Where(a => a.Nom.ToLower() == updatedAffilie.Nom.ToLower() 
                       && a.Prenom.ToLower() == updatedAffilie.Prenom.ToLower()
                       && a.DateNaissance == updatedAffilie.DateNaissance)
                .ToListAsync(ct);

            if (duplicates.Any())
                throw new AffilieDuplicateException(duplicates.First().IdAffilie, 
                    "Un affilié avec les mêmes informations existe déjà");
        }

        private async Task UpdateAffilieAsync(Affilie existingAffilie, Affilie updatedAffilie, CancellationToken ct)
        {
            // Mettre à jour tous les champs de l'affilié
            existingAffilie.Nom = updatedAffilie.Nom;
            existingAffilie.Prenom = updatedAffilie.Prenom;
            existingAffilie.DateNaissance = updatedAffilie.DateNaissance;
            existingAffilie.Telephone = updatedAffilie.Telephone;
            existingAffilie.Postnom = updatedAffilie.Postnom;
            existingAffilie.ProvinceResidence = updatedAffilie.ProvinceResidence;
            existingAffilie.CommuneResidence = updatedAffilie.CommuneResidence;
            existingAffilie.QuartierResidence = updatedAffilie.QuartierResidence;
            existingAffilie.AvenueResidence = updatedAffilie.AvenueResidence;
            existingAffilie.NumeroResidence = updatedAffilie.NumeroResidence;
            existingAffilie.CommuneActivite = updatedAffilie.CommuneActivite;
            existingAffilie.QuartierActivite = updatedAffilie.QuartierActivite;
            existingAffilie.AvenueActivite = updatedAffilie.AvenueActivite;
            existingAffilie.NumeroActivite = updatedAffilie.NumeroActivite;
            existingAffilie.DateModification = DateTime.Now;
            existingAffilie.Statut = updatedAffilie.Statut;

            // Mettre à jour le NomComplet automatiquement
            existingAffilie.NomComplet = $"{updatedAffilie.Nom} {updatedAffilie.Prenom}".Trim();
        }

        private async Task UpdateAdhesionAsync(Adhesion existingAdhesion, Adhesion updatedAdhesion, CancellationToken ct)
        {
            existingAdhesion.StatutDossier = updatedAdhesion.StatutDossier;
            existingAdhesion.DateModification = DateTime.Now;
        }

        private async Task ManageSouscriptionsAsync(
            int adhesionId, 
            IEnumerable<SouscriptionPrestation> updatedSouscriptions, 
            CancellationToken ct)
        {
            // 1. Récupérer les souscriptions existantes de l'affilié
            var existingAdhesion = await _db.Adhesions
                .Include(a => a.Affilie)
                .FirstOrDefaultAsync(a => a.IdAdhesion == adhesionId, ct);
            
            if (existingAdhesion == null)
                throw new AdhesionNotFoundException(adhesionId);

            var existingSouscriptions = await _db.SouscriptionsPrestations
                .Where(sp => sp.AffilieId == existingAdhesion.AffilieId)
                .ToListAsync(ct);

            var updatedSouscriptionsList = updatedSouscriptions.ToList();

            // 2. Identifier les souscriptions à supprimer
            var toDelete = existingSouscriptions
                .Where(es => !updatedSouscriptionsList.Any(us => us.PrestationId == es.PrestationId))
                .ToList();

            // 3. Identifier les souscriptions à ajouter
            var toAdd = updatedSouscriptionsList
                .Where(us => !existingSouscriptions.Any(es => es.PrestationId == us.PrestationId))
                .Select(us => new SouscriptionPrestation
                {
                    AffilieId = existingAdhesion.AffilieId,
                    PrestationId = us.PrestationId,
                    DateSouscription = us.DateSouscription,
                    Statut = us.Statut,
                    DateCreation = DateTime.Now
                })
                .ToList();

            // 4. Identifier les souscriptions à mettre à jour
            var toUpdate = existingSouscriptions
                .Where(es => updatedSouscriptionsList.Any(us => us.PrestationId == es.PrestationId))
                .Join(updatedSouscriptionsList, 
                    es => es.PrestationId, 
                    us => us.PrestationId, 
                    (es, us) => new { Existing = es, Updated = us })
                .ToList();

            // 5. Exécuter les opérations
            _db.SouscriptionsPrestations.RemoveRange(toDelete);
            _db.SouscriptionsPrestations.AddRange(toAdd);

            foreach (var item in toUpdate)
            {
                item.Existing.DateSouscription = item.Updated.DateSouscription;
                item.Existing.Statut = item.Updated.Statut;
                item.Existing.DateModification = DateTime.Now;
            }
        }

        private async Task ManageDependantsAsync(
            int affilieId, 
            IEnumerable<Dependant> updatedDependants, 
            CancellationToken ct)
        {
            // 1. Récupérer les dépendants existants
            var existingDependants = await _db.Dependants
                .Where(d => d.AffilieId == affilieId)
                .ToListAsync(ct);

            var updatedDependantsList = updatedDependants.ToList();

            // 2. Identifier les dépendants à supprimer
            var toDelete = existingDependants
                .Where(ed => !updatedDependantsList.Any(ud => ud.IdDependant != 0 && ud.IdDependant == ed.IdDependant))
                .ToList();

            // 3. Identifier les dépendants à ajouter (ceux avec IdDependant = 0)
            var toAdd = updatedDependantsList
                .Where(ud => ud.IdDependant == 0)
                .Select(ud => new Dependant
                {
                    AffilieId = affilieId,
                    Nom = ud.Nom,
                    Adresse = ud.Adresse,
                    LienParente = ud.LienParente,
                    DateNaissance = ud.DateNaissance,
                    Telephone = ud.Telephone,
                    CertificatScolariteData = ud.CertificatScolariteData,
                    CertificatScolariteContentType = ud.CertificatScolariteContentType,
                    Statut = ud.Statut,
                    DateCreation = DateTime.Now
                })
                .ToList();

            // 4. Identifier les dépendants à mettre à jour
            var toUpdate = existingDependants
                .Where(ed => updatedDependantsList.Any(ud => ud.IdDependant != 0 && ud.IdDependant == ed.IdDependant))
                .Join(updatedDependantsList.Where(ud => ud.IdDependant != 0), 
                    ed => ed.IdDependant, 
                    ud => ud.IdDependant, 
                    (ed, ud) => new { Existing = ed, Updated = ud })
                .ToList();

            // 5. Exécuter les opérations
            _db.Dependants.RemoveRange(toDelete);
            _db.Dependants.AddRange(toAdd);

            foreach (var item in toUpdate)
            {
                item.Existing.Nom = item.Updated.Nom;
                item.Existing.Adresse = item.Updated.Adresse;
                item.Existing.LienParente = item.Updated.LienParente;
                item.Existing.DateNaissance = item.Updated.DateNaissance;
                item.Existing.Telephone = item.Updated.Telephone;
                if (item.Updated.CertificatScolariteData != null && item.Updated.CertificatScolariteData.Length > 0)
                {
                    item.Existing.CertificatScolariteData = item.Updated.CertificatScolariteData;
                    item.Existing.CertificatScolariteContentType = item.Updated.CertificatScolariteContentType;
                }
                item.Existing.Statut = item.Updated.Statut;
                item.Existing.DateModification = DateTime.Now;
            }
        }

        public async Task<PersonneContact> CreateOrUpdatePersonneContactAsync(
            int affilieId,
            PersonneContact personneContact,
            CancellationToken ct = default)
        {
            await UpsertPersonneContactAsync(affilieId, personneContact, ct);
            await _db.SaveChangesAsync(ct);

            return await _db.PersonnesContact
                .FirstAsync(p => p.AffilieId == affilieId, ct);
        }

        public async Task<Adhesion> CompleteNiveau2EncodeurAsync(
            int adhesionId,
            IEnumerable<Dependant> dependants,
            PersonneContact? personneContact,
            AdhesionNiveau2EncodeurDto input,
            CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                var adhesion = await _db.Adhesions
                    .Include(a => a.Affilie)
                    .FirstOrDefaultAsync(a => a.IdAdhesion == adhesionId, ct);

                if (adhesion == null)
                    throw new AdhesionNotFoundException(adhesionId);

                if (!string.Equals(
                        adhesion.StatutDossier?.Trim(),
                        AdhesionNiveau2Regles.StatutEnAttente,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new AdhesionNotInWaitingStateException(adhesionId);
                }

                await ManageDependantsAsync(adhesion.AffilieId, dependants, ct);
                if (personneContact != null)
                    await UpsertPersonneContactAsync(adhesion.AffilieId, personneContact, ct);

                AdhesionNiveau2Regles.AppliquerIdentiteActivite(adhesion.Affilie, input);

                AffilieFichierApplicator.AppliquerPiecesIdentiteOptionnelles(
                    adhesion.Affilie,
                    input.PhotoBase64,
                    input.PhotoContentType,
                    input.CarteIdentiteBase64,
                    input.CarteIdentiteContentType);

                if (input.Valider)
                {
                    adhesion.StatutDossier = AdhesionNiveau2Regles.StatutValide;
                    adhesion.DateModification = DateTime.Now;
                }

                adhesion.Affilie.DateModification = DateTime.Now;

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return adhesion;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        private async Task UpsertPersonneContactAsync(
            int affilieId,
            PersonneContact personneContact,
            CancellationToken ct)
        {
            var existing = await _db.PersonnesContact
                .FirstOrDefaultAsync(p => p.AffilieId == affilieId, ct);

            if (existing == null)
            {
                personneContact.AffilieId = affilieId;
                personneContact.DateCreation = DateTime.Now;
                personneContact.Statut = true;
                _db.PersonnesContact.Add(personneContact);
                return;
            }

            existing.NomComplet = personneContact.NomComplet;
            existing.LienParente = personneContact.LienParente;
            existing.Adresse = personneContact.Adresse;
            existing.Statut = true;
            existing.DateModification = DateTime.Now;
        }

    // 🆕 MÉTHODES POUR GÉRER LES DÉPENDANTS
    public async Task<List<Dependant>> CreateDependantsAsync(int affilieId, IEnumerable<Dependant> dependants, CancellationToken ct = default)
    {
        var dependantsToCreate = dependants.Select(d => new Dependant
        {
            AffilieId = affilieId,
            Nom = d.Nom.Trim(),
            Adresse = d.Adresse,
            LienParente = d.LienParente,
            DateNaissance = d.DateNaissance,
            Telephone = d.Telephone,
            CertificatScolariteData = d.CertificatScolariteData,
            CertificatScolariteContentType = d.CertificatScolariteContentType,
            DateCreation = DateTime.Now,
            Statut = true
        }).ToList();

        _db.Dependants.AddRange(dependantsToCreate);
        await _db.SaveChangesAsync(ct);
        
        _logger.LogInformation("Création de {Count} dépendants pour l'affilié {AffilieId}", dependantsToCreate.Count, affilieId);
        return dependantsToCreate;
    }

    public async Task<List<Dependant>> GetDependantsByAffilieIdAsync(int affilieId, CancellationToken ct = default)
    {
        return await _db.Dependants
            .AsNoTracking()
            .Include(d => d.Antecedants)
                .ThenInclude(a => a.Affilie)
            .Where(d => d.AffilieId == affilieId && d.Statut)
            .OrderBy(d => d.Nom)
            .ToListAsync(ct);
    }

    public async Task<bool> DeleteDependantsAsync(int affilieId, CancellationToken ct = default)
    {
        var dependants = await _db.Dependants
            .Where(d => d.AffilieId == affilieId)
            .ToListAsync(ct);

        if (dependants.Any())
        {
            _db.Dependants.RemoveRange(dependants);
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Suppression de {Count} dépendants pour l'affilié {AffilieId}", dependants.Count, affilieId);
            return true;
        }

        return false;
    }

    // 🆕 CRÉATION DU COMPTE UTILISATEUR POUR L'AFFILIÉ
    private async Task CreateAffilieUserAsync(Affilie affilie, CancellationToken ct)
    {
        // Vérifier si l'utilisateur n'existe pas déjà
        var existingUser = await _db.Utilisateurs
            .AnyAsync(u => u.AffilieId == affilie.IdAffilie, ct);
        
        if (!existingUser)
        {
            _logger.LogInformation("Création du compte utilisateur pour l'affilié: {AffilieId}", affilie.IdAffilie);
            
            var affilieRole = await _db.Roles
                .FirstOrDefaultAsync(r => r.Nom == "Affilié" || r.Code == "AF", ct);

            if (affilieRole == null)
            {
                _logger.LogWarning("Rôle « Affilié » introuvable, utilisation du rôle par défaut");
                affilieRole = await _db.Roles.FirstOrDefaultAsync(r => r.Nom == "User", ct);
            }

            if (affilieRole != null)
            {
                var utilisateur = new Models.Authentication.Utilisateur
                {
                    NomUtilisateur = affilie.NomComplet,
                    DefaultUsername = affilie.CodeAdhesion,
                    EmailUtilisateur = affilie.EmailAffilie,
                    PhoneUtilisateur = PhoneNumberHelper.NormalizeForStorage(affilie.Telephone)
                        ?? affilie.Telephone?.Trim(), 
                    MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("123456"), // ✅ Hasher le mot de passe par défaut
                    DoitChangerMotDePasse = true,
                    Statut = true,
                    AffilieId = affilie.IdAffilie,
                    DateCreation = DateTime.Now
                };

                _db.Utilisateurs.Add(utilisateur);
                await _db.SaveChangesAsync(ct);

                // Ajouter le rôle utilisateur
                var userRole = new Models.Authentication.UserRole
                {
                    UtilisateurId = utilisateur.IdUtilisateur,
                    RoleId = affilieRole.IdRole,
                    IsPrimary = true,
                    Statut = true,
                    DateAttribution = DateTime.Now
                };

                _db.UserRoles.Add(userRole);
                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Compte utilisateur créé avec succès pour l'affilié {AffilieId} - NomUtilisateur: {NomUtilisateur}, DefaultUsername: {DefaultUsername}", 
                    affilie.IdAffilie, affilie.NomComplet, affilie.CodeAdhesion);
            }
            else
            {
                _logger.LogError("Impossible de créer le compte utilisateur pour l'affilié {AffilieId}: aucun rôle trouvé", 
                    affilie.IdAffilie);
            }
        }
        else
        {
            _logger.LogInformation("Un compte utilisateur existe déjà pour l'affilié: {AffilieId}", affilie.IdAffilie);
        }
    }

    private static DateTime ResolveDateCollecte(Collecte collecte)
    {
        if (collecte.Mois is >= 1 and <= 12 && collecte.Annee is >= 2020 and <= 2100)
            return new DateTime(collecte.Annee, collecte.Mois, 1);

        return DateTime.Now;
    }

    private async Task ProcessCommissionForCollecteAsync(Collecte collecte, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("=== DÉBUT COMMISSIONNEMENT POUR ADHÉSION ===");
            _logger.LogInformation("Appel du CommissionService pour la collecte {CollecteId}", collecte.IdCollecte);
            
            await _commissionService.ProcessCommissionAsync(collecte, ct);
            
            _logger.LogInformation("CommissionService terminé avec succès");
            _logger.LogInformation("=== FIN COMMISSIONNEMENT POUR ADHÉSION ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du commissionnement pour la collecte {CollecteId}", collecte.IdCollecte);
            // Ne pas bloquer l'adhésion si le commissionnement échoue
            _logger.LogWarning("L'adhésion a été créée mais le commissionnement a échoué");
        }
    }
    }
}
