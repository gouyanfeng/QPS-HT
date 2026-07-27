using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmContacts;

public class CreateCrmContactCommand : IRequest<CrmContactDto>
{
    public Guid CustomerId { get; set; }

    public CrmContactCreateRequest Request { get; set; } = null!;
}

public class CreateCrmContactHandler : IRequestHandler<CreateCrmContactCommand, CrmContactDto>
{
    private readonly IDbContext _dbContext;

    public CreateCrmContactHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CrmContactDto> Handle(CreateCrmContactCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Request.ContactName) &&
            string.IsNullOrWhiteSpace(request.Request.Phone))
        {
            throw new BusinessException(400, "联系人姓名和电话至少填写一项");
        }

        var customer = await _dbContext.CrmCustomers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId && !c.IsDeleted, cancellationToken);
        if (customer == null)
        {
            throw new BusinessException(404, "客户不存在");
        }

        var shouldBePrimary = request.Request.IsPrimary ||
            (string.IsNullOrWhiteSpace(customer.PrimaryContactName) &&
                string.IsNullOrWhiteSpace(customer.PrimaryContactPhone));

        var contact = CrmContact.Create(
            request.CustomerId,
            request.Request.ContactName,
            request.Request.Phone,
            request.Request.PhoneType,
            request.Request.Wechat,
            request.Request.RoleName,
            shouldBePrimary,
            request.Request.Remark);

        if (shouldBePrimary)
        {
            await UnmarkSiblingPrimaryContacts(request.CustomerId, contact.Id, cancellationToken);
            customer.UpdatePrimaryContact(contact.ContactName, contact.Phone);
        }

        _dbContext.CrmContacts.Add(contact);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(contact);
    }

    private async Task UnmarkSiblingPrimaryContacts(Guid customerId, Guid contactId, CancellationToken cancellationToken)
    {
        var siblings = await _dbContext.CrmContacts
            .Where(c => c.CustomerId == customerId && c.Id != contactId && c.IsPrimary)
            .ToListAsync(cancellationToken);

        foreach (var sibling in siblings)
        {
            sibling.UnmarkPrimary();
        }
    }

    private static CrmContactDto MapToDto(CrmContact contact)
    {
        return new CrmContactDto
        {
            Id = contact.Id,
            CustomerId = contact.CustomerId,
            ContactName = contact.ContactName,
            Phone = contact.Phone,
            PhoneType = contact.PhoneType,
            Wechat = contact.Wechat,
            RoleName = contact.RoleName,
            IsPrimary = contact.IsPrimary,
            Status = contact.Status,
            Remark = contact.Remark,
            CreatedAt = contact.CreatedAt,
            UpdatedAt = contact.UpdatedAt
        };
    }
}
