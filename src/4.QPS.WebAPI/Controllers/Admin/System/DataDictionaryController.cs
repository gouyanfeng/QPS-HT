using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.System.DataDictionaries;
using QPS.Application.Features.System.DataDictionaries;
using QPS.Application.Extensions;

namespace QPS.WebAPI.Controllers.Admin.System;

[ApiController]
[Route("api/admin/data-dictionaries")]
[Authorize]
public class DataDictionaryController : ControllerBase
{
    private readonly IMediator _mediator;

    public DataDictionaryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<DataDictionaryDto>>> GetDataDictionaries(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortField = "SortOrder",
        [FromQuery] string sortDirection = "Ascending",
        [FromQuery] string? code = null,
        [FromQuery] string? name = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] Guid? parentId = null)
    {
        var query = new GetDataDictionariesQuery
        {
            Page = page,
            PageSize = pageSize,
            SortField = sortField,
            SortDirection = sortDirection,
            Code = code,
            Name = name,
            IsActive = isActive,
            ParentId = parentId
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("tree")]
    public async Task<ActionResult<List<DataDictionaryDto>>> GetDataDictionaryTree()
    {
        var result = await _mediator.Send(new GetDataDictionaryTreeQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DataDictionaryDto>> GetDataDictionary(Guid id)
    {
        var result = await _mediator.Send(new GetDataDictionaryQuery { Id = id });
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DataDictionaryDto>> CreateDataDictionary([FromBody] DataDictionaryCreateRequest request)
    {
        var command = new CreateDataDictionaryCommand { Request = request };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetDataDictionary), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DataDictionaryDto>> UpdateDataDictionary(Guid id, [FromBody] DataDictionaryUpdateRequest request)
    {
        var command = new UpdateDataDictionaryCommand { Id = id, Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDataDictionary(Guid id)
    {
        await _mediator.Send(new DeleteDataDictionaryCommand { Id = id });
        return NoContent();
    }
}