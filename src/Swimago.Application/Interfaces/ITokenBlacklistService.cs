namespace Swimago.Application.Interfaces;

public interface ITokenBlacklistService
{
    Task BlacklistTokenAsync(string token, TimeSpan expiresIn);
    Task<bool> IsTokenBlacklistedAsync(string token);
}
