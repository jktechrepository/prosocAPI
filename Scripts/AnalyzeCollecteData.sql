-- SCRIPT D'ANALYSE DES DONNÉES COLLECTE EXISTANTES
-- Utiliser ce script pour analyser l'impact avant migration

-- 1. Compter le nombre total de collectes
SELECT COUNT(*) as TotalCollectes 
FROM Collectes;

-- 2. Analyser les collectes avec SouscriptionPrestationId
SELECT 
    COUNT(*) as CollectesAvecSouscription,
    COUNT(DISTINCT SouscriptionPrestationId) as NbSouscriptionsDifferentes
FROM Collectes 
WHERE SouscriptionPrestationId IS NOT NULL;

-- 3. Analyser les collectes sans SouscriptionPrestationId (probablement des frais)
SELECT 
    COUNT(*) as CollectesSansSouscription,
    MIN(DateCreation) as PremiereCollecte,
    MAX(DateCreation) as DerniereCollecte
FROM Collectes 
WHERE SouscriptionPrestationId IS NULL;

-- 4. Distribution par mode de paiement
SELECT 
    ModePaiement,
    COUNT(*) as Nombre,
    SUM(Montant) as TotalMontant
FROM Collectes 
GROUP BY ModePaiement;

-- 5. Échantillon de collectes pour analyse manuelle
SELECT 
    IdCollecte,
    ReferencePaiement,
    Montant,
    ModePaiement,
    SouscriptionPrestationId,
    DateCreation,
    AgentId
FROM Collectes 
ORDER BY DateCreation DESC 
LIMIT 10;

-- 6. Vérifier les références de paiement uniques
SELECT 
    COUNT(*) as Total,
    COUNT(DISTINCT ReferencePaiement) as ReferencesUniques
FROM Collectes;

-- 7. Analyser les montants par type (basé sur présence/absence de SouscriptionPrestationId)
SELECT 
    CASE 
        WHEN SouscriptionPrestationId IS NOT NULL THEN 'Avec Souscription'
        ELSE 'Sans Souscription (probablement frais)'
    END as TypeEstime,
    COUNT(*) as Nombre,
    AVG(Montant) as MontantMoyen,
    MIN(Montant) as MontantMin,
    MAX(Montant) as MontantMax,
    SUM(Montant) as TotalMontant
FROM Collectes 
GROUP BY 
    CASE 
        WHEN SouscriptionPrestationId IS NOT NULL THEN 'Avec Souscription'
        ELSE 'Sans Souscription (probablement frais)'
    END;
