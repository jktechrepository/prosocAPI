# API Statistiques Prosoc

## Endpoints

| Methode | Route | Description |
|---|---|---|
| GET | `/api/Statistiques/generales` | KPIs generaux |
| GET | `/api/Statistiques/financieres` | Stats financieres |
| GET | `/api/Statistiques/operationnelles` | Repartitions territoriales et activite |
| GET | `/api/Statistiques/performance` | Recouvrement et top agents |
| GET | `/api/Statistiques/consolidees` | Tous les blocs en une reponse |

Authentification requise (`[Authorize]`). Permission requise : **`READ_STATISTIQUES`** (rôles `Admin`, `Financier`, `Caissier`).

Documentation intégration frontend Vue.js : [`FRONTEND_INTEGRATION_STATISTIQUES.md`](../FRONTEND_INTEGRATION_STATISTIQUES.md)

## Filtres query communs

| Parametre | Type | Description |
|---|---|---|
| `categorieAdhesionId` | int? | Filtre par categorie d'adhesion |
| `communeId` | int? | Filtre par commune (via zone de l'agent) |
| `zoneSocialeId` | int? | Filtre par zone sociale de l'agent |
| `typeAdhesionId` | int? | Filtre par type d'adhesion |
| `tarifCotisationId` | int? | Filtre par tarif de cotisation |
| `dateDebut` | datetime? | Debut de periode (financieres, consolidees) |
| `dateFin` | datetime? | Fin de periode (financieres, consolidees) |

## Exemple : generales

```http
GET /api/Statistiques/generales?zoneSocialeId=3&categorieAdhesionId=1
Authorization: Bearer {token}
```

```json
{
  "totalAffilies": 120,
  "nombreObligationsMoisPrecedent": 95,
  "totalArrieres": 15000.00,
  "totalCollectesMois": 8500.00,
  "tauxRecouvrement": 72.50,
  "nombreCollectesMois": 42,
  "dateGeneration": "2026-07-10T12:00:00"
}
```

## Exemple : operationnelles (extrait)

```json
{
  "repartitionAffiliesParCategorie": [
    {
      "categorieAdhesionId": 1,
      "nomCategorie": "Standard",
      "nombreAffilies": 80,
      "pourcentage": 66.67,
      "montantTotal": 12000.00
    }
  ],
  "repartitionAffiliesParZone": [
    {
      "zoneSocialeId": 3,
      "nomZone": "Zone A",
      "nomCommune": "Gombe",
      "nombreAffilies": 45,
      "pourcentage": 37.50
    }
  ],
  "affilieActivite": {
    "nombreAffiliesActifs": 110,
    "nombreAffiliesInactifs": 10,
    "totalAffilies": 120,
    "pourcentageActifs": 91.67,
    "pourcentageInactifs": 8.33
  }
}
```

## Regles metier

- Collecte valide : `CollecteStatutPaiementRegles.EstValide(statutPaiement)`
- Montants : `MontantDevisePrincipale ?? Montant`
- Obligations : entite `ArrieresAffilie` (cotisation, frais, souscription)
- Top agents : roles `Caissier`, `Percepteur`, `AT`, `AA` et variantes
