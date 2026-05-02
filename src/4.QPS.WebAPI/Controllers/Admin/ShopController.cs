using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Features.Shops;
using QPS.Application.Contracts.Shops;
using QPS.Application.Pagination;

namespace QPS.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/shops")]
[Authorize]
public class ShopController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShopController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<ShopDto>>> GetShops(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortField = "Name",
        [FromQuery] string sortDirection = "Ascending",
        [FromQuery] string? name = null)
    {
        var query = new GetShopsQuery
        {
            Page = page,
            PageSize = pageSize,
            SortField = sortField,
            SortDirection = sortDirection,
            Name = name
        };
        var shops = await _mediator.Send(query);
        return Ok(shops);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ShopDto>> GetShop(Guid id)
    {
        var query = new GetShopQuery { Id = id };
        var shop = await _mediator.Send(query);
        return Ok(shop);
    }

    [HttpPost]
    public async Task<ActionResult<ShopDto>> CreateShop([FromBody] ShopCreateRequest request)
    {
        var command = new CreateShopCommand { Request = request };
        var shop = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetShop), new { id = shop.Id }, shop);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ShopDto>> UpdateShop(Guid id, [FromBody] ShopUpdateRequest request)
    {
        var command = new UpdateShopCommand { Id = id, Request = request };
        var shop = await _mediator.Send(command);
        return Ok(shop);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteShop(Guid id)
    {
        var command = new DeleteShopCommand { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}