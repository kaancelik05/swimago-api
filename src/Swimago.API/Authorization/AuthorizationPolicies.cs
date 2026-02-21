namespace Swimago.API.Authorization;

public static class AuthorizationPolicies
{
    public const string CustomerOnly = nameof(CustomerOnly);
    public const string HostOnly = nameof(HostOnly);
    public const string HostOrAdmin = nameof(HostOrAdmin);
    public const string AdminOnly = nameof(AdminOnly);
}
