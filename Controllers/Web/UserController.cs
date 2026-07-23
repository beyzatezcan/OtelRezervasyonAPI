using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using otelrezervation.Services; 
using otelrezervation.DTOs; 

namespace otelrezervation.Controllers.Web;

// API'deki UsersController ile karismamasi icin adini UserController yaptik.
// Bu controller JSON degil, dogrudan HTML sayfasini (View) donecek.
[Authorize(Roles = "Admin")]
public class UserController : Controller
{
    private readonly IUserService _userService;


    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // Musteriler sayfasına girildiginde calisacak metod
    public async Task<IActionResult> Index()
    {
        // 1. servisten tum musterileri iste 
        var users = await _userService.GetAllUsersAsync();

        // 2. gelen musteri listesini HTML sayfasına (View'a) gonder
        return View(users);
    }

    // Kullanici ekleme (CREATE)
    // 1. Kullaniciya bos formu gostermek icin (Sadece HTML sayfasini açar)
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // 2. Kullanicinin doldurdugu formu alip veritabanina kaydetmek icin
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        // Eger kullanici formda eksik/hatali bilgi girerse (Email formati yanlisi gibi)
        if (!ModelState.IsValid)
            return View(dto); 

        await _userService.CreateUserAsync(dto);
        TempData["SuccessMessage"] = "Müşteri başarıyla eklendi.";
        return RedirectToAction("Index"); 
    }

    // MUSteri duzenleme (EDIT)
    // 1. Kullanicinin eski bilgilerini bulup formun icini doldurmak icin
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Düzenlenecek müşteri bulunamadı.";
            return RedirectToAction("Index");
        }

        // Musterinin eski bilgilerini Update formuna aktaralim
        var updateDto = new UpdateUserDto
        {
            Ad = user.Ad,
            Soyad = user.Soyad,
            Email = user.Email,
            Telefon = user.Telefon
        };

        // Form gonderilirken ID'ye ihtiyacimiz olacak, o yuzden ID'yi hafizaya (ViewBag) atiyoruz
        ViewBag.UserId = user.Id;
        return View(updateDto);
    }

    // 2. Formdaki guncel bilgileri kaydetmek icin
    [HttpPost]
    public async Task<IActionResult> Edit(int id, UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.UserId = id; // Hata durumunda formu geri basarken ID'yi unutmamak icin
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

    //  Kullanici silme islemi
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);
            if (result)
            {
                // İslem basariliysa TempData ile View'a mesaj gönderiyoruz
                // TempData -bir sonraki request boyunca gecerli olan degisken
                // kullanici sayfayi yenilese bile mesaj gorunur    
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
