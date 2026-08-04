using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using otelrezervation.Models;
using otelrezervation.Services;

namespace otelrezervation.Controllers.Web
{
    public class ContactController : Controller
    {
        private readonly IContactService _contactService;
        private readonly IEmailService _emailService;

        public ContactController(IContactService contactService, IEmailService emailService)
        {
            _contactService = contactService;
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
                // 1. Servis katmanı üzerinden veritabanına kaydet
                await _contactService.CreateMessageAsync(model);

                // 2. Mail at (Otelin mail adresine bildirim)
                string body = $"<p><strong>Gönderen:</strong> {model.AdSoyad}</p>" +
                              $"<p><strong>E-Posta:</strong> {model.Email}</p>" +
                              $"<p><strong>Tarih:</strong> {model.GonderilmeTarihi}</p>" +
                              $"<hr/><p>{model.Mesaj}</p>";
                
                await _emailService.SendEmailAsync("test@lumina.com", $"Yeni İletişim Mesajı - {model.AdSoyad}", body);

                TempData["SuccessMessage"] = "Mesajınız başarıyla iletildi. En kısa sürede size dönüş yapacağız.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Lütfen formdaki hataları düzeltip tekrar deneyin.";
            return View("Index", model);
        }

        // ADMIN: Gelen Mesajları Listeleme Sayfası
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Messages()
        {
            var mesajlar = await _contactService.GetAllMessagesAsync();
            return View(mesajlar);
        }

        // ADMIN: Mesaj silme
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            await _contactService.DeleteMessageAsync(id);
            TempData["SuccessMessage"] = "Mesaj başarıyla silindi.";
            return RedirectToAction("Messages");
        }
    }
}
