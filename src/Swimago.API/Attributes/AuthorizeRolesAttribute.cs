using Microsoft.AspNetCore.Authorization;
using Swimago.Domain.Enums;

namespace Swimago.API.Attributes;

/// <summary>
/// Authorization attribute for role-based access control
/// </summary>
public class AuthorizeRolesAttribute : AuthorizeAttribute
{
    public AuthorizeRolesAttribute(params Role[] roles)
    {
        Roles = string.Join(",", roles.Select(r => r.ToString()));
    }
}
