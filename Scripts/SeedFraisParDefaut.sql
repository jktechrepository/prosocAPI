-- SCRIPT D'INSERTION DES FRAIS PAR DÉFAUT
-- À exécuter après la création de la table Frais

-- Insertion des frais standards pour la migration
INSERT INTO Frais (
    Libelle, 
    Montant, 
    TauxCommission,
    Periodicite,
    DeviseId, 
    CreeParId, 
    DateCreation, 
    Statut,
    EstSupprime
) VALUES 
-- Frais d'adhésion (montant typique : 5000 CDF)
('Frais Adhesion', 5000.00, 25.00, 'Ponctuel', 1, 1, NOW(), 1, 0),

-- Frais de carte membre (montant typique : 2000 CDF)
('Frais Carte Membre', 2000.00, 25.00, 'Ponctuel', 1, 1, NOW(), 1, 0),

-- Frais de dossier (montant typique : 1000 CDF)
('Frais Dossier', 1000.00, 25.00, 'Ponctuel', 1, 1, NOW(), 1, 0),

-- Frais de traitement (montant typique : 1500 CDF)
('Frais Traitement', 1500.00, 25.00, 'Ponctuel', 1, 1, NOW(), 1, 0),

-- Frais d'inscription (montant typique : 3000 CDF)
('Frais Inscription', 3000.00, 25.00, 'Ponctuel', 1, 1, NOW(), 1, 0),

-- Frais de certificat (montant typique : 2500 CDF)
('Frais Certificat', 2500.00, 25.00, 'Ponctuel', 1, 1, NOW(), 1, 0),

-- Frais administratifs (montant typique : 500 CDF)
('Frais Administratifs', 500.00, 25.00, 'Ponctuel', 1, 1, NOW(), 1, 0),

-- Frais de timbre (montant typique : 200 CDF)
('Frais Timbre', 200.00, 25.00, 'Ponctuel', 1, 1, NOW(), 1, 0),

-- Pénalité retard cotisation (J+3) — ajuster FraisPenaliteRetardCotisationId dans appsettings
('Penalite Retard Cotisation', 5000.00, 25.00, 'Ponctuel', 1, 1, NOW(), 1, 0);

-- Vérification de l'insertion
SELECT 
    IdFrais,
    Libelle,
    Montant,
    TauxCommission,
    'CDF' as Devise,
    DateCreation
FROM Frais 
WHERE EstSupprime = 0
ORDER BY Montant DESC;
