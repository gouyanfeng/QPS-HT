using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmContacts;

public class UpdateCrmContactStatusCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public CrmContactStatusRequest Request { get; set; } = null!;
}

public class UpdateCrmContactStatusHandler : IRequestHandler<UpdateCrmContactStatusCommand, bool>
{
    private const string CustomerEntityType = CrmCodes.HerbBaseEntityType;
    private const string InvalidStatus = "INVALID";

    private readonly IDbContext _dbContext;

    public UpdateCrmContactStatusHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(UpdateCrmContactStatusCommand request, CancellationToken cancellationToken)
    {
        var contact = await _dbContext.CrmContacts
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (contact == null)
        {
            throw new BusinessException(404, "联系人不存在");
        }

        var customer = await _dbContext.CrmHerbBases
            .FirstOrDefaultAsync(c => c.Id == contact.EntityId && contact.EntityType == CustomerEntityType && !c.IsDeleted, cancellationToken);
        if (customer == null)
        {
            throw new BusinessException(404, "客户不存在");
        }

        var wasPrimary = contact.IsPrimary;

        contact.MarkStatus(request.Request.Status, request.Request.Remark);

        if (wasPrimary && contact.Status == InvalidStatus)
        {
            contact.UnmarkPrimary();
            customer.ClearPrimaryContact();
            await PromoteOldestValidContact(customer, contact.Id, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task PromoteOldestValidContact(CrmHerbBase customer, Guid excludedContactId, CancellationToken cancellationToken)
    {
        var replacement = await _dbContext.CrmContacts
            .Where(c => c.EntityType == CustomerEntityType && c.EntityId == customer.Id && c.Id != excludedContactId && c.Status != InvalidStatus)
            .OrderBy(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (replacement == null)
        {
            return;
        }

        replacement.MarkPrimary();
        customer.UpdatePrimaryContact(replacement.ContactName, replacement.Phone);
    }
}
