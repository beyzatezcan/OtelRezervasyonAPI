using Microsoft.AspNetCore.Mvc;
using otelrezervation.DTOs;
using otelrezervation.Services; // Service'i kullanmak için

namespace otelrezervation.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class RoomsController : ControllerBase
{
    // AppDbContext (veritabanı) GİTTİ! 
    // Sadece IRoomService (menü) VAR!
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    // Tüm Odaları Listele (GET)
    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        // Tüm işi servise pasladık
        var rooms = await _roomService.GetAllRoomsAsync();
        return Ok(rooms);
    }
    
    // Id'ye Göre Tek Oda Getir (GET) - Yeni eklendi!
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoom(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);

        if (room == null)
            return NotFound("Oda bulunamadı.");

        return Ok(room);
    }

    // Odaya Yeni Kayıt Ekleme (POST)
    [HttpPost]
    public async Task<IActionResult> AddRoom(CreateRoomDto dto)
    {
        try
        {
            // Oda var mı kontrolünü ve eklemeyi servis yapıyor, biz sadece sonucu dönüyoruz
            var room = await _roomService.CreateRoomAsync(dto);
            return Ok(room);
        }
        catch (InvalidOperationException ex)
        {
            // Eğer servis "Oda numarası kullanılıyor" diye hata fırlatırsa 400 Bad Request dön
            return BadRequest(ex.Message);
        }
    }

    // Odayı Güncelleme (PUT)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoom(int id, UpdateRoomDto dto)
    {
        try
        {
            var room = await _roomService.UpdateRoomAsync(id, dto);

            if (room == null)
                return NotFound("Güncellenecek oda bulunamadı.");

            return Ok(room);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Odayı Silme (DELETE)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        try
        {
            var result = await _roomService.DeleteRoomAsync(id);

            if (!result)
                return NotFound("Silinecek oda bulunamadı.");

            return Ok("Oda başarıyla silindi.");
        }
        catch (InvalidOperationException ex)
        {
            // Servis "Aktif rezervasyon var, silemezsin!" derse buraya düşecek
            return BadRequest(ex.Message);
        }
    }
}