using Microsoft.AspNetCore.Mvc;

namespace otelrezervation.Controllers.Web;

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
