# 📘 GUIDE D'APPLICATION DE LA MIGRATION DES PERMISSIONS

## 🎯 Objectif

Ajouter **81 permissions granulaires** pour couvrir tous les modules de l'application ProsocAPI.

---

## 📋 Prérequis

✅ Avoir exécuté le script `fix-all-permissions.sql` pour corriger les permissions actuelles  
✅ Avoir sauvegardé la base de données  
✅ Avoir arrêté l'API

---

## 🚀 ÉTAPES D'APPLICATION

### Étape 1 : Créer la Migration

```bash
cd /Users/mac/Documents/ProsocAPI
dotnet ef migrations add AddCompletePermissionsSystem
```

**Note** : La migration a déjà été créée manuellement dans `Migrations/AddCompletePermissionsSystem.cs`

### Étape 2 : Vérifier la Migration

```bash
# Lister toutes les migrations
dotnet ef migrations list

# Vous devriez voir AddCompletePermissionsSystem dans la liste
```

### Étape 3 : Appliquer la Migration

```bash
# Appliquer la migration à la base de données
dotnet ef database update
```

**OU** si vous préférez via l'API au démarrage :

```bash
# L'API appliquera automatiquement les migrations au démarrage
dotnet run
```

### Étape 4 : Vérifier l'Application

```sql
-- Vérifier le nombre de permissions
SELECT COUNT(*) as TotalPermissions FROM Permissions;
-- Résultat attendu : 81

-- Vérifier les permissions du Super-Admin
SELECT COUNT(*) as PermissionsSuperAdmin 
FROM RolePermissions 
WHERE RoleId = 1;
-- Résultat attendu : 81

-- Vérifier les permissions de l'Admin
SELECT COUNT(*) as PermissionsAdmin 
FROM RolePermissions 
WHERE RoleId = 2;
-- Résultat attendu : ~60

-- Voir toutes les nouvelles permissions
SELECT IdPermission, Nom, Categorie, Description 
FROM Permissions 
ORDER BY IdPermission;
```

---

## 📊 CE QUI SERA AJOUTÉ

### Nouvelles Permissions (69 permissions)

| Module | Permissions | IDs |
|--------|-------------|-----|
| Devices | 3 | 10-12 |
| Agents | 9 | 13-21 |
| Affiliés & Adhésions | 12 | 22-33 |
| Produits & Prestations | 15 | 34-48 |
| Financier | 9 | 49-57 |
| Médical | 6 | 58-63 |
| Géographique | 6 | 64-69 |
| Communication | 6 | 70-75 |
| Rapports & Analytics | 6 | 76-81 |

### Attribution Automatique

- **Super-Admin** : 81 permissions (TOUTES)
- **Admin** : ~60 permissions (sans delete et system.admin)
- **Superviseur** : ~30 permissions (lecture + rapports)

---

## ⚠️ POINTS D'ATTENTION

### 1. Sauvegarde Obligatoire

```bash
# Sauvegarder la base de données AVANT la migration
mysqldump -h localhost -P 3306 -u kansa -pkansa@2025 dev-prosoc_db > backup_avant_migration_$(date +%Y%m%d_%H%M%S).sql
```

### 2. La Migration Supprime les Anciennes RolePermissions

La migration exécute :
```sql
DELETE FROM RolePermissions;
```

Puis recrée toutes les associations selon le nouveau système.

### 3. Permissions Existantes (1-9)

Les permissions 1-9 existantes sont **conservées** :
- users.read, users.write, users.delete
- roles.read, roles.write, roles.delete
- permissions.read, permissions.write
- system.admin

---

## 🧪 TESTS APRÈS MIGRATION

### Test 1 : Authentification Super-Admin

```http
POST http://localhost:7116/api/Utilisateur/login
Content-Type: application/json

{
  "emailOuTelephone": "superadmin@prosoc.cd",
  "motDePasse": "Super-Admin"
}
```

**Résultat attendu** : 81 permissions dans la réponse

### Test 2 : Authentification Admin

```http
POST http://localhost:7116/api/Utilisateur/login
Content-Type: application/json

{
  "emailOuTelephone": "admin@prosoc.cd",
  "motDePasse": "Admin"
}
```

**Résultat attendu** : ~60 permissions dans la réponse

### Test 3 : Vérifier les Logs

Chercher dans les logs :
```
✅ Permissions via rôles: 81 permissions trouvées  (pour Super-Admin)
✅ Permissions via rôles: 60 permissions trouvées  (pour Admin)
```

---

## 🔄 ROLLBACK (En cas de problème)

### Option 1 : Annuler la Migration

```bash
# Revenir à la migration précédente
dotnet ef database update <NomMigrationPrecedente>
```

### Option 2 : Restaurer la Sauvegarde

```bash
# Restaurer depuis la sauvegarde
mysql -h localhost -P 3306 -u kansa -pkansa@2025 dev-prosoc_db < backup_avant_migration_YYYYMMDD_HHMMSS.sql
```

---

## 📝 PROCHAINES ÉTAPES APRÈS LA MIGRATION

### 1. Mettre à Jour les Contrôleurs

Ajouter les attributs d'autorisation :

```csharp
[Authorize(Policy = "agents.read")]
[HttpGet]
public async Task<ActionResult<List<AgentReadDto>>> GetAll() { ... }

[Authorize(Policy = "agents.write")]
[HttpPost]
public async Task<ActionResult<AgentReadDto>> Create([FromBody] AgentCreateDto dto) { ... }

[Authorize(Policy = "agents.delete")]
[HttpDelete("{id}")]
public async Task<ActionResult> Delete(int id) { ... }
```

### 2. Créer les Policies dans Program.cs

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("agents.read", policy => 
        policy.RequireClaim("permission", "agents.read"));
    options.AddPolicy("agents.write", policy => 
        policy.RequireClaim("permission", "agents.write"));
    // ... etc pour toutes les permissions
});
```

### 3. Ajouter les Permissions dans les Claims JWT

Modifier `EnhancedAuthService.cs` pour ajouter les permissions dans les claims du token.

---

## ✅ CHECKLIST DE VÉRIFICATION

- [ ] Sauvegarde de la base de données effectuée
- [ ] Migration créée (`dotnet ef migrations add`)
- [ ] Migration appliquée (`dotnet ef database update`)
- [ ] 81 permissions présentes dans la table Permissions
- [ ] Super-Admin a 81 permissions
- [ ] Admin a ~60 permissions
- [ ] Superviseur a ~30 permissions
- [ ] Tests d'authentification réussis
- [ ] Logs confirment le chargement des permissions
- [ ] Documentation mise à jour

---

## 🆘 EN CAS DE PROBLÈME

### Erreur : "Table Permissions doesn't exist"
→ Vérifier que toutes les migrations précédentes sont appliquées

### Erreur : "Duplicate entry for key 'PRIMARY'"
→ Des permissions avec ces IDs existent déjà, exécuter d'abord le nettoyage

### Permissions toujours vides
→ Vérifier les logs, s'assurer que la migration s'est bien exécutée

---

## 📞 SUPPORT

Pour toute question ou problème :
1. Vérifier les logs de l'application
2. Vérifier les données en base avec les requêtes SQL fournies
3. Consulter `PERMISSIONS_COMPLETES_PROPOSITION.md` pour la documentation complète
