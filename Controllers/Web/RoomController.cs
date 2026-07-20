using Microsoft.AspNetCore.Mvc;
using otelrezervation.Services;
using otelrezervation.DTOs;

namespace otelrezervation.Controllers.Web;

public class RoomController : Controller
{
    private readonly IRoomService _roomService;

    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    // 1. odalari listele 
    public async Task<IActionResult> Index()
    {
        var rooms = await _roomService.GetAllRoomsAsync();
        return View(rooms);
    }

    // 2. oda ekleme formunu goster
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // 3. oda ekleme formunu kaydet
    [HttpPost]
    public async Task<IActionResult> Create(CreateRoomDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _roomService.CreateRoomAsync(dto);
            TempData["SuccessMessage"] = "Oda başarıyla eklendi.";
            return RedirectToAction("Index");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("OdaNumarasi", ex.Message);
            return View(dto);
        }
    }

    // 4. oda duzenleme formunu eski bilgilerle doldurarak goster
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
        {
            TempData["ErrorMessage"] = "Düzenlenecek oda bulunamadı.";
            return RedirectToAction("Index");
        }

        var updateDto = new UpdateRoomDto
        {
            OdaNumarasi = room.OdaNumarasi,
            GecelikFiyat = room.GecelikFiyat
        };

        ViewBag.RoomId = room.Id;
        return View(updateDto);
    }

    // 5. oda duzenleme formundaki yeni bilgileri kaydet
    [HttpPost]
    public async Task<IActionResult> Edit(int id, UpdateRoomDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.RoomId = id;
            return View(dto);
        }

        try
        {
            var updatedRoom = await _roomService.UpdateRoomAsync(id, dto);
            if (updatedRoom == null)
            {
                TempData["ErrorMessage"] = "Güncellenecek oda bulunamadı.";
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = "Oda başarıyla güncellendi.";
            return RedirectToAction("Index");
        }
        catch (InvalidOperationException ex)
        {
            ViewBag.RoomId = id;
            ModelState.AddModelError("OdaNumarasi", ex.Message);
            return View(dto);
        }
    }

    // 6. odayı sil
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _roomService.DeleteRoomAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Oda başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Silinecek oda bulunamadı.";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction("Index");
    }
}
