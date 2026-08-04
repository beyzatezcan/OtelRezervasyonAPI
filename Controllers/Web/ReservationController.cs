using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using otelrezervation.Services;
using otelrezervation.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace otelrezervation.Controllers.Web;

[Authorize(Roles = "Admin,Receptionist")] // Admin ve Resepsiyonist erisebilir
public class ReservationController : Controller
{
    private readonly AppDbContext _context; // appdbcontext sinifini kullanarak veritabani islemleri yapiyoruz
    private readonly IReservationService _reservationService; // IReservationService arayuzunu kullanarak rezervasyon islemleri yapiyoruz
    private readonly IRoomService _roomService; // IRoomService arayuzunu kullanarak oda islemleri yapiyoruz
    private readonly IUserService _userService; // IUserService arayuzunu kullanarak kullanici islemleri yapiyoruz
    private readonly IEmailService _emailService; 

    public ReservationController(AppDbContext context, IReservationService reservationService, IRoomService roomService, IUserService userService, IEmailService emailService)
    {
        _context = context;
        _reservationService = reservationService;
        _roomService = roomService;
        _userService = userService;
        _emailService = emailService;
    }

// tum rezervasyonlarin listelendigi kontrol
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var reservations = await _reservationService.GetAllReservationsAsync();
        return View(reservations);
    }

    // Yöneticinin manuel olarak müşteri seçip rezervasyon yapması
    [HttpGet]
    public async Task<IActionResult> Create(int? roomId, DateTime? checkIn, DateTime? checkOut)
    {
        ViewBag.Users = await _userService.GetAllUsersAsync();
        ViewBag.Rooms = await _roomService.GetAllRoomsAsync();
        
        var dto = new otelrezervation.DTOs.CreateReservationDto();
        if (roomId.HasValue)
        {
            dto.RoomId = roomId.Value;
        }
        
        if (checkIn.HasValue)
        {
            dto.GirisTarihi = checkIn.Value;
        }
        else
        {
            dto.GirisTarihi = System.DateTime.Today;
        }

        if (checkOut.HasValue)
        {
            dto.CikisTarihi = checkOut.Value;
        }
        else
        {
            dto.CikisTarihi = System.DateTime.Today.AddDays(1);
        }

        var tumRezervasyonlar = await _context.Reservations
            .Where(r => r.CikisTarihi >= System.DateTime.Today && r.Status != otelrezervation.Models.ReservationStatus.Cancelled)
            .Select(r => new { r.RoomId, Giris = r.GirisTarihi, Cikis = r.CikisTarihi })
            .ToListAsync();
        ViewBag.TumDoluTarihler = System.Text.Json.JsonSerializer.Serialize(tumRezervasyonlar);
        
        return View(dto);
    }

    // yonetici tarafindan manuel rezervasyon olusturma
    [HttpPost]
    public async Task<IActionResult> Create(otelrezervation.DTOs.CreateReservationDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Users = await _userService.GetAllUsersAsync();
            ViewBag.Rooms = await _roomService.GetAllRoomsAsync();
            
            var tumRezervasyonlar = await _context.Reservations
                .Where(r => r.CikisTarihi >= System.DateTime.Today && r.Status != otelrezervation.Models.ReservationStatus.Cancelled)
                .Select(r => new { r.RoomId, Giris = r.GirisTarihi, Cikis = r.CikisTarihi })
                .ToListAsync();
            ViewBag.TumDoluTarihler = System.Text.Json.JsonSerializer.Serialize(tumRezervasyonlar);

            return View(dto);
        }

        try
        {
            await _reservationService.CreateReservationAsync(dto);
            
            // Mail Gonderimi
            var user = await _context.Users.FindAsync(dto.UserId);
            var room = await _roomService.GetRoomByIdAsync(dto.RoomId);
            if (user != null && room != null)
            {
                int geceSayisi = (dto.CikisTarihi - dto.GirisTarihi).Days;
                if (geceSayisi <= 0) geceSayisi = 1;
                decimal toplamTutar = geceSayisi * room.GecelikFiyat;

                string subject = "Manuel Rezervasyonunuz Oluşturuldu - Lumina Resort & SPA";
                string body = $@"
                    <h2>Sayın {user.Ad} {user.Soyad}, Rezervasyonunuz Yetkililerimiz Tarafından Oluşturuldu!</h2>
                    <p>Bizi tercih ettiğiniz için teşekkür ederiz. Tatilinizin detayları aşağıdadır:</p>
                    <ul>
                        <li><strong>Oda Numarası:</strong> {room.OdaNumarasi} ({room.OdaTipi})</li>
                        <li><strong>Giriş Tarihi:</strong> {dto.GirisTarihi.ToString("dd.MM.yyyy")}</li>
                        <li><strong>Çıkış Tarihi:</strong> {dto.CikisTarihi.ToString("dd.MM.yyyy")}</li>
                        <li><strong>Gece Sayısı:</strong> {geceSayisi}</li>
                        <li><strong>Toplam Tutar:</strong> {toplamTutar.ToString("N0")} ₺</li>
                    </ul>
                    <p>İyi tatiller dileriz!</p>
                    <br/>
                    <p>Lumina Resort & SPA</p>
                ";
                await _emailService.SendEmailAsync(user.Email, subject, body); // yonetici tarafindan yapilan rezervasyonlar icin kullaniciya mail gonderir
            }

            TempData["SuccessMessage"] = "Rezervasyon sistem tarafından başarıyla oluşturuldu.";
            return RedirectToAction("Index");
        }
        catch (System.InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.Users = await _userService.GetAllUsersAsync();
            ViewBag.Rooms = await _roomService.GetAllRoomsAsync();
            
            var tumRezervasyonlar = await _context.Reservations
                .Where(r => r.CikisTarihi >= System.DateTime.Today && r.Status != otelrezervation.Models.ReservationStatus.Cancelled)
                .Select(r => new { r.RoomId, Giris = r.GirisTarihi, Cikis = r.CikisTarihi })
                .ToListAsync();
            ViewBag.TumDoluTarihler = System.Text.Json.JsonSerializer.Serialize(tumRezervasyonlar);

            return View(dto);
        }
    }
    
    // yoneticinin veya resepsiyonistin rezervasyon durumunu güncellemesi (Check-in, Check-out, İptal)
    [HttpPost]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> UpdateStatus(int id, ReservationStatus status)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation != null)
        {
            if (status == ReservationStatus.CheckedIn && System.DateTime.Today < reservation.GirisTarihi.Date)
            {
                TempData["ErrorMessage"] = "Giriş tarihi henüz gelmediği için Check-in yapılamaz.";
                return RedirectToAction("Index");
            }

            reservation.Status = status;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Rezervasyon durumu '{status}' olarak güncellendi.";
        }
        return RedirectToAction("Index");
    }

    // yoneticinin rezervasyon iptal etmesi/silmesi
    [HttpPost]
    [Authorize(Roles = "Admin")] // Sadece admin silebilir, resepsiyonist silemez!
    public async Task<IActionResult> Delete(int id)
    {
        await _reservationService.DeleteReservationAsync(id);
        TempData["SuccessMessage"] = "Rezervasyon sistemden başarıyla silindi.";
        return RedirectToAction("Index");
    }
}
