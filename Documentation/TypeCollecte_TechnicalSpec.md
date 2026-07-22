# SPÉCIFICATION TECHNIQUE : TYPECOLLECTE ET RELATION FRAIS

## 📋 OVERVIEW

Cette spécification décrit l'implémentation du champ `TypeCollecte` dans le modèle `Collecte` et la relation avec le modèle `Frais`.

## 🏗️ MODÈLES DE DONNÉES

### TypeCollecte (Énumération)
```csharp
public enum TypeCollecte
{
    Frais = 1,        // Collecte liée à un frais spécifique
    Souscription = 2  // Collecte liée à une souscription de prestation
}
```

### Collecte (Modèle mis à jour)
```csharp
public class Collecte
{
    [Key]
    public int IdCollecte { get; set; }
    
    // NOUVEAU : Type de collecte
    [Required]
    public TypeCollecte TypeCollecte { get; set; }
    
    // NOUVEAU : Relation avec Frais (nullable)
    public int? FraisId { get; set; }
    [ForeignKey("FraisId")]
    public virtual Frais? Frais { get; set; }
    
    // CHAMPS EXISTANTS
    public int? SouscriptionPrestationId { get; set; }
    public virtual SouscriptionPrestation? SouscriptionPrestationRef { get; set; }
    
    // ... autres champs existants
}
```

### Frais (Modèle mis à jour)
```csharp
public class Frais
{
    [Key]
    public int IdFrais { get; set; }
    
    // CHAMPS EXISTANTS
    [Required, StringLength(100)]
    public string Libelle { get; set; } = string.Empty;
    
    [Required, Column(TypeName = "decimal(18,2)")]
    public double Montant { get; set; }
    
    // NOUVEAU : Collection de collectes associées
    public virtual ICollection<Collecte> Collectes { get; set; } = new List<Collecte>();
    
    // ... autres champs existants
}
```

## 🔧 RÈGLES MÉTIER

### Validation des relations
```csharp
public bool IsValid()
{
    return TypeCollecte switch
    {
        TypeCollecte.Frais => FraisId.HasValue,
        TypeCollecte.Souscription => SouscriptionPrestationId.HasValue,
        _ => false
    };
}
```

### Contraintes de base de données
- `TypeCollecte` : NOT NULL, DEFAULT 1 (Frais)
- `FraisId` : NULLABLE, FOREIGN KEY vers Frais.IdFrais
- `SouscriptionPrestationId` : NULLABLE (existant)

## 🔄 LOGIQUE D'APPLICATION

### Service Layer Updates

#### CollecteService
```csharp
public async Task<Collecte> CreateAsync(Collecte collecte)
{
    // Validation du type et de la relation
    if (!ValidateCollecteType(collecte))
        throw new InvalidOperationException("Type de collecte invalide");
    
    // Logique existante...
}

private bool ValidateCollecteType(Collecte collecte)
{
    return collecte.TypeCollecte switch
    {
        TypeCollecte.Frais => collecte.FraisId.HasValue,
        TypeCollecte.Souscription => collecte.SouscriptionPrestationId.HasValue,
        _ => false
    };
}
```

#### FraisService
```csharp
public async Task<List<Collecte>> GetCollectesByFraisAsync(int fraisId)
{
    return await _db.Collectes
        .Where(c => c.FraisId == fraisId && c.Statut && !c.EstSupprime)
        .Include(c => c.Devise)
        .ToListAsync();
}

public async Task<double> GetTotalCollectesByFraisAsync(int fraisId)
{
    return await _db.Collectes
        .Where(c => c.FraisId == fraisId && c.Statut && !c.EstSupprime)
        .SumAsync(c => c.Montant);
}
```

## 🌐 API ENDPOINTS

### Nouveaux endpoints
```csharp
// GET /api/Collecte/by-type/{typeCollecte}
[HttpGet("by-type/{typeCollecte}")]
public async Task<ActionResult<List<Collecte>>> GetByType(TypeCollecte typeCollecte)

// GET /api/Collecte/frais/{fraisId}
[HttpGet("frais/{fraisId}")]
public async Task<ActionResult<List<Collecte>>> GetByFrais(int fraisId)

// POST /api/Collecte/frais
[HttpPost("frais")]
public async Task<ActionResult<Collecte>> CreateFraisCollecte(CreateFraisCollecteDto dto)
```

### DTOs mis à jour
```csharp
public class CreateCollecteDto
{
    public TypeCollecte TypeCollecte { get; set; }
    public int? FraisId { get; set; }
    public int? SouscriptionPrestationId { get; set; }
    public double Montant { get; set; }
    public int DeviseId { get; set; }
    public string ModePaiement { get; set; }
    public int AgentId { get; set; }
    public int AffilieId { get; set; }
}

public class CreateFraisCollecteDto
{
    public int FraisId { get; set; }
    public double Montant { get; set; }
    public int DeviseId { get; set; }
    public string ModePaiement { get; set; }
    public int AgentId { get; set; }
    public int AffilieId { get; set; }
}
```

## 📊 REQUÊTES SQL OPTIMISÉES

### Index recommandés
```sql
-- Index sur TypeCollecte pour filtrage rapide
CREATE INDEX IX_Collectes_TypeCollecte ON Collectes (TypeCollecte);

-- Index sur FraisId pour les jointures
CREATE INDEX IX_Collectes_FraisId ON Collectes (FraisId);

-- Index composite pour les requêtes fréquentes
CREATE INDEX IX_Collectes_TypeFrais ON Collectes (TypeCollecte, FraisId) WHERE FraisId IS NOT NULL;
```

### Requêtes typiques
```sql
-- Collectes par type
SELECT * FROM Collectes WHERE TypeCollecte = 1;

-- Collectes pour un frais spécifique
SELECT c.*, f.Libelle as FraisLibelle 
FROM Collectes c 
JOIN Frais f ON c.FraisId = f.IdFrais 
WHERE c.FraisId = @fraisId;

-- Statistiques par type
SELECT 
    TypeCollecte,
    COUNT(*) as Nombre,
    SUM(Montant) as Total
FROM Collectes 
GROUP BY TypeCollecte;
```

## 🧪 TESTING

### Tests unitaires
```csharp
[Test]
public async Task CreateFraisCollecte_WithValidData_ShouldSucceed()
{
    var fraisCollecte = new Collecte
    {
        TypeCollecte = TypeCollecte.Frais,
        FraisId = 1,
        Montant = 5000,
        // ... autres propriétés
    };
    
    var result = await _collecteService.CreateAsync(fraisCollecte);
    
    Assert.IsNotNull(result);
    Assert.AreEqual(TypeCollecte.Frais, result.TypeCollecte);
    Assert.AreEqual(1, result.FraisId);
}
```

### Tests d'intégration
```csharp
[Test]
public async Task GetCollectesByFrais_ShouldReturnCorrectData()
{
    // Arrange
    var fraisId = 1;
    
    // Act
    var result = await _fraisService.GetCollectesByFraisAsync(fraisId);
    
    // Assert
    Assert.IsNotNull(result);
    Assert.IsTrue(result.All(c => c.FraisId == fraisId));
}
```

## 🚀 DÉPLOIEMENT

### Checklist de déploiement
- [ ] Backup de la base de données
- [ ] Exécuter la migration EF Core
- [ ] Exécuter le script de migration des données
- [ ] Valider les données migrées
- [ ] Déployer le code mis à jour
- [ ] Exécuter les tests de smoke
- [ ] Monitorer les performances

### Monitoring
- Temps de réponse des endpoints
- Taux d'erreur des nouvelles fonctionnalités
- Performance des requêtes SQL
- Validation des données migrées

---

*Cette spécification doit être utilisée comme référence pendant le développement et la revue de code.*
