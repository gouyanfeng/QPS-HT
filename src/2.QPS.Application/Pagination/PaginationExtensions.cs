using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace QPS.Application.Pagination;

/// <summary>
/// 分页扩展方法
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// 应用分页和排序
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="query">查询对象</param>
    /// <param name="request">分页请求</param>
    /// <returns>分页后的查询对象</returns>
    public static IQueryable<T> ApplyPaginationAndSorting<T>(this IQueryable<T> query, PaginationRequest request)
    {
        // 排序
        query = ApplySorting(query, request.SortField, request.SortDirection);

        // 分页
        var skip = (request.Page - 1) * request.PageSize;
        query = query.Skip(skip).Take(request.PageSize);

        return query;
    }

    /// <summary>
    /// 应用排序
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="query">查询对象</param>
    /// <param name="sortField">排序字段</param>
    /// <param name="sortDirection">排序方向</param>
    /// <returns>排序后的查询对象</returns>
    private static IQueryable<T> ApplySorting<T>(IQueryable<T> query, string sortField, string sortDirection)
    {
        var isAscending = sortDirection.Equals("Ascending", StringComparison.OrdinalIgnoreCase);

        // 使用反射获取排序字段
        var propertyInfo = typeof(T).GetProperty(sortField, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (propertyInfo == null)
        {
            // 如果排序字段不存在，使用默认排序
            propertyInfo = typeof(T).GetProperty("CreatedAt", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        }

        if (propertyInfo != null)
        {
            query = isAscending
                ? query.OrderBy(e => propertyInfo.GetValue(e))
                : query.OrderByDescending(e => propertyInfo.GetValue(e));
        }

        return query;
    }

    /// <summary>
    /// 执行分页查询
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="query">查询对象</param>
    /// <param name="request">分页请求</param>
    /// <returns>分页响应</returns>
    public static async Task<PaginationResponse<T>> ToPaginationResponseAsync<T>(this IQueryable<T> query, PaginationRequest request)
    {
        // 获取总记录数
        var totalCount = await query.CountAsync();

        // 应用分页和排序
        var paginatedQuery = query.ApplyPaginationAndSorting(request);

        // 执行查询
        var data = await paginatedQuery.ToListAsync();

        // 创建分页响应
        return new PaginationResponse<T>(data, totalCount, request.Page, request.PageSize);
    }
}