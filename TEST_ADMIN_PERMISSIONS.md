# 🧪 TEST DES PERMISSIONS ADMIN ANTECEDENT

## 📋 Instructions de Test

### 1. Connexion en tant qu'Admin
```bash
# Obtenir un token Admin
curl -X POST "http://localhost:5000/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "nomUtilisateur": "admin@prosoc.cd",
    "motDePasse": "admin123",
    "fcmToken": "test",
    "deviceType": "web",
    "deviceModel": "test",
    "osVersion": "test"
  }'
```

### 2. Test des Permissions ANTECEDENT

#### Test 1: Création d'Antécédent
```bash
curl -X POST "http://localhost:5000/api/Antecedent" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer TOKEN_ADMIN" \
  -d '{
    "nom": "Test Antécédent Admin",
    "prenom": "Test",
    "description": "Antécédent créé par l''Admin",
    "dateNaissance": "1990-01-01",
    "affilieId": 1
  }'
# Attendu: 201 Created (si Admin a CREATE_ANTECEDENT)
# Rejet: 403 Forbidden (si permission manquante)
```

#### Test 2: Lecture des Antécédents
```bash
curl -X GET "http://localhost:5000/api/Antecedent" \
  -H "Authorization: Bearer TOKEN_ADMIN"
# Attendu: 200 OK (si Admin a READ_ANTECEDENT)
# Rejet: 403 Forbidden (si permission manquante)
```

#### Test 3: Recherche Avancée
```bash
curl -X POST "http://localhost:5000/api/Antecedent/advanced" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer TOKEN_ADMIN" \
  -d '{
    "nom": "Test",
    "dateNaissanceDebut": "1980-01-01",
    "dateNaissanceFin": "2000-12-31"
  }'
# Attendu: 200 OK (si Admin a READ_ANTECEDENT)
# Rejet: 403 Forbidden (si permission manquante)
```

#### Test 4: Mise à Jour
```bash
curl -X PUT "http://localhost:5000/api/Antecedent/1" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer TOKEN_ADMIN" \
  -d '{
    "nom": "Test Modifié",
    "prenom": "Test Updated",
    "description": "Antécédent modifié par l''Admin"
  }'
# Attendu: 200 OK (si Admin a UPDATE_ANTECEDENT)
# Rejet: 403 Forbidden (si permission manquante)
```

#### Test 5: Suppression
```bash
curl -X DELETE "http://localhost:5000/api/Antecedent/1" \
  -H "Authorization: Bearer TOKEN_ADMIN"
# Attendu: 200 OK (si Admin a DELETE_ANTECEDENT)
# Rejet: 403 Forbidden (si permission manquante)
```

### 3. Vérification des Permissions en Base de Données

```sql
-- Vérifier toutes les permissions du rôle Admin
SELECT 
    p.Categorie,
    COUNT(*) as nombre_permissions,
    GROUP_CONCAT(p.Nom ORDER BY p.Nom SEPARATOR ', ') as liste_permissions
FROM RolePermissions rp
JOIN Roles r ON rp.RoleId = r.IdRole
JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE r.Nom = 'Admin'
GROUP BY p.Categorie
ORDER BY p.Categorie;
```

## 🎯 Critères de Succès

### ✅ Succès Attendu
- **200 OK** sur tous les endpoints ANTECEDENT
- **201 Created** sur POST /api/Antecedent
- **403 Forbidden** uniquement si permissions manquantes
- **Messages clairs** dans les réponses d'erreur

### ❌ Échecs à Détecter
- **401 Unauthorized** : Token invalide ou expiré
- **403 Forbidden** : Permissions ANTECEDENT manquantes
- **404 Not Found** : Endpoint non implémenté
- **500 Internal Server Error** : Erreur de configuration

## 📊 Résultats Attendus

Si tout fonctionne correctement :
- **Admin peut créer** des antécédents (CREATE_ANTECEDENT)
- **Admin peut lire** tous les antécédents (READ_ANTECEDENT)
- **Admin peut modifier** les antécédents (UPDATE_ANTECEDENT)
- **Admin peut supprimer** les antécédents (DELETE_ANTECEDENT)
- **Admin peut faire** des recherches avancées (READ_ANTECEDENT)

---

## 🔍 Diagnostic

Si les tests échouent :
1. **Vérifier le token** : Est-ce bien un token Admin ?
2. **Vérifier la base** : Les permissions sont-elles bien enregistrées ?
3. **Vérifier le controller** : Les attributs `[Authorize]` sont-ils corrects ?
4. **Vérifier le logging** : Y a-t-il des erreurs dans les logs ?

---

*Script de test préparé pour validation complète des permissions Admin ANTECEDENT*
