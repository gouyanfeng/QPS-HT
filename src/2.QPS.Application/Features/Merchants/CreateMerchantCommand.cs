using MediatR;
using QPS.Application.Contracts.Merchants;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;

namespace QPS.Application.Features.Merchants;

/// <summary>
/// 创建商户命令
/// </summary>
public class CreateMerchantCommand : IRequest<MerchantDto>
{
    /// <summary>
    /// 创建商户请求
    /// </summary>
    public MerchantCreateRequest Request { get; set; }
}

/// <summary>
/// 创建商户处理器
/// </summary>
public class CreateMerchantHandler : IRequestHandler<CreateMerchantCommand, MerchantDto>
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    public CreateMerchantHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 处理创建商户请求
    /// </summary>
    /// <param name="request">创建商户命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>商户DTO</returns>
    public async Task<MerchantDto> Handle(CreateMerchantCommand request, CancellationToken cancellationToken)
    {
        // 创建商户
        var merchant = Merchant.Create(
            request.Request.Name,
            request.Request.Phone,
            request.Request.ExpiryDate
        );

        // 保存到数据库
        _dbContext.Merchants.Add(merchant);
        await _dbContext.SaveChangesAsync(cancellationToken);

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