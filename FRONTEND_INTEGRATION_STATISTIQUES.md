# Intégration Frontend — Module Statistiques (Vue.js)

Ce document guide l'intégration côté **frontend web Vue.js** du module **Statistiques** Prosoc.

Référence backend complémentaire : [`docs/API_STATISTIQUES_PROSOC.md`](docs/API_STATISTIQUES_PROSOC.md)

---

## 1) Pré-requis

| Élément | Détail |
|---|---|
| Authentification | JWT Bearer — header `Authorization: Bearer <token>` |
| Permission requise | `READ_STATISTIQUES` (claim JWT `permission`) |
| Rôles autorisés | `Admin`, `Financier`, `Caissier` |
| Base URL API | ex. `https://dev-prosoc.asdc-rdc.org` (sans `/api` à la fin) |
| Format JSON | **camelCase** (sérialisation ASP.NET Core par défaut) |

### Qui peut accéder ?

- **Admin** : bypass côté serveur (`HasPermission` retourne `true` pour Admin/SuperAdmin), mais le claim est quand même présent après migration.
- **Financier** et **Caissier** : doivent avoir le claim `permission: READ_STATISTIQUES` dans le JWT.
- Tout autre rôle sans cette permission reçoit **403**.

> Après déploiement de la permission en production, demander aux utilisateurs concernés de **se reconnecter** pour rafraîchir le JWT.

### Migration SQL production

```bash
mysql -h <host> -u <user> -p <database> < sql/MigrateReadStatistiquesPermission.idempotent.sql
```

---

## 2) Endpoints disponibles

Base : `GET /api/Statistiques/*`

| Route | Description | Filtres période |
|---|---|---|
| `GET /api/Statistiques/generales` | KPIs globaux (affiliés, collectes, arriérés, recouvrement) | Non |
| `GET /api/Statistiques/financieres` | Chiffre d'affaires, évolution mensuelle, modes de paiement | `dateDebut`, `dateFin` |
| `GET /api/Statistiques/operationnelles` | Répartitions territoriales, activité affiliés | Non |
| `GET /api/Statistiques/performance` | Recouvrement par catégorie, top agents | Non |
| `GET /api/Statistiques/consolidees` | **Tous les blocs** en une seule réponse | `dateDebut`, `dateFin` |

### Recommandation UI

| Écran | Endpoint conseillé |
|---|---|
| Dashboard principal (vue d'ensemble) | `consolidees` — **1 seul appel** |
| Onglet « Finances » avec sélecteur de période | `financieres` |
| Onglet « Territoire » | `operationnelles` |
| Onglet « Performance agents » | `performance` |
| Widgets KPI header | `generales` (léger) ou extraire `consolidees.generales` |

---

## 3) Filtres query communs

Tous les endpoints acceptent les mêmes filtres optionnels (logique **AND** côté serveur).

| Paramètre query | Type | Description |
|---|---|---|
| `categorieAdhesionId` | `number?` | Catégorie d'adhésion (`TypeAdhesion.CategorieAdhesionId`) |
| `communeId` | `number?` | Commune (via zone de l'agent créateur) |
| `zoneSocialeId` | `number?` | Zone sociale de l'agent |
| `typeAdhesionId` | `number?` | Type d'adhésion |
| `tarifCotisationId` | `number?` | Tarif de cotisation (`CotisationAffilieId`) |
| `dateDebut` | `string?` | Début de période ISO 8601 — **financieres** et **consolidees** uniquement |
| `dateFin` | `string?` | Fin de période ISO 8601 — **financieres** et **consolidees** uniquement |

Exemple :

```http
GET /api/Statistiques/consolidees?zoneSocialeId=3&categorieAdhesionId=1&dateDebut=2026-01-01&dateFin=2026-06-30
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Alimentation des listes déroulantes (filtres UI)

| Filtre UI | Endpoint catalogue | Permission catalogue typique |
|---|---|---|
| Catégorie d'adhésion | `GET /api/CategorieAdhesion?page=1&pageSize=100` | `READ_CATEGORIE_ADHESION` |
| Type d'adhésion | `GET /api/TypeAdhesion?page=1&pageSize=100` | `READ_TYPE_ADHESION` |
| Commune | `GET /api/Commune?page=1&pageSize=100` | `READ_COMMUNE` |
| Zone sociale | `GET /api/ZoneSociale?page=1&pageSize=100` | `READ_ZONE_SOCIALE` |
| Tarif cotisation | `GET /api/TarifCotisation?page=1&pageSize=100` | `READ_COTISATION_AFFILIE` |

Les rôles Financier et Caissier disposent déjà de ces permissions de lecture catalogue.

---

## 4) Contrats de réponse (TypeScript)

Créer un fichier `src/types/statistiques.ts` :

```ts
/** Filtres envoyés en query string */
export interface StatistiquesFiltres {
  categorieAdhesionId?: number | null;
  communeId?: number | null;
  zoneSocialeId?: number | null;
  typeAdhesionId?: number | null;
  tarifCotisationId?: number | null;
  dateDebut?: string | null; // ISO date, ex. "2026-01-01"
  dateFin?: string | null;
}

export interface StatistiquesGeneralesDto {
  totalAffilies: number;
  nombreObligationsMoisPrecedent: number;
  totalArrieres: number;
  totalCollectesMois: number;
  tauxRecouvrement: number;
  nombreCollectesMois: number;
  dateGeneration: string;
}

export interface EvolutionMensuelleDto {
  mois: string;
  montantObligations: number;
  montantCollectes: number;
  montantArrieres: number;
  nombreObligations: number;
  nombreCollectes: number;
}

export interface RepartitionPaiementDto {
  modePaiement: string;
  montantTotal: number;
  nombreCollectes: number;
  pourcentage: number;
}

export interface StatistiquesFinancieresDto {
  chiffreAffaires: number;
  montantArrieres: number;
  montantPaye: number;
  montantDu: number;
  evolutionMensuelle: EvolutionMensuelleDto[];
  repartitionPaiements: RepartitionPaiementDto[];
  dateGeneration: string;
}

export interface RepartitionAffilieParCategorieDto {
  categorieAdhesionId: number;
  nomCategorie: string;
  nombreAffilies: number;
  pourcentage: number;
  montantTotal: number;
}

export interface RepartitionAffilieParZoneDto {
  zoneSocialeId: number;
  nomZone: string;
  nomCommune: string;
  nombreAffilies: number;
  pourcentage: number;
}

export interface StatistiqueObligationMoisDto {
  mois: string;
  montantTotal: number;
  nombreObligations: number;
  montantMoyen: number;
}

export interface AffilieActiviteDto {
  nombreAffiliesActifs: number;
  nombreAffiliesInactifs: number;
  totalAffilies: number;
  pourcentageActifs: number;
  pourcentageInactifs: number;
}

export interface StatistiquesOperationnellesDto {
  repartitionAffiliesParCategorie: RepartitionAffilieParCategorieDto[];
  repartitionAffiliesParZone: RepartitionAffilieParZoneDto[];
  statistiquesObligationsMois: StatistiqueObligationMoisDto[];
  affilieActivite: AffilieActiviteDto;
  dateGeneration: string;
}

export interface TauxRecouvrementParCategorieDto {
  categorieAdhesionId: number;
  nomCategorie: string;
  tauxRecouvrement: number;
  montantDu: number;
  montantPaye: number;
}

export interface TopAgentDto {
  idAgent: number;
  nomAgent: string;
  roleAgent?: string | null;
  montantCollecte: number;
  nombreCollectes: number;
  tauxConversion: number;
}

export interface StatistiquesPerformanceMensuelleDto {
  mois: string;
  tauxRecouvrement: number;
  montantCollecte: number;
  nombreCollectes: number;
  ticketMoyen: number;
}

export interface StatistiquesPerformanceDto {
  tauxRecouvrementGlobal: number;
  tauxRecouvrementParCategorie: TauxRecouvrementParCategorieDto[];
  topAgents: TopAgentDto[];
  performanceMensuelle: StatistiquesPerformanceMensuelleDto[];
  dateGeneration: string;
}

export interface PeriodeStatistiquesDto {
  dateDebut?: string | null;
  dateFin?: string | null;
  libellePeriode: string;
}

export interface StatistiquesConsolideesDto {
  generales: StatistiquesGeneralesDto;
  financieres: StatistiquesFinancieresDto;
  operationnelles: StatistiquesOperationnellesDto;
  performance: StatistiquesPerformanceDto;
  periode: PeriodeStatistiquesDto;
  dateGeneration: string;
}
```

---

## 5) Exemples de réponses JSON

### Générales (`GET /api/Statistiques/generales`)

```json
{
  "totalAffilies": 120,
  "nombreObligationsMoisPrecedent": 95,
  "totalArrieres": 15000.00,
  "totalCollectesMois": 8500.00,
  "tauxRecouvrement": 72.50,
  "nombreCollectesMois": 42,
  "dateGeneration": "2026-07-10T14:00:00"
}
```

### Financières (extrait)

```json
{
  "chiffreAffaires": 8500.00,
  "montantArrieres": 15000.00,
  "montantPaye": 7200.00,
  "montantDu": 15000.00,
  "evolutionMensuelle": [
    {
      "mois": "juin 2026",
      "montantObligations": 9000.00,
      "montantCollectes": 7200.00,
      "montantArrieres": 1800.00,
      "nombreObligations": 95,
      "nombreCollectes": 38
    }
  ],
  "repartitionPaiements": [
    {
      "modePaiement": "Especes",
      "montantTotal": 5000.00,
      "nombreCollectes": 25,
      "pourcentage": 58.82
    }
  ],
  "dateGeneration": "2026-07-10T14:00:00"
}
```

### Consolidées (structure racine)

```json
{
  "generales": { "...": "..." },
  "financieres": { "...": "..." },
  "operationnelles": { "...": "..." },
  "performance": { "...": "..." },
  "periode": {
    "dateDebut": "2026-01-01T00:00:00",
    "dateFin": "2026-06-30T00:00:00",
    "libellePeriode": "Période personnalisée"
  },
  "dateGeneration": "2026-07-10T14:00:00"
}
```

---

## 6) Service API Vue (Axios)

Créer `src/services/statistiquesApi.ts` :

```ts
import axios, { type AxiosInstance } from 'axios';
import type {
  StatistiquesConsolideesDto,
  StatistiquesFinancieresDto,
  StatistiquesFiltres,
  StatistiquesGeneralesDto,
  StatistiquesOperationnellesDto,
  StatistiquesPerformanceDto,
} from '@/types/statistiques';

function toQueryParams(filtres?: StatistiquesFiltres): Record<string, string | number> {
  if (!filtres) return {};
  const params: Record<string, string | number> = {};
  for (const [key, value] of Object.entries(filtres)) {
    if (value !== null && value !== undefined && value !== '') {
      params[key] = value;
    }
  }
  return params;
}

export function createStatistiquesApi(client: AxiosInstance) {
  return {
    getGenerales: (filtres?: StatistiquesFiltres) =>
      client.get<StatistiquesGeneralesDto>('/api/Statistiques/generales', {
        params: toQueryParams(filtres),
      }),

    getFinancieres: (filtres?: StatistiquesFiltres) =>
      client.get<StatistiquesFinancieresDto>('/api/Statistiques/financieres', {
        params: toQueryParams(filtres),
      }),

    getOperationnelles: (filtres?: StatistiquesFiltres) =>
      client.get<StatistiquesOperationnellesDto>('/api/Statistiques/operationnelles', {
        params: toQueryParams(filtres),
      }),

    getPerformance: (filtres?: StatistiquesFiltres) =>
      client.get<StatistiquesPerformanceDto>('/api/Statistiques/performance', {
        params: toQueryParams(filtres),
      }),

    getConsolidees: (filtres?: StatistiquesFiltres) =>
      client.get<StatistiquesConsolideesDto>('/api/Statistiques/consolidees', {
        params: toQueryParams(filtres),
      }),
  };
}
```

Instance Axios recommandée (`src/plugins/axios.ts`) :

```ts
import axios from 'axios';
import { useAuthStore } from '@/stores/auth';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: { Accept: 'application/json' },
});

apiClient.interceptors.request.use((config) => {
  const auth = useAuthStore();
  if (auth.token) {
    config.headers.Authorization = `Bearer ${auth.token}`;
  }
  return config;
});
```

---

## 7) Composable Vue 3

Créer `src/composables/useStatistiques.ts` :

```ts
import { ref, computed } from 'vue';
import { apiClient } from '@/plugins/axios';
import { createStatistiquesApi } from '@/services/statistiquesApi';
import type { StatistiquesConsolideesDto, StatistiquesFiltres } from '@/types/statistiques';

const statistiquesApi = createStatistiquesApi(apiClient);

export function useStatistiques() {
  const loading = ref(false);
  const error = ref<string | null>(null);
  const data = ref<StatistiquesConsolideesDto | null>(null);
  const filtres = ref<StatistiquesFiltres>({});

  const kpis = computed(() => data.value?.generales ?? null);

  async function chargerConsolidees(override?: StatistiquesFiltres) {
    loading.value = true;
    error.value = null;
    try {
      const params = { ...filtres.value, ...override };
      const { data: response } = await statistiquesApi.getConsolidees(params);
      data.value = response;
    } catch (e: unknown) {
      error.value = extractApiError(e);
      throw e;
    } finally {
      loading.value = false;
    }
  }

  function resetFiltres() {
    filtres.value = {};
  }

  return {
    loading,
    error,
    data,
    filtres,
    kpis,
    chargerConsolidees,
    resetFiltres,
  };
}

function extractApiError(e: unknown): string {
  if (axios.isAxiosError(e)) {
    const status = e.response?.status;
    const message = (e.response?.data as { message?: string })?.message;
    if (status === 403) return message ?? 'Accès refusé — permission READ_STATISTIQUES requise';
    if (status === 401) return 'Session expirée — veuillez vous reconnecter';
    return message ?? 'Erreur lors du chargement des statistiques';
  }
  return 'Erreur inattendue';
}
```

> Ajouter `import axios from 'axios'` en tête du fichier pour `axios.isAxiosError`.

---

## 8) Contrôle d'accès côté Vue

### Lecture des permissions depuis le JWT

Les permissions sont des claims multiples `permission` dans le token :

```ts
// src/utils/permissions.ts
import { jwtDecode } from 'jwt-decode';

interface JwtPayload {
  permission?: string | string[];
  role?: string | string[];
  primaryRole?: string;
}

export function getPermissionsFromToken(token: string): string[] {
  const payload = jwtDecode<JwtPayload>(token);
  const raw = payload.permission;
  if (!raw) return [];
  return Array.isArray(raw) ? raw : [raw];
}

export function hasPermission(token: string, permission: string): boolean {
  const roles = getRolesFromToken(token);
  if (roles.includes('Admin') || roles.includes('SuperAdmin')) return true;
  return getPermissionsFromToken(token).includes(permission);
}

export function getRolesFromToken(token: string): string[] {
  const payload = jwtDecode<JwtPayload>(token);
  const roleClaim =
    payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ??
    payload.role;
  if (!roleClaim) return [];
  return Array.isArray(roleClaim) ? roleClaim : [roleClaim];
}

export const PERMISSION_READ_STATISTIQUES = 'READ_STATISTIQUES';
```

### Garde de route Vue Router

```ts
// src/router/guards/statistiquesGuard.ts
import type { NavigationGuard } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import { hasPermission, PERMISSION_READ_STATISTIQUES } from '@/utils/permissions';

export const statistiquesGuard: NavigationGuard = (_to, _from, next) => {
  const auth = useAuthStore();
  if (!auth.token) return next({ name: 'login' });
  if (!hasPermission(auth.token, PERMISSION_READ_STATISTIQUES)) {
    return next({ name: 'forbidden' });
  }
  next();
};
```

```ts
// src/router/index.ts (extrait)
{
  path: '/statistiques',
  name: 'statistiques',
  component: () => import('@/views/StatistiquesView.vue'),
  beforeEnter: statistiquesGuard,
  meta: { permission: 'READ_STATISTIQUES', title: 'Statistiques' },
}
```

### Directive `v-can` (menu latéral)

```ts
// src/directives/can.ts
import type { Directive } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { hasPermission } from '@/utils/permissions';

export const vCan: Directive<HTMLElement, string> = {
  mounted(el, binding) {
    const auth = useAuthStore();
    if (!auth.token || !hasPermission(auth.token, binding.value)) {
      el.style.display = 'none';
      // ou el.remove() selon votre UX
    }
  },
};
```

Usage dans le menu :

```vue
<router-link v-can="'READ_STATISTIQUES'" to="/statistiques">
  Statistiques
</router-link>
```

---

## 9) Exemple de page Vue (`StatistiquesView.vue`)

```vue
<script setup lang="ts">
import { onMounted } from 'vue';
import { useStatistiques } from '@/composables/useStatistiques';
import KpiCard from '@/components/statistiques/KpiCard.vue';
import FiltresStatistiques from '@/components/statistiques/FiltresStatistiques.vue';
import ChartEvolution from '@/components/statistiques/ChartEvolution.vue';
import ChartRepartitionZone from '@/components/statistiques/ChartRepartitionZone.vue';
import TableTopAgents from '@/components/statistiques/TableTopAgents.vue';

const { loading, error, data, filtres, kpis, chargerConsolidees } = useStatistiques();

onMounted(() => chargerConsolidees());

async function onFiltresChange() {
  await chargerConsolidees();
}
</script>

<template>
  <div class="statistiques-page">
    <header class="page-header">
      <h1>Statistiques</h1>
      <FiltresStatistiques v-model="filtres" @apply="onFiltresChange" />
    </header>

    <div v-if="loading" class="loading">Chargement…</div>
    <div v-else-if="error" class="error">{{ error }}</div>

    <template v-else-if="data">
      <!-- KPIs -->
      <section class="kpi-grid">
        <KpiCard label="Affiliés actifs" :value="kpis?.totalAffilies" />
        <KpiCard label="Collectes du mois" :value="kpis?.totalCollectesMois" format="currency" />
        <KpiCard label="Arriérés" :value="kpis?.totalArrieres" format="currency" />
        <KpiCard label="Taux recouvrement" :value="kpis?.tauxRecouvrement" format="percent" />
      </section>

      <!-- Graphiques -->
      <section class="charts-grid">
        <ChartEvolution :series="data.financieres.evolutionMensuelle" />
        <ChartRepartitionZone :items="data.operationnelles.repartitionAffiliesParZone" />
      </section>

      <!-- Top agents -->
      <TableTopAgents :agents="data.performance.topAgents" />
    </template>
  </div>
</template>
```

---

## 10) Mapping données → composants graphiques

| Donnée API | Composant UI suggéré |
|---|---|
| `generales.*` | Cartes KPI (4 indicateurs) |
| `financieres.evolutionMensuelle` | Graphique ligne/barres (obligations vs collectes) |
| `financieres.repartitionPaiements` | Camembert / donut par `modePaiement` |
| `operationnelles.repartitionAffiliesParCategorie` | Barres horizontales par `nomCategorie` |
| `operationnelles.repartitionAffiliesParZone` | Carte ou barres par `nomZone` / `nomCommune` |
| `operationnelles.affilieActivite` | Jauge actifs/inactifs |
| `performance.tauxRecouvrementParCategorie` | Barres comparatives |
| `performance.topAgents` | Tableau classement (max 10) |
| `performance.performanceMensuelle` | Courbe tendance 6 mois |

### Formatage affichage

```ts
// src/utils/format.ts
export function formatMontant(value: number, devise = 'USD'): string {
  return new Intl.NumberFormat('fr-FR', {
    style: 'currency',
    currency: devise,
    minimumFractionDigits: 2,
  }).format(value);
}

export function formatPourcent(value: number): string {
  return `${value.toFixed(2)} %`;
}
```

---

## 11) Gestion des erreurs

| Code HTTP | Cause | Action UI |
|---|---|---|
| **401** | Token absent ou expiré | Rediriger vers login / refresh token |
| **403** | Permission `READ_STATISTIQUES` manquante | Page « Accès refusé » + masquer le menu |
| **500** | Erreur serveur | Message + bouton « Réessayer » |

Corps 403 typique :

```json
{
  "success": false,
  "message": "Permission requise : READ_STATISTIQUES",
  "timestamp": "2026-07-10T14:00:00Z",
  "traceId": "00-..."
}
```

Corps 500 typique :

```json
{
  "message": "Erreur lors de la récupération des statistiques générales"
}
```

---

## 12) Règles métier utiles pour l'UI

- **Scope global** : pas de filtre société ; les stats couvrent toute la base (filtres territoriaux optionnels).
- **Collectes valides** : seules les collectes au statut de paiement valide sont comptées.
- **Montants** : toujours en **devise principale** (`MontantDevisePrincipale` côté serveur).
- **Taux de recouvrement** : collectes du mois M / obligations attendues du mois M-1.
- **Chiffre d'affaires** (`financieres.chiffreAffaires`) : toujours le mois calendaire en cours, indépendamment de `dateDebut`/`dateFin`.
- **`dateDebut` / `dateFin`** : recalculent principalement `evolutionMensuelle` et `montantPaye` ; les KPIs « mois courant » restent sur le mois actuel.
- **Top agents** : rôles Caissier, Percepteur, AT, AA — tri par montant collecté décroissant, max 10.

---

## 13) Structure de dossiers frontend suggérée

```
src/
├── types/
│   └── statistiques.ts
├── services/
│   └── statistiquesApi.ts
├── composables/
│   └── useStatistiques.ts
├── utils/
│   ├── permissions.ts
│   └── format.ts
├── directives/
│   └── can.ts
├── components/statistiques/
│   ├── KpiCard.vue
│   ├── FiltresStatistiques.vue
│   ├── ChartEvolution.vue
│   ├── ChartRepartitionZone.vue
│   └── TableTopAgents.vue
└── views/
    └── StatistiquesView.vue
```

---

## 14) Checklist d'intégration

- [ ] Variable d'environnement `VITE_API_BASE_URL` configurée
- [ ] Permission `READ_STATISTIQUES` vérifiée avant affichage menu/route
- [ ] Utilisateurs Admin / Financier / Caissier reconnectés après migration SQL
- [ ] Filtres query utilisent les noms Prosoc (`zoneSocialeId`, pas `idAxe`)
- [ ] Dashboard principal utilise `consolidees` pour limiter les appels réseau
- [ ] Sélecteur de période envoie `dateDebut` / `dateFin` au format ISO
- [ ] Gestion 401 / 403 / 500 implémentée
- [ ] Montants formatés en devise principale
- [ ] Tests manuels avec au moins un compte Financier et un compte Caissier

---

## 15) Tests manuels rapides (cURL)

```bash
TOKEN="eyJhbGciOiJIUzI1NiIs..."
BASE="https://dev-prosoc.asdc-rdc.org"

curl -s "$BASE/api/Statistiques/consolidees" \
  -H "Authorization: Bearer $TOKEN" | jq .

curl -s "$BASE/api/Statistiques/generales?zoneSocialeId=1" \
  -H "Authorization: Bearer $TOKEN" | jq .

curl -s "$BASE/api/Statistiques/financieres?dateDebut=2026-01-01&dateFin=2026-06-30" \
  -H "Authorization: Bearer $TOKEN" | jq .
```
