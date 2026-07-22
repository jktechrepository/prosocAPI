# 📚 Documentation API Prosoc

## 🎯 **Vue d'ensemble**

L'API Prosoc est une solution complète de gestion mutualiste avec **pagination universelle**, conçue pour offrir des performances optimales et une expérience développeur exceptionnelle.

### **🚀 Caractéristiques principales**
- **Pagination universelle** sur tous les endpoints de liste
- **Performance optimisée** avec pagination côté serveur
- **Swagger UI** complet et interactif
- **Architecture unifiée** avec BaseApiController
- **Gestion d'erreurs** robuste
- **Logging** intégré

---

## � **Guide de Pagination Universelle**

### **🔧 Paramètres de pagination**

Tous les endpoints de pagination acceptent les paramètres suivants :

| Paramètre | Type | Description | Valeur par défaut |
|-----------|-------|-------------|-------------------|
| `pageNumber` | integer | Numéro de la page (commence à 1) | 1 |
| `pageSize` | integer | Nombre d'éléments par page (1-100) | 20 |
| `sortBy` | string | Champ de tri | null |
| `sortDirection` | string | Direction du tri (`asc` ou `desc`) | `asc` |
| `search` | string | Terme de recherche global | null |
| `filters` | string | Filtres avancés (format JSON) | null |

### **�📋 Format de réponse paginée**

```json
{
  "data": [
    {
      "id": 1,
      "nom": "Exemple",
      "dateCreation": "2024-01-01T00:00:00Z"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 150,
    "totalPages": 8,
    "hasNext": true,
    "hasPrevious": false,
    "firstItem": 1,
    "lastItem": 20
  },
  "filters": [],
  "sorting": {
    "sortBy": "nom",
    "sortDirection": "asc"
  }
}
```

### **🔍 Exemples d'utilisation**

#### **Pagination simple**
```http
GET /api/Utilisateurs?pageNumber=2&pageSize=10
```

#### **Avec tri**
```http
GET /api/Utilisateurs?pageNumber=1&pageSize=20&sortBy=nom&sortDirection=desc
```

#### **Avec recherche**
```http
GET /api/Utilisateurs?search=john&pageNumber=1&pageSize=20
```

#### **Avec filtres avancés**
```http
GET /api/Utilisateurs?filters=[{"field":"statut","operator":"eq","value":"ACTIF"}]
```

---

## 📋 Notes de Version - Mise à jour Mars 2026

### ✨ Changements Récents

#### 🔄 **Pagination Universelle - Mars 2026** 
- **Nouveau système** de pagination universelle sur tous les contrôleurs
- **Architecture unifiée** avec BaseApiController
- **Performance optimisée** avec pagination côté serveur (IQueryable)
- **Endpoints paginés** : 43 endpoints créés/modifiés
- **Contrôleurs transformés** : 31 contrôleurs avec pagination
- **Swagger amélioré** : Routes uniques et documentation complète
- **Filtres avancés** : Support JSON pour filtres complexes
- **Tri personnalisé** : sortBy et sortDirection sur tous les endpoints
- **Métadonnées complètes** : totalPages, hasNext, hasPrevious, etc.

#### 🔄 Module Retrait Agent - Mars 2026
- **Nouveau système** de retrait pour les agents avec validation périodique
- **Périodes autorisées** : 15-20 et 30+ du mois uniquement
- **Génération de jetons** uniques (format "JRT" + 8 caractères)
- **Validation automatique** des soldes WalletAgent
- **Workflow complet** : Demande → Validation → Jeton → Utilisation
- **Nouveaux endpoints** : `/api/retraitagent/*` (15+ endpoints)
- **Nouvelles tables** : `DemandesRetraitAgents` et `JetonsRetraits`

---

## 🛣️ **Routes de l'API - Pagination Universelle**

### **� Catégories d'endpoints avec pagination**

#### **🔐 Authentification**
- `POST /api/Auth/login` - Connexion utilisateur
- `POST /api/Auth/register` - Inscription utilisateur
- `POST /api/Auth/refresh` - Rafraîchissement token

#### **👥 Gestion des utilisateurs**
- `GET /api/Utilisateurs` - **Liste paginée** des utilisateurs
- `GET /api/Utilisateurs/{id}` - Détails utilisateur
- `POST /api/Utilisateurs` - Création utilisateur
- `PUT /api/Utilisateurs/{id}` - Mise à jour utilisateur
- `DELETE /api/Utilisateurs/{id}` - Suppression utilisateur

#### **🏥 Gestion des affiliés**
- `GET /api/Affilies` - **Liste paginée** des affiliés
- `GET /api/Affilies/{id}` - Détails affilié
- `GET /api/Affilies/by-agent/{agentId}` - Affiliés par agent
- `GET /api/Affilies/by-agent/{agentId}/paginated` - **Affiliés par agent (paginé)**

#### **💰 Gestion des collectes**
- `GET /api/Collectes` - **Liste paginée** des collectes
- `GET /api/Collectes/{id}` - Détails collecte
- `GET /api/Collectes/by-affilie/{affilieId}/simple` - Collectes par affilié
- `GET /api/Collectes/by-affilie/{affilieId}/paginated` - **Collectes par affilié (paginé)**
- `GET /api/Collectes/by-agent/{agentId}` - Collectes par agent
- `GET /api/Collectes/by-devise/{deviseId}` - Collectes par devise

#### **🎫 Gestion des prestations**
- `GET /api/Prestations` - **Liste paginée** des prestations
- `GET /api/Prestations/{id}` - Détails prestation
- `GET /api/Prestations/by-produit-mutuel/{produitMutuelId}` - Prestations par produit mutuel
- `GET /api/Prestations/by-produit-mutuel/{produitMutuelId}/paginated` - **Prestations par produit mutuel (paginé)**
- `GET /api/Prestations/by-produit-assureur/{produitAssureurId}` - Prestations par produit assureur

#### **👨‍⚕️ Gestion des agents**
- `GET /api/Agents` - **Liste paginée** des agents
- `GET /api/Agents/{id}` - Détails agent
- `GET /api/Agents/by-superviseur/{superviseurId}` - Agents par superviseur

#### **💳 Gestion des wallets**
- `GET /api/WalletAgents` - **Liste paginée** des wallets agents
- `GET /api/WalletAgents/{id}` - Détails wallet agent
- `GET /api/WalletAgents/by-agent/{agentId}` - Wallet par agent
- `GET /api/WalletAgents/by-agent/{agentId}/paginated` - **Wallet par agent (paginé)**
- `GET /api/WalletMouvement` - **Liste paginée** de tous les mouvements wallet
- `GET /api/WalletMouvement/by-agent/{agentId}` - Mouvements wallet par agent (liste complète, rétrocompatibilité)
- `GET /api/WalletMouvement/by-agent/{agentId}/paginated` - **Mouvements wallet par agent (paginé)**

Exemple — mouvements par agent (paginé) :

```http
GET /api/WalletMouvement/by-agent/3/paginated?pageNumber=1&pageSize=20&sortBy=DateOperation&sortDirection=desc
Authorization: Bearer {token}
```

Paramètres query : `pageNumber` (défaut 1), `pageSize` (défaut 20), `sortBy`, `sortDirection`, `search`.

Chaque élément de `WalletMouvementReadDto` inclut : `deviseId`, `deviseCode`, `deviseNom`, `deviseSymbole` (aligné sur `WalletAgentReadDto`).

#### **💳 Wallet virtuel agent**
- `GET /api/WalletVirtuelAgent` - **Liste paginée** des wallets virtuels
- `GET /api/WalletVirtuelAgent/by-agent/{agentId}` - Wallet virtuel par agent
- `GET /api/WalletVirtuelAgent/solde/{agentId}` - Solde virtuel courant
- `GET /api/WalletVirtuelAgent/by-agent/{agentId}/mouvements` - Historique mouvements (liste complète)
- `GET /api/WalletVirtuelAgent/by-agent/{agentId}/mouvements/paginated` - **Historique mouvements (paginé)**

Sources de mouvements enregistrées : `AJOUT_SOLDE`, `AJUSTEMENT_SOLDE`, `COLLECTE_COMPTE_VIRTUEL`, `CREATION`.

Exemple — mouvements wallet virtuel par agent (paginé) :

```http
GET /api/WalletVirtuelAgent/by-agent/3/mouvements/paginated?pageNumber=1&pageSize=20&sortBy=DateOperation&sortDirection=desc
Authorization: Bearer {token}
```

#### **🏥 Hôpitaux partenaires**
- `GET /api/HopitalPartenaires` - **Liste paginée** des hôpitaux partenaires
- `GET /api/HopitalPartenaires/{id}` - Détails hôpital partenaire

#### **🎫 Jetons médicaux**
- `GET /api/JetonMedicals` - **Liste paginée** des jetons médicaux
- `GET /api/JetonMedicals/{id}` - Détails jeton médical
- `GET /api/JetonMedicals/by-affilie/{affilieId}` - Jetons par affilié

#### **📄 Demandes de bon d'envoi**
- `GET /api/DemandeBonEnvois` - **Liste paginée** des demandes
- `GET /api/DemandeBonEnvois/{id}` - Détails demande
- `GET /api/DemandeBonEnvois/by-affilie/{affilieId}` - Demandes par affilié
- `GET /api/DemandeBonEnvois/by-statut/{statut}/simple` - Demandes par statut
- `GET /api/DemandeBonEnvois/by-statut/{statut}/paginated` - **Demandes par statut (paginé)**

#### **🔄 Demandes de retrait**
- `GET /api/RetraitAgents` - **Liste paginée** des demandes de retrait
- `GET /api/RetraitAgents/{id}` - Détails demande de retrait
- `GET /api/RetraitAgents/by-agent/{agentId}` - Demandes par agent
- `GET /api/RetraitAgents/by-statut/{statut}` - Demandes par statut

#### **🎯 Autres contrôleurs avec pagination**
- `GET /api/Adhesions` - **Liste paginée** des adhésions
- `GET /api/Dependants` - **Liste paginée** des dépendants
- `GET /api/Superviseurs` - **Liste paginée** des superviseurs
- `GET /api/ProduitsMutuels` - **Liste paginée** des produits mutuels
- `GET /api/ProduitsAssureurs` - **Liste paginée** des produits assureurs
- `GET /api/Devises` - **Liste paginée** des devises
- `GET /api/Communes` - **Liste paginée** des communes
- `GET /api/Provinces` - **Liste paginée** des provinces
- `GET /api/CategoriesAdhesions` - **Liste paginée** des catégories d'adhésions
- `GET /api/TypesAdhesions` - **Liste paginée** des types d'adhésions
- `GET /api/Assureurs` - **Liste paginée** des assureurs
- `GET /api/CategoriesAgents` - **Liste paginée** des catégories d'agents
- `GET /api/Roles` - **Liste paginée** des rôles
- `GET /api/Permissions` - **Liste paginée** des permissions
- `GET /api/Antecedents` - **Liste paginée** des antécédents
- `GET /api/BonsEnvoi` - **Liste paginée** des bons d'envoi
- `GET /api/SouscriptionsPrestations` - **Liste paginée** des souscriptions prestations
- `GET /api/TargetAgent` - **Liste paginée** des objectifs par rôle applicatif
- `GET /api/TargetAgent/by-role/{roleNom}` - Objectifs d'un rôle (ex. `Agent%20(AT)`)
- `POST /api/TargetAgent` - Créer un objectif par rôle (body : `roleNom`, `libelleTarget`, `periodicite`, `statut`)
- `GET /api/WalletMouvement` - **Liste paginée** des mouvements wallets (voir aussi `by-agent/{agentId}/paginated`)
- `GET /api/WalletsVirtuelsAgents` - **Liste paginée** des wallets virtuels
- `GET /api/ZonesSociales` - **Liste paginée** des zones sociales

---

## �📊 **Dashboard Affilié - Mars 2026**
---

## 🔐 **Authentification**

### **🔑 JWT Token**
L'API utilise l'authentification JWT Bearer.

#### **En-tête requis**
```http
Authorization: Bearer <votre_token_jwt>
```

#### **Durée de vie des tokens**
- **Access Token** : 1 heure
- **Refresh Token** : 7 jours

---

## 📊 **Codes d'erreur**

### **🔴 Erreurs client (4xx)**
| Code | Description | Solution |
|------|-------------|-----------|
| 400 | Bad Request | Paramètres invalides |
| 401 | Unauthorized | Token manquant ou invalide |
| 403 | Forbidden | Permissions insuffisantes |
| 404 | Not Found | Ressource introuvable |
| 422 | Unprocessable Entity | Validation échouée |

### **🔴 Erreurs serveur (5xx)**
| Code | Description | Solution |
|------|-------------|-----------|
| 500 | Internal Server Error | Contacter l'admin |
| 503 | Service Unavailable | Service temporairement indisponible |

---

## 🚀 **Déploiement**

### **📋 Prérequis**
- .NET 6.0 Runtime
- MySQL 8.0+
- Redis (optionnel, pour le cache)

### **🔧 Configuration**
```bash
# Installation des dépendances
dotnet restore

# Compilation
dotnet build --configuration Release

# Publication
dotnet publish --configuration Release --output ./publish
```

### **🌍 Variables d'environnement**
```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultServer=votre_connection_mysql
JWT__Secret=votre_secret_jwt
JWT__Issuer=votre_domaine
JWT__Audience=votre_audience
```

---

## 📈 **Performance**

### **⚡ Optimisations implémentées**
- **Pagination côté serveur** avec IQueryable
- **Indexation** des colonnes fréquemment filtrées
- **Lazy Loading** pour les relations
- **Cache Redis** pour les données fréquemment accédées
- **Compression Gzip** des réponses

### **📊 Métriques recommandées**
- **Temps de réponse** : < 200ms (95th percentile)
- **Débit** : > 1000 requêtes/seconde
- **Disponibilité** : > 99.9%

---

## 🧪 **Tests**

### **🔬 Tests unitaires**
```bash
# Exécution des tests unitaires
dotnet test ./Prosoc.Tests.Unit

# Avec couverture de code
dotnet test ./Prosoc.Tests.Unit --collect:"XPlat Code Coverage"
```

### **🔬 Tests d'intégration**
```bash
# Exécution des tests d'intégration
dotnet test ./Prosoc.Tests.Integration
```

---

## 📞 **Support**

### **🆘 En cas de problème**
1. **Vérifier les logs** de l'application
2. **Consulter Swagger UI** pour valider les requêtes
3. **Vérifier le statut** des services dépendants
4. **Contacter l'équipe** de support technique

### **📧 Contact support**
- **Email** : support@prosoc.cd
- **Documentation** : https://docs.prosoc.cd
- **Status Page** : https://status.prosoc.cd

---

## 📝 **Notes de version**

### **🆕 Version 2.0.0**
- ✅ **Pagination universelle** sur tous les endpoints
- ✅ **Swagger UI** amélioré
- ✅ **Performance** optimisée
- ✅ **Architecture** unifiée
- ✅ **Gestion d'erreurs** robuste

### **📜 Historique**
- **v1.0.0** : Version initiale
- **v1.5.0** : Ajout des dashboards
- **v1.8.0** : Optimisations de performance
- **v2.0.0** : Pagination universelle complète

---

## 🎯 **Bonnes pratiques**

### **✅ Recommandations**
1. **Utiliser la pagination** pour les grandes collections
2. **Implémenter le retry** pour les appels réseau
3. **Valider les entrées** côté client
4. **Utiliser les filtres** pour réduire la charge
5. **Mettre en cache** les données statiques

### **❌ À éviter**
1. **Désactiver la pagination** (risque de timeout)
2. **Ignorer les codes d'erreur**
3. **Envoyer des données sensibles** en clair
4. **Surcharger le serveur** avec des requêtes massives

---

## 🏆 **Conclusion**

L'API Prosoc offre une solution complète, performante et évolutive pour la gestion mutualiste. Avec sa **pagination universelle** et son architecture moderne, elle constitue une base solide pour le développement d'applications robustes.

### **🌟 Points forts de la version 2.0.0**
- **43 endpoints** avec pagination intégrée
- **31 contrôleurs** transformés
- **Performance** côté serveur optimisée
- **Swagger** sans conflits de routes
- **Documentation** complète et interactive

**Pour plus d'informations, consultez le Swagger UI :** `https://votre-domaine/swagger`

---

*📅 Dernière mise à jour : Mars 2026*  
*👨‍💻 Auteur : Équipe de développement Prosoc*  
*📄 Version : 2.0.0*

#### 🔄 Modèle Utilisateur - Simplification
- **Suppression** des champs `PrenomUtilisateur` et `PostNomUtilisateur`
- **Conservation** du champ `NomUtilisateur` comme identifiant principal
- **Ajout** des champs `EmailUtilisateur` et `PhoneUtilisateur` (nullable, unique)
- **Modification** du champ `NomComplet` : utilise maintenant `NomUtilisateur` comme source unique

#### 🔄 Modèle Affilie - Amélioration
- **Ajout** du champ `NomComplet` (required, varchar(200))
- **Génération automatique** du `NomComplet` : `Nom + " " + Postnom + " " + Prenom`
- **Logique implémentée** dans `AffilieService.UpdateAsync` et `AdhesionService.CreateWithAffilieAsync`

#### 📸 Nouveaux Champs PhotoUrl - Mars 2026
- **Ajout** du champ `PhotoUrl` au modèle `Agent` (VARCHAR(500), nullable)
- **Ajout** du champ `PhotoUrl` au modèle `Affilie` (VARCHAR(500), nullable)
- **Mise à jour** des DTOs, services et contrôleurs pour gérer les URLs de photos
- **Scripts SQL** de production générés : `AddPhotoUrlToAgent-Production.sql` et `AddPhotoUrlToAffilie-Production.sql`

#### 🔐 Authentification
- **Consolidation** des endpoints sous `/api/Utilisateur/login`
- **Suppression** des contrôleurs legacy : `Auth`, `AuthTest`, `EnhancedAuth`
- **Réponse unifiée** pour `GET /api/Utilisateur/{id}` et `POST /api/Utilisateur/login`

#### 📊 Base de Données
- **Migration** générée : `20260309064518_RemovePrenomPostNomFromUtilisateur.cs`
- **Suppression** des colonnes `PostNomUtilisateur` et `PrenomUtilisateur` de la table `Utilisateurs`
- **Ajout** des colonnes `EmailUtilisateur` et `PhoneUtilisateur` avec contraintes uniques
- **Nouvelles migrations** : `AddPhotoUrlToAgent`, `AddPhotoUrlToAffilie`, `AddRetraitAgentSystem`

---

## Table des matières
1. [Généralités](#généralités)
2. [Authentification](#authentification)
3. [Agents](#agents)
4. [Adhésions](#adhésions)
5. [Affiliés](#affiliés)
6. [📸 Gestion des Photos de Profil](#-gestion-des-photos-de-profil)
7. [💰 Module Retrait Agent](#-module-retrait-agent)
8. [📊 Dashboard Affilié](#-dashboard-affilié)
9. [Zones Sociales](#zones-sociales)
10. [Communes](#communes)
11. [Provinces](#provinces)
12. [Collectes](#collectes)
13. [Devises](#devises)
14. [Prestations](#prestations)
15. [Catégories](#catégories)
16. [Utilisateurs et Rôles](#utilisateurs-et-rôles)
17. [Exemples d'intégration](#exemples-dintégration)
   - [Vue.js](#vuejs)
   - [Flutter](#flutter)

---

## Généralités

### Base URL
- **Production**: `https://dev-prosoc.asdc-rdc.org`
- **Local**: `https://localhost:7116`

### Format des réponses
Toutes les réponses sont au format JSON.

### En-têtes requis
```http
Content-Type: application/json
Authorization: Bearer {token_jwt}
```

### Gestion des erreurs
- **200**: Succès
- **201**: Créé avec succès
- **400**: Requête invalide
- **401**: Non authentifié
- **403**: Non autorisé
- **404**: Ressource non trouvée
- **429**: Trop de requêtes (rate limiting)
- **500**: Erreur serveur interne

---

## Authentification

### POST /api/utilisateur/login
Permet d'obtenir un token JWT pour les requêtes authentifiées.

#### Corps de la requête
```json
{
  "nomUtilisateur": "admin@prosoc.cd",
  "motDePasse": "Admin"
}
```

#### Réponse réussie
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAtUtc": "2026-03-03T20:00:00Z",
  "utilisateur": {
    "idUtilisateur": 1,
    "referenceUtilisateur": "",
    "nomComplet": "admin",
    "nomUtilisateur": "admin",
    "email": "admin@prosoc.cd",
    "telephone": "+243999999999",
    "photoUrl": null,
    "genre": null,
    "statut": true,
    "dateCreation": "2026-03-03T20:56:51.622409",
    "isConnecte": false,
    "doitChangerMotDePasse": false,
    "agentId": 1,
    "affilieId": null
  }
}
```

#### Notes importantes
- L'authentification peut se faire par `nomUtilisateur`, `EmailUtilisateur` ou `PhoneUtilisateur`
- Les anciens endpoints `/api/auth/login`, `/api/authtest/*` et `/api/enhancedauth/*` ont été supprimés
- Le champ `NomComplet` dans la réponse utilise maintenant `NomUtilisateur` comme source unique

#### Erreurs possibles
- **401**: Identifiants invalides
- **429**: Trop de tentatives de connexion

---

## Agents

### GET /api/agent
Récupère la liste de tous les agents.

#### En-têtes requis
```http
Authorization: Bearer {token_jwt}
```

#### Réponse
```json
[
  {
    "id": 1,
    "nomComplet": "Jean Dupont",
    "matricule": "MAT001",
    "phone": "+243812345678",
    "emailAgent": "jean.dupont@prosoc.cd",
    "fonction": "Agent de terrain",
    "roleAgent": "Agent",
    "photoUrl": "https://example.com/photos/jean-dupont.jpg",
    "dateCreation": "2026-03-03T10:00:00",
    "dateModification": null,
    "statut": true,
    "zoneSocialeId": 1,
    "zoneSocialeNom": "Kinshasa-Ville"
  }
]
```

### GET /api/agent/{id}
Récupère un agent spécifique par son ID.

#### Paramètres
- `id` (int): ID de l'agent

#### Réponse
```json
{
  "id": 1,
  "nomComplet": "Jean Dupont",
  "matricule": "MAT001",
  "phone": "+243812345678",
  "emailAgent": "jean.dupont@prosoc.cd",
  "fonction": "Agent de terrain",
  "roleAgent": "Agent",
  "photoUrl": "https://example.com/photos/jean-dupont.jpg",
  "dateCreation": "2026-03-03T10:00:00",
  "dateModification": null,
  "statut": true,
  "zoneSocialeId": 1,
  "zoneSocialeNom": "Kinshasa-Ville"
}
```

### POST /api/agent
Crée un nouvel agent.

#### Corps de la requête
```json
{
  "nomComplet": "Marie Curie",
  "matricule": "MAT002",
  "phone": "+243812345679",
  "emailAgent": "marie.curie@prosoc.cd",
  "fonction": "Agent de terrain",
  "roleAgent": "Agent",
  "photoUrl": "https://example.com/photos/marie-curie.jpg",
  "zoneSocialeId": 1,
  "categorieAgentId": 1,
  "statut": true
}
```

#### Réponse
```json
{
  "id": 2,
  "nomComplet": "Marie Curie",
  "matricule": "MAT002",
  "phone": "+243812345679",
  "emailAgent": "marie.curie@prosoc.cd",
  "fonction": "Agent de terrain",
  "roleAgent": "Agent",
  "photoUrl": "https://example.com/photos/marie-curie.jpg",
  "dateCreation": "2026-03-03T11:00:00",
  "dateModification": null,
  "statut": true,
  "zoneSocialeId": 1,
  "zoneSocialeNom": "Kinshasa-Ville"
}
```

### PUT /api/agent/{id}
Met à jour un agent existant.

#### Corps de la requête
```json
{
  "nomComplet": "Jean Dupont Senior",
  "matricule": "MAT001-UPD",
  "phone": "+243812345678",
  "emailAgent": "jean.dupont@prosoc.cd",
  "fonction": "Agent senior",
  "roleAgent": "Superviseur",
  "photoUrl": "https://example.com/photos/jean-dupont-updated.jpg",
  "zoneSocialeId": 2,
  "categorieAgentId": 1,
  "statut": true
}
```

### DELETE /api/agent/{id}
Supprime un agent.

#### Paramètres
- `id` (int): ID de l'agent à supprimer

#### Réponse
```json
{
  "message": "Agent supprimé avec succès"
}
```

---

## Adhésions

### GET /api/adhesion
Récupère la liste de toutes les adhésions.

#### Réponse
```json
[
  {
    "idAdhesion": 1,
    "statutDossier": "Actif",
    "agentId": 1,
    "affilieId": 1,
    "typeAdhesionId": 1,
    "dateCreation": "2026-03-03T10:00:00",
    "dateModification": null
  }
]
```

### POST /api/adhesion/with-affilie
Crée une nouvelle adhésion avec un affilié (endpoint complexe).

#### Corps de la requête
```json
{
  "affilie": {
    "nom": "Pierre Martin",
    "prenom": "Jean",
    "dateNaissance": "1990-01-01T00:00:00",
    "telephone": "+243812345677"
  },
  "adhesion": {
    "statutDossier": "En cours",
    "agentId": 1,
    "typeAdhesionId": 1
  },
  "souscriptions": [
    {
      "prestationId": 1,
      "montant": 100.00
    }
  ],
  "collecte": {
    "referencePaiement": "REF001",
    "modePaiement": "Mobile Money",
    "montantRecu": 100.00,
    "montantAttendu": 100.00,
    "dateCollecte": "2026-03-03T10:00:00"
  }
}
```

---

## Affiliés

### GET /api/affilie
Récupère la liste de tous les affiliés.

#### Réponse
```json
[
  {
    "idAffilie": 1,
    "codeAdhesion": "ADH001",
    "nom": "Martin",
    "postnom": "Pierre",
    "prenom": "Jean",
    "nomComplet": "Martin Pierre Jean",
    "dateNaissance": "1990-01-01T00:00:00",
    "telephone": "+243812345677",
    "provinceResidence": "Kinshasa",
    "communeResidence": "Lemba",
    "quartierResidence": "Salongo",
    "avenueResidence": "By Pass",
    "numeroResidence": "123",
    "communeActivite": "Lemba",
    "quartierActivite": "Salongo",
    "avenueActivite": "By Pass",
    "numeroActivite": "456",
    "photoUrl": "https://example.com/photos/martin-pierre-jean.jpg",
    "dateCreation": "2026-03-03T10:00:00",
    "dateModification": null,
    "statut": true
  }
]
```

### POST /api/affilie
Crée un nouvel affilié.

#### Corps de la requête
```json
{
  "codeAdhesion": "ADH002",
  "nom": "Martin",
  "postnom": "Pierre",
  "prenom": "Jean",
  "dateNaissance": "1990-01-01T00:00:00",
  "telephone": "+243812345677",
  "provinceResidence": "Kinshasa",
  "communeResidence": "Lemba",
  "quartierResidence": "Salongo",
  "avenueResidence": "By Pass",
  "numeroResidence": "123",
  "communeActivite": "Lemba",
  "quartierActivite": "Salongo",
  "avenueActivite": "By Pass",
  "numeroActivite": "456",
  "photoUrl": "https://example.com/photos/jean-martin.jpg",
  "statut": true
}
```

**Note**: Le champ `nomComplet` est généré automatiquement à partir des champs `nom`, `postnom` et `prenom` selon la format : `nom + " " + postnom + " " + prenom`.

### PUT /api/affilie/{id}
Met à jour un affilié existant.

#### Corps de la requête
```json
{
  "codeAdhesion": "ADH001",
  "nom": "Martin",
  "postnom": "Pierre",
  "prenom": "Jean",
  "dateNaissance": "1990-01-01T00:00:00",
  "telephone": "+243812345677",
  "provinceResidence": "Kinshasa",
  "communeResidence": "Lemba",
  "quartierResidence": "Salongo",
  "avenueResidence": "By Pass",
  "numeroResidence": "123",
  "communeActivite": "Lemba",
  "quartierActivite": "Salongo",
  "avenueActivite": "By Pass",
  "numeroActivite": "456",
  "photoUrl": "https://example.com/photos/martin-pierre-updated.jpg",
  "statut": true
}
```

**Note**: Le champ `nomComplet` sera automatiquement mis à jour lors de la modification.

### DELETE /api/affilie/{id}
Supprime un affilié.

---

## 📸 Gestion des Photos de Profil

### Vue d'ensemble
Les modèles `Agent` et `Affilie` disposent maintenant d'un champ `PhotoUrl` pour stocker les URLs des photos de profil.

### Caractéristiques du champ PhotoUrl
- **Type** : `string?` (nullable, optionnel)
- **Longueur maximale** : 500 caractères
- **Format** : URL complète (http/https) vers l'image
- **Encodage** : UTF-8 (utf8mb4)

### Endpoints concernés

#### Agents
- `GET /api/agent` - Retourne `photoUrl` dans la liste
- `GET /api/agent/{id}` - Retourne `photoUrl` dans les détails
- `POST /api/agent` - Accepte `photoUrl` en création
- `PUT /api/agent/{id}` - Accepte `photoUrl` en mise à jour

#### Affiliés
- `GET /api/affilie` - Retourne `photoUrl` dans la liste
- `GET /api/affilie/{id}` - Retourne `photoUrl` dans les détails
- `POST /api/affilie` - Accepte `photoUrl` en création
- `PUT /api/affilie/{id}` - Accepte `photoUrl` en mise à jour
- `GET /api/adhesion/{id}` - Retourne `photoUrl` dans les données de l'affilié

### Exemples d'utilisation

#### Création avec photo
```json
{
  "nomComplet": "Jean Dupont",
  "photoUrl": "https://storage.example.com/photos/agents/jean-dupont.jpg"
}
```

#### Mise à jour de la photo
```json
{
  "nomComplet": "Jean Dupont",
  "photoUrl": "https://storage.example.com/photos/agents/jean-dupont-updated.jpg"
}
```

#### Réponse avec photo
```json
{
  "id": 1,
  "nomComplet": "Jean Dupont",
  "photoUrl": "https://storage.example.com/photos/agents/jean-dupont.jpg"
}
```

### Bonnes pratiques
- **URLs valides** : Utiliser des URLs complètes et accessibles
- **Optimisation** : Compresser les images pour des temps de chargement rapides
- **Sécurité** : Utiliser des URLs signées ou authentifiées si nécessaire
- **Fallback** : Prévoir une image par défaut si `photoUrl` est null

### Migration de base de données
Les scripts SQL suivants sont disponibles pour la production :
- `AddPhotoUrlToAgent-Production.sql` - Ajoute `PhotoUrl` à la table `Agents`
- `AddPhotoUrlToAffilie-Production.sql` - Ajoute `PhotoUrl` à la table `Affilies`

---

## 💰 Module Retrait Agent

### Généralités
Le module de retrait agent permet aux agents de demander des retraits de leur WalletAgent avec validation stricte des périodes et des soldes.

### Périodes de retrait autorisées
- **Période 1** : Du 15 au 20 du mois inclus
- **Période 2** : À partir du 30 jusqu'à la fin du mois

### Workflow complet
1. **Demande de retrait** → Validation période et solde
2. **Validation** → Génération d'un jeton unique
3. **Utilisation du jeton** → Retrait au bureau de la mutuelle
4. **Mise à jour du solde** → Déduction automatique du WalletAgent

### Vérification de période

#### POST /api/retraitagent/verifier-periode
Vérifie si une date est dans une période de retrait autorisée.

#### Corps de la requête
```json
"2026-03-16"
```

#### Réponse
```json
{
  "estPeriodeAutorisee": true,
  "date": "2026-03-16",
  "jourDuMois": 16,
  "periodeAutorisee": "15-20",
  "message": "Période de retrait autorisée (jours 15-20)"
}
```

### Vérification de solde

#### POST /api/retraitagent/verifier-solde
Vérifie si le solde de l'agent est suffisant pour le retrait demandé.

#### Corps de la requête
```json
{
  "agentId": 1,
  "montantDemande": 50000
}
```

#### Réponse
```json
{
  "agentId": 1,
  "agentNom": "Jean Dupont",
  "montantDemande": 50000,
  "soldeDisponible": 150000,
  "soldeSuffisant": true,
  "difference": 100000,
  "message": "Solde suffisant pour le retrait"
}
```

### Création de demande

#### POST /api/retraitagent
Crée une nouvelle demande de retrait.

#### Corps de la requête
```json
{
  "agentId": 1,
  "montantDemande": 50000,
  "typeRetrait": "PARTIEL",
  "motifRetrait": "Frais de scolarité"
}
```

#### Réponse
```json
{
  "idDemande": 1,
  "agentId": 1,
  "agentNom": "Jean Dupont",
  "montantDemande": 50000,
  "typeRetrait": "PARTIEL",
  "statutDemande": "EN_ATTENTE",
  "motifRetrait": "Frais de scolarité",
  "dateDemande": "2026-03-16T10:30:00",
  "jetonRetraitId": null,
  "jetonRetraitCode": null
}
```

### Validation et génération de jeton

#### POST /api/retraitagent/valider-et-generer-jeton
Valide une demande et génère un jeton de retrait.

#### Corps de la requête
```json
{
  "idDemande": 1,
  "agentValidationId": 2,
  "statutDemande": "VALIDEE"
}
```

#### Réponse
```json
{
  "succes": true,
  "message": "Demande validée et jeton généré avec succès",
  "demande": {
    "idDemande": 1,
    "statutDemande": "VALIDEE",
    "dateValidation": "2026-03-16T11:00:00",
    "jetonRetraitId": 1,
    "jetonRetraitCode": "JRTA1B2C3D4"
  },
  "jeton": {
    "idJeton": 1,
    "codeJeton": "JRTA1B2C3D4",
    "dateGeneration": "2026-03-16T11:00:00",
    "dateExpiration": "2026-03-23T11:00:00",
    "estValide": true,
    "estUtilise": false
  }
}
```

### Utilisation du jeton

#### POST /api/retraitagent/utiliser-jeton
Utilise un jeton pour effectuer le retrait au bureau de la mutuelle.

#### Corps de la requête
```json
{
  "idJeton": 1,
  "codeJeton": "JRTA1B2C3D4",
  "agentId": 1,
  "observationUtilisation": "Retrait effectué avec succès"
}
```

#### Réponse
```json
{
  "succes": true,
  "message": "Jeton utilisé avec succès",
  "jeton": {
    "idJeton": 1,
    "codeJeton": "JRTA1B2C3D4",
    "estUtilise": true,
    "dateUtilisation": "2026-03-16T14:30:00",
    "observationUtilisation": "Retrait effectué avec succès"
  },
  "demande": {
    "idDemande": 1,
    "statutDemande": "TRAITEE",
    "dateTraitement": "2026-03-16T14:30:00"
  },
  "walletAgent": {
    "soldePrecedent": 150000,
    "soldeActuel": 100000,
    "montantRetire": 50000
  }
}
```

### Endpoints supplémentaires

#### GET /api/retraitagent
Récupère toutes les demandes de retrait.

#### GET /api/retraitagent/{id}
Récupère une demande spécifique.

#### GET /api/retraitagent/by-agent/{agentId}
Récupère les demandes d'un agent spécifique.

#### GET /api/retraitagent/by-statut/{statut}
Récupère les demandes par statut (EN_ATTENTE, VALIDEE, TRAITEE, REJETEE).

#### GET /api/retraitagent/en-attente
Récupère les demandes en attente de validation.

#### GET /api/retraitagent/validees
Récupère les demandes validées avec jeton.

#### GET /api/retraitagent/traitees
Récupère les demandes traitées.

#### GET /api/retraitagent/stats/{date}
Récupère les statistiques mensuelles.

#### PUT /api/retraitagent/{id}
Met à jour une demande.

#### DELETE /api/retraitagent/{id}
Supprime une demande.

---

## 📊 Dashboard Affilié

### Généralités
Le dashboard affilié fournit une vue complète et personnalisée pour chaque affilié, incluant ses KPIs, historique, graphiques et préférences.

### Dashboard complet

#### GET /api/dashboardaffilie/resume/{affilieId}?annee=2026
Récupère le dashboard complet de l'affilié.

#### Réponse
```json
{
  "kpis": {
    "idAffilie": 1,
    "codeAdhesion": "AFF001",
    "nomComplet": "Jean Dupont",
    "soldeTotal": 100000,
    "soldeDisponible": 100000,
    "totalCotisations": 150000,
    "totalPrestations": 50000,
    "nombrePrestations": 3,
    "montantDerniereCotisation": 10000,
    "dateDerniereCotisation": "2026-03-15T10:00:00",
    "tauxUtilisation": 33.33,
    "ancienneteMois": 12,
    "estActif": true
  },
  "informations": {
    "idAffilie": 1,
    "codeAdhesion": "AFF001",
    "nomComplet": "Jean Dupont",
    "telephone": "+243123456789",
    "dateNaissance": "1980-01-01",
    "photoUrl": "https://example.com/photos/affilie1.jpg",
    "dateAdhesion": "2025-03-01",
    "statutAdhesion": "Actif",
    "typeAdhesion": "Premium"
  },
  "cotisationsRecentes": [...],
  "prestationsRecentes": [...],
  "beneficiaires": [...],
  "graphiques": {
    "cotisationsMensuelles": [...],
    "prestationsMensuelles": [...],
    "evolutionSolde": [...],
    "repartitionPrestations": [...]
  },
  "notificationsRecentes": [...],
  "documentsEnAttente": [...],
  "preferences": {...}
}
```

### KPIs de l'affilié

#### GET /api/dashboardaffilie/kpis/{affilieId}
Récupère les indicateurs clés de performance. Les montants (`totalCotisations`, `totalPrestations`, `soldeTotal`, `soldeDisponible`, `montantDerniereCotisation`, `montantDernierePrestation`, `restePlafond`) sont consolidés en **devise principale** via `MontantDevisePrincipale` (repli sur `Montant`). Le champ `devisePrincipaleCode` indique la devise (ex. `USD`).

#### Réponse
```json
{
  "idAffilie": 1,
  "codeAdhesion": "AFF001",
  "nomComplet": "Jean Dupont",
  "soldeTotal": 100000,
  "soldeDisponible": 100000,
  "totalCotisations": 150000,
  "totalPrestations": 50000,
  "devisePrincipaleCode": "USD",
  "nombrePrestations": 3,
  "montantDerniereCotisation": 10000,
  "dateDerniereCotisation": "2026-03-15T10:00:00",
  "tauxUtilisation": 33.33,
  "tauxCouverture": 100,
  "ancienneteMois": 12,
  "estActif": true,
  "nombreBeneficiaires": 1,
  "montantPlafond": 1000000,
  "restePlafond": 950000
}
```

### Cotisations

#### GET /api/dashboardaffilie/cotisations/{affilieId}?mois=3&annee=2026
Récupère les cotisations d'une période spécifique.

#### Réponse
```json
[
  {
    "idCotisation": 1,
    "montant": 10000,
    "dateCotisation": "2026-03-15T10:00:00",
    "typeCotisation": "MENSUELLE",
    "reference": "REF001",
    "statut": "PAYE",
    "agentCollecteur": "Agent A",
    "modePaiement": "MOBILE_MONEY",
    "cumulMois": 10000,
    "cumulAnnee": 120000,
    "estEnRetard": false
  }
]
```

#### GET /api/dashboardaffilie/cotisations/recentes/{affilieId}?limit=10
Récupère les cotisations récentes.

### Prestations

#### GET /api/dashboardaffilie/prestations/{affilieId}?mois=3&annee=2026
Récupère les prestations d'une période spécifique.

#### Réponse
```json
[
  {
    "idPrestation": 1,
    "montantTotal": 20000,
    "montantRembourse": 18000,
    "montantPriseEnCharge": 2000,
    "tauxRemboursement": 90,
    "datePrestation": "2026-03-10T14:30:00",
    "dateDemande": "2026-03-08T09:00:00",
    "dateRemboursement": "2026-03-12T16:00:00",
    "typePrestation": "CONSULTATION",
    "prestationNom": "Consultation générale",
    "statut": "REMBOURSE",
    "beneficiaire": "Jean Dupont",
    "structureSante": "Hôpital Central",
    "referenceFacture": "FACT001",
    "delaiTraitementJours": 4,
    "tauxRemboursementMoyen": 90
  }
]
```

#### GET /api/dashboardaffilie/prestations/recentes/{affilieId}?limit=10
Récupère les prestations récentes.

### Bénéficiaires

#### GET /api/dashboardaffilie/beneficiaires/{affilieId}
Récupère les bénéficiaires de l'affilié.

#### Réponse
```json
[
  {
    "idBeneficiaire": 1,
    "idAffilie": 1,
    "nomComplet": "Marie Dupont",
    "lienParente": "Épouse",
    "dateNaissance": "1985-05-15",
    "typeBeneficiaire": "PRINCIPAL",
    "estActif": true,
    "dateAjout": "2025-03-01",
    "numeroCNI": "CD123456",
    "telephone": "+243987654321",
    "plafondIndividuel": 500000,
    "utiliseAnnee": 20000,
    "resteDisponible": 480000,
    "age": 40,
    "estPrincipal": false
  }
]
```

### Graphiques et statistiques

#### GET /api/dashboardaffilie/graphiques/{affilieId}?annee=2026
Récupère les graphiques et statistiques.

#### Réponse
```json
{
  "cotisationsMensuelles": [
    {
      "mois": 1,
      "annee": 2026,
      "moisAnnee": "Jan 2026",
      "montantCotise": 10000,
      "objectifCotisation": 10000,
      "tauxRealisation": 100,
      "nombreCotisations": 1,
      "moyenneCotisation": 10000,
      "cumulAnnee": 10000
    }
  ],
  "prestationsMensuelles": [
    {
      "mois": 1,
      "annee": 2026,
      "moisAnnee": "Jan 2026",
      "montantTotalPrestations": 15000,
      "montantRembourse": 13500,
      "nombrePrestations": 1,
      "tauxRemboursementMoyen": 90,
      "moyennePrestation": 15000,
      "cumulAnnee": 15000
    }
  ],
  "evolutionSolde": [
    {
      "date": "2026-01-01T00:00:00",
      "soldeApresOperation": 10000,
      "variation": 10000,
      "variationPourcentage": 100,
      "typeOperation": "COTISATION",
      "cumulCotisations": 10000,
      "cumulPrestations": 0
    }
  ],
  "repartitionPrestations": [
    {
      "typePrestation": "CONSULTATION",
      "montantTotal": 30000,
      "nombrePrestations": 2,
      "pourcentageTotal": 60,
      "montantMoyen": 15000,
      "tauxRemboursementMoyen": 90
    }
  ],
  "resumeAnnuel": {
    "annee": 2026,
    "totalCotisations": 120000,
    "totalPrestations": 50000,
    "soldeFinAnnee": 70000,
    "variationAnnuelle": 70000,
    "variationPourcentage": 140,
    "tauxUtilisationMoyen": 41.67
  }
}
```

### Notifications

#### GET /api/dashboardaffilie/notifications/{affilieId}?limit=20
Récupère les notifications de l'affilié.

#### Réponse
```json
[
  {
    "idNotification": 1,
    "typeNotification": "RAPPEL_COTISATION",
    "titre": "Rappel de cotisation",
    "message": "Votre cotisation de Mars est due",
    "dateNotification": "2026-03-01T09:00:00",
    "estLue": false,
    "dateLecture": null,
    "priorite": "HAUTE",
    "categorie": "COTISATION",
    "estActionRequise": true,
    "urlAction": "/cotisations/payer",
    "codeAdhesion": "AFF001",
    "nomAffilie": "Jean Dupont"
  }
]
```

#### GET /api/dashboardaffilie/notifications/non-lues/{affilieId}
Récupère le nombre de notifications non lues.

#### PUT /api/dashboardaffilie/notifications/{idNotification}/lire
Marque une notification comme lue.

### Documents

#### GET /api/dashboardaffilie/documents/{affilieId}
Récupère tous les documents de l'affilié.

#### GET /api/dashboardaffilie/documents/en-attente/{affilieId}
Récupère les documents en attente de validation.

#### Réponse
```json
[
  {
    "idDocument": 1,
    "typeDocument": "CARTE_IDENTITE",
    "nomDocument": "Carte d'identité.jpg",
    "urlDocument": "https://storage.example.com/docs/1.jpg",
    "dateUpload": "2026-03-01T10:00:00",
    "extension": "jpg",
    "tailleOctets": 2048576,
    "tailleAffichee": "2.0 MB",
    "estValide": false,
    "dateValidation": null,
    "validateur": null,
    "motifRejet": null,
    "estObligatoire": true,
    "dateExpiration": "2028-01-01",
    "joursAvantExpiration": 700
  }
]
```

### Préférences

#### GET /api/dashboardaffilie/preferences/{affilieId}
Récupère les préférences de l'affilié.

#### PUT /api/dashboardaffilie/preferences/{affilieId}
Met à jour les préférences de l'affilié.

#### Corps de la requête
```json
{
  "notificationsEmail": true,
  "notificationsSMS": false,
  "languePreferee": "fr",
  "fuseauHoraire": "UTC+1",
  "recevoirRappelsCotisation": true,
  "recevoirAlertesPrestation": true,
  "recevoirNewsletter": false,
  "frequenceRappelsJours": 7,
  "emailSecondaire": "jean.dupont@example.com",
  "modeSombre": false,
  "formatRapports": "PDF",
  "partagerDonneesStatistiques": true
}
```

### Exports

#### GET /api/dashboardaffilie/export/cotisations/{affilieId}?mois=3&annee=2026&format=PDF
Exporte les cotisations au format spécifié (PDF, Excel, CSV).

#### GET /api/dashboardaffilie/export/prestations/{affilieId}?mois=3&annee=2026&format=EXCEL
Exporte les prestations au format spécifié.

### Alertes

#### GET /api/dashboardaffilie/alertes/cotisation/{affilieId}
Récupère les alertes de cotisation.

#### GET /api/dashboardaffilie/alertes/prestation/{affilieId}
Récupère les alertes de prestation.

#### GET /api/dashboardaffilie/alertes/document/{affilieId}
Récupère les alertes de document.

#### GET /api/dashboardaffilie/alertes/expiration/{affilieId}
Récupère les alertes d'expiration.

### Résumé annuel

#### GET /api/dashboardaffilie/resume-annuel/{affilieId}?annee=2026
Récupère le résumé annuel de l'affilié.

#### Réponse
```json
{
  "annee": 2026,
  "totalCotisations": 120000,
  "totalPrestations": 50000,
  "soldeFinAnnee": 70000,
  "soldeDebutAnnee": 0,
  "variationAnnuelle": 70000,
  "variationPourcentage": 100,
  "totalCotisationsEffectuees": 12,
  "totalPrestationsRecues": 3,
  "tauxUtilisationMoyen": 41.67,
  "tauxRemboursementMoyen": 90,
  "meilleurMoisCotisation": 10000,
  "meilleurMoisPrestation": 15000,
  "joursCouvertureMoyenne": 365,
  "satisfactionGlobale": 4.5
}
```

---

## Zones Sociales

### GET /api/zonesociale
Récupère la liste de toutes les zones sociales.

#### En-têtes requis
```http
Authorization: Bearer {token_jwt}
```

#### Réponse
```json
[
  {
    "id": 1,
    "nom": "Kinshasa-Ville",
    "communeId": 1,
    "communeNom": "Kinshasa",
    "statut": true
  }
]
```

### GET /api/zonesociale/{id}
Récupère une zone sociale spécifique par son ID.

#### Paramètres
- `id` (int): ID de la zone sociale

#### Réponse
```json
{
  "id": 1,
  "nom": "Kinshasa-Ville",
  "communeId": 1,
  "communeNom": "Kinshasa",
  "statut": true
}
```

### POST /api/zonesociale
Crée une nouvelle zone sociale.

#### Corps de la requête
```json
{
  "nom": "Lubumbashi-Centre",
  "communeId": 2,
  "statut": true
}
```

### PUT /api/zonesociale/{id}
Met à jour une zone sociale existante.

### DELETE /api/zonesociale/{id}
Supprime une zone sociale.

---

## Communes

### GET /api/commune
Récupère la liste de toutes les communes.

#### Réponse
```json
[
  {
    "id": 1,
    "nom": "Kinshasa",
    "provinceId": 1,
    "provinceNom": "Kinshasa",
    "nombreZones": 5
  }
]
```

### GET /api/commune/{id}
Récupère une commune spécifique par son ID.

### POST /api/commune
Crée une nouvelle commune.

#### Corps de la requête
```json
{
  "nom": "Matete",
  "provinceId": 1,
  "statut": true
}
```

### PUT /api/commune/{id}
Met à jour une commune existante.

### DELETE /api/commune/{id}
Supprime une commune.

---

## Provinces

### GET /api/province
Récupère la liste de toutes les provinces.

#### Réponse
```json
[
  {
    "id": 1,
    "nom": "Kinshasa",
    "statut": true,
    "dateCreation": "2026-03-03T10:00:00",
    "dateModification": null
  }
]
```

### GET /api/province/{id}
Récupère une province spécifique par son ID.

### POST /api/province
Crée une nouvelle province.

#### Corps de la requête
```json
{
  "nom": "Haut-Katanga",
  "statut": true
}
```

---

## Collectes

### GET /api/collecte
Récupère la liste de toutes les collectes.

#### Réponse
```json
[
  {
    "idCollecte": 1,
    "affilieId": 1,
    "affilieNom": "Martin Pierre",
    "agentId": 1,
    "agentNom": "Jean Dupont",
    "referencePaiement": "REF001",
    "modePaiement": "Mobile Money",
    "operateur": "Vodacom",
    "statutPaiement": "Payé",
    "montantRecu": 100.00,
    "montantAttendu": 100.00,
    "deviseId": 1,
    "deviseNom": "Franc Congolais",
    "deviseCode": "CDF",
    "dateCollecte": "2026-03-03T10:00:00",
    "observation": "Paiement reçu",
    "dateCreation": "2026-03-03T10:00:00",
    "dateModification": null,
    "statut": true
  }
]
```

### POST /api/collecte
Crée une nouvelle collecte.

#### Corps de la requête
```json
{
  "referencePaiement": "REF002",
  "modePaiement": "Mobile Money",
  "operateur": "Airtel",
  "statutPaiement": "Payé",
  "montantRecu": 150.00,
  "montantAttendu": 150.00,
  "dateCollecte": "2026-03-03T11:00:00",
  "affilieId": 1,
  "agentId": 1,
  "deviseId": 1,
  "souscriptionPrestationId": 1,
  "observation": "Paiement mensuel"
}
```

### GET /api/collecte/{id}
Récupère une collecte spécifique par son ID.

### PUT /api/collecte/{id}
Met à jour une collecte existante.

### DELETE /api/collecte/{id}
Supprime une collecte.

---

## Devises

### GET /api/devise
Récupère la liste de toutes les devises.

#### Réponse
```json
[
  {
    "idDevise": 1,
    "nom": "Franc Congolais",
    "code": "CDF",
    "symbole": "FC",
    "tauxChange": 1.0,
    "statut": true,
    "dateCreation": "2026-03-03T10:00:00",
    "dateModification": null
  },
  {
    "idDevise": 2,
    "nom": "Dollar Américain",
    "code": "USD",
    "symbole": "$",
    "tauxChange": 2500.0,
    "statut": true,
    "dateCreation": "2026-03-03T10:00:00",
    "dateModification": null
  }
]
```

### POST /api/devise
Crée une nouvelle devise.

#### Corps de la requête
```json
{
  "nom": "Euro",
  "code": "EUR",
  "symbole": "€",
  "tauxChange": 2700.0,
  "statut": true
}
```

### PUT /api/devise/{id}
Met à jour une devise existante.

### DELETE /api/devise/{id}
Supprime une devise.

---

## Prestations

### GET /api/prestation
Récupère la liste de toutes les prestations.

#### Réponse
```json
[
  {
    "idPrestation": 1,
    "libelle": "Assurance Santé",
    "description": "Couverture médicale complète",
    "cotisation": 100.00,
    "categoriePrestationId": 1,
    "categoriePrestationNom": "Santé",
    "statut": true,
    "dateCreation": "2026-03-03T10:00:00",
    "dateModification": null
  }
]
```

### POST /api/prestation
Crée une nouvelle prestation.

#### Corps de la requête
```json
{
  "libelle": "Assurance Éducation",
  "description": "Couverture frais scolaires",
  "cotisation": 50.00,
  "categoriePrestationId": 2,
  "statut": true
}
```

### GET /api/prestation/{id}
Récupère une prestation spécifique par son ID.

### PUT /api/prestation/{id}
Met à jour une prestation existante.

### DELETE /api/prestation/{id}
Supprime une prestation.

---

## Catégories

### Catégories d'Adhésions

#### GET /api/categorieadhesion
Récupère la liste des catégories d'adhésions.

#### Réponse
```json
[
  {
    "idCategorieAdhesion": 1,
    "libelle": "Premium",
    "description": "Adhésion premium avec tous les avantages",
    "statut": true,
    "dateCreation": "2026-03-03T10:00:00",
    "dateModification": null
  }
]
```

### Catégories d'Agents

#### GET /api/categorieagent
Récupère la liste des catégories d'agents.

#### Réponse
```json
[
  {
    "idCategorieAgent": 1,
    "libelleCategorie": "Agent Principal",
    "descriptionCategorie": "Agent avec droits complets",
    "statut": true,
    "dateCreation": "2026-03-03T10:00:00",
    "dateModification": null
  }
]
```

### Catégories de Prestations

#### GET /api/categorieprestation
Récupère la liste des catégories de prestations.

#### Réponse
```json
[
  {
    "idCategoriePrestation": 1,
    "libelleCategorie": "Santé",
    "descriptionCategorie": "Prestations de santé",
    "statut": true,
    "dateCreation": "2026-03-03T10:00:00",
    "dateModification": null
  }
]
```

### Types d'Adhésions

#### GET /api/typeadhesion
Récupère la liste des types d'adhésions.

#### Réponse
```json
[
  {
    "id": 1,
    "libelle": "Individuel",
    "description": "Adhésion individuelle",
    "dateCreation": "2026-03-03T10:00:00",
    "dateModification": null
  },
  {
    "id": 2,
    "libelle": "Familial",
    "description": "Adhésion familiale",
    "dateCreation": "2026-03-03T10:00:00",
    "dateModification": null
  }
]
```

---

## Utilisateurs et Rôles

### Utilisateurs

#### GET /api/utilisateur
Récupère la liste de tous les utilisateurs.

#### Réponse
```json
[
  {
    "idUtilisateur": 1,
    "referenceUtilisateur": null,
    "nomUtilisateur": "admin",
    "emailUtilisateur": "admin@prosoc.cd",
    "phoneUtilisateur": "+243999999999",
    "statut": true,
    "dateCreation": "2026-03-03T10:00:00",
    "roleId": 1,
    "agentId": 1,
    "affilieId": null
  }
]
```

#### POST /api/utilisateur
Crée un nouvel utilisateur.

#### Corps de la requête
```json
{
  "nomUtilisateur": "jean.dupont",
  "emailUtilisateur": "jean.dupont@prosoc.cd",
  "phoneUtilisateur": "+243812345678",
  "motDePasse": "MotDePasse123!",
  "statut": true,
  "roleId": 2,
  "agentId": 1
}
```

#### GET /api/utilisateur/{id}
Récupère un utilisateur spécifique par son ID.

#### Réponse
```json
{
  "idUtilisateur": 1,
  "referenceUtilisateur": null,
    "nomComplet": "admin",
    "nomUtilisateur": "admin",
    "email": "admin@prosoc.cd",
    "telephone": "+243999999999",
    "photoUrl": null,
    "genre": null,
    "statut": true,
    "dateCreation": "2026-03-03T10:00:00",
    "isConnecte": false,
    "doitChangerMotDePasse": false,
    "agentId": 1,
    "affilieId": null
}
```

#### PUT /api/utilisateur/{id}
Met à jour un utilisateur existant.

#### Corps de la requête
```json
{
  "nomUtilisateur": "jean.dupont.updated",
  "emailUtilisateur": "jean.dupont.updated@prosoc.cd",
  "phoneUtilisateur": "+243812345679",
  "statut": true,
  "roleId": 2,
  "agentId": 1
}
```

#### DELETE /api/utilisateur/{id}
Supprime un utilisateur.

#### POST /api/utilisateur/login
Authentifie un utilisateur et retourne un token JWT.

#### Corps de la requête
```json
{
  "nomUtilisateur": "admin@prosoc.cd",
  "motDePasse": "Admin"
}
```

#### Réponse réussie
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAtUtc": "2026-03-03T20:00:00Z",
  "utilisateur": {
    "idUtilisateur": 1,
    "referenceUtilisateur": "",
    "nomComplet": "admin",
    "nomUtilisateur": "admin",
    "email": "admin@prosoc.cd",
    "telephone": "+243999999999",
    "photoUrl": null,
    "genre": null,
    "statut": true,
    "dateCreation": "2026-03-03T20:56:51.622409",
    "isConnecte": false,
    "doitChangerMotDePasse": false,
    "agentId": 1,
    "affilieId": null
  }
}
```

### Rôles

#### GET /api/role
Récupère la liste de tous les rôles.

#### Réponse
```json
[
  {
    "idRole": 1,
    "nomRole": "Admin",
    "descriptionRole": "Administrateur système",
    "dateCreation": "2026-03-03T10:00:00",
    "dateModification": null
  },
  {
    "idRole": 2,
    "nomRole": "Agent",
    "descriptionRole": "Agent d'adhésion",
    "dateCreation": "2026-03-03T10:00:00",
    "dateModification": null
  }
]
```

### Permissions

#### GET /api/permission
Récupère la liste de toutes les permissions.

#### Réponse
```json
[
  {
    "idPermission": 2,
    "nomPermission": "READ_USER",
    "descriptionPermission": "Voir les utilisateurs",
    "dateCreation": "2026-03-03T10:00:00",
    "dateModification": null
  }
]
```

---

## Exemples d'intégration

### Vue.js

#### Installation des dépendances
```bash
npm install axios
```

#### Service API (api.js)
```javascript
// src/services/api.js
import axios from 'axios';

const API_BASE_URL = process.env.VUE_APP_API_URL || 'https://dev-prosoc.asdc-rdc.org';

class ApiService {
  constructor() {
    this.client = axios.create({
      baseURL: API_BASE_URL,
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json'
      }
    });

    // Intercepteur pour ajouter le token JWT
    this.client.interceptors.request.use(config => {
      const token = localStorage.getItem('token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });

    // Intercepteur pour gérer les erreurs
    this.client.interceptors.response.use(
      response => response,
      error => {
        if (error.response?.status === 401) {
          localStorage.removeItem('token');
          window.location.href = '/login';
        }
        return Promise.reject(error);
      }
    );
  }

  // Authentification
  async login(credentials) {
    try {
      const response = await this.client.post('/api/auth/login', credentials);
      const { accessToken, expiresAtUtc, utilisateurId, nomUtilisateur, role } = response.data;
      
      // Sauvegarder le token et les infos utilisateur
      localStorage.setItem('token', accessToken);
      localStorage.setItem('user', JSON.stringify({
        id: utilisateurId,
        username: nomUtilisateur,
        role: role
      }));
      
      return response.data;
    } catch (error) {
      throw new Error(error.response?.data?.message || 'Erreur de connexion');
    }
  }

  // Agents
  async getAgents() {
    const response = await this.client.get('/api/agent');
    return response.data;
  }

  async createAgent(agentData) {
    const response = await this.client.post('/api/agent', agentData);
    return response.data;
  }

  async updateAgent(id, agentData) {
    const response = await this.client.put(`/api/agent/${id}`, agentData);
    return response.data;
  }

  async deleteAgent(id) {
    await this.client.delete(`/api/agent/${id}`);
  }

  // Adhésions
  async getAdhesions() {
    const response = await this.client.get('/api/adhesion');
    return response.data;
  }

  async createAdhesionWithAffilie(adhesionData) {
    const response = await this.client.post('/api/adhesion/with-affilie', adhesionData);
    return response.data;
  }

  // Affiliés
  async getAffilies() {
    const response = await this.client.get('/api/affilie');
    return response.data;
  }
}

export default new ApiService();
```

#### Composant Vue.js (AgentsList.vue)
```vue
<template>
  <div class="agents-list">
    <h1>Liste des Agents</h1>
    
    <!-- Formulaire d'ajout -->
    <div class="agent-form">
      <h2>Ajouter un Agent</h2>
      <form @submit.prevent="addAgent">
        <div class="form-group">
          <label>Nom Complet:</label>
          <input v-model="newAgent.nomComplet" required />
        </div>
        <div class="form-group">
          <label>Téléphone:</label>
          <input v-model="newAgent.phone" type="tel" required />
        </div>
        <div class="form-group">
          <label>Code AT:</label>
          <input v-model="newAgent.codeAT" />
        </div>
        <div class="form-group">
          <label>Zone Sociale:</label>
          <select v-model="newAgent.zoneSocialeId">
            <option v-for="zone in zonesSociales" :key="zone.id" :value="zone.id">
              {{ zone.nom }}
            </option>
          </select>
        </div>
        <button type="submit" :disabled="loading">
          {{ loading ? 'Ajout...' : 'Ajouter' }}
        </button>
      </form>
    </div>

    <!-- Liste des agents -->
    <div class="agents-table">
      <h2>Agents Existants</h2>
      <table v-if="agents.length > 0">
        <thead>
          <tr>
            <th>ID</th>
            <th>Nom</th>
            <th>Téléphone</th>
            <th>Code AT</th>
            <th>Zone</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="agent in agents" :key="agent.id">
            <td>{{ agent.id }}</td>
            <td>{{ agent.nomComplet }}</td>
            <td>{{ agent.phone }}</td>
            <td>{{ agent.codeAT || '-' }}</td>
            <td>{{ agent.zoneSocialeNom || '-' }}</td>
            <td>
              <button @click="editAgent(agent)" class="btn-edit">Modifier</button>
              <button @click="deleteAgent(agent.id)" class="btn-delete">Supprimer</button>
            </td>
          </tr>
        </tbody>
      </table>
      <p v-else>Aucun agent trouvé</p>
    </div>
  </div>
</template>

<script>
import api from '@/services/api';

export default {
  name: 'AgentsList',
  data() {
    return {
      agents: [],
      zonesSociales: [],
      newAgent: {
        nomComplet: '',
        phone: '',
        codeAT: '',
        zoneSocialeId: null,
        categorieAgentId: 1,
        statut: true
      },
      loading: false
    };
  },
  async mounted() {
    await this.loadAgents();
    await this.loadZonesSociales();
  },
  methods: {
    async loadAgents() {
      try {
        this.agents = await api.getAgents();
      } catch (error) {
        console.error('Erreur lors du chargement des agents:', error);
        this.$toast.error('Erreur lors du chargement des agents');
      }
    },

    async loadZonesSociales() {
      try {
        // Charger les zones sociales depuis l'API
        this.zonesSociales = await api.getZonesSociales();
      } catch (error) {
        console.error('Erreur lors du chargement des zones:', error);
      }
    },

    async addAgent() {
      this.loading = true;
      try {
        await api.createAgent(this.newAgent);
        this.$toast.success('Agent créé avec succès');
        this.newAgent = {
          nomComplet: '',
          phone: '',
          codeAT: '',
          zoneSocialeId: null,
          categorieAgentId: 1,
          statut: true
        };
        await this.loadAgents();
      } catch (error) {
        console.error('Erreur lors de la création:', error);
        this.$toast.error('Erreur lors de la création de l\'agent');
      } finally {
        this.loading = false;
      }
    },

    async deleteAgent(id) {
      if (!confirm('Êtes-vous sûr de vouloir supprimer cet agent ?')) {
        return;
      }
      
      try {
        await api.deleteAgent(id);
        this.$toast.success('Agent supprimé avec succès');
        await this.loadAgents();
      } catch (error) {
        console.error('Erreur lors de la suppression:', error);
        this.$toast.error('Erreur lors de la suppression de l\'agent');
      }
    },

    editAgent(agent) {
      // Logique pour modifier un agent
      this.newAgent = { ...agent };
    }
  }
};
</script>

<style scoped>
.agents-list {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.agent-form {
  background: #f5f5f5;
  padding: 20px;
  border-radius: 8px;
  margin-bottom: 30px;
}

.form-group {
  margin-bottom: 15px;
}

.form-group label {
  display: block;
  margin-bottom: 5px;
  font-weight: bold;
}

.form-group input, .form-group select {
  width: 100%;
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
}

button {
  background: #007bff;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 4px;
  cursor: pointer;
}

button:disabled {
  background: #ccc;
  cursor: not-allowed;
}

.agents-table table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 20px;
}

.agents-table th, .agents-table td {
  border: 1px solid #ddd;
  padding: 8px;
  text-align: left;
}

.agents-table th {
  background: #f8f9fa;
}

.btn-edit {
  background: #28a745;
  margin-right: 5px;
}

.btn-delete {
  background: #dc3545;
}
</style>
```

#### Store Vuex (store.js)
```javascript
// src/store/index.js
import Vue from 'vue';
import Vuex from 'vuex';
import api from '@/services/api';

Vue.use(Vuex);

export default new Vuex.Store({
  state: {
    user: null,
    token: null,
    isAuthenticated: false
  },
  
  mutations: {
    SET_AUTH(state, { token, user }) {
      state.token = token;
      state.user = user;
      state.isAuthenticated = !!token;
      localStorage.setItem('token', token);
      localStorage.setItem('user', JSON.stringify(user));
    },
    
    CLEAR_AUTH(state) {
      state.token = null;
      state.user = null;
      state.isAuthenticated = false;
      localStorage.removeItem('token');
      localStorage.removeItem('user');
    }
  },
  
  actions: {
    async login({ commit }, credentials) {
      try {
        const response = await api.login(credentials);
        commit('SET_AUTH', {
          token: response.accessToken,
          user: {
            id: response.utilisateurId,
            username: response.nomUtilisateur,
            role: response.role
          }
        });
        return response;
      } catch (error) {
        commit('CLEAR_AUTH');
        throw error;
      }
    },
    
    logout({ commit }) {
      commit('CLEAR_AUTH');
    },
    
    initializeAuth({ commit }) {
      const token = localStorage.getItem('token');
      const user = localStorage.getItem('user');
      
      if (token && user) {
        commit('SET_AUTH', {
          token,
          user: JSON.parse(user)
        });
      }
    }
  },
  
  getters: {
    isAuthenticated: state => state.isAuthenticated,
    currentUser: state => state.user,
    authToken: state => state.token
  }
});
```

---

### Flutter

#### Installation des dépendances
```yaml
dependencies:
  flutter:
    sdk: flutter
  http: ^0.13.5
  json_annotation: ^4.8.1
  shared_preferences: ^2.2.2

dev_dependencies:
  flutter_test:
    sdk: flutter
  json_serializable: ^6.7.1
  build_runner: ^2.4.6
```

#### Service API (api_service.dart)
```dart
// lib/services/api_service.dart
import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

class ApiService {
  static const String _baseUrl = 'https://dev-prosoc.asdc-rdc.org';
  static const String _tokenKey = 'auth_token';
  
  late String _token;
  
  ApiService() {
    _loadToken();
  }
  
  Future<void> _loadToken() async {
    final prefs = await SharedPreferences.getInstance();
    _token = prefs.getString(_tokenKey) ?? '';
  }
  
  Future<void> _saveToken(String token) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_tokenKey, token);
    _token = token;
  }
  
  Future<void> _clearToken() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_tokenKey);
    _token = '';
  }
  
  Map<String, String> _getHeaders({bool requireAuth = true}) {
    final headers = <String, String>{
      'Content-Type': 'application/json',
    };
    
    if (requireAuth && _token.isNotEmpty) {
      headers['Authorization'] = 'Bearer $_token';
    }
    
    return headers;
  }
  
  // Authentification
  Future<LoginResponse> login(LoginRequest request) async {
    try {
      final response = await http.post(
        Uri.parse('$_baseUrl/api/auth/login'),
        headers: _getHeaders(requireAuth: false),
        body: jsonEncode(request.toJson()),
      );
      
      if (response.statusCode == 200) {
        final loginResponse = LoginResponse.fromJson(jsonDecode(response.body));
        await _saveToken(loginResponse.accessToken);
        return loginResponse;
      } else {
        throw Exception('Échec de l\'authentification');
      }
    } catch (e) {
      throw Exception('Erreur de connexion: $e');
    }
  }
  
  Future<void> logout() async {
    await _clearToken();
  }
  
  // Agents
  Future<List<Agent>> getAgents() async {
    try {
      final response = await http.get(
        Uri.parse('$_baseUrl/api/agent'),
        headers: _getHeaders(),
      );
      
      if (response.statusCode == 200) {
        final List<dynamic> jsonData = jsonDecode(response.body);
        return jsonData.map((json) => Agent.fromJson(json)).toList();
      } else {
        throw Exception('Erreur lors du chargement des agents');
      }
    } catch (e) {
      throw Exception('Erreur: $e');
    }
  }
  
  Future<Agent> createAgent(AgentCreateRequest request) async {
    try {
      final response = await http.post(
        Uri.parse('$_baseUrl/api/agent'),
        headers: _getHeaders(),
        body: jsonEncode(request.toJson()),
      );
      
      if (response.statusCode == 201) {
        return Agent.fromJson(jsonDecode(response.body));
      } else {
        throw Exception('Erreur lors de la création de l\'agent');
      }
    } catch (e) {
      throw Exception('Erreur: $e');
    }
  }
  
  Future<Agent> updateAgent(int id, AgentUpdateRequest request) async {
    try {
      final response = await http.put(
        Uri.parse('$_baseUrl/api/agent/$id'),
        headers: _getHeaders(),
        body: jsonEncode(request.toJson()),
      );
      
      if (response.statusCode == 200) {
        return Agent.fromJson(jsonDecode(response.body));
      } else {
        throw Exception('Erreur lors de la mise à jour de l\'agent');
      }
    } catch (e) {
      throw Exception('Erreur: $e');
    }
  }
  
  Future<void> deleteAgent(int id) async {
    try {
      final response = await http.delete(
        Uri.parse('$_baseUrl/api/agent/$id'),
        headers: _getHeaders(),
      );
      
      if (response.statusCode != 200) {
        throw Exception('Erreur lors de la suppression de l\'agent');
      }
    } catch (e) {
      throw Exception('Erreur: $e');
    }
  }
  
  // Adhésions
  Future<List<Adhesion>> getAdhesions() async {
    try {
      final response = await http.get(
        Uri.parse('$_baseUrl/api/adhesion'),
        headers: _getHeaders(),
      );
      
      if (response.statusCode == 200) {
        final List<dynamic> jsonData = jsonDecode(response.body);
        return jsonData.map((json) => Adhesion.fromJson(json)).toList();
      } else {
        throw Exception('Erreur lors du chargement des adhésions');
      }
    } catch (e) {
      throw Exception('Erreur: $e');
    }
  }
  
  Future<Adhesion> createAdhesionWithAffilie(AdhesionCreateRequest request) async {
    try {
      final response = await http.post(
        Uri.parse('$_baseUrl/api/adhesion/with-affilie'),
        headers: _getHeaders(),
        body: jsonEncode(request.toJson()),
      );
      
      if (response.statusCode == 201) {
        return Adhesion.fromJson(jsonDecode(response.body));
      } else {
        throw Exception('Erreur lors de la création de l\'adhésion');
      }
    } catch (e) {
      throw Exception('Erreur: $e');
    }
  }
}
```

#### Modèles (models.dart)
```dart
// lib/models/models.dart

class LoginRequest {
  final String nomUtilisateur;
  final String motDePasse;
  final String? fcmToken;
  final String? deviceType;
  final String? deviceModel;
  final String? osVersion;
  
  LoginRequest({
    required this.nomUtilisateur,
    required this.motDePasse,
    this.fcmToken,
    this.deviceType,
    this.deviceModel,
    this.osVersion,
  });
  
  Map<String, dynamic> toJson() {
    return {
      'nomUtilisateur': nomUtilisateur,
      'motDePasse': motDePasse,
      'fcmToken': fcmToken,
      'deviceType': deviceType,
      'deviceModel': deviceModel,
      'osVersion': osVersion,
    };
  }
}

class LoginResponse {
  final String accessToken;
  final DateTime expiresAtUtc;
  final int utilisateurId;
  final String nomUtilisateur;
  final String? role;
  
  LoginResponse({
    required this.accessToken,
    required this.expiresAtUtc,
    required this.utilisateurId,
    required this.nomUtilisateur,
    this.role,
  });
  
  factory LoginResponse.fromJson(Map<String, dynamic> json) {
    return LoginResponse(
      accessToken: json['accessToken'],
      expiresAtUtc: DateTime.parse(json['expiresAtUtc']),
      utilisateurId: json['utilisateurId'],
      nomUtilisateur: json['nomUtilisateur'],
      role: json['role'],
    );
  }
}

class Agent {
  final int id;
  final String? codeAT;
  final String nomComplet;
  final String matricule;
  final String phone;
  final DateTime dateCreation;
  final DateTime? dateModification;
  final bool statut;
  final int? zoneSocialeId;
  final String? zoneSocialeNom;
  
  Agent({
    required this.id,
    this.codeAT,
    required this.nomComplet,
    required this.matricule,
    required this.phone,
    required this.dateCreation,
    this.dateModification,
    required this.statut,
    this.zoneSocialeId,
    this.zoneSocialeNom,
  });
  
  factory Agent.fromJson(Map<String, dynamic> json) {
    return Agent(
      id: json['id'],
      codeAT: json['codeAT'],
      nomComplet: json['nomComplet'] ?? '',
      matricule: json['matricule'] ?? '',
      phone: json['phone'] ?? '',
      dateCreation: DateTime.parse(json['dateCreation']),
      dateModification: json['dateModification'] != null 
          ? DateTime.parse(json['dateModification']) 
          : null,
      statut: json['statut'] ?? false,
      zoneSocialeId: json['zoneSocialeId'],
      zoneSocialeNom: json['zoneSocialeNom'],
    );
  }
}

class AgentCreateRequest {
  final String? codeAT;
  final String nomComplet;
  final String? matricule;
  final String phone;
  final int? zoneSocialeId;
  final int? categorieAgentId;
  final bool statut;
  
  AgentCreateRequest({
    this.codeAT,
    required this.nomComplet,
    this.matricule,
    required this.phone,
    this.zoneSocialeId,
    this.categorieAgentId,
    this.statut = true,
  });
  
  Map<String, dynamic> toJson() {
    return {
      'codeAT': codeAT,
      'nomComplet': nomComplet,
      'matricule': matricule,
      'phone': phone,
      'zoneSocialeId': zoneSocialeId,
      'categorieAgentId': categorieAgentId,
      'statut': statut,
    };
  }
}

class Adhesion {
  final int idAdhesion;
  final String statutDossier;
  final int agentId;
  final int affilieId;
  final int typeAdhesionId;
  final DateTime dateCreation;
  final DateTime? dateModification;
  
  Adhesion({
    required this.idAdhesion,
    required this.statutDossier,
    required this.agentId,
    required this.affilieId,
    required this.typeAdhesionId,
    required this.dateCreation,
    this.dateModification,
  });
  
  factory Adhesion.fromJson(Map<String, dynamic> json) {
    return Adhesion(
      idAdhesion: json['idAdhesion'],
      statutDossier: json['statutDossier'] ?? '',
      agentId: json['agentId'],
      affilieId: json['affilieId'],
      typeAdhesionId: json['typeAdhesionId'],
      dateCreation: DateTime.parse(json['dateCreation']),
      dateModification: json['dateModification'] != null 
          ? DateTime.parse(json['dateModification']) 
          : null,
    );
  }
}
```

#### Écran Flutter (agents_screen.dart)
```dart
// lib/screens/agents_screen.dart
import 'package:flutter/material.dart';
import '../services/api_service.dart';
import '../models/models.dart';

class AgentsScreen extends StatefulWidget {
  @override
  _AgentsScreenState createState() => _AgentsScreenState();
}

class _AgentsScreenState extends State<AgentsScreen> {
  List<Agent> _agents = [];
  bool _isLoading = false;
  final _formKey = GlobalKey<FormState>();
  final _nomCompletController = TextEditingController();
  final _phoneController = TextEditingController();
  final _codeATController = TextEditingController();
  int? _selectedZoneId;
  
  @override
  void initState() {
    super.initState();
    _loadAgents();
  }
  
  Future<void> _loadAgents() async {
    setState(() => _isLoading = true);
    try {
      final agents = await ApiService().getAgents();
      setState(() {
        _agents = agents;
        _isLoading = false;
      });
    } catch (e) {
      setState(() => _isLoading = false);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Erreur: $e')),
      );
    }
  }
  
  Future<void> _createAgent() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }
    
    setState(() => _isLoading = true);
    
    try {
      final request = AgentCreateRequest(
        nomComplet: _nomCompletController.text,
        phone: _phoneController.text,
        codeAT: _codeATController.text.isEmpty ? null : _codeATController.text,
        zoneSocialeId: _selectedZoneId,
        categorieAgentId: 1,
        statut: true,
      );
      
      await ApiService().createAgent(request);
      
      _nomCompletController.clear();
      _phoneController.clear();
      _codeATController.clear();
      _selectedZoneId = null;
      
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Agent créé avec succès')),
      );
      
      await _loadAgents();
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Erreur: $e')),
      );
    } finally {
      setState(() => _isLoading = false);
    }
  }
  
  Future<void> _deleteAgent(int id) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: Text('Confirmation'),
        content: Text('Voulez-vous vraiment supprimer cet agent ?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: Text('Non'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(context, true),
            child: Text('Oui'),
          ),
        ],
      ),
    );
    
    if (confirmed == true) {
      try {
        await ApiService().deleteAgent(id);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Agent supprimé avec succès')),
        );
        await _loadAgents();
      } catch (e) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Erreur: $e')),
        );
      }
    }
  }
  
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Gestion des Agents'),
        backgroundColor: Colors.blue,
      ),
      body: Padding(
        padding: EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Formulaire d'ajout
            Card(
              child: Padding(
                padding: EdgeInsets.all(16.0),
                child: Form(
                  key: _formKey,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Ajouter un Agent',
                        style: TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      SizedBox(height: 16),
                      TextFormField(
                        controller: _nomCompletController,
                        decoration: InputDecoration(
                          labelText: 'Nom Complet',
                          border: OutlineInputBorder(),
                        ),
                        validator: (value) {
                          if (value == null || value.isEmpty) {
                            return 'Ce champ est requis';
                          }
                          return null;
                        },
                      ),
                      SizedBox(height: 16),
                      TextFormField(
                        controller: _phoneController,
                        decoration: InputDecoration(
                          labelText: 'Téléphone',
                          border: OutlineInputBorder(),
                        ),
                        validator: (value) {
                          if (value == null || value.isEmpty) {
                            return 'Ce champ est requis';
                          }
                          return null;
                        },
                      ),
                      SizedBox(height: 16),
                      TextFormField(
                        controller: _codeATController,
                        decoration: InputDecoration(
                          labelText: 'Code AT (optionnel)',
                          border: OutlineInputBorder(),
                        ),
                      ),
                      SizedBox(height: 16),
                      DropdownButtonFormField<int>(
                        value: _selectedZoneId,
                        decoration: InputDecoration(
                          labelText: 'Zone Sociale',
                          border: OutlineInputBorder(),
                        ),
                        items: [
                          DropdownMenuItem<int>(
                            value: 1,
                            child: Text('Kinshasa-Ville'),
                          ),
                          DropdownMenuItem<int>(
                            value: 2,
                            child: Text('Lubumbashi'),
                          ),
                        ],
                        onChanged: (value) {
                          setState(() => _selectedZoneId = value);
                        },
                      ),
                      SizedBox(height: 24),
                      SizedBox(
                        width: double.infinity,
                        child: ElevatedButton(
                          onPressed: _isLoading ? null : _createAgent,
                          child: _isLoading
                              ? CircularProgressIndicator(color: Colors.white)
                              : Text('Ajouter un Agent'),
                          style: ElevatedButton.styleFrom(
                            backgroundColor: Colors.blue,
                            padding: EdgeInsets.symmetric(vertical: 16),
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
            SizedBox(height: 24),
            
            // Liste des agents
            Expanded(
              child: Card(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Padding(
                      padding: EdgeInsets.all(16.0),
                      child: Text(
                        'Liste des Agents',
                        style: TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                    Expanded(
                      child: _isLoading
                          ? Center(child: CircularProgressIndicator())
                          : _agents.isEmpty
                              ? Center(child: Text('Aucun agent trouvé'))
                              : ListView.builder(
                                  itemCount: _agents.length,
                                  itemBuilder: (context, index) {
                                    final agent = _agents[index];
                                    return ListTile(
                                      title: Text(agent.nomComplet),
                                      subtitle: Text(
                                        '${agent.phone} - ${agent.codeAT ?? 'Pas de code AT'}',
                                      ),
                                      trailing: Row(
                                        mainAxisSize: MainAxisSize.min,
                                        children: [
                                          IconButton(
                                            icon: Icon(Icons.edit, color: Colors.green),
                                            onPressed: () {
                                              // Logique de modification
                                            },
                                          ),
                                          IconButton(
                                            icon: Icon(Icons.delete, color: Colors.red),
                                            onPressed: () => _deleteAgent(agent.id),
                                          ),
                                        ],
                                      ),
                                    );
                                  },
                                ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
```

#### Provider d'authentification (auth_provider.dart)
```dart
// lib/providers/auth_provider.dart
import 'package:flutter/foundation.dart';
import '../services/api_service.dart';
import '../models/models.dart';

class AuthProvider with ChangeNotifier {
  LoginResponse? _user;
  bool _isLoading = false;
  String? _error;
  
  LoginResponse? get user => _user;
  bool get isLoading => _isLoading;
  String? get error => _error;
  bool get isAuthenticated => _user != null;
  
  Future<void> login(String nomUtilisateur, String motDePasse) async {
    _setLoading(true);
    _clearError();
    
    try {
      final request = LoginRequest(
        nomUtilisateur: nomUtilisateur,
        motDePasse: motDePasse,
        deviceType: 'Mobile',
        deviceModel: 'Flutter App',
      );
      
      final response = await ApiService().login(request);
      _user = response;
      notifyListeners();
    } catch (e) {
      _setError(e.toString());
    } finally {
      _setLoading(false);
    }
  }
  
  Future<void> logout() async {
    await ApiService().logout();
    _user = null;
    notifyListeners();
  }
  
  void _setLoading(bool loading) {
    _isLoading = loading;
    notifyListeners();
  }
  
  void _setError(String error) {
    _error = error;
    notifyListeners();
  }
  
  void _clearError() {
    _error = null;
    notifyListeners();
  }
}
```

---

## Bonnes pratiques

### Gestion des erreurs
- Toujours vérifier les codes de statut HTTP
- Implémenter un système de retry pour les erreurs réseau
- Afficher des messages d'erreur clairs à l'utilisateur

### Sécurité
- Ne jamais stocker les tokens en clair
- Utiliser HTTPS en production
- Implémenter le rafraîchissement des tokens
- Valider les entrées utilisateur

### Performance
- Utiliser la pagination pour les grandes listes
- Implémenter le cache local
- Optimiser les images et assets

### Testing
- Tester tous les endpoints avec Postman ou Swagger
- Simuler des erreurs réseau
- Tester avec différents rôles utilisateur

---

## 📚 Guide de Migration

### Pour les développeurs utilisant l'API v1.0 → v2.0

#### ⚠️ Changements majeurs dans le modèle Utilisateur
**Avant (v1.0)**:
```json
{
  "nomUtilisateur": "admin@prosoc.cd",
  "prenomUtilisateur": "Admin",
  "postNomUtilisateur": "System",
  "email": "admin@prosoc.cd"
}
```

**Après (v2.0)**:
```json
{
  "nomUtilisateur": "admin",
  "emailUtilisateur": "admin@prosoc.cd",
  "phoneUtilisateur": "+243999999999"
}
```

#### 🔧 Actions requises pour la migration
1. **Mettre à jour** les formulaires de création/modification d'utilisateurs
2. **Adapter** le code frontend pour utiliser `emailUtilisateur` et `phoneUtilisateur`
3. **Supprimer** les références à `prenomUtilisateur` et `postNomUtilisateur`
4. **Mettre à jour** la logique d'affichage du `nomComplet` (utilise maintenant `nomUtilisateur`)

#### 🔄 Changements d'endpoints
- **Ancien**: `POST /api/auth/login` → **Nouveau**: `POST /api/utilisateur/login`
- **Supprimés**: `/api/authtest/*` et `/api/enhancedauth/*`
- **Amélioré**: `GET /api/utilisateur/{id}` retourne maintenant le même format que le login

#### 📊 Base de données
Appliquer la migration EF Core :
```bash
dotnet ef database update
```

---

## 🚀 Bonnes Practices

#### 📋 Checklist de développement
- [ ] Utiliser les nouveaux champs `emailUtilisateur` et `phoneUtilisateur`
- [ ] Ne plus référencer `prenomUtilisateur` et `postNomUtilisateur`
- [ ] Utiliser `/api/utilisateur/login` pour l'authentification
- [ ] Gérer le champ `nomComplet` généré automatiquement pour les affiliés
- [ ] Tester la migration des données existantes
- [ ] Intégrer le module de retrait agent avec validation périodique
- [ ] Implémenter le dashboard affilié avec KPIs et graphiques
- [ ] Utiliser les nouveaux endpoints `/api/retraitagent/*` et `/api/dashboardaffilie/*`

#### 🔒 Recommandations de sécurité
- Valider les formats d'email et de téléphone côté client
- Utiliser le champ `nomUtilisateur` pour l'affichage (source unique de vérité)
- Implémenter la gestion des erreurs pour les champs manquants
- Sécuriser les endpoints de retrait agent avec validation stricte
- Protéger les données sensibles du dashboard affilié

#### 📱 Notes pour les applications mobiles
- Mettre à jour les formulaires d'inscription
- Adapter les écrans de profil utilisateur
- Utiliser le nouveau format de réponse d'authentification
- Intégrer le workflow de retrait agent avec validation périodique
- Implémenter le dashboard affilié avec graphiques interactifs

#### 🚀 Notes pour le développement web
- Utiliser les nouveaux endpoints de retrait agent pour la gestion des retraits
- Intégrer le dashboard affilié dans l'interface utilisateur
- Implémenter les graphiques avec les données des endpoints appropriés
- Gérer les notifications et alertes en temps réel
- Supporter les exports multi-formats (PDF, Excel, CSV)

---

## Support

Pour toute question sur l'intégration de l'API Prosoc, contactez l'équipe technique à l'adresse : support@prosoc.cd
