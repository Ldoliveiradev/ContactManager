using ContactManager.Application.Common;
using FluentAssertions;

namespace ContactManager.Application.Tests;

public class PaginationFilterTests
{
    [Fact]
    public void PageSize_AboveMax_IsClamped()
    {
        var filter = new PaginationFilter(PageSize: 1000);
        filter.PageSize.Should().Be(PaginationFilter.MaxPageSize);
    }

    [Fact]
    public void PageSize_BelowOne_DefaultsToSix()
    {
        var filter = new PaginationFilter(PageSize: 0);
        filter.PageSize.Should().Be(6);
    }

    [Fact]
    public void Page_BelowOne_DefaultsToOne()
    {
        var filter = new PaginationFilter(Page: -1);
        filter.Page.Should().Be(1);
    }

    [Fact]
    public void PageSize_WithinBounds_IsUnchanged()
    {
        var filter = new PaginationFilter(PageSize: 25);
        filter.PageSize.Should().Be(25);
    }
}
