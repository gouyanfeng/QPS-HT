using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
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
    private const string HerbBaseEntityType = CrmCodes.HerbBaseEntityType;

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

        var records = await _dbContext.CrmTransferRecords
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
                ToOwnerUserId = record.ToOwnerUserId,
                OperatorUserId = record.OperatorUserId,
                Remark = record.Remark,
                CreatedAt = record.CreatedAt
            })
            .ToListAsync(cancellationToken);

        await FillUserNames(records, cancellationToken);
        return records;
    }

    private async Task FillUserNames(List<CrmTransferRecordDto> records, CancellationToken cancellationToken)
    {
        var userIds = records
            .SelectMany(record => new[] { record.FromOwnerUserId, record.ToOwnerUserId, record.OperatorUserId })
            .Where(userId => userId.HasValue)
            .Select(userId => userId!.Value)
            .Distinct()
            .ToList();

        if (userIds.Count == 0)
        {
            return;
        }

        var userNameLookup = await _dbContext.SystemUsers
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(
                user => user.Id,
                user => string.IsNullOrWhiteSpace(user.RealName) ? user.Username : user.RealName,
                cancellationToken);

        foreach (var record in records)
        {
            record.FromOwnerUserName = GetUserName(userNameLookup, record.FromOwnerUserId);
            record.ToOwnerUserName = GetUserName(userNameLookup, record.ToOwnerUserId);
            record.OperatorUserName = GetUserName(userNameLookup, record.OperatorUserId);
        }
    }

    private static string GetUserName(Dictionary<Guid, string> userNameLookup, Guid? userId)
    {
        return userId.HasValue && userNameLookup.TryGetValue(userId.Value, out var userName)
            ? userName
            : string.Empty;
    }
}




