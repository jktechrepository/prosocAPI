using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class DemandeBonEnvoiService : IDemandeBonEnvoiRepository
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<DemandeBonEnvoiService> _logger;
        private readonly IJetonMedicalRepository _jetonMedicalRepository;
        private readonly IBonEnvoiRepository _bonEnvoiRepository;
        private readonly IAffilieRepository _affilieRepository;
        private readonly IAdhesionRepository _adhesionRepository;
        private readonly IBonEnvoiQrCodeService _qrCodeService;

        public DemandeBonEnvoiService(
            ProsocDbContext db, 
            ILogger<DemandeBonEnvoiService> logger,
            IJetonMedicalRepository jetonMedicalRepository,
            IBonEnvoiRepository bonEnvoiRepository,
            IAffilieRepository affilieRepository,
            IAdhesionRepository adhesionRepository,
            IBonEnvoiQrCodeService qrCodeService)
        {
            _db = db;
            _logger = logger;
            _jetonMedicalRepository = jetonMedicalRepository;
            _bonEnvoiRepository = bonEnvoiRepository;
            _affilieRepository = affilieRepository;
            _adhesionRepository = adhesionRepository;
            _qrCodeService = qrCodeService;
        }

        public async Task<List<DemandeBonEnvoi>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.DemandesBonEnvoi
                .Include(d => d.Affilie)
                .Include(d => d.Prestation)
                .Include(d => d.Agent)
                .Include(d => d.BonEnvoi)
                .Include(d => d.JetonMedical)
                .OrderByDescending(d => d.DateDemande)
                .ToListAsync(ct);
        }

        public async Task<DemandeBonEnvoi?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.DemandesBonEnvoi
                .Include(d => d.Affilie)
                .Include(d => d.Prestation)
                .Include(d => d.Agent)
                .Include(d => d.BonEnvoi)
                .Include(d => d.JetonMedical)
                .FirstOrDefaultAsync(d => d.IdDemande == id, ct);
        }

        public async Task<List<DemandeBonEnvoi>> GetByAffilieAsync(int affilieId, CancellationToken ct = default)
        {
            return await _db.DemandesBonEnvoi
                .Include(d => d.Affilie)
                .Include(d => d.Prestation)
                .Include(d => d.Agent)
                .Include(d => d.BonEnvoi)
                .Include(d => d.JetonMedical)
                .Where(d => d.AffilieId == affilieId)
                .OrderByDescending(d => d.DateDemande)
                .ToListAsync(ct);
        }

        public async Task<List<DemandeBonEnvoi>> GetByStatutAsync(string statut, CancellationToken ct = default)
        {
            return await _db.DemandesBonEnvoi
                .Include(d => d.Affilie)
                .Include(d => d.Prestation)
                .Include(d => d.Agent)
                .Include(d => d.BonEnvoi)
                .Include(d => d.JetonMedical)
                .Where(d => d.StatutDemande == statut)
                .OrderByDescending(d => d.DateDemande)
                .ToListAsync(ct);
        }

        public async Task<DemandeBonEnvoi> CreateAsync(DemandeBonEnvoi entity, CancellationToken ct = default)
        {
            _db.DemandesBonEnvoi.Add(entity);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Demande de bon d'envoi créée: {IdDemande} pour l'affilié {AffilieId}", 
                entity.IdDemande, entity.AffilieId);

            return entity;
        }

        public async Task<DemandeBonEnvoi?> UpdateAsync(int id, DemandeBonEnvoi entity, CancellationToken ct = default)
        {
            var existing = await _db.DemandesBonEnvoi.FirstOrDefaultAsync(d => d.IdDemande == id, ct);
            if (existing == null)
                return null;

            existing.StatutDemande = entity.StatutDemande;
            existing.DateValidation = entity.DateValidation;
            existing.BonEnvoiId = entity.BonEnvoiId;
            existing.JetonMedicalId = entity.JetonMedicalId;
            existing.DateModification = DateTime.Now;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.DemandesBonEnvoi.FirstOrDefaultAsync(d => d.IdDemande == id, ct);
            if (existing == null)
                return false;

            _db.DemandesBonEnvoi.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        /// <summary>
        /// Vérifie si l'affilié est en ordre pour recevoir un bon d'envoi
        /// </summary>
        public async Task<VerificationEligibiliteDto> VerifierEligibiliteAsync(int affilieId, CancellationToken ct = default)
        {
            var affilié = await _affilieRepository.GetByIdAsync(affilieId, ct);
            if (affilié == null)
            {
                return new VerificationEligibiliteDto
                {
                    EstEligible = false,
                    Motif = "Affilié non trouvé"
                };
            }

            // Vérifier le statut de l'affilié
            if (!affilié.Statut)
            {
                return new VerificationEligibiliteDto
                {
                    EstEligible = false,
                    Motif = "Affilié inactif"
                };
            }

            // Vérifier l'adhésion
            var adhesionActive = await _adhesionRepository.GetByAffilieIdAsync(affilieId, ct);
            
            if (adhesionActive == null)
            {
                return new VerificationEligibiliteDto
                {
                    EstEligible = false,
                    Motif = "Aucune adhésion active"
                };
            }

            // Vérifier si le dossier est complet
            if (adhesionActive.StatutDossier != "Complet")
            {
                return new VerificationEligibiliteDto
                {
                    EstEligible = false,
                    Motif = $"Dossier d'adhésion incomplet: {adhesionActive.StatutDossier}"
                };
            }

            // Vérifier les cotisations (dernier 3 mois)
            var dateLimite = DateTime.Now.AddMonths(-3);
            var cotisationsRecentes = await _db.Collectes
                .Where(c => c.AffilieId == affilieId
                    && c.DateCollecte >= dateLimite
                    && c.StatutPaiement != null
                    && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement))
                .CountAsync(ct);

            if (cotisationsRecentes < 3)
            {
                return new VerificationEligibiliteDto
                {
                    EstEligible = false,
                    Motif = $"Cotisations insuffisantes: {cotisationsRecentes}/3 mois requis"
                };
            }

            // Vérifier les souscriptions de prestations
            var souscriptions = await _db.SouscriptionsPrestations
                .Include(sp => sp.Prestation)
                .Where(sp => sp.AffilieId == affilieId && sp.Statut)
                .ToListAsync(ct);

            if (!souscriptions.Any())
            {
                return new VerificationEligibiliteDto
                {
                    EstEligible = false,
                    Motif = "Aucune prestation souscrite"
                };
            }

            return new VerificationEligibiliteDto
            {
                EstEligible = true,
                Motif = "Éligible",
                AffilieNom = affilié.NomComplet,
                AdhesionStatut = adhesionActive.StatutDossier,
                CotisationsOk = cotisationsRecentes >= 3,
                PrestationsSouscrites = souscriptions.Select(sp => sp.Prestation?.NomPrestation).ToList()!
            };
        }

        /// <summary>
        /// Confirmation agent en un clic : accepte (bon + jeton + QR) ou rejette la demande.
        /// </summary>
        public async Task<DemandeBonEnvoiConfirmationResultDto> ConfirmerDemandeAsync(
            int demandeId,
            DemandeBonEnvoiConfirmerDto dto,
            CancellationToken ct = default)
        {
            if (dto.AgentId <= 0)
            {
                return new DemandeBonEnvoiConfirmationResultDto
                {
                    Succes = false,
                    Message = "L'identifiant agent est obligatoire.",
                    IdDemande = demandeId
                };
            }

            var demande = await _db.DemandesBonEnvoi
                .Include(d => d.Prestation)
                .Include(d => d.Affilie)
                .FirstOrDefaultAsync(d => d.IdDemande == demandeId, ct);

            if (demande == null)
            {
                return new DemandeBonEnvoiConfirmationResultDto
                {
                    Succes = false,
                    Message = "Demande non trouvée.",
                    IdDemande = demandeId
                };
            }

            if (demande.StatutDemande != "EN_ATTENTE")
            {
                return new DemandeBonEnvoiConfirmationResultDto
                {
                    Succes = false,
                    Message = $"Demande déjà traitée (statut : {demande.StatutDemande}).",
                    IdDemande = demandeId,
                    StatutDemande = demande.StatutDemande
                };
            }

            var agentExists = await _db.Agents.AnyAsync(a => a.IdAgent == dto.AgentId && a.Statut, ct);
            if (!agentExists)
            {
                return new DemandeBonEnvoiConfirmationResultDto
                {
                    Succes = false,
                    Message = "Agent introuvable ou inactif.",
                    IdDemande = demandeId
                };
            }

            if (!dto.Accepter)
            {
                demande.StatutDemande = "REJETEE";
                demande.AgentId = dto.AgentId;
                demande.ObservationAgent = dto.MotifRejet ?? dto.ObservationAgent;
                demande.DateValidation = DateTime.Now;
                demande.DateModification = DateTime.Now;
                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Demande #{DemandeId} rejetée par l'agent {AgentId}", demandeId, dto.AgentId);

                return new DemandeBonEnvoiConfirmationResultDto
                {
                    Succes = true,
                    Message = "Demande rejetée.",
                    IdDemande = demandeId,
                    StatutDemande = "REJETEE"
                };
            }

            if (!dto.HopitalPartenaireId.HasValue || dto.HopitalPartenaireId.Value <= 0)
            {
                return new DemandeBonEnvoiConfirmationResultDto
                {
                    Succes = false,
                    Message = "L'hôpital partenaire est obligatoire pour valider la demande.",
                    IdDemande = demandeId
                };
            }

            var hopitalExists = await _db.HopitalPartenaires
                .AnyAsync(h => h.IdHopital == dto.HopitalPartenaireId.Value && h.Statut, ct);
            if (!hopitalExists)
            {
                return new DemandeBonEnvoiConfirmationResultDto
                {
                    Succes = false,
                    Message = "Hôpital partenaire introuvable ou inactif.",
                    IdDemande = demandeId
                };
            }

            var verification = await VerifierEligibiliteAsync(demande.AffilieId, ct);
            if (!verification.EstEligible)
            {
                demande.StatutDemande = "REJETEE";
                demande.AgentId = dto.AgentId;
                demande.ObservationAgent = verification.Motif;
                demande.DateValidation = DateTime.Now;
                demande.DateModification = DateTime.Now;
                await _db.SaveChangesAsync(ct);

                return new DemandeBonEnvoiConfirmationResultDto
                {
                    Succes = false,
                    Message = $"Demande rejetée : {verification.Motif}",
                    IdDemande = demandeId,
                    StatutDemande = "REJETEE"
                };
            }

            try
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                var jeton = new JetonMedical
                {
                    AffilieId = demande.AffilieId,
                    HopitalPartenaireId = dto.HopitalPartenaireId.Value,
                    Observation = $"Demande #{demande.IdDemande} - {demande.Prestation?.NomPrestation}",
                    DateExpiration = DateTime.Now.AddDays(30),
                    CodeJeton = await GenerateUniqueJetonCodeAsync(ct)
                };
                _db.JetonsMedicaux.Add(jeton);
                await _db.SaveChangesAsync(ct);

                var bon = new BonEnvoi
                {
                    AffilieId = demande.AffilieId,
                    PrestationId = demande.PrestationId,
                    NumeroBon = await GenererNumeroBonAsync(ct),
                    DateEmission = DateTime.Now,
                    Statut = true,
                    JetonMedicalId = jeton.IdJeton
                };
                _db.BonsEnvoi.Add(bon);
                await _db.SaveChangesAsync(ct);
                await _qrCodeService.ApplyQrToBonAsync(bon, ct);
                await _db.SaveChangesAsync(ct);

                demande.StatutDemande = "VALIDEE";
                demande.AgentId = dto.AgentId;
                demande.ObservationAgent = dto.ObservationAgent;
                demande.DateValidation = DateTime.Now;
                demande.BonEnvoiId = bon.IdBonEnvoi;
                demande.JetonMedicalId = jeton.IdJeton;
                demande.DateModification = DateTime.Now;

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                _logger.LogInformation(
                    "Demande #{DemandeId} confirmée - Bon {BonNumero}, Jeton {JetonCode}",
                    demandeId, bon.NumeroBon, jeton.CodeJeton);

                return new DemandeBonEnvoiConfirmationResultDto
                {
                    Succes = true,
                    Message = "Demande confirmée. Bon d'envoi et jeton médical générés.",
                    IdDemande = demandeId,
                    StatutDemande = "VALIDEE",
                    BonEnvoiId = bon.IdBonEnvoi,
                    BonEnvoiNumero = bon.NumeroBon,
                    QrCodePayload = bon.QrCodePayload,
                    QrCodeImageBase64 = bon.QrCodeImageBase64,
                    JetonMedicalId = jeton.IdJeton,
                    JetonMedicalCode = jeton.CodeJeton
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur confirmation demande {DemandeId}", demandeId);
                return new DemandeBonEnvoiConfirmationResultDto
                {
                    Succes = false,
                    Message = "Erreur technique lors de la confirmation.",
                    IdDemande = demandeId
                };
            }
        }

        /// <summary>
        /// Valide une demande et génère le bon d'envoi + jeton médical (legacy — préférer ConfirmerDemandeAsync).
        /// </summary>
        public async Task<ResultatGenerationDto> ValiderEtGenererAsync(int demandeId, int agentId, int hopitalPartenaireId, CancellationToken ct = default)
        {
            var result = await ConfirmerDemandeAsync(demandeId, new DemandeBonEnvoiConfirmerDto
            {
                AgentId = agentId,
                Accepter = true,
                HopitalPartenaireId = hopitalPartenaireId
            }, ct);

            return new ResultatGenerationDto
            {
                Succes = result.Succes,
                Message = result.Message,
                BonEnvoiId = result.BonEnvoiId,
                BonEnvoiNumero = result.BonEnvoiNumero,
                JetonMedicalId = result.JetonMedicalId,
                JetonMedicalCode = result.JetonMedicalCode
            };
        }

        public async Task<DemandeBonEnvoiStatsDto> GetStatsAsync(DateTime date, CancellationToken ct = default)
        {
            var debutMois = new DateTime(date.Year, date.Month, 1);
            var finMois = debutMois.AddMonths(1).AddDays(-1);

            var totalDemandes = await _db.DemandesBonEnvoi
                .Where(d => d.DateDemande >= debutMois && d.DateDemande <= finMois)
                .CountAsync(ct);

            var demandesEnAttente = await _db.DemandesBonEnvoi
                .Where(d => d.DateDemande >= debutMois && d.DateDemande <= finMois && d.StatutDemande == "EN_ATTENTE")
                .CountAsync(ct);

            var demandesValidees = await _db.DemandesBonEnvoi
                .Where(d => d.DateDemande >= debutMois && d.DateDemande <= finMois && d.StatutDemande == "VALIDEE")
                .CountAsync(ct);

            var demandesRejetees = await _db.DemandesBonEnvoi
                .Where(d => d.DateDemande >= debutMois && d.DateDemande <= finMois && d.StatutDemande == "REJETEE")
                .CountAsync(ct);

            var bonsGeneres = await _db.DemandesBonEnvoi
                .Where(d => d.DateDemande >= debutMois && d.DateDemande <= finMois && d.BonEnvoiId.HasValue)
                .CountAsync(ct);

            var jetonsGeneres = await _db.DemandesBonEnvoi
                .Where(d => d.DateDemande >= debutMois && d.DateDemande <= finMois && d.JetonMedicalId.HasValue)
                .CountAsync(ct);

            var tauxValidation = totalDemandes > 0 ? (decimal)demandesValidees / totalDemandes * 100 : 0;

            return new DemandeBonEnvoiStatsDto
            {
                TotalDemandes = totalDemandes,
                DemandesEnAttente = demandesEnAttente,
                DemandesValidees = demandesValidees,
                DemandesRejetees = demandesRejetees,
                BonsGeneres = bonsGeneres,
                JetonsGeneres = jetonsGeneres,
                TauxValidation = Math.Round(tauxValidation, 2),
                DateStats = date
            };
        }

        private async Task<string> GenererNumeroBonAsync(CancellationToken ct = default)
        {
            const string prefix = "BON";
            var random = new Random();
            string numero;
            int attempts = 0;
            const int maxAttempts = 10;

            do
            {
                var suffix = random.Next(100000, 999999).ToString();
                numero = $"{prefix}{suffix}";
                
                var exists = await _db.BonsEnvoi
                    .AnyAsync(b => b.NumeroBon == numero, ct);
                
                if (!exists)
                    return numero;
                
                attempts++;
            } while (attempts < maxAttempts);

            throw new InvalidOperationException("Impossible de générer un numéro de bon unique après plusieurs tentatives");
        }

        private async Task<string> GenerateUniqueJetonCodeAsync(CancellationToken ct = default)
        {
            const int maxAttempts = 10;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                var random = new Random();
                var buffer = new char[11];
                buffer[0] = 'J';
                buffer[1] = 'E';
                buffer[2] = 'T';
                for (var i = 3; i < buffer.Length; i++)
                {
                    buffer[i] = chars[random.Next(chars.Length)];
                }

                var code = new string(buffer);
                var exists = await _db.JetonsMedicaux.AnyAsync(j => j.CodeJeton == code, ct);
                if (!exists)
                {
                    return code;
                }
            }

            throw new InvalidOperationException("Impossible de générer un code jeton unique après plusieurs tentatives");
        }
    }

    // DTOs pour les opérations complexes
    public class VerificationEligibiliteDto
    {
        public bool EstEligible { get; set; }
        public string Motif { get; set; } = string.Empty;
        public string? AffilieNom { get; set; }
        public string? AdhesionStatut { get; set; }
        public bool CotisationsOk { get; set; }
        public List<string>? PrestationsSouscrites { get; set; }
    }

    public class ResultatGenerationDto
    {
        public bool Succes { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? BonEnvoiId { get; set; }
        public string? BonEnvoiNumero { get; set; }
        public int? JetonMedicalId { get; set; }
        public string? JetonMedicalCode { get; set; }
    }
}
