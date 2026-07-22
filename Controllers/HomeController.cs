using Microsoft.AspNetCore.Mvc;

namespace otelrezervation.Controllers;

public class HomeController : Controller
{
    // API'lerimizde 'IActionResult' OK dondururken MVC'de view dondurur.
    public IActionResult Index()
    {
        return View();
    }
}
