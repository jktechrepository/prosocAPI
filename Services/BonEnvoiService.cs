using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services.Repositories;
using Prosoc.Utilities;

namespace ProsocAPI.Services
{
    public class BonEnvoiService : IBonEnvoiRepository
    {
        private readonly ProsocDbContext _db;
        private readonly IBonEnvoiQrCodeService _qrCodeService;

        public BonEnvoiService(ProsocDbContext db, IBonEnvoiQrCodeService qrCodeService)
        {
            _db = db;
            _qrCodeService = qrCodeService;
        }

        public async Task<List<BonEnvoi>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.BonsEnvoi
                .Include(b => b.Affilie)
                .Include(b => b.Prestation)
                .AsNoTracking()
                .OrderByDescending(x => x.DateEmission)
                .ToListAsync(ct);
        }

        public async Task<BonEnvoi?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.BonsEnvoi
                .Include(b => b.Affilie)
                .Include(b => b.Prestation)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdBonEnvoi == id, ct);
        }

        public async Task<BonEnvoi?> GetByNumeroBonAsync(string numeroBon, CancellationToken ct = default)
        {
            return await _db.BonsEnvoi
                .Include(b => b.Affilie)
                .Include(b => b.Prestation)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.NumeroBon == numeroBon, ct);
        }

        public async Task<List<BonEnvoi>> GetByAffilieAsync(int affilieId, CancellationToken ct = default)
        {
            return await _db.BonsEnvoi
                .Include(b => b.Affilie)
                .Include(b => b.Prestation)
                .AsNoTracking()
                .Where(x => x.AffilieId == affilieId)
                .OrderByDescending(x => x.DateEmission)
                .ToListAsync(ct);
        }

        public async Task<List<BonEnvoi>> GetByPrestationAsync(int prestationId, CancellationToken ct = default)
        {
            return await _db.BonsEnvoi
                .Include(b => b.Affilie)
                .Include(b => b.Prestation)
                .AsNoTracking()
                .Where(x => x.PrestationId == prestationId)
                .OrderByDescending(x => x.DateEmission)
                .ToListAsync(ct);
        }

        public async Task<List<BonEnvoi>> GetNonUtilisesAsync(CancellationToken ct = default)
        {
            return await _db.BonsEnvoi
                .Include(b => b.Affilie)
                .Include(b => b.Prestation)
                .AsNoTracking()
                .Where(x => !x.EstUtilise && x.Statut)
                .OrderByDescending(x => x.DateEmission)
                .ToListAsync(ct);
        }

        public async Task<List<BonEnvoi>> GetUtilisesAsync(CancellationToken ct = default)
        {
            return await _db.BonsEnvoi
                .Include(b => b.Affilie)
                .Include(b => b.Prestation)
                .AsNoTracking()
                .Where(x => x.EstUtilise)
                .OrderByDescending(x => x.DateUtilisation)
                .ToListAsync(ct);
        }

        public async Task<BonEnvoi> CreateAsync(BonEnvoi entity, CancellationToken ct = default)
        {
            _db.BonsEnvoi.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<BonEnvoi?> UpdateAsync(int id, BonEnvoi entity, CancellationToken ct = default)
        {
            var existing = await _db.BonsEnvoi.FirstOrDefaultAsync(x => x.IdBonEnvoi == id, ct);
            if (existing == null)
                return null;

            existing.NumeroBon = entity.NumeroBon;
            existing.AffilieId = entity.AffilieId;
            existing.PrestationId = entity.PrestationId;
            existing.DateUtilisation = entity.DateUtilisation;
            existing.EstUtilise = entity.EstUtilise;
            existing.Statut = entity.Statut;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.BonsEnvoi.FirstOrDefaultAsync(x => x.IdBonEnvoi == id, ct);
            if (existing == null)
                return false;

            _db.BonsEnvoi.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> MarquerCommeUtiliseAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.BonsEnvoi.FirstOrDefaultAsync(x => x.IdBonEnvoi == id, ct);
            if (existing == null || existing.EstUtilise)
                return false;

            existing.EstUtilise = true;
            existing.DateUtilisation = DateTime.Now;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        /// <summary>
        /// Vérifie un QR scanné et retourne les informations du bon (option : marquer comme utilisé).
        /// </summary>
        public async Task<BonEnvoiScanResultDto> ScannerAsync(BonEnvoiScanRequestDto request, CancellationToken ct = default)
        {
            var claims = _qrCodeService.TryValidatePayload(request.QrCodePayload);
            if (claims == null)
            {
                return new BonEnvoiScanResultDto
                {
                    Valide = false,
                    Message = "QR code invalide, expiré ou falsifié."
                };
            }

            var bon = await _db.BonsEnvoi
                .Include(b => b.Affilie)
                .Include(b => b.Prestation)
                .FirstOrDefaultAsync(b => b.IdBonEnvoi == claims.IdBonEnvoi && b.NumeroBon == claims.NumeroBon, ct);

            if (bon == null || !bon.Statut)
            {
                return new BonEnvoiScanResultDto
                {
                    Valide = false,
                    Message = "Bon d'envoi introuvable ou désactivé."
                };
            }

            var demandeLiee = await _db.DemandesBonEnvoi
                .Include(d => d.JetonMedical)
                .FirstOrDefaultAsync(d => d.BonEnvoiId == bon.IdBonEnvoi, ct);
            var demandeId = demandeLiee?.IdDemande;
            var jetonLie = bon.JetonMedicalId is > 0
                ? await _db.JetonsMedicaux.FirstOrDefaultAsync(j => j.IdJeton == bon.JetonMedicalId, ct)
                : demandeLiee?.JetonMedical;
            var jetonCode = jetonLie?.CodeJeton;

            if (jetonLie == null)
            {
                return new BonEnvoiScanResultDto
                {
                    Valide = false,
                    Message = "Bon d'envoi non conforme: jeton médical lié introuvable.",
                    DemandeId = demandeId
                };
            }

            var now = DateTime.Now;
            if (!jetonLie.EstValide || jetonLie.EstUtilise ||
                (jetonLie.DateExpiration.HasValue && jetonLie.DateExpiration.Value <= now))
            {
                return new BonEnvoiScanResultDto
                {
                    Valide = false,
                    Message = "Bon d'envoi non conforme: jeton médical lié invalide, expiré ou déjà utilisé.",
                    DemandeId = demandeId,
                    JetonMedicalCode = jetonCode
                };
            }

            if (bon.EstUtilise)
            {
                return new BonEnvoiScanResultDto
                {
                    Valide = false,
                    Message = "Ce bon a déjà été utilisé.",
                    Bon = BonEnvoiDtoMapper.ToReadDto(bon),
                    DemandeId = demandeId,
                    AffilieMatricule = bon.Affilie?.CodeAdhesion,
                    JetonMedicalCode = jetonCode
                };
            }

            if (request.MarquerUtilise && !bon.EstUtilise)
            {
                bon.EstUtilise = true;
                bon.DateUtilisation = DateTime.Now;
                bon.DateModification = DateTime.Now;
                await _db.SaveChangesAsync(ct);
            }

            return new BonEnvoiScanResultDto
            {
                Valide = true,
                Message = request.MarquerUtilise
                    ? "Bon d'envoi valide et marqué comme utilisé."
                    : "Bon d'envoi valide.",
                Bon = BonEnvoiDtoMapper.ToReadDto(bon),
                DemandeId = demandeId,
                AffilieMatricule = bon.Affilie?.CodeAdhesion,
                JetonMedicalCode = jetonCode
            };
        }
    }
}
