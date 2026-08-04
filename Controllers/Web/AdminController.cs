using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using otelrezervation.Models;
using otelrezervation.Services;

namespace otelrezervation.Controllers.Web
{
    [Authorize(Roles = "Admin,Receptionist")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRoomService _roomService;
        private readonly IReservationService _reservationService;
        private readonly IContactService _contactService;
        private readonly AppDbContext _context;

        public AdminController(
            IUserService userService, 
            IRoomService roomService, 
            IReservationService reservationService,
            IContactService contactService,
            AppDbContext context)
        {
            _userService = userService;
            _roomService = roomService;
            _reservationService = reservationService;
            _contactService = contactService;
            _context = context; // Dashboard'daki son 5 rezervasyon sorgusu için hala gerekli
        }

        public async Task<IActionResult> Index(DateTime? checkIn, DateTime? checkOut)
        {
            // Servisler uzerinden dinamik veri cekimi (ViewBag ile view'a aktarilacak)
            ViewBag.TotalUsers = await _userService.GetTotalUserCountAsync();
            ViewBag.TotalRooms = await _roomService.GetTotalRoomCountAsync();
            ViewBag.TotalReservations = await _reservationService.GetTotalReservationCountAsync();
            ViewBag.TotalMessages = await _contactService.GetTotalMessageCountAsync();

            // Son 5 Rezervasyon (Dashboard tablosu icin)
            ViewBag.RecentReservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Room)
                .OrderByDescending(r => r.Id)
                .Take(5)
                .ToListAsync();

            // Hızlı Müsaitlik Sorgulama
            if (checkIn.HasValue && checkOut.HasValue)
            {
                if (checkIn.Value >= checkOut.Value)
                {
                    ViewBag.SearchError = "Çıkış tarihi, giriş tarihinden sonra olmalıdır.";
                }
                else
                {
                    var cakisanOdaIdleri = await _context.Reservations
                        .Where(r => r.Status != ReservationStatus.Cancelled &&
                                    r.GirisTarihi < checkOut.Value && 
                                    r.CikisTarihi > checkIn.Value)
                        .Select(r => r.RoomId)
                        .ToListAsync();

                    ViewBag.AvailableRooms = await _context.Rooms
                        .Where(r => !cakisanOdaIdleri.Contains(r.Id))
                        .ToListAsync();
                    
                    ViewBag.CheckIn = checkIn.Value.ToString("yyyy-MM-dd");
                    ViewBag.CheckOut = checkOut.Value.ToString("yyyy-MM-dd");
                }
            }

            return View();
        }
    }
}
