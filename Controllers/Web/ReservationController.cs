using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using otelrezervation.Services;
using otelrezervation.DTOs;

namespace otelrezervation.Controllers.Web;

[Authorize]
public class ReservationController : Controller
{
    private readonly IReservationService _reservationService;
    private readonly IRoomService _roomService;
    private readonly IUserService _userService;

    public ReservationController(IReservationService reservationService, IRoomService roomService, IUserService userService)
    {
        _reservationService = reservationService;
        _roomService = roomService;
        _userService = userService;
    }

    // 1. Tüm rezervasyonları listele (Index)
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var reservations = await _reservationService.GetAllReservationsAsync();
        return View(reservations);
    }

    // 2. Yeni rezervasyon formunu göster (Create GET)
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await DoldurSelectList();
        return View();
    }

    // 3. Formdan gelen rezervasyonu kaydet (Create POST)
    [HttpPost]
    public async Task<IActionResult> Create(CreateReservationDto dto)
    {
        if (!ModelState.IsValid)
        {
            await DoldurSelectList();
            return View(dto);
        }

        try
        {
            await _reservationService.CreateReservationAsync(dto);
            TempData["SuccessMessage"] = "Rezervasyon başarıyla eklendi.";
            return RedirectToAction("Index");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await DoldurSelectList();
            return View(dto);
        }
    }

    // 4. Rezervasyon Silme (Delete POST)
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _reservationService.DeleteReservationAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Rezervasyon iptal edildi.";
        }
        else
        {
            TempData["ErrorMessage"] = "İptal edilecek rezervasyon bulunamadı.";
        }
        return RedirectToAction("Index");
    }

    // YARDIMCI METOT: Dropdownları doldurur
    private async Task DoldurSelectList()
    {
        var rooms = await _roomService.GetAllRoomsAsync();
        var users = await _userService.GetAllUsersAsync();

        // İsim Soyisim birlikte göstermek için anonim obje listesi yapıyoruz
        var userList = users.Select(u => new { Id = u.Id, FullName = u.Ad + " " + u.Soyad }).ToList();

        ViewBag.Rooms = new SelectList(rooms, "Id", "OdaNumarasi");
        ViewBag.Users = new SelectList(userList, "Id", "FullName");
    }
}
