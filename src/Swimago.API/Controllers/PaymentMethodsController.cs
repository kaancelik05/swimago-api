using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swimago.API.Authorization;
using Swimago.Application.DTOs.PaymentMethods;
using Swimago.Application.Interfaces;
using System.Security.Claims;

namespace Swimago.API.Controllers;

[Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
[ApiController]
[Route("api/payment-methods")]
[Produces("application/json")]
public class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly ILogger<PaymentMethodsController> _logger;

    public PaymentMethodsController(
        IPaymentMethodService paymentMethodService,
        ILogger<PaymentMethodsController> logger)
    {
        _paymentMethodService = paymentMethodService;
        _logger = logger;
    }

    /// <summary>
    /// Get user's payment methods
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaymentMethodListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _paymentMethodService.GetPaymentMethodsAsync(userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Add a new payment method
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentMethodResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add([FromBody] CreatePaymentMethodRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        try
        {
            var result = await _paymentMethodService.AddPaymentMethodAsync(userId, request, cancellationToken);
            return CreatedAtAction(nameof(GetAll), result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update a payment method
    /// </summary>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(PaymentMethodResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentMethodRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var result = await _paymentMethodService.UpdatePaymentMethodAsync(userId, id, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a payment method
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        try
        {
            await _paymentMethodService.DeletePaymentMethodAsync(userId, id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Set payment method as default
    /// </summary>
    [HttpPatch("{id}/default")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        try
        {
            await _paymentMethodService.SetDefaultPaymentMethodAsync(userId, id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Set payment method as default (PUT alias)
    /// </summary>
    [HttpPut("{id}/default")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> SetDefaultPut(Guid id, CancellationToken cancellationToken)
        => SetDefault(id, cancellationToken);
}
