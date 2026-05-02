using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Features.Orders;
using QPS.Application.Contracts.Orders;

namespace QPS.WebAPI.Controllers.App;

[ApiController]
[Route("api/app/booking")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("createOrder")]
    public async Task<ActionResult<Guid>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var orderId = await _mediator.Send(new CreateOrderCommand { Request = request });
        return Ok(orderId);
    }

    [HttpPost("settleOrder")]
    public async Task<ActionResult<bool>> SettleOrder([FromBody] SettleOrderRequest request)
    {
        var result = await _mediator.Send(new SettleOrderCommand { OrderId = request.OrderId });
        return Ok(result);
    }
}