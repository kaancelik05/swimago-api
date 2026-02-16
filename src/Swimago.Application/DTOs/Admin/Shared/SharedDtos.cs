using System.Text.Json.Serialization;

namespace Swimago.Application.DTOs.Admin.Shared;

public class MultiLanguageDto
{
    public string? Tr { get; set; }
    public string? En { get; set; }
    public string? De { get; set; }
    public string? Ru { get; set; }
}

public class ImageDto
{
    public Guid? Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Alt { get; set; }
    public MultiLanguageDto? Caption { get; set; }
    public int Order { get; set; }
    public bool IsPrimary { get; set; }
}

public class AmenityDto
{
    public Guid? Id { get; set; }
    public string Icon { get; set; } = string.Empty;
    public MultiLanguageDto Label { get; set; } = new();
    public bool Available { get; set; }
}

public class BookingBreakdownItemDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class BreadcrumbItemDto
{
    public string Label { get; set; } = string.Empty;
    public string? Link { get; set; }
}

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
