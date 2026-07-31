using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmHerbBases;

public class GetCrmTransferRecordsQuery : IRequest<List<CrmTransferRecordDto>>
{
    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }
}

public class GetCrmTransferRecordsHandler : IRequestHandler<GetCrmTransferRecordsQuery, List<CrmTransferRecordDto>>
{
    private const string HerbBaseEntityType = "CRM_HERB_BASE";

    private readonly IDbContext _dbContext;

    public GetCrmTransferRecordsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CrmTransferRecordDto>> Handle(GetCrmTransferRecordsQuery request, CancellationToken cancellationToken)
    {
        if (request.EntityType == HerbBaseEntityType)
        {
            var herbBaseExists = await _dbContext.CrmHerbBases
                .AnyAsync(herbBase => herbBase.Id == request.EntityId && !herbBase.IsDeleted, cancellationToken);
            if (!herbBaseExists)
            {
                throw new BusinessException(404, "药材基地不存在");
            }
        }

        return await _dbContext.CrmTransferRecords
            .AsNoTracking()
            .Where(record =>
                record.EntityType == request.EntityType &&
                record.EntityId == request.EntityId &&
                !record.IsDeleted)
            .OrderByDescending(record => record.CreatedAt)
            .Select(record => new CrmTransferRecordDto
            {
                Id = record.Id,
                EntityType = record.EntityType,
                EntityId = record.EntityId,
                FromOwnerUserId = record.FromOwnerUserId,
                FromOwnerUserName = record.FromOwnerUserName,
                ToOwnerUserId = record.ToOwnerUserId,
                ToOwnerUserName = record.ToOwnerUserName,
                OperatorUserId = record.OperatorUserId,
                OperatorUserName = record.OperatorUserName,
                Remark = record.Remark,
                CreatedAt = record.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}




