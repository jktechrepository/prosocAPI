# 📋 Instructions de Déploiement - Module Retrait Agent

## 🎯 Objectif
Déployer le nouveau système de retrait agent en production avec validation des périodes, gestion des jetons et intégration avec WalletAgent.

---

## 📦 Composants à Déployer

### 1. Base de Données
- **Script SQL**: `AddRetraitAgentSystem-Production.sql`
- **Nouvelles tables**:
  - `DemandesRetraitAgents` - Gestion des demandes de retrait
  - `JetonsRetraits` - Gestion des jetons de retrait

### 2. Application
- **Nouveaux fichiers**:
  - `Models/Core/DemandeRetraitAgent.cs`
  - `Models/Core/JetonRetrait.cs`
  - `Models/DTOs/Core/RetraitAgentDtos.cs` (mis à jour)
  - `Services/RetraitAgentService.cs`
  - `Services/WalletAgentService.cs`
  - `Services/Repositories/IDemandeRetraitAgentRepository.cs`
  - `Services/Repositories/IWalletAgentRepository.cs`
  - `Controllers/RetraitAgentController.cs`

---

## 🚀 Étapes de Déploiement

### Phase 1: Préparation de la Base de Données

#### 1.1. Sauvegarde de Sécurité
```sql
-- Créer une sauvegarde complète avant modifications
mysqldump -u [utilisateur] -p [base] > backup_pre_retrait_agent_$(date +%Y%m%d_%H%M%S).sql
```

#### 1.2. Application du Script de Migration
```bash
# Se connecter à la base de données MySQL
mysql -u [utilisateur] -p [base]

# Exécuter le script de migration
source /chemin/vers/AddRetraitAgentSystem-Production.sql;
```

#### 1.3. Vérification de la Migration
```sql
-- Vérifier que les tables ont été créées
SHOW TABLES LIKE 'DemandesRetraitAgents';
SHOW TABLES LIKE 'JetonsRetraits';

-- Vérifier les clés étrangères
DESCRIBE DemandesRetraitAgents;
DESCRIBE JetonsRetraits;

-- Vérifier les index
SHOW INDEX FROM DemandesRetraitAgents;
SHOW INDEX FROM JetonsRetraits;
```

### Phase 2: Déploiement de l'Application

#### 2.1. Arrêt de l'Application
```bash
# Arrêter le service
sudo systemctl stop prosoc-api
# OU
docker stop prosoc-api
```

#### 2.2. Mise à Jour des Fichiers
```bash
# Sauvegarder la version actuelle
cp -r /var/www/prosoc-api /var/www/prosoc-api.backup.$(date +%Y%m%d_%H%M%S)

# Déployer les nouveaux fichiers
rsync -av --exclude='appsettings.Production.json' \
  ./bin/Release/net6.0/publish/ \
  /var/www/prosoc-api/

# S'assurer que les permissions sont correctes
chown -R www-data:www-data /var/www/prosoc-api
chmod -R 755 /var/www/prosoc-api
```

#### 2.3. Mise à Jour de la Configuration
```bash
# Vérifier que appsettings.Production.json contient les bonnes configurations
cat /var/www/prosoc-api/appsettings.Production.json | grep -E "(ConnectionStrings|Logging|AllowedHosts)"
```

#### 2.4. Redémarrage de l'Application
```bash
# Démarrer le service
sudo systemctl start prosoc-api
# OU
docker start prosoc-api

# Vérifier le statut
sudo systemctl status prosoc-api
# OU
docker ps | grep prosoc-api
```

---

## 🔍 Tests de Validation Post-Déploiement

### 1. Tests de Base de Données
```sql
-- Vérifier l'existence des nouvelles tables
SELECT COUNT(*) as nb_tables 
FROM information_schema.tables 
WHERE table_schema = '[nom_base]' 
AND table_name IN ('DemandesRetraitAgents', 'JetonsRetraits');

-- Devrait retourner 2
```

### 2. Tests API

#### 2.1. Test de Vérification de Période
```bash
curl -X POST "https://votre-domaine.com/api/retraitagent/verifier-periode" \
  -H "Content-Type: application/json" \
  -d '"2026-03-16"' \
  -H "Authorization: Bearer [token_valide]"
```

#### 2.2. Test de Vérification de Solde
```bash
curl -X POST "https://votre-domaine.com/api/retraitagent/verifier-solde" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer [token_valide]" \
  -d '{"agentId": 1, "montantDemande": 50000}'
```

#### 2.3. Test de Création de Demande
```bash
curl -X POST "https://votre-domaine.com/api/retraitagent" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer [token_valide]" \
  -d '{
    "agentId": 1,
    "montantDemande": 50000,
    "typeRetrait": "PARTIEL",
    "motifRetrait": "Test de déploiement"
  }'
```

#### 2.4. Test de Validation et Génération de Jeton
```bash
curl -X POST "https://votre-domaine.com/api/retraitagent/valider-et-generer-jeton" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer [token_admin]" \
  -d '{
    "idDemande": 1,
    "agentValidationId": 2,
    "statutDemande": "VALIDEE"
  }'
```

---

## 📊 Monitoring Post-Déploiement

### 1. Logs de l'Application
```bash
# Consulter les logs récents
sudo journalctl -u prosoc-api -f --since "5 minutes ago"

# OU pour Docker
docker logs prosoc-api --since 5m
```

### 2. Métriques à Surveiller
- **Nombre de demandes créées** par jour/période
- **Taux de validation** des demandes
- **Temps moyen de traitement** des demandes
- **Erreurs de génération de jetons**
- **Utilisation des jetons** (valides/expirés)

### 3. Alertes à Configurer
- **Échec de connexion à la base de données**
- **Erreurs de validation des périodes**
- **Soldes insuffisants fréquents**
- **Jeton invalide tenté d'être utilisé**

---

## 🔄 Procédure de Rollback

### En Cas de Problème

#### 1. Base de Données
```sql
-- Restaurer la sauvegarde
mysql -u [utilisateur] -p [base] < backup_pre_retrait_agent_[date].sql
```

#### 2. Application
```bash
# Revenir à la version précédente
sudo systemctl stop prosoc-api
rm -rf /var/www/prosoc-api
mv /var/www/prosoc-api.backup.[timestamp] /var/www/prosoc-api
sudo systemctl start prosoc-api
```

---

## 📝 Checklist de Déploiement

### Avant Déploiement
- [ ] Sauvegarde complète de la base de données
- [ ] Test du script SQL en environnement de staging
- [ ] Vérification des dépendances et configurations
- [ ] Arrêt planifié des services critiques

### Pendant Déploiement
- [ ] Application du script SQL sans erreur
- [ ] Déploiement des fichiers binaires
- [ ] Configuration des variables d'environnement
- [ ] Redémarrage des services

### Après Déploiement
- [ ] Tests API fonctionnels
- [ ] Vérification des logs d'erreurs
- [ ] Monitoring des métriques clés
- [ ] Documentation des changements

---

## 🚨 Points d'Attention

1. **Périodes de Retrait**: Le système ne permet les retraits que du 15-20 et à partir du 30 du mois
2. **Validation des Soldes**: Vérifier que les WalletAgent ont des soldes suffisants
3. **Génération de Jetons**: Les jetons sont valides 7 jours et uniques
4. **Mise à Jour des Soldes**: Automatique lors de l'utilisation des jetons
5. **Logs**: Activer un niveau de logging détaillé pour le debugging

---

## 📞 Support et Dépannage

### En Cas d'Erreur
1. **Vérifier les logs**: `/var/log/prosoc-api/` ou `journalctl`
2. **Tester la connexion**: `mysql -u [user] -p [base]`
3. **Vérifier les permissions**: `ls -la /var/www/prosoc-api/`
4. **Redémarrer les services**: `systemctl restart prosoc-api`

### Contact Support
- **Logs à fournir**: Dernières 100 lignes des logs d'application
- **Informations**: Version déployée, heure de déploiement, erreurs rencontrées
- **Tests**: Résultats des tests API post-déploiement

---

## ✅ Validation Finale

Après déploiement, exécuter ces commandes pour valider:

```bash
# 1. Vérifier que l'API répond
curl -f https://votre-domaine.com/api/health || echo "API DOWN"

# 2. Vérifier les nouvelles routes
curl -X GET "https://votre-domaine.com/api/retraitagent" \
  -H "Authorization: Bearer [token]" \
  -w "%{http_code}\n" | grep 200

# 3. Vérifier la base de données
mysql -u [user] -p -e "SELECT COUNT(*) FROM DemandesRetraitAgents;"

echo "✅ Déploiement terminé avec succès!"
```

---

**Note**: Ce document doit être adapté selon votre infrastructure spécifique (Docker, systemd, etc.)
