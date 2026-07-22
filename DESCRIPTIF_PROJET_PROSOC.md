# Descriptif du projet ProsocAPI

**Version :** 1.0  
**Date :** mai 2026  
**Auteur :** équipe technique Prosoc / Kansabusiness  

---

## 1. Résumé

**ProsocAPI** est le backend de la plateforme **PROSOC**, un système de gestion mutualiste destiné à digitaliser les opérations d’une mutuelle d’assurance en Afrique. L’API centralise l’ensemble des processus métier — adhésions, affiliés, collectes, commissions, produits, prestations, wallets agents et tableaux de bord — et sert d’interface unique aux applications **web** (administration) et **mobile** (collecte terrain).

Le projet est développé en **ASP.NET Core 6** et expose une API REST sécurisée, documentée via Swagger, consommée par les frontends hébergés notamment sur `prosoc.maash.com` et les environnements de test Kansa business.

---

## 2. Contexte et objectifs

### 2.1 Contexte

La mutuelle opère avec un réseau d’**agents de terrain** (agents sociaux, agents techniques) qui recueillent les informations des adhérents sur le terrain, encadrés par des **superviseurs** et des **opérateurs régionaux**. Les flux financiers passent par la **collecte** (mobile money, compte virtuel, espèces via wallet virtuel) et alimentent un système de **commissionnement** automatique des agents.

### 2.2 Objectifs de la plateforme

| Objectif | Description |
|----------|-------------|
| Digitaliser les adhésions | Enregistrement, validation et suivi des adhésions particuliers et entreprises |
| Fiabiliser la collecte terrain | Saisie mobile synchronisée avec contrôle qualité régional |
| Automatiser la rémunération | Calcul des commissions, wallets agents et retraits encadrés |
| Offrir une vision consolidée | Dashboards par rôle (admin, agent, affilié, financier, percepteur, superviseur) |
| Sécuriser l’accès | Authentification JWT, rôles et permissions granulaires |
| Notifier en temps réel | Email, SMS, push Firebase et notifications in-app via SignalR |

### 2.3 Périmètre de ProsocAPI

ProsocAPI est la **couche serveur** : elle ne constitue pas l’interface utilisateur finale, mais porte toute la logique métier, la persistance des données et les intégrations externes (stockage fichiers, notifications, etc.).

---

## 3. Utilisateurs et rôles

| Profil | Usage principal |
|--------|-----------------|
| **Agent de terrain (AT)** | Création d’adhésions, collecte des cotisations, consultation du wallet et demandes de retrait |
| **Superviseur** | Pilotage du réseau d’agents, objectifs (`TargetAgent`), vue zone sociale |
| **Opérateur / administrateur régional** | Mise à jour et validation des dossiers affiliés |
| **Affilié** | Consultation de son profil, paiement de souscriptions, historique |
| **Percepteur / financier** | Suivi des flux financiers et des collectes |
| **Administrateur** | Paramétrage global, rôles, permissions, dashboards admin |
| **Applications mobiles** | Synchronisation offline, configuration app, sessions utilisateur |

L’accès aux fonctionnalités est contrôlé par un modèle **RBAC** (rôles + permissions) couplé à l’authentification JWT.

---

## 4. Fonctionnalités principales

### 4.1 Adhésion et membres

- Création et gestion des **adhésions** (solo, famille F3, famille F6, entreprises).
- Gestion des **affiliés**, **dépendants** et **antécédents** médicaux.
- Génération automatique des **codes d’adhésion** et **matricules**.
- Règles métier d’éligibilité (niveaux d’adhésion, personnes à charge, liens de parenté).
- Upload et traitement de **fichiers** (photos, pièces justificatives) via stockage cloud.

### 4.2 Collecte et finances

- Enregistrement des **collectes** (types, devises, modes de paiement).
- **Commissionnement** configurable par frais et par produit (taux fixes ou hybrides).
- Gestion des **frais**, **transactions** et **souscriptions en arriérés**.
- **Cotisations affilié** avec règles de tarification (périodicité, âge, montants).
- **Paiement affilié** : consultation des souscriptions payables et historique des paiements.

### 4.3 Produits et prestations

- Catalogue **produits mutuels** et **produits assureur**.
- **Prestations** et **souscriptions** associées aux affiliés.
- Synchronisation des prestations liées aux produits.
- Partenaires : **assureurs**, **hôpitaux partenaires**.

### 4.4 Agents, wallets et retraits

- Profils **agents** par catégorie, quotas et encadrement.
- **Wallets** agent et virtuel, **mouvements** et soldes en temps réel.
- Workflow de **demande et validation de retrait** (jeton de retrait, fenêtres calendaires).
- **Retenue automatique MAASH** : prélèvement périodique configurable pour les catégories d’agents éligibles au produit MAASH.

### 4.5 Santé et bons

- **Jetons médicaux** et circuit **bons d’envoi** (demande → validation → émission).

### 4.6 Notifications et temps réel

- Notifications multi-canaux : **email** (SMTP), **SMS** (Twilio), **push** (Firebase).
- File d’attente asynchrone avec service hébergé en arrière-plan.
- Préférences utilisateur et agent par type de notification.
- **SignalR** pour la diffusion temps réel vers les clients connectés.

### 4.7 Mobile et synchronisation

- Configuration application mobile, sessions et données de sync.
- Synchronisation des utilisateurs et appareils (`UserDevice`).

### 4.8 Tableaux de bord

Vues agrégées dédiées par profil :

- Dashboard **administrateur**
- Dashboard **agent**
- Dashboard **affilié**
- Dashboard **financier**
- Dashboard **percepteur**
- Dashboard **superviseur**

### 4.9 Référentiels géographiques

Provinces, communes et **zones sociales** pour structurer le réseau territorial.

---

## 5. Workflows métier (vue synthétique)

```
┌─────────────┐     ┌──────────────┐     ┌─────────────┐     ┌──────────────┐
│  Adhésion   │ ──► │  Validation  │ ──► │  Collecte   │ ──► │ Commission   │
│  (terrain)  │     │  (régional)  │     │  (paiement) │     │  (agent)     │
└─────────────┘     └──────────────┘     └─────────────┘     └──────────────┘
                                                                    │
                                                                    ▼
                                                            ┌──────────────┐
                                                            │ Wallet /     │
                                                            │ Retrait      │
                                                            └──────────────┘
```

**Étapes types d’une adhésion :**

1. L’agent terrain saisit les informations et pièces du futur affilié.
2. L’opérateur régional complète ou corrige le dossier.
3. L’agent ou l’administrateur valide l’adhésion lorsque le dossier est complet.
4. Une collecte est enregistrée (frais d’adhésion, cotisation, etc.).
5. Les commissions sont calculées et créditées sur le wallet de l’agent.
6. Des notifications informent les parties prenantes à chaque étape clé.

---

## 6. Architecture technique (synthèse)

### 6.1 Organisation du code

| Couche | Rôle |
|--------|------|
| `Controllers/` | Exposition HTTP des ressources (~55 contrôleurs) |
| `Services/` | Logique métier et orchestration |
| `Models/` | Entités domaine, DTOs, configuration |
| `Data/` | `ProsocDbContext`, migrations EF Core, données de seed |
| `Hubs/` | Communication temps réel SignalR |
| `Middleware/` | Gestion des erreurs, rate limiting, CORS |
| `Prosoc.Tests.Unit/` | Tests unitaires (règles métier, services) |
| `Prosoc.Tests.Integration/` | Tests d’intégration API |

### 6.2 Stack technique

| Composant | Technologie |
|-----------|-------------|
| Runtime | .NET 6.0 |
| Framework web | ASP.NET Core Web API |
| Base de données | MySQL / MariaDB (Pomelo EF Core) |
| Authentification | JWT Bearer + refresh tokens (BCrypt pour les mots de passe) |
| Documentation API | Swagger / OpenAPI |
| Logs | Serilog (console, fichiers rotatifs, option MySQL) |
| Stockage fichiers | Amazon S3 |
| Notifications push | Firebase Admin SDK |
| SMS | Twilio |
| Limitation de débit | AspNetCoreRateLimit |
| Validation | FluentValidation |
| Export | EPPlus |
| Tests | xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing |

### 6.3 Principes d’architecture

- API **REST** avec pagination standardisée (offset et curseur).
- Pattern **service-as-repository** : les interfaces `I*Repository` sont implémentées par des services injectés via DI.
- Traitements **asynchrones** pour les notifications et la retenue MAASH (hosted services).
- Séparation claire entre **modèles de domaine**, **DTOs** et **modèles d’authentification**.

---

## 7. Intégrations et déploiement

### 7.1 Intégrations externes

- **Frontends web** : environnements test et développement (`testprosoc.kansaconsulting.com`, `devprosoc.kansaconsulting.com`) et application MAASH (`prosoc.maash.com`).
- **AWS S3** : stockage des fichiers uploadés (photos affiliés, certificats, etc.).
- **Firebase** : notifications push mobiles.
- **Twilio** : envoi de SMS (activable par configuration).
- **SMTP** : envoi d’emails transactionnels.

### 7.2 Environnements

| Environnement | Usage |
|---------------|-------|
| Développement local | Débogage, Swagger, base MySQL locale |
| Test / staging | Validation fonctionnelle avec frontends de test |
| Production | Exploitation mutualiste (URL selon déploiement client) |

La configuration applicative est centralisée dans `appsettings.json` (chaînes de connexion, JWT, CORS, intégrations). **Les secrets ne doivent jamais être versionnés** : utiliser des variables d’environnement ou un gestionnaire de secrets en production.

### 7.3 Sécurité

- Authentification **JWT** avec expiration et renouvellement par refresh token.
- Contrôle d’accès par **rôles et permissions**.
- **Rate limiting** par IP et par endpoint (protection login, réinitialisation mot de passe, batch).
- **CORS** restreint aux origines autorisées.
- Journalisation structurée des erreurs et événements sensibles.

---

## 8. Qualité et maintenance

### 8.1 Tests

Le projet inclut des projets de tests dédiés couvrant notamment :

- Règles de tarification produit (`ProduitTarifRules`)
- Éligibilité et souscription
- Commissions et cotisations affilié
- Retraits agent

### 8.2 Migrations de base de données

Le schéma est versionné via **Entity Framework Core Migrations** (`Migrations/`). Toute évolution du modèle doit passer par une migration nommée et testée avant déploiement.

### 8.3 Observabilité

- Logs applicatifs Serilog (niveau Information par défaut, Warning+ en base selon config).
- Fichiers de logs journaliers dans `logs/` (conservation configurable).

---

## 9. Documentation associée

Le dépôt contient plusieurs documents complémentaires selon le besoin :

| Document | Contenu |
|----------|---------|
| `DESCRIPTIF_PROJET_PROSOC.md` | **Ce document** — présentation générale du projet |
| `DOCUMENTATION_COMPLETE_PROSOCAPI.md` | Cartographie technique détaillée (modèles, services, contrôleurs) |
| `API-DOCUMENTATION-NEW.md` | Guide développeur API (endpoints, exemples, auth) |
| `Documentation/PaiementAffilie_TechnicalSpec.md` | Spécification module paiement affilié |
| `Rapport de Réunion Technique_Workflow.md` | Workflows métier validés en réunion |
| Swagger (`/swagger`) | Référence interactive des endpoints |

---

## 10. État actuel et évolutions

### 10.1 Maturité

ProsocAPI couvre un **périmètre métier large** : la majorité des modules cœur (adhésion, collecte, commission, wallet, dashboards, notifications) sont implémentés et opérationnels. Des évolutions récentes portent notamment sur les **cotisations affilié**, la **tarification produit**, les **fichiers binaires affilié** et l’**intégration MAASH** (retenue automatique agents).

### 10.2 Axes d’évolution identifiés

- Harmonisation du **versioning API** sur l’ensemble des contrôleurs.
- Renforcement de la **couverture de tests** sur les parcours critiques.
- Consolidation documentaire (réduire la redondance entre fichiers `.md`).
- Évaluation d’une **file de notifications persistante** pour la résilience en production.
- Poursuite des fonctionnalités métier listées dans `TODO_FONCTIONNALITES_FUTURES.md`.

---

## 11. Contacts et exploitation

- **Organisation :** Kansa Consulting / plateforme Prosoc–MAASH  
- **Dépôt :** `ProsocAPI` (solution .NET `Prosoc.sln`)  
- **Démarrage local :** `dotnet run` puis accès Swagger sur le port configuré  
- **Support technique :** équipe développement ProsocAPI  

---

*Ce descriptif peut être adapté (logo, chiffres d’affaires, calendrier de déploiement, contacts nominatifs) selon le public visé : direction, partenaires, appels d’offres ou onboarding développeurs.*
