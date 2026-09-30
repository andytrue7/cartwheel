namespace Cartwheel.Domain.Repositories;

/// <summary>One page of items plus the number of items across all pages.</summary>
/// <param name="Items">The items on the requested page; empty when the page is past the end.</param>
/// <param name="TotalCount">How many items match, over all pages.</param>
/// <param name="Page">The request this result answers.</param>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, PageRequest Page);
