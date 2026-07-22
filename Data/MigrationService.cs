using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Data
{
    public class MigrationService
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<MigrationService> _logger;

        public MigrationService(ProsocDbContext db, ILogger<MigrationService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<bool> MigrateDependantAssureurPermissionsAsync(CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Début de la migration des permissions DEPENDANT/ASSUREUR");

                // Étape 1: Vérifier les permissions existantes
                var existingPermissions = await _db.Permissions
                    .Where(p => p.Nom.Contains("DEPENDANT") || p.Nom.Contains("ASSUREUR"))
                    .ToListAsync(ct);

                _logger.LogInformation("Permissions existantes: {Count}", existingPermissions.Count);

                // Étape 2: Ajouter les permissions manquantes
                var permissionsToAdd = new[]
                {
                    new Permission { Nom = "CREATE_DEPENDANT", Description = "Créer un dépendant", Categorie = "DEPENDANT", Action = "CREATE", Statut = true, DateCreation = DateTime.Now },
                    new Permission { Nom = "READ_DEPENDANT", Description = "Voir les dépendants", Categorie = "DEPENDANT", Action = "READ", Statut = true, DateCreation = DateTime.Now },
                    new Permission { Nom = "UPDATE_DEPENDANT", Description = "Modifier un dépendant", Categorie = "DEPENDANT", Action = "UPDATE", Statut = true, DateCreation = DateTime.Now },
                    new Permission { Nom = "DELETE_DEPENDANT", Description = "Supprimer un dépendant", Categorie = "DEPENDANT", Action = "DELETE", Statut = true, DateCreation = DateTime.Now },
                    new Permission { Nom = "CREATE_ASSUREUR", Description = "Créer un assureur", Categorie = "ASSUREUR", Action = "CREATE", Statut = true, DateCreation = DateTime.Now },
                    new Permission { Nom = "READ_ASSUREUR", Description = "Voir les assureurs", Categorie = "ASSUREUR", Action = "READ", Statut = true, DateCreation = DateTime.Now },
                    new Permission { Nom = "UPDATE_ASSUREUR", Description = "Modifier un assureur", Categorie = "ASSUREUR", Action = "UPDATE", Statut = true, DateCreation = DateTime.Now },
                    new Permission { Nom = "DELETE_ASSUREUR", Description = "Supprimer un assureur", Categorie = "ASSUREUR", Action = "DELETE", Statut = true, DateCreation = DateTime.Now }
                };

                foreach (var permission in permissionsToAdd)
                {
                    var existing = await _db.Permissions
                        .FirstOrDefaultAsync(p => p.Nom == permission.Nom, ct);

                    if (existing == null)
                    {
                        _db.Permissions.Add(permission);
                        _logger.LogInformation("Permission ajoutée: {Permission}", permission.Nom);
                    }
                }

                await _db.SaveChangesAsync(ct);

                // Étape 3: Attribuer aux rôles cibles
                var targetRoles = await _db.Roles
                    .Where(r => r.Nom == "IT" || r.Nom == "Superviseur" || r.Nom == "Agent (AT)" || r.Nom == "Agent (AA)")
                    .ToListAsync(ct);

                var targetPermissions = await _db.Permissions
                    .Where(p => p.Nom.Contains("DEPENDANT") || p.Nom.Contains("ASSUREUR"))
                    .ToListAsync(ct);

                foreach (var role in targetRoles)
                {
                    foreach (var permission in targetPermissions)
                    {
                        var existing = await _db.RolePermissions
                            .FirstOrDefaultAsync(rp => rp.RoleId == role.IdRole && rp.PermissionId == permission.IdPermission, ct);

                        if (existing == null)
                        {
                            var rolePermission = new RolePermission
                            {
                                RoleId = role.IdRole,
                                PermissionId = permission.IdPermission,
                                DateAttribution = DateTime.Now
                            };

                            _db.RolePermissions.Add(rolePermission);
                            _logger.LogInformation("Permission '{Permission}' attribuée au rôle '{Role}'", permission.Nom, role.Nom);
                        }
                    }
                }

                await _db.SaveChangesAsync(ct);

                // Étape 4: Vérification finale
                var finalCount = await _db.RolePermissions
                    .Include(rp => rp.Role)
                    .Include(rp => rp.Permission)
                    .Where(rp => (rp.Role!.Nom == "IT" || rp.Role!.Nom == "Superviseur" || 
                                  rp.Role!.Nom == "Agent (AT)" || rp.Role!.Nom == "Agent (AA)") &&
                                  (rp.Permission!.Nom.Contains("DEPENDANT") || rp.Permission!.Nom.Contains("ASSUREUR")))
                    .CountAsync(ct);

                _logger.LogInformation("Migration terminée: {Count} permissions DEPENDANT/ASSUREUR trouvées", finalCount);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la migration des permissions");
                return false;
            }
        }
    }
}
