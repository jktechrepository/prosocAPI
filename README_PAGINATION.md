# Pagination Universelle ProsocAPI

## 📋 Vue d'ensemble

Ce document décrit l'implémentation de la pagination universelle pour l'API ProsocAPI, permettant une pagination cohérente et performante sur tous les endpoints.

## 🏗️ Architecture

### Composants principaux

1. **Models/Pagination/**
   - `PaginationRequest.cs` - Modèle de requête de pagination
   - `PaginatedResponse.cs` - Réponse paginée standardisée
   - `FilterRequest.cs` - Filtres génériques

2. **Services/**
   - `PaginationService.cs` - Service de pagination universel
   - Gère le tri, filtrage, recherche et pagination

3. **Controllers/**
   - `BaseApiController.cs` - Contrôleur de base avec fonctionnalités de pagination
   - `AdhesionPaginationController.cs` - Exemple d'implémentation

4. **Extensions/**
   - `ServiceCollectionExtensions.cs` - Configuration des services

## 🚀 Utilisation

### Configuration

Dans `Program.cs`:

```csharp
// Ajouter les services de pagination
builder.Services.AddPaginationServices(builder.Configuration);

// Configuration optionnelle
builder.Services.Configure<PaginationOptions>(options =>
{
    options.DefaultPageSize = 20;
    options.MaxPageSize = 100;
    options.MaxSearchResults = 1000;
    options.EnableCache = true;
    options.CacheDurationSeconds = 300;
    options.DefaultSearchFields = new List<string> { "Name", "Description" };
});
```

### Configuration dans appsettings.json

```json
{
  "Pagination": {
    "DefaultPageSize": 20,
    "MaxPageSize": 100,
    "MaxSearchResults": 1000,
    "EnableCache": true,
    "CacheDurationSeconds": 300,
    "DefaultSearchFields": ["Name", "Description"]
  }
}
```

## 📖 Endpoints

### Pagination de base

```http
GET /api/v1/adhesionpagination?page=1&pageSize=20&sortBy=DateCreation&sortDirection=desc&search=test
```

**Paramètres:**
- `page` (int): Numéro de page (défaut: 1)
- `pageSize` (int): Taille de page (défaut: 20, max: 100)
- `sortBy` (string): Champ de tri
- `sortDirection` (string): Direction du tri (asc/desc)
- `search` (string): Terme de recherche
- `statut` (string): Filtre par statut
- `typeAdhesionId` (int): Filtre par type d'adhésion
- `affilieId` (int): Filtre par affilié
- `dateDebut` (DateTime): Filtre date de début
- `dateFin` (DateTime): Filtre date de fin

### Pagination avancée

```http
POST /api/v1/adhesionpagination/advanced
Content-Type: application/json

{
  "page": 1,
  "pageSize": 20,
  "sortBy": "DateCreation",
  "sortDirection": "desc",
  "search": "test",
  "filterList": [
    {
      "field": "Statut",
      "operator": "eq",
      "value": "Actif"
    },
    {
      "field": "MontantTotal",
      "operator": "gt",
      "value": "1000"
    }
  ],
  "includeFields": ["Id", "CodeAdhesion", "Statut"],
  "excludeFields": ["Affilie"]
}
```

### Export Excel

```http
POST /api/v1/adhesionpagination/export
Content-Type: application/json

{
  "page": 1,
  "pageSize": 100
}
```

## 📊 Réponses

### Réponse paginée standard

```json
{
  "data": [
    {
      "id": 1,
      "codeAdhesion": "CODE0001",
      "affilie": {
        "id": 1,
        "nom": "Test",
        "prenom": "User"
      },
      "statut": "Actif",
      "dateCreation": "2024-01-01T00:00:00Z"
    }
  ],
  "currentPage": 1,
  "pageSize": 20,
  "totalItems": 150,
  "totalPages": 8,
  "hasNextPage": true,
  "hasPreviousPage": false,
  "startItem": 1,
  "endItem": 20
}
```

### Réponse étendue

```json
{
  "data": [...],
  "currentPage": 1,
  "pageSize": 20,
  "totalItems": 150,
  "totalPages": 8,
  "hasNextPage": true,
  "hasPreviousPage": false,
  "startItem": 1,
  "endItem": 20,
  "executionTimeMs": 45,
  "appliedFilters": ["Statut eq Actif", "MontantTotal gt 1000"],
  "appliedSorting": "DateCreation desc",
  "fromCache": false,
  "apiVersion": "v1",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## 🔍 Opérateurs de filtre

| Opérateur | Description | Exemple |
|-----------|-------------|---------|
| `eq` | Égal | `{"field": "Statut", "operator": "eq", "value": "Actif"}` |
| `ne` | Différent | `{"field": "Statut", "operator": "ne", "value": "Inactif"}` |
| `gt` | Supérieur | `{"field": "Montant", "operator": "gt", "value": "1000"}` |
| `gte` | Supérieur ou égal | `{"field": "Montant", "operator": "gte", "value": "1000"}` |
| `lt` | Inférieur | `{"field": "Montant", "operator": "lt", "value": "5000"}` |
| `lte` | Inférieur ou égal | `{"field": "Montant", "operator": "lte", "value": "5000"}` |
| `contains` | Contient | `{"field": "Nom", "operator": "contains", "value": "test"}` |
| `startswith` | Commence par | `{"field": "Nom", "operator": "startswith", "value": "A"}` |
| `endswith` | Se termine par | `{"field": "Email", "operator": "endswith", "value": "@example.com"}` |

## 🧪 Tests

### Tests unitaires

```bash
dotnet test --filter "FullyQualifiedName~PaginationServiceTests"
```

### Tests d'intégration

```bash
dotnet test --filter "FullyQualifiedName~PaginationIntegrationTests"
```

### Tests de performance

```bash
dotnet test --filter "FullyQualifiedName~PaginationIntegrationTests" --logger "console;verbosity=detailed"
```

## 📈 Performance

### Optimisations

1. **Indexation de base de données**
   ```sql
   CREATE INDEX IX_Adhesions_DateCreation ON Adhesions(DateCreation);
   CREATE INDEX IX_Adhesions_Statut ON Adhesions(Statut);
   CREATE INDEX IX_Adhesions_TypeAdhesionId ON Adhesions(TypeAdhesionId);
   ```

2. **Cache Redis**
   - Configuration automatique avec Redis
   - Durée de cache configurable (défaut: 5 minutes)

3. **Limitation des résultats**
   - Maximum de 1000 résultats pour les exports
   - Maximum de 100 éléments par page

### Monitoring

Les métriques de performance sont disponibles dans:
- Temps d'exécution dans les réponses étendues
- Logs structurés avec Serilog
- Health checks pour la base de données et Redis

## 🔧 Implémentation dans d'autres contrôleurs

### Étapes

1. **Hériter de BaseApiController**
   ```csharp
   public class MonController : BaseApiController
   {
       public MonController(
           IPaginationService paginationService,
           IOptions<PaginationOptions> paginationOptions,
           ILogger<MonController> logger) 
           : base(paginationService, paginationOptions, logger)
       {
       }
   }
   ```

2. **Créer un endpoint paginé**
   ```csharp
   [HttpGet]
   public async Task<ActionResult<PaginatedResponse<MonDto>>> GetPaginated(
       [FromQuery] PaginationRequest request)
   {
       var query = _context.MaTable.AsQueryable();
       return await CreatePaginatedResponseAsync(query, request);
   }
   ```

3. **Mapper les entités vers les DTOs**
   ```csharp
   private MonDto MapToDto(MonEntity entity)
   {
       return new MonDto
       {
           Id = entity.Id,
           // ... autres propriétés
       };
   }
   ```

## 🚨 Bonnes pratiques

### Pour les développeurs

1. **Toujours utiliser les méthodes de base** pour la cohérence
2. **Valider les paramètres** avec `[ValidatePagination]`
3. **Logger les actions** avec `LogApiAction()`
4. **Utiliser les DTOs** pour les réponses
5. **Documenter les filtres disponibles** dans les commentaires XML

### Pour les clients API

1. **Utiliser la pagination** pour les grands ensembles de données
2. **Privilégier les filtres** à la recherche quand possible
3. **Limiter les champs** avec include/exclude pour réduire la taille
4. **Gérer les erreurs** 400 pour les paramètres invalides
5. **Utiliser les métadonnées** pour la navigation

## 🔄 Migration

### Pour les endpoints existants

1. **Créer une nouvelle version** du endpoint avec pagination
2. **Garder l'ancien endpoint** pendant une période de transition
3. **Documenter la dépréciation** dans les réponses
4. **Communiquer les changements** aux clients

### Exemple de migration

```csharp
// Ancien endpoint (déprécié)
[HttpGet("old")]
[Obsolete("Use GetPaginated instead")]
public async Task<ActionResult<List<AdhesionDto>> GetAll()
{
    // Implémentation existante
}

// Nouveau endpoint avec pagination
[HttpGet("paginated")]
public async Task<ActionResult<PaginatedResponse<AdhesionDto>>> GetPaginated(
    [FromQuery] PaginationRequest request)
{
    // Nouvelle implémentation
}
```

## 📚 Ressources

- **Documentation Swagger**: `/swagger`
- **Tests d'exemples**: `Tests.Unit/Services/PaginationServiceTests.cs`
- **Tests d'intégration**: `Tests.Integration/Controllers/PaginationIntegrationTests.cs`
- **Configuration**: `Program.cs` et `appsettings.json`

## 🤝 Support

Pour toute question ou problème concernant l'implémentation de la pagination:

1. Consulter la documentation Swagger
2. Vérifier les logs d'erreur
3. Exécuter les tests unitaires
4. Contacter l'équipe de développement

---

*Ce document sera mis à jour au fur et à mesure de l'évolution de la fonctionnalité de pagination.*
