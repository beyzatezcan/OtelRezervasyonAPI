using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otelrezervation.Models;

namespace otelrezervation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReservationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReservationsController(AppDbContext context)
    {
        _context = context;
    }

    // 1. Tüm Rezervasyonları Listele (GET)
    [HttpGet]
    public async Task<IActionResult> GetReservations()
    {
        var reservations = await _context.Reservations.ToListAsync();
        return Ok(reservations);
    }

    // 2. Yeni Rezervasyon Yap (POST)
    [HttpPost]
    public async Task<IActionResult> MakeReservation(Reservation yeniRezervasyon)
    {
        // GÜVENLİK DUVARI 1: Müşteri (User) Gerçekten Var Mı?
        var user = await _context.Users.FindAsync(yeniRezervasyon.UserId);
        if (user == null)
        {
            return NotFound("Hata: Rezervasyon yapılmak istenen kullanıcı (Müşteri) sistemde bulunamadı!");
        }

        // GÜVENLİK DUVARI 2: Oda (Room) Gerçekten Var Mı?
        var room = await _context.Rooms.FindAsync(yeniRezervasyon.RoomId);
        if (room == null)
        {
            return NotFound("Hata: Rezerve edilmek istenen oda sistemde bulunamadı!");
        }

        // GÜVENLİK DUVARI 3: TARİH ÇARPIŞMA KONTROLÜ (Collision Detection)
        // Eğer içerideki bir rezervasyonun giriş tarihi bizim çıkışımızdan önceyse 
        // VE içerideki rezervasyonun çıkış tarihi bizim girişimizden sonraysa -> ÇARPIŞMA VARDIR!
        bool isRoomTaken = await _context.Reservations.AnyAsync(r => 
            r.RoomId == yeniRezervasyon.RoomId &&
            r.GirisTarihi < yeniRezervasyon.CikisTarihi && 
            r.CikisTarihi > yeniRezervasyon.GirisTarihi);

        if (isRoomTaken)
        {
            return BadRequest("Kritik Hata: Seçilen oda, belirtilen tarihler arasında zaten dolu! Tarihler çakışıyor.");
        }

        // Tüm güvenlik duvarları aşıldıysa, rezervasyonu veritabanına kaydet
        _context.Reservations.Add(yeniRezervasyon);
        await _context.SaveChangesAsync();

        return Ok(yeniRezervasyon);
    }
}