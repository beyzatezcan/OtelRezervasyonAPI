using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using otelrezervation.Models;
using otelrezervation.Services;

namespace otelrezervation.Controllers.Web
{
    public class BlogController : Controller
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        // GET: /Blog (Herkese açık blog listesi)
        public async Task<IActionResult> Index()
        {
            var blogs = await _blogService.GetAllBlogsAsync();
            return View(blogs);
        }

        // GET: /Blog/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var blog = await _blogService.GetBlogByIdAsync(id.Value);
            if (blog == null) return NotFound();

            return View(blog);
        }

        // GET: /Blog/AdminIndex (Yönetici Listesi)
        [Authorize(Roles = "Admin,Marketing")]
        public async Task<IActionResult> AdminIndex()
        {
            var blogs = await _blogService.GetAllBlogsAsync();
            return View(blogs);
        }

        // GET: /Blog/Create
        [Authorize(Roles = "Admin,Marketing")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Blog/Create
        [Authorize(Roles = "Admin,Marketing")]
        [HttpPost]
        public async Task<IActionResult> Create(Blog blog)
        {
            if (ModelState.IsValid)
            {
                await _blogService.CreateBlogAsync(blog);
                TempData["SuccessMessage"] = "Blog yazısı başarıyla eklendi.";
                return RedirectToAction(nameof(AdminIndex));
            }
            return View(blog);
        }

        // GET: /Blog/Edit
        [Authorize(Roles = "Admin,Marketing")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var blog = await _blogService.GetBlogByIdAsync(id.Value);
            if (blog == null) return NotFound();

            return View(blog);
        }

        // POST: /Blog/Edit
        [Authorize(Roles = "Admin,Marketing")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Blog blog)
        {
            if (id != blog.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _blogService.UpdateBlogAsync(id, blog);
                if (result == null) return NotFound();

                TempData["SuccessMessage"] = "Blog yazısı başarıyla güncellendi.";
                return RedirectToAction(nameof(AdminIndex));
            }
            return View(blog);
        }

        // POST: /Blog/Delete
        [Authorize(Roles = "Admin,Marketing")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _blogService.DeleteBlogAsync(id);
            TempData["SuccessMessage"] = "Blog yazısı başarıyla silindi.";
            return RedirectToAction(nameof(AdminIndex));
        }
    }
}
