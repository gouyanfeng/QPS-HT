using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmFollowRecords;

public class CreateCrmFollowRecordCommand : IRequest<bool>
{
    public Guid CustomerId { get; set; }

    public CrmFollowRecordCreateRequest Request { get; set; } = null!;
}

public class CreateCrmFollowRecordHandler : IRequestHandler<CreateCrmFollowRecordCommand, bool>
{
    private const string CustomerEntityType = CrmCodes.HerbBaseEntityType;

    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateCrmFollowRecordHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(CreateCrmFollowRecordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Request.FollowResult))
        {
            throw new BusinessException(400, "沟通结果不能为空");
        }

        var customer = await _dbContext.CrmHerbBases
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId && !c.IsDeleted, cancellationToken);
        if (customer == null)
        {
            throw new BusinessException(404, "客户不存在");
        }

        CrmContact? contact = null;
        if (request.Request.ContactId.HasValue)
        {
            contact = await _dbContext.CrmContacts
                .FirstOrDefaultAsync(c => c.Id == request.Request.ContactId.Value, cancellationToken);

            if (contact == null || contact.EntityType != CustomerEntityType || contact.EntityId != request.CustomerId)
            {
                throw new BusinessException(404, "联系人不存在");
            }
        }

        var operatorUserId = Guid.TryParse(_currentUserService.UserId, out var parsedUserId)
            ? parsedUserId
            : (Guid?)null;

        var record = CrmFollowRecord.Create(
            request.CustomerId,
            request.Request.ContactId,
            request.Request.FollowType,
            request.Request.FollowResult,
            request.Request.IntentLevel,
            request.Request.Content,
            request.Request.NextFollowAt,
            operatorUserId);

        _dbContext.CrmFollowRecords.Add(record);
        customer.UpdateFollowSummary(DateTime.Now, request.Request.FollowResult, request.Request.NextFollowAt);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


