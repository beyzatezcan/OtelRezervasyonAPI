using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using otelrezervation.Services;
using otelrezervation.DTOs;
using otelrezervation.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace otelrezervation.Controllers.Web;

[Authorize(Roles = "Customer")] 
public class CustomerController : Controller
{
    private readonly AppDbContext _context;
    private readonly IReservationService _reservationService;
    private readonly IRoomService _roomService;

    public CustomerController(AppDbContext context, IReservationService reservationService, IRoomService roomService)
    {
        _context = context;
        _reservationService = reservationService;
        _roomService = roomService;
    }

    [HttpGet]
    public async Task<IActionResult> MyReservations()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Auth");
        
        int userId = int.Parse(userIdStr);
        
        // Entity uzerinden sorgulayalim cunku ReservationDto icinde UserId yok
        var myReservations = await _context.Reservations
            .Include(r => r.Room)
            .Where(r => r.UserId == userId)
            .ToListAsync();

        return View(myReservations);
    }

    [HttpGet]
    public async Task<IActionResult> BookRoom(int roomId)
    {
        var room = await _roomService.GetRoomByIdAsync(roomId);
        if (room == null) return RedirectToAction("Index", "Home");

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var dto = new CreateReservationDto
        {
            RoomId = roomId,
            UserId = int.Parse(userIdStr!),
            GirisTarihi = DateTime.Today.AddDays(1),
            CikisTarihi = DateTime.Today.AddDays(2)
        };

        ViewBag.OdaNumarasi = room.OdaNumarasi;
        ViewBag.GecelikFiyat = room.GecelikFiyat;

        // Odaya ait mevcut rezervasyonlarin tarihlerini (Gelecekteki) cek
        var doluTarihler = await _context.Reservations
            .Where(r => r.RoomId == roomId && r.CikisTarihi >= DateTime.Today)
            .Select(r => new { Giris = r.GirisTarihi, Cikis = r.CikisTarihi })
            .ToListAsync();
            
        ViewBag.DoluTarihler = System.Text.Json.JsonSerializer.Serialize(doluTarihler);

        return View(dto);
    }

    [HttpPost]
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
                .Where(r => r.RoomId == dto.RoomId && r.CikisTarihi >= DateTime.Today)
                .Select(r => new { Giris = r.GirisTarihi, Cikis = r.CikisTarihi })
                .ToListAsync();
            ViewBag.DoluTarihler = System.Text.Json.JsonSerializer.Serialize(doluTarihler);

            return View(dto);
        }

        try
        {
            dto.UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _reservationService.CreateReservationAsync(dto);
            
            TempData["SuccessMessage"] = "Harika! Rezervasyonunuz başarıyla oluşturuldu.";
            return RedirectToAction("MyReservations");
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
    
    [HttpPost]
    public async Task<IActionResult> CancelReservation(int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var res = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUserId);
        
        if (res != null)
        {
            await _reservationService.DeleteReservationAsync(id);
            TempData["SuccessMessage"] = "Rezervasyonunuz başarıyla iptal edildi.";
        }
        else
        {
            TempData["ErrorMessage"] = "Yetkisiz işlem! Bu rezervasyon size ait değil.";
        }
        
        return RedirectToAction("MyReservations");
    }
}
