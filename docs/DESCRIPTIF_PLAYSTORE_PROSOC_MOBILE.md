# PROSOC Mobile — Descriptif pour fiche Google Play Store

**Document de référence** pour rédiger la fiche Play Store de l’application mobile PROSOC (agents de terrain et profils associés).  
**Source technique** : API Prosoc v2.1 (`API-DOCUMENTATION-NEW.md`, dashboards agents, wallets, retraits).  
**Date** : juin 2026

---

## 1. Informations générales (console Play)

| Champ Play Console | Proposition |
|--------------------|-------------|
| **Nom de l’application** | PROSOC Agent |
| **Nom du développeur** | Kansa Business / ASDC (à confirmer selon compte éditeur) |
| **Catégorie** | Entreprise (Business) |
| **Catégorie secondaire** | Finance (optionnel) |
| **Classification du contenu** | Tous publics (outil professionnel B2B) |
| **Site web** | `https://prosoc.maash.com` (ou URL officielle mutuelle) |
| **E-mail de contact** | support@… (à compléter) |
| **Politique de confidentialité** | URL obligatoire (à héberger) |

---

## 2. Description courte (max. 80 caractères)

**Option A (recommandée)**  
`Gérez adhésions, collectes et commissions PROSOC depuis le terrain.`

**Option B**  
`Application officielle des agents PROSOC : adhésion, collecte, wallet.`

**Option C**  
`Mutuelle PROSOC : collecte terrain, suivi adhérents et retraits agents.`

---

## 3. Description complète (fiche Play Store)

Copier-coller et adapter le nom de l’organisation si besoin.

---

**PROSOC Agent** est l’application mobile officielle de la plateforme **PROSOC**, solution de gestion mutualiste pour les réseaux d’agents en République Démocratique du Congo et en Afrique centrale.

Conçue pour les **agents de terrain**, les **chefs d’équipe** et les profils opérationnels du réseau, l’application vous permet de travailler au contact des adhérents tout en restant connecté au système central de la mutuelle.

### Pourquoi PROSOC Agent ?

- **Moins de paperasse** : enregistrez les adhésions et les informations des bénéficiaires directement sur le terrain.
- **Collecte fiable** : enregistrez cotisations, frais et souscriptions avec plusieurs modes de paiement (espèces, mobile money, compte virtuel).
- **Transparence financière** : consultez vos commissions, votre solde wallet et l’historique de vos mouvements en temps réel.
- **Pilotage de votre activité** : tableaux de bord, objectifs mensuels, graphiques et suivi de vos adhérents.
- **Retraits encadrés** : demandez le retrait de vos commissions selon le calendrier officiel de la mutuelle.

### Fonctionnalités principales

**Adhésion & adhérents**
- Création d’adhésions (particulier, famille, entreprise) avec pièces justificatives (photo, carte d’identité).
- Saisie des dépendants et antécédents médicaux.
- Suivi des dossiers et des adhérents récents.
- Alertes sur les cotisations en retard ou à jour.

**Collecte & paiements**
- Enregistrement des collectes : frais d’adhésion, cotisations périodiques, souscriptions aux produits mutuels ou assureur.
- Prise en charge du **mobile money** et des paiements électroniques (FlexPay) avec confirmation en temps réel.
- Gestion multi-devises (CDF, USD) avec conversion automatique côté serveur.
- Consultation des collectes en attente de validation.

**Commissions & wallet agent**
- Visualisation du solde commission (devise principale, ex. USD).
- Historique détaillé des mouvements (crédits commission, retenues, retraits).
- Primes générées sur les souscriptions (assurance / mutuelle).
- Wallet virtuel pour certains modes de paiement terrain.

**Retraits agents**
- Vérification du solde disponible avant demande.
- Création de demandes de retrait pendant les **périodes autorisées** (15–20 et à partir du 30 de chaque mois).
- Suivi du statut : en attente, validée, traitée ou rejetée.
- Jeton de retrait sécurisé pour le retrait effectif en agence.

**Tableau de bord & performance**
- KPIs du mois : collectes, commissions, nombre d’adhésions.
- Graphiques d’activité (collectes, adhésions, commissions).
- Objectifs mensuels vs réalisé (cibles par rôle).
- Vue consolidée « terrain » : primes, commissions, suivi adhérents.

**Encadrement (chefs d’équipe)**
- Vue de la zone sociale : performance des agents de terrain.
- Consultation des collectes et mouvements wallet des agents de l’équipe (selon périmètre territorial).

**Notifications**
- Alertes push (Firebase) sur les événements importants : validations, commissions, statuts de paiement.
- Notifications in-app et synchronisation temps réel (SignalR) pour les paiements FlexPay.

**Sécurité & accès**
- Connexion sécurisée par compte professionnel (identifiant, e-mail ou téléphone).
- Authentification JWT avec renouvellement de session.
- Droits d’accès selon le rôle (agent terrain, chef d’équipe, encodeur, etc.) : chaque utilisateur ne voit que ses données autorisées.

### Public concerné

Cette application est réservée aux **agents et collaborateurs autorisés** du réseau PROSOC. Un compte fourni par votre superviseur ou par l’administration de la mutuelle est nécessaire pour se connecter.

### Données et confidentialité

L’application traite des données professionnelles et, le cas échéant, des **données personnelles et de santé** des adhérents (identité, coordonnées, antécédents) dans le cadre strict de la gestion mutualiste. Les données sont transmises de manière chiffrée (HTTPS) vers les serveurs PROSOC et ne sont pas vendues à des tiers.

Consultez notre politique de confidentialité pour le détail des traitements, durées de conservation et droits des personnes.

### Support

Pour toute assistance technique ou demande d’accès : contactez votre superviseur régional ou le support PROSOC à [e-mail support].

---

*PROSOC — Plateforme de gestion mutualiste. Développée par Kansa Business.*

---

## 4. Liste « Nouveautés de cette version » (exemple)

À personnaliser à chaque release.

```
• Retraits en devise principale (USD) avec vérification de solde améliorée
• Réservation automatique du solde lors d’une demande de retrait
• Tableau de bord agent consolidé (KPIs, primes, commissions)
• Paiements mobile money et carte via FlexPay avec notification temps réel
• Corrections de stabilité et performances
```

---

## 5. Points forts (bullets pour visuels / bannière)

- Adhésion terrain complète (photo, pièce d’identité, dépendants)
- Collecte multi-modes : espèces, mobile money, compte virtuel
- Commissions automatiques et wallet en temps réel
- Retraits agents sécurisés par jeton
- Dashboard avec objectifs et graphiques
- Notifications push et suivi des paiements
- Multi-devises CDF / USD
- Accès sécurisé par rôle professionnel

---

## 6. Mots-clés & balises (ASO)

**Français** : mutuelle, assurance, collecte, agent terrain, cotisation, adhésion, commission, wallet, mobile money, RDC, Congo, PROSOC, MAASH, santé, réseau agents

**Anglais (optionnel)** : mutual insurance, field agent, premium collection, membership, commission wallet

---

## 7. Fiche « Sécurité des données » (Play Console — brouillon)

À valider avec le DPO / responsable juridique avant publication.

| Type de données | Collectées ? | Partagées ? | Finalité |
|-----------------|-------------|-------------|----------|
| Nom, prénom | Oui | Non (sauf obligations légales) | Compte agent, adhésions |
| Adresse e-mail | Oui | Non | Authentification, notifications |
| Numéro de téléphone | Oui | Non | Authentification, mobile money, SMS |
| Photos (adhérents) | Oui | Non | Dossier d’adhésion |
| Documents d’identité | Oui | Non | Vérification adhésion |
| Données de santé (antécédents) | Oui | Non | Gestion mutualiste / prestations |
| Informations financières (collectes, wallet) | Oui | Non | Comptabilité agent, retraits |
| Identifiants d’appareil | Oui | Non | Notifications push (Firebase) |
| Position approximative | Optionnel* | Non | Si fonctionnalité terrain activée |

\* À activer uniquement si l’app mobile utilise réellement la géolocalisation (`location_tracking` dans la config mobile).

**Chiffrement** : données en transit chiffrées (TLS/HTTPS).  
**Suppression de compte** : sur demande auprès de l’administrateur mutuelle (pas d’auto-suppression grand public).

---

## 8. Permissions Android — justifications (texte aide Play)

| Permission | Justification utilisateur |
|------------|---------------------------|
| **Internet** | Connexion aux serveurs PROSOC pour synchroniser adhésions, collectes et soldes. |
| **Appareil photo** | Photographier l’adhérent et sa pièce d’identité lors de l’adhésion. |
| **Stockage / Photos** | Joindre des documents ou images existantes au dossier adhérent. |
| **Notifications** | Recevoir les alertes de validation, commissions et statuts de paiement. |
| **Téléphone** (si lecture état) | Optionnel — identification de l’appareil pour la session sécurisée. |
| **Localisation** | Uniquement si activée : contextualiser la collecte terrain (à confirmer selon build). |

---

## 9. Captures d’écran — scènes suggérées

Pour maximiser la conversion sur le Play Store, prévoir 6 à 8 captures :

1. **Écran de connexion** — aspect professionnel, logo PROSOC  
2. **Dashboard agent** — KPIs du mois, solde commission  
3. **Liste adhérents / suivi** — cotisations à jour / alertes  
4. **Formulaire d’adhésion** — saisie affilié simplifiée  
5. **Écran collecte** — choix type + mode de paiement  
6. **Wallet / mouvements** — historique commissions  
7. **Demande de retrait** — montant et statut  
8. **Notification** — paiement FlexPay confirmé (optionnel)

**Format** : 1080×1920 ou 1080×2340 (portrait). Éviter les données personnelles réelles sur les captures (utiliser des données de démo).

---

## 10. Annexes techniques (équipe produit / dev)

Mapping synthétique **fonctionnalité mobile → API** (base : `https://dev-prosoc.asdc-rdc.org` en dev ; URL prod à confirmer).

| Fonctionnalité app | Endpoints API principaux |
|--------------------|--------------------------|
| Connexion | `POST /api/utilisateur/login`, `POST /api/utilisateur/refresh`, `GET /api/utilisateur/me` |
| Dashboard terrain | `GET /api/DashboardAgent/terrain`, `/kpis`, `/graphs`, `/objectifs` |
| Commissions & solde | `GET /api/DashboardAgent/commissions-resume`, `GET /api/wallets-agents/{agentId}` |
| Mouvements wallet | `GET /api/WalletMouvement/by-agent/{agentId}/paginated` |
| Adhésion terrain | `POST /api/adhesion/with-affilie`, `GET /api/adhesion/{id}` |
| Collecte | `POST /api/Collecte`, FlexPay `POST /api/Collecte/with-paiement-electronique` |
| Suivi adhérents | `GET /api/DashboardAgent/suivi-adherents` |
| Retrait agent | `POST /api/RetraitAgent`, `POST /api/RetraitAgent/verifier-solde` |
| Chef d’équipe | `GET /api/DashboardChefEquipe/kpis`, `/agents` |
| Notifications temps réel | SignalR `notificationHub`, `FlexPayPaymentUpdated` |
| Config mobile | Endpoints `MobileAppConfig` / sync (selon build client) |

**Rôles mobiles typiques** : `Agent (AT)`, `Agent (AA)`, `Chef d'équipe`, éventuellement `Superviseur`.

**Devises** : devise principale **USD** pour commissions et retraits ; collectes possibles en **CDF** avec conversion serveur.

---

## 11. Checklist avant publication

- [ ] URL politique de confidentialité en ligne
- [ ] Compte développeur Google Play actif
- [ ] Icône 512×512 et bannière feature graphic 1024×500
- [ ] Captures d’écran (min. 2, recommandé 6+)
- [ ] Description courte ≤ 80 caractères
- [ ] Classification du contenu remplie
- [ ] Formulaire sécurité des données complété
- [ ] APK/AAB signé (release) testé sur Android 8+
- [ ] Comptes de test fournis aux reviewers Google (login / mot de passe démo)
- [ ] Mention « réservé aux agents autorisés » visible dans la description

---

## 12. Texte pour les reviewers Google (notes de version interne)

```
Application B2B réservée aux agents professionnels de la mutuelle PROSOC.
Identifiants de démonstration :
  Utilisateur : [demo_agent@prosoc.cd]
  Mot de passe : [à fournir]

L’application nécessite un compte préalablement créé par l’administrateur.
Elle ne permet pas l’inscription publique.
```

---

*Document généré à partir de la documentation API Prosoc v2.1 — à adapter selon le nom commercial final de l’app sur le store.*
