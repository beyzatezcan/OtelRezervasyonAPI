using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using otelrezervation.DTOs;
using System.Security.Claims;

namespace otelrezervation.Controllers.Web;

public class AuthController : Controller
{
    // Login sayfasini gosterir (GET)
    [HttpGet]
    public IActionResult Login()
    {
        // Eger zaten giris yapmissa, login sayfasini gosterme, anasayfaya at
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    // Formdan gelen verilerle giris yapmayi dener (POST)
    [HttpPost]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        // Form verileri kurallara (Required vb) uymuyorsa sayfayi tekrar goster
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        // Basit Admin kontrolu (Sistemin gercek sahibi)
        // Ileride bu kontrol veritabanindaki bir 'Admins' tablosundan yapilabilir.
        if (dto.Email == "admin@otel.com" && dto.Password == "123456")
        {
            // 1. Kimlik Kartindaki bilgileri (Claims) hazirla
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Sistem Yöneticisi"),
                new Claim(ClaimTypes.Email, dto.Email),
                new Claim(ClaimTypes.Role, "Admin")
            };

            // 2. Kimlik Kartini olustur
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // 3. Tarayiciya cerezi (Cookie) birakip giris islemini tamamla
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity));

            // Basariyla giris yapti, anasayfaya yonlendir
            return RedirectToAction("Index", "Home");
        }

        // Sifre yanlissa hata mesaji ekle ve sayfayi geri goster
        ModelState.AddModelError("", "E-posta veya şifre hatalı!");
        return View(dto);
    }

    // Cikis yapma islemi 
    public async Task<IActionResult> Logout()
    {
        // Tarayicidaki cerezi sil
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        // Login sayfasina geri at
        return RedirectToAction("Login", "Auth");
    }
}
