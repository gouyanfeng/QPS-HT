using MediatR;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.Merchants;
using QPS.Application.Features.Merchants;

namespace QPS.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/merchants")]
public class MerchantController : ControllerBase
{
    private readonly IMediator _mediator;

    public MerchantController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<MerchantDto>>> GetMerchants()
    {
        var merchants = await _mediator.Send(new GetMerchantsQuery());
        return Ok(merchants);
    }
}