using MediatR;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm.CrmCustomers;
using QPS.Application.Extensions;

namespace QPS.WebAPI.Controllers.Admin.Crm;

/// <summary>
/// 客户控制器
/// </summary>
[Route("api/admin/crm/customers")]
[ApiController]
public class CrmCustomerController : ControllerBase
{
    private readonly IMediator _mediator;

    public CrmCustomerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 获取客户列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginationResponse<CrmCustomerDto>>> GetCustomers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortField = "CreatedAt",
        [FromQuery] string sortDirection = "Descending",
        [FromQuery] string? customerName = null,
        [FromQuery] string? keyword = null,
        [FromQuery] string? customerType = null,
        [FromQuery] string? grade = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? ownerUserId = null,
        [FromQuery] string? mainProduct = null,
        [FromQuery] string? province = null,
        [FromQuery] string? city = null,
        [FromQuery] DateTime? nextFollowFrom = null,
        [FromQuery] DateTime? nextFollowTo = null,
        [FromQuery] bool? onlyOverdue = null,
        [FromQuery] bool? onlyNoNextFollow = null)
    {
        var query = new GetCrmCustomersQuery
        {
            Page = page,
            PageSize = pageSize,
            SortField = sortField,
            SortDirection = sortDirection,
            CustomerName = customerName,
            Keyword = keyword,
            CustomerType = customerType,
            Grade = grade,
            Status = status,
            OwnerUserId = ownerUserId,
            MainProduct = mainProduct,
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
    /// 获取客户详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CrmCustomerDto>> GetCustomer(Guid id)
    {
        var query = new GetCrmCustomerQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 创建客户
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CrmCustomerDto>> CreateCustomer([FromBody] CrmCustomerCreateRequest request)
    {
        var command = new CreateCrmCustomerCommand { Request = request };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCustomer), new { id = result.Id }, result);
    }

    /// <summary>
    /// 更新客户
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CrmCustomerDto>> UpdateCustomer(Guid id, [FromBody] CrmCustomerUpdateRequest request)
    {
        var command = new UpdateCrmCustomerCommand { Id = id, Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// 删除客户
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteCustomer(Guid id)
    {
        var command = new DeleteCrmCustomerCommand { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
