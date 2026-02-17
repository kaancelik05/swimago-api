using Microsoft.AspNetCore.Http;
using Swimago.Application.DTOs.Users;

namespace Swimago.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserDashboardResponse> GetDashboardAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<UpdateAvatarResponse> UpdateAvatarAsync(Guid userId, IFormFile file, CancellationToken cancellationToken = default);
    Task UpdateSettingsAsync(Guid userId, UpdateSettingsRequest request, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<ChangeEmailResponse> ChangeEmailAsync(Guid userId, ChangeEmailRequest request, CancellationToken cancellationToken = default);
    Task DeleteAccountAsync(Guid userId, DeleteAccountRequest request, CancellationToken cancellationToken = default);
}
