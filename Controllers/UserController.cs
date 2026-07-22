using Microsoft.AspNetCore.Mvc;
using otelrezervation.Services; 
using otelrezervation.DTOs; 

namespace otelrezervation.Controllers;

// DİKKAT: API'deki UsersController ile karışmaması için adını UserController yaptık.
// Bu controller JSON değil, doğrudan HTML sayfası (View) dönecek.
public class UserController : Controller
{
    private readonly IUserService _userService;

    // Mutfağı (Service) garsona veriyoruz
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // Müşteriler sayfasına girildiğinde çalışacak metod
    public async Task<IActionResult> Index()
    {
        // 1. Mutfaktan tüm müşterileri iste (Bu metodu önceden sen yazmıştın!)
        var users = await _userService.GetAllUsersAsync();

        // 2. Gelen müşteri listesini HTML sayfasına (View'a) gönder
        return View(users);
    }

    // --- YENİ MÜŞTERİ EKLEME (CREATE) ---
    // 1. Kullanıcıya boş formu göstermek için (Sadece HTML sayfasını açar)
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // 2. Kullanıcının doldurduğu formu alıp veritabanına kaydetmek için
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        // Eğer kullanıcı formda eksik/hatalı bilgi girerse (Email formatı yanlışı gibi)
        if (!ModelState.IsValid)
            return View(dto); // Hatalı kısımları formda kalsın diye geri döndür

        await _userService.CreateUserAsync(dto);
        TempData["SuccessMessage"] = "Müşteri başarıyla eklendi.";
        return RedirectToAction("Index"); // İşlem bitince listeye geri dön
    }

    // --- MÜŞTERİ DÜZENLEME (EDIT) ---
    // 1. Kullanıcının eski bilgilerini bulup formun içini doldurmak için
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Düzenlenecek müşteri bulunamadı.";
            return RedirectToAction("Index");
        }

        // Müşterinin eski bilgilerini Update formuna aktaralım
        var updateDto = new UpdateUserDto
        {
            Ad = user.Ad,
            Soyad = user.Soyad,
            Email = user.Email,
            Telefon = user.Telefon
        };

        // Form gönderilirken ID'ye ihtiyacımız olacak, o yüzden ID'yi hafızaya (ViewBag) atıyoruz
        ViewBag.UserId = user.Id;
        return View(updateDto);
    }

    // 2. Formdaki güncel bilgileri kaydetmek için
    [HttpPost]
    public async Task<IActionResult> Edit(int id, UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.UserId = id; // Hata durumunda formu geri basarken ID'yi unutmamak için
            return View(dto);
        }

        var updatedUser = await _userService.UpdateUserAsync(id, dto);
        if (updatedUser == null)
        {
            TempData["ErrorMessage"] = "Güncellenecek müşteri bulunamadı.";
        }
        else
        {
            TempData["SuccessMessage"] = "Müşteri başarıyla güncellendi.";
        }

        return RedirectToAction("Index");
    }

    // YENİ: Kullanıcı silme işlemi
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);
            if (result)
            {
                // İşlem başarılıysa TempData ile View'a mesaj gönderiyoruz
                TempData["SuccessMessage"] = "Müşteri başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Silinecek müşteri bulunamadı.";
            }
        }
        catch (InvalidOperationException ex)
        {
            // UserService'de yazdığımız "Aktif rezervasyonu var" hatasını yakalıyoruz
            TempData["ErrorMessage"] = ex.Message;
        }

        // İşlem bittikten sonra tekrar listeleme sayfasına (Index) geri dön
        return RedirectToAction("Index");
    }
}
