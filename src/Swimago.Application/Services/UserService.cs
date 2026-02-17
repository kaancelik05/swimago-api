using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Swimago.Application.DTOs.Users;
using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;

namespace Swimago.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IReservationRepository reservationRepository,
        IFavoriteRepository favoriteRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _reservationRepository = reservationRepository;
        _favoriteRepository = favoriteRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching profile for user {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı");

        var reservations = (await _reservationRepository.GetByGuestIdAsync(userId, cancellationToken)).ToList();
        var favorites = (await _favoriteRepository.GetByUserIdAsync(userId, cancellationToken)).ToList();

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
                FirstName: GetLocalizedText(user.Profile?.FirstName),
                LastName: GetLocalizedText(user.Profile?.LastName),
                Avatar: user.Profile?.Avatar,
                PhoneNumber: user.Profile?.PhoneNumber,
                Bio: GetLocalizedText(user.Profile?.Bio),
                DateOfBirth: null,
                Country: null,
                City: null
            ),
            Stats: new UserStatsDto(
                TotalReservations: reservations.Count,
                CompletedReservations: completedReservations,
                TotalReviews: 0,
                TotalSpent: totalSpent,
                FavoriteCount: favorites.Count,
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

    public async Task<UserDashboardResponse> GetDashboardAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var reservations = (await _reservationRepository.GetByGuestIdAsync(userId, cancellationToken))
            .OrderBy(x => x.StartTime)
            .ToList();

        var favorites = (await _favoriteRepository.GetByUserIdAsync(userId, cancellationToken)).ToList();

        var now = DateTime.UtcNow;
        var upcomingReservations = reservations
            .Where(x => x.StartTime >= now && x.Status != ReservationStatus.Cancelled)
            .ToList();

        var upcoming = upcomingReservations.FirstOrDefault();

        var favoriteSpotPreviews = favorites
            .Select(x => x.Listing)
            .Where(x => x != null)
            .Take(5)
            .Select(x => new DashboardFavoriteSpotDto(
                Id: x!.Id,
                Title: GetLocalizedText(x.Title),
                Location: string.IsNullOrWhiteSpace(x.Country) ? x.City : $"{x.City}, {x.Country}",
                ImageUrl: x.Images.FirstOrDefault(i => i.IsCover)?.Url ?? x.Images.FirstOrDefault()?.Url,
                Rating: x.Rating,
                Price: $"{GetCurrencySymbol(x.PriceCurrency)}{x.BasePricePerDay:0.##}",
                PriceUnit: "perPerson",
                IsFavorite: true
            ))
            .ToList();

        var upcomingDto = upcoming == null
            ? null
            : new DashboardUpcomingReservationDto(
                Id: upcoming.Id,
                ResortName: GetLocalizedText(upcoming.Listing?.Title),
                Location: upcoming.Listing == null
                    ? string.Empty
                    : (string.IsNullOrWhiteSpace(upcoming.Listing.Country)
                        ? upcoming.Listing.City
                        : $"{upcoming.Listing.City}, {upcoming.Listing.Country}"),
                ImageUrl: upcoming.Listing?.Images.FirstOrDefault(i => i.IsCover)?.Url
                    ?? upcoming.Listing?.Images.FirstOrDefault()?.Url,
                Date: upcoming.StartTime.Date,
                Time: $"{upcoming.StartTime:HH:mm}",
                Guests: $"{upcoming.GuestCount} Adults",
                Status: upcoming.Status.ToString().ToLowerInvariant(),
                IsFavorite: upcoming.Listing != null && favorites.Any(f => f.VenueId == upcoming.Listing.Id)
            );

        return new UserDashboardResponse(
            Stats: new UserDashboardStatsDto(
                UpcomingTrips: upcomingReservations.Count,
                TotalBookings: reservations.Count,
                RewardPoints: reservations.Count(r => r.Status == ReservationStatus.Completed) * 100,
                WeeklyDelta: reservations.Count(r => r.CreatedAt >= now.AddDays(-7))
            ),
            UpcomingReservation: upcomingDto,
            FavoriteSpots: favoriteSpotPreviews
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

        var avatarUrl = $"/avatars/{userId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        user.Profile ??= new UserProfile { Id = Guid.NewGuid(), UserId = userId };
        user.Profile.Avatar = avatarUrl;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        if (request.DataSharing.HasValue)
            user.PrivacySettings.DataSharing = request.DataSharing.Value;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
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

        // TODO: verify current password hash and set new hash with proper password hasher.
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ChangeEmailResponse> ChangeEmailAsync(Guid userId, ChangeEmailRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NewEmail) || !request.NewEmail.Contains('@'))
        {
            throw new ArgumentException("Geçerli bir e-posta adresi giriniz");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı");

        var normalizedEmail = request.NewEmail.Trim().ToLowerInvariant();
        var existing = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existing != null && existing.Id != userId)
        {
            throw new ArgumentException("Bu e-posta adresi zaten kullanılıyor");
        }

        // TODO: verify password before email change.
        user.Email = normalizedEmail;
        user.IsEmailVerified = false;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ChangeEmailResponse(user.Email, "Email change request received");
    }

    public async Task DeleteAccountAsync(Guid userId, DeleteAccountRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Account deletion requested for user {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı");

        user.Status = UserStatus.Banned;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string GetLocalizedText(Dictionary<string, string>? values)
    {
        if (values == null || values.Count == 0)
        {
            return string.Empty;
        }

        if (values.TryGetValue("tr", out var tr) && !string.IsNullOrWhiteSpace(tr))
        {
            return tr;
        }

        if (values.TryGetValue("en", out var en) && !string.IsNullOrWhiteSpace(en))
        {
            return en;
        }

        return values.Values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty;
    }

    private static string GetCurrencySymbol(string currency)
    {
        return currency.ToUpperInvariant() switch
        {
            "USD" => "$",
            "EUR" => "€",
            "TRY" => "₺",
            _ => string.Empty
        };
    }
}
