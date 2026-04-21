namespace QPS.Application.Pagination;

/// <summary>
/// 分页请求基类
/// </summary>
public class PaginationRequest
{
    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// 排序字段
    /// </summary>
    public string SortField { get; set; } = "CreatedAt";

    /// <summary>
    /// 排序方向
    /// </summary>
    public string SortDirection { get; set; } = "Descending";
}