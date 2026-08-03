using MediatR;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm.CrmHerbBases;
using QPS.Application.Features.Crm.CrmHerbBaseSubjects;
using QPS.Application.Extensions;

namespace QPS.WebAPI.Controllers.Admin.Crm;

/// <summary>
/// 药材基地控制器
/// </summary>
[Route("api/admin/crm/herb-bases")]
[ApiController]
public class CrmHerbBaseController : ControllerBase
{
    private const string HerbBaseEntityType = "CRM_HERB_BASE";

    private readonly IMediator _mediator;

    public CrmHerbBaseController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/api/admin/crm/herb-base-subjects")]
    public async Task<ActionResult<PaginationResponse<CrmHerbBaseSubjectDto>>> GetHerbBaseSubjects(
        [FromQuery] GetCrmHerbBaseSubjectsQuery query)
    {
        return Ok(await _mediator.Send(query));
    }

    [HttpGet("/api/admin/crm/herb-base-subjects/{id:guid}")]
    public async Task<ActionResult<CrmHerbBaseSubjectDetailDto>> GetHerbBaseSubject(Guid id)
    {
        return Ok(await _mediator.Send(new GetCrmHerbBaseSubjectQuery { Id = id }));
    }

    [HttpPatch("/api/admin/crm/herb-base-subjects/assign-owner")]
    public async Task<ActionResult<bool>> AssignHerbBaseSubjectOwner(
        [FromBody] CrmHerbBaseSubjectAssignOwnerRequest request)
    {
        return Ok(await _mediator.Send(new AssignCrmHerbBaseSubjectOwnerCommand { Request = request }));
    }

    /// <summary>
    /// 获取药材基地列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginationResponse<CrmHerbBaseDto>>> GetCustomers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortField = "CreatedAt",
        [FromQuery] string sortDirection = "Descending",
        [FromQuery] string? baseName = null,
        [FromQuery] string? herbBaseName = null,
        [FromQuery] string? keyword = null,
        [FromQuery] string? grade = null,
        [FromQuery] string? status = null,
        [FromQuery] string? sourcePlatform = null,
        [FromQuery] Guid? ownerUserId = null,
        [FromQuery] List<string>? mainProducts = null,
        [FromQuery] string? province = null,
        [FromQuery] string? city = null,
        [FromQuery] DateTime? nextFollowFrom = null,
        [FromQuery] DateTime? nextFollowTo = null,
        [FromQuery] bool? onlyOverdue = null,
        [FromQuery] bool? onlyNoNextFollow = null)
    {
        var query = new GetCrmHerbBasesQuery
        {
            Page = page,
            PageSize = pageSize,
            SortField = sortField,
            SortDirection = sortDirection,
            BaseName = baseName,
            HerbBaseName = herbBaseName,
            Keyword = keyword,
            Grade = grade,
            Status = status,
            SourcePlatform = sourcePlatform,
            OwnerUserId = ownerUserId,
            MainProducts = mainProducts,
            Province = province,
            City = city,
            NextFollowFrom = nextFollowFrom,
            NextFollowTo = nextFollowTo,
            OnlyOverdue = onlyOverdue,
            OnlyNoNextFollow = onlyNoNextFollow
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 获取药材基地详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CrmHerbBaseDto>> GetCustomer(Guid id)
    {
        var query = new GetCrmHerbBaseQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}/owner-transfers")]
    public async Task<ActionResult<List<CrmTransferRecordDto>>> GetTransferRecords(Guid id)
    {
        var query = new GetCrmTransferRecordsQuery { EntityType = HerbBaseEntityType, EntityId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPatch("assign-owner")]
    public async Task<ActionResult<bool>> AssignOwner([FromBody] CrmHerbBaseAssignOwnerRequest request)
    {
        var command = new AssignCrmHerbBaseOwnerCommand { Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// 创建药材基地
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<bool>> CreateCustomer([FromBody] CrmHerbBaseCreateRequest request)
    {
        var command = new CreateCrmHerbBaseCommand { Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// 更新药材基地
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<bool>> UpdateCustomer(Guid id, [FromBody] CrmHerbBaseUpdateRequest request)
    {
        var command = new UpdateCrmHerbBaseCommand { Id = id, Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// 删除药材基地
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteCustomer(Guid id)
    {
        var command = new DeleteCrmHerbBaseCommand { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}




