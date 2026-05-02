using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.Orders;
using QPS.Application.Features.Orders;
using QPS.Application.Pagination;

namespace QPS.WebAPI.Controllers.Admin;

[Route("api/admin/orders")]
[ApiController]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<OrderDto>>> GetOrders([FromQuery] GetOrdersQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        var result = await _mediator.Send(new GetOrderQuery { OrderId = id });
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var result = await _mediator.Send(new CreateOrderCommand { Request = request });
        return CreatedAtAction(nameof(GetOrder), new { id = result }, result);
    }

    [HttpPost("{id}/settle")]
    public async Task<ActionResult> SettleOrder(Guid id)
    {
        await _mediator.Send(new SettleOrderCommand { OrderId = id });
        return NoContent();
    }
}