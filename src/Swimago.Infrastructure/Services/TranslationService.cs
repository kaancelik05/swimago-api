using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Swimago.Application.Interfaces;

namespace Swimago.Infrastructure.Services;

public class TranslationService : ITranslationService
{
    private readonly ILogger<TranslationService> _logger;
    private readonly string? _apiKey;

    public TranslationService(ILogger<TranslationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _apiKey = configuration["OpenAI:ApiKey"];
    }

    public async Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage, CancellationToken cancellationToken = default)
    {
        // TODO: Implement OpenAI GPT translation
        // Use gpt-4o-mini for cost-effective translation
        
        _logger.LogInformation("Translation requested: {SourceLang} -> {TargetLang}", sourceLanguage, targetLanguage);
        
        // Mock response for now
        await Task.CompletedTask;
        return $"[Translated to {targetLanguage}]: {text}";
    }

    public async Task<Dictionary<string, string>> TranslateToMultipleLanguagesAsync(
        string text, 
        string sourceLanguage, 
        IEnumerable<string> targetLanguages, 
        CancellationToken cancellationToken = default)
    {
        var translations = new Dictionary<string, string>();
        
        foreach (var targetLang in targetLanguages)
        {
            var translation = await TranslateAsync(text, sourceLanguage, targetLang, cancellationToken);
            translations[targetLang] = translation;
        }
        
        return translations;
    }
}
