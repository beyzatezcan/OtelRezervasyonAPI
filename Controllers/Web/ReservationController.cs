using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using otelrezervation.Services;
using otelrezervation.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace otelrezervation.Controllers.Web;

[Authorize(Roles = "Admin, Receptionist")] // Admin ve Resepsiyonist erisebilir
public class ReservationController : Controller
{
    private readonly AppDbContext _context; // appdbcontext sinifini kullanarak veritabani islemleri yapiyoruz
    private readonly IReservationService _reservationService; // IReservationService arayuzunu kullanarak rezervasyon islemleri yapiyoruz
    private readonly IRoomService _roomService; // IRoomService arayuzunu kullanarak oda islemleri yapiyoruz
    private readonly IUserService _userService; // IUserService arayuzunu kullanarak kullanici islemleri yapiyoruz

    public ReservationController(AppDbContext context, IReservationService reservationService, IRoomService roomService, IUserService userService)
    {
        _context = context;
        _reservationService = reservationService;
        _roomService = roomService;
        _userService = userService;
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
    public async Task<IActionResult> Create(int? roomId)
    {
        ViewBag.Users = await _userService.GetAllUsersAsync();
        ViewBag.Rooms = await _roomService.GetAllRoomsAsync();
        
        var dto = new otelrezervation.DTOs.CreateReservationDto();
        if (roomId.HasValue)
        {
            dto.RoomId = roomId.Value;
        }

        var tumRezervasyonlar = await _context.Reservations
            .Where(r => r.CikisTarihi >= System.DateTime.Today)
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
                .Where(r => r.CikisTarihi >= System.DateTime.Today)
                .Select(r => new { r.RoomId, Giris = r.GirisTarihi, Cikis = r.CikisTarihi })
                .ToListAsync();
            ViewBag.TumDoluTarihler = System.Text.Json.JsonSerializer.Serialize(tumRezervasyonlar);

            return View(dto);
        }

        try
        {
            await _reservationService.CreateReservationAsync(dto);
            TempData["SuccessMessage"] = "Rezervasyon sistem tarafından başarıyla oluşturuldu.";
            return RedirectToAction("Index");
        }
        catch (System.InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.Users = await _userService.GetAllUsersAsync();
            ViewBag.Rooms = await _roomService.GetAllRoomsAsync();
            
            var tumRezervasyonlar = await _context.Reservations
                .Where(r => r.CikisTarihi >= System.DateTime.Today)
                .Select(r => new { r.RoomId, Giris = r.GirisTarihi, Cikis = r.CikisTarihi })
                .ToListAsync();
            ViewBag.TumDoluTarihler = System.Text.Json.JsonSerializer.Serialize(tumRezervasyonlar);

            return View(dto);
        }
    }
    
    // Yöneticinin rezervasyon iptal etmesi/silmesi
    [HttpPost]
    [Authorize(Roles = "Admin")] // Sadece Admin silebilir, Resepsiyonist silemez!
    public async Task<IActionResult> Delete(int id)
    {
        await _reservationService.DeleteReservationAsync(id);
        TempData["SuccessMessage"] = "Rezervasyon sistemden başarıyla silindi.";
        return RedirectToAction("Index");
    }
}
