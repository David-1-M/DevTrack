using DevTrackAPI.Models;
using DevTrackAPI.Validation;

namespace DevTrack.Api.Tests.Unit;

public class TaskListQueryTests
{
    private static bool Parse(
        string? status, int? projectId, string? sortBy, string? sortOrder,
        int? page, int? pageSize, out TaskListQuery query)
        => TaskListQuery.TryCreate(status, projectId, sortBy, sortOrder,
            page, pageSize, out query, out _);

    [Fact]
    public void DefaultsToCreatedAtAscendingAndFirstPage()
    {
        Assert.True(Parse(null, null, null, null, null, null, out var query));
        Assert.Null(query.Status);
        Assert.Equal("createdAt", query.SortBy);
        Assert.False(query.Descending);
        Assert.Equal(1, query.Page);
        Assert.Equal(10, query.PageSize);
        Assert.Equal(0, query.Skip);
    }

    [Fact]
    public void ParsesStatusAndDueDateSortCaseInsensitively()
    {
        Assert.True(Parse("inprogress", 3, "DUEDATE", "DESC", 2, 20, out var query));
        Assert.Equal(WorkTaskStatus.InProgress, query.Status);
        Assert.Equal(3, query.ProjectId);
        Assert.Equal("dueDate", query.SortBy);
        Assert.True(query.Descending);
        Assert.Equal(20, query.Skip);
    }

    [Theory]
    [InlineData("unknown", null, null, null, 1, 10)]
    [InlineData("99", null, null, null, 1, 10)]
    [InlineData(null, 0, null, null, 1, 10)]
    [InlineData(null, null, "title", null, 1, 10)]
    [InlineData(null, null, null, "up", 1, 10)]
    [InlineData(null, null, null, null, 0, 10)]
    [InlineData(null, null, null, null, 1, 101)]
    [InlineData(null, null, null, null, int.MaxValue, 100)]
    public void RejectsInvalidFilters(string? status, int? projectId,
        string? sortBy, string? sortOrder, int? page, int? pageSize)
    {
        Assert.False(Parse(status, projectId, sortBy, sortOrder,
            page, pageSize, out _));
    }
}
