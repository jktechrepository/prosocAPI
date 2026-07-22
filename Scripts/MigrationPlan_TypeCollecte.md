# PLAN DE MIGRATION : AJOUT TYPECOLLECTE ET RELATION FRAIS

## 📋 RÉSUMÉ

Ce document décrit la stratégie de migration pour ajouter le champ `TypeCollecte` et la relation `FraisId` au modèle `Collecte`.

## 🎯 OBJECTIFS

1. Ajouter `TypeCollecte` pour différencier frais vs souscriptions
2. Ajouter `FraisId` pour lier les collectes de frais aux frais correspondants
3. Maintenir la rétrocompatibilité
4. Migrer les données existantes proprement

## 📊 ANALYSE D'IMPACT

### 🔍 DONNÉES EXISTANTES À ANALYSER

- **Total collectes** : À déterminer avec le script SQL
- **Collectes avec SouscriptionPrestationId** : Probablement des souscriptions régulières
- **Collectes sans SouscriptionPrestationId** : Probablement des frais ponctuels

### 🎯 STRATÉGIE DE CLASSIFICATION

#### RÈGLES DE DÉTECTION AUTOMATIQUE
```sql
-- Collectes de type SOUSCRIPTION
WHERE SouscriptionPrestationId IS NOT NULL

-- Collectes de type FRAIS (à confirmer manuellement)
WHERE SouscriptionPrestationId IS NULL
```

## 🚀 PLAN D'EXÉCUTION

### PHASE 1 : PRÉPARATION (✅ COMPLÉTÉE)
- [x] Créer énumération `TypeCollecte`
- [x] Créer script d'analyse des données
- [x] Documenter le plan de migration

### PHASE 2 : MODÈLES (À FAIRE)
- [ ] Modifier `Collecte.cs` avec les nouveaux champs
- [ ] Ajouter relation dans `Frais.cs`
- [ ] Créer les DTOs nécessaires

### PHASE 3 : MIGRATION BD (À FAIRE)
- [ ] Créer migration EF Core
- [ ] Exécuter script de migration des données
- [ ] Valider les données migrées

### PHASE 4 : SERVICES (À FAIRE)
- [ ] Mettre à jour `CollecteService`
- [ ] Ajouter validation `TypeCollecte`
- [ ] Gérer les relations Frais/Collecte

### PHASE 5 : API (À FAIRE)
- [ ] Mettre à jour `CollecteController`
- [ ] Ajouter nouveaux endpoints
- [ ] Mettre à jour les DTOs

### PHASE 6 : TESTS (À FAIRE)
- [ ] Tests unitaires
- [ ] Tests d'intégration
- [ ] Tests de performance

## 📋 SCRIPT DE MIGRATION DES DONNÉES

### ÉTAPE 1 : Ajouter les colonnes
```sql
ALTER TABLE Collectes 
ADD COLUMN TypeCollecte INT NULL,
ADD COLUMN FraisId INT NULL;
```

### ÉTAPE 2 : Classifier les collectes existantes
```sql
-- Marquer comme Souscription les collectes avec SouscriptionPrestationId
UPDATE Collectes 
SET TypeCollecte = 2  -- Souscription
WHERE SouscriptionPrestationId IS NOT NULL;

-- Marquer comme Frais les collectes sans SouscriptionPrestationId
UPDATE Collectes 
SET TypeCollecte = 1  -- Frais
WHERE SouscriptionPrestationId IS NULL AND TypeCollecte IS NULL;
```

### ÉTAPE 3 : Créer les frais par défaut (si nécessaire)
```sql
-- Insérer des frais par défaut pour les types de frais courants
INSERT INTO Frais (Libelle, Montant, DeviseId, CreeParId, DateCreation, Statut)
VALUES 
('Frais Adhesion', 5000, 1, 1, NOW(), 1),
('Frais Carte Membre', 2000, 1, 1, NOW(), 1),
('Frais Dossier', 1000, 1, 1, NOW(), 1);
```

### ÉTAPE 4 : Lier les collectes aux frais (basé sur l'analyse)
```sql
-- Exemple : Lier les collectes de 5000 sans souscription au frais d'adhésion
UPDATE Collectes 
SET FraisId = (SELECT IdFrais FROM Frais WHERE Libelle = 'Frais Adhesion' LIMIT 1)
WHERE TypeCollecte = 1 
AND Montant = 5000 
AND SouscriptionPrestationId IS NULL;
```

## ⚠️ POINTS D'ATTENTION

### 🚨 RISQUES CRITIQUES
1. **Perte de données** : Toujours sauvegarder avant migration
2. **Performance** : La migration peut prendre du temps sur gros volume
3. **Incohérence** : Validation manuelle nécessaire après migration

### 🛡️ MESURES DE SÉCURITÉ
1. **Backup complet** avant toute modification
2. **Test sur environnement staging** d'abord
3. **Rollback plan** préparé
4. **Monitoring** pendant la migration

## 📈 MÉTRIQUES DE SUCCÈS

- [ ] 0% de données corrompues
- [ ] Performance maintenue (temps réponse < 200ms)
- [ ] Tous les tests passent
- [ ] Rétrocompatibilité préservée

## 🔄 PLAN DE ROLLBACK

Si la migration échoue :
1. Restaurer le backup
2. Annuler la migration EF Core
3. Revenir à la version précédente du code
4. Analyser les logs d'erreur

## 📞 CONTACTS

- **Développeur principal** : Équipe API
- **DBA** : Équipe Base de Données
- **Métier** : Équipe Produit

---

*Ce document doit être validé par toutes les parties prenantes avant le début de la migration.*
