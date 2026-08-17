using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmVendors;

public class GetCrmVendorFollowRecordsQuery : IRequest<List<CrmFollowRecordDto>>
{
    public Guid VendorId { get; set; }
}

public class GetCrmVendorFollowRecordsHandler : IRequestHandler<GetCrmVendorFollowRecordsQuery, List<CrmFollowRecordDto>>
{
    private const string VendorEntityType = CrmCodes.VendorEntityType;

    private readonly IDbContext _dbContext;

    public GetCrmVendorFollowRecordsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CrmFollowRecordDto>> Handle(GetCrmVendorFollowRecordsQuery request, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.CrmVendors.AnyAsync(
            vendor => vendor.Id == request.VendorId && !vendor.IsDeleted,
            cancellationToken);
        if (!exists)
        {
            throw new BusinessException(404, "厂商不存在");
        }

        return await _dbContext.CrmFollowRecords
            .Include(record => record.Contact)
            .Where(record => record.EntityType == VendorEntityType && record.EntityId == request.VendorId)
            .OrderByDescending(record => record.CreatedAt)
            .Select(record => new CrmFollowRecordDto
            {
                Id = record.Id,
                EntityType = record.EntityType,
                EntityId = record.EntityId,
                ContactId = record.ContactId,
                ContactName = record.Contact != null ? record.Contact.ContactName : null,
                FollowType = record.FollowType,
                FollowResult = record.FollowResult,
                IntentLevel = record.IntentLevel,
                Content = record.Content,
                NextFollowAt = record.NextFollowAt,
                OperatorUserId = record.OperatorUserId,
                CreatedAt = record.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
