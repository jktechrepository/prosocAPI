using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using System.Linq.Expressions;
using Xunit;

namespace Prosoc.Tests.Unit.Services
{
    /// <summary>
    /// Tests unitaires pour PaginationService
    /// </summary>
    public class PaginationServiceTests
    {
        private readonly Mock<ILogger<PaginationService>> _mockLogger;
        private readonly PaginationOptions _paginationOptions;
        private readonly PaginationService _paginationService;

        public PaginationServiceTests()
        {
            _mockLogger = new Mock<ILogger<PaginationService>>();
            _paginationOptions = new PaginationOptions
            {
                DefaultPageSize = 20,
                MaxPageSize = 100,
                MaxSearchResults = 1000,
                EnableCache = true,
                CacheDurationSeconds = 300,
                DefaultSearchFields = new List<string> { "Name", "Description" }
            };

            _paginationService = new PaginationService(_mockLogger.Object, 
                Options.Create(_paginationOptions));
        }

        #region CreatePaginatedResponseAsync Tests

        [Fact]
        public async Task CreatePaginatedResponseAsync_WithValidRequest_ReturnsPaginatedResponse()
        {
            // Arrange
            var data = new List<TestEntity>
            {
                new() { Id = 1, Name = "Test 1", Description = "Description 1" },
                new() { Id = 2, Name = "Test 2", Description = "Description 2" },
                new() { Id = 3, Name = "Test 3", Description = "Description 3" }
            }.AsQueryable();

            var request = new PaginationRequest
            {
                Page = 1,
                PageSize = 2
            };

            // Act
            var result = await _paginationService.CreatePaginatedResponseAsync(data, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(3, result.TotalItems);
            Assert.Equal(2, result.TotalPages);
            Assert.True(result.HasNextPage);
            Assert.False(result.HasPreviousPage);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task CreatePaginatedResponseAsync_WithInvalidPage_NormalizesPageToOne()
        {
            // Arrange
            var data = new List<TestEntity>
            {
                new() { Id = 1, Name = "Test 1" }
            }.AsQueryable();

            var request = new PaginationRequest
            {
                Page = -1,
                PageSize = 10
            };

            // Act
            var result = await _paginationService.CreatePaginatedResponseAsync(data, request);

            // Assert
            Assert.Equal(1, result.CurrentPage);
        }

        [Fact]
        public async Task CreatePaginatedResponseAsync_WithLargePageSize_LimitsToMaxPageSize()
        {
            // Arrange
            var data = new List<TestEntity>
            {
                new() { Id = 1, Name = "Test 1" }
            }.AsQueryable();

            var request = new PaginationRequest
            {
                Page = 1,
                PageSize = 200 // Plus grand que MaxPageSize (100)
            };

            // Act
            var result = await _paginationService.CreatePaginatedResponseAsync(data, request);

            // Assert
            Assert.Equal(100, result.PageSize);
        }

        [Fact]
        public async Task CreatePaginatedResponseAsync_WithoutSort_AppliesDefaultSortingByIdDescending()
        {
            // Arrange — sans sortBy, le plus récent (Id le plus élevé) doit arriver en premier
            var data = new List<TestEntity>
            {
                new() { Id = 1, Name = "First" },
                new() { Id = 3, Name = "Third" },
                new() { Id = 2, Name = "Second" }
            }.AsQueryable();

            var request = new PaginationRequest
            {
                Page = 1,
                PageSize = 2
            };

            // Act
            var result = await _paginationService.CreatePaginatedResponseAsync(data, request);

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(3, result.Data.First().Id);
            Assert.Equal(2, result.Data.Last().Id);
        }

        [Fact]
        public async Task CreatePaginatedResponseAsync_WithSort_AppliesSorting()
        {
            // Arrange
            var data = new List<TestEntity>
            {
                new() { Id = 1, Name = "Zebra", Description = "Last" },
                new() { Id = 2, Name = "Apple", Description = "First" },
                new() { Id = 3, Name = "Banana", Description = "Middle" }
            }.AsQueryable();

            var request = new PaginationRequest
            {
                Page = 1,
                PageSize = 10,
                SortBy = "Name",
                SortDirection = "asc"
            };

            // Act
            var result = await _paginationService.CreatePaginatedResponseAsync(data, request);

            // Assert
            Assert.Equal("Apple", result.Data.First().Name);
            Assert.Equal("Zebra", result.Data.Last().Name);
        }

        [Fact]
        public async Task CreatePaginatedResponseAsync_WithSearch_AppliesSearchFilter()
        {
            // Arrange
            var data = new List<TestEntity>
            {
                new() { Id = 1, Name = "Apple Test", Description = "Contains test" },
                new() { Id = 2, Name = "Banana", Description = "No match" },
                new() { Id = 3, Name = "Test Orange", Description = "Starts with test" }
            }.AsQueryable();

            var request = new PaginationRequest
            {
                Page = 1,
                PageSize = 10,
                Search = "Test"
            };

            // Act
            var result = await _paginationService.CreatePaginatedResponseAsync(data, request);

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, item => Assert.Contains("Test", item.Name));
        }

        #endregion

        #region ApplyFilters Tests

        [Fact]
        public void ApplyFilters_WithValidFilters_AppliesFilters()
        {
            // Arrange
            var data = new List<TestEntity>
            {
                new() { Id = 1, Name = "Test", IsActive = true, CreatedDate = DateTime.Now.AddDays(-2) },
                new() { Id = 2, Name = "Test 2", IsActive = false, CreatedDate = DateTime.Now.AddDays(-1) },
                new() { Id = 3, Name = "Test 3", IsActive = true, CreatedDate = DateTime.Now }
            }.AsQueryable();

            var filters = new List<FilterRequest>
            {
                new() { Field = "IsActive", Operator = "eq", Value = "true" },
                new() { Field = "CreatedDate", Operator = "gte", Value = DateTime.Now.AddDays(-1).ToString("O") }
            };

            // Act
            var result = _paginationService.ApplyFilters(data, filters);

            // Assert
            Assert.Single(result);
            Assert.Equal(3, result.First().Id);
        }

        [Fact]
        public void ApplyFilters_WithInvalidField_HandlesGracefully()
        {
            // Arrange
            var data = new List<TestEntity>
            {
                new() { Id = 1, Name = "Test" }
            }.AsQueryable();

            var filters = new List<FilterRequest>
            {
                new() { Field = "NonExistentField", Operator = "eq", Value = "test" }
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _paginationService.ApplyFilters(data, filters));
            
            Assert.Contains("NonExistentField", exception.Message);
        }

        [Fact]
        public void ApplyFilters_WithContainsOperator_AppliesStringContains()
        {
            // Arrange
            var data = new List<TestEntity>
            {
                new() { Id = 1, Name = "Apple Banana" },
                new() { Id = 2, Name = "Orange" },
                new() { Id = 3, Name = "Banana Split" }
            }.AsQueryable();

            var filters = new List<FilterRequest>
            {
                new() { Field = "Name", Operator = "contains", Value = "Banana" }
            };

            // Act
            var result = _paginationService.ApplyFilters(data, filters);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, item => Assert.Contains("Banana", item.Name));
        }

        #endregion

        #region ApplySorting Tests

        [Fact]
        public void ApplySorting_WithValidField_AppliesSorting()
        {
            // Arrange
            var data = new List<TestEntity>
            {
                new() { Id = 3, Name = "Zebra" },
                new() { Id = 1, Name = "Apple" },
                new() { Id = 2, Name = "Banana" }
            }.AsQueryable();

            // Act
            var result = _paginationService.ApplySorting(data, "Id", "asc");

            // Assert
            var orderedIds = result.Select(x => x.Id).ToList();
            Assert.Equal(new[] { 1, 2, 3 }, orderedIds);
        }

        [Fact]
        public void ApplySorting_WithDescendingOrder_AppliesDescendingSorting()
        {
            // Arrange
            var data = new List<TestEntity>
            {
                new() { Id = 1, Name = "Apple" },
                new() { Id = 2, Name = "Banana" },
                new() { Id = 3, Name = "Zebra" }
            }.AsQueryable();

            // Act
            var result = _paginationService.ApplySorting(data, "Name", "desc");

            // Assert
            Assert.Equal("Zebra", result.First().Name);
            Assert.Equal("Apple", result.Last().Name);
        }

        [Fact]
        public void ApplySorting_WithInvalidField_ReturnsOriginalQuery()
        {
            // Arrange
            var data = new List<TestEntity>
            {
                new() { Id = 1, Name = "Test" }
            }.AsQueryable();

            // Act
            var result = _paginationService.ApplySorting(data, "NonExistentField", "asc");

            // Assert
            Assert.Equal(data, result);
        }

        #endregion

        #region ApplySearch Tests

        [Fact]
        public void ApplySearch_WithDefaultSearchFields_SearchesInSpecifiedFields()
        {
            // Arrange
            var data = new List<TestEntity>
            {
                new() { Id = 1, Name = "Apple Test", Description = "No match" },
                new() { Id = 2, Name = "Banana", Description = "Test description" },
                new() { Id = 3, Name = "Orange", Description = "No match" }
            }.AsQueryable();

            // Act
            var result = _paginationService.ApplySearch(data, "Test");

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void ApplySearch_WithCustomSearchFields_SearchesInCustomFields()
        {
            // Arrange
            var data = new List<TestEntity>
            {
                new() { Id = 1, Name = "Apple", Description = "Test in description" },
                new() { Id = 2, Name = "Test in name", Description = "No match" },
                new() { Id = 3, Name = "Orange", Description = "No match" }
            }.AsQueryable();

            var searchFields = new List<string> { "Name" };

            // Act
            var result = _paginationService.ApplySearch(data, "Test", searchFields);

            // Assert
            Assert.Single(result);
            Assert.Equal(2, result.First().Id);
        }

        [Fact]
        public void ApplySearch_WithEmptySearchTerm_ReturnsOriginalQuery()
        {
            // Arrange
            var data = new List<TestEntity>
            {
                new() { Id = 1, Name = "Test" }
            }.AsQueryable();

            // Act
            var result = _paginationService.ApplySearch(data, "");

            // Assert
            Assert.Equal(data, result);
        }

        #endregion

        #region Helper Classes

        private class TestEntity
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        #endregion
    }
}
