namespace OrderProcessing.Application.Common;

public sealed class PagedResult<T>
{
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public IReadOnlyList<T> Items { get; }

    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}
