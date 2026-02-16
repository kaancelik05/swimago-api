using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swimago.API.Attributes;
using Swimago.Application.DTOs.Blog;
using Swimago.Application.DTOs.Common;
using Swimago.Application.Interfaces;
using Swimago.Domain.Enums;
using System.Security.Claims;

namespace Swimago.API.Controllers;

/// <summary>
/// Blog and content management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BlogController : ControllerBase
{
    private readonly IBlogService _blogService;
    private readonly ILogger<BlogController> _logger;

    public BlogController(IBlogService blogService, ILogger<BlogController> logger)
    {
        _blogService = blogService;
        _logger = logger;
    }

    /// <summary>
    /// Get all published blog posts with pagination (public endpoint)
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BlogPostDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublished([FromQuery] PaginationQuery query, CancellationToken cancellationToken)
    {
        // Note: The interface returns BlogPostResponse, but controller originally declared returning BlogPostDto.
        // Assuming PagedResult<BlogPostResponse> can be returned as IActionResult
        var posts = await _blogService.GetPublishedPostsAsync(query, cancellationToken);
        return Ok(posts);
    }

    /// <summary>
    /// Get blog post by slug (public endpoint, SEO-friendly)
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(BlogPostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var post = await _blogService.GetBlogPostBySlugAsync(slug, cancellationToken);
        if (post == null) return NotFound();
        return Ok(post);
    }

    /// <summary>
    /// Create a new blog post (Admin only)
    /// </summary>
    [AuthorizeRoles(Role.Admin)]
    [HttpPost]
    [ProducesResponseType(typeof(BlogPostDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateBlogPostRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var post = await _blogService.CreateBlogPostAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetBySlug), new { slug = post.Slug }, post);
    }

    /// <summary>
    /// Update an existing blog post (Author or Admin)
    /// </summary>
    [AuthorizeRoles(Role.Admin)]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BlogPostDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBlogPostRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var post = await _blogService.UpdateBlogPostAsync(id, userId, request, cancellationToken);
        return Ok(post);
    }

    /// <summary>
    /// Delete a blog post (Author or Admin)
    /// </summary>
    [AuthorizeRoles(Role.Admin)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _blogService.DeleteBlogPostAsync(id, userId, cancellationToken);
        return NoContent();
    }
}
