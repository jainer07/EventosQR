using Microsoft.AspNetCore.Mvc;

namespace eventos_qr.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
