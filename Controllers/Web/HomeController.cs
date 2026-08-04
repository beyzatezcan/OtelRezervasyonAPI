using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using otelrezervation.Services;
using otelrezervation.Models;

namespace otelrezervation.Controllers.Web;

[AllowAnonymous] 
public class HomeController : Controller
{
    private readonly IRoomService _roomService;

    public HomeController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public async Task<IActionResult> Index()
    {
        var rooms = await _roomService.GetAllRoomsAsync();
        return View(rooms);
    }

    public async Task<IActionResult> Konaklama()
    {
        var rooms = await _roomService.GetAllRoomsAsync();
        return View(rooms);
    }

    public IActionResult About()
    {
        return View();
    }

    public async Task<IActionResult> RoomDetails(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
        {
            return NotFound();
        }
        return View(room);
    }
}
