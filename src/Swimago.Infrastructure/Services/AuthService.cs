using AutoMapper;
using Microsoft.Extensions.Configuration;
using Swimago.Application.DTOs.Auth;
using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;
using System.Security.Authentication;

namespace Swimago.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IMapper mapper,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists");
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            CreatedAt = DateTime.UtcNow,
            IsEmailVerified = false
        };

        user.Profile = new UserProfile
        {
            FirstName = new Dictionary<string, string> { { "tr", request.FirstName } },
            LastName = new Dictionary<string, string> { { "tr", request.LastName } },
            PhoneNumber = request.PhoneNumber
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new AuthenticationException("Invalid email or password");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, cancellationToken);

        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            throw new AuthenticationException("Invalid or expired refresh token");
        }

        return await GenerateAuthResponse(user);
    }

    private async Task<AuthResponse> GenerateAuthResponse(User user)
    {
        var token = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiryMinutes = double.Parse(_configuration["Jwt:ExpiryMinutes"]!);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpiryDays"]!));

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<AuthResponse>(user);
        response.Token = token;
        response.TokenExpiry = DateTime.UtcNow.AddMinutes(expiryMinutes);
        response.RefreshToken = refreshToken;
        return response;
    }
}
