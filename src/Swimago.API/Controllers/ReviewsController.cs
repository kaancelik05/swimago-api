using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swimago.API.Authorization;
using Swimago.Application.DTOs.Reviews;
using Swimago.Application.Interfaces;
using System.Security.Claims;

namespace Swimago.API.Controllers;

/// <summary>
/// Review and rating management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    /// <summary>
    /// Create a review for a completed reservation
    /// </summary>
    /// <param name="request">Review details (rating 1-5 and comment)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created review</returns>
    /// <response code="201">Review created successfully</response>
    /// <response code="400">User hasn't completed a reservation for this listing or already reviewed</response>
    /// <response code="404">Listing not found</response>
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    [HttpPost]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        _logger.LogInformation("User {UserId} creating review for listing {ListingId}", userId, request.ListingId);

        try
        {
            var response = await _reviewService.CreateReviewAsync(userId, request, cancellationToken);
            
            _logger.LogInformation("Review created successfully: {ReviewId}", response.Id);
            
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Review creation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Listing not found: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get review by ID
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Review details</returns>
    /// <response code="200">Returns review details</response>
    /// <response code="404">Review not found</response>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _reviewService.GetReviewByIdAsync(id, cancellationToken);
        
        if (response == null)
            return NotFound(new { error = "Yorum bulunamadı" });

        return Ok(response);
    }

    /// <summary>
    /// Get all reviews for a listing (public endpoint)
    /// </summary>
    /// <param name="listingId">Listing ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of reviews for the listing</returns>
    /// <response code="200">Returns list of reviews</response>
    [AllowAnonymous]
    [HttpGet("listing/{listingId}")]
    [ProducesResponseType(typeof(IEnumerable<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListingReviews(Guid listingId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching reviews for listing {ListingId}", listingId);
        
        var response = await _reviewService.GetListingReviewsAsync(listingId, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Add host response to a review
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <param name="request">Host response text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated review with host response</returns>
    /// <response code="200">Host response added successfully</response>
    /// <response code="400">Review already has a host response</response>
    /// <response code="403">User is not the host of this listing</response>
    /// <response code="404">Review not found</response>
    [Authorize(Policy = AuthorizationPolicies.HostOnly)]
    [HttpPost("{id}/host-response")]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddHostResponse(Guid id, [FromBody] AddHostResponseRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        _logger.LogInformation("Host {UserId} adding response to review {ReviewId}", userId, id);

        try
        {
            var response = await _reviewService.AddHostResponseAsync(id, userId, request, cancellationToken);
            
            _logger.LogInformation("Host response added successfully to review {ReviewId}", id);
            
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Host response failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized host response attempt: {Message}", ex.Message);
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Yorum bulunamadı" });
        }
    }

    /// <summary>
    /// Delete a review (only by the review author)
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Review deleted successfully</response>
    /// <response code="403">User is not the review author</response>
    /// <response code="404">Review not found</response>
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        _logger.LogInformation("User {UserId} attempting to delete review {ReviewId}", userId, id);

        try
        {
            await _reviewService.DeleteReviewAsync(id, userId, cancellationToken);
            
            _logger.LogInformation("Review {ReviewId} deleted successfully", id);
            
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Unauthorized delete attempt by user {UserId}", userId);
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Yorum bulunamadı" });
        }
    }
}
