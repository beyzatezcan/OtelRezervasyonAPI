using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using otelrezervation.Services;

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
}
