using Microsoft.AspNetCore.Mvc;
using otelrezervation.DTOs;
using otelrezervation.Services;  

namespace otelrezervation.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // 1. Liste
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    // 2. tek kullaniciyi getirme
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);

        if (user == null)
            return NotFound("Kullanıcı bulunamadı.");

        return Ok(user);
    }

    // 3. yeni kullanici ekleme
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
            return BadRequest(ex.Message);
        }
    }

    // 4. kullaniciyi guncelleme
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

    // 5. kullaniciyi silme
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);

            if (!result)
                return NotFound("Kullanıcı bulunamadı.");

            return Ok("Kullanıcı başarıyla silindi.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}