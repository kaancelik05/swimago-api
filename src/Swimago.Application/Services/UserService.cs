using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Swimago.Application.DTOs.Users;
using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Domain.Interfaces;
using Swimago.Domain.Enums;

namespace Swimago.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IReservationRepository reservationRepository,
        IFavoriteRepository favoriteRepository,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _reservationRepository = reservationRepository;
        _favoriteRepository = favoriteRepository;
        _logger = logger;
    }

    public async Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching profile for user {UserId}", userId);
        
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı");

        var reservations = await _reservationRepository.GetByGuestIdAsync(userId, cancellationToken);
        var favorites = await _favoriteRepository.GetByUserIdAsync(userId, cancellationToken);

        var completedReservations = reservations.Count(r => r.Status == ReservationStatus.Completed);
        var totalSpent = reservations.Where(r => r.Status == ReservationStatus.Completed).Sum(r => r.FinalPrice);

        return new UserProfileResponse(
            Id: user.Id,
            Email: user.Email,
            Role: user.Role.ToString(),
            IsEmailVerified: user.IsEmailVerified,
            CreatedAt: user.CreatedAt,
            LastLoginAt: user.LastLoginAt,
            Profile: new UserProfileDetailsDto(
                FirstName: user.Profile?.FirstName.GetValueOrDefault("tr"),
                LastName: user.Profile?.LastName.GetValueOrDefault("tr"),
                Avatar: user.Profile?.Avatar,
                PhoneNumber: user.Profile?.PhoneNumber,
                Bio: user.Profile?.Bio?.GetValueOrDefault("tr"),
                DateOfBirth: null,
                Country: null,
                City: null
            ),
            Stats: new UserStatsDto(
                TotalReservations: reservations.Count(),
                CompletedReservations: completedReservations,
                TotalReviews: 0,
                TotalSpent: totalSpent,
                FavoriteCount: favorites.Count(),
                MembershipLevel: user.MembershipLevel
            ),
            Settings: new UserSettingsResponseDto(
                EmailNotifications: user.NotificationSettings.EmailNotifications,
                SmsNotifications: false,
                PushNotifications: user.NotificationSettings.PushNotifications,
                Language: user.LanguageSettings.Language,
                Currency: user.LanguageSettings.Currency,
                ProfilePublic: user.PrivacySettings.ProfileVisibility
            )
        );
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating profile for user {UserId}", userId);
        
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı");

        user.Profile ??= new UserProfile { Id = Guid.NewGuid(), UserId = userId };
        
        if (request.FirstName != null)
            user.Profile.FirstName = new Dictionary<string, string> { { "tr", request.FirstName } };
        if (request.LastName != null)
            user.Profile.LastName = new Dictionary<string, string> { { "tr", request.LastName } };
        if (request.PhoneNumber != null)
            user.Profile.PhoneNumber = request.PhoneNumber;
        if (request.Bio != null)
            user.Profile.Bio = new Dictionary<string, string> { { "tr", request.Bio } };

        await _userRepository.UpdateAsync(user, cancellationToken);

        return await GetProfileAsync(userId, cancellationToken);
    }

    public async Task<UpdateAvatarResponse> UpdateAvatarAsync(Guid userId, IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Lütfen bir dosya seçin");

        if (file.Length > 5 * 1024 * 1024)
            throw new ArgumentException("Dosya boyutu 5MB'dan büyük olamaz");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType))
            throw new ArgumentException("Sadece JPEG, PNG veya WebP dosyaları kabul edilir");

        _logger.LogInformation("Updating avatar for user {UserId}", userId);
        
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı");

        // TODO: Implement file upload to storage service
        var avatarUrl = $"/avatars/{userId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        user.Profile ??= new UserProfile { Id = Guid.NewGuid(), UserId = userId };
        user.Profile.Avatar = avatarUrl;
        
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new UpdateAvatarResponse(avatarUrl);
    }

    public async Task UpdateSettingsAsync(Guid userId, UpdateSettingsRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating settings for user {UserId}", userId);
        
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı");

        if (request.EmailNotifications.HasValue)
            user.NotificationSettings.EmailNotifications = request.EmailNotifications.Value;
        if (request.PushNotifications.HasValue)
            user.NotificationSettings.PushNotifications = request.PushNotifications.Value;
        if (request.Language != null)
            user.LanguageSettings.Language = request.Language;
        if (request.Currency != null)
            user.LanguageSettings.Currency = request.Currency;
        if (request.ProfilePublic.HasValue)
            user.PrivacySettings.ProfileVisibility = request.ProfilePublic.Value;

        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (request.NewPassword != request.ConfirmPassword)
            throw new ArgumentException("Yeni şifreler eşleşmiyor");

        if (request.NewPassword.Length < 8)
            throw new ArgumentException("Şifre en az 8 karakter olmalıdır");

        _logger.LogInformation("Changing password for user {UserId}", userId);
        
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı");

        // TODO: Verify current password and update with Supabase Auth
    }

    public async Task DeleteAccountAsync(Guid userId, DeleteAccountRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Account deletion requested for user {UserId}", userId);
        
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı");

        // TODO: Verify password and soft delete user
        user.Status = UserStatus.Banned; // Or Pending/Deleted
        await _userRepository.UpdateAsync(user, cancellationToken);
    }
}
