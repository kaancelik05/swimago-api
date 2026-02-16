namespace Swimago.Application.DTOs.Admin.Media;

public class MediaUploadResponse
{
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
