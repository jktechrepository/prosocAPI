# 🔐 SYSTÈME DE PERMISSIONS COMPLET - PROPOSITION

## 📋 Analyse des Permissions Actuelles

### ⚠️ PROBLÈME IDENTIFIÉ

Les permissions actuelles sont **trop génériques** :
- `users.read`, `users.write`, `users.delete`
- `roles.read`, `roles.write`, `roles.delete`
- `permissions.read`, `permissions.write`
- `system.admin`
- `reports.read`
- `financial.read`, `financial.write`

**Il manque des permissions pour** :
- ❌ Agents
- ❌ Affiliés
- ❌ Adhésions
- ❌ Dépendants
- ❌ Produits (Mutuels & Assureurs)
- ❌ Prestations
- ❌ Collectes
- ❌ Bons d'envoi
- ❌ Antécédents
- ❌ Retraits agents
- ❌ Targets agents
- ❌ Provinces/Communes
- ❌ Devises
- ❌ Notifications

---

## 🎯 PROPOSITION : SYSTÈME DE PERMISSIONS GRANULAIRE

### 📐 Structure Recommandée

Format : `{module}.{action}`

**Actions standards** :
- `read` - Lire/Consulter
- `write` - Créer/Modifier
- `delete` - Supprimer
- `manage` - Gestion complète (read + write + delete)

---

## 📊 PERMISSIONS COMPLÈTES PAR MODULE

### 1️⃣ MODULE AUTHENTIFICATION & UTILISATEURS (12 permissions)

| ID | Permission | Description |
|----|-----------|-------------|
| 1 | `users.read` | Consulter les utilisateurs |
| 2 | `users.write` | Créer/modifier les utilisateurs |
| 3 | `users.delete` | Supprimer les utilisateurs |
| 4 | `roles.read` | Consulter les rôles |
| 5 | `roles.write` | Créer/modifier les rôles |
| 6 | `roles.delete` | Supprimer les rôles |
| 7 | `permissions.read` | Consulter les permissions |
| 8 | `permissions.write` | Créer/modifier les permissions |
| 9 | `system.admin` | Administration système complète |
| 10 | `devices.read` | Consulter les appareils connectés |
| 11 | `devices.write` | Gérer les appareils |
| 12 | `devices.delete` | Supprimer les appareils |

### 2️⃣ MODULE AGENTS (9 permissions)

| ID | Permission | Description |
|----|-----------|-------------|
| 13 | `agents.read` | Consulter les agents |
| 14 | `agents.write` | Créer/modifier les agents |
| 15 | `agents.delete` | Supprimer les agents |
| 16 | `agents.wallet.read` | Consulter les wallets agents |
| 17 | `agents.wallet.write` | Gérer les wallets agents |
| 18 | `agents.targets.read` | Consulter les objectifs agents |
| 19 | `agents.targets.write` | Gérer les objectifs agents |
| 20 | `agents.retraits.read` | Consulter les retraits agents |
| 21 | `agents.retraits.write` | Gérer les retraits agents |

### 3️⃣ MODULE AFFILIÉS & ADHÉSIONS (12 permissions)

| ID | Permission | Description |
|----|-----------|-------------|
| 22 | `affilies.read` | Consulter les affiliés |
| 23 | `affilies.write` | Créer/modifier les affiliés |
| 24 | `affilies.delete` | Supprimer les affiliés |
| 25 | `adhesions.read` | Consulter les adhésions |
| 26 | `adhesions.write` | Créer/modifier les adhésions |
| 27 | `adhesions.delete` | Supprimer les adhésions |
| 28 | `dependants.read` | Consulter les dépendants |
| 29 | `dependants.write` | Créer/modifier les dépendants |
| 30 | `dependants.delete` | Supprimer les dépendants |
| 31 | `type-adhesions.read` | Consulter les types d'adhésion |
| 32 | `type-adhesions.write` | Créer/modifier les types d'adhésion |
| 33 | `type-adhesions.delete` | Supprimer les types d'adhésion |

### 4️⃣ MODULE PRODUITS & PRESTATIONS (15 permissions)

| ID | Permission | Description |
|----|-----------|-------------|
| 34 | `produits-mutuels.read` | Consulter les produits mutuels |
| 35 | `produits-mutuels.write` | Créer/modifier les produits mutuels |
| 36 | `produits-mutuels.delete` | Supprimer les produits mutuels |
| 37 | `produits-assureurs.read` | Consulter les produits assureurs |
| 38 | `produits-assureurs.write` | Créer/modifier les produits assureurs |
| 39 | `produits-assureurs.delete` | Supprimer les produits assureurs |
| 40 | `assureurs.read` | Consulter les assureurs |
| 41 | `assureurs.write` | Créer/modifier les assureurs |
| 42 | `assureurs.delete` | Supprimer les assureurs |
| 43 | `prestations.read` | Consulter les prestations |
| 44 | `prestations.write` | Créer/modifier les prestations |
| 45 | `prestations.delete` | Supprimer les prestations |
| 46 | `bons-envoi.read` | Consulter les bons d'envoi |
| 47 | `bons-envoi.write` | Créer/modifier les bons d'envoi |
| 48 | `bons-envoi.delete` | Supprimer les bons d'envoi |

### 5️⃣ MODULE FINANCIER (9 permissions)

| ID | Permission | Description |
|----|-----------|-------------|
| 49 | `collectes.read` | Consulter les collectes |
| 50 | `collectes.write` | Créer/modifier les collectes |
| 51 | `collectes.delete` | Supprimer les collectes |
| 52 | `devises.read` | Consulter les devises |
| 53 | `devises.write` | Créer/modifier les devises |
| 54 | `devises.delete` | Supprimer les devises |
| 55 | `financial.reports` | Consulter les rapports financiers |
| 56 | `financial.stats` | Consulter les statistiques financières |
| 57 | `financial.export` | Exporter les données financières |

### 6️⃣ MODULE MÉDICAL (6 permissions)

| ID | Permission | Description |
|----|-----------|-------------|
| 58 | `antecedents.read` | Consulter les antécédents médicaux |
| 59 | `antecedents.write` | Créer/modifier les antécédents |
| 60 | `antecedents.delete` | Supprimer les antécédents |
| 61 | `souscriptions.read` | Consulter les souscriptions prestations |
| 62 | `souscriptions.write` | Créer/modifier les souscriptions |
| 63 | `souscriptions.delete` | Supprimer les souscriptions |

### 7️⃣ MODULE GÉOGRAPHIQUE (6 permissions)

| ID | Permission | Description |
|----|-----------|-------------|
| 64 | `provinces.read` | Consulter les provinces |
| 65 | `provinces.write` | Créer/modifier les provinces |
| 66 | `provinces.delete` | Supprimer les provinces |
| 67 | `communes.read` | Consulter les communes |
| 68 | `communes.write` | Créer/modifier les communes |
| 69 | `communes.delete` | Supprimer les communes |

### 8️⃣ MODULE NOTIFICATIONS & COMMUNICATION (6 permissions)

| ID | Permission | Description |
|----|-----------|-------------|
| 70 | `notifications.read` | Consulter les notifications |
| 71 | `notifications.write` | Créer/envoyer des notifications |
| 72 | `notifications.delete` | Supprimer les notifications |
| 73 | `sms.send` | Envoyer des SMS |
| 74 | `emails.send` | Envoyer des emails |
| 75 | `push.send` | Envoyer des notifications push |

### 9️⃣ MODULE RAPPORTS & ANALYTICS (6 permissions)

| ID | Permission | Description |
|----|-----------|-------------|
| 76 | `reports.dashboard` | Accéder au tableau de bord |
| 77 | `reports.agents` | Rapports sur les agents |
| 78 | `reports.affilies` | Rapports sur les affiliés |
| 79 | `reports.financial` | Rapports financiers |
| 80 | `reports.export` | Exporter les rapports |
| 81 | `reports.custom` | Créer des rapports personnalisés |

---

## 🎯 ATTRIBUTION DES PERMISSIONS PAR RÔLE

### Super-Admin (TOUTES - 81 permissions)
✅ Accès complet à toutes les permissions

### Admin (60 permissions)
✅ Toutes les permissions SAUF :
- ❌ `*.delete` (suppression)
- ❌ `system.admin`
- ❌ `permissions.write`
- ❌ `roles.delete`
- ❌ `users.delete`

### Superviseur (30 permissions)
✅ Permissions de lecture + rapports :
- ✅ Tous les `*.read`
- ✅ Tous les `reports.*`
- ❌ Pas de `*.write` ni `*.delete`

### Agent (AT) - Agent de Terrain (15 permissions)
✅ Permissions opérationnelles :
- ✅ `affilies.read`, `affilies.write`
- ✅ `adhesions.read`, `adhesions.write`
- ✅ `dependants.read`, `dependants.write`
- ✅ `collectes.read`, `collectes.write`
- ✅ `agents.wallet.read`
- ✅ `agents.targets.read`
- ✅ `notifications.read`

### Agent (AA) - Agent Administratif (20 permissions)
✅ Permissions administratives :
- ✅ `affilies.*`
- ✅ `adhesions.*`
- ✅ `dependants.*`
- ✅ `prestations.read`, `prestations.write`
- ✅ `bons-envoi.*`
- ✅ `reports.affilies`

### Affilié (5 permissions)
✅ Permissions limitées :
- ✅ `affilies.read` (son propre profil)
- ✅ `dependants.read` (ses dépendants)
- ✅ `prestations.read`
- ✅ `bons-envoi.read`
- ✅ `notifications.read`

---

## 📝 PROCHAINES ÉTAPES

1. **Créer une migration** pour ajouter toutes ces permissions
2. **Créer un script SQL** pour insérer les permissions
3. **Mettre à jour les RolePermissions** pour chaque rôle
4. **Ajouter des attributs d'autorisation** dans les contrôleurs
5. **Créer des tests** pour vérifier les permissions

---

## 💡 RECOMMANDATION

**Approche Progressive** :
1. ✅ Commencer par les modules critiques (Agents, Affiliés, Adhésions)
2. ✅ Ajouter progressivement les autres modules
3. ✅ Tester après chaque ajout
4. ✅ Documenter les changements

**Voulez-vous que je crée** :
- Un script SQL pour ajouter toutes ces permissions ?
- Une migration EF Core ?
- Les deux ?
