using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
  public class PenaliteAffilieService : IPenaliteAffilieService
  {
    private const decimal MontantTolerance = 0.01m;

    private readonly ProsocDbContext _db;
    private readonly IParametresMetierProvider _parametresMetierProvider;
    private readonly ILogger<PenaliteAffilieService> _logger;

    public PenaliteAffilieService(
        ProsocDbContext db,
        IParametresMetierProvider parametresMetierProvider,
        ILogger<PenaliteAffilieService> logger)
    {
      _db = db;
      _parametresMetierProvider = parametresMetierProvider;
      _logger = logger;
    }

    public async Task<List<PenaliteAffilie>> AppliquerPenalitesRetardCotisationAsync(
        DateTime date,
        CancellationToken ct = default)
    {
      var options = await _parametresMetierProvider.GetPenaliteAsync(ct);
      if (!options.ApplicationAutomatiqueActivee || !options.RetardCotisationActive)
        return new List<PenaliteAffilie>();

      var frais = await TryResolveFraisPenaliteAsync(options, ct);
      if (frais == null)
        return new List<PenaliteAffilie>();

      var dateReference = date.Date;
      var delaiGrace = Math.Max(0, options.DelaiGraceJours);
      var nouvelles = new List<PenaliteAffilie>();

      var arrieresEligibles = await _db.ArrieresAffilie
          .AsNoTracking()
          .Where(a => a.Statut
                      && a.TypeObligation == TypeCollecte.Cotisation
                      && a.RestAPayer > 0
                      && (a.StatutPaiement == ArrieresAffilieStatuts.EnAttente
                          || a.StatutPaiement == ArrieresAffilieStatuts.EnRetard
                          || a.StatutPaiement == ArrieresAffilieStatuts.PartiellementPaye))
          .ToListAsync(ct);

      foreach (var arriere in arrieresEligibles)
      {
        var joursRetard = (dateReference - arriere.DateEcheance.Date).Days;
        if (joursRetard < delaiGrace)
          continue;

        if (await PenaliteExisteAsync(arriere.IdArrieresAffilie, TypePenalite.RetardCotisation, ct))
          continue;

        var penalite = new PenaliteAffilie
        {
          AffilieId = arriere.AffilieId,
          ArrieresAffilieId = arriere.IdArrieresAffilie,
          FraisId = frais.IdFrais,
          TypePenalite = TypePenalite.RetardCotisation,
          Montant = (decimal)frais.Montant,
          DeviseId = frais.DeviseId,
          JoursRetard = joursRetard,
          Motif = $"Retard cotisation {arriere.Mois:D2}/{arriere.Annee} — {joursRetard} jour(s) après échéance",
          Statut = PenaliteAffilieStatuts.Appliquee,
          DateApplication = dateReference,
          DateCreation = DateTime.Now,
          StatutActif = true
        };

        _db.PenalitesAffilie.Add(penalite);
        nouvelles.Add(penalite);
      }

      if (nouvelles.Count > 0)
        await _db.SaveChangesAsync(ct);

      _logger.LogInformation(
          "Pénalités retard cotisation : {Count} appliquée(s) pour {Date}",
          nouvelles.Count,
          dateReference);

      return nouvelles;
    }

    public async Task<PenaliteAffilie?> ProcessCollecteForPenaliteAsync(
        Collecte collecte,
        CancellationToken ct = default)
    {
      if (!collecte.PenaliteAffilieId.HasValue)
        return null;

      if (collecte.TypeCollecte != TypeCollecte.Frais || !collecte.FraisId.HasValue)
      {
        _logger.LogWarning(
            "Collecte {CollecteId} : PenaliteAffilieId renseigné mais type invalide",
            collecte.IdCollecte);
        return null;
      }

      var penalite = await _db.PenalitesAffilie
          .FirstOrDefaultAsync(p => p.IdPenaliteAffilie == collecte.PenaliteAffilieId.Value, ct);

      if (penalite == null)
        throw new ArgumentException($"Pénalité {collecte.PenaliteAffilieId} introuvable");

      if (penalite.Statut == PenaliteAffilieStatuts.Annulee)
        throw new InvalidOperationException($"La pénalité {penalite.IdPenaliteAffilie} est annulée");

      if (penalite.Statut == PenaliteAffilieStatuts.Payee)
        return penalite;

      if (collecte.AffilieId != penalite.AffilieId)
        throw new ArgumentException("La collecte ne correspond pas à l'affilié de la pénalité");

      if (collecte.FraisId != penalite.FraisId)
        throw new ArgumentException("Le frais de la collecte ne correspond pas à la pénalité");

      if (Math.Abs(collecte.Montant - penalite.Montant) > MontantTolerance)
        throw new ArgumentException(
            $"Montant invalide pour la pénalité. Attendu : {penalite.Montant:F2}, reçu : {collecte.Montant:F2}");

      penalite.Statut = PenaliteAffilieStatuts.Payee;
      penalite.DatePaiement = DateTime.Now;
      penalite.DateModification = DateTime.Now;
      collecte.PenaliteAffilieId = penalite.IdPenaliteAffilie;

      await _db.SaveChangesAsync(ct);

      _logger.LogInformation(
          "Pénalité {PenaliteId} soldée par collecte {CollecteId}",
          penalite.IdPenaliteAffilie,
          collecte.IdCollecte);

      return penalite;
    }

    public async Task<List<PenaliteAffilie>> GetByAffilieAsync(int affilieId, CancellationToken ct = default)
    {
      return await _db.PenalitesAffilie
          .Include(p => p.Frais)
          .Include(p => p.ArrieresAffilie)
          .Include(p => p.Devise)
          .Where(p => p.AffilieId == affilieId && p.StatutActif)
          .OrderByDescending(p => p.DateApplication)
          .ToListAsync(ct);
    }

    public async Task<List<PenaliteAffilie>> GetByArriereAsync(int arrieresAffilieId, CancellationToken ct = default)
    {
      return await _db.PenalitesAffilie
          .Include(p => p.Frais)
          .Include(p => p.Devise)
          .Where(p => p.ArrieresAffilieId == arrieresAffilieId && p.StatutActif)
          .OrderByDescending(p => p.DateApplication)
          .ToListAsync(ct);
    }

    public async Task<PenaliteAffilie> AnnulerPenaliteAsync(
        int id,
        string motifAnnulation,
        CancellationToken ct = default)
    {
      if (string.IsNullOrWhiteSpace(motifAnnulation))
        throw new ArgumentException("Le motif d'annulation est requis");

      var penalite = await _db.PenalitesAffilie
          .FirstOrDefaultAsync(p => p.IdPenaliteAffilie == id, ct)
          ?? throw new ArgumentException($"Pénalité {id} introuvable");

      if (penalite.Statut == PenaliteAffilieStatuts.Payee)
        throw new InvalidOperationException("Impossible d'annuler une pénalité déjà payée");

      penalite.Statut = PenaliteAffilieStatuts.Annulee;
      penalite.MotifAnnulation = motifAnnulation.Trim();
      penalite.DateAnnulation = DateTime.Now;
      penalite.DateModification = DateTime.Now;

      await _db.SaveChangesAsync(ct);
      return penalite;
    }

    public async Task<PenaliteResumeDto> GetResumeAsync(CancellationToken ct = default)
    {
      var penalites = await _db.PenalitesAffilie
          .Where(p => p.StatutActif)
          .ToListAsync(ct);

      return new PenaliteResumeDto
      {
        TotalPenalites = penalites.Count,
        TotalAppliquees = penalites.Count(p => p.Statut == PenaliteAffilieStatuts.Appliquee),
        TotalPayees = penalites.Count(p => p.Statut == PenaliteAffilieStatuts.Payee),
        TotalAnnulees = penalites.Count(p => p.Statut == PenaliteAffilieStatuts.Annulee),
        MontantTotalDu = penalites
            .Where(p => p.Statut == PenaliteAffilieStatuts.Appliquee)
            .Sum(p => p.Montant),
        AffiliesConcernes = penalites
            .Where(p => p.Statut == PenaliteAffilieStatuts.Appliquee)
            .Select(p => p.AffilieId)
            .Distinct()
            .Count()
      };
    }

    private async Task<Frais?> TryResolveFraisPenaliteAsync(PenaliteOptions options, CancellationToken ct)
    {
      var code = string.IsNullOrWhiteSpace(options.FraisPenaliteCode)
          ? FraisCodes.PenaliteRetardCotisation
          : options.FraisPenaliteCode.Trim().ToUpperInvariant();

      var frais = await _db.Frais
          .AsNoTracking()
          .FirstOrDefaultAsync(f =>
              f.Code == code && f.Statut && !f.EstSupprime, ct);

      if (frais == null)
      {
        _logger.LogWarning(
            "Frais pénalité introuvable pour le code {Code} — aucune pénalité appliquée. " +
            "Vérifiez le catalogue Frais (seed / migration).",
            code);
      }

      return frais;
    }

    private Task<bool> PenaliteExisteAsync(
        int arrieresAffilieId,
        TypePenalite typePenalite,
        CancellationToken ct)
    {
      return _db.PenalitesAffilie.AnyAsync(p =>
          p.ArrieresAffilieId == arrieresAffilieId
          && p.TypePenalite == typePenalite
          && p.StatutActif
          && p.Statut != PenaliteAffilieStatuts.Annulee, ct);
    }
  }
}
