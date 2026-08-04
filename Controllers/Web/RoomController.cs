using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using otelrezervation.Services;
using otelrezervation.DTOs;

namespace otelrezervation.Controllers.Web;

[Authorize(Roles = "Admin")]
public class RoomController : Controller
{
    private readonly IRoomService _roomService;
    private readonly IWebHostEnvironment _env;

    public RoomController(IRoomService roomService, IWebHostEnvironment env)
    {
        _roomService = roomService;
        _env = env;
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
            if (dto.ImageFiles != null && dto.ImageFiles.Count > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "rooms");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                
                foreach(var file in dto.ImageFiles)
                {
                    if (file.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        
                        dto.ImageUrls.Add("/images/rooms/" + uniqueFileName);
                    }
                }
            }

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
            GecelikFiyat = room.GecelikFiyat,
            OdaTipi = room.OdaTipi,
            Kapasite = room.Kapasite,
            Aciklama = room.Aciklama,
            ImageUrls = room.ImageUrls ?? new List<string>()
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
            if (dto.ImageFiles != null && dto.ImageFiles.Count > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "rooms");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                
                foreach(var file in dto.ImageFiles)
                {
                    if (file.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        
                        dto.ImageUrls.Add("/images/rooms/" + uniqueFileName);
                    }
                }
            }

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
