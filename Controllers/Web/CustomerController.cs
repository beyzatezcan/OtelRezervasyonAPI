using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using otelrezervation.Services;
using otelrezervation.DTOs;
using otelrezervation.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace otelrezervation.Controllers.Web;

[Authorize] 
public class CustomerController : Controller
{
    private readonly AppDbContext _context;
    private readonly IReservationService _reservationService;
    private readonly IRoomService _roomService;
    private readonly IEmailService _emailService;

    public CustomerController(AppDbContext context, IReservationService reservationService, IRoomService roomService, IEmailService emailService)
    {
        _context = context;
        _reservationService = reservationService;
        _roomService = roomService;
        _emailService = emailService;
    }

    [Authorize(Roles = "Customer")]
    [HttpGet] // musterinin yaptigi tum rezervasyonlari listeler
    public async Task<IActionResult> MyReservations()
    {   //musteri yapiyor ama kim oldugunu bulmamiz lazim 
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier); 
        if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Auth"); // eger user id yoksa login sayfasına yönlendir
        
        int userId = int.Parse(userIdStr);
        
        // Entity uzerinden sorgulayalim cunku ReservationDto icinde UserId yok
        var myReservations = await _context.Reservations
            .Include(r => r.Room)
            .Where(r => r.UserId == userId)
            .ToListAsync();

        return View(myReservations);
    }

    [HttpGet] //musteri profilini goruntuler
    public async Task<IActionResult> Profile()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Auth");
        
        var user = await _context.Users.FindAsync(int.Parse(userIdStr)); 
        if (user == null) return RedirectToAction("Logout", "Auth");

        var dto = new UpdateProfileDto
        {
            Ad = user.Ad,
            Soyad = user.Soyad,
            Telefon = user.Telefon
        };
        ViewBag.UserEmail = user.Email; // E-posta sabittir, salt okunur gösterilecek

        return View(dto);
    }

    [HttpPost] // profili günceller
    public async Task<IActionResult> Profile(UpdateProfileDto dto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Auth");

        var user = await _context.Users.FindAsync(int.Parse(userIdStr));
        if (user == null) return NotFound();

        ViewBag.UserEmail = user.Email; // Hata anında formu geri dönerken lazim 

        if (!ModelState.IsValid) return View(dto);

        // Sifre degistirme talebi var mi?
        if (!string.IsNullOrEmpty(dto.NewPassword))
        {
            if (user.Password != dto.CurrentPassword)
            {
                ModelState.AddModelError("CurrentPassword", "Mevcut şifrenizi yanlış girdiniz.");
                return View(dto);
            }
            user.Password = dto.NewPassword;
        }

        user.Ad = dto.Ad;
        user.Soyad = dto.Soyad;
        user.Telefon = dto.Telefon;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        // Navbar'daki ismin anında güncellenmesi için çerezleri (Cookie) yeniliyoruz
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Ad + " " + user.Soyad),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
        return RedirectToAction("Profile");
    }

    [Authorize(Roles = "Customer")]
    [HttpGet] // oda secme islemi 
    public async Task<IActionResult> BookRoom(int roomId)
    {
        var room = await _roomService.GetRoomByIdAsync(roomId);
        if (room == null) return RedirectToAction("Konaklama", "Home");

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier); // user id bulur
        
        var dto = new CreateReservationDto
        {
            RoomId = roomId,
            UserId = int.Parse(userIdStr!),
            GirisTarihi = DateTime.Today.AddDays(1),
            CikisTarihi = DateTime.Today.AddDays(2)
        };

        ViewBag.OdaNumarasi = room.OdaNumarasi;
        ViewBag.GecelikFiyat = room.GecelikFiyat;

        // Odaya ait mevcut rezervasyonlarin tarihlerini cek
        var doluTarihler = await _context.Reservations
            .Where(r => r.RoomId == roomId && r.CikisTarihi >= DateTime.Today && r.Status != ReservationStatus.Cancelled)
            .Select(r => new { Giris = r.GirisTarihi, Cikis = r.CikisTarihi })
            .ToListAsync();
            
        ViewBag.DoluTarihler = System.Text.Json.JsonSerializer.Serialize(doluTarihler);

        return View(dto);
    }

    [Authorize(Roles = "Customer")]
    [HttpPost] // rezervasyon yapma islemi
    public async Task<IActionResult> BookRoom(CreateReservationDto dto)
    {
        if (!ModelState.IsValid)
        {
            var room = await _roomService.GetRoomByIdAsync(dto.RoomId);
            if(room != null) 
            {
                ViewBag.OdaNumarasi = room.OdaNumarasi;
                ViewBag.GecelikFiyat = room.GecelikFiyat;
            }
            
            var doluTarihler = await _context.Reservations
                .Where(r => r.RoomId == dto.RoomId && r.CikisTarihi >= DateTime.Today && r.Status != ReservationStatus.Cancelled)
                .Select(r => new { Giris = r.GirisTarihi, Cikis = r.CikisTarihi })
                .ToListAsync();
            ViewBag.DoluTarihler = System.Text.Json.JsonSerializer.Serialize(doluTarihler);

            return View(dto);
        }

        try
        {
            dto.UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _reservationService.CreateReservationAsync(dto);
            
            // Rezervasyon basarili oldu, email gonderelim
            var user = await _context.Users.FindAsync(dto.UserId);
            var room = await _roomService.GetRoomByIdAsync(dto.RoomId);
            if (user != null && room != null)
            {
                int geceSayisi = (dto.CikisTarihi - dto.GirisTarihi).Days;
                if (geceSayisi <= 0) geceSayisi = 1;
                decimal toplamTutar = geceSayisi * room.GecelikFiyat;

                string subject = "Rezervasyon Onayı - Lumina Resort & SPA";
                string body = $@"
                    <h2>Sayın {user.Ad} {user.Soyad}, Rezervasyonunuz Onaylandı!</h2>
                    <p>Bizi tercih ettiğiniz için teşekkür ederiz. Tatilinizin detayları aşağıdadır:</p>
                    <ul>
                        <li><strong>Oda Numarası:</strong> {room.OdaNumarasi} ({room.OdaTipi})</li>
                        <li><strong>Giriş Tarihi:</strong> {dto.GirisTarihi.ToString("dd.MM.yyyy")}</li>
                        <li><strong>Çıkış Tarihi:</strong> {dto.CikisTarihi.ToString("dd.MM.yyyy")}</li>
                        <li><strong>Gece Sayısı:</strong> {geceSayisi}</li>
                        <li><strong>Toplam Tutar:</strong> {toplamTutar.ToString("N0")} ₺</li>
                    </ul>
                    <p>Otelimize giriş yaparken kimliğinizi ibraz etmeniz yeterlidir. İyi tatiller dileriz!</p>
                    <br/>
                    <p>Lumina Resort & SPA</p>
                ";

                await _emailService.SendEmailAsync(user.Email, subject, body);
            }

            TempData["SuccessMessage"] = "Harika! Rezervasyonunuz başarıyla oluşturuldu.";
            return RedirectToAction("Konaklama", "Home");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            var room = await _roomService.GetRoomByIdAsync(dto.RoomId);
            if(room != null) 
            {
                ViewBag.OdaNumarasi = room.OdaNumarasi;
                ViewBag.GecelikFiyat = room.GecelikFiyat;
            }
            
            var doluTarihler = await _context.Reservations
                .Where(r => r.RoomId == dto.RoomId && r.CikisTarihi >= DateTime.Today)
                .Select(r => new { Giris = r.GirisTarihi, Cikis = r.CikisTarihi })
                .ToListAsync();
            ViewBag.DoluTarihler = System.Text.Json.JsonSerializer.Serialize(doluTarihler);

            return View(dto);
        }
    }
    
    [Authorize(Roles = "Customer")]
    [HttpPost] // rezervasyonu iptal etme islemi
    public async Task<IActionResult> CancelReservation(int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var res = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUserId);
        
        if (res != null)
        {
            // V3: Müşteri iptali için 48 saat kuralı
            if (res.Status == ReservationStatus.Pending && (res.GirisTarihi - DateTime.Now).TotalHours >= 48)
            {
                res.Status = ReservationStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Rezervasyonunuz başarıyla iptal edildi.";
            }
            else
            {
                TempData["ErrorMessage"] = "İptal işlemi başarısız. Sadece giriş tarihinize en az 48 saat kalan ve henüz onaylanmamış rezervasyonları iptal edebilirsiniz.";
            }
        }
        else
        {
            TempData["ErrorMessage"] = "Yetkisiz işlem! Bu rezervasyon bulunamadı.";
        }
        
        return RedirectToAction("MyReservations");
    }

    // Degerlendirme ekleme (Get)
    [Authorize(Roles = "Customer")]
    [HttpGet]
    public async Task<IActionResult> AddReview(int roomId, int reservationId)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Musteri gercekten bu odaya gelmis mi ve cikis tarihi gecti mi kontrol et
        var hasValidReservation = await _context.Reservations
            .AnyAsync(r => r.Id == reservationId && r.UserId == currentUserId && r.RoomId == roomId && r.CikisTarihi < DateTime.Now && (r.Status == ReservationStatus.CheckedOut || r.Status == ReservationStatus.CheckedIn));

        if (!hasValidReservation)
        {
            TempData["ErrorMessage"] = "Değerlendirme yapmak için odada konaklamış ve çıkış tarihinizin geçmiş olması gerekmektedir.";
            return RedirectToAction("MyReservations");
        }

        // Zaten yorum yapmis mi kontrolu
        var existingReview = await _context.Reviews.AnyAsync(r => r.RoomId == roomId && r.UserId == currentUserId);
        if (existingReview)
        {
            TempData["ErrorMessage"] = "Bu odaya daha önce değerlendirme yaptınız.";
            return RedirectToAction("MyReservations");
        }

        var review = new Review { RoomId = roomId, UserId = currentUserId };
        return View(review);
    }

    // Degerlendirme ekleme (Post)
    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<IActionResult> AddReview(Review review)
    {
        ModelState.Remove("User");
        ModelState.Remove("Room");
        
        if (!ModelState.IsValid) return View(review);

        // guvenlik kontrolu
        review.UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!); // overpostingi onlemek icin id eslemesi yapar
        review.Tarih = DateTime.Now;
        review.IsApproved = true; 

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Değerlendirmeniz başarıyla kaydedildi! Teşekkür ederiz.";
        return RedirectToAction("MyReservations");
    }
}
