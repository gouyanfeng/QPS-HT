using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Features.Customers;
using QPS.Application.Contracts.Customers;
using QPS.Application.Pagination;

namespace QPS.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/customers")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<CustomerDto>>> GetCustomers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortField = "Nickname",
        [FromQuery] string sortDirection = "Ascending",
        [FromQuery] string? phone = null,
        [FromQuery] string? nickname = null)
    {
        var query = new GetCustomersQuery
        {
            Page = page,
            PageSize = pageSize,
            SortField = sortField,
            SortDirection = sortDirection,
            Phone = phone,
            Nickname = nickname
        };
        var customers = await _mediator.Send(query);
        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(Guid id)
    {
        var query = new GetCustomerQuery { Id = id };
        var customer = await _mediator.Send(query);
        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CustomerCreateRequest request)
    {
        var command = new CreateCustomerCommand { Request = request };
        var customer = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDto>> UpdateCustomer(Guid id, [FromBody] CustomerUpdateRequest request)
    {
        var command = new UpdateCustomerCommand { Id = id, Request = request };
        var customer = await _mediator.Send(command);
        return Ok(customer);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteCustomer(Guid id)
    {
        var command = new DeleteCustomerCommand { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}