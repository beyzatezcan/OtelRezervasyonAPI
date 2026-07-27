using Microsoft.AspNetCore.Mvc;
using otelrezervation.Models;
using otelrezervation.Services;
using System.Threading.Tasks;

namespace otelrezervation.Controllers.Web
{
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public ContactController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: /Contact
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Contact/Send
        [HttpPost]
        public async Task<IActionResult> Send(ContactMessage model)
        {
            if (ModelState.IsValid)
            {
                // 1. Veritabanına kaydet
                _context.ContactMessages.Add(model);
                await _context.SaveChangesAsync();

                // 2. Mail at (Otelin mail adresine bildirim)
                string body = $"<p><strong>Gönderen:</strong> {model.AdSoyad}</p>" +
                              $"<p><strong>E-Posta:</strong> {model.Email}</p>" +
                              $"<p><strong>Tarih:</strong> {model.GonderilmeTarihi}</p>" +
                              $"<hr/><p>{model.Mesaj}</p>";
                              
                // "info@luminaresort.com" kismi otelin yonetim maili olarak dusunulebilir. 
                // SMTP baglaninca gercekten gidecek. 
                // Şimdilik sistem uzerinden kime gidecegini kendi ayarlari icindeki aliciya gonderecek sekilde kodlayabiliriz.
                // Ornegin test mailini alici olarak da yazabiliriz.
                
                await _emailService.SendEmailAsync("test@lumina.com", $"Yeni İletişim Mesajı - {model.AdSoyad}", body);

                TempData["SuccessMessage"] = "Mesajınız başarıyla iletildi. En kısa sürede size dönüş yapacağız.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Lütfen formdaki hataları düzeltip tekrar deneyin.";
            return View("Index", model);
        }
    }
}
