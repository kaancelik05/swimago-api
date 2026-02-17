using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swimago.API.Authorization;
using Swimago.Application.DTOs.Admin.Media;
using Swimago.Application.Interfaces;

namespace Swimago.API.Controllers.Admin;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[ApiController]
[Route("api/admin/media")]
[Produces("application/json")]
public class AdminMediaController : ControllerBase
{
    private readonly IAdminMediaService _mediaService;

    public AdminMediaController(IAdminMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(MediaUploadResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string folder, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded" });

        var result = await _mediaService.UploadFileAsync(file, folder, cancellationToken);
        return Ok(result);
    }

    [HttpPost("upload-multiple")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(List<MediaUploadResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadFiles(List<IFormFile> files, [FromForm] string folder, CancellationToken cancellationToken)
    {
        if (files == null || files.Count == 0)
            return BadRequest(new { error = "No files uploaded" });

        var result = await _mediaService.UploadFilesAsync(files, folder, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{fileName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteFile(string fileName, CancellationToken cancellationToken)
    {
        await _mediaService.DeleteFileAsync(fileName, cancellationToken);
        return NoContent();
    }
}
