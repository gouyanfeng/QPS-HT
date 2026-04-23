using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.Merchants;
using QPS.Application.Features.Merchants;
using QPS.Application.Pagination;
using System;

namespace QPS.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/merchants")]
[Authorize]
public class MerchantController : ControllerBase
{
    private readonly IMediator _mediator;

    public MerchantController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<MerchantDto>>> GetMerchants(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string sortField = "CreatedAt",
    [FromQuery] string sortDirection = "Descending",
    [FromQuery] string? name = null,
    [FromQuery] string? phone = null,
    [FromQuery] bool? isActive = null,
    [FromQuery] DateTime? startDate = null,
    [FromQuery] DateTime? endDate = null)
    {
        var query = new GetMerchantsQuery
        {
            Page = page,
            PageSize = pageSize,
            SortField = sortField,
            SortDirection = sortDirection,
            Name = name,
            Phone = phone,
            IsActive = isActive,
            StartDate = startDate,
            EndDate = endDate
        };
        var merchants = await _mediator.Send(query);
        return Ok(merchants);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<MerchantDto>> GetMerchant(Guid id)
    {
        var merchant = await _mediator.Send(new GetMerchantQuery { Id = id });
        return Ok(merchant);
    }

    [HttpPost]
    public async Task<ActionResult<MerchantDto>> CreateMerchant([FromBody] MerchantCreateRequest request)
    {
        var merchant = await _mediator.Send(new CreateMerchantCommand { Request = request });
        return CreatedAtAction(nameof(GetMerchant), new { id = merchant.Id }, merchant);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MerchantDto>> UpdateMerchant(Guid id, [FromBody] MerchantUpdateRequest request)
    {
        var merchant = await _mediator.Send(new UpdateMerchantCommand { Id = id, Request = request });
        return Ok(merchant);
    }


}