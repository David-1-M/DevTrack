using DevTrackAPI.Validation;

namespace DevTrack.Api.Tests.Unit;

public class ProjectListQueryTests
{
    [Fact]
    public void DefaultsToFirstPageWithTenItems()
    {
        var valid = ProjectListQuery.TryCreate(null, null, null, out var query, out var error);

        Assert.True(valid);
        Assert.Null(error);
        Assert.Equal(1, query.Page);
        Assert.Equal(10, query.PageSize);
        Assert.Equal(0, query.Skip);
        Assert.Null(query.Search);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(int.MaxValue, 100)]
    public void RejectsInvalidPagination(int page, int pageSize)
    {
        Assert.False(ProjectListQuery.TryCreate(page, pageSize, null, out _, out var error));
        Assert.NotNull(error);
    }

    [Fact]
    public void CalculatesOffsetAndTrimsSearch()
    {
        Assert.True(ProjectListQuery.TryCreate(3, 20, "  backend  ", out var query, out _));
        Assert.Equal(40, query.Skip);
        Assert.Equal("backend", query.Search);
    }

    [Fact]
    public void RejectsExcessivelyLongSearch()
    {
        Assert.False(ProjectListQuery.TryCreate(1, 10, new string('x', 101), out _, out _));
    }

    [Fact]
    public void AcceptsMaximumPageSize()
    {
        Assert.True(ProjectListQuery.TryCreate(1, 100, "", out var query, out _));
        Assert.Equal(100, query.PageSize);
        Assert.Null(query.Search);
    }
}
