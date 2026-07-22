using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Data
{
    public class UpdatePermissionsService
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<UpdatePermissionsService> _logger;

        public UpdatePermissionsService(ProsocDbContext db, ILogger<UpdatePermissionsService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<bool> UpdateDependantAndAssureurPermissionsAsync(CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Début de la mise à jour des permissions DEPENDANT et ASSUREUR");

                // Étape 1: Ajouter les permissions manquantes
                await AddMissingPermissionsAsync(ct);

                // Étape 2: Attribuer les permissions aux rôles
                await AssignPermissionsToRolesAsync(ct);

                // Étape 3: Vérifier les permissions attribuées
                await VerifyPermissionsAsync(ct);

                _logger.LogInformation("Mise à jour des permissions terminée avec succès");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour des permissions");
                return false;
            }
        }

        private async Task AddMissingPermissionsAsync(CancellationToken ct)
        {
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
                else
                {
                    _logger.LogInformation("Permission existante: {Permission}", permission.Nom);
                }
            }

            await _db.SaveChangesAsync(ct);
        }

        private async Task AssignPermissionsToRolesAsync(CancellationToken ct)
        {
            // Superviseur exclus : ses permissions ASSUREUR/DEPENDANT sont gérées par
            // SeedData.GetSuperviseurRolePermissionNames() (pas de CREATE/READ/UPDATE_ASSUREUR).
            var targetRoles = new[] { "IT", "Agent (AT)", "Agent (AA)" };
            var targetPermissions = new[] { "CREATE_DEPENDANT", "READ_DEPENDANT", "UPDATE_DEPENDANT", "DELETE_DEPENDANT",
                                            "CREATE_ASSUREUR", "READ_ASSUREUR", "UPDATE_ASSUREUR", "DELETE_ASSUREUR" };

            var roles = await _db.Roles
                .Where(r => targetRoles.Contains(r.Nom))
                .ToListAsync(ct);

            var permissions = await _db.Permissions
                .Where(p => targetPermissions.Contains(p.Nom))
                .ToListAsync(ct);

            foreach (var role in roles)
            {
                foreach (var permission in permissions)
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
        }

        private async Task VerifyPermissionsAsync(CancellationToken ct)
        {
            var rolePermissions = await _db.RolePermissions
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .Where(rp => (rp.Role!.Nom == "IT" ||
                              rp.Role!.Nom == "Agent (AT)" || rp.Role!.Nom == "Agent (AA)") &&
                              (rp.Permission!.Nom.Contains("DEPENDANT") || rp.Permission!.Nom.Contains("ASSUREUR")))
                .ToListAsync(ct);

            _logger.LogInformation("Vérification: {Count} permissions DEPENDANT/ASSUREUR trouvées", rolePermissions.Count);

            foreach (var rp in rolePermissions)
            {
                _logger.LogInformation("Rôle: {Role} - Permission: {Permission}", rp.Role!.Nom, rp.Permission!.Nom);
            }
        }
    }
}
