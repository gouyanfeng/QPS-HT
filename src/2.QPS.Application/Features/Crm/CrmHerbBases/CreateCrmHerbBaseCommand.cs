using MediatR;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;

namespace QPS.Application.Features.Crm.CrmHerbBases;

/// <summary>
/// 创建药材基地命令
/// </summary>
public class CreateCrmHerbBaseCommand : IRequest<CrmHerbBaseDto>
{
    public CrmHerbBaseCreateRequest Request { get; set; } = null!;
}

/// <summary>
/// 创建药材基地处理器
/// </summary>
public class CreateCrmHerbBaseHandler : IRequestHandler<CreateCrmHerbBaseCommand, CrmHerbBaseDto>
{
    private readonly IDbContext _dbContext;

    public CreateCrmHerbBaseHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CrmHerbBaseDto> Handle(CreateCrmHerbBaseCommand request, CancellationToken cancellationToken)
    {
        var mainProducts = CrmHerbBaseMainProducts.Normalize(
            request.Request.MainProducts,
            request.Request.MainProduct);
        var mainProductSummary = CrmHerbBaseMainProducts.BuildSummary(mainProducts);
        var baseName = string.IsNullOrWhiteSpace(request.Request.BaseName)
            ? request.Request.HerbBaseName
            : request.Request.BaseName;

        var customer = CrmHerbBase.Create(
            baseName,
            mainProductSummary,
            request.Request.Grade,
            request.Request.Score,
            request.Request.Province,
            request.Request.City,
            request.Request.Area,
            request.Request.Address,
            request.Request.Lat,
            request.Request.Lng,
            request.Request.SourcePlatform,
            request.Request.SourceId,
            request.Request.OwnerUserId,
            request.Request.Remark,
            request.Request.ParentId,
            request.Request.SubjectName
        );

        if (!string.IsNullOrWhiteSpace(request.Request.PrimaryContactName) ||
            !string.IsNullOrWhiteSpace(request.Request.PrimaryContactPhone))
        {
            customer.UpdatePrimaryContact(
                request.Request.PrimaryContactName ?? string.Empty,
                request.Request.PrimaryContactPhone ?? string.Empty);
        }

        _dbContext.CrmHerbBases.Add(customer);
        CrmHerbBaseMainProducts.Sync(_dbContext, customer.Id, mainProducts);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new CrmHerbBaseDto
        {
            Id = customer.Id,
            ParentId = customer.ParentId,
            BaseName = customer.BaseName,
            HerbBaseName = customer.BaseName,
            SubjectName = customer.SubjectName,
            MainProduct = customer.MainProduct,
            MainProducts = mainProducts,
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
        };

        await CrmHerbBaseOwners.FillAsync(_dbContext, new List<CrmHerbBaseDto> { dto }, cancellationToken);

        return dto;
    }
}



