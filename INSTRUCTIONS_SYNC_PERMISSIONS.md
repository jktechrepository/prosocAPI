# Instructions de Synchronisation des Permissions DEPENDANT/ASSUREUR

## 🚨 Problème Résolu
Les nouvelles permissions DEPENDANT et ASSUREUR ne sont pas attribuées automatiquement aux nouveaux agents car la base de données n'est pas synchronisée.

## ✅ Solution Implémentée
Un service de mise à jour des permissions a été créé avec un endpoint API pour synchroniser manuellement les permissions.

## 📋 Étapes à Suivre

### 1. Démarrer l'Application
```bash
cd /Users/mac/Documents/ProsocAPI
dotnet run
```

### 2. Synchroniser les Permissions
Exécuter la requête suivante avec un token d'administrateur :

```bash
curl -X POST "http://localhost:5000/api/UpdatePermissions/sync-dependant-assureur-permissions" \
  -H "Authorization: Bearer VOTRE_TOKEN_ADMIN" \
  -H "Content-Type: application/json"
```

**Réponse attendue :**
```json
{
  "Message": "Permissions DEPENDANT et ASSUREUR synchronisées avec succès",
  "Timestamp": "2025-03-25T11:30:00Z"
}
```

### 3. Vérifier les Permissions
Créer un nouvel agent avec le rôle "Agent (AT)" et vérifier qu'il a maintenant les permissions :
- CREATE_DEPENDANT
- READ_DEPENDANT  
- UPDATE_DEPENDANT
- CREATE_ASSUREUR
- READ_ASSUREUR
- UPDATE_ASSUREUR

### 4. Tester les Endpoints
```bash
# Test avec le nouvel agent
curl -X GET "http://localhost:5000/api/Dependant" \
  -H "Authorization: Bearer TOKEN_NOUVEL_AGENT"

# Attendre : 200 OK (au lieu de 403 Forbidden)
```

## 🔍 Vérification en Base de Données
```sql
SELECT r.Nom as Role, p.Nom as Permission 
FROM RolePermissions rp
JOIN Roles r ON rp.RoleId = r.IdRole  
JOIN Permissions p ON rp.PermissionId = p.IdPermission
WHERE p.Nom LIKE '%DEPENDANT%' OR p.Nom LIKE '%ASSUREUR%'
ORDER BY r.Nom, p.Nom;
```

## 📞 Support
Si vous rencontrez des problèmes, vérifiez les logs de l'application pour les messages de synchronisation.
