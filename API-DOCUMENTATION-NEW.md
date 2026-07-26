# 📚 Documentation API Prosoc v2.1
> *Système de Gestion Mutualiste Moderne* ![Version](https://img.shields.io/badge/version-2.1-blue) ![Status](https://img.shields.io/badge/status-Production-green)

---

## 🎯 Vue d'ensemble

### 🚀 Caractéristiques principales
- **Architecture RESTful** avec pagination universelle
- **Authentification JWT** avec refresh tokens  
- **Commissionnement automatique** (taux configurable par `Frais`/`Produit`)
- **Wallets multiples** (Agent + Virtuel)
- **Workflows métier** complets (Adhésion → Collecte → Commission)
- **Support multi-modes** de paiement (Mobile Money, VIRTUAL_ACCOUNT, etc.)

### 📊 Statistiques actuelles
- **45+ endpoints** opérationnels
- **12 contrôleurs** principaux  
- **Mise à jour** : Mars 2026
- **Base URL** : `https://dev-prosoc.asdc-rdc.org`

### 🎯 Public cible
- **Développeurs Frontend** : React, Vue.js, Angular
- **Développeurs Mobile** : Flutter, React Native
- **Intégrateurs** : Partenaires techniques
- **Administrateurs système** : Monitoring et gestion

---

## 🚀 Getting Started

### 📋 Prérequis techniques
- **Runtime** : .NET 6.0+
- **Base de données** : MySQL 8.0+
- **Authentification** : JWT Bearer Token
- **Formats supportés** : JSON (application/json)

### 🔧 Configuration rapide

#### 🌍 URLs d'environnement
| Environnement | Base URL | Swagger UI |
|---------------|-----------|------------|
| Production | `https://dev-prosoc.asdc-rdc.org` | [Swagger](https://dev-prosoc.asdc-rdc.org/swagger) |
| Local | `https://localhost:7116` | [Swagger](http://localhost:7116/swagger) |
| Staging | `https://staging-prosoc.asdc-rdc.org` | [Swagger](https://staging-prosoc.asdc-rdc.org/swagger) |

#### 📋 En-têtes requis
```http
Content-Type: application/json
Authorization: Bearer {votre_token_jwt}
Accept: application/json
```

### 🎯 Votre premier appel

#### 🔐 Obtenir un token d'accès
```bash
curl -X POST "https://dev-prosoc.asdc-rdc.org/api/utilisateur/login" \
  -H "Content-Type: application/json" \
  -d '{
    "nomUtilisateur": "admin@prosoc.cd",
    "motDePasse": "votre_mot_de_passe"
  }'
```

#### 📤 Tester l'authentification
```bash
curl -X GET "https://dev-prosoc.asdc-rdc.org/api/utilisateur/me" \
  -H "Authorization: Bearer {votre_token}"
```

---

## 🔐 Authentification

### 🎯 Vue d'ensemble du système d'authentification
L'API utilise un système JWT (JSON Web Tokens) avec :
- **Access Token** : 1 heure de validité
- **Refresh Token** : 7 jours de validité  
- **Support multi-identifiants** : Email, Téléphone, Nom d'utilisateur

### POST /api/utilisateur/login
Permet d'obtenir un token JWT pour l'accès à l'API.

#### 📋 Corps de la requête
```json
{
  "nomUtilisateur": "admin@prosoc.cd",
  "motDePasse": "votre_mot_de_passe"
}
```

**Note** : Le champ `nomUtilisateur` accepte :
- Adresse email : `user@domain.com`
- Numéro téléphone : `+243XXXXXXXXX`
- Nom d'utilisateur : `username`

#### 📤 Réponse réussie (200 OK)
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkFkbWluIiwicm9sZSI6IkFkbWluIiwiZXhwIjoxNzQ0NTY3ODkwLCJpc3MiOiJodHRwczovL2xvY2FsaG9zdDo4MDgwL2FwaS9hdXRoL2xvZ2luIiwiaWF0IjoiaHR0cHM6Ly9sb2NhbGhvc3Q6ODA4MC9hcGkvYXV0aC9sb2dpbiJ9.NQrYz7H9oLmX_q8jWkqJhKxMwYpZl4xQaM8kRw",
  "expiresAtUtc": "2026-03-16T20:00:00Z",
  "refreshToken": "def50200-1a4b-4c7c-8d9b-5f9b8c7e2f3a",
  "utilisateur": {
    "idUtilisateur": 1,
    "referenceUtilisateur": "ADMIN001",
    "nomComplet": "Admin User",
    "nomUtilisateur": "admin",
    "emailUtilisateur": "admin@prosoc.cd",
    "phoneUtilisateur": "+243999999999",
    "photoUrl": null,
    "genre": "M",
    "statut": true,
    "dateCreation": "2026-03-03T20:56:51.622409",
    "isConnecte": false,
    "doitChangerMotDePasse": false,
    "agentId": 1,
    "affilieId": null
  }
}
```

#### 🔴 Codes d'erreur
| Code HTTP | Message d'erreur | Cause | Solution |
|-----------|------------------|-------|---------|
| 401 | "Identifiants invalides" | Nom utilisateur ou mot de passe incorrect | Vérifier les identifiants |
| 429 | "Trop de tentatives de connexion" | Rate limiting activé | Attendre 15 minutes |
| 500 | "Erreur interne du serveur" | Problème technique | Contacter le support |

### POST /api/utilisateur/refresh
Rafraîchit le access token en utilisant le refresh token.

#### 📋 Corps de la requête
```json
{
  "refreshToken": "def50200-1a4b-4c7c-8d9b-5f9b8c7e2f3a"
}
```

#### 📤 Réponse réussie
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAtUtc": "2026-03-16T21:00:00Z"
}
```

### GET /api/utilisateur/me
Récupère les informations de l'utilisateur connecté.

#### 📤 Réponse réussie
```json
{
  "idUtilisateur": 1,
  "referenceUtilisateur": "ADMIN001",
  "nomComplet": "Admin User",
  "nomUtilisateur": "admin",
  "emailUtilisateur": "admin@prosoc.cd",
  "phoneUtilisateur": "+243999999999",
  "photoUrl": "https://storage.prosoc.cd/photos/admin.jpg",
  "genre": "M",
  "statut": true,
  "agentId": 1,
  "affilieId": null
}
```

### Permissions JWT — module Collecte

Les claims `permission` du JWT contrôlent l’affichage des actions côté front (ex. bouton « modifier une collecte »).

| Permission | Description | Rôles opérationnels (AT, AA, Superviseur, Percepteur, Caissier, Financier) | Admin / SuperAdmin | IT |
|------------|-------------|----------------------------------------------------------------------------|--------------------|-----|
| `CREATE_COLLECTE` | Enregistrer une collecte | ✅ | ✅ | ❌ |
| `READ_COLLECTE` | Consulter les collectes | ✅ | ✅ | ✅ |
| `UPDATE_COLLECTE` | Modifier une collecte existante | ❌ | ❌ (retirée) | ❌ |

**Règle métier** : les rôles terrain peuvent **créer et consulter** les collectes, pas les corriger en place. **Aucun rôle** ne dispose de `UPDATE_COLLECTE` : `PUT /api/Collecte/{id}` renvoie **403**. Le champ `statutPaiement` n'accepte que **`EN_ATTENTE`** (FlexPay en cours) ou **`VALIDE`** (paiement terminé). `POST /api/DashboardAdmin/validate-collecte/{id}` force `VALIDE` (idempotent, rétrocompat).

> **Reconnexion obligatoire** : après déploiement, exécuter `sql/MigrateRemoveUpdateCollectePermission.idempotent.sql` (ou redémarrer l’app pour la migration seed), puis **déconnecter / reconnecter** tous les comptes pour purger le claim `UPDATE_COLLECTE` du JWT.

### Visibilité hiérarchique — `GET /api/Agent`

Les listes et le détail agent (`GET /api/Agent`, `/paginated`, `POST /advanced`, `GET /{id}`) sont filtrés selon `Role.Niveau` du caller (rôles JWT) :

- plus le **chiffre** `Niveau` est **petit**, plus le rôle est **haut** (SuperAdmin `0` … Agent AT `7`) ;
- un utilisateur ne voit que les agents dont `MIN(Niveau des rôles liés)` **≥** `MIN(Niveau du caller)` ;
- multi-rôles : le **meilleur** rôle (MIN numérique) détermine la visibilité ;
- agents **sans** utilisateur/rôle : exclus, sauf pour SuperAdmin (`Niveau == 0`) ;
- `GET /{id}` hors périmètre → **404**.

Orthogonal au filtre territorial (zone / commune).

`AgentReadDto` expose aussi la catégorie :

| Champ | Description |
|-------|-------------|
| `categorieAgentId` | FK catégorie (`null` si non renseignée) |
| `categorieAgentCode` | Code court (ex. `AT`) |
| `categorieAgentDescription` | Description de la catégorie |

### Permissions JWT — espace membre Affilié

Rôle JWT : **`Affilié`** (avec accent). Le membre connecté ne voit **que son dossier** : fiche personnelle, personne de contact, adhésion, dépendants et antécédents qui lui sont rattachés.

> **Reconnexion obligatoire** : après déploiement, exécuter `sql/MigrateAffilieRolePermissions.idempotent.sql` (ou redémarrer l’app pour la migration seed), puis **déconnecter / reconnecter** les comptes Affilié pour obtenir un JWT à jour.

#### Permissions retirées du rôle Affilié

| Permission | Raison |
|------------|--------|
| `READ_AFFILIE` | Pas de liste globale des affiliés |
| `READ_ADHESION` | Pas de liste globale des adhésions |

#### Permissions conservées (extrait)

| Permission | Usage membre |
|------------|--------------|
| `UPDATE_AFFILIE` | Modifier **son** profil |
| `READ_DEPENDANT`, `CREATE_DEPENDANT`, `UPDATE_DEPENDANT` | Gérer **ses** dépendants |
| `READ_ANTECEDENT`, `CREATE_ANTECEDENT`, `UPDATE_ANTECEDENT` | Gérer **ses** antécédents |
| `ACCESS_DASHBOARD_AFFILIE` | Tableau de bord (son `affilieId` uniquement) |
| `PAIEMENT_AFFILIE`, `READ_SOUSCRIPTION_PRESTATION`, … | Paiements et catalogue |

#### Règles API (403 Forbidden)

| Action réservée au personnel | Endpoints bloqués pour le membre |
|------------------------------|----------------------------------|
| Listes globales | `GET /api/Affilie`, `GET /api/Adhesion`, `GET /api/Adhesion/paginated`, `POST /api/Adhesion/advanced`, `GET /api/Dependant`, `POST /api/Dependant/advanced`, `GET /api/Antecedent`, `POST /api/Antecedent/advanced` |
| Encodage / admin adhésion | `GET /api/Adhesion/{id}/fiche-encodeur`, `PUT /api/Adhesion/{id}/niveau-2-encodeur`, `DELETE /api/Adhesion/{id}`, `PUT /api/Adhesion/UpdateWithAffilieAsync/{id}` |
| Données d’un autre affilié | Toute route avec un `affilieId` / `id` ne correspondant pas au JWT (`AffilieId` claim ou fiche utilisateur) |

#### Endpoints dédiés espace membre

| Endpoint | Description |
|----------|-------------|
| `GET /api/Affilie/mon-profil` | Fiche affilié + **personne de contact** + synthèse adhésion |
| `GET /api/Adhesion/mon-adhesion` | Dossier adhésion complet du membre |
| `GET /api/Dependant/mes-dependants` | Dépendants paginés du membre |
| `GET /api/Antecedent/mes-antecedents` | Antécédents paginés du membre (titulaire + dépendants) |
| `GET /api/Affilie/{id}/antecedants` | Antécédents paginés d’un affilié (membre: uniquement son id ; staff: `READ_ANTECEDENT`) |
| `GET /api/Affilie/{id}/dependants` | Dépendants paginés d’un affilié (membre: uniquement son id ; staff: `READ_DEPENDANT`) |
| `GET /api/Dependant/{id}/antecedants` | Antécédents paginés d’un dépendant (membre: uniquement ses dépendants ; staff: `READ_ANTECEDENT`) |
| `GET /api/DashboardAffilie/*/{affilieId}` | Dashboard — `{affilieId}` **doit** être celui du membre |

Le front doit résoudre `affilieId` depuis le JWT (`AffilieId` ou `GET /api/utilisateur/me` → `affilieId`) et **ne plus appeler** les listes globales pour ce rôle.

#### Exemple — `GET /api/Affilie/mon-profil`

```http
GET /api/Affilie/mon-profil
Authorization: Bearer {token_affilie}
```

```json
{
  "affilie": {
    "idAffilie": 456,
    "codeAdhesion": "F3-2026-KIN-001",
    "nom": "Kabila",
    "prenom": "Marie",
    "nomComplet": "Marie Kabila",
    "telephone": "+243999888777",
    "hasPhoto": true,
    "hasCarteIdentite": true,
    "photoBase64": "<base64>",
    "photoUrl": "<base64 — même valeur que photoBase64>",
    "carteIdentiteBase64": "<base64>",
    "statut": true
  },
  "personneContact": {
    "idPersonneContact": 12,
    "affilieId": 456,
    "nomComplet": "Paul Kabila",
    "lienParente": "Époux",
    "adresse": "Kinshasa, Selembao",
    "statut": true
  },
  "adhesionId": 789,
  "statutDossier": "VALIDÉ",
  "typeAdhesion": "F3"
}
```

---

## Référentiels

### GET /api/referentiel/liens-parente

**Accès** : public (`AllowAnonymous`).

Retourne la liste officielle des liens de parenté pour aligner les formulaires frontend (personne de contact, dépendants, bénéficiaires Maash).

**Réponse** :

```json
{
  "liens": [
    { "code": "PERE", "libelle": "Père", "categorie": "ASCENDANT" },
    { "code": "EPOUSE", "libelle": "Épouse", "categorie": "CONJOINT" },
    { "code": "FILS", "libelle": "Fils", "categorie": "ENFANT" }
  ],
  "liensEnfant": ["ENFANT", "FILS", "FILLE"],
  "liensConjoint": ["CONJOINT", "EPOUSE", "EPOUX", "MARI", "FEMME", "MARIE"]
}
```

| Champ | Description |
|-------|-------------|
| `liens[].code` | Valeur à envoyer dans `lienParente` (recommandé) |
| `liens[].libelle` | Libellé d'affichage français |
| `liens[].categorie` | `ASCENDANT`, `FAMILLE_ELARGIE`, `FRATRIE`, `CONJOINT`, `ENFANT`, `AUTRE_CONTACT`, `AUTRE` |
| `liensEnfant` | Codes déclenchant règles enfant (certificat scolarité 18–25 ans) |
| `liensConjoint` | Codes conjoint (âge min. 15 ans si date de naissance) |

**23 codes** : `PERE`, `MERE`, `GRAND_PERE`, `GRAND_MERE`, `ONCLE`, `TANTE`, `FRERE`, `SOEUR`, `COUSIN`, `COUSINE`, `CONJOINT`, `EPOUSE`, `EPOUX`, `MARI`, `FEMME`, `MARIE`, `ENFANT`, `FILS`, `FILLE`, `AMI`, `VOISIN`, `COLLEGUE`, `AUTRE`.

L'API accepte aussi les libellés français (`Épouse`, `Fils`, `Conjoint(e)`…) ; la valeur persistée est toujours le `code` normalisé.

---

## 📋 Gestion des Adhésions

### 🎯 Vue d'ensemble du workflow d'adhésion

```mermaid
graph TD
    A[Demande d'adhésion] --> B[Validation des données]
    B --> C[Création affilié]
    C --> D[Création utilisateur]
    D --> E[Génération code adhésion]
    E --> F[Traitement collectes]
    F --> G[Commissionnement]
    F --> H[Débit wallet virtuel]
    G --> I[Crédit wallet agent]
    H --> J[Création souscriptions]
    J --> K[Adhésion complète]
```

### 🎯 POST /api/adhesion/with-affilie
**Endpoint principal** pour créer une adhésion complète avec affilié, collectes, souscriptions et dépendants.

#### 📌 Niveau 1 — Agent de Terrain (AT) — champs obligatoires

| Bloc | Champ API | Obligatoire |
|------|-----------|-------------|
| **Âge titulaire** | `dateNaissance` — entre **18 et 54 ans** inclus | Oui |
| | À partir de **55 ans** : pas d'adhésion en titulaire (personne à charge uniquement) | — |
| **Identité** | `nom`, `prenom` (+ `postnom` optionnel) | Oui |
| | `emailAffilie` | Optionnel (unicité vérifiée si renseigné) |
| | `photoBase64` + `photoContentType` | Recommandé (optionnel niveau 1 ; max 1 Mo, BLOB en base) |
| | `carteIdentiteBase64` + `carteIdentiteContentType` | Recommandé (optionnel niveau 1 ; image ou PDF, max 1 Mo) |
| **Adresse résidence** | `provinceResidence` | Oui (utilisée pour le code adhésion) |
| | `communeResidence`, `quartierResidence`, `avenueResidence`, `numeroResidence` | Optionnels (complétion recommandée au niveau 2 encodeur) |
| **Lieu d'activité** | `communeActivite`, `quartierActivite`, `avenueActivite`, `numeroActivite` | Optionnels |
| **Souscription & collecte** | `collectes[]` avec au moins une entrée (`Frais` seuls OK ; `Souscription` / `Cotisation` optionnelles) | Oui |
| | `collectes[].souscription.prestationId` si `Souscription` | Oui (si ligne Souscription présente) |
| | `collectes[].statutPaiement` = `VALIDE` sur au moins une ligne | Oui (sauf flux FlexPay : voir section FlexPay) |

| **Personne de contact** | `personneContact.nomComplet`, `lienParente`, `adresse` | Optionnel (si fourni, les 3 champs sont requis) |
| **Personnes à charge** | `dependants[]` | Optionnel (voir aussi niveau 2 encodeur) |

La **validation** du dossier (`VALIDÉ`) relève du **niveau 2** (encodeur AA).

#### StatutDossier (valeurs canoniques)

Deux valeurs uniquement (écriture normalisée côté API) :

| Canon | Sens |
|-------|------|
| `EN ATTENTE` | Dossier à compléter / encoder — seul état accepté par `PUT …/niveau-2-encodeur` et `PUT …/UpdateWithAffilieAsync/{id}` |
| `VALIDÉ` | Dossier validé (`valider: true` au niveau 2) — requis pour éligibilité bon d’envoi / KPIs AA « dossier validé » |

**Création** (`with-affilie`, etc.) : le statut client est ignoré → toujours `EN ATTENTE`.  
**UpdateWithAffilie** : ne modifie plus `statutDossier` (transition uniquement via niveau 2).  
**Legacy** (`COMPLET`, `VALIDE`, `A`, `En Attente`, …) : migrés / lus via `AdhesionStatutDossierRegles` (`sql/MigrateAdhesionStatutDossierCanonical.idempotent.sql`).

#### 📌 Niveau 2 — Agent Administratif / Encodeur

Saisie des informations de la fiche papier pour un dossier **`EN ATTENTE`** (créé au niveau 1).

**Dossier complet** (prérequis de `valider: true` → `StatutDossier = VALIDÉ`) — les **4 blocs** suivants doivent être réunis :

| # | Bloc | Critères |
|---|------|----------|
| 1 | **Identité affilié** | `nom`, `prenom`, `dateNaissance` (en base et/ou complétés dans le PUT) |
| 2 | **Adresse activité** | `communeActivite` + `quartierActivite` (avenue / numéro optionnels) |
| 3 | **Photo + pièce d'identité** | présentes en base ou fournies (`photoBase64` / `carteIdentiteBase64`) |
| 4 | **Personne à contacter** | `nomComplet` + `lienParente` + `adresse` (body ou déjà en base) |

Les **personnes à charge** restent **optionnelles**. Sans `valider: true`, le PUT peut compléter progressivement (statut reste `EN ATTENTE`).

| Bloc | Champs | Obligatoire |
|------|--------|-------------|
| **Identité / activité** | `nom`, `prenom`, `postnom`, `telephone`, `dateNaissance`, `communeActivite`, `quartierActivite`, … | Pour **valider** : identité + commune/quartier activité |
| **Personnes à charge** | `dependants[]` | Liste optionnelle ; règles Remarque 4 ci-dessous |
| **Personne de contact** | `personneContact.nomComplet`, `lienParente`, `adresse` | Oui pour valider (pré-saisie possible au niveau 1) |
| **Photo / carte d'identité** | `photoBase64` + `photoContentType`, `carteIdentiteBase64` + `carteIdentiteContentType` | **Obligatoires si `valider: true`** |
| **Validation** | `valider: true` | Passe le dossier à `VALIDÉ` si les 4 blocs sont complets |

**Endpoints :**

- `GET /api/adhesion/{id}/fiche-encodeur` — lire la fiche (`hasPhoto`, `hasCarteIdentite`, `identiteComplete`, `adresseActiviteComplete`, `dossierComplet`, …)
- `PUT /api/adhesion/{id}/niveau-2-encodeur` — enregistrer et éventuellement valider

```json
{
  "personneContact": {
    "nomComplet": "Marie Kabila",
    "lienParente": "EPOUSE",
    "adresse": "Kinshasa, Selembao, Sans-fil, av. Lukunga 12"
  },
  "communeActivite": "Gombe",
  "quartierActivite": "Centre",
  "avenueActivite": "av. Commerce",
  "numeroActivite": "10",
  "dependants": [
    {
      "nomComplet": "Jean Kabila Jr",
      "lienParente": "FILS",
      "adresse": "Kinshasa, Selembao, Sans-fil",
      "dateNaissance": "2010-05-12"
    }
  ],
  "photoBase64": "<base64 image>",
  "photoContentType": "image/jpeg",
  "carteIdentiteBase64": "<base64 image ou PDF>",
  "carteIdentiteContentType": "image/jpeg",
  "valider": true
}
```

`GET /api/adhesion/{id}/fiche-encodeur` expose aussi l'identité et l'adresse d'activité courantes pour le front encodeur.

Liens de parenté acceptés : voir `GET /api/referentiel/liens-parente` (23 codes canoniques + libellés). L’API accepte aussi les libellés français (`Épouse`, `Fils`, `Conjoint(e)`…) normalisés à l’enregistrement (`LienParenteRegles`).

#### Remarque 4 — Personnes à charge et âge d'adhésion

| Règle | Détail |
|-------|--------|
| **Titulaire** | Adhésion possible uniquement si **18 ≤ âge ≤ 54** ans (`dateNaissance`). |
| **55 ans et plus** | Ne peut **pas** adhérer en titulaire — doit être déclaré **personne à charge** d'un autre affilié. |
| **Enfant 0–17 ans** | Accepté sans justificatif (liens `ENFANT`, `FILS`, `FILLE`). |
| **Enfant 18–25 ans** | Accepté **uniquement** avec **certificat de scolarité** (image ou PDF, max 1 Mo). |
| **Enfant > 25 ans** | Refusé comme personne à charge enfant. |

Lecture du certificat : `GET /api/dependant/{id}/certificat-scolarite`  
Les mêmes règles s'appliquent aux `dependants[]` de `POST /api/adhesion/with-affilie` et à `PUT /api/adhesion/{id}/niveau-2-encodeur`.

Codes d'erreur : `VALIDATION_AGE_MINIMUM`, `VALIDATION_AGE_MAXIMUM_ADHERENT`, messages regroupés sous validation dépendants.

#### 🔄 Workflow complet
1. **Validation** des données d'entrée avec règles métier
2. **Création** de l'affilié et de son compte utilisateur automatique
3. **Génération** du code d'adhésion unique (format: TYPE-ANNEE-PROVINCE-NUMERO)
4. **Traitement** des collectes avec commissionnement automatique (taux dynamique)
5. **Débit** automatique du wallet virtuel si mode `VIRTUAL_ACCOUNT`
6. **Création** des souscriptions et dépendants avec validation croisée
7. **Transaction** atomique : tout est validé ou rien n'est sauvegardé

#### 📋 Corps de la requête
```json
{
  "nom": "kasongo",
  "prenom": "billy", 
  "postnom": "Ntumba",
  "dateNaissance": "1980-02-27T09:08:53.467Z",
  "telephone": "+24384 8109394",
  "emailAffilie": "billykasongo80@gmail.com",
  "provinceResidence": "Kinshasa",
  "communeResidence": "Selembao",
  "quartierResidence": "Sans-fil",
  "avenueResidence": "Lukunga", 
  "numeroResidence": "50",
  "photoBase64": "<base64 de l'image>",
  "photoContentType": "image/jpeg",
  "carteIdentiteBase64": "<base64 de la carte>",
  "carteIdentiteContentType": "image/jpeg",
  "affilieStatut": true,
  "statutDossier": "En Attente",
  "typeAdhesionId": 1,
  "agentId": 3,
  "adhesionStatut": true,
  "personneContact": {
    "nomComplet": "Marie Kabila",
    "lienParente": "EPOUSE",
    "adresse": "Kinshasa, Selembao, Sans-fil, av. Lukunga 12"
  },
  "collectes": [
    {
      "typeCollecte": "Frais",
      "fraisId": 1,
      "montant": 1.5,
      "mois": 3,
      "annee": 2026,
      "modePaiement": "VIRTUAL_ACCOUNT",
      "statutPaiement": "VALIDE",
      "montantRecu": 1.5,
      "montantAttendu": 1.5,
      "deviseId": 2,
      "statut": true
    },
    {
      "typeCollecte": "Souscription",
      "souscription": {
        "prestationId": 1,
        "statut": true
      },
      "fraisId": null,
      "montant": 5,
      "mois": 3,
      "annee": 2026,
      "modePaiement": "MOBILE_MONEY",
      "referencePaiement": "REF-MOBILE-001",
      "statutPaiement": "VALIDE",
      "montantRecu": 5,
      "montantAttendu": 5,
      "deviseId": 2,
      "statut": true
    },
    {
      "typeCollecte": "Souscription", 
      "souscription": {
        "prestationId": 2,
        "statut": true
      },
      "fraisId": null,
      "montant": 10,
      "mois": 3,
      "annee": 2026,
      "modePaiement": "VIRTUAL_ACCOUNT",
      "statutPaiement": "VALIDE",
      "montantRecu": 10,
      "montantAttendu": 10,
      "deviseId": 2,
      "statut": true
    }
  ],
  "dependants": [],
  "antecedants": []
}
```

#### GET /api/adhesion/{id}

Retourne l'adhésion complète (`AdhesionWithAffilieReadDto`) : affilié, collectes, souscriptions, dépendants, antécédents, personne de contact.

**Espace membre Affilié** : utiliser de préférence `GET /api/Adhesion/mon-adhesion`. L'accès à `GET /api/Adhesion/{id}` est autorisé uniquement si `{id}` appartient à l'affilié connecté ; sinon **403**. Les listes `GET /api/Adhesion`, `paginated` et `advanced` renvoient **403** pour ce rôle.

#### GET /api/Adhesion — liste paginée et paramètre `Search`

Endpoints concernés : `GET /api/Adhesion`, `GET /api/Adhesion/paginated`, `GET /api/Adhesion/en-ligne-sans-gestionnaire`.

Paramètres de pagination communs (`PaginationRequest`) :

| Paramètre | Type | Description |
|-----------|------|-------------|
| `Page` | int | Numéro de page (défaut : 1) |
| `PageSize` | int | Taille de page (défaut : 20, max : 100) |
| `SortBy` | string | Champ de tri sur l'entité `Adhesion` |
| `SortDirection` | string | `asc` ou `desc` |
| `Search` | string | Recherche textuelle (max 100 caractères, insensible à la casse) |
| `Filters` | string (JSON) | Filtres génériques sur les propriétés directes de `Adhesion` |

**Comportement de `Search`** : recherche en sous-chaîne (`Contains`) avec logique **OR** sur les champs suivants :

| Champ | Exemple de terme |
|-------|------------------|
| `IdAdhesion` | `42` (si le terme est un entier) |
| `AffilieId` | `15` (si le terme est un entier) |
| `StatutDossier` | `VALID`, `ATTENTE`, `A` |
| `Affilie.NomComplet` | `Jean Mukendi` |
| `Affilie.Nom`, `Prenom`, `Postnom` | `Mukendi` |
| `Affilie.CodeAdhesion` | `PROSOC-2024-001` |
| `Affilie.Telephone` | `0812345678` |
| `Affilie.EmailAffilie` | `jean@example.cd` |
| `TypeAdhesion.Libelle` | `F3`, `Solo` |

Exemple :

```http
GET /api/Adhesion?page=1&pageSize=20&Search=mukendi
Authorization: Bearer {token}
```

Réponse : `PaginatedResponse<AdhesionReadDto>` (métadonnées de pagination inchangées).

**Note** : contrairement à `GET /api/Affilie` (projection vers `AffilieReadDto`), la recherche adhésion inclut explicitement les données affilié et type d'adhésion liés. Le paramètre `Filters` (JSON) reste limité aux propriétés scalaires directes de `Adhesion` ; pour des filtres métier typés, utiliser `POST /api/Adhesion/advanced`.

| Bloc | Champs notables |
|------|-----------------|
| `affilie` | `nom`, `prenom`, `telephone`, **`emailAffilie`**, adresses, `hasPhoto`, `hasCarteIdentite` |
| `personneContact` | Présent si saisi au niveau 1 ou par l'encodeur |
| `collectes`, `dependants`, `antecedants` | Listes associées à l'affilié |

#### 🎯 Validation des données
| Champ | Règle de validation | Exemple |
|-------|-------------------|---------|
| `photoBase64` / `carteIdentiteBase64` | Optionnels niveau 1 ; obligatoires pour `valider: true` niveau 2 ; max 1 Mo chacun | Base64 ou `data:image/jpeg;base64,...` |
| Lecture fichiers | `GET /api/affilie/{id}/photo`, `GET /api/affilie/{id}/carte-identite` | Retourne le binaire |
| Multipart (alternative) | `POST /api/adhesion/with-affilie-multipart` | `payload` (JSON) + `photo` et `carteIdentite` optionnels |
| `communeResidence` / `quartierResidence` | Obligatoires | `"Selembao"` |
| `typeCollecte` | `Frais`, `Souscription` ou `Cotisation` | `"Cotisation"` |
| `collectes` (niveau 1) | Au moins une collecte + paiement confirmé (sauf FlexPay en attente). **FRAIS seuls** autorisés ; `Souscription` / `Cotisation` optionnelles | `FRAIS` seuls, ou `FRAIS` + `SOUSCRIPTION`(s) / `COTISATION` |
| Cotisation à l'adhésion | **Facultative** : lot `FRAIS` + `SOUSCRIPTION(s)` sans ligne `Cotisation` ; cotisation périodique payable plus tard (collecte agent, arriérés) | voir éligibilité produits |
| `modePaiement` | Doit être dans la liste des modes valides | `"VIRTUAL_ACCOUNT"` |
| `referencePaiement` | Obligatoire sauf pour VIRTUAL_ACCOUNT | `"REF-001"` |
| `souscription.prestationId` | Requis si typeCollecte = "Souscription" | `1` |
| `cotisationAffilieId` | Requis si typeCollecte = "Cotisation" ; doit correspondre au `typeAdhesionId` | `1` |
| `montant` (cotisation) | `montantUnitaire × (1 + nombre de dépendants)` | F3 + 3 dépendants, 5/pers. → `20` |

#### 💰 Cotisation affilié (grille tarifaire)

**CRUD catalogue explicite** : `/api/TarifCotisation`  
**Calcul préalable** : `GET /api/TarifCotisation/{id}/montant-total?nombreDependants=3`  
**Lookup par type adhésion** : `GET /api/TarifCotisation/type-adhesion/{typeAdhesionId}`  
**Lookup par affilié** : `GET /api/TarifCotisation/Affilie?idAffilie={id}`

```json
{
  "cotisationAffilieId": 1,
  "typeAdhesionId": 2,
  "typeAdhesionLibelle": "F3",
  "periodicite": "Mensuel",
  "montantUnitaire": 5.00,
  "nombreDependants": 3,
  "nombrePersonnes": 4,
  "montantTotal": 20.00
}
```

Exemple de collecte cotisation dans `collectes[]` :

```json
{
  "typeCollecte": "Cotisation",
  "cotisationAffilieId": 1,
  "montant": 20.00,
  "mois": 5,
  "annee": 2026,
  "modePaiement": "ESPECE",
  "referencePaiement": "REF-COT-001",
  "statutPaiement": "VALIDE",
  "deviseId": 2,
  "statut": true
}
```

### Clarification domaine `CotisationAffilie`

- `CotisationAffilie` est un **catalogue de tarifs** (grille de référence) par `TypeAdhesionId` et `Periodicite`.
- `POST /api/TarifCotisation` est la version catalogue explicite: le payload exige `typeAdhesionId` et n'accepte pas `idAffilie`.
- Les **paiements réels** d'un affilié passent par `Collecte` avec `TypeCollecte = Cotisation`.
- La génération des arriérés (`ArrieresAffilie`) s'appuie sur cette grille tarifaire.

Règle de cohérence:

- La mise à jour d'un tarif peut être bloquée si des arriérés non soldés existent déjà sur ce tarif, afin de préserver l'historique de calcul.

### Exemple front: tarif puis paiement affilié (2 appels)

#### 1) Créer le tarif de référence (catalogue)

`POST /api/TarifCotisation`

```json
{
  "typeAdhesionId": 1,
  "periodicite": "Mensuel",
  "montant": 5.0,
  "statut": true
}
```

Réponse attendue: tarif créé avec `id` (ex. `id=12`).

#### 2) Enregistrer le paiement d'un affilié

`POST /api/Collecte`

```json
{
  "typeCollecte": "Cotisation",
  "affilieId": 3,
  "agentId": 1,
  "cotisationAffilieId": 12,
  "montant": 20.0,
  "mois": 5,
  "annee": 2026,
  "modePaiement": "MOBILE_MONEY",
  "statutPaiement": "VALIDE",
  "deviseId": 1,
  "statut": true
}
```

Notes:

- Permissions requises : `CREATE_COLLECTE` (JWT). La modification (`PUT /api/Collecte/{id}`) est **interdite** pour tous les rôles — voir section [Permissions JWT — module Collecte](#permissions-jwt--module-collecte). Guichet : `statutPaiement` normalisé en `VALIDE` si omis ou legacy.
- `montant` côté collecte doit être le montant total attendu (souvent `montantUnitaire × (1 + nombreDependants)`).
- Vous pouvez calculer ce montant via `GET /api/TarifCotisation/{id}/montant-total?nombreDependants=N`.
- **Souscription / période** : si la somme des collectes `VALIDE` pour la même `souscriptionPrestationId` + `mois` + `annee` atteint déjà le tarif produit, un nouveau paiement est refusé (`400`, message contenant `DEJA_PAYEE_PERIODE`). Les paiements partiels restent autorisés tant que le total n’est pas atteint. Canaux : `POST /api/Collecte`, FlexPay collecte, `POST /api/Affilie/paiement`.
- C’est ce paiement `Collecte` qui impute/régularise l’arriéré, pas le `POST /api/TarifCotisation`.

#### 📤 Réponse réussie (201 Created)
```json
{
  "idAdhesion": 123,
  "codeAdhesion": "KD-26-KIN-001",
  "statutDossier": "Actif",
  "affilie": {
    "idAffilie": 456,
    "codeAdhesion": "KD-26-KIN-001",
    "nomComplet": "kasongo Ntumba billy",
    "dateNaissance": "1980-02-27T09:08:53.467Z",
    "telephone": "+24384 8109394",
    "emailAffilie": "billykasongo80@gmail.com",
    "statut": true
  },
  "utilisateur": {
    "idUtilisateur": 789,
    "nomUtilisateur": "KD-26-KIN-001",
    "emailUtilisateur": "billykasongo80@gmail.com",
    "statut": true
  },
  "collectes": [
    {
      "idCollecte": 1001,
      "typeCollecte": "Frais",
      "montant": 1.5,
      "modePaiement": "VIRTUAL_ACCOUNT",
      "statutPaiement": "VALIDE",
      "agentId": 3
    }
  ],
  "souscriptions": [
    {
      "idSouscriptionPrestation": 501,
      "prestationId": 1,
      "affilieId": 456,
      "statut": true
    }
  ],
  "dependants": [],
  "antecedants": []
}
```

#### 🔴 Codes d'erreur spécifiques
| Code HTTP | Message d'erreur | Cause | Solution |
|-----------|------------------|-------|---------|
| 400 | "TypeCollecte invalide" | Valeur non supportée | Utiliser "Frais" ou "Souscription" |
| 400 | "Mode de paiement invalide" | Mode non reconnu | Utiliser modes valides : ESPECE, MOBILE_MONEY, CARTE_BANCAIRE, VIREMENT_BANCAIRE, CHEQUE, VIRTUAL_ACCOUNT |
| 400 | "Référence paiement obligatoire" | Mode paiement nécessite référence | Ajouter referencePaiement (sauf VIRTUAL_ACCOUNT, MOBILE_MONEY, CARTE_BANCAIRE) |
| 400 | "Solde wallet virtuel insuffisant" | Débit impossible | Vérifier solde ou utiliser autre mode de paiement |
| 400 | "Ce compte ne peut pas percevoir un paiement depuis le mode de paiement WalletVirtuel..." | Rôle JWT hors whitelist pour `VIRTUAL_ACCOUNT` | Utiliser un compte AT / Chef d'équipe / Superviseur / Percepteur, ou un autre mode (ESPECE, FlexPay) |
| 400 | "Collecte de type SOUSCRIPTION doit avoir une SouscriptionPrestationId" | Souscription sans prestation | Ajouter objet souscription avec prestationId |
| 409 | "Adhésion déjà existe" | Affilié déjà adhéré | Vérifier statut existant ou créer nouvel affilié |
| 500 | "Erreur technique lors de la création d'adhésion" | Problème base de données ou service | Contacter le support technique |

---

## 💳 FlexPay (paiement électronique)

> **Réutilisation / portage vers une autre API** : voir le guide portable [`docs/Integration-FlexPay-Portable.md`](docs/Integration-FlexPay-Portable.md) (contrats FlexPay, **choix de devise de paiement CDF/USD**, pattern hold→callback→finalize, checklist de portage, annexe Prosoc).

Module **asynchrone** : aucune collecte ni adhésion n'est créée avant confirmation FlexPay (`callback` avec `code = "0"`).

### Modes concernés

| Mode API | FlexPay | Création immédiate |
|----------|---------|-------------------|
| `MOBILE_MONEY` | Oui | Non |
| `CARTE_BANCAIRE` | Oui | Non |
| `ESPECE`, `CHEQUE`, `VIREMENT_BANCAIRE`, `VIRTUAL_ACCOUNT` | Non | Oui (flux synchrone existant) |

> `ORANGE_MONEY` / `AIRTEL_MONEY` legacy → normalisés en `MOBILE_MONEY`.

#### Restriction `VIRTUAL_ACCOUNT` (WalletVirtuel) par rôle

Le mode `VIRTUAL_ACCOUNT` débite le wallet virtuel de l’agent gestionnaire. Il est réservé aux rôles terrain JWT :

| Rôle JWT (`Role.Nom`) | Code |
|----------------------|------|
| `Agent (AT)` | AT |
| `Chef d'équipe` | CE |
| `Superviseur` | SP |
| `Percepteur` | PR |

- **Multi-rôles** : autorisé si **au moins un** rôle JWT est dans la liste.
- **Hors liste** (Caissier, Admin, Financier, Affilié, etc., y compris SuperAdmin) : **400**  
  `{ "message": "Ce compte ne peut pas percevoir un paiement depuis le mode de paiement WalletVirtuel. Veuillez contacter le support." }`
- Flux concernés : `POST /api/Collecte`, adhésion sync `with-affilie`, achat souscription sync, paiement affilié sync si `VIRTUAL_ACCOUNT`.
- `ESPECE` / FlexPay restent inchangés pour les autres rôles.

### Configuration (Admin / Financier)

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/InfoPaiementMarchand/actif` | Config marchand active (token masqué) |
| POST | `/api/InfoPaiementMarchand` | Créer / activer une config |
| PUT | `/api/InfoPaiementMarchand/{id}` | Mettre à jour |

Section `appsettings.json` :

```json
"FlexPay": {
  "Enabled": true,
  "HoldMinutes": 15,
  "CallbackBaseUrl": "https://votre-api/api/FlexPay/callback",
  "MontantTolerance": 0.05
}
```

### Endpoints publics / secours

| Méthode | Route | Auth | Rôle |
|---------|-------|------|------|
| POST | `/api/FlexPay/callback` | Non | Webhook FlexPay → finalise collecte ou adhésion |
| GET | `/api/FlexPay/verifier/{orderNumber}` | Anon / JWT | Poll secours : finalise si FlexPay `status=0` ; sinon `pending: true` (pas un refus) |
| GET | `/api/FlexPay/approve\|cancel\|decline` | Non | Pages retour carte bancaire |

### Temps réel SignalR (tous flux FlexPay)

Après `202 Accepted` / initiation FlexPay, le client peut recevoir le résultat du callback **sans polling** :

1. Connexion SignalR : `{baseUrl}/flexPayHub` (pas de JWT requis pour les flux publics).
2. Appeler `JoinFlexPayPayment(idCollecteEnAttente)` avec le GUID retourné dans `InitiateFlexPayResponseDto`.
3. Écouter l'événement **`FlexPayPaymentUpdated`** (payload `FlexPayPaymentUpdatedDto` : `success`, `failed`, `alreadyProcessed`, `idAdhesion`, `idCollecte`, `sourceFlux`, etc.).
4. **Fallback** : `GET /api/FlexPay/verifier/{orderNumber}` si WebSocket indisponible.
   - Continuer le poll tant que `pending === true` (paiement MM encore en cours chez FlexPay).
   - Succès uniquement si `idAdhesion` / `idCollecte` présents, ou `alreadyProcessed` / message de finalisation réussie.
   - Ne pas interpréter un statut non-`0` du check comme refus métier : seul le **callback** `code != "0"` marque l’échec définitif.

Exemple client (JavaScript) :

```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${apiBase}/flexPayHub`)
  .withAutomaticReconnect()
  .build();

connection.on("FlexPayPaymentUpdated", (payload) => {
  if (payload.success || payload.alreadyProcessed) {
    // redirection succès (adhésion / collecte créée)
  } else if (payload.failed) {
    // paiement refusé FlexPay
  } else {
    // erreur métier (payload.message)
  }
});

await connection.start();
await connection.invoke("JoinFlexPayPayment", idCollecteEnAttente);
// puis initier POST FlexPay (adhésion, collecte, etc.)
```

Groupe serveur : `flexpay_{idCollecteEnAttente}`. Si l'utilisateur est authentifié (`IdUtilisateur` sur la collecte en attente), le même événement est aussi envoyé au groupe `user_{id}` du `notificationHub`.

### Flux par endpoint métier

```mermaid
sequenceDiagram
    participant Client
    participant API
    participant FlexPay
    participant DB

    Client->>API: POST collecte/adhésion (MOBILE_MONEY)
    API->>DB: CollecteEnAttente + Hold (15 min)
    API->>FlexPay: Initier paiement
    FlexPay-->>Client: Push MM ou paymentUrl
    FlexPay->>API: POST /callback code=0
    API->>DB: Collecte / Adhésion créée
```

| Endpoint | FlexPay | Réponse initiation |
|----------|---------|-------------------|
| `POST /api/Collecte` | MM / Carte | `200` + `InitiateFlexPayResponseDto` |
| `POST /api/Collecte/with-paiement-electronique` | MM / Carte | `202 Accepted` + `InitiateFlexPayResponseDto` (public) |
| `POST /api/Affilie/paiement` | MM / Carte | `200` + `InitiateFlexPayResponseDto` |
| `POST /api/Adhesion/with-affilie-paiement-electronique` | MM / Carte | `202 Accepted` + `InitiateFlexPayResponseDto` |
| `POST /api/SouscriptionPrestation/paiement-electronique` | MM / Carte | `202 Accepted` + `InitiateFlexPayResponseDto` (JWT ; souscription + collecte créées au callback) |

### Carte bancaire (`CARTE_BANCAIRE`)

Même pipeline FlexPay que `MOBILE_MONEY` : `CollecteEnAttente` + hold, finalisation **uniquement** au `POST /api/FlexPay/callback` avec `code = "0"`. Alias acceptés : `CARTE`, `CARD`.

| Aspect | `MOBILE_MONEY` | `CARTE_BANCAIRE` |
|--------|----------------|------------------|
| Initiation API | `InitierPaiementMobileMoneyAsync` | `InitierPaiementCarteV1Async` (`FlexPay:CardPaymentUrl`) |
| `telephonePaiement` | Obligatoire | Non requis |
| Réponse initiation | Push opérateur (pas d’URL obligatoire) | **`paymentUrl`** obligatoire pour redirection navigateur |
| `TransactionFlexPay.typePaiement` | `"1"` | `"2"` |
| Pages retour | — | `GET /api/FlexPay/approve`, `cancel`, `decline` (informatives ; ne créent pas la collecte) |

Prérequis admin : `InfoPaiementMarchand.actifCarteBancaire = true` et token marchand valide. Si désactivé : `400` « Carte bancaire FlexPay désactivée ».

Exemple collecte publique carte :

```json
{
  "modePaiement": "CARTE_BANCAIRE",
  "devisePaiementId": 1,
  "collecte": {
    "typeCollecte": "Frais",
    "fraisId": 1,
    "affilieId": 42,
    "agentId": 1,
    "montant": 1500,
    "deviseId": 1,
    "modePaiement": "CARTE_BANCAIRE",
    "statutPaiement": "EN_ATTENTE"
  }
}
```

Checklist validation production :

1. `InfoPaiementMarchand` : `actifCarteBancaire = true`.
2. `appsettings` : `FlexPay:CardPaymentUrl`, `FlexPay:CallbackBaseUrl` accessibles depuis Internet.
3. Test `POST .../with-paiement-electronique` ou adhésion électronique avec `CARTE_BANCAIRE` → `202` + `paymentUrl` non vide.
4. Paiement sur la page FlexPay → callback `code=0` → entités créées + événement SignalR `FlexPayPaymentUpdated` (groupe `flexpay_{idCollecteEnAttente}`).

### Collecte FlexPay publique (`with-paiement-electronique`)

Endpoint dédié pour initier **une seule** collecte (cotisation, frais ou souscription) sans authentification, sur le même modèle que l'adhésion FlexPay.

Règles :

- `[AllowAnonymous]` — pas de JWT requis.
- `modePaiement` racine : `MOBILE_MONEY` ou `CARTE_BANCAIRE` uniquement.
- `telephonePaiement` obligatoire si `MOBILE_MONEY`.
- `devisePaiementId` doit être identique à `collecte.deviseId` (`CDF` ou `USD`).
- La collecte en base n'est créée qu'au **callback FlexPay** succès (`CollecteEnAttente`, source `CollectePaiementElectroniquePublic`).
- `agentId` et `affilieId` restent obligatoires dans `collecte` (parrainage / affilié existant).

Exemple payload :

```json
{
  "modePaiement": "MOBILE_MONEY",
  "telephonePaiement": "0822222222",
  "devisePaiementId": 1,
  "collecte": {
    "typeCollecte": "Frais",
    "fraisId": 1,
    "affilieId": 42,
    "agentId": 1,
    "montant": 1500,
    "mois": 6,
    "annee": 2026,
    "deviseId": 1,
    "modePaiement": "MOBILE_MONEY",
    "statutPaiement": "EN_ATTENTE"
  }
}
```

### Souscription prestation FlexPay (`paiement-electronique`)

Endpoint pour **acheter une nouvelle prestation** via FlexPay. Auth **JWT** (affilié → `affilieId` forcé ; staff pour un affilié cible).

```http
POST /api/SouscriptionPrestation/paiement-electronique
Authorization: Bearer {token}
```

Règles :

- `modePaiement` racine : `MOBILE_MONEY` ou `CARTE_BANCAIRE` uniquement.
- `telephonePaiement` obligatoire si `MOBILE_MONEY`.
- `devisePaiementId` = `achat.collecte.deviseId` (`CDF` ou `USD`).
- **Aucune** `SouscriptionPrestation` / `Collecte` en base avant le callback `code = "0"` (`SourceFlux = SouscriptionAchatPaiementElectronique`).
- `POST /api/SouscriptionPrestation` synchrone refuse désormais MM/carte et renvoie vers cet endpoint.
- Réponse **`202 Accepted`** + `InitiateFlexPayResponseDto` (préfixe référence `SP-`).
- SignalR : même hub `/flexPayHub` + `JoinFlexPayPayment(idCollecteEnAttente)`.

Exemple payload :

```json
{
  "affilieId": 42,
  "modePaiement": "MOBILE_MONEY",
  "telephonePaiement": "0822222222",
  "devisePaiementId": 1,
  "achat": {
    "prestationId": 26,
    "statut": true,
    "collecte": {
      "agentId": 3,
      "montant": 5000,
      "deviseId": 1,
      "modePaiement": "MOBILE_MONEY",
      "mois": 7,
      "annee": 2026
    }
  }
}
```

### Adhésion FlexPay (`with-affilie-paiement-electronique`)

Règles spécifiques :

- **Toutes** les collectes du lot doivent être `MOBILE_MONEY` ou `CARTE_BANCAIRE` (pas de mélange avec ESPECE / VIRTUAL_ACCOUNT).
- **Une seule** transaction FlexPay pour le **montant total** (paiement partiel interdit).
- Même **devise de paiement** sur toutes les lignes (`CDF` ou `USD`).
- `modePaiement`, `telephonePaiement` (MM) et `devisePaiementId` sont transmis au niveau racine du payload.
- Pas de `referencePaiement` obligatoire à l'initiation (générée au callback).
- `statutPaiement` confirmé **non requis** à l'initiation (`EN_ATTENTE` accepté).
- Affilié, adhésion, souscriptions et collectes créés **uniquement** au callback succès.
- **Adhésion en ligne** : omettre `agentId` ou envoyer `null` — `Adhesion.AgentId` reste `null` jusqu'à affectation d'un gestionnaire AT par un admin.
- **Parcours public (sans JWT)** : autorisé (`AllowAnonymous`). Au callback, `Adhesion.UtilisateurId` et `Collecte.OperateurUtilisateurId` peuvent rester `null`. En prod : exécuter `sql/MigrateAdhesionUtilisateurIdNullable.idempotent.sql` si la colonne était encore NOT NULL.

Important:

- `POST /api/Adhesion/with-affilie` est désormais réservé au flux synchrone (ESPECE / VIREMENT_BANCAIRE / CHEQUE / VIRTUAL_ACCOUNT).
- Si `MOBILE_MONEY` ou `CARTE_BANCAIRE` est envoyé sur `with-affilie`, l'API renvoie `400` et oriente vers `with-affilie-paiement-electronique`.

Exemple payload endpoint dédié:

```json
{
  "modePaiement": "MOBILE_MONEY",
  "telephonePaiement": "0822222222",
  "devisePaiementId": 1,
  "adhesion": {
    "nom": "Doe",
    "prenom": "John",
    "typeAdhesionId": 1,
    "agentId": null,
    "collectes": [
      {
        "typeCollecte": "Cotisation",
        "cotisationAffilieId": 1,
        "montant": 1.5,
        "deviseId": 1,
        "modePaiement": "MOBILE_MONEY",
        "statutPaiement": "EN_ATTENTE",
        "mois": 5,
        "annee": 2026
      }
    ]
  }
}
```

### Adhésions en ligne sans gestionnaire AT

Les affiliés ayant adhéré en ligne (FlexPay) sans gestionnaire assigné ont `Adhesion.AgentId = null`.

| Endpoint | Rôle | Description |
|----------|------|-------------|
| `GET /api/Adhesion/en-ligne-sans-gestionnaire` | `Admin`, `Superviseur` | Liste paginée des adhésions actives sans agent AT |

Workflow backoffice :

```http
GET /api/Adhesion/en-ligne-sans-gestionnaire?page=1&pageSize=20
Authorization: Bearer {token_admin}

PUT /api/Agent/{agentAtId}/affecter-affilies
Authorization: Bearer {token_admin}
Content-Type: application/json

{ "affilieIds": [456, 457] }
```

Après affectation, `Adhesion.AgentId` et `Collecte.AgentId` sont mis à jour ; l'affilié disparaît de la liste en ligne. Au login, `utilisateur.idAgentGestionnaireCompte` reflète le nouvel agent.

Exemple collectes FlexPay :

```json
"collectes": [
  {
    "typeCollecte": "Cotisation",
    "cotisationAffilieId": 1,
    "montant": 1.5,
    "deviseId": 1,
    "modePaiement": "MOBILE_MONEY",
    "statutPaiement": "EN_ATTENTE",
    "mois": 5,
    "annee": 2026
  },
  {
    "typeCollecte": "Souscription",
    "montant": 5000,
    "deviseId": 1,
    "modePaiement": "MOBILE_MONEY",
    "statutPaiement": "EN_ATTENTE",
    "souscription": { "prestationId": 12 }
  }
]
```

Réponse `202 Accepted` :

```json
{
  "idCollecteEnAttente": "uuid",
  "orderNumberFlexPay": "ORD-...",
  "referenceFlexPay": "AD-...",
  "montantFlexPay": 5001.5,
  "codeDevisePaiement": "CDF",
  "holdExpireAt": "2026-05-24T23:15:00Z",
  "flexPayAccepted": true,
  "message": "Adhésion en attente — validez le paiement Mobile Money."
}
```

### Callback (extrait)

```json
{
  "code": "0",
  "orderNumber": "ORD-...",
  "reference": "PS-...",
  "amount": "5000",
  "currency": "CDF",
  "providerReference": "...",
  "channel": "ORANGE"
}
```

Réponse API :

```json
{
  "message": "Collecte 123 créée.",
  "result": {
    "success": true,
    "alreadyProcessed": false,
    "idCollecte": 123,
    "idAdhesion": null,
    "idCollecteEnAttente": "uuid"
  }
}
```

Pour une adhésion, `idAdhesion` est renseigné après finalisation.

**Montant callback** : doit correspondre à `montantFlexPay` renvoyé à l'initiation (tolérance `FlexPay:MontantTolerance`). En multidevise, le montant est converti côté serveur (ex. frais 1,50 USD → ~4275 CDF si taux USD→CDF = 2850).

**Adhésion multi-collectes** : chaque ligne de collecte reçoit une `referencePaiement` distincte dérivée de l'`orderNumber` (`{orderNumber}-Cotisation-1`, `{orderNumber}-Souscription-2`, …) pour respecter l'unicité en base.

### Tests d'intégration (référence)

Projet `Prosoc.Tests.Integration/FlexPay/` — 5 scénarios automatisés (SQLite + stub FlexPay) :

| Test | Vérifie |
|------|---------|
| `Callback_CodeZero_CreeCollecte` | Callback succès → 1 collecte agent |
| `Callback_Idempotent_DeuxiemeAppelNeDupliquePas` | 2ᵉ callback → `alreadyProcessed` |
| `Callback_CodeRefuse_NeCreePasCollecte` | `code ≠ 0` → pas de collecte, statut échec |
| `InitiateCollecte_MobileMoney_RetourneEnAttenteSansCollecte` | POST collecte MM → en attente, 0 collecte |
| `AdhesionFlexPay_InitiationPuisCallback_CreeAdhesion` | `202` puis callback → adhésion + collectes |

```bash
dotnet test Prosoc.Tests.Integration/Prosoc.Tests.Integration.csproj --filter "FullyQualifiedName~FlexPay"
```

### Holds (anti-doublon)

Pendant 15 minutes (configurable), un hold bloque un second paiement électronique pour la même clé métier (affilié + période + type, ou téléphone pour adhésion).

---

## 💰 Gestion des Wallets

### 🎯 Architecture des wallets

```mermaid
graph LR
    A[Agent] --> B[Wallet Agent]
    A --> C[Wallet Virtuel]
    D[Collecte VIRTUAL_ACCOUNT] --> C
    E[Commission] --> B
    F[Retrait Agent] --> B
```

### 💳 Wallet Virtuel Agent

#### GET /api/WalletVirtuelAgent/by-agent/{agentId}
Récupère le wallet virtuel d'un agent (solde, devise).

#### GET /api/WalletVirtuelAgent/solde/{agentId}
Solde virtuel courant.

#### PUT /api/WalletVirtuelAgent/{id}/ajouter-solde
Recharge manuelle du wallet virtuel.

**Permission** : `UPDATE_WALLET_VIRTUEL` (Admin / SuperAdmin bypass). Le rôle **Financier** n’a pas cette permission (lecture seule via `READ_WALLET_VIRTUEL`) — retrait ciblé prod : `sql/MigrateRemoveFinancierUpdateWalletVirtuel.idempotent.sql` ; reconnexion JWT.

**Restriction hiérarchique** : le caller ne peut recharger que le wallet d'un agent dont `MIN(Role.Niveau)` est **strictement supérieur** au sien (plus junior). Auto-recharge interdite. SuperAdmin (`Niveau` 0) : aucune restriction. Agent cible sans rôle lié : refusé (sauf SuperAdmin).

Si interdit → **403** :
```json
{
  "codeErreur": "HIERARCHIE_RECHARGE_INTERDITE",
  "message": "Vous ne pouvez recharger que le wallet virtuel d'un agent de niveau hiérarchique inférieur au vôtre."
}
```

Même règle appliquée aux crédits manuels : `PUT /{id}` (ajustement), `PUT modifier-solde-wallet-agents`, et `POST` si `soldeInitial > 0`.

Corps :
```json
{
  "montant": 100.00,
  "observation": "Remise caisse matinée"
}
```

#### GET /api/WalletVirtuelAgent/by-agent/{agentId}/mouvements
Historique complet des mouvements (recharges, ajustements, débits collectes VA).

Filtres query optionnels : `typeOperation` (`CREDIT`/`DEBIT`), `source` (`AJOUT_SOLDE`, `CREATION`, `AJUSTEMENT_SOLDE`, `COLLECTE_COMPTE_VIRTUEL`), `dateDebut`, `dateFin`.

#### GET /api/WalletVirtuelAgent/by-agent/{agentId}/mouvements/paginated
Même historique, paginé (`pageNumber`, `pageSize`, `sortBy=DateOperation`).

| Champ | Description |
|-------|-------------|
| `agentId`, `agentNom` | Agent **bénéficiaire** (propriétaire du wallet) |
| `idAgentFrom`, `nomAgentFrom` | Agent à l'**origine** de l'opération (recharge / ajustement) — dérivé de l'opérateur JWT ; `null` si compte sans fiche agent |
| `deviseId`, `deviseCode`, `deviseNom`, `deviseSymbole` | Devise du mouvement |
| `soldeAvant`, `soldeApres` | Soldes au moment de l'opération (null sur historique antérieur à la migration) |
| `operateurUtilisateurId`, `operateurNom` | Utilisateur ayant effectué la recharge/ajustement |
| `source`, `sourceLibelle` | Code technique et libellé métier |
| `collecteId`, `affilieNom`, `affilieCode` | Contexte affilié pour les débits `COLLECTE_COMPTE_VIRTUEL` |

```json
{
  "idWalletVirtuelMouvement": 12,
  "walletVirtuelId": 3,
  "montant": 100.00,
  "typeOperation": "CREDIT",
  "source": "AJOUT_SOLDE",
  "sourceLibelle": "Recharge manuelle",
  "description": "Remise caisse matinée",
  "dateOperation": "2026-06-29T10:00:00Z",
  "agentId": 5,
  "agentNom": "Agent Test",
  "idAgentFrom": 12,
  "nomAgentFrom": "Superviseur Recharge",
  "deviseCode": "USD",
  "soldeAvant": 200.00,
  "soldeApres": 300.00,
  "operateurNom": "financier"
}
```

Sources enregistrées : `AJOUT_SOLDE`, `CREATION`, `AJUSTEMENT_SOLDE`, `COLLECTE_COMPTE_VIRTUEL`.

### 💰 Wallet Agent

#### GET /api/wallets-agents/{agentId}
Récupère le solde du wallet commission d'un agent.

#### 📤 Réponse réussie
```json
{
  "idWalletAgent": 1,
  "agentId": 3,
  "soldeCourant": 25000,
  "dateModification": "2026-03-16T10:00:00Z",
  "totalCommissions": 50000,
  "TotalRetraits": 25000
}
```

#### GET /api/WalletMouvement/by-agent/{agentId}/paginated

Historique des mouvements du wallet commission (crédits commission, débits retrait, etc.).

| Champ | Description |
|-------|-------------|
| `deviseId`, `deviseCode`, `deviseNom`, `deviseSymbole` | Devise du mouvement (CDF / USD) |
| `montant`, `typeOperation`, `source` | Montant et nature de l'opération |
| `walletAgentId`, `agentNom`, `agentMatricule` | Contexte agent |

```json
{
  "idWalletMouvement": 45,
  "walletId": 1,
  "montant": 2500.00,
  "typeOperation": "CREDIT",
  "source": "COMMISSION",
  "deviseId": 2,
  "deviseCode": "USD",
  "deviseNom": "Dollar américain",
  "deviseSymbole": "$",
  "agentNom": "Agent Test"
}
```

### SoldeCourant vs SoldeDisponible

Chaque `WalletAgent` expose deux soldes :

| Champ | Rôle |
|-------|------|
| `SoldeCourant` | Solde **comptable** : commissions créditées, retenues MAASH, retraits effectués. Utilisé par le **dashboard** (somme convertie en devise principale). |
| `SoldeDisponible` | Solde **retirable** : montant réellement disponible pour une demande de retrait. Toujours `≤ SoldeCourant`. |

**Cycle de vie retrait** :

1. **Crédit commission** : `SoldeCourant` et `SoldeDisponible` augmentent ensemble.
2. **Création demande** (`POST /api/RetraitAgent`) : `SoldeDisponible` est **réservé** (décrémenté), `SoldeCourant` inchangé.
3. **Rejet, suppression ou expiration du jeton** (`EN_ATTENTE` / `VALIDEE`) : réservation **libérée** sur `SoldeDisponible`.
4. **Utilisation jeton** (`TRAITEE`) : seul `SoldeCourant` est débité (la réservation a déjà réduit `SoldeDisponible`).

### Retrait agent (devise principale)

Les retraits agents s'effectuent **exclusivement en devise principale** (`EstDevisePrincipale = true`, en production : **USD**).

| Règle | Détail |
|-------|--------|
| Wallet utilisé | `WalletAgent` dont `DeviseId` = devise principale |
| Solde vérifié | `SoldeDisponible` (pas `SoldeCourant`) |
| Réservation | À la création de la demande, `SoldeDisponible` est décrémenté |
| Crédit commission | `CommissionService` convertit la commission collectée vers la devise principale et crédite **à la fois** `SoldeCourant` et `SoldeDisponible` |
| Montant demandé | Exprimé en devise principale (minimum 1 000) |

#### POST /api/RetraitAgent/verifier-solde

Vérifie si l'agent dispose de `SoldeDisponible` suffisant sur son wallet en devise principale.

```json
{
  "agentId": 3,
  "montantDemande": 50000,
  "soldeDisponible": 75000,
  "soldeSuffisant": true,
  "difference": 25000,
  "deviseId": 2,
  "deviseCode": "USD",
  "deviseSymbole": "$",
  "message": "Solde suffisant pour le retrait (USD)"
}
```

Si aucun wallet en devise principale n'existe pour l'agent, `soldeSuffisant` est `false` et le message indique l'absence de wallet (`Aucun wallet en devise principale (USD) pour cet agent.`).

#### POST /api/RetraitAgent

Crée une demande de retrait. Les champs `deviseCode` / `deviseSymbole` sont renvoyés dans les DTOs de lecture et dans `RetraitWorkflowResultDto` lors de la création.

**Intégration mobile** : afficher le solde dashboard (`SoldeCourant` converti en devise principale) uniquement si `SoldeDisponible` sur le wallet principal est cohérent ; en cas d'écart historique, la vérification de solde avant retrait fait foi.

**Migration données existantes** (idempotent) :

```bash
mysql -h <host> -u <user> -p <database> < sql/MigrateRetraitDevisePrincipale.idempotent.sql
```

Au démarrage, `SeedData.MigrateRetraitDevisePrincipaleAsync` applique la même logique (mouvements `MIG_RETRAIT_DEVISE`).

### Caisse guichet (session + mouvements)

Rôles : `Admin`, `Caissier`, `Financier`, `Percepteur`.

| Endpoint | Description |
|----------|-------------|
| `POST /api/Caisse/session/ouvrir` | Ouvre une session (`soldeOuverture` = fond de caisse) |
| `GET /api/Caisse/session/courante` | Session OUVERTE du caissier connecté |
| `GET /api/Caisse/sessions` | Liste paginée des sessions (filtres `dateDebut`, `dateFin`, `statut`) |
| `GET /api/Caisse/session/{id}/solde` | Solde théorique (ouverture + entrées − sorties) |
| `GET /api/Caisse/session/{id}/mouvements` | Journal paginé |
| `POST /api/Caisse/session/{id}/cloturer` | Clôture avec `soldeReelCloture` (écart éventuel) |

**Entrées** : collecte guichet avec `OperateurUtilisateurId` et session ouverte :
- `ESPECE` → `MouvementCaisse` `ENTREE` / source `COLLECTE_ESPECE`
- `MOBILE_MONEY`, `CARTE_BANCAIRE` (FlexPay finalisé au guichet) → `ENTREE` / source `COLLECTE_ELECTRONIQUE`

**Sorties** : paiement retrait via jeton (ci-dessous) → `MouvementCaisse` `SORTIE` + `WalletMouvement` `DEBIT` (`RETRAIT_JETON`).

#### POST /api/RetraitAgent/utiliser-jeton — POST /api/RetraitAgent/marquer-paye

Alias métier identiques. Rôles : `Admin`, `Caissier`, `Financier`, `Percepteur`.

Prérequis production : **session caisse OUVERTE** et solde suffisant. Guide Postman : [`GUIDE_SESSION_CAISSE_RETRAIT_POSTMAN.md`](GUIDE_SESSION_CAISSE_RETRAIT_POSTMAN.md).

```json
{
  "idJeton": 12,
  "codeJeton": "JRTABC12345",
  "agentId": 3,
  "observationUtilisation": "Paiement guichet",
  "sessionCaisseId": null
}
```

Réponse (`RetraitPaiementResultDto`) : `montantPaye`, `soldeWalletApres`, `soldeCaisseSessionApres`, `walletMouvementId`, `mouvementCaisseId`.

Codes d'erreur : `SESSION_CAISSIER_REQUISE`, `SOLDE_CAISSE_INSUFFISANT`, `JETON_DEJA_UTILISE` (HTTP 409), `JETON_EXPIRE`, etc.

**Migration production** :

```bash
mysql -h <host> -u <user> -p <database> < sql/MigrateCaisseSession.production.idempotent.sql
mysql -h <host> -u <user> -p <database> < sql/MigrateCaisseRetraitAgentPermissions.idempotent.sql
```

### Dashboard Admin

Rôle requis : `Admin` ou `SuperAdmin`.

| Endpoint | Description |
|----------|-------------|
| `GET /api/DashboardAdmin/kpis` | KPIs globaux (affiliés, agents, collectes du mois, file validation) |
| `GET /api/DashboardAdmin/agents-performance` | Top agents par volume de collectes |
| `GET /api/DashboardAdmin/collectes-en-attente` | Liste des collectes en attente de validation admin |
| `POST /api/DashboardAdmin/validate-collecte/{id}` | Force `StatutPaiement` → `VALIDE` (idempotent) |
| `POST /api/DashboardAdmin/toggle-agent/{id}` | Activer / désactiver un agent |

#### Sémantique des KPIs (`GET /api/DashboardAdmin/kpis`)

| Champ | Signification |
|-------|---------------|
| `totalCollectesMois` / `nombreCollectesMois` | Somme des collectes actives (`Statut = true`) du mois calendaire en cours, **tous agents**, consolidée en **devise principale** (`MontantDevisePrincipale`, repli sur `Montant`). Champ `devisePrincipaleCode` (ex. `USD`) |
| `collectesEnAttente` | Paiement **confirmé** (`OK`, `PAYE`, `CONFIRMÉ`, …) mais **pas encore validé admin** (`Validé`). Exclut FlexPay `EN_ATTENTE` et collectes déjà validées |
| `progressionCollectesMois` | Variation MTD du montant collecté vs **même fenêtre** du mois précédent (1 → jour courant, jour borné si mois plus court). **100 %** si la période précédente = 0 et le mois courant > 0 |
| `nouvellesAdhesionsAujourdhui` | Adhésions **actives** créées aujourd’hui |
| `totalCommissionsMois` | Somme des mouvements wallet `COMM_COLLECTE` depuis le 1er du mois, consolidée en **devise principale** (conversion au taux actif à la date du mouvement). Même `devisePrincipaleCode` que `totalCollectesMois` |

Exemple de réponse :

```json
{
  "totalAffilies": 7,
  "totalAgents": 24,
  "totalCollectesMois": 5095.5,
  "devisePrincipaleCode": "USD",
  "totalCommissionsMois": 19,
  "nouvellesAdhesionsAujourdhui": 2,
  "collectesEnAttente": 18,
  "nombreCollectesMois": 20,
  "progressionCollectesMois": 100,
  "agentsInactifs": 0,
  "derniereCollecte": "2026-06-13T16:17:34.641942",
  "affiliesInactifs": 0
}
```

Les montants de `GET /api/DashboardAdmin/agents-performance` (`totalCollectes`, `montantMoyenCollecte`) et de `GET /api/DashboardAdmin/collectes-en-attente` (`montant`) sont consolidés en **devise principale** (`MontantDevisePrincipale`, repli sur `Montant`).

### Dashboard SuperAdmin

Rôle requis : `SuperAdmin` uniquement.

| Endpoint | Description |
|----------|-------------|
| `GET /api/DashboardSuperAdmin/summary` | Dashboard consolidé (KPIs admin + gouvernance système + top agents + file validation) |
| `GET /api/DashboardSuperAdmin/kpis-admin` | KPIs métier (même périmètre et sémantique que `DashboardAdmin/kpis`) |
| `GET /api/DashboardSuperAdmin/kpis-systeme` | KPIs plateforme (utilisateurs, rôles, permissions, FlexPay) — **sans montants financiers** |
| `GET /api/DashboardSuperAdmin/utilisateurs-par-role` | Répartition des utilisateurs actifs par rôle |

**Montants consolidés** : les KPIs admin (`totalCollectesMois`, `totalCommissionsMois`), le top agents (`totalCollectes`) et les collectes en attente (`montant`) réutilisent la même logique que le dashboard Admin, en **devise principale** (`devisePrincipaleCode`, ex. `USD`). Le champ `devisePrincipaleCode` est présent sur `kpisAdmin` et au niveau racine du `summary`.

### 📊 Dashboard Agent de Terrain (Remarque 5/6)

Rôle requis : `Agent (AT)`, `Agent (AA)` ou `Chef d'équipe`. L’`agentId` est résolu depuis le JWT (`AgentId` ou `Utilisateur.AgentId`).

| Endpoint | Description |
|----------|-------------|
| `GET /api/DashboardAgent/terrain` | **Vue consolidée** : KPIs, primes, commissions, suivi adhérents |
| `GET /api/DashboardAgent/resume` | Alias de la vue consolidée |
| `GET /api/DashboardAgent/primes-generees` | Primes (collectes `Souscription`) — assurance / mutuelle |
| `GET /api/DashboardAgent/commissions-resume` | Solde wallet + mouvements `COMM_COLLECTE` |
| `GET /api/DashboardAgent/suivi-adherents` | Liste adhérents — `cotisationAJour` (période courante), `statutGlobal` / `statutCotisation` / `statutPrestation` (arriérés), filtre `?statutGlobal=EN_ORDRE|HORS_ORDRE` |
| `GET /api/DashboardAgent/kpis` | Indicateurs du mois |
| `GET /api/DashboardAgent/commissions` | Synthèse commissions par mois |
| `GET /api/DashboardAgent/affilies-recents` | Derniers adhérents enregistrés |
| `GET /api/DashboardAgent/graphs` | Graphiques (collectes, adhésions, commissions) |
| `GET /api/DashboardAgent/objectifs` | Objectifs vs réalisé (cible `TargetAgent` mensuelle du **rôle** de l'agent, ex. `Agent (AT)` → 100) |

**Montants consolidés** : collectes (`totalCollectesMois`, `moyenneCollecte`, graphiques, primes, suivi adhérents, collectes en attente) via `MontantDevisePrincipale` (repli `Montant`) ; commissions wallet (`COMM_COLLECTE`) converties au taux actif à la date du mouvement ; `soldeWallet` = somme des wallets agent convertie. Champ `devisePrincipaleCode` (ex. `USD`) sur les KPIs et la vue `terrain` / `resume`.

### Dashboard Chef d'équipe (C.E)

Rôle requis : `Chef d'équipe`. Permissions dédiées : `ACCESS_DASHBOARD_CHEF_EQUIPE`, `READ_EQUIPE_ZONE`, `READ_EQUIPE_WALLET_MOVEMENT`, `READ_EQUIPE_COLLECTE`.

Périmètre : le chef connecté doit être **titulaire** de la zone (`ZoneSociale.ChefEquipeAgentId`). Il voit les agents `Agent (AT)` actifs de cette zone. Le C.E conserve aussi les capacités terrain d'un AT sur son propre agent.

| Endpoint | Description |
|----------|-------------|
| `GET /api/DashboardChefEquipe/kpis` | KPIs de la zone (volume collectes du mois, montants, en attente) |
| `GET /api/DashboardChefEquipe/agents` | Liste des AT de la zone avec stats mensuelles |
| `GET /api/DashboardChefEquipe/agents/{agentId}/mouvements-wallet` | Lecture wallet de l'agent ciblé (si dans la zone) |
| `GET /api/DashboardChefEquipe/agents/{agentId}/collectes` | Collectes récentes de l'agent ciblé (si dans la zone) |

Garde-fous : `GET /api/WalletMouvement/by-agent/{agentId}` et `GET /api/Collecte/by-agent/{agentId}` appliquent la même contrainte de zone pour le rôle `Chef d'équipe` (403 si hors périmètre ou si l'utilisateur n'est pas titulaire CE de la zone).

### Encadrement territorial (admin)

Rôles requis : `Admin`, `SuperAdmin` ou `IT`. Affectation atomique : FK territoriale + synchronisation du rôle JWT (`Chef d'équipe` / `Superviseur`).

| Endpoint | Description |
|----------|-------------|
| `PUT /api/ZoneSociale/{id}/chef-equipe` | Nomme le CE titulaire de la zone (`body: { "agentId": n }`). L'agent doit avoir `ZoneSocialeId` = zone cible. |
| `DELETE /api/ZoneSociale/{id}/chef-equipe` | Retire le CE titulaire et le rôle si plus titulaire ailleurs |
| `PUT /api/Commune/{id}/superviseur` | Nomme le SP titulaire de la commune. L'agent doit être rattaché à une zone de cette commune. |
| `DELETE /api/Commune/{id}/superviseur` | Retire le SP titulaire |

Réponse type (`TerritorialAffectationResultDto`) : `territoryId`, `previousAgentId`, `previousAgentNom`, `newAgentId`, `newAgentNom`.

Les DTOs `ZoneSocialeReadDto` et `CommuneReadDto` exposent `chefEquipeAgentId` / `chefEquipeNom` et `superviseurAgentId` / `superviseurNom`.

**Superviseur** : le périmètre dashboard/hiérarchie est désormais **communal** (tous les agents actifs des zones de la commune titulaire), et non plus l'arbre `Agent.SuperviseurId`. Les endpoints `AffecterAgent` / `RetirerAgent` du superviseur sont dépréciés — utiliser l'affectation territoriale ci-dessus.

Migration données : `sql/MigrateTerritorialEncadrement.idempotent.sql` ou seed au démarrage (`MigrateTerritorialEncadrementAsync`).

### Dashboard Agent Administratif / Encodeur (AA)

Rôle requis : `Agent (AA)`. L’`agentId` est résolu depuis le JWT (`AgentId` ou `Utilisateur.AgentId`).

**Périmètre des données** : uniquement les adhésions actives où `Adhesion.AgentId` = agent connecté (`Statut = true`). Les dossiers créés au niveau 1 par un agent **AT** restent rattachés à cet AT tant qu’ils ne sont pas **affectés** à l’encodeur AA.

| Endpoint | Description |
|----------|-------------|
| `GET /api/DashboardAgentAA/summary` | Vue consolidée encodeur (KPIs, répartition, dossiers à traiter, dépendants/antécédents récents) |
| `GET /api/DashboardAgentAA/kpis` | KPIs encodeur (dossiers, dépendants, antécédents, **collectes/commissions du mois** en devise principale) |
| `GET /api/DashboardAgentAA/dossiers-a-traiter` | Dossiers non validés (`StatutDossier` ≠ `VALIDÉ`) |
| `GET /api/DashboardAgentAA/dependants-recents` | Derniers dépendants des affiliés affectés |
| `GET /api/DashboardAgentAA/antecedents-recents` | Derniers antécédents des affiliés affectés |
| `GET /api/DashboardAgentAA/repartition-statuts` | Répartition par `StatutDossier` |

#### Prérequis : affectation des affiliés

Pour qu’un encodeur AA voie des KPIs non nuls, un administrateur ou superviseur doit affecter les affiliés à son agent :

```http
PUT /api/Agent/{agentId}/affecter-affilies
Authorization: Bearer {token_admin_ou_superviseur}
Content-Type: application/json

{
  "affilieIds": [1, 2]
}
```

**Rôles requis** : `Admin` ou `Superviseur`.

**Mode sélectif** : `agentId` dans l’URL = agent **cible** ; `affilieIds` = liste des affiliés à transférer.

**Mode massif** (transfert de tout le portefeuille actif d’un agent vers un autre) :

```http
PUT /api/Agent/{agentCibleId}/affecter-affilies
Authorization: Bearer {token_admin_ou_superviseur}
Content-Type: application/json

{
  "sourceAgentId": 5,
  "affilieIds": []
}
```

Résout automatiquement tous les affiliés actifs (`Adhesion.Statut = true`, `Affilie.Statut = true`) dont `Adhesion.AgentId = sourceAgentId`. L’agent source et l’agent cible doivent être différents.

**Réponse** : `totalReussites`, `totalEchecs`, et pour chaque affilié `ancienAgentId` + message. `400` si aucun succès ; `404` si agent cible ou source introuvable.

Cette opération met à jour `Adhesion.AgentId` et `Collecte.AgentId` (le dossier disparaît du dashboard de l’agent précédent).

**Diagnostic** (base MySQL) :

```bash
dotnet run --project Scripts/DashboardAaDiagnostic
# ou exécuter sql/DiagnosticDashboardAgentAA.sql
```

Si `COUNT(*) FROM Adhesions WHERE AgentId = {id}` = 0, le dashboard renvoie des zéros — comportement attendu, pas un bug de calcul.

**Montants consolidés** (`totalCollectesMois`, `totalCommissionsMois`) : même règle que le dashboard admin — `MontantDevisePrincipale` (repli `Montant`) pour les collectes des affiliés affectés ; conversion des mouvements wallet `COMM_COLLECTE` de l'agent AA au taux actif. Champ `devisePrincipaleCode` (ex. `USD`) sur les KPIs et au niveau racine du `summary`.

**Distinction** : `GET /api/adhesion/{id}/fiche-encodeur` charge un dossier par id **sans** filtre agent ; le dashboard AA, lui, ne compte que les dossiers **affectés**.

### Dashboard Agent Hôpital

Rôle requis : `Agent (Hôpital)`. Le périmètre est filtré sur l’hôpital partenaire lié à l’utilisateur connecté (`HopitalPartenaireId`).

| Endpoint | Description |
|----------|-------------|
| `GET /api/DashboardAgentHopital/summary` | Vue consolidée (KPIs jetons/bons, patients, dépendants, antécédents) |
| `GET /api/DashboardAgentHopital/kpis` | Compteurs jetons, bons, patients |
| `GET /api/DashboardAgentHopital/jetons-en-attente` | Jetons valides non utilisés |
| `GET /api/DashboardAgentHopital/bons-recents` | Derniers bons d’envoi liés à l’hôpital |
| `GET /api/DashboardAgentHopital/patients` | Patients (affiliés) ayant des jetons/bons sur l’hôpital |
| `GET /api/DashboardAgentHopital/dependants` | Dépendants des patients du périmètre |
| `GET /api/DashboardAgentHopital/antecedents` | Antécédents des patients du périmètre |
| `GET /api/DashboardAgentHopital/repartition-prestations` | Répartition jetons/bons par prestation |

**Montants consolidés** : valeur catalogue des prestations (`Prestation.Montant`) liées aux jetons (via `DemandeBonEnvoi`) et aux bons de l’hôpital, convertie au taux actif à la date d’émission ou d’utilisation. Champs KPIs : `valeurPrestationsJetonsTotal`, `valeurPrestationsJetonsMois`, `valeurPrestationsJetonsUtilisesMois`, `valeurPrestationsBonsTotal`, `valeurPrestationsBonsMois`, `valeurPrestationsBonsUtilisesMois`. `montantPrestation` sur jetons en attente et bons récents ; `montantTotalJetons` / `montantTotalBons` sur la répartition. Champ `devisePrincipaleCode` (ex. `USD`) sur les KPIs et le `summary`. Un jeton sans demande liée ne contribue pas aux montants.

### Dashboard Caissier

Rôle requis : `Caissier`. Les données sont filtrées sur `Collecte.OperateurUtilisateurId` = utilisateur connecté.

| Endpoint | Description |
|----------|-------------|
| `GET /api/DashboardCaissier/summary` | Vue consolidée (KPIs, collectes récentes, répartitions, adhésions du jour) |
| `GET /api/DashboardCaissier/kpis` | KPIs caissier |
| `GET /api/DashboardCaissier/collectes-recentes` | Dernières collectes saisies par le caissier |
| `GET /api/DashboardCaissier/collectes` | Historique paginé des collectes guichet (filtres `dateDebut`, `dateFin`, `modePaiement`) |
| `GET /api/DashboardCaissier/repartition-type` | Répartition du jour par type de collecte |
| `GET /api/DashboardCaissier/repartition-mode` | Répartition du jour par mode de paiement |
| `GET /api/DashboardCaissier/adhesions-du-jour` | Adhésions créées aujourd'hui par le caissier |

**Montants consolidés** : `montantDuJour`, `montantSemaine`, `montantMois`, `montantMoyen`, montants des répartitions et collectes récentes utilisent `MontantDevisePrincipale` (repli sur `Montant`). Champ `devisePrincipaleCode` (ex. `USD`) sur les KPIs.

### Dashboard Financier

Rôle requis : `Financier` (ou permissions équivalentes).

| Endpoint | Description |
|----------|-------------|
| `GET /api/DashboardFinancier/kpis` | KPIs financiers globaux (12 mois glissants) |
| `GET /api/DashboardFinancier/performances-mensuelles` | CA, collectes et commissions par mois |
| `GET /api/DashboardFinancier/revenus-source` | Répartition collectes vs commissions |
| `GET /api/DashboardFinancier/top-agents` | Classement agents par CA |
| `GET /api/DashboardFinancier/commissions-agents` | Commissions par agent |
| `GET /api/DashboardFinancier/produits-stats` | Statistiques produits |
| `GET /api/DashboardFinancier/tendances` | Tendances journalières |
| `GET /api/DashboardFinancier/transactions-periode` | Agrégats transactions |
| `GET /api/DashboardFinancier/objectifs` | Objectifs vs réalisé (stub monétaire CA / collectes) |
| `GET /api/DashboardFinancier/objectifs-agents` | Reporting TargetAgent adhésions : synthèse par rôle + détail par agent (`mois` / `annee`, défaut = mois courant) |
| `GET /api/DashboardFinancier/revenus-region` | Revenus par région (simplifié) |
| `GET /api/DashboardFinancier/rentabilite` | Indicateurs de rentabilité |

**Objectifs agents (`objectifs-agents`)** : uniquement les agents actifs dont le rôle a un `TargetAgent` actif **Mensuelle**. `objectifTotal` = `Nombre × nbAgents` du rôle ; `realise` = count adhésions sur `[1er du mois, 1er du mois suivant)` ; détail trié par progression décroissante. Les agents sans cible mensuelle sont exclus (pas de défaut magique `100`).

**Montants consolidés** : collectes via `MontantDevisePrincipale` (repli `Montant`) ; commissions wallet `COMM_COLLECTE` converties au taux actif à la date du mouvement. Champ `codeDeviseConsolidation` (ex. `USD`) sur les KPIs et le dashboard complet.

### Perception collectes compte virtuel (AT)

Document détaillé : [`PROCESSUS_PERCEPTION_VIRTUELLE.md`](PROCESSUS_PERCEPTION_VIRTUELLE.md)

Rôles : `Admin`, `Percepteur`, `Financier`.

Quand un AT encaisse via `VIRTUAL_ACCOUNT`, son wallet virtuel est débité. Le percepteur récupère ensuite l'argent physique et confirme la perception via ce module (journal dédié, sans session caisse).

| Endpoint | Description |
|----------|-------------|
| `GET /api/PerceptionVirtuelle/collectes-en-attente` | Collectes VA `VALIDE` + `NON_PERCU` (filtres `agentId`, dates) |
| `GET /api/PerceptionVirtuelle/synthese-agents` | Montant / nombre en attente par AT |
| `GET /api/PerceptionVirtuelle/historique` | Journal des perceptions du percepteur connecté (pagination + `dateDebut` / `dateFin`) |
| `GET /api/PerceptionVirtuelle/historique-global` | Journal global (Admin / Financier) — filtres percepteur, agent, dates |
| `GET /api/PerceptionVirtuelle/reconciliation` | Synthèse réconciliation VA + anomalies |
| `GET /api/PerceptionVirtuelle/export` | Export Excel (`format=excel`) |
| `GET /api/PerceptionVirtuelle/{id}` | Détail perception + lignes |
| `POST /api/PerceptionVirtuelle/confirmer` | Confirme la perception d'un lot de collectes |

```json
{
  "agentId": 12,
  "collecteIds": [101, 102],
  "observation": "Remise terrain"
}
```

Réponse : `montantTotal`, `nombreCollectes`, `perceptionVirtuelleId`, `soldeRestantAgent`.

Codes d'erreur : `COLLECTE_DEJA_PERCUE` (HTTP 409), `AGENT_INCOHERENT`, `DEBIT_VIRTUEL_MANQUANT`, etc.

**Migration production** :

```bash
mysql -h <host> -u <user> -p <database> < sql/MigratePerceptionVirtuelle.production.idempotent.sql
mysql -h <host> -u <user> -p <database> < sql/MigratePerceptionVirtuellePermissions.idempotent.sql
mysql -h <host> -u <user> -p <database> < sql/MigrateFinancierPerceptionVirtuellePermissions.idempotent.sql
```

### Dashboard Percepteur

Rôle requis : `Percepteur`, `Admin` ou `Financier` (consultation / rapports).

| Endpoint | Description |
|----------|-------------|
| `GET /api/DashboardPercepteur/kpis` | KPIs percepteur (montants perçus, transactions, objectifs) |
| `GET /api/DashboardPercepteur/transactions` | Dernières collectes **globales** (ne pas utiliser comme historique personnel) |
| `GET /api/DashboardPercepteur/mes-collectes-guichet` | Historique paginé des collectes guichet du percepteur connecté |
| `GET /api/DashboardPercepteur/performances-journalieres` | Performances journalières |
| `GET /api/DashboardPercepteur/resume-mensuel` | Résumés mensuels |
| `GET /api/DashboardPercepteur/top-agents` | Top agents par montant perçu |
| `GET /api/DashboardPercepteur/transactions-type` | Répartition par type |
| `GET /api/DashboardPercepteur/paiements-mode` | Répartition par mode de paiement |
| `GET /api/DashboardPercepteur/agents-stats` | Statistiques par agent |
| `GET /api/DashboardPercepteur/tendances` | Tendances des transactions |
| `GET /api/DashboardPercepteur/objectifs` | Objectifs journalier / mensuel |
| `GET /api/DashboardPercepteur/rapport-perception` | Rapport perception Agent (VA) vs Affilié (guichet) — synthèse + lignes paginées |

**Montants consolidés** : collectes via `MontantDevisePrincipale` (repli `Montant`) ; commissions `COMM_COLLECTE` converties au taux actif. Champ `devisePrincipaleCode` (ex. `USD`) sur les KPIs. `soldeAPercevoir` / `montantEnAttente` = somme des collectes `VIRTUAL_ACCOUNT` `NON_PERCU` (voir module PerceptionVirtuelle). KPIs : `montantVirtuelEnAttente`, `nombreCollectesVirtuellesEnAttente`. KPIs et `summary` incluent aussi `rapportPerception` (synthèse Agent / Affilié).

#### Rapport perception (`GET /api/DashboardPercepteur/rapport-perception`)

Rôle : `Percepteur`, `Admin` ou `Financier`.

**Query** :

| Paramètre | Valeurs | Défaut |
|-----------|---------|--------|
| `origine` | `AGENT`, `AFFILIE`, `TOUS` | `TOUS` |
| `statut` | `EN_ATTENTE`, `PERCU`, `TOUS` | `TOUS` |
| `dateDebut`, `dateFin` | dates | — |
| `agentId` | filtre canal Agent (VA) | — |
| `affilieId` | filtre affilié | — |
| `pageNumber`, `pageSize` | pagination | 1, 20 |

**Classification** :
- **AGENT** : `VIRTUAL_ACCOUNT` + débit wallet virtuel associé ; suivi `NON_PERCU` / `PERCU`.
- **AFFILIE** : guichet synchrone validé (espèce, chèque, virement…) hors compte virtuel ; perçu dès paiement `VALIDE`.

**Réponse** : `synthese` (totaux Agent / Affilié / `totalPerçu`, `deviseCode`) + `lignes` paginées (`originePerception`, `statutPerception`, collecte, montants, agent, affilié, dates, `perceptionVirtuelleId`, `walletVirtuelMouvementId`, etc.).

### Dashboard Superviseur

Rôle requis : `Superviseur` ou `Admin`.

**Identifiant `{superviseurId}`** : toujours `Agents.IdAgent` (claim JWT `AgentId` ou `Utilisateur.AgentId`), **jamais** `Utilisateurs.IdUtilisateur`. Exemple Flutter :

```dart
final agentId = int.parse(jwtClaims['AgentId'] ?? profile.agentId.toString());
await api.get('/api/DashboardSuperviseur/indicateurs-performance/$agentId');
```

**Périmètre** : le superviseur doit être **titulaire** d'une commune (`Communes.SuperviseurAgentId`). Sinon les endpoints renvoient **422** avec `codeErreur: BUSINESS_SUPERVISEUR_SANS_COMMUNE_TITULAIRE`. Correction admin : `PUT /api/Commune/{communeId}/superviseur` body `{ "agentId": n }`. Diagnostic SQL : `sql/DiagnoseSuperviseurCommuneTitulaire.idempotent.sql`.

**Permissions JWT** : le rôle `Superviseur` n'a **pas** `UPDATE_ADHESION` ni `UPDATE_AFFILIE`, ni `CREATE_ASSUREUR` / `READ_ASSUREUR` / `UPDATE_ASSUREUR`, ni `CREATE_PRODUIT_ASSUREUR` (création / lecture adhésion-affilié conservées ; `READ_PRODUIT_ASSUREUR` conservé). **Prod (retrait ciblé)** : `sql/MigrateRemoveSuperviseurRestrictedPermissions.idempotent.sql`. **Alignement catalogue complet** : `sql/MigrateSuperviseurRolePermissions.idempotent.sql` (ou seed au démarrage). Puis **reconnecter** les comptes Superviseur pour purger le JWT.

| Endpoint | Description |
|----------|-------------|
| `GET /api/DashboardSuperviseur/dashboard/{superviseurId}` | Dashboard complet (stats, top agents, tendances, hiérarchie) |
| `GET /api/DashboardSuperviseur/kpis/{superviseurId}` | KPIs superviseur et équipe |
| `GET /api/DashboardSuperviseur/top-agents/{superviseurId}` | Top agents par montant collecté |
| `GET /api/DashboardSuperviseur/tendances/{superviseurId}` | Tendances mensuelles de l'équipe |
| `GET /api/DashboardSuperviseur/objectifs/{superviseurId}` | Objectifs d'équipe |
| `GET /api/DashboardSuperviseur/rapport-mois/{superviseurId}` | Rapport de performance mensuel |
| `GET /api/DashboardSuperviseur/hierarchie-dashboard/{superviseurId}` | Hiérarchie avec montants par agent |
| `GET /api/DashboardSuperviseur/activite-journaliere/{superviseurId}` | Activité journalière de l'équipe |
| `GET /api/DashboardSuperviseur/performances-detaillees/{superviseurId}` | Performances détaillées par agent |
| `GET /api/DashboardSuperviseur/alertes-resume/{superviseurId}` | Résumé des alertes équipe |
| `GET /api/DashboardSuperviseur/export-dashboard/{superviseurId}` | Export des données dashboard |
| `GET /api/DashboardSuperviseur/indicateurs-performance/{superviseurId}` | Indicateurs de performance clés (widgets) |

**Montants consolidés** : collectes de la hiérarchie via `MontantDevisePrincipale` (repli `Montant`) ; commissions wallet `COMM_COLLECTE` converties au taux actif à la date du mouvement. Champ `devisePrincipaleCode` (ex. `USD`) sur `SuperviseurStatsDto` et le dashboard complet ; `montantTotalEquipe`, performances agents et totaux hiérarchie en devise principale.

### Dashboard Affilié (espace membre)

Rôle requis : **`Affilié`**. Permission : `ACCESS_DASHBOARD_AFFILIE`.

**Périmètre** : toutes les routes `{affilieId}` vérifient que l'identifiant correspond à l'affilié du JWT. Un membre qui tente d'accéder au dashboard d'un autre affilié reçoit **403**.

| Endpoint | Description |
|----------|-------------|
| `GET /api/DashboardAffilie/resume/{affilieId}` | Résumé complet du dashboard |
| `GET /api/DashboardAffilie/kpis/{affilieId}` | KPIs principaux |
| `GET /api/DashboardAffilie/info/{affilieId}` | Informations de base |
| `GET /api/DashboardAffilie/cotisations/{affilieId}` | Cotisations (query : `mois`, `annee`) |
| `GET /api/DashboardAffilie/cotisations/recentes/{affilieId}` | Cotisations récentes |
| `GET /api/DashboardAffilie/prestations/{affilieId}` | Prestations (query : `mois`, `annee`) |
| `GET /api/DashboardAffilie/prestations/recentes/{affilieId}` | Prestations récentes |
| `GET /api/DashboardAffilie/beneficiaires/{affilieId}` | Bénéficiaires (famille) |
| `GET /api/DashboardAffilie/graphiques/{affilieId}` | Graphiques (`annee`) |
| `GET /api/DashboardAffilie/notifications/{affilieId}` | Notifications |
| `GET /api/DashboardAffilie/notifications/non-lues/{affilieId}` | Compteur non lues |
| `PUT /api/DashboardAffilie/notifications/{idNotification}/lire` | Marquer une notification lue |
| `GET /api/DashboardAffilie/documents/{affilieId}` | Documents |
| `GET /api/DashboardAffilie/documents/en-attente/{affilieId}` | Documents en attente |
| `GET /api/DashboardAffilie/preferences/{affilieId}` | Préférences |
| `PUT /api/DashboardAffilie/preferences/{affilieId}` | Mettre à jour les préférences |
| `GET /api/DashboardAffilie/resume-annuel/{affilieId}` | Résumé annuel |
| `GET /api/DashboardAffilie/export/cotisations/{affilieId}` | Export cotisations (PDF / Excel / CSV) |
| `GET /api/DashboardAffilie/export/prestations/{affilieId}` | Export prestations |
| `GET /api/DashboardAffilie/alertes/cotisation/{affilieId}` | Alertes cotisation |
| `GET /api/DashboardAffilie/alertes/prestation/{affilieId}` | Alertes prestation |
| `GET /api/DashboardAffilie/alertes/document/{affilieId}` | Alertes document |
| `GET /api/DashboardAffilie/alertes/expiration/{affilieId}` | Alertes expiration |

**Montants consolidés** : KPIs et exports utilisent la devise principale (`MontantDevisePrincipale`, repli `Montant`) lorsque applicable ; champ `devisePrincipaleCode` sur le résumé.

#### Objectifs agents (`TargetAgent`) — par rôle applicatif

**Breaking change (juin 2026)** : `agentId` est remplacé par `roleNom` (rôle JWT, ex. `"Agent (AT)"`). Un objectif s'applique à **tous** les agents du rôle.

**Permissions** :
- Lecture (`GET`) : `READ_TARGET_AGENT` **ou** `MANAGE_OBJECTIFS` (Admin / SuperAdmin bypass). Rôle **Financier** : `READ_TARGET_AGENT` (consultation seule — reconnexion JWT obligatoire après déploiement).
- Écriture (`POST` / `PUT` / `DELETE`) : `MANAGE_OBJECTIFS` (Superviseur).

| Méthode | Description |
|---------|-------------|
| `GET /api/TargetAgent` | Liste paginée |
| `GET /api/TargetAgent/by-role/{roleNom}` | Objectifs d'un rôle |
| `GET /api/TargetAgent/actifs` | Objectifs actifs |
| `POST /api/TargetAgent` | Créer (unicité : un actif par `(roleNom, periodicite)`) |
| `PUT /api/TargetAgent/{id}` | Modifier |
| `DELETE /api/TargetAgent/{id}` | Supprimer |

**Exemple POST**

```json
{
  "roleNom": "Agent (AT)",
  "libelleTarget": "Objectif adhésions AT — mensuel",
  "periodicite": "Mensuelle",
  "statut": true
}
```

`nombre` est calculé automatiquement : Journalière → 5, Hebdomadaire → 25, Mensuelle → 100 (KPIs workflow F3-6).

**Exemple réponse**

```json
{
  "idTargetAgent": 3,
  "roleId": 7,
  "roleNom": "Agent (AT)",
  "libelleTarget": "Objectif adhésions AT — mensuel",
  "periodicite": "Mensuelle",
  "nombre": 100,
  "statut": true
}
```

Complément : `GET /api/AgentCommission/dashboard` — historique détaillé des commissions (filtres, export).

#### Exemple — `GET /api/DashboardAgent/terrain`

```json
{
  "agentId": 3,
  "nomAgent": "Jean AT",
  "kpis": {
    "totalAffilies": 42,
    "totalCommissionsMois": 1250.5,
    "nouvellesAdhesionsMois": 8
  },
  "primes": {
    "totalPrimesMois": 3200,
    "totalPrimesAssuranceMois": 1800,
    "totalPrimesMutuelleMois": 1400,
    "nombreSouscriptionsMois": 12,
    "details": []
  },
  "commissions": {
    "soldeWallet": 25000,
    "totalCommissionsMois": 1250.5,
    "mouvementsRecents": []
  },
  "suiviAdherents": [
    {
      "codeAdhesion": "F3-2026-KIN-001",
      "nomComplet": "Marie Kabila",
      "statutDossier": "VALIDÉ",
      "cotisationAJour": true,
      "alerte": null
    }
  ],
  "dateGeneration": "2026-05-21T11:00:00"
}
```

**Primes** = montants des collectes de type **Souscription** (produits assurance ou mutuelle) générées par l’agent.  
**Commissions** = crédits wallet (`COMM_COLLECTE`) liés aux collectes.  
**Suivi adhérents** = statut dossier, `cotisationAJour` (période en cours), statuts conformité arriérés (`statutGlobal`, `statutCotisation`, `statutPrestation`), alertes.

### Conformité cotisation / prestation

Statut calculé depuis `ArrieresAffilie` (aucun arriéré impayé échu) :

| Valeur | Signification |
|--------|----------------|
| `EN_ORDRE` | Aucun arriéré en souffrance pour le périmètre concerné |
| `HORS_ORDRE` | Au moins un arriéré `EN_RETARD`, `PARTIELLEMENT_PAYE` ou `EN_ATTENTE` échu |

**Différence importante** :
- `cotisationAJour` (dashboard agent) = cotisation **période courante** payée (éligibilité produit)
- `statutCotisation` = aucun **arriéré cotisation** impayé dans l'historique généré

| Endpoint | Description |
|----------|-------------|
| `GET /api/AffilieConformite/{affilieId}` | Détail conformité (agent sur ses adhérents, affilié sur son id, admin/financier) |
| `GET /api/AffilieConformite/mes-conformite` | Affilié connecté |
| `GET /api/AffilieConformite` | Liste paginée — filtres : `statutGlobal`, `statutCotisation`, `statutPrestation`, `agentId`, `search` |

```json
{
  "affilieId": 12,
  "statutGlobal": "HORS_ORDRE",
  "statutCotisation": "HORS_ORDRE",
  "statutPrestation": "EN_ORDRE",
  "nombreArrieresOuverts": 2,
  "montantRestantDu": 150.00,
  "arrieresOuverts": [
    {
      "typeObligation": "COTISATION",
      "periode": "05-2026",
      "montantRestant": 80.00,
      "statutPaiement": "EN_RETARD"
    }
  ]
}
```

Les KPIs affilié (`GET /api/DashboardAffilie/kpis/{affilieId}`) exposent aussi `statutGlobal`, `statutCotisation`, `statutPrestation`, `nombreArrieresOuverts`, `montantRestantDu`.

---

## 🏥 Gestion des Affiliés

### 👤 Espace membre — profil et périmètre

Voir aussi [Permissions JWT — espace membre Affilié](#permissions-jwt--espace-membre-affilié).

| Endpoint | Rôle | Description |
|----------|------|-------------|
| `GET /api/Affilie/mon-profil` | Affilié | Profil + personne de contact + synthèse adhésion |
| `GET /api/Affilie/{id}` | Affilié (son id) / staff (`READ_AFFILIE`) | Détail affilié (`AffilieReadDto`) |
| `PUT /api/Affilie/{id}` | Affilié (son id) / staff | Mise à jour |
| `GET /api/Affilie/{id}/photo` | Affilié (son id) / staff | Photo (binaire) |
| `GET /api/Affilie/{id}/carte-identite` | Affilié (son id) / staff | Carte d'identité (binaire) |
| `GET /api/Affilie/{id}/antecedants` | Affilié (son id) / staff (`READ_ANTECEDENT`) | Antécédents paginés (fiche affilié) |
| `GET /api/Affilie/{id}/dependants` | Affilié (son id) / staff (`READ_DEPENDANT`) | Dépendants paginés (fiche affilié) |
| `GET /api/Affilie` | Staff uniquement | Liste paginée — **403** pour le membre |

`AffilieReadDto` (liste `GET /api/Affilie`, détail `GET /api/Affilie/{id}`, create/update, `mon-profil.affilie`) expose aussi :

| Champ | Description |
|-------|-------------|
| `photoBase64` | Contenu photo encodé en base64 (`null` si absente) |
| `photoUrl` | **Même valeur** que `photoBase64` (alias pour le front) |
| `carteIdentiteBase64` | Contenu carte d'identité encodé en base64 (`null` si absente) |
| `hasPhoto` / `hasCarteIdentite` | Indicateurs booléens |
| `dependants` | Liste `DependantReadDto[]` (vide si aucun) — inclut `certificatScolariteBase64` / `certificatScolariteContentType` et `antecedants[]` du dépendant |
| `antecedants` | Liste `AntecedentReadDto[]` de l'affilié (titulaire + liés à l'affilié) |
| `personneContact` | `PersonneContactReadDto` ou `null` |

Sur `GET /api/Affilie/mon-profil`, `personneContact` reste aussi au **racine** du payload (en plus de `affilie.personneContact`) pour compatibilité.

### 👨‍👩‍👧 Dépendants (espace membre)

| Endpoint | Rôle | Description |
|----------|------|-------------|
| `GET /api/Dependant/mes-dependants` | Affilié | Liste paginée de **ses** dépendants |
| `GET /api/Dependant/{id}` | Affilié (si rattaché) / staff | Détail |
| `POST /api/Dependant` | Affilié (force `affilieId` du JWT) / staff | Création |
| `PUT /api/Dependant/{id}` | Affilié (si rattaché) / staff | Mise à jour |
| `GET /api/Dependant/by-affilie/{affilieId}` | Affilié (son id) / staff | Par affilié (équivalent à `GET /api/Affilie/{id}/dependants`) |
| `GET /api/Dependant/{id}/antecedants` | Affilié (si rattaché) / staff (`READ_ANTECEDENT`) | Antécédents paginés du dépendant |
| `GET /api/Dependant` | Staff uniquement | Liste globale — **403** pour le membre |

**Plafond `TypeAdhesion.MaxDependants`** : avant création (`POST`), l’API charge l’adhésion de l’affilié, compte les dépendants déjà rattachés et refuse (**400**) si `count + 1 > MaxDependants` (ex. Solo=0, F3=2, F6=5). Même contrôle sur `PUT` uniquement si `affilieId` change vers un autre affilié (transfert). Affilié sans adhésion → **400**. Le contrôle réutilise `TypeAdhesionDependantsValidationService` (identique au flux d’adhésion).

`DependantReadDto` inclut toujours `antecedants: AntecedentReadDto[]` (tableau vide si aucun), plus `certificatScolariteBase64` / `certificatScolariteContentType` lorsque le justificatif est présent. Seuls les antécédents **du dépendant** y figurent — pas ceux du titulaire. Les antécédents du titulaire restent accessibles via `GET /api/Affilie/{id}/antecedants`, `affilie.antecedants`, ou `GET /api/Antecedent`.

`GET /api/Dependant/{id}/antecedants` reste disponible pour une pagination dédiée des antécédents d’un dépendant.

### 🩺 Antécédents (espace membre)

| Endpoint | Rôle | Description |
|----------|------|-------------|
| `GET /api/Antecedent/mes-antecedents` | Affilié | Liste paginée de **ses** antécédents (titulaire + dépendants) |
| `GET /api/Antecedent/{id}` | Affilié (si rattaché) / staff | Détail |
| `POST /api/Antecedent` | Affilié (force `affilieId` du JWT) / staff | Création (`dependantId` optionnel : `null` = titulaire) |
| `PUT /api/Antecedent/{id}` | Affilié (si rattaché) / staff | Mise à jour (`dependantId` optionnel) |
| `GET /api/Antecedent` | Staff uniquement | Liste globale — **403** pour le membre |

`AntecedentReadDto` inclut `dependantId` et `dependantNom` : `dependantId` à `null` signifie un antécédent du titulaire ; une valeur renseignée indique l’antécédent d’un dépendant du même affilié.

`GET /api/Affilie/{id}/dependants` est l’endpoint recommandé pour l’écran « fiche affilié » lorsqu’on part d’un `affilieId` (équivalent REST de `GET /api/Dependant/by-affilie/{affilieId}`). Chaque `DependantReadDto` inclut `antecedants[]`.

`GET /api/Affilie/{id}/antecedants` est l’endpoint recommandé pour l’écran « fiche affilié » lorsqu’on part d’un `affilieId` (staff ou membre). Il retourne tous les antécédents de l’affilié (titulaire et dépendants) avec `dependantId` / `dependantNom`.

`GET /api/Dependant/{id}/antecedants` est l’endpoint recommandé pour la fiche dépendant : uniquement les antécédents rattachés à ce dépendant.

### 📋 Adhésion (espace membre)

| Endpoint | Rôle | Description |
|----------|------|-------------|
| `GET /api/Adhesion/mon-adhesion` | Affilié | Dossier adhésion complet |
| `GET /api/Adhesion/{id}` | Affilié (si son dossier) / staff (`READ_ADHESION`) | Détail |
| `GET /api/Adhesion`, `paginated`, `advanced` | Staff uniquement | **403** pour le membre |

La **personne de contact** n'a pas de controller dédié : elle est incluse dans `mon-profil` et dans le détail adhésion.

### 🎯 GET /api/affilie/souscriptions
Récupère la liste des souscriptions d'un affilié connecté.

#### 🔍 Requête
```http
GET /api/affilie/souscriptions
Authorization: Bearer {token}
```

#### 📤 Réponse réussie
```json
[
  {
    "id": 1,
    "affilieId": 456,
    "prestationId": 1,
    "prestationNom": "Consultation médicale",
    "prestationDescription": "Consultation générale avec médecin généraliste",
    "dateSouscription": "2026-03-01T00:00:00Z",
    "dateCreation": "2026-03-01T00:00:00Z",
    "statut": true,
    "montantPrestation": 100.00,
    "frequencePaiement": "Mensuel",
    "estDejaPayeeCeMois": false,
    "affilieNom": "kasongo Ntumba billy",
    "affiliePrenom": "billy"
  }
]
```

### 💳 POST /api/affilie/paiement
Permet à un affilié de payer sa souscription.

#### 📋 Corps de la requête
```json
{
  "souscriptionPrestationId": 1,
  "montant": 100.00,
  "modePaiement": "Mobile Money",
  "referencePaiement": "REF-PAY-001",
  "deviseId": 2,
  "observation": "Paiement mensuel Mars 2026"
}
```

#### 📤 Réponse réussie (201 Created)
```json
{
  "idCollecte": 1002,
  "typeCollecte": "Souscription",
  "souscriptionPrestationId": 1,
  "affilieId": 456,
  "agentId": 3,
  "montant": 100.00,
  "referencePaiement": "REF-PAY-001",
  "modePaiement": "Mobile Money",
  "statutPaiement": "VALIDE",
  "montantRecu": 100.00,
  "montantAttendu": 100.00,
  "dateCollecte": "2026-03-16T15:30:00Z",
  "statut": true
}
```

---

## 📱 Guides d'Intégration

### 🎯 React + TypeScript

#### 📦 Installation des dépendances
```bash
npm install axios @types/axios
npm install @types/jsonwebtoken --save-dev
```

#### 🔧 Service API TypeScript
```typescript
// src/services/prosoc-api.ts
export interface Utilisateur {
  idUtilisateur: number;
  nomComplet: string;
  nomUtilisateur: string;
  emailUtilisateur: string;
  photoUrl?: string;
  agentId?: number;
  affilieId?: number;
}

export interface Collecte {
  idCollecte: number;
  typeCollecte: 'Frais' | 'Souscription';
  montant: number;
  modePaiement: string;
  referencePaiement?: string;
}

export interface AdhesionWithAffilieDto {
  nom: string;
  prenom: string;
  postnom: string;
  dateNaissance: string;
  telephone: string;
  emailAffilie: string;
  provinceResidence: string;
  communeResidence: string;
  agentId: number;
  typeAdhesionId: number;
  collectes: Collecte[];
  dependants: any[];
  antecedants: any[];
}

export interface ProsocAPIResponse<T> {
  data: T;
  message?: string;
  errors?: string[];
}

class ProsocAPI {
  private baseURL = 'https://dev-prosoc.asdc-rdc.org/api';
  private token: string;

  constructor(token: string) {
    this.token = token;
  }

  private get headers() {
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${this.token}`
    };
  }

  async login(credentials: { nomUtilisateur: string; motDePasse: string }): Promise<{ accessToken: string; utilisateur: Utilisateur }> {
    const response = await axios.post(
      `${this.baseURL}/utilisateur/login`,
      credentials,
      { headers: { 'Content-Type': 'application/json' } }
    );
    return response.data;
  }

  async createAdhesion(data: AdhesionWithAffilieDto): Promise<ProsocAPIResponse<any>> {
    const response = await axios.post(
      `${this.baseURL}/adhesion/with-affilie`,
      data,
      { headers: this.headers }
    );
    return response.data;
  }

  async getSouscriptions(affilieId: number): Promise<Collecte[]> {
    const response = await axios.get(
      `${this.baseURL}/affilie/souscriptions`,
      { headers: this.headers }
    );
    return response.data;
  }

  async getWalletVirtuel(agentId: number): Promise<any> {
    const response = await axios.get(
      `${this.baseURL}/wallets-virtuels/${agentId}`,
      { headers: this.headers }
    );
    return response.data;
  }
}
```

#### 🎯 Hook React personnalisé
```typescript
// src/hooks/useProsocAPI.ts
import { useMutation, useQuery } from '@tanstack/react-query';
import { ProsocAPI } from '../services/prosoc-api';

export const useProsocAPI = (token: string) => {
  const api = new ProsocAPI(token);

  const createAdhesion = useMutation({
    mutationFn: api.createAdhesion,
    onSuccess: (data) => {
      console.log('Adhésion créée avec succès:', data);
      // Invalider les requêtes en cache
      queryClient.invalidateQueries(['souscriptions']);
      queryClient.invalidateQueries(['wallet']);
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Erreur lors de la création';
      console.error('Erreur création adhésion:', message);
    }
  });

  const getSouscriptions = useQuery({
    queryKey: ['souscriptions'],
    queryFn: () => api.getSouscriptions(affilieId),
    enabled: !!affilieId
  });

  const getWalletVirtuel = useQuery({
    queryKey: ['wallet-virtuel', agentId],
    queryFn: () => api.getWalletVirtuel(agentId),
    enabled: !!agentId
  });

  return { 
    createAdhesion, 
    getSouscriptions, 
    getWalletVirtuel 
  };
};
```

#### 🎯 Composant React d'exemple
```typescript
// src/components/AdhesionForm.tsx
import React from 'react';
import { useProsocAPI } from '../hooks/useProsocAPI';

export const AdhesionForm: React.FC = () => {
  const { createAdhesion } = useProsocAPI('votre_token');
  const [isSubmitting, setIsSubmitting] = React.useState(false);

  const handleSubmit = async (formData: AdhesionWithAffilieDto) => {
    setIsSubmitting(true);
    try {
      await createAdhesion.mutateAsync(formData);
      alert('Adhésion créée avec succès !');
    } catch (error) {
      alert('Erreur lors de la création');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      {/* Champs du formulaire */}
      <button 
        type="submit" 
        disabled={isSubmitting}
        className="btn btn-primary"
      >
        {isSubmitting ? 'Création...' : 'Créer l\'adhésion'}
      </button>
    </form>
  );
};
```

### 🎯 Flutter + Dart

#### 📦 Dépendances dans pubspec.yaml
```yaml
dependencies:
  flutter:
    sdk: flutter
  http: ^0.13.5
  json_annotation: ^4.8.1
  intl: ^0.18.0

dev_dependencies:
  flutter_test:
    sdk: flutter
  json_serializable: ^6.7.1
  build_runner: ^2.4.6
  retrofit_generator: ^8.0.4
```

#### 🔧 Modèles Dart avec JSON Serialization
```dart
// lib/models/adhesion_models.dart
import 'package:json_annotation/json_annotation.dart';

part 'adhesion_models.g.dart';

@JsonSerializable()
class Collecte {
  @JsonKey(name: 'idCollecte')
  final int idCollecte;
  
  @JsonKey(name: 'typeCollecte')
  final String typeCollecte;
  
  @JsonKey(name: 'montant')
  final double montant;
  
  @JsonKey(name: 'modePaiement')
  final String modePaiement;
  
  @JsonKey(name: 'referencePaiement')
  final String? referencePaiement;

  Collecte({
    required this.idCollecte,
    required this.typeCollecte,
    required this.montant,
    required this.modePaiement,
    this.referencePaiement,
  });

  factory Collecte.fromJson(Map<String, dynamic> json) => _$CollecteFromJson(json);
}

@JsonSerializable()
class AdhesionWithAffilieDto {
  final String nom;
  final String prenom;
  final String postnom;
  final String dateNaissance;
  final String telephone;
  final String emailAffilie;
  final String provinceResidence;
  final String communeResidence;
  final int agentId;
  final int typeAdhesionId;
  final List<Collecte> collectes;
  final List<dynamic> dependants;
  final List<dynamic> antecedants;

  AdhesionWithAffilieDto({
    required this.nom,
    required this.prenom,
    required this.postnom,
    required this.dateNaissance,
    required this.telephone,
    required this.emailAffilie,
    required this.provinceResidence,
    required this.communeResidence,
    required this.agentId,
    required this.typeAdhesionId,
    required this.collectes,
    this.dependants = const [],
    this.antecedants = const [],
  });

  factory AdhesionWithAffilieDto.fromJson(Map<String, dynamic> json) => _$AdhesionWithAffilieDtoFromJson(json);
}
```

#### 🔧 Service API Flutter
```dart
// lib/services/prosoc_api_service.dart
import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:prosoc_api/models/adhesion_models.dart';

class ProsocAPIService {
  final String _baseURL = 'https://dev-prosoc.asdc-rdc.org/api';
  final String _token;

  ProsocAPIService(this._token);

  Map<String, String> get _headers => {
    'Content-Type': 'application/json',
    'Authorization': 'Bearer $_token',
  };

  Future<Map<String, dynamic>> login({
    required String nomUtilisateur,
    required String motDePasse,
  }) async {
    final response = await http.post(
      Uri.parse('$_baseURL/utilisateur/login'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'nomUtilisateur': nomUtilisateur,
        'motDePasse': motDePasse,
      }),
    );

    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    } else {
      throw Exception('Erreur de connexion: ${response.statusCode}');
    }
  }

  Future<Map<String, dynamic>> createAdhesion(AdhesionWithAffilieDto data) async {
    final response = await http.post(
      Uri.parse('$_baseURL/adhesion/with-affilie'),
      headers: _headers,
      body: jsonEncode(data.toJson()),
    );

    if (response.statusCode == 201) {
      return jsonDecode(response.body);
    } else {
      final errorData = jsonDecode(response.body);
      throw Exception('Erreur création adhésion: ${errorData['message']}');
    }
  }

  Future<List<dynamic>> getSouscriptions(int affilieId) async {
    final response = await http.get(
      Uri.parse('$_baseURL/affilie/souscriptions'),
      headers: _headers,
    );

    if (response.statusCode == 200) {
      final List<dynamic> data = jsonDecode(response.body);
      return data;
    } else {
      throw Exception('Erreur récupération souscriptions: ${response.statusCode}');
    }
  }
}
```

#### 🎯 Widget Flutter d'exemple
```dart
// lib/widgets/adhesion_form.dart
import 'package:flutter/material.dart';
import 'package:prosoc_api/models/adhesion_models.dart';
import 'package:prosoc_api/services/prosoc_api_service.dart';

class AdhesionForm extends StatefulWidget {
  @override
  _AdhesionFormState createState() => _AdhesionFormState();
}

class _AdhesionFormState extends State<AdhesionForm> {
  final _formKey = GlobalKey<FormState>();
  final _apiService = ProsocAPIService('votre_token');
  bool _isSubmitting = false;

  Future<void> _submitForm() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _isSubmitting = true);

    try {
      final formData = AdhesionWithAffilieDto(
        nom: 'kasongo',
        prenom: 'billy',
        postnom: 'Ntumba',
        dateNaissance: '1980-02-27T09:08:53.467Z',
        telephone: '+24384 8109394',
        emailAffilie: 'billykasongo80@gmail.com',
        provinceResidence: 'Kinshasa',
        communeResidence: 'Selembao',
        agentId: 3,
        typeAdhesionId: 1,
        collectes: [
          Collecte(
            idCollecte: 0,
            typeCollecte: 'Frais',
            montant: 1.5,
            modePaiement: 'VIRTUAL_ACCOUNT',
          ),
        ],
      );

      final result = await _apiService.createAdhesion(formData);
      
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Adhésion créée avec succès!')),
      );
      
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Erreur: ${e.toString()}')),
      );
    } finally {
      setState(() => _isSubmitting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Nouvelle Adhésion')),
      body: Padding(
        padding: EdgeInsets.all(16.0),
        child: Form(
          key: _formKey,
          child: Column(
            children: [
              TextFormField(
                decoration: InputDecoration(labelText: 'Nom'),
                validator: (value) => value?.isEmpty ?? true ? 'Champ requis' : null,
              ),
              SizedBox(height: 16),
              TextFormField(
                decoration: InputDecoration(labelText: 'Prénom'),
                validator: (value) => value?.isEmpty ?? true ? 'Champ requis' : null,
              ),
              SizedBox(height: 24),
              ElevatedButton(
                onPressed: _isSubmitting ? null : _submitForm,
                child: _isSubmitting 
                  ? CircularProgressIndicator(color: Colors.white)
                  : Text('Créer l\'adhésion'),
                style: ElevatedButton.styleFrom(
                  minimumSize: Size(double.infinity, 50),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
```

---

## 🚀 Déploiement

### 🐳 Docker Configuration

#### Dockerfile
```dockerfile
# Étape 1 : Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Prosoc.csproj", "."]
RUN dotnet restore "./Prosoc.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet publish "Prosoc.csproj" -c Release -o /app/publish

# Étape 2 : Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProsocAPI.dll"]
```

#### docker-compose.yml
```yaml
version: '3.8'
services:
  prosoc-api:
    build: .
    ports:
      - "7116:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultServer=Server=mysql;Database=prosocdb;Uid=root;Pwd=password;
      - JWT__Secret=votre_secret_jwt_super_securise
      - JWT__Issuer=https://dev-prosoc.asdc-rdc.org
      - JWT__Audience=https://dev-prosoc.asdc-rdc.org
    depends_on:
      - mysql
      - redis

  mysql:
    image: mysql:8.0
    environment:
      - MYSQL_ROOT_PASSWORD=password
      - MYSQL_DATABASE=prosocdb
    volumes:
      - mysql_data:/var/lib/mysql

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

volumes:
  mysql_data:
```

### 📊 Monitoring et Health Checks

#### Health Check Endpoint
```http
GET /api/health
```

**Réponse healthy :**
```json
{
  "status": "Healthy",
  "timestamp": "2026-03-16T15:30:00Z",
  "version": "2.1.0",
  "uptime": "2.15:30:45",
  "database": "Connected",
  "memory": {
    "allocated": "256MB",
    "used": "180MB"
  }
}
```

#### Metrics Endpoint
```http
GET /api/metrics
Authorization: Bearer {admin_token}
```

---

## 🧪 Testing

### 🔬 Tests unitaires avec Jest
```bash
# Installation
npm install --save-dev jest @types/jest ts-jest

# Exécution
npm test

# Avec couverture
npm test --coverage
```

### 🔬 Tests d'intégration avec Postman
```json
{
  "info": {
    "name": "Prosoc API Collection",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Authentification",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"nomUtilisateur\": \"admin@prosoc.cd\",\n  \"motDePasse\": \"votre_mot_de_passe\"\n}"
        },
        "url": {
          "raw": "{{base_url}}/api/utilisateur/login"
        }
      }
    }
  ]
}
```

---

## 📞 Support et Dépannage

### 🆘 En cas de problème

#### 📋 Checklist de dépannage
1. **Vérifier le statut du service** : https://status.prosoc.cd
2. **Consulter les logs d'erreur** : Application Insights ou logs locaux
3. **Valider le format du token JWT** : Utiliser jwt.io pour décoder
4. **Vérifier les en-têtes HTTP** : Content-Type et Authorization
5. **Tester avec Swagger UI** : https://dev-prosoc.asdc-rdc.org/swagger

#### 🔍 Codes d'erreur communs
| Code HTTP | Catégorie | Action recommandée |
|-----------|------------|-------------------|
| 400 | Validation | Corriger les données envoyées |
| 401 | Authentification | Vérifier le token JWT |
| 403 | Autorisation | Vérifier les permissions |
| 404 | Ressource | Vérifier l'URL et les IDs |
| 429 | Rate Limiting | Attendre avant de réessayer |
| 500 | Serveur | Contacter le support technique |

### 📧 Contact support
- **Email technique** : support@prosoc.cd
- **Documentation** : https://docs.prosoc.cd
- **Status Page** : https://status.prosoc.cd
- **Swagger UI** : https://dev-prosoc.asdc-rdc.org/swagger
- **Health Check** : https://dev-prosoc.asdc-rdc.org/api/health

### 🎯 Temps de réponse cibles
| Type d'endpoint | Temps cible | Temps acceptable |
|-----------------|---------------|----------------|
| Authentification | < 200ms | < 500ms |
| Lecture (GET) | < 300ms | < 800ms |
| Création (POST) | < 500ms | < 1200ms |
| Dashboard | < 800ms | < 1500ms |

---

## 📦 Produits (Mutuel & Assureur)

### ⚠️ Changement breaking (étape 2.1)

| Ancien champ JSON | Nouveau champ | Notes |
|-------------------|---------------|-------|
| `prixMensuel` | `montant` | Montant unitaire du produit (> 0) |
| — | `periodicite` | `Mensuel` ou `Annuel` (insensible à la casse à la saisie) |
| — | `ageMin`, `ageMax` | Tranche d’éligibilité en années entières (ex. `0`–`18`) |

Les réponses `GET/POST/PUT` des contrôleurs `ProduitMutuel` et `ProduitAssureur` exposent ces champs. Une prestation liée est créée/mise à jour avec une description incluant montant, périodicité et tranche d’âge.

### Exemple — création produit mutuel

```http
POST /api/ProduitMutuel
Authorization: Bearer {token}
Content-Type: application/json

{
  "nom": "Pack Famille",
  "montant": 25.00,
  "periodicite": "Mensuel",
  "ageMin": 0,
  "ageMax": 65,
  "deviseId": 1,
  "tauxCommissionAT": 15,
  "tauxCommissionAA": 5,
  "tauxCommissionAAMash": 3,
  "tauxCommissionAAStructure": 2,
  "estGratuit": false,
  "statut": true
}
```

### Taux de commission (4 taux indépendants)

Chaque taux est un pourcentage **indépendant** (0–100), appliqué sur des périmètres métier distincts — ils ne sont pas contraints à totaliser 100 %.

| Champ JSON | Rôle |
|------------|------|
| `tauxCommissionAT` | Agent de terrain (commission collecte souscription = ce taux) |
| `tauxCommissionAA` | Agent administratif |
| `tauxCommissionAAMash` | Commission affectée Maash |
| `tauxCommissionAAStructure` | Commission structure |

**Breaking** : `tauxCommission` (unique) est remplacé par les 4 champs ci-dessus. L’ancienne valeur DB est migrée vers `tauxCommissionAT`.

**Produit assureur** : `autrePrime` est un **alias lecture/écriture** de `tauxCommissionAT` uniquement (AUTRE PRIME métier).

```json
{
  "nom": "Assurance santé",
  "montant": 50,
  "periodicite": "Mensuel",
  "ageMin": 18,
  "ageMax": 65,
  "assureurId": 1,
  "deviseId": 1,
  "autrePrime": 12,
  "tauxCommissionAA": 4,
  "tauxCommissionAAMash": 2,
  "tauxCommissionAAStructure": 1,
  "statut": true
}
```

### Validations métier

- `montant` > 0
- `ageMin` ≥ 0, `ageMax` ≥ `ageMin`, `ageMax` ≤ 120
- `periodicite` : valeurs normalisées en `Mensuel` / `Annuel`
- chaque taux de commission : 0–100 %

Erreurs de validation : réponse **400** avec message explicite (`ArgumentException`).

### Endpoints CRUD

| Méthode | ProduitMutuel | ProduitAssureur |
|---------|---------------|-----------------|
| `GET` | `/api/ProduitMutuel`, `/{id}`, `/actives` | `/api/ProduitAssureur`, `/{id}`, `/actives`, `/by-assureur/{assureurId}` |
| `POST` | `/api/ProduitMutuel` | `/api/ProduitAssureur` |
| `PUT` | `/api/ProduitMutuel/{id}` | `/api/ProduitAssureur/{id}` |
| `DELETE` | `/api/ProduitMutuel/{id}` | `/api/ProduitAssureur/{id}` |

### Synchronisation Prestation (étape 2.6)

À la **création** et à chaque **mise à jour** d’un produit :

- une `Prestation` liée est **créée** si elle n’existe pas ;
- sinon toutes les prestations liées sont **mises à jour** : `nomPrestation`, `montant`, `deviseId`, `description`, `statut` (alignés sur le produit).

La réponse inclut `prestationCree` et `prestationId` après `POST` et `PUT`.

**Suppression** : refusée (**400**) si une souscription existe sur une prestation liée ; sinon les prestations liées sont supprimées puis le produit.

**Références** : `deviseId` et `assureurId` (assureur) doivent exister et être actifs.

### Remarque 1 — Deux catégories de `ProduitMutuel`

| Catégorie métier | Champ API | Règle |
|------------------|-----------|--------|
| **Produit gratuit** (ex. MAASH, inclus dans la cotisation mensuelle) | `estGratuit: true` | `montant = 0` ; commissions AT/AA forcées à **0** ; collecte souscription = **0** |
| **Produit payant** | `estGratuit: false` | `montant > 0` ; commission AT = **`tauxCommissionAT`** propre à chaque produit (peut différer d’un produit à l’autre) |

**Accès** (gratuit ou payant) : l’affilié doit avoir la **cotisation mensuelle à jour** pour la période courante (`TypeCollecte = Cotisation` validée). Sans cotisation réglée, aucun achat/souscription produit mutuel ou assureur.

### Produit gratuit vs payant (`estGratuit`) — détail technique

| Type | `estGratuit` | `montant` | Commission AT (souscription) | Collecte souscription |
|------|--------------|-----------|------------------------------|------------------------|
| **Gratuit** (ex. Maash, lié cotisation) | `true` | `0` obligatoire | `0` % (taux forcés à 0 à l’enregistrement) | Montant collecte = `0` |
| **Payant** | `false` | `> 0` | `tauxCommissionAT` du produit | Montant collecte = `montant` du produit |

```json
{
  "nom": "Service MAASH",
  "montant": 0,
  "estGratuit": true,
  "periodicite": "Mensuel",
  "ageMin": 0,
  "ageMax": 120,
  "deviseId": 1,
  "statut": true
}
```

### Éligibilité achat produit (étape 2.8)

Avant **création de souscription** ou **collecte souscription**, l’API vérifie :

1. **Âge** : l’affilié doit être dans `[ageMin, ageMax]` du produit (âge calculé à la date du jour).
2. **Cotisation à jour** : une collecte `TypeCollecte = Cotisation` valide doit exister pour la **période courante** (mois en cours si cotisation mensuelle, année en cours si annuelle), liée à la grille `CotisationAffilie` du type d’adhésion.

**Exception adhésion niveau 1** : `POST /api/Adhesion/with-affilie` — **FRAIS seuls** suffisent (souscription produit et cotisation **non obligatoires** dans le même payload). La souscription produit peut être faite plus tard via `POST /api/SouscriptionPrestation`. Si une ligne `Cotisation` ou `Souscription` est incluse, elle est validée (montant × personnes assurées / `prestationId`). Pour les affiliés **déjà existants** (`affilieId > 0`), la règle « cotisation à jour » reste appliquée avant tout nouvel achat produit.

Erreurs typiques (**400**) :

- Cotisation non réglée pour la période
- Âge hors tranche du produit
- Produit inactif

### Remarque 2 — Retenue MAASH agents (5 USD/mois)

Pour les **agents** (catégories `AT`, `AA`, `AP`, `AS`, `CA`, `FI`, `IT`, `AD`, etc.), une **retenue à la source de 5 USD/mois** est prélevée sur le **wallet agent** afin de couvrir l’agent et **sa famille** sur le service **MAASH** (`ProduitMutuel` référencé par `NomProduitMaash` dans `appsettings`).

| Élément | Détail |
|---------|--------|
| Montant | `AgentMaash:MontantRetenueUsd` (défaut **5**) |
| Période | Un paiement par agent et par mois (`annee` + `mois`) |
| Débit wallet | `WalletMouvement` : `typeOperation = DEBIT`, `source = RETENUE_MAASH` |
| Bénéficiaires | Liste famille (nom complet, lien parenté, adresse) — mêmes règles que niveau 2 encodeur (`LienParenteRegles`) |

**Configuration** (`appsettings.json` → section `AgentMaash`) :

```json
{
  "MontantRetenueUsd": 5,
  "DeviseId": 2,
  "CodesCategoriesEligibles": [ "AT", "AA", "AP", "AS", "CA", "FI", "IT", "AD" ],
  "NomProduitMaash": "MAASH"
}
```

#### `GET /api/agent-maash/{agentId}/couverture`

Réponse type :

```json
{
  "agentId": 12,
  "nomCompletAgent": "Jean Dupont",
  "estEligible": true,
  "cotisationMaashPayeePourPeriodeCourante": false,
  "montantRetenueMensuelle": 5,
  "deviseId": 2,
  "periodeCourante": "2026-05",
  "dateDerniereRetenue": null,
  "produitMaashId": 3,
  "produitMaashNom": "Service MAASH",
  "beneficiairesFamille": []
}
```

#### `POST /api/agent-maash/{agentId}/retenue`

Corps optionnel :

```json
{
  "annee": 2026,
  "mois": 5,
  "beneficiairesFamille": [
    {
      "nomComplet": "Marie Dupont",
      "lienParente": "CONJOINT",
      "adresse": "12 Av. Liberté, Kinshasa"
    }
  ]
}
```

- Débite le wallet si le solde ≥ 5 USD et si la retenue du mois n’est pas déjà payée.
- Si `beneficiairesFamille` est fourni, remplace la liste enregistrée pour l’agent.

Erreurs typiques (**400**) : agent non éligible, retenue déjà payée pour la période, solde wallet insuffisant, lien de parenté invalide.

#### Retenue automatique (planificateur)

Un service d’arrière-plan (`AgentMaashRetenueBackgroundService`) prévoit la retenue pour **tous les agents éligibles** actifs disposant d’un wallet.

| Paramètre `AgentMaash` | Défaut | Rôle |
|------------------------|--------|------|
| `RetenueAutomatiqueActivee` | `true` | Active/désactive le planificateur |
| `JourExecution` | `1` | À partir de ce jour du mois (1–28) |
| `HeureExecution` | `2` | Heure locale minimale (0–23) |
| `IntervalleControleMinutes` | `60` | Vérification périodique |
| `RetenterEchecsQuotidiennement` | `true` | Re-tente chaque jour les agents non prélevés (ex. solde insuffisant) |

Comportement :

1. Chaque jour ≥ `JourExecution`, après `HeureExecution`, au plus **une exécution par jour**.
2. Pour chaque agent éligible sans retenue sur la période courante : débit wallet (comme `POST …/retenue`).
3. Les agents déjà payés sont ignorés ; les échecs sont journalisés et retentés le lendemain si `RetenterEchecsQuotidiennement` est actif.

**Déclenchement manuel (admin)** :

`POST /api/agent-maash/executer-retenue-automatique`  
Rôles : `Admin`, `SuperAdmin`, `IT`, `Financier`

Réponse type :

```json
{
  "annee": 2026,
  "mois": 5,
  "nbSucces": 42,
  "nbDejaPaye": 3,
  "nbEchec": 2,
  "nbAgentsEligibles": 47,
  "dateExecution": "2026-05-21T02:15:00",
  "echecs": [
    { "agentId": 8, "nomComplet": "Paul K.", "message": "Solde wallet insuffisant..." }
  ]
}
```

> **Distinction** : pour les **affiliés**, MAASH peut rester un produit **gratuit** (`estGratuit: true`) inclus dans la cotisation. Pour les **agents**, la couverture famille passe par la **retenue mensuelle** sur le wallet, indépendamment de la souscription affilié.

---

## 📝 Notes de Version

### 🆕 Version 2.1.1 (Juin 2026)
- ✅ **Espace membre Affilié** : retrait `READ_AFFILIE` / `READ_ADHESION` ; périmètre API (listes 403, accès scopé par `AffilieId`)
- ✅ **Nouveaux endpoints** : `mon-profil`, `mon-adhesion`, `mes-dependants`, `mes-antecedents`
- ✅ **Dashboard Affilié** : protection de toutes les routes `{affilieId}`
- ✅ **Migration SQL** : `sql/MigrateAffilieRolePermissions.idempotent.sql`
- ✅ **TargetAgent par rôle** (breaking change) : `agentId` → `roleNom` ; `GET by-role/{roleNom}` ; dashboard AT lit la cible mensuelle du rôle ; seed 5/25/100 pour `Agent (AT)`
- ✅ **Collecte** : retrait `UPDATE_COLLECTE` pour tous les rôles ; `PUT /api/Collecte/{id}` → 403 ; migration `sql/MigrateRemoveUpdateCollectePermission.idempotent.sql`
- ✅ **Superviseur** : retrait `UPDATE_ADHESION` / `UPDATE_AFFILIE`, `CREATE_ASSUREUR` / `READ_ASSUREUR` / `UPDATE_ASSUREUR` et `CREATE_PRODUIT_ASSUREUR` ; scripts `sql/MigrateRemoveSuperviseurRestrictedPermissions.idempotent.sql` (ciblé) et `sql/MigrateSuperviseurRolePermissions.idempotent.sql` (catalogue) ; reconnexion JWT
- ✅ **Financier** : retrait `UPDATE_AFFILIE` (lecture affilié et `UPDATE_ADHESION` conservés) ; script `sql/MigrateRemoveFinancierUpdateAffilie.idempotent.sql` (ciblé) ; catalogues `sql/MigrateFinancierRolePermissions*.idempotent.sql` ; reconnexion JWT
- ✅ **Financier** : plus d’alimentation wallet virtuel — `UPDATE_WALLET_VIRTUEL` exigée sur les crédits (`ajouter-solde`, ajustements, solde initial) ; script `sql/MigrateRemoveFinancierUpdateWalletVirtuel.idempotent.sql` ; reconnexion JWT
- ✅ **Financier / IT** : `CREATE_DEVISE` + `CREATE_TAUX_CHANGE` (POST `/api/Devise`, POST `/api/Devise/taux-change`) ; script `sql/MigrateFinancierCreateDeviseTaux.idempotent.sql` ; reconnexion JWT
- ✅ **Financier** : `CREATE_FRAIS` (POST `/api/Frais`) ; script `sql/MigrateFinancierCreateFrais.idempotent.sql` ; reconnexion JWT
- ✅ **Financier** : `CREATE`/`UPDATE` ProduitAssureur et ProduitMutuel ; script `sql/MigrateFinancierProduitPermissions.idempotent.sql` ; reconnexion JWT
- ✅ **Financier** : `UPDATE`/`DELETE` SouscriptionPrestation ; script `sql/MigrateFinancierSouscriptionPrestationWrite.idempotent.sql` ; reconnexion JWT
- ✅ **Caissier** : retrait `UPDATE`/`DELETE` SouscriptionPrestation (lecture seule) ; script `sql/MigrateRemoveCaissierSouscriptionPrestationWrite.idempotent.sql` ; reconnexion JWT
- ✅ **Caissier** : `UPDATE_ADHESION` / `UPDATE_AFFILIE` (héritage Percepteur) ; script `sql/MigrateCaissierUpdateAdhesionAffilie.idempotent.sql` pour lier en prod ; reconnexion JWT
- ✅ **Caissier** : `CREATE_DEMANDE_RETRAIT_AGENT` / `READ_DEMANDE_RETRAIT_AGENT` / `VALIDATE_DEMANDE_RETRAIT_AGENT` (create/voir/valider demande ; `CONFIRM_RETRAIT_AGENT` reste le paiement jeton) ; script `sql/MigrateCaissierDemandeRetraitAgentPermissions.idempotent.sql` ; Agent (AT) reçoit CREATE+READ, Superviseur VALIDATE ; reconnexion JWT
- ✅ **Souscription** : blocage d’un nouveau paiement si la période (`mois`/`annee`) est déjà soldée (somme collectes `VALIDE` ≥ tarif) — code `DEJA_PAYEE_PERIODE` ; mapping `mois`/`annee` corrigé sur `POST /api/Collecte`
- ✅ **Agent** : filtre hiérarchique `Role.Niveau` sur `GET /api/Agent` (listes + détail) — un rôle bas ne voit pas les niveaux plus hauts
- ✅ **Wallet virtuel** : `idAgentFrom` / `nomAgentFrom` sur les mouvements (origine de la recharge)
- ✅ **Adhésion FlexPay anonyme** : finalisation sans `UtilisateurId` ; `Adhesions.UtilisateurId` nullable ; script `sql/MigrateAdhesionUtilisateurIdNullable.idempotent.sql`

### 🆕 Version 2.1.0 (Mars 2026)
- ✅ **Produits** : tarification, commissions, gratuit/payant, CRUD, sync `Prestation`, éligibilité âge + cotisation à jour
- ✅ **Endpoint Adhésion** : `/api/adhesion/with-affilie` complètement fonctionnel
- ✅ **Wallet Virtuel** : Débit automatique pour VIRTUAL_ACCOUNT
- ✅ **Commissionnement** : taux dynamique automatique pour les agents
- ✅ **TypeCollecte** : Support chaînes et nombres avec convertisseur
- ✅ **Validation robuste** : Messages d'erreur détaillés
- ✅ **Documentation complète** : Guides d'intégration React/Flutter

### 📜 Historique des versions
- **v2.0.0** : Pagination universelle, authentification unifiée
- **v1.8.0** : Optimisations de performance et dashboards
- **v1.5.0** : Modules retrait agent et jetons médicaux
- **v1.0.0** : Version initiale avec endpoints de base

---

## 🏆 Conclusion

L'API Prosoc v2.1 offre une solution **complète, robuste et évolutive** pour la gestion mutualiste moderne. Avec ses **workflows métier intégrés**, son **système de wallets multiples** et ses **guides d'intégration complets**, elle constitue une base solide pour le développement d'applications professionnelles.

### 🌟 Points forts de la v2.1.0
- **45+ endpoints** opérationnels et documentés
- **Workflows métier** complets (Adhésion → Collecte → Commission)
- **Support multi-mode** de paiement (Mobile Money, VIRTUAL_ACCOUNT, etc.)
- **Architecture RESTful** avec pagination universelle
- **Documentation interactive** avec exemples fonctionnels
- **Guides d'intégration** pour React et Flutter
- **Monitoring intégré** avec health checks et métriques

**Pour commencer l'intégration, consultez le Swagger UI :** `https://dev-prosoc.asdc-rdc.org/swagger`

---

*📅 Dernière mise à jour : 1 Juin 2026*  
*👨‍💻 Auteur : Équipe de développement Prosoc*  
*📄 Version : 2.1.1*  
*🚀 Statut : Production*
