# 📋 INSTRUCTIONS DE DÉPLOIEMENT PRODUCTION
## Système de Jetons Médicaux PROSOC

---

## 🎯 **Étapes de Déploiement**

### **1. Préparation de la Base de Données**

```bash
# 1.1. Sauvegarder la base de données existante
mysqldump -u [username] -p [database_name] > backup_before_jeton_system.sql

# 1.2. Vérifier la version MariaDB (doit être 10.6+)
mysql --version
```

### **2. Exécution du Script SQL**

```bash
# 2.1. Se connecter à MySQL/MariaDB
mysql -u [username] -p [database_name]

# 2.2. Exécuter le script de migration
source /path/to/PRODUCTION-SCRIPT-COMPLET.sql;

# 2.3. Vérifier l'installation
SHOW TABLES LIKE '%Hopital%';
SHOW TABLES LIKE '%Jeton%';
SHOW TABLES LIKE '%Demande%';
```

### **3. Déploiement de l'Application**

```bash
# 3.1. Compiler l'application
dotnet build --configuration Release

# 3.2. Publier l'application
dotnet publish --configuration Release --output ./publish

# 3.3. Copier les fichiers sur le serveur
scp -r ./publish/* user@server:/path/to/application/
```

### **4. Configuration**

```bash
# 4.1. Mettre à jour appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your_server;Database=prosoc_prod;User=your_user;Password=your_password;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}

# 4.2. Redémarrer l'application
sudo systemctl restart prosoc-api
```

---

## 🔍 **Vérifications Post-Déploiement**

### **1. Tests de Base**

```http
# Vérifier que l'API répond
GET https://your-domain.com/api/health

# Tester les nouveaux endpoints
GET https://your-domain.com/api/hopitalpartenaire
GET https://your-domain.com/api/jetonmedical
GET https://your-domain.com/api/demandebonenvoi
```

### **2. Tests Fonctionnels**

```bash
# Créer un hôpital partenaire
curl -X POST "https://your-domain.com/api/hopitalpartenaire" \
  -H "Authorization: Bearer [token]" \
  -H "Content-Type: application/json" \
  -d '{
    "nom": "Hôpital Test",
    "codeAcces": "TEST001",
    "adresse": "Adresse test",
    "estActif": true
  }'

# Créer une demande de bon d'envoi
curl -X POST "https://your-domain.com/api/demandebonenvoi" \
  -H "Authorization: Bearer [token]" \
  -H "Content-Type: application/json" \
  -d '{
    "affilieId": 1,
    "prestationId": 1,
    "typeDemande": "DISTANCE",
    "agentId": 1
  }'
```

---

## 📊 **Monitoring et Maintenance**

### **1. Surveillance des Performances**

```sql
-- Requêtes les plus lentes
SELECT * FROM sys.statements_with_full_tables_scans 
WHERE db = 'prosoc_prod';

-- Utilisation des index
SELECT * FROM sys.schema_index_statistics 
WHERE table_schema = 'prosoc_prod';
```

### **2. Nettoyage Automatique**

```sql
-- Archiver les jetons expirés (à exécuter mensuellement)
UPDATE JetonsMedicaux 
SET EstValide = FALSE, Statut = FALSE 
WHERE DateExpiration < DATE_SUB(NOW(), INTERVAL 30 DAY)
AND EstValide = TRUE;

-- Nettoyer les demandes anciennes (à exécuter annuellement)
UPDATE DemandesBonEnvoi 
SET Statut = FALSE 
WHERE DateCreation < DATE_SUB(NOW(), INTERVAL 2 YEAR)
AND StatutDemande = 'REJETEE';
```

---

## 🚨 **Dépannage**

### **Problèmes Communs**

#### **1. Erreur de connexion**
```bash
# Vérifier les permissions MySQL
SHOW GRANTS FOR CURRENT_USER();

# Vérifier que les tables existent
SHOW TABLES;
```

#### **2. Erreur de migration**
```bash
# Vérifier l'historique des migrations
SELECT * FROM __EFMigrationsHistory;

# Réinitialiser si nécessaire
DELETE FROM __EFMigrationsHistory WHERE MigrationId LIKE '%JetonMedical%';
```

#### **3. Performance lente**
```sql
-- Analyser les requêtes lentes
SHOW PROCESSLIST;

-- Optimiser les tables
OPTIMIZE TABLE JetonsMedicaux;
OPTIMIZE TABLE DemandesBonEnvoi;
```

---

## 📈 **KPIs à Surveiller**

### **1. Volume de Transactions**
- Demandes/jour : ~200
- Jetons générés/jour : ~200
- Validations/hôpital/jour : ~180

### **2. Temps de Réponse**
- API response time : < 200ms
- Database query time : < 50ms
- Jeton validation : < 100ms

### **3. Taux de Succès**
- Validation réussie : > 95%
- Génération réussie : > 98%
- Disponibilité API : > 99.5%

---

## 🔄 **Mises à Jour Futures**

### **1. Sauvegarde Avant Mise à Jour**
```bash
# Sauvegarde complète
mysqldump -u [username] -p [database_name] > backup_v[version].sql
```

### **2. Processus de Mise à Jour**
1. Arrêter l'application
2. Sauvegarder la base
3. Appliquer les nouvelles migrations
4. Redémarrer l'application
5. Vérifier les fonctionnalités

---

## 📞 **Support Technique**

### **Contacts d'Urgence**
- **Développeur Principal** : [Contact]
- **Administrateur Base** : [Contact]
- **Support Production** : [Contact]

### **Documentation**
- **API Documentation** : `/swagger`
- **Database Schema** : Voir script SQL
- **Tests HTTP** : `test-demande-bon-envoi.http`

---

## ✅ **Checklist de Validation**

- [ ] Base de données sauvegardée
- [ ] Script SQL exécuté avec succès
- [ ] Tables créées correctement
- [ ] Index créés et fonctionnels
- [ ] Application compilée et déployée
- [ ] Tests fonctionnels validés
- [ ] Monitoring configuré
- [ ] Documentation mise à jour
- [ ] Équipe formée
- [ ] Utilisateurs notifiés

---

**🎉 Déploiement terminé ! Le système de jetons médicaux est maintenant en production.**
