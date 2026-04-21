using MediatR;
using QPS.Application.Contracts.Merchants;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;
using System;

namespace QPS.Application.Features.Merchants;

/// <summary>
/// 获取商户详情查询
/// </summary>
public class GetMerchantQuery : IRequest<MerchantDto>
{
    /// <summary>
    /// 商户ID
    /// </summary>
    public Guid Id { get; set; }
}

/// <summary>
/// 获取商户详情处理器
/// </summary>
public class GetMerchantHandler : IRequestHandler<GetMerchantQuery, MerchantDto>
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    public GetMerchantHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 处理获取商户详情请求
    /// </summary>
    /// <param name="request">获取商户详情查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>商户DTO</returns>
    public async Task<MerchantDto> Handle(GetMerchantQuery request, CancellationToken cancellationToken)
    {
        // 查询商户
        var merchant = await _dbContext.Merchants.FindAsync(request.Id, cancellationToken);

        if (merchant == null)
        {
            throw new DomainException("商户不存在");
        }

        // 转换为DTO
        return new MerchantDto
        {
            Id = merchant.Id,
            Name = merchant.Name,
            Phone = merchant.Phone,
            ExpiryDate = merchant.ExpiryDate,
            IsActive = merchant.IsActive,
            CreatedAt = merchant.CreatedAt
        };
    }
}