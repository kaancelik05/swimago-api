using Microsoft.AspNetCore.Mvc;
using Swimago.Application.DTOs.Newsletter;
using Swimago.Application.Interfaces;

namespace Swimago.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class NewsletterController : ControllerBase
{
    private readonly INewsletterService _newsletterService;
    private readonly ILogger<NewsletterController> _logger;

    public NewsletterController(
        INewsletterService newsletterService,
        ILogger<NewsletterController> logger)
    {
        _newsletterService = newsletterService;
        _logger = logger;
    }

    /// <summary>
    /// Subscribe to newsletter
    /// </summary>
    [HttpPost("subscribe")]
    [ProducesResponseType(typeof(NewsletterSubscribeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Subscribe([FromBody] NewsletterSubscribeRequest request, CancellationToken cancellationToken)
    {
        var result = await _newsletterService.SubscribeAsync(request, cancellationToken);
        
        if (!result.Success && result.Message == "Geçerli bir e-posta adresi giriniz")
             return BadRequest(result);
             
        return Ok(result);
    }

    /// <summary>
    /// Unsubscribe from newsletter
    /// </summary>
    [HttpPost("unsubscribe")]
    [ProducesResponseType(typeof(NewsletterSubscribeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unsubscribe([FromQuery] string email, [FromQuery] string? token, CancellationToken cancellationToken)
    {
        var result = await _newsletterService.UnsubscribeAsync(email, token, cancellationToken);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
