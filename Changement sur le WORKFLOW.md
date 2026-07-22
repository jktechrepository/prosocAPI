# 📘 Changement sur le WORKFLOW
---

### Étape 1 : Model CotisationAffilie
Nous devons creer un model CotisationAffilie.
Il doit avoir les champs suivant :
int IdCotisationMensuel (Clef Primaire auto incrementE),
double Montant,
string Periodicite (Cotisation Mensuel ou Annuel),
int TypeAdhesionId (clef etrangaire qui pointe vers la table TypeAdhesion)

Note : Ici on doit comprendre que les CotisationAffilies sont des frais supplementaire que doivent payer les affilies independement des autres frais.

Remarque : A SAVOIR,le montant CotisationAffilie et Payable par personne (donc pour une Adhesion pour une famille de 4, le montant reel doit etre calculer sur base du nombre des personne). Celui qui ne paie pas la cotisation mensuelle n’a pas droit à l’achat du ProduitAssureur /ProduitMutuel.

### Étape 2 : ProduitAssurance / ProduitMutuel

``` Lors de la creation de chaque produit on doit :
- Renseigner le montant du produit ;
- Renseigner le TauxCommissionAT (Agent de terrain). La commission de l’AT pour le produit assurance peut être nommée AUTRE PRIME ;
- Renseigner le TauxCommissionAA (Agent Administratif) ;
- Renseigner le TauxCommissionAAMash (commission affectE Maash) ;
- Renseigner le TauxCommissionAAStructure (commission structure) ;
- La periodicite (payable Mensuelement ou Annuellement) ;
- Tranche d'age d'eligibilite ;

```

Jobs descriptions :
📌 Étapes de contrôle :
• AT : faire l’adhésion , récolter des fonds, suivre et orienter l’affilié .
• Chef d'équipe : surveiller toutes les opérations de l’ AT, corriger les erreurs de
l’AT, centraliser tous les rapports des AT sous sa responsabilité ou supervision.
• Superviseur communal : centraliser les rapports des chefs d'équipes des
communes, orienter et suivre les target définis.
• Superviseur de district : contrôler et évaluer les compétences de chaque
superviseur communal, proposer les stratégies de performance pour chaque
commune et centraliser les rapports de chaque commune sous sa supervision.
• Responsable commercial et des opérations de terrain : planifier, définir la
politique commerciale, suivre et évaluer les target de tous les districts, analyser
les rapports de terrain et faire des suggestions à la hiérarchie

Précisions target Pour Agent de Terrain (AT): KPIs
❖ Adhésion
• En nombre (moyenne) :
o Journalière : 5 adhésions (F3-6)
o Hebdomadaire : 25 adhésions (F3-6)
o Mensuel : 100 adhésions (F3-6)

Adhésion : informations nécessaires :
❖ 1er niveau :  Agent de Terrain AT (enregistrement système)
• Informations personnelles :
o Photo
o Carte d’identité
o Nom complet
• Adresse résidence
• Souscription
• Confirmation
❖ 2e niveau : Encodeur "Agent Administratif"( Il enregistre les infos qui etait recupere dans fiche d’adhésion au format papier )
1. Informations des personnes à charge :
o Nom complet
o Sélectionner lien de parenté
o Adresse
• Personne de contact :
o Nom complet
o Sélectionner lien de parenté
o Adresse
• Vérification et validation.


Remarque 1 : Nous avons 2 catégories de ProduitMutuel :
    -    Produit gratuit : ils sont associés aux cotisation mensuelle ;
    -    Produit payant : Pour les produits payants la commission varie selon les produits ;
    
    
Remarque 2 : Il y a une retenue à la source  de 5$/mois pour les Agents (AT,AA, etc.) pour  permetre  a l'agent et toute sa famille de  bénéficier du service MAASH (ProduitMituel). 


Remarque 3 (Montant Penaliter a definir): On applique une PENALITER a l'affilie EN CAS DE RETARD d'au moins 3 jours DE PAIEMENT DE COTISATIONS.

Remarque 4 : PERSONNE EN CHARGE :
- Enfant de 0 à 18 ans. De 18 à 25 ans sont accepte, seulement s’il présente les documents prouvant qu’il est étudiant.
- ON NE PEUT PAS PRENDRE L’ADHESION LORSQU’ON A DEJA 55 ANS, on doit etre declarer comme personne a charge.

Remarque 5 : Répartition des commissions (Produit d’assurance) pour une Adhésion effectue en ligne :
- Agents de terrain : 15 %
- Compte MASH : 10 %

Remarque 6 : Pour les Dashboard des Agents de Terrain
Les agents doivent disposer d’un dashboard pour visualiser les primes générées, les commissions et le
suivi des adhérents.


 

