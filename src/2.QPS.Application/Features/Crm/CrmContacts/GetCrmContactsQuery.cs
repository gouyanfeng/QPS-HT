using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;

namespace QPS.Application.Features.Crm.CrmContacts;

public class GetCrmContactsQuery : IRequest<List<CrmContactDto>>
{
    public Guid CustomerId { get; set; }
}

public class GetCrmContactsHandler : IRequestHandler<GetCrmContactsQuery, List<CrmContactDto>>
{
    private readonly IDbContext _dbContext;

    public GetCrmContactsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CrmContactDto>> Handle(GetCrmContactsQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.CrmContacts
            .Where(c => c.CustomerId == request.CustomerId)
            .OrderByDescending(c => c.IsPrimary)
            .ThenBy(c => c.CreatedAt)
            .Select(c => new CrmContactDto
            {
                Id = c.Id,
                CustomerId = c.CustomerId,
                ContactName = c.ContactName,
                Phone = c.Phone,
                PhoneType = c.PhoneType,
                Wechat = c.Wechat,
                RoleName = c.RoleName,
                IsPrimary = c.IsPrimary,
                Status = c.Status,
                Remark = c.Remark,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
