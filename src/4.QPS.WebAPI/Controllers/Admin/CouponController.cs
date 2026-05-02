using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Features.Coupons;
using QPS.Application.Contracts.Coupons;
using QPS.Application.Pagination;

namespace QPS.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/coupons")]
[Authorize]
public class CouponController : ControllerBase
{
    private readonly IMediator _mediator;

    public CouponController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<CouponDto>>> GetCoupons(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortField = "Title",
        [FromQuery] string sortDirection = "Ascending",
        [FromQuery] string? title = null,
        [FromQuery] string? couponType = null)
    {
        var query = new GetCouponsQuery
        {
            Page = page,
            PageSize = pageSize,
            SortField = sortField,
            SortDirection = sortDirection,
            Title = title,
            CouponType = couponType
        };
        var coupons = await _mediator.Send(query);
        return Ok(coupons);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CouponDto>> GetCoupon(Guid id)
    {
        var query = new GetCouponQuery { Id = id };
        var coupon = await _mediator.Send(query);
        return Ok(coupon);
    }

    [HttpPost]
    public async Task<ActionResult<CouponDto>> CreateCoupon([FromBody] CouponCreateRequest request)
    {
        var command = new CreateCouponCommand { Request = request };
        var coupon = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCoupon), new { id = coupon.Id }, coupon);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CouponDto>> UpdateCoupon(Guid id, [FromBody] CouponUpdateRequest request)
    {
        var command = new UpdateCouponCommand { Id = id, Request = request };
        var coupon = await _mediator.Send(command);
        return Ok(coupon);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteCoupon(Guid id)
    {
        var command = new DeleteCouponCommand { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}