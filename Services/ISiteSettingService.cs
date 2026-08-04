using otelrezervation.Models;

namespace otelrezervation.Services;

public interface ISiteSettingService
{
    string GetValue(string key, string defaultValue = "");
    Task UpdateValueAsync(string key, string value, string pageName = "General");
    Task<List<SiteSetting>> GetAllSettingsAsync();
    Task<List<SiteSetting>> GetSettingsByPageAsync(string pageName);
    Task DeleteValueAsync(string key);
}
