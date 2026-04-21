using System.Collections.Generic;

namespace QPS.Application.Pagination;

/// <summary>
/// 分页响应类
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class PaginationResponse<T>
{
    /// <summary>
    /// 数据列表
    /// </summary>
    public List<T> Data { get; set; }

    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="data">数据列表</param>
    /// <param name="totalCount">总记录数</param>
    /// <param name="currentPage">当前页码</param>
    /// <param name="pageSize">每页大小</param>
    public PaginationResponse(List<T> data, int totalCount, int currentPage, int pageSize)
    {
        Data = data;
        TotalCount = totalCount;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalPages = (totalCount + pageSize - 1) / pageSize;
    }
}