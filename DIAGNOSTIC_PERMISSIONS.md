# 🔍 DIAGNOSTIC - Problème des Permissions Vides

## 📋 Contexte
**Utilisateur** : admin@prosoc.cd  
**Mot de passe** : Admin  
**Problème** : La liste des permissions retourne vide lors de l'authentification

---

## 🔎 ANALYSE DU CODE

### 1. Méthode `GetUserPermissionsAsync` (EnhancedAuthService.cs)

```csharp
private async Task<List<Permission>> GetUserPermissionsAsync(int userId, CancellationToken ct)
{
    // Récupérer les permissions via les rôles de l'utilisateur
    var rolePermissions = await _context.UserRoles
        .Where(ur => ur.UtilisateurId == userId && ur.Statut)
        .SelectMany(ur => _context.RolePermissions
            .Where(rp => rp.RoleId == ur.RoleId)
            .Select(rp => rp.Permission))
        .ToListAsync(ct);

    // Récupérer les permissions directement assignées à l'utilisateur
    var directPermissions = await _context.UserPermissions
        .Where(up => up.UtilisateurId == userId)
        .Include(up => up.Permission)
        .Select(up => up.Permission!)
        .ToListAsync(ct);

    // Combiner et dédoublonner les permissions
    return rolePermissions.Concat(directPermissions)
        .Where(p => p != null && p.Statut)
        .GroupBy(p => p.IdPermission)
        .Select(g => g.First())
        .ToList();
}
```

**⚠️ PROBLÈME POTENTIEL** : La requête LINQ peut ne pas charger correctement la navigation `rp.Permission`

---

## 🐛 CAUSES POSSIBLES

### Cause #1 : Navigation Property non chargée
La requête `SelectMany` ne charge pas explicitement la propriété de navigation `Permission` dans `RolePermissions`.

**Solution** : Ajouter `.Include()`

### Cause #2 : Données seed non appliquées
Les données de seed dans `ProsocDbContext.cs` peuvent ne pas avoir été migrées correctement.

**Vérification** : Exécuter le script SQL de diagnostic

### Cause #3 : Problème de relation EF Core
Les relations entre `UserRoles`, `RolePermissions` et `Permissions` peuvent ne pas être configurées correctement.

---

## 🔧 SOLUTIONS PROPOSÉES

### Solution #1 : Corriger la requête LINQ (RECOMMANDÉ)

```csharp
private async Task<List<Permission>> GetUserPermissionsAsync(int userId, CancellationToken ct)
{
    // Récupérer les permissions via les rôles de l'utilisateur
    var rolePermissions = await _context.UserRoles
        .Where(ur => ur.UtilisateurId == userId && ur.Statut)
        .Include(ur => ur.Role)
        .SelectMany(ur => _context.RolePermissions
            .Where(rp => rp.RoleId == ur.RoleId)
            .Include(rp => rp.Permission)  // ✅ AJOUT CRUCIAL
            .Select(rp => rp.Permission))
        .Where(p => p != null && p.Statut)
        .ToListAsync(ct);

    // Récupérer les permissions directement assignées à l'utilisateur
    var directPermissions = await _context.UserPermissions
        .Where(up => up.UtilisateurId == userId)
        .Include(up => up.Permission)
        .Where(up => up.Permission != null && up.Permission.Statut)
        .Select(up => up.Permission!)
        .ToListAsync(ct);

    // Combiner et dédoublonner les permissions
    return rolePermissions.Concat(directPermissions)
        .GroupBy(p => p.IdPermission)
        .Select(g => g.First())
        .ToList();
}
```

### Solution #2 : Requête alternative plus explicite

```csharp
private async Task<List<Permission>> GetUserPermissionsAsync(int userId, CancellationToken ct)
{
    // Récupérer les IDs des rôles de l'utilisateur
    var userRoleIds = await _context.UserRoles
        .Where(ur => ur.UtilisateurId == userId && ur.Statut)
        .Select(ur => ur.RoleId)
        .ToListAsync(ct);

    // Récupérer les permissions via les rôles
    var rolePermissions = await _context.RolePermissions
        .Where(rp => userRoleIds.Contains(rp.RoleId))
        .Include(rp => rp.Permission)
        .Where(rp => rp.Permission != null && rp.Permission.Statut)
        .Select(rp => rp.Permission!)
        .ToListAsync(ct);

    // Récupérer les permissions directes
    var directPermissions = await _context.UserPermissions
        .Where(up => up.UtilisateurId == userId)
        .Include(up => up.Permission)
        .Where(up => up.Permission != null && up.Permission.Statut)
        .Select(up => up.Permission!)
        .ToListAsync(ct);

    // Combiner et dédoublonner
    return rolePermissions.Concat(directPermissions)
        .GroupBy(p => p.IdPermission)
        .Select(g => g.First())
        .ToList();
}
```

---

## 📊 VÉRIFICATIONS À EFFECTUER

### 1. Vérifier les données en base
Exécuter le script SQL : `test-permissions-debug.sql`

### 2. Vérifier les logs
Activer le logging EF Core pour voir les requêtes SQL générées :

```json
"Logging": {
  "LogLevel": {
    "Microsoft.EntityFrameworkCore.Database.Command": "Information"
  }
}
```

### 3. Tester avec un endpoint de debug

```csharp
[HttpGet("debug/permissions/{userId}")]
public async Task<ActionResult> DebugPermissions(int userId)
{
    var userRoles = await _context.UserRoles
        .Where(ur => ur.UtilisateurId == userId)
        .Include(ur => ur.Role)
        .ToListAsync();

    var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

    var rolePermissions = await _context.RolePermissions
        .Where(rp => roleIds.Contains(rp.RoleId))
        .Include(rp => rp.Permission)
        .ToListAsync();

    return Ok(new {
        UserRoles = userRoles,
        RolePermissions = rolePermissions
    });
}
```

---

## 🎯 PROCHAINES ÉTAPES

1. ✅ Créer le script de diagnostic SQL
2. ⏳ Exécuter le script pour vérifier les données
3. ⏳ Appliquer la Solution #2 (plus robuste)
4. ⏳ Tester l'authentification
5. ⏳ Vérifier que les permissions sont retournées

---

## 📝 NOTES IMPORTANTES

- L'utilisateur `admin@prosoc.cd` devrait avoir l'IdUtilisateur = 2
- Son rôle devrait être Admin (IdRole = 2)
- Le rôle Admin devrait avoir les permissions 1, 2, 4, 5, 7, 10, 11, 12 selon le seed data
