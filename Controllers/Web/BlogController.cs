using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otelrezervation.Models;
using System.Threading.Tasks;

namespace otelrezervation.Controllers.Web
{
    public class BlogController : Controller
    {
        private readonly AppDbContext _context;

        public BlogController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Blog
        public async Task<IActionResult> Index()
        {
            var blogs = await _context.Blogs.OrderByDescending(b => b.OlusturulmaTarihi).ToListAsync();
            return View(blogs);
        }
    }
}
