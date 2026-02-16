namespace Swimago.Application.Interfaces;

public interface ITranslationService
{
    Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage, CancellationToken cancellationToken = default);
    Task<Dictionary<string, string>> TranslateToMultipleLanguagesAsync(string text, string sourceLanguage, IEnumerable<string> targetLanguages, CancellationToken cancellationToken = default);
}
