namespace Cartwheel.Domain.Repositories;

/// <summary>Which page of a list is wanted: 1-based page number and how many items per page.</summary>
public sealed record PageRequest
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    // The cap keeps (Page - 1) * PageSize inside int: 999_999 * 100 is about 100 million,
    // far below int.MaxValue (about 2.1 billion). Keep this in sync with ProductListQuery's [Range].
    public const int MaxPage = 1_000_000;

    public PageRequest(int page = 1, int pageSize = DefaultPageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(page, MaxPage);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageSize, MaxPageSize);

        Page = page;
        PageSize = pageSize;
    }

    public int Page { get; }

    public int PageSize { get; }

    /// <summary>How many items come before this page. Can't overflow because of the caps.</summary>
    public int Skip => (Page - 1) * PageSize;

    public static PageRequest First { get; } = new();
}
