using QPS.Application.Pagination;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace QPS.UnitTests;

public class PaginationExtensionsTests
{
    [Fact]
    public void ApplyPaginationAndSorting_ShouldReturnCorrectPage()
    {
        // Arrange
        var items = new List<TestItem>
        {
            new TestItem { Id = 1, Name = "Item 1", CreatedAt = new DateTime(2023, 1, 1) },
            new TestItem { Id = 2, Name = "Item 2", CreatedAt = new DateTime(2023, 1, 2) },
            new TestItem { Id = 3, Name = "Item 3", CreatedAt = new DateTime(2023, 1, 3) },
            new TestItem { Id = 4, Name = "Item 4", CreatedAt = new DateTime(2023, 1, 4) },
            new TestItem { Id = 5, Name = "Item 5", CreatedAt = new DateTime(2023, 1, 5) }
        };
        var query = items.AsQueryable();
        var request = new PaginationRequest { Page = 2, PageSize = 2, SortField = "CreatedAt", SortDirection = "Ascending" };

        // Act
        var result = query.ApplyPaginationAndSorting(request).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(3, result[0].Id);
        Assert.Equal(4, result[1].Id);
    }

    [Fact]
    public void ApplyPaginationAndSorting_ShouldUseDefaultSortingWhenFieldNotFound()
    {
        // Arrange
        var items = new List<TestItem>
        {
            new TestItem { Id = 1, Name = "Item 1", CreatedAt = new DateTime(2023, 1, 1) },
            new TestItem { Id = 2, Name = "Item 2", CreatedAt = new DateTime(2023, 1, 2) },
            new TestItem { Id = 3, Name = "Item 3", CreatedAt = new DateTime(2023, 1, 3) }
        };
        var query = items.AsQueryable();
        var request = new PaginationRequest { Page = 1, PageSize = 10, SortField = "NonExistentField", SortDirection = "Ascending" };

        // Act
        var result = query.ApplyPaginationAndSorting(request).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(3, result[0].Id); // Should be sorted by CreatedAt descending
        Assert.Equal(2, result[1].Id);
        Assert.Equal(1, result[2].Id);
    }

    [Fact]
    public async Task ToPaginationResponseAsync_ShouldReturnCorrectPaginationResponse()
    {
        // Arrange
        var items = new List<TestItem>
        {
            new TestItem { Id = 1, Name = "Item 1", CreatedAt = new DateTime(2023, 1, 1) },
            new TestItem { Id = 2, Name = "Item 2", CreatedAt = new DateTime(2023, 1, 2) },
            new TestItem { Id = 3, Name = "Item 3", CreatedAt = new DateTime(2023, 1, 3) },
            new TestItem { Id = 4, Name = "Item 4", CreatedAt = new DateTime(2023, 1, 4) },
            new TestItem { Id = 5, Name = "Item 5", CreatedAt = new DateTime(2023, 1, 5) }
        };
        var query = items.AsQueryable();
        var request = new PaginationRequest { Page = 2, PageSize = 2, SortField = "CreatedAt", SortDirection = "Ascending" };

        // Act
        var result = await query.ToPaginationResponseAsync(request);

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(2, result.List.Count);
        Assert.Equal(3, result.List[0].Id);
        Assert.Equal(4, result.List[1].Id);
    }

    private class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}