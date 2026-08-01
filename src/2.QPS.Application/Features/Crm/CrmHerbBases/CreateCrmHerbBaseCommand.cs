using MediatR;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;

namespace QPS.Application.Features.Crm.CrmHerbBases;

/// <summary>
/// 创建药材基地命令
/// </summary>
public class CreateCrmHerbBaseCommand : IRequest<bool>
{
    public CrmHerbBaseCreateRequest Request { get; set; } = null!;
}

/// <summary>
/// 创建药材基地处理器
/// </summary>
public class CreateCrmHerbBaseHandler : IRequestHandler<CreateCrmHerbBaseCommand, bool>
{
    private readonly IDbContext _dbContext;

    public CreateCrmHerbBaseHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(CreateCrmHerbBaseCommand request, CancellationToken cancellationToken)
    {
        var baseName = string.IsNullOrWhiteSpace(request.Request.BaseName)
            ? request.Request.HerbBaseName
            : request.Request.BaseName;

        var customer = CrmHerbBase.Create(
            baseName,
            request.Request.Grade,
            request.Request.Score,
            request.Request.Province,
            request.Request.City,
            request.Request.Area,
            request.Request.Address,
            request.Request.Lat,
            request.Request.Lng,
            request.Request.SourcePlatform,
            request.Request.SourceId,
            request.Request.OwnerUserId,
            request.Request.Remark,
            request.Request.ParentId,
            request.Request.SubjectName
        );

        if (!string.IsNullOrWhiteSpace(request.Request.PrimaryContactName) ||
            !string.IsNullOrWhiteSpace(request.Request.PrimaryContactPhone))
        {
            customer.UpdatePrimaryContact(
                request.Request.PrimaryContactName ?? string.Empty,
                request.Request.PrimaryContactPhone ?? string.Empty);
        }

        _dbContext.CrmHerbBases.Add(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}



