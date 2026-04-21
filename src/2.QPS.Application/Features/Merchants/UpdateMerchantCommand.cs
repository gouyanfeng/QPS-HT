using MediatR;
using QPS.Application.Contracts.Merchants;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;
using System;

namespace QPS.Application.Features.Merchants;

/// <summary>
/// 更新商户命令
/// </summary>
public class UpdateMerchantCommand : IRequest<MerchantDto>
{
    /// <summary>
    /// 商户ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 更新商户请求
    /// </summary>
    public MerchantUpdateRequest Request { get; set; }
}

/// <summary>
/// 更新商户处理器
/// </summary>
public class UpdateMerchantHandler : IRequestHandler<UpdateMerchantCommand, MerchantDto>
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    public UpdateMerchantHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 处理更新商户请求
    /// </summary>
    /// <param name="request">更新商户命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>商户DTO</returns>
    public async Task<MerchantDto> Handle(UpdateMerchantCommand request, CancellationToken cancellationToken)
    {
        // 查询商户
        var merchant = await _dbContext.Merchants.FindAsync(request.Id, cancellationToken);

        if (merchant == null)
        {
            throw new DomainException("商户不存在");
        }

        // 更新商户信息
        merchant.Update(request.Request.Name, request.Request.Phone, request.Request.ExpiryDate);
        
        // 更新商户状态
        if (request.Request.IsActive)
        {
            merchant.Activate();
        }
        else
        {
            merchant.Deactivate();
        }

        // 保存到数据库
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