using System.Collections.Concurrent;

namespace Swimago.API.Middleware;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, ClientRequestInfo> _clients = new();
    private readonly int _requestLimit = 100; // requests per minute
    private readonly TimeSpan _timeWindow = TimeSpan.FromMinutes(1);

    public RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        
        if (!_clients.TryGetValue(clientId, out var clientInfo))
        {
            clientInfo = new ClientRequestInfo();
            _clients[clientId] = clientInfo;
        }

        // Clean up old requests
        clientInfo.RequestTimes.RemoveAll(t => DateTime.UtcNow - t > _timeWindow);

        if (clientInfo.RequestTimes.Count >= _requestLimit)
        {
            _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
            
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = _timeWindow.TotalSeconds.ToString();
            
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit aşıldı. Lütfen daha sonra tekrar deneyin.",
                retryAfter = $"{_timeWindow.TotalSeconds} saniye"
            });
            
            return;
        }

        clientInfo.RequestTimes.Add(DateTime.UtcNow);
        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get user ID from claims if authenticated
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user_{userId}";
        }

        // Fall back to IP address
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip_{ip}";
    }

    private class ClientRequestInfo
    {
        public List<DateTime> RequestTimes { get; set; } = new();
    }
}
