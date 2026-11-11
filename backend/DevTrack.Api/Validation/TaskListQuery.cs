using DevTrackAPI.Models;

namespace DevTrackAPI.Validation;

/// <summary>Validated filters, order and page settings for the task listing.</summary>
public sealed record TaskListQuery(
    WorkTaskStatus? Status,
    int? ProjectId,
    string SortBy,
    bool Descending,
    int Page,
    int PageSize)
{
    public int Skip => (Page - 1) * PageSize;

    public static bool TryCreate(
        string? status,
        int? projectId,
        string? sortBy,
        string? sortOrder,
        int? page,
        int? pageSize,
        out TaskListQuery query,
        out string? error)
    {
        query = null!;
        error = null;

        WorkTaskStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<WorkTaskStatus>(status.Trim(), true, out var value)
                || !Enum.IsDefined(value))
            {
                error = "Status must be ToDo, InProgress, or Completed.";
                return false;
            }
            parsedStatus = value;
        }

        if (projectId.HasValue && projectId.Value < 1)
        {
            error = "ProjectId must be a positive integer.";
            return false;
        }

        var requestedSort = string.IsNullOrWhiteSpace(sortBy)
            ? "createdAt"
            : sortBy.Trim();
        if (requestedSort.Equals("createdAt", StringComparison.OrdinalIgnoreCase))
            requestedSort = "createdAt";
        else if (requestedSort.Equals("dueDate", StringComparison.OrdinalIgnoreCase))
            requestedSort = "dueDate";
        else
        {
            error = "SortBy must be createdAt or dueDate.";
            return false;
        }

        var order = string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder.Trim();
        if (!order.Equals("asc", StringComparison.OrdinalIgnoreCase)
            && !order.Equals("desc", StringComparison.OrdinalIgnoreCase))
        {
            error = "SortOrder must be asc or desc.";
            return false;
        }

        var resolvedPage = page ?? 1;
        var resolvedPageSize = pageSize ?? 10;
        if (resolvedPage < 1 || resolvedPageSize is < 1 or > 100
            || ((long)resolvedPage - 1) * resolvedPageSize > int.MaxValue)
        {
            error = "Page must be positive and pageSize must be between 1 and 100, with a supported offset.";
            return false;
        }

        query = new TaskListQuery(
            parsedStatus, projectId, requestedSort,
            order.Equals("desc", StringComparison.OrdinalIgnoreCase),
            resolvedPage, resolvedPageSize);
        return true;
    }
}
