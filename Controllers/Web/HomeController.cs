using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace otelrezervation.Controllers.Web;

[Authorize] // sadece login olanlar erisebilir
public class HomeController : Controller
{
    // API'lerimizde 'IActionResult' diyorduk ve Ok(veri) dönüyorduk.
    // MVC'de 'IActionResult' diyoruz ve View() (Görsel Sayfa) dönüyoruz.
    public IActionResult Index()
    {
        // Views/Home/Index.cshtml sayfasını açar
        return View();
    }
}
