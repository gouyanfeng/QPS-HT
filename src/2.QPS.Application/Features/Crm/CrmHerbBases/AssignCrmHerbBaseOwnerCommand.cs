using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmHerbBases;

public class AssignCrmHerbBaseOwnerCommand : IRequest<List<CrmHerbBaseDto>>
{
    public CrmHerbBaseAssignOwnerRequest Request { get; set; } = null!;
}

public class AssignCrmHerbBaseOwnerHandler : IRequestHandler<AssignCrmHerbBaseOwnerCommand, List<CrmHerbBaseDto>>
{
    private const string HerbBaseEntityType = "CRM_HERB_BASE";

    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public AssignCrmHerbBaseOwnerHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<List<CrmHerbBaseDto>> Handle(AssignCrmHerbBaseOwnerCommand request, CancellationToken cancellationToken)
    {
        var herbBaseIds = request.Request.HerbBaseIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (herbBaseIds.Count == 0)
        {
            throw new BusinessException(400, "请选择要分配的药材基地");
        }

        var customers = await _dbContext.CrmHerbBases
            .Where(customer => herbBaseIds.Contains(customer.Id) && !customer.IsDeleted)
            .ToListAsync(cancellationToken);

        if (customers.Count != herbBaseIds.Count)
        {
            throw new BusinessException(404, "药材基地不存在");
        }

        string toOwnerUserName = string.Empty;
        if (request.Request.OwnerUserId.HasValue)
        {
            var owner = await _dbContext.SystemUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Id == request.Request.OwnerUserId.Value && user.IsActive, cancellationToken);
            if (owner == null)
            {
                throw new BusinessException(404, "负责人不存在");
            }

            toOwnerUserName = owner.RealName;
        }

        var fromOwnerIds = customers
            .Where(customer => customer.OwnerUserId.HasValue)
            .Select(customer => customer.OwnerUserId!.Value)
            .Distinct()
            .ToList();
        var fromOwnerLookup = await _dbContext.SystemUsers
            .AsNoTracking()
            .Where(user => fromOwnerIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.RealName, cancellationToken);

        var operatorUserId = Guid.TryParse(_currentUserService.UserId, out var parsedUserId)
            ? parsedUserId
            : (Guid?)null;
        var operatorUserName = _currentUserService.Username ?? string.Empty;
        if (operatorUserId.HasValue)
        {
            operatorUserName = await _dbContext.SystemUsers
                .AsNoTracking()
                .Where(user => user.Id == operatorUserId.Value)
                .Select(user => user.RealName)
                .FirstOrDefaultAsync(cancellationToken) ?? operatorUserName;
        }
        var remark = request.Request.Remark?.Trim() ?? string.Empty;

        foreach (var customer in customers)
        {
            var fromOwnerUserId = customer.OwnerUserId;
            var fromOwnerUserName = fromOwnerUserId.HasValue &&
                fromOwnerLookup.TryGetValue(fromOwnerUserId.Value, out var ownerUserName)
                ? ownerUserName
                : string.Empty;

            customer.AssignOwner(request.Request.OwnerUserId);
            _dbContext.CrmTransferRecords.Add(CrmTransferRecord.Create(
                HerbBaseEntityType,
                customer.Id,
                fromOwnerUserId,
                fromOwnerUserName,
                request.Request.OwnerUserId,
                toOwnerUserName,
                operatorUserId,
                operatorUserName,
                remark));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var dtos = customers.Select(customer => new CrmHerbBaseDto
        {
            Id = customer.Id,
            ParentId = customer.ParentId,
            BaseName = customer.BaseName,
            HerbBaseName = customer.BaseName,
            SubjectName = customer.SubjectName,
            MainProduct = customer.MainProduct,
            Grade = customer.Grade,
            Score = customer.Score,
            Province = customer.Province,
            City = customer.City,
            Area = customer.Area,
            Address = customer.Address,
            Lat = customer.Lat,
            Lng = customer.Lng,
            SourcePlatform = customer.SourcePlatform,
            SourceId = customer.SourceId,
            Status = customer.Status,
            OwnerUserId = customer.OwnerUserId,
            Remark = customer.Remark,
            PrimaryContactName = customer.PrimaryContactName,
            PrimaryContactPhone = customer.PrimaryContactPhone,
            LastFollowAt = customer.LastFollowAt,
            LastFollowResult = customer.LastFollowResult,
            NextFollowAt = customer.NextFollowAt,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        }).ToList();

        await CrmHerbBaseMainProducts.FillAsync(_dbContext, dtos, cancellationToken);
        await CrmHerbBaseOwners.FillAsync(_dbContext, dtos, cancellationToken);

        return dtos;
    }
}




