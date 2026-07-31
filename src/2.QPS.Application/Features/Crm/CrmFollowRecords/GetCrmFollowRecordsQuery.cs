using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;

namespace QPS.Application.Features.Crm.CrmFollowRecords;

public class GetCrmFollowRecordsQuery : IRequest<List<CrmFollowRecordDto>>
{
    public Guid CustomerId { get; set; }
}

public class GetCrmFollowRecordsHandler : IRequestHandler<GetCrmFollowRecordsQuery, List<CrmFollowRecordDto>>
{
    private readonly IDbContext _dbContext;

    public GetCrmFollowRecordsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CrmFollowRecordDto>> Handle(GetCrmFollowRecordsQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.CrmFollowRecords
            .Include(r => r.Contact)
            .Where(r => r.CustomerId == request.CustomerId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new CrmFollowRecordDto
            {
                Id = r.Id,
                CustomerId = r.CustomerId,
                ContactId = r.ContactId,
                ContactName = r.Contact != null ? r.Contact.ContactName : null,
                FollowType = r.FollowType,
                FollowResult = r.FollowResult,
                IntentLevel = r.IntentLevel,
                Content = r.Content,
                NextFollowAt = r.NextFollowAt,
                OperatorUserId = r.OperatorUserId,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}


