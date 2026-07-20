using Microsoft.AspNetCore.Mvc;
using otelrezervation.DTOs;
using otelrezervation.Services;  // ← Service'i kullanmak için

namespace otelrezervation.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    // ESKİ: private readonly AppDbContext _context;        → Veritabanı direkt controller'daydı
    // YENİ: private readonly IUserService _userService;    → Artık Service'e soruyor
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // 1. LİSTELEME — Eskiden 10+ satırdı, şimdi 2 satır
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    // 2. TEK KULLANICI GETİRME — Yeni özellik (Service'te hazırlamıştık)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);

        if (user == null)
            return NotFound("Kullanıcı bulunamadı.");

        return Ok(user);
    }

    // 3. EKLEME — Eskiden email kontrolü, entity çevirisi hep buradaydı
    // Şimdi controller sadece "Service'e ver, sonucu dön" diyor
    [HttpPost]
    public async Task<IActionResult> AddUser(CreateUserDto dto)
    {
        try
        {
            var user = await _userService.CreateUserAsync(dto);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            // Service hata fırlattıysa (email zaten var gibi) → 400 dön
            return BadRequest(ex.Message);
        }
    }

    // 4. GÜNCELLEME
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, dto);

            if (user == null)
                return NotFound("Güncellenecek kullanıcı bulunamadı.");

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // 5. SİLME
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);

            if (!result)
                return NotFound("Silinecek kullanıcı bulunamadı.");

            return Ok("Kullanıcı başarıyla silindi.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}