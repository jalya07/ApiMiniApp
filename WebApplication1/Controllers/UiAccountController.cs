using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

public class UiAccountController : Controller
{
    // GET
    public IActionResult Register()
    {
        return View();
    }
}