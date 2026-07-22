using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class PaginationServiceApplySortingTests
{
    private readonly PaginationService _paginationService;

    public PaginationServiceApplySortingTests()
    {
        _paginationService = new PaginationService(
            new Mock<ILogger<PaginationService>>().Object,
            Options.Create(new PaginationOptions
            {
                DefaultPageSize = 20,
                MaxPageSize = 100
            }));
    }

    [Fact]
    public void ApplySorting_Agent_SortById_UsesIdAgentDescending()
    {
        var data = new List<Agent>
        {
            new() { IdAgent = 3, NomComplet = "C", Matricule = "M3", Phone = "0990000003", Statut = true },
            new() { IdAgent = 1, NomComplet = "A", Matricule = "M1", Phone = "0990000001", Statut = true },
            new() { IdAgent = 2, NomComplet = "B", Matricule = "M2", Phone = "0990000002", Statut = true }
        }.AsQueryable();

        var result = _paginationService.ApplySorting(data, "id", "desc").ToList();

        Assert.Equal(new[] { 3, 2, 1 }, result.Select(a => a.IdAgent).ToArray());
    }

    [Fact]
    public void ApplySorting_Agent_SortByIdAgent_IgnoreCase()
    {
        var data = new List<Agent>
        {
            new() { IdAgent = 2, NomComplet = "B", Matricule = "M2", Phone = "0990000002", Statut = true },
            new() { IdAgent = 1, NomComplet = "A", Matricule = "M1", Phone = "0990000001", Statut = true }
        }.AsQueryable();

        var result = _paginationService.ApplySorting(data, "idagent", "asc").ToList();

        Assert.Equal(new[] { 1, 2 }, result.Select(a => a.IdAgent).ToArray());
    }
}
