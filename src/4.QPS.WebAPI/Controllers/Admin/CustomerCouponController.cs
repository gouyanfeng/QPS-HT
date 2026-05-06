using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Features.Coupons;
using QPS.Application.Contracts.Coupons;
using QPS.Application.Pagination;

namespace QPS.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/customer-coupons")]
[Authorize]
public class CustomerCouponController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomerCouponController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<CustomerCouponDto>>> GetCustomerCoupons(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortField = "CreatedAt",
        [FromQuery] string sortDirection = "Descending",
        [FromQuery] Guid? customerId = null,
        [FromQuery] string? status = null)
    {
        var query = new GetCustomerCouponsQuery
        {
            Page = page,
            PageSize = pageSize,
            SortField = sortField,
            SortDirection = sortDirection,
            CustomerId = customerId,
            Status = status
        };
        var customerCoupons = await _mediator.Send(query);
        return Ok(customerCoupons);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerCouponDto>> GetCustomerCoupon(Guid id)
    {
        var query = new GetCustomerCouponQuery { Id = id };
        var customerCoupon = await _mediator.Send(query);
        return Ok(customerCoupon);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerCouponDto>> CreateCustomerCoupon([FromBody] CreateCustomerCouponCommand request)
    {
        var customerCoupon = await _mediator.Send(request);
        return CreatedAtAction(nameof(GetCustomerCoupon), new { id = customerCoupon.Id }, customerCoupon);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerCouponDto>> UpdateCustomerCoupon(Guid id, [FromBody] UpdateCustomerCouponStatusRequest request)
    {
        var command = new UpdateCustomerCouponCommand { Id = id, Status = request.Status };
        var customerCoupon = await _mediator.Send(command);
        return Ok(customerCoupon);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCustomerCoupon(Guid id)
    {
        var command = new DeleteCustomerCouponCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}

public class UpdateCustomerCouponStatusRequest
{
    public string Status { get; set; }
}