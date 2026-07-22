# Mapping KPI -> Sources Prosoc

## Scope

- Stats globales avec filtres optionnels (pas de scope JWT automatique).
- Pas de parametre `idSociete`.

## Filtres query

| Parametre | Source Prosoc |
|---|---|
| `categorieAdhesionId` | `TypeAdhesion.CategorieAdhesionId` |
| `communeId` | `Agent.Zone.CommuneId` |
| `zoneSocialeId` | `Agent.ZoneSocialeId` |
| `typeAdhesionId` | `Adhesion.TypeAdhesionId` |
| `tarifCotisationId` | `Collecte.CotisationAffilieId` / `ArrieresAffilie.CotisationAffilieId` |
| `dateDebut` / `dateFin` | Fenetre temporelle (financieres, consolidees) |

## Generales

- `totalAffilies` -> `Adhesions` actives distinctes par `AffilieId`
- `nombreObligationsMoisPrecedent` -> `ArrieresAffilie` actifs du mois M-1
- `totalArrieres` -> somme `ArrieresAffilie.RestAPayer` actifs
- `totalCollectesMois` -> `Collectes` valides du mois (`CollecteStatutPaiementRegles.EstValide`)
- `nombreCollectesMois` -> nombre de collectes valides du mois
- `tauxRecouvrement` -> collectes valides (M) / obligations attendues (M-1)

## Financieres

- `chiffreAffaires` -> somme collectes valides du mois
- `montantPaye` -> somme collectes valides sur periode filtree
- `montantArrieres` / `montantDu` -> somme `RestAPayer` actifs
- `repartitionPaiements` -> regroupement par `Collecte.ModePaiement`
- `evolutionMensuelle` -> obligations + collectes par mois

## Operationnelles

- `repartitionAffiliesParCategorie` -> `Adhesion.TypeAdhesion.CategorieAdhesion`
- `repartitionAffiliesParZone` -> `AgentCreateur.ZoneSociale` + `Commune`
- `statistiquesObligationsMois` -> agrégats mensuels `ArrieresAffilie`
- `affilieActivite` -> actifs/inactifs via `Affilie.Statut`

## Performance

- `tauxRecouvrementGlobal` -> collectes M / obligations M-1
- `tauxRecouvrementParCategorie` -> `ArrieresAffilie` par categorie d'adhesion
- `topAgents` -> collectes valides par agent (roles Caissier, Percepteur, AT, AA)
- `performanceMensuelle` -> collectes valides sur 6 mois
