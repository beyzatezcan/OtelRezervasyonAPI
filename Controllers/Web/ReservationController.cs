using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using otelrezervation.Services;
using otelrezervation.DTOs;

namespace otelrezervation.Controllers.Web;

public class ReservationController : Controller
{
    private readonly IReservationService _reservationService;
    private readonly IUserService _userService;
    private readonly IRoomService _roomService;

    // controller sadece kendi servisini degil, User ve Room bilgilerini almak icin
    // diger servisleri de kullanacak.
    public ReservationController(IReservationService reservationService, IUserService userService, IRoomService roomService)
    {
        _reservationService = reservationService;
        _userService = userService;
        _roomService = roomService;
    }

    // 1. rezervasyonlari listele
    public async Task<IActionResult> Index()
    {
        var reservations = await _reservationService.GetAllReservationsAsync();
        return View(reservations);
    }

    // 2. yeni rezervasyon formunu goster
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await DoldurSecimKutulariniAsync();
        
        return View();
    }

    // 3. rezervasyon formunu kaydet
    [HttpPost]
    public async Task<IActionResult> Create(CreateReservationDto dto)
    {
        if (!ModelState.IsValid)
        {
            await DoldurSecimKutulariniAsync();
            return View(dto);
        }

        try
        {
            await _reservationService.CreateReservationAsync(dto);
            TempData["SuccessMessage"] = "Rezervasyon başarıyla oluşturuldu.";
            return RedirectToAction("Index");
        }
        catch (InvalidOperationException ex)
        {
            // servisten gelen kurallar (Tarih gecmise ait olamaz, oda dolu vb.) hatalari
            await DoldurSecimKutulariniAsync();
            ModelState.AddModelError("", ex.Message);
            return View(dto);
        }
    }

    // 4. rezervasyonu iptal et (sil)
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _reservationService.DeleteReservationAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Rezervasyon başarıyla iptal edildi.";
        }
        else
        {
            TempData["ErrorMessage"] = "İptal edilecek rezervasyon bulunamadı.";
        }

        return RedirectToAction("Index");
    }

    // html ile olusturdugumuz secim (dropdown) kutularini doldurur
    private async Task DoldurSecimKutulariniAsync()
    {
        // tum musterileri ve odalari veritabanindan cekiyoruz
        var users = await _userService.GetAllUsersAsync();
        var rooms = await _roomService.GetAllRoomsAsync();

        // musterilerin adi ve soyadini birlestiriyoruz
        var userList = users.Select(u => new 
        { 
            Id = u.Id, 
            AdSoyad = u.Ad + " " + u.Soyad 
        }).ToList();
        
        // ViewBag (Hafıza) içine listeleri ekliyoruz, View (HTML) bunları kullanacak
        ViewBag.Users = new SelectList(userList, "Id", "AdSoyad");
        ViewBag.Rooms = new SelectList(rooms, "Id", "OdaNumarasi");
    }
}
