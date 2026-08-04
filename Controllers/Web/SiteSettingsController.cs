using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using otelrezervation.Services;
using otelrezervation.Models;

namespace otelrezervation.Controllers.Web;

[Authorize(Roles = "Admin,Marketing")]
public class SiteSettingsController : Controller
{
    private readonly ISiteSettingService _siteSettingService;
    private readonly IWebHostEnvironment _env; 

    public SiteSettingsController(ISiteSettingService siteSettingService, IWebHostEnvironment env)
    {
        _siteSettingService = siteSettingService;
        _env = env; 
    }

    [HttpGet] 
    public IActionResult PageManager()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> EditPage(string page)
    {
        if (string.IsNullOrEmpty(page))
        {
            return RedirectToAction(nameof(Index));
        }

        var settings = await _siteSettingService.GetAllSettingsAsync();
        var pageSettings = settings.Where(s => s.PageName == page).ToList();
        
        ViewBag.CurrentPage = page;
        // EditAbout.cshtml, EditHome.cshtml gibi özel sayfalara yönlendir
        return View($"Edit{page}", pageSettings);
    }

    [HttpGet] 
    public async Task<IActionResult> Index()
    {
        var settings = await _siteSettingService.GetSettingsByPageAsync("General");
        return View(settings);
    }

    [HttpPost] 
    public async Task<IActionResult> UpdateImage(string key, IFormFile imageFile, string pageName = "General")
    {
        if (string.IsNullOrEmpty(key) || imageFile == null || imageFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Lütfen güncellenecek ayarı ve bir resim dosyası seçin.";
            return string.IsNullOrEmpty(pageName) || pageName == "General" ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(EditPage), new { page = pageName });
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            TempData["ErrorMessage"] = "Sadece resim dosyaları (.jpg, .png, vs.) yüklenebilir.";
            return string.IsNullOrEmpty(pageName) || pageName == "General" ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(EditPage), new { page = pageName });
        }

        string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "dynamic");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(fileStream);
        }

        string imageUrl = "/images/dynamic/" + uniqueFileName;
        
        await _siteSettingService.UpdateValueAsync(key, imageUrl, pageName);

        TempData["SuccessMessage"] = "Görsel başarıyla güncellendi.";
        return string.IsNullOrEmpty(pageName) || pageName == "General" ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(EditPage), new { page = pageName });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateText(string key, string value, string pageName = "General")
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
        {
            TempData["ErrorMessage"] = "Key ve Value alanları boş olamaz.";
            return string.IsNullOrEmpty(pageName) || pageName == "General" ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(EditPage), new { page = pageName });
        }

        await _siteSettingService.UpdateValueAsync(key, value, pageName);
        TempData["SuccessMessage"] = "Ayar başarıyla güncellendi.";
        return string.IsNullOrEmpty(pageName) || pageName == "General" ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(EditPage), new { page = pageName });
    }
    [HttpPost]
    public async Task<IActionResult> DeleteSetting(string key, string pageName = "General")
    {
        if (!string.IsNullOrEmpty(key))
        {
            await _siteSettingService.DeleteValueAsync(key);
            TempData["SuccessMessage"] = $"'{key}' anahtarı başarıyla silindi.";
        }
        else
        {
            TempData["ErrorMessage"] = "Silinecek anahtar bulunamadı.";
        }
        
        return string.IsNullOrEmpty(pageName) || pageName == "General" ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(EditPage), new { page = pageName });
    }
}
