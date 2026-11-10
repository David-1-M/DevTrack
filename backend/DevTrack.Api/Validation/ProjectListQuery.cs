namespace DevTrackAPI.Validation;

/// <summary>
/// Validated query settings for the authenticated project list.
/// </summary>
public sealed record ProjectListQuery(int Page, int PageSize, string? Search)
{
    public int Skip => (Page - 1) * PageSize;

    public static bool TryCreate(
        int? page,
        int? pageSize,
        string? search,
        out ProjectListQuery query,
        out string? error)
    {
        query = null!;
        error = null;

        var resolvedPage = page ?? 1;
        var resolvedPageSize = pageSize ?? 10;

        if (resolvedPage < 1)
        {
            error = "Page must be greater than or equal to 1.";
            return false;
        }

        if (resolvedPageSize < 1 || resolvedPageSize > 100)
        {
            error = "PageSize must be between 1 and 100.";
            return false;
        }

        if ((long)(resolvedPage - 1) * resolvedPageSize > int.MaxValue)
        {
            error = "Requested page exceeds the supported offset.";
            return false;
        }

        var normalizedSearch = search?.Trim();
        if (normalizedSearch?.Length > 100)
        {
            error = "Search must be 100 characters or fewer.";
            return false;
        }

        query = new ProjectListQuery(
            resolvedPage,
            resolvedPageSize,
            string.IsNullOrWhiteSpace(normalizedSearch)
                ? null
                : normalizedSearch);
        return true;
    }
}
