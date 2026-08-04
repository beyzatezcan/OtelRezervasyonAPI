using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using otelrezervation.Models;

namespace otelrezervation.Services;

public class SiteSettingService : ISiteSettingService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "SiteSettingsCache";

    public SiteSettingService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public string GetValue(string key, string defaultValue = "")
    {
         // cache'den tüm ayarları al
        if (!_cache.TryGetValue(CacheKey, out Dictionary<string, string> settings))
        {
            // cache'den alınamazsa veritabanından al (async kullanmamak daha güvenli)
            settings = _context.SiteSettings
                .Where(s => s.Value != null)
                .ToDictionary(s => s.Key, s => s.Value!);

            // cache options 
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromDays(1));

            
            _cache.Set(CacheKey, settings, cacheEntryOptions);
        }

        if (settings != null && settings.TryGetValue(key, out var value))
        {
            return value;
        }

        return defaultValue;
    }

    public async Task UpdateValueAsync(string key, string value, string pageName = "General")
    {
        var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting != null)
        {
            setting.Value = value;
            setting.PageName = pageName;
        }
        else
        {
            _context.SiteSettings.Add(new SiteSetting { Key = key, Value = value, PageName = pageName });
        }
        
        await _context.SaveChangesAsync();

        // cache'i temizle 
        _cache.Remove(CacheKey);
    }

    public async Task<List<SiteSetting>> GetAllSettingsAsync()
    {
        return await _context.SiteSettings.OrderBy(s => s.Id).ToListAsync();
    }

    public async Task<List<SiteSetting>> GetSettingsByPageAsync(string pageName)
    {
        return await _context.SiteSettings
            .Where(s => s.PageName == pageName)
            .OrderBy(s => s.Id)
            .ToListAsync();
    }

    public async Task DeleteValueAsync(string key)
    {
        var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting != null)
        {
            _context.SiteSettings.Remove(setting);
            await _context.SaveChangesAsync();
            _cache.Remove(CacheKey);
        }
    }
}
