# 🚀 PROCÉDURE D'INSTALLATION DES 81 PERMISSIONS

## ⚠️ PROBLÈME RENCONTRÉ

Erreur au démarrage de l'API :
```
MySqlConnector.MySqlException: Duplicate entry '1' for key 'permissions.PRIMARY'
```

**Cause** : L'API essaie d'appliquer des migrations automatiquement alors que les permissions existent déjà.

---

## ✅ SOLUTION : PROCÉDURE EN 3 ÉTAPES

### ÉTAPE 1 : Exécuter le Script SQL Manuellement

**Via MySQL Workbench** :
1. Ouvrir MySQL Workbench
2. Se connecter à `dev-prosoc_db`
3. Ouvrir le fichier `create-81-permissions-complete.sql`
4. **Exécuter le script complet** (⚡ Execute)
5. Vérifier que tu vois "81 permissions créées"

**Via phpMyAdmin** :
1. Accéder à phpMyAdmin
2. Sélectionner `dev-prosoc_db`
3. Onglet "SQL"
4. Copier-coller le contenu de `create-81-permissions-complete.sql`
5. Exécuter

**Via DBeaver/TablePlus** :
1. Se connecter à `dev-prosoc_db`
2. Ouvrir `create-81-permissions-complete.sql`
3. Exécuter le script

---

### ÉTAPE 2 : Vérifier en Base de Données

Exécuter cette requête pour vérifier :

```sql
-- Vérifier le nombre total de permissions
SELECT COUNT(*) as TotalPermissions FROM Permissions;
-- Attendu: 81

-- Vérifier les permissions par rôle
SELECT 
    r.Nom as RoleName,
    COUNT(rp.IdRolePermission) as NombrePermissions
FROM Roles r
LEFT JOIN RolePermissions rp ON r.IdRole = rp.RoleId
WHERE r.IdRole IN (1, 2, 3)
GROUP BY r.IdRole, r.Nom
ORDER BY r.IdRole;
-- Attendu:
-- Super-Admin: 81
-- Admin: 60
-- Superviseur: 31
```

---

### ÉTAPE 3 : Démarrer l'API

Une fois le script exécuté avec succès :

```bash
cd /Users/mac/Documents/ProsocAPI
dotnet run
```

L'API devrait démarrer sans erreur.

---

## 🧪 TESTS APRÈS DÉMARRAGE

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

**Résultat attendu** : 60 permissions dans la réponse

---

## 📋 RÉSUMÉ DES PERMISSIONS

| Rôle | Nombre | Détails |
|------|--------|---------|
| **Super-Admin** | 81 | Toutes les permissions |
| **Admin** | 60 | Toutes sauf delete et system.admin |
| **Superviseur** | 31 | Lecture + rapports uniquement |

---

## ❓ EN CAS DE PROBLÈME

### Problème : "Duplicate entry"
**Solution** : Le script contient déjà `DELETE FROM Permissions;` au début. Assure-toi d'exécuter le script complet.

### Problème : "Foreign key constraint fails"
**Solution** : Le script supprime d'abord les RolePermissions, puis les Permissions. L'ordre est correct.

### Problème : Permissions toujours vides
**Solution** : Vérifier que le script s'est bien exécuté avec la requête de vérification ci-dessus.

---

## ✅ CHECKLIST

- [ ] Exécuter `create-81-permissions-complete.sql` via MySQL Workbench/phpMyAdmin
- [ ] Vérifier que 81 permissions sont créées
- [ ] Vérifier les attributions aux rôles (81, 60, 31)
- [ ] Démarrer l'API avec `dotnet run`
- [ ] Tester l'authentification Super-Admin (81 permissions)
- [ ] Tester l'authentification Admin (60 permissions)
- [ ] Confirmer que tout fonctionne ✅

---

## 🎯 FICHIER À EXÉCUTER

**Chemin** : `/Users/mac/Documents/ProsocAPI/create-81-permissions-complete.sql`

**Important** : Exécuter ce script **AVANT** de démarrer l'API !
