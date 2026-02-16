namespace Swimago.API.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // HSTS - Force HTTPS
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        
        // Prevent clickjacking
        context.Response.Headers["X-Frame-Options"] = "DENY";
        
        // Prevent MIME-sniffing
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        
        // XSS Protection (legacy but still useful)
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        
        // Referrer Policy
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        
        // Content Security Policy
        context.Response.Headers["Content-Security-Policy"] = 
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self' data:; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none';";
        
        // Permissions Policy (formerly Feature Policy)
        context.Response.Headers["Permissions-Policy"] = 
            "geolocation=(self), microphone=(), camera=()";

        await _next(context);
    }
}
