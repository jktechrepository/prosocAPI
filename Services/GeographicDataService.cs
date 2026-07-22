using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    public interface IGeographicDataService
    {
        Task EnsureProvincesAndCommunesAsync(CancellationToken cancellationToken = default);
    }

    public class GeographicDataService : IGeographicDataService
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<GeographicDataService> _logger;

        public GeographicDataService(ProsocDbContext db, ILogger<GeographicDataService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task EnsureProvincesAndCommunesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Vérification et mise à jour des provinces et communes...");

            await EnsureProvincesAsync(cancellationToken);
            await EnsureCommunesAsync(cancellationToken);

            _logger.LogInformation("Mise à jour géographique terminée avec succès");
        }

        private async Task EnsureProvincesAsync(CancellationToken cancellationToken)
        {
            var existingProvinces = await _db.Provinces
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var expectedProvinces = GetExpectedProvinces();

            // Ajouter les provinces manquantes
            var missingProvinces = expectedProvinces
                .Where(ep => !existingProvinces.Any(p => p.Nom == ep.Nom))
                .ToList();

            if (missingProvinces.Any())
            {
                _logger.LogInformation("Ajout de {Count} provinces manquantes", missingProvinces.Count);
                _db.Provinces.AddRange(missingProvinces);
                await _db.SaveChangesAsync(cancellationToken);
            }

            // Marquer comme actives les provinces existantes
            var inactiveProvinces = existingProvinces
                .Where(p => !p.Statut && expectedProvinces.Any(ep => ep.Nom == p.Nom))
                .ToList();

            if (inactiveProvinces.Any())
            {
                _logger.LogInformation("Activation de {Count} provinces existantes", inactiveProvinces.Count);
                foreach (var province in inactiveProvinces)
                {
                    province.Statut = true;
                }
                await _db.SaveChangesAsync(cancellationToken);
            }

            // Désactiver les provinces qui ne sont plus dans la liste attendue
            var obsoleteProvinces = existingProvinces
                .Where(p => p.Statut && !expectedProvinces.Any(ep => ep.Nom == p.Nom))
                .ToList();

            if (obsoleteProvinces.Any())
            {
                _logger.LogInformation("Désactivation de {Count} provinces obsolètes", obsoleteProvinces.Count);
                foreach (var province in obsoleteProvinces)
                {
                    province.Statut = false;
                }
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task EnsureCommunesAsync(CancellationToken cancellationToken)
        {
            var kinshasaProvince = await _db.Provinces
                .FirstOrDefaultAsync(p => p.Nom == "Kinshasa" && p.Statut, cancellationToken);

            if (kinshasaProvince == null)
            {
                _logger.LogWarning("Province Kinshasa non trouvée, impossible de créer les communes");
                return;
            }

            var existingCommunes = await _db.Communes
                .Where(c => c.ProvinceId == kinshasaProvince.IdProvince)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var expectedCommunes = GetExpectedKinshasaCommunes(kinshasaProvince.IdProvince);

            // Ajouter les communes manquantes
            var missingCommunes = expectedCommunes
                .Where(ec => !existingCommunes.Any(c => c.Nom == ec.Nom))
                .ToList();

            if (missingCommunes.Any())
            {
                _logger.LogInformation("Ajout de {Count} communes manquantes à Kinshasa", missingCommunes.Count);
                _db.Communes.AddRange(missingCommunes);
                await _db.SaveChangesAsync(cancellationToken);
            }

            // Marquer comme actives les communes existantes
            var inactiveCommunes = existingCommunes
                .Where(c => !c.Statut && expectedCommunes.Any(ec => ec.Nom == c.Nom))
                .ToList();

            if (inactiveCommunes.Any())
            {
                _logger.LogInformation("Activation de {Count} communes existantes", inactiveCommunes.Count);
                foreach (var commune in inactiveCommunes)
                {
                    commune.Statut = true;
                }
                await _db.SaveChangesAsync(cancellationToken);
            }

            // Désactiver les communes qui ne sont plus dans la liste attendue
            var obsoleteCommunes = existingCommunes
                .Where(c => c.Statut && !expectedCommunes.Any(ec => ec.Nom == c.Nom))
                .ToList();

            if (obsoleteCommunes.Any())
            {
                _logger.LogInformation("Désactivation de {Count} communes obsolètes", obsoleteCommunes.Count);
                foreach (var commune in obsoleteCommunes)
                {
                    commune.Statut = false;
                }
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        private List<Province> GetExpectedProvinces()
        {
            return new List<Province>
            {
                new Province { Nom = "Kinshasa", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Bandundu", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Bas-Congo", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Bas-Uele", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Équateur", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Haut-Katanga", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Haut-Lomami", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Haut-Uele", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Ituri", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Kasaï", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Kasaï-Central", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Kasaï-Oriental", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Kongo Central", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Kwango", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Kwilu", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Lomami", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Lualaba", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Maniema", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Mongala", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Nord-Kivu", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Nord-Ubangi", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Sud-Kivu", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Sud-Ubangi", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Tshopo", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Tshuapa", Statut = true, DateCreation = DateTime.Now },
                new Province { Nom = "Tanganyika", Statut = true, DateCreation = DateTime.Now }
            };
        }

        private List<Commune> GetExpectedKinshasaCommunes(int provinceId)
        {
            return new List<Commune>
            {
                new Commune { Nom = "Gombe", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Lemba", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Matete", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Kalamu", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Kasa-Vubu", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Kintambo", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Kimbanseke", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Kinshasa", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Lingwala", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Limete", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Mont-Ngafula", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Ngaba", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Ngaliema", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Nsele", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Pikine", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Selembao", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Bandalungwa", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Barumbu", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Bumbu", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Kinkole", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Kisenso", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Kokolo", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Makala", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now },
                new Commune { Nom = "Masina", ProvinceId = provinceId, Statut = true, DateCreation = DateTime.Now }
            };
        }
    }
}
