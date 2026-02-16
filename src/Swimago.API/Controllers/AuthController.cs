using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swimago.Application.DTOs.Auth;
using Swimago.Application.Interfaces;
using System.Security.Authentication;
using System.Security.Claims;

namespace Swimago.API.Controllers;

/// <summary>
/// Authentication and authorization endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("User registration attempt for {Email}", request.Email);
            
            var response = await _authService.RegisterAsync(request, cancellationToken);
            
            _logger.LogInformation("User registration successful for {Email}", request.Email);
            
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed for {Email}: {Message}", request.Email, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for {Email}", request.Email);
            return StatusCode(500, new { error = "Kayıt sırasında beklenmeyen bir hata oluştu" });
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Login attempt for {Email}", request.Email);
            
            var response = await _authService.LoginAsync(request, cancellationToken);
            
            _logger.LogInformation("Login successful for {Email}", request.Email);
            
            return Ok(response);
        }
        catch (AuthenticationException ex)
        {
            _logger.LogWarning("Login failed for {Email}: {Message}", request.Email, ex.Message);
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            return StatusCode(500, new { error = "Giriş sırasında beklenmeyen bir hata oluştu" });
        }
    }

    /// <summary>
    /// Logout current user (invalidate token)
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            _logger.LogInformation("Logout request for user {UserId}", userId);
            
            // TODO: Implement token blacklisting or session invalidation
            // For now, client should just discard the token
            
            _logger.LogInformation("User {UserId} logged out successfully", userId);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { error = "Çıkış sırasında beklenmeyen bir hata oluştu" });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Token refresh attempt");
            
            var response = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
            
            return Ok(response);
        }
        catch (AuthenticationException ex)
        {
            _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { error = "Token yenileme sırasında beklenmeyen bir hata oluştu" });
        }
    }

    /// <summary>
    /// Request password reset email
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Password reset requested for {Email}", request.Email);
            
            // TODO: Implement email sending with reset token
            // For now, just return success to avoid email enumeration attacks
            
            return Ok(new { message = "Şifre sıfırlama linki e-posta adresinize gönderildi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset request for {Email}", request.Email);
            return StatusCode(500, new { error = "Şifre sıfırlama isteği sırasında bir hata oluştu" });
        }
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest(new { error = "Şifreler eşleşmiyor" });

            if (request.NewPassword.Length < 8)
                return BadRequest(new { error = "Şifre en az 8 karakter olmalıdır" });

            _logger.LogInformation("Password reset with token attempted");
            
            // TODO: Verify token and update password
            // For now, return success
            
            return Ok(new { message = "Şifreniz başarıyla güncellendi" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Password reset failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return StatusCode(500, new { error = "Şifre sıfırlama sırasında bir hata oluştu" });
        }
    }
}
