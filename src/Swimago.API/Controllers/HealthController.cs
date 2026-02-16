using Microsoft.AspNetCore.Mvc;

namespace Swimago.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            service = "Swimago API"
        });
    }

    [HttpGet("db")]
    public async Task<IActionResult> CheckDatabase(
        [FromServices] Swimago.Infrastructure.Data.ApplicationDbContext dbContext)
    {
        try
        {
            await dbContext.Database.CanConnectAsync();
            return Ok(new
            {
                status = "healthy",
                database = "connected",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "unhealthy",
                database = "disconnected",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
