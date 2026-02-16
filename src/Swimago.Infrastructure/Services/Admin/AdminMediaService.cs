using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Swimago.Application.DTOs.Admin.Media;
using Swimago.Application.Interfaces;

namespace Swimago.Infrastructure.Services.Admin;

public class AdminMediaService : IAdminMediaService
{
    private readonly IConfiguration _configuration;
    // Inject Supabase Client here if available, or usage HTTP client
    // For this implementation plan, we will simulate the structure.
    
    public AdminMediaService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<MediaUploadResponse> UploadFileAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        // Integration with Supabase Storage would go here.
        // 1. Create Supabase Client
        // 2. Upload file to bucket 'images' inside 'folder'
        // 3. Get Public URL
        
        // Mock implementation
        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var mockUrl = $"https://supabase-project.supabase.co/storage/v1/object/public/images/{folder}/{fileName}";

        return await Task.FromResult(new MediaUploadResponse
        {
            Url = mockUrl,
            FileName = fileName,
            FileSize = file.Length
        });
    }

    public async Task<List<MediaUploadResponse>> UploadFilesAsync(List<IFormFile> files, string folder, CancellationToken cancellationToken = default)
    {
        var responses = new List<MediaUploadResponse>();
        foreach (var file in files)
        {
            responses.Add(await UploadFileAsync(file, folder, cancellationToken));
        }
        return responses;
    }

    public async Task DeleteFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        // Delete from Supabase Storage
        await Task.CompletedTask;
    }
}
