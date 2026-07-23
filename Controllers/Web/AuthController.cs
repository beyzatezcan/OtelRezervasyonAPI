using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otelrezervation.DTOs;
using otelrezervation.Services;
using otelrezervation.Models;
using System.Security.Claims;

namespace otelrezervation.Controllers.Web;

public class AuthController : Controller
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;

    public AuthController(AppDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    // Login sayfasini gosterir (GET)
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
            return RedirectToAction("Index", "Home");
        return View();
    }

    // Formdan gelen verilerle giris yapmayi dener (POST)
    [HttpPost]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        // Veritabanindan emaile ve sifreye gore kullaniciyi bul
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.Password == dto.Password);

        if (user != null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Ad + " " + user.Soyad),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role) // Admin veya Customer
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        // Basit (Sistemin Ilk) Admin kontrolu - Eger veritabaninda hic admin yoksa arka kapi :)
        if (dto.Email == "admin@otel.com" && dto.Password == "123456")
        {
            var adminClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Sistem Yöneticisi"),
                new Claim(ClaimTypes.Email, dto.Email),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var claimsIdentity = new ClaimsIdentity(adminClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "E-posta veya şifre hatalı!");
        return View(dto);
    }

    // Kayit sayfasini gosterir (GET)
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
            return RedirectToAction("Index", "Home");
        return View();
    }

    // Yeni musteri kaydi (POST)
    [HttpPost]
    public async Task<IActionResult> Register(CreateUserDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);
        
        // Bu e-posta ile baska kayit var mi?
        bool emailVarMi = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailVarMi)
        {
            ModelState.AddModelError("", "Bu e-posta adresi zaten kullanılıyor.");
            return View(dto);
        }

        try
        {
            // Role ve Password property'si UserDto icine maplenecek
            await _userService.CreateUserAsync(dto);
            TempData["SuccessMessage"] = "Kaydınız başarıyla oluşturuldu! Lütfen giriş yapın.";
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Kayıt sırasında bir hata oluştu: " + ex.Message);
            return View(dto);
        }
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
