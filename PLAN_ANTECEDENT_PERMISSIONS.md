# 📋 Plan d'Action : Permissions ANTECEDENT

## 🎯 Objectif
Ajouter les permissions ANTECEDENT au système de gestion des permissions existant, en suivant le même pattern que DEPENDANT et ASSUREUR.

## 📊 Permissions à Ajouter

### Permissions CRUD ANTECEDENT
```
CREATE_ANTECEDENT  - Créer un antécédent
READ_ANTECEDENT   - Voir les antécédents  
UPDATE_ANTECEDENT - Modifier un antécédent
DELETE_ANTECEDENT - Supprimer un antécédent
```

## 🎯 Rôles Cibles

### Rôles avec Accès Complet
- **IT** : Accès complet (CREATE, READ, UPDATE, DELETE)
- **Superviseur** : Accès complet (CREATE, READ, UPDATE, DELETE)
- **Agent (AT)** : Accès limité (CREATE, READ, UPDATE)
- **Agent (AA)** : Accès limité (CREATE, READ, UPDATE)

### Rôles avec Accès Lecture Seule
- **Assureur** : Accès lecture seule (READ uniquement)

## 🛠️ Étapes d'Implémentation

### Phase 1 : SeedData.cs
1. Ajouter les 4 permissions ANTECEDENT dans le tableau `permissions`
2. Ajouter les permissions ANTECEDENT aux rôles cibles dans les sections appropriées
3. Suivre le même pattern que DEPENDANT/ASSUREUR

### Phase 2 : Controllers
1. Créer/vérifier `AntecedentController.cs`
2. Ajouter les vérifications de permissions `HasPermission("CREATE_ANTECEDENT")`
3. Sécuriser tous les endpoints

### Phase 3 : Base de Données
1. Mettre à jour le script SQL de migration
2. Inclure les nouvelles permissions ANTECEDENT
3. Assurer la rétrocompatibilité

## 🔍 Points d'Attention

1. **Cohérence** : Maintenir le pattern ACTION_RESSOURCE
2. **Sécurité** : Appliquer les mêmes règles que DEPENDANT/ASSUREUR
3. **Documentation** : Mettre à jour Swagger et commentaires
4. **Tests** : Valider tous les scénarios

## 📈 Bénéfices Attendus

- ✅ **Système cohérent** : Même pattern pour toutes les entités
- ✅ **Sécurité renforcée** : Contrôle d'accès granulaire
- ✅ **Maintenance facilitée** : Structure uniforme des permissions
- ✅ **Évolutivité** : Préparer l'ajout futur d'autres entités

## ⚠️ Risques et Mitigations

| Risque | Impact | Mitigation |
|---------|--------|------------|
| Incohérence | Permissions mal attribuées | Tests complets |
| Performance | Impact sur les temps de réponse | Indexation optimisée |
| Sécurité | Accès non autorisé | Vérifications systématiques |

## 🚀 Plan de Déploiement

1. **Développement** : Implémenter les permissions ANTECEDENT
2. **Tests** : Valider en environnement de développement
3. **Documentation** : Mettre à jour les endpoints
4. **Déploiement** : Appliquer en production
5. **Formation** : Documenter les nouvelles fonctionnalités

---
*Plan préparé le 26/03/2026 pour validation et implémentation*
