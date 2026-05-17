using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers;

public class AccountController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}