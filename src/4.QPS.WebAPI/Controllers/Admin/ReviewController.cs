using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Features.Reviews;
using QPS.Application.Contracts.Reviews;
using QPS.Application.Pagination;

namespace QPS.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/reviews")]
[Authorize]
public class ReviewController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReviewController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<ReviewDto>>> GetReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortField = "CreatedAt",
        [FromQuery] string sortDirection = "Descending",
        [FromQuery] Guid? roomId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? orderId = null)
    {
        var query = new GetReviewsQuery
        {
            Page = page,
            PageSize = pageSize,
            SortField = sortField,
            SortDirection = sortDirection,
            RoomId = roomId,
            CustomerId = customerId,
            OrderId = orderId
        };
        var reviews = await _mediator.Send(query);
        return Ok(reviews);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewDto>> GetReview(Guid id)
    {
        var query = new GetReviewQuery { Id = id };
        var review = await _mediator.Send(query);
        return Ok(review);
    }

    [HttpPost]
    public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] ReviewCreateRequest request)
    {
        var command = new CreateReviewCommand { Request = request };
        var review = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetReview), new { id = review.Id }, review);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ReviewDto>> UpdateReview(Guid id, [FromBody] ReviewUpdateRequest request)
    {
        var command = new UpdateReviewCommand { Id = id, Request = request };
        var review = await _mediator.Send(command);
        return Ok(review);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteReview(Guid id)
    {
        var command = new DeleteReviewCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}