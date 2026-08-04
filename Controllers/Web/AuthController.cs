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
    private readonly IEmailService _emailService;

    public AuthController(AppDbContext context, IUserService userService, IEmailService emailService)
    {
        _context = context;
        _userService = userService;
        _emailService = emailService;
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

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }

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

    // --- SIFREMI UNUTTUM AKISI ---

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
            return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user != null)
        {
            // Benzersiz bir token olustur
            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpiry = DateTime.Now.AddHours(1); // 1 saat gecerli
            await _context.SaveChangesAsync();

            // Linki olustur
            var resetLink = Url.Action("ResetPassword", "Auth", 
                new { email = user.Email, token = user.ResetToken }, 
                Request.Scheme);

            // E-postayi gonder
            string subject = "Şifre Sıfırlama Talebi - Lumina Resort & SPA";
            string body = $@"
                <h3>Şifre Sıfırlama Talebi</h3>
                <p>Merhaba {user.Ad} {user.Soyad},</p>
                <p>Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayabilirsiniz. (Bu bağlantı 1 saat geçerlidir.)</p>
                <p><a href='{resetLink}'>Şifremi Sıfırla</a></p>
                <p>Eğer bu talebi siz yapmadıysanız lütfen bu e-postayı dikkate almayın.</p>
                <br/>
                <p>Lumina Resort & SPA Ekibi</p>
            ";

            await _emailService.SendEmailAsync(user.Email, subject, body);
        }

        // Guvenlik acisindan kullanici yoksa bile 'gonderildi' diyoruz ki email enumeration saldirilari onlensin
        TempData["SuccessMessage"] = "Eğer sistemimizde kayıtlı bir e-posta adresi girdiyseniz, şifre sıfırlama bağlantısı gönderilmiştir.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult ResetPassword(string email, string token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            TempData["ErrorMessage"] = "Geçersiz şifre sıfırlama bağlantısı.";
            return RedirectToAction("Login");
        }

        var dto = new ResetPasswordDto { Email = email, Token = token };
        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var user = await _context.Users.FirstOrDefaultAsync(u => 
            u.Email == dto.Email && 
            u.ResetToken == dto.Token && 
            u.ResetTokenExpiry > DateTime.Now);

        if (user == null)
        {
            TempData["ErrorMessage"] = "Geçersiz veya süresi dolmuş bir şifre sıfırlama bağlantısı kullandınız.";
            return RedirectToAction("Login");
        }

        // Sifreyi guncelle ve tokeni temizle
        user.Password = dto.NewPassword;
        user.ResetToken = null;
        user.ResetTokenExpiry = null;
        
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Şifreniz başarıyla güncellendi! Yeni şifrenizle giriş yapabilirsiniz.";
        return RedirectToAction("Login");
    }
}
