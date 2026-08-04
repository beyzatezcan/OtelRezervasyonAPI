using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otelrezervation.Models;

namespace otelrezervation.Controllers.Web
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly AppDbContext _context;

        public UserManagementController(AppDbContext context)
        {
            _context = context;
        }

        // Tüm personeli ve müşterileri listele
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // Kullanıcının rolünü güncelle (Admin, Customer, vb.)
        [HttpPost]
        public async Task<IActionResult> ChangeRole(int userId, string newRole)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Role = newRole;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{user.Ad} {user.Soyad} isimli kullanıcının yetkisi '{newRole}' olarak güncellendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
            }

            return RedirectToAction("Index", "UserManagement");
        }
    }
}
