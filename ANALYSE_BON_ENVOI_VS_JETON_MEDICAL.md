# Analyse : Bon d'envoi vs Jeton Medical

## Vue d'ensemble

Les deux concepts ne sont pas des alternatives : ils sont crees ensemble lors de la validation d'une demande dans `DemandeBonEnvoiService.ConfirmerDemandeAsync`.

Role metier simplifie :
- Bon d'envoi = justificatif de prestation (affilie + prestation), presente via QR code signe.
- Jeton medical = autorisation scopee a un hopital (code alphanumerique), validee/consommee cote hopital.

## 1) Differences de modele de donnees

| Aspect | BonEnvoi | JetonMedical |
|---|---|---|
| Identifiant public | `NumeroBon` (`BON######`) | `CodeJeton` (`JET` + 8 caracteres) |
| Liens directs | `AffilieId`, `PrestationId` | `AffilieId`, `HopitalPartenaireId` |
| Lien prestation | Oui (FK directe) | Non (indirect via `DemandeBonEnvoi`) |
| Lien hopital | Non (indirect via demande -> jeton) | Oui (FK directe) |
| QR code | `QrCodePayload`, `QrCodeImageBase64` | Aucun |
| Expiration | Via signature QR (`BonEnvoiQr:ValidityDays`, 30j) | `DateExpiration` en base (30j par defaut) |
| Etats | `EstUtilise`, `Statut` | `EstUtilise`, `EstValide`, `Statut` |

Point cle : le bon decrit quelle prestation est couverte ; le jeton decrit ou (quel hopital) l'affilie peut se faire soigner.

## 2) Differences d'architecture API

### Bon d'envoi : 3 couches
- Demande : `DemandeBonEnvoiController` (workflow demande -> validation)
- Bon : `BonEnvoiController` (CRUD + scan QR + marquage utilise)
- QR : `BonEnvoiQrCodeService` (signature HMAC + image)

### Jeton medical : 1 couche directe
- Jeton : `JetonMedicalController` (emission, validation, utilisation, archivage)

Il n'existe pas de "DemandeJeton" : le jeton est cree manuellement (`POST /api/JetonMedical`) ou automatiquement lors de la confirmation d'une demande de bon.

## 3) Differences de creation

### Chemin principal (workflow unifie)

Dans `ConfirmerDemandeAsync` :
1. Verification de l'eligibilite affilie.
2. Creation du `JetonMedical` (hopital obligatoire, expiration 30j).
3. Creation du `BonEnvoi` (numero auto, prestation de la demande).
4. Application du QR signe sur le bon.
5. Liaison de la demande : `BonEnvoiId` + `JetonMedicalId`, statut `VALIDEE`.

### Chemins secondaires

| | Bon d'envoi | Jeton medical |
|---|---|---|
| Creation manuelle | `POST /api/BonEnvoi` (sans QR, sans eligibilite) | `POST /api/JetonMedical` (code auto, expiration 30j) |
| Permission dediee | Pas de controle explicite create/confirm | Pas de permission `CREATE_JETON_MEDICAL` |

## 4) Differences d'utilisation cote hopital

| | Bon d'envoi | Jeton medical |
|---|---|---|
| Endpoint principal | `POST /api/BonEnvoi/scanner` | `POST /api/JetonMedical/valider` puis `/utiliser` |
| Mecanisme | Scan QR signe (HMAC) | Saisie code `JETxxxxxxxx` |
| Permission | `SCAN_BON_ENVOI` (enforced) | `USE_JETON_MEDICAL` (+ roles admin/IT/agent hopital) |
| Scope hopital | Via `DemandeBonEnvoi -> JetonMedical.HopitalPartenaireId` | Direct sur `JetonMedical.HopitalPartenaireId` |
| Validation | Signature + expiration QR + `EstUtilise` | `EstValide`, `EstUtilise`, `DateExpiration`, hopital |
| Couplage | Retourne le `JetonMedicalCode` lie | Independant : utiliser un jeton ne marque pas le bon |

Point critique : consommer le bon (scan QR) et consommer le jeton (`POST /utiliser`) sont deux actions separees.

## 5) Differences de permissions

| Permission | Bon d'envoi | Jeton medical |
|---|---|---|
| Lecture | `READ_BON_ENVOI`, `READ_DEMANDE_BON_ENVOI` | `READ_JETON_MEDICAL` |
| Creation workflow | `CONFIRM_DEMANDE_BON_ENVOI` (claims JWT, pas enforce partout) | Automatique a la confirmation |
| Utilisation | `SCAN_BON_ENVOI` | `USE_JETON_MEDICAL` |
| Role hopital | Filtrage via `HopitalScopeHelper` sur bons | Filtrage direct sur jetons |

## 6) Differences de cycle de vie

### Bon d'envoi
- Emis (`EstUtilise=false`)
- Scan QR valide + `MarquerUtilise` -> utilise
- QR expire / signature invalide -> rejet au scan

### Jeton medical
- Emis (`EstValide=true`, `EstUtilise=false`)
- Date expiration depassee -> expire
- `POST /utiliser` -> utilise
- `POST /archiver-expires` -> `EstValide=false`

Le jeton possede l'etat `EstValide` (archivage) que le bon n'a pas. Le bon possede un QR signe que le jeton n'a pas.

## 7) Hub DemandeBonEnvoi

`DemandeBonEnvoi` est le pivot qui relie les deux :
- `PrestationId` = portee metier de la demande et du bon.
- `HopitalPartenaireId` (a la confirmation) = portee du jeton.
- `BonEnvoiId` + `JetonMedicalId` = artefacts generes.

Le dashboard hopital agrege les montants via : Jeton -> Demande -> Prestation.

## 8) Quand utiliser quoi

| Besoin | Entite |
|---|---|
| Prouver qu'une prestation est autorisee pour un affilie | Bon d'envoi |
| Restreindre l'acces a un hopital precis | Jeton medical |
| Presentation physique / scan QR | Bon d'envoi |
| Saisie code au guichet hopital | Jeton medical |
| Workflow complet affilie -> agent -> hopital | `DemandeBonEnvoi` (genere les deux) |

## 9) Points d'attention actuels

1. Pas de FK directe Bon <-> Jeton (lien via `DemandeBonEnvoi`).
2. Utilisation desynchronisee : bon utilise != jeton utilise automatiquement.
3. Creation manuelle de bon sans QR ni controle d'eligibilite.
4. Verification de prestation partielle a la confirmation (eligibilite generale, pas stricte par prestation demandee).
5. `CONFIRM_DEMANDE_BON_ENVOI` present en JWT mais pas force partout au niveau controller.
6. `UtiliserJetonAsync` ne revalide pas tous les etats comme `ValiderJetonAsync`.

Ces ecarts peuvent produire des incoherences en test si l'equipe utilise uniquement un flux (bon ou jeton) sans l'autre, ou si elle se base sur les claims sans controle API explicite.
