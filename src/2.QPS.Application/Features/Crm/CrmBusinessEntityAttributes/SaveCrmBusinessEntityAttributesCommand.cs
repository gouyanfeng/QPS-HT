using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;

namespace QPS.Application.Features.Crm.CrmBusinessEntityAttributes;

public class SaveCrmBusinessEntityAttributesCommand : IRequest<bool>
{
    public CrmBusinessEntityAttributeSaveRequest Request { get; set; } = null!;
}

public class SaveCrmBusinessEntityAttributesHandler : IRequestHandler<SaveCrmBusinessEntityAttributesCommand, bool>
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// 保存 CRM 业务实体属性处理器。
    /// </summary>
    public SaveCrmBusinessEntityAttributesHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 编排保存业务实体属性用例。
    /// </summary>
    public async Task<bool> Handle(SaveCrmBusinessEntityAttributesCommand request, CancellationToken cancellationToken)
    {
        // 编排保存业务实体属性用例：
        // 规范化新值、删除旧值、按顺序写入新属性。
        var values = NormalizeValues(request.Request.Values);

        var oldAttributes = await GetOldAttributes(request.Request, cancellationToken);

        _dbContext.CrmBusinessEntityAttributes.RemoveRange(oldAttributes);

        AddNewAttributes(request.Request, values);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// 规范化属性值列表。
    /// </summary>
    private static List<string> NormalizeValues(IEnumerable<string> values)
    {
        return values
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// 获取实体当前已有的同类属性。
    /// </summary>
    private async Task<List<CrmBusinessEntityAttribute>> GetOldAttributes(
        CrmBusinessEntityAttributeSaveRequest request,
        CancellationToken cancellationToken)
    {
        return await _dbContext.CrmBusinessEntityAttributes
            .Where(attribute =>
                attribute.EntityType == request.EntityType &&
                attribute.EntityId == request.EntityId &&
                attribute.AttributeCode == request.AttributeCode)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 按请求顺序新增业务实体属性。
    /// </summary>
    private void AddNewAttributes(CrmBusinessEntityAttributeSaveRequest request, List<string> values)
    {
        var sortOrder = 1;

        foreach (var value in values)
        {
            _dbContext.CrmBusinessEntityAttributes.Add(new CrmBusinessEntityAttribute(
                request.EntityType,
                request.EntityId,
                request.AttributeCode,
                value,
                sortOrder++));
        }
    }
}
