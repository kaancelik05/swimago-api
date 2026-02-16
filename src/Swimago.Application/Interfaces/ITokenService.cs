using Swimago.Domain.Entities;
using System.Security.Claims;

namespace Swimago.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<Claim>? additionalClaims = null);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
