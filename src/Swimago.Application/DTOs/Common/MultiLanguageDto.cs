namespace Swimago.Application.DTOs.Common;

public record MultiLanguageDto(
    string Tr,
    string? En = null,
    string? De = null,
    string? Ru = null
)
{
    /// <summary>
    /// Create a MultiLanguageDto from a Dictionary
    /// </summary>
    public static MultiLanguageDto FromDictionary(Dictionary<string, string>? dict)
    {
        if (dict == null || dict.Count == 0)
            return new MultiLanguageDto("");
            
        return new MultiLanguageDto(
            Tr: dict.GetValueOrDefault("tr") ?? dict.Values.FirstOrDefault() ?? "",
            En: dict.GetValueOrDefault("en"),
            De: dict.GetValueOrDefault("de"),
            Ru: dict.GetValueOrDefault("ru")
        );
    }
}
