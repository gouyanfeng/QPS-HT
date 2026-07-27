using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmContacts;

public class SetPrimaryCrmContactCommand : IRequest<CrmContactDto>
{
    public Guid Id { get; set; }
}

public class SetPrimaryCrmContactHandler : IRequestHandler<SetPrimaryCrmContactCommand, CrmContactDto>
{
    private const string InvalidStatus = "无效";

    private readonly IDbContext _dbContext;

    public SetPrimaryCrmContactHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CrmContactDto> Handle(SetPrimaryCrmContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _dbContext.CrmContacts
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (contact == null)
        {
            throw new BusinessException(404, "联系人不存在");
        }

        if (contact.Status == InvalidStatus)
        {
            throw new BusinessException(400, "无效联系人不能设为主联系人");
        }

        var customer = await _dbContext.CrmCustomers
            .FirstOrDefaultAsync(c => c.Id == contact.CustomerId && !c.IsDeleted, cancellationToken);
        if (customer == null)
        {
            throw new BusinessException(404, "客户不存在");
        }

        var siblings = await _dbContext.CrmContacts
            .Where(c => c.CustomerId == contact.CustomerId && c.Id != contact.Id && c.IsPrimary)
            .ToListAsync(cancellationToken);

        foreach (var sibling in siblings)
        {
            sibling.UnmarkPrimary();
        }

        contact.MarkPrimary();
        customer.UpdatePrimaryContact(contact.ContactName, contact.Phone);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(contact);
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
