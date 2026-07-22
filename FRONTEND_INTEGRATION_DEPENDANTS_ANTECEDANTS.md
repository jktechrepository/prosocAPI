# Frontend Integration - Dependants et Antecedants

Ce document résume les derniers changements backend a prendre en compte pour l'integration frontend.

## 1) Nouveaux comportements API

### A. Antecedents rattaches a un dependant

Les antecedents peuvent maintenant etre rattaches soit:
- au titulaire (affilie)
- a un dependant specifique

Regle:
- `dependantId = null` -> antecedent du titulaire
- `dependantId = <id>` -> antecedent du dependant

### B. `DependantReadDto` enrichi

Toutes les reponses `DependantReadDto` incluent desormais:
- `antecedants: AntecedentReadDto[]`

Le tableau est toujours present:
- vide (`[]`) si aucun antecedent
- rempli uniquement avec les antecedents du dependant concerne

### C. Nouveau endpoint fiche affilie

Nouveau endpoint:
- `GET /api/Affilie/{id}/dependants`

Objectif:
- recuperer les dependants d'un affilie depuis la fiche affilie (route REST naturelle)

L'endpoint existant est conserve:
- `GET /api/Dependant/by-affilie/{affilieId}`

## 2) Endpoints a utiliser cote frontend

### Recommandes pour fiche affilie

- `GET /api/Affilie/{id}/dependants`
- `GET /api/Affilie/{id}/antecedants`

### Recommandes pour fiche dependant

- `GET /api/Dependant/{id}` (inclut `antecedants[]`)
- `GET /api/Dependant/{id}/antecedants` (liste paginee dediee antecedents dependant)

## 3) Formats de payload (exemples)

### A. Creation antecedent

`POST /api/Antecedent`

```json
{
  "description": "Asthme",
  "affilieId": 123,
  "dependantId": 456,
  "statut": true
}
```

Notes:
- pour un antecedent titulaire, envoyer `dependantId: null` (ou omettre selon le serializer)
- `dependantId` doit appartenir au meme `affilieId`, sinon retour `400`

### B. Lecture dependant (nouveau contenu)

`GET /api/Dependant/{id}`

```json
{
  "idDependant": 456,
  "nom": "Jean Enfant",
  "affilieId": 123,
  "antecedants": [
    {
      "idAntecedant": 77,
      "description": "Asthme",
      "affilieId": 123,
      "dependantId": 456,
      "dependantNom": "Jean Enfant",
      "dateCreation": "2026-07-06T11:20:00Z",
      "dateModification": null,
      "statut": true
    }
  ]
}
```

### C. Liste dependants d'un affilie

`GET /api/Affilie/{id}/dependants?pageNumber=1&pageSize=20`

Reponse:
- `PaginatedResponse<DependantReadDto>`
- chaque item contient `antecedants[]`

## 4) Permissions et scope

- `GET /api/Affilie/{id}/dependants`
  - membre affilie: seulement son propre `id`
  - staff: permission `READ_DEPENDANT`

- `GET /api/Affilie/{id}/antecedants`
  - membre affilie: seulement son propre `id`
  - staff: permission `READ_ANTECEDENT`

- `GET /api/Dependant/{id}/antecedants`
  - membre affilie: dependants rattaches a son compte
  - staff: permission `READ_ANTECEDENT`

## 5) Impacts frontend (checklist)

- Mettre a jour les types/models frontend:
  - `DependantReadDto` -> ajouter `antecedants: AntecedentReadDto[]`
  - `AntecedentCreateDto` / `AntecedentUpdateDto` -> gerer `dependantId?: number | null`

- Ecrans fiche affilie:
  - utiliser `GET /api/Affilie/{id}/dependants` pour la liste dependants
  - conserver `GET /api/Affilie/{id}/antecedants` pour vue globale des antecedents (titulaire + dependants)

- Ecrans dependant:
  - afficher `dependant.antecedants` directement depuis `GET /api/Dependant/{id}`
  - utiliser la route paginee dediee si besoin de pagination/recherche propre antecedents

- Validation formulaire creation/modification antecedent:
  - si mode "dependant", envoyer `dependantId`
  - si mode "titulaire", forcer `dependantId = null`

## 6) Compatibilite

- Pas de rupture sur:
  - `GET /api/Dependant/by-affilie/{affilieId}` (toujours disponible)
  - `GET /api/Dependant/{id}/antecedants` (conserve)

- Nouveau endpoint ajoute:
  - `GET /api/Affilie/{id}/dependants`

