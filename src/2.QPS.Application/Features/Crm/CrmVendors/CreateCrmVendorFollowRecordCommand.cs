using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmVendors;

public class CreateCrmVendorFollowRecordCommand : IRequest<bool>
{
    public Guid VendorId { get; set; }

    public CrmFollowRecordCreateRequest Request { get; set; } = null!;
}

public class CreateCrmVendorFollowRecordHandler : IRequestHandler<CreateCrmVendorFollowRecordCommand, bool>
{
    private const string VendorEntityType = CrmCodes.VendorEntityType;

    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateCrmVendorFollowRecordHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(CreateCrmVendorFollowRecordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Request.FollowResult))
        {
            throw new BusinessException(400, "沟通结果不能为空");
        }

        var vendorExists = await _dbContext.CrmVendors.AnyAsync(
            vendor => vendor.Id == request.VendorId && !vendor.IsDeleted,
            cancellationToken);
        if (!vendorExists)
        {
            throw new BusinessException(404, "厂商不存在");
        }

        await EnsureContactBelongsToVendor(request, cancellationToken);

        _dbContext.CrmFollowRecords.Add(CrmFollowRecord.Create(
            entityType: VendorEntityType,
            entityId: request.VendorId,
            contactId: request.Request.ContactId,
            followType: request.Request.FollowType,
            followResult: request.Request.FollowResult,
            intentLevel: request.Request.IntentLevel,
            content: request.Request.Content,
            nextFollowAt: request.Request.NextFollowAt,
            operatorUserId: GetOperatorUserId()));

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task EnsureContactBelongsToVendor(CreateCrmVendorFollowRecordCommand command, CancellationToken cancellationToken)
    {
        if (!command.Request.ContactId.HasValue)
        {
            return;
        }

        var exists = await _dbContext.CrmContacts.AnyAsync(
            contact =>
                contact.Id == command.Request.ContactId.Value &&
                contact.EntityType == VendorEntityType &&
                contact.EntityId == command.VendorId &&
                !contact.IsDeleted,
            cancellationToken);

        if (!exists)
        {
            throw new BusinessException(404, "联系人不存在");
        }
    }

    private Guid? GetOperatorUserId()
    {
        return Guid.TryParse(_currentUserService.UserId, out var parsedUserId)
            ? parsedUserId
            : null;
    }
}
