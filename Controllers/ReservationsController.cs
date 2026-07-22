using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otelrezervation.Models;
using otelrezervation.DTOs;    

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

    // 1.tum rezervasyonlari listele (GET)
    [HttpGet]
    public async Task<IActionResult> GetReservations()
    {
        // Reservations tablosuna Room tablosunu da dahil ediyoruz (Include)
        // cunku ReservationDto'da OdaNumarasi göstereceğiz, oda bilgisine erişmemiz lazim
        var reservations = await _context.Reservations
            .Include(r => r.Room)    
            .ToListAsync();

        // Entity - DTO cevirisi
        // UserId ve RoomId gizli - disariya sadece oda numarasi ve tarihler cikiyor
        var reservationDtos = reservations.Select(r => new ReservationDto
        {
            Id = r.Id,
            OdaNumarasi = r.Room.OdaNumarasi,    // RoomId yerine oda numarasi gosteriyoruz
            GirisTarihi = r.GirisTarihi,
            CikisTarihi = r.CikisTarihi
        }).ToList();

        return Ok(reservationDtos);
    }

    // 2. Yeni Rezervasyon Yap (POST)
    // MakeReservation(CreateReservationDto dto) - sadece izin verilen alanlar
    [HttpPost]
    public async Task<IActionResult> MakeReservation(CreateReservationDto dto)
    {
        // GUVENLİK DUVARI 1: Musteri (User) gercekten var mi?
        var user = await _context.Users.FindAsync(dto.UserId);
        if (user == null)
        {
            return NotFound("Hata: Rezervasyon yapılmak istenen musteri sistemde bulunamadı!");
        }

        // GUVENLİK DUVARI 2: Oda (Room) gercekten var mi?
        var room = await _context.Rooms.FindAsync(dto.RoomId);
        if (room == null)
        {
            return NotFound("Hata: Rezerve edilmek istenen oda sistemde bulunamadı!");
        }

        // GUVENLİK DUVARI 3: tarih cakismasi kontrolu
        bool isRoomTaken = await _context.Reservations.AnyAsync(r =>
            r.RoomId == dto.RoomId &&
            r.GirisTarihi < dto.CikisTarihi &&
            r.CikisTarihi > dto.GirisTarihi);

        if (isRoomTaken)
        {
            return BadRequest("kritik hata: Secilen oda, belirtilen tarihler arasinda zaten dolu!");
        }

        // dto - entity cevirisi (giris) veritabanina yazmak icin
        var yeniRezervasyon = new Reservation
        {
            UserId = dto.UserId,
            RoomId = dto.RoomId,
            GirisTarihi = dto.GirisTarihi,
            CikisTarihi = dto.CikisTarihi
            // Id YOK — veritabanı otomatik verecek
        };

        _context.Reservations.Add(yeniRezervasyon);
        await _context.SaveChangesAsync();

        // entity - dto cevirisi (cikis) kullaniciya dondurmek icin
        // Disariya UserId ve RoomId gostermiyoruz
        var reservationDto = new ReservationDto
        {
            Id = yeniRezervasyon.Id,
            OdaNumarasi = room.OdaNumarasi,     
            GirisTarihi = yeniRezervasyon.GirisTarihi,
            CikisTarihi = yeniRezervasyon.CikisTarihi
        };

        return Ok(reservationDto);
    }
}