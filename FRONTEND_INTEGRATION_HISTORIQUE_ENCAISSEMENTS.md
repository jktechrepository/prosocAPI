# Intégration Frontend — Historique des encaissements (Caissier / Percepteur)

Guide Flutter (ou mobile) pour afficher l’**historique des encaissements** selon le rôle connecté.

Référence backend : [`API-DOCUMENTATION-NEW.md`](API-DOCUMENTATION-NEW.md) (§ Dashboard Caissier, Caisse guichet, Perception virtuelle, Dashboard Percepteur).

---

## 1) Pré-requis

| Élément | Détail |
|--------|--------|
| Authentification | JWT Bearer — `Authorization: Bearer <token>` |
| Rôles | `Caissier`, `Percepteur` (et `Admin` / `Financier` selon endpoint) |
| Base URL | ex. `https://dev-prosoc.asdc-rdc.org` |
| JSON | **camelCase** |

---

## 2) Quel endpoint selon le rôle ?

Il n’existe **pas** d’endpoint unique « Historique des encaissements ». Brancher l’UI selon le **type** d’encaissement :

| Rôle | Type | Endpoint principal | Usage |
|------|------|-------------------|--------|
| **Caissier** | Collectes guichet (mes saisies) | `GET /api/DashboardCaissier/collectes` | Historique paginé + filtres dates / mode |
| **Caissier** | Aperçu rapide (accueil) | `GET /api/DashboardCaissier/collectes-recentes?limit=50` | Dernières N lignes sans pagination |
| **Caissier** | Journal caisse (session) | `GET /api/Caisse/sessions` puis `GET /api/Caisse/session/{id}/mouvements` | Entrées espèce / électronique, sorties retraits |
| **Percepteur** | Perceptions compte virtuel (AT) | `GET /api/PerceptionVirtuelle/historique` | Journal des perceptions confirmées |
| **Percepteur** | Collectes guichet (si saisie directe) | `GET /api/DashboardPercepteur/mes-collectes-guichet` | Même format que caissier, filtré `OperateurUtilisateurId` |
| **Percepteur** | Rapport opérationnel global | `GET /api/DashboardPercepteur/rapport-perception` | VA + guichet, **vue globale** (filtres manuels) |

### À ne pas utiliser comme « mes encaissements »

| Endpoint | Pourquoi |
|----------|----------|
| `GET /api/DashboardPercepteur/transactions` | Dernières collectes **globales** (tous opérateurs), pas le périmètre personnel |
| `GET /api/Collecte` | Liste toutes les collectes sans filtre opérateur |

---

## 3) Caissier — collectes guichet

### 3.1 Historique paginé (écran principal)

```http
GET /api/DashboardCaissier/collectes?pageNumber=1&pageSize=20&dateDebut=2026-07-01&dateFin=2026-07-16&modePaiement=ESPECE
Authorization: Bearer <token>
```

| Query | Type | Description |
|-------|------|-------------|
| `pageNumber` | int | Page (défaut 1) |
| `pageSize` | int | Taille (1–100, défaut 20) |
| `dateDebut` | date? | Filtre `DateCollecte >=` |
| `dateFin` | date? | Filtre `DateCollecte <=` |
| `modePaiement` | string? | ex. `ESPECE`, `MOBILE_MONEY` |

Réponse : `PaginatedResponse<CaissierCollecteDto>` (`data`, `totalItems`, `hasNextPage`, …).

Champs ligne : `idCollecte`, `dateCollecte`, `montant`, `typeCollecte`, `statut`, `reference`, `nomAffilie`, `modePaiement`, `notes`.

### 3.2 Collectes récentes (widget accueil)

```http
GET /api/DashboardCaissier/collectes-recentes?limit=20
```

Liste non paginée, même périmètre (`OperateurUtilisateurId` = utilisateur JWT).

### 3.3 Journal de caisse multi-sessions

**Étape 1** — Lister les sessions :

```http
GET /api/Caisse/sessions?pageNumber=1&pageSize=10&statut=CLOTUREE
```

Filtres optionnels : `dateDebut`, `dateFin`, `statut` (`OUVERTE` | `CLOTUREE`).

**Étape 2** — Mouvements d’une session :

```http
GET /api/Caisse/session/{idSessionCaisse}/mouvements?pageNumber=1&pageSize=50
```

Sources utiles : `COLLECTE_ESPECE`, `COLLECTE_ELECTRONIQUE`, `RETRAIT_AGENT`.

**Session courante** (guichet ouvert) :

```http
GET /api/Caisse/session/courante
```

### Exemple Dart (collectes paginées)

```dart
Future<PaginatedCollectes> fetchHistoriqueCaissier({
  required int page,
  DateTime? dateDebut,
  DateTime? dateFin,
}) async {
  final q = {
    'pageNumber': '$page',
    'pageSize': '20',
    if (dateDebut != null) 'dateDebut': dateDebut.toIso8601String().split('T').first,
    if (dateFin != null) 'dateFin': dateFin.toIso8601String().split('T').first,
  };
  final uri = Uri.parse('$baseUrl/api/DashboardCaissier/collectes').replace(queryParameters: q);
  final res = await http.get(uri, headers: {'Authorization': 'Bearer $token'});
  if (res.statusCode != 200) throw Exception(res.body);
  return PaginatedCollectes.fromJson(jsonDecode(res.body));
}
```

---

## 4) Percepteur — perceptions virtuelles (VA)

### 4.1 Historique personnel (recommandé)

```http
GET /api/PerceptionVirtuelle/historique?pageNumber=1&pageSize=20&dateDebut=2026-07-01&dateFin=2026-07-16
Authorization: Bearer <token>
```

Filtre serveur : `PercepteurUtilisateurId` = utilisateur connecté.

Réponse : `PaginatedResponse<PerceptionVirtuelleReadDto>` avec `lignes` (détail collectes par perception).

### 4.2 Détail d’une perception

```http
GET /api/PerceptionVirtuelle/{idPerceptionVirtuelle}
```

### 4.3 Collectes guichet saisies par le percepteur

Si le percepteur encaisse aussi au guichet :

```http
GET /api/DashboardPercepteur/mes-collectes-guichet?pageNumber=1&pageSize=20
```

Mêmes query que `DashboardCaissier/collectes` (`dateDebut`, `dateFin`, `modePaiement`).

### 4.4 Rapport global (supervision, pas « mon historique »)

```http
GET /api/DashboardPercepteur/rapport-perception?origine=AGENT&statut=PERCU&pageNumber=1&pageSize=20
```

Paramètres : `origine` (`AGENT` | `AFFILIE` | `TOUS`), `statut` (`EN_ATTENTE` | `PERCU` | `TOUS`), `agentId`, `affilieId`, dates.

---

## 5) Parcours UI recommandé

```text
Connexion → lire rôle JWT
  ├─ Caissier
  │    ├─ Onglet « Mes collectes » → DashboardCaissier/collectes (pagination infinie)
  │    └─ Onglet « Caisse » → Caisse/sessions → mouvements par session
  └─ Percepteur
       ├─ Onglet « Perceptions VA » → PerceptionVirtuelle/historique
       ├─ Onglet « Mes collectes guichet » (optionnel) → mes-collectes-guichet
       └─ Onglet « Rapport » (supervision) → rapport-perception
```

---

## 6) Codes HTTP courants

| Code | Cause |
|------|--------|
| 401 | JWT absent / expiré |
| 403 | Rôle non autorisé sur le contrôleur |
| 404 | `Caisse/session/courante` sans session ouverte |

---

## 7) KPIs complémentaires

| Rôle | Endpoint | Usage |
|------|----------|--------|
| Caissier | `GET /api/DashboardCaissier/kpis` | Totaux jour / semaine / mois |
| Percepteur | `GET /api/DashboardPercepteur/kpis` | Vue agrégée perception (global) |

Les KPIs ne remplacent pas la liste détaillée de l’historique.
