using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swimago.API.Authorization;
using Swimago.Application.DTOs.Admin.Blogs;
using Swimago.Application.DTOs.Admin.Shared;
using Swimago.Application.Interfaces;

namespace Swimago.API.Controllers.Admin;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[ApiController]
[Route("api/admin/blogs")]
[Produces("application/json")]
public class AdminBlogsController : ControllerBase
{
    private readonly IAdminBlogService _blogService;

    public AdminBlogsController(IAdminBlogService blogService)
    {
        _blogService = blogService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<BlogListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBlogs(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] bool? isPublished,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _blogService.GetBlogsAsync(search, category, isPublished, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BlogDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBlog(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _blogService.GetBlogAsync(id, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Blog not found" });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(BlogDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBlog([FromBody] CreateBlogRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _blogService.CreateBlogAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetBlog), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BlogDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBlog(Guid id, [FromBody] CreateBlogRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _blogService.UpdateBlogAsync(id, request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Blog not found" });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteBlog(Guid id, CancellationToken cancellationToken)
    {
        await _blogService.DeleteBlogAsync(id, cancellationToken);
        return NoContent();
    }
}
