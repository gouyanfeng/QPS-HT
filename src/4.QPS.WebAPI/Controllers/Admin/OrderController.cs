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

    [HttpPost("{id}/pay")]
    public async Task<ActionResult> PayOrder(Guid id, [FromBody] PayOrderRequest request)
    {
        await _mediator.Send(new PayOrderCommand { OrderId = id, Amount = request.Amount, PaymentMethod = request.PaymentMethod });
        return NoContent();
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult> CompleteOrder(Guid id, [FromBody] CompleteOrderRequest request)
    {
        await _mediator.Send(new CompleteOrderCommand
        {
            OrderId = id,
            OriginAmount = request.OriginAmount,
            DiscountAmount = request.DiscountAmount,
            ActualAmount = request.ActualAmount,
            PaymentMethod = request.PaymentMethod
        });
        return NoContent();
    }
}