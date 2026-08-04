using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using otelrezervation.DTOs;
using otelrezervation.Services; 

namespace otelrezervation.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class RoomsController : ControllerBase
{
    // Sadece IRoomService kullaniyoruz menu gibi dusunebiliriz interface
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    // Tum odalari listele (GET)
    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        // Tum isleri servise pasladik
        var rooms = await _roomService.GetAllRoomsAsync();
        return Ok(rooms);
    }
    
    // Id'ye Gore Tek Oda Getir (GET) 
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoom(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);

        if (room == null)
            return NotFound("Oda bulunamadı.");

        return Ok(room);
    }

    // Odaya yeni kayit ekleme (POST)
    [HttpPost]
    public async Task<IActionResult> AddRoom(CreateRoomDto dto)
    {
        try
        {
            // Oda var mi kontrolu ve eklemeyi servis yapiyor, biz sadece sonucu donuyoruz
            var room = await _roomService.CreateRoomAsync(dto);
            return Ok(room);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Odayi guncelleme (PUT)
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

    // Odayi silme (DELETE)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        try
        {
            var result = await _roomService.DeleteRoomAsync(id);

            if (!result)
                return NotFound("Silinecek oda bulunamadı.");

            return Ok("Oda basariyla silindi.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}