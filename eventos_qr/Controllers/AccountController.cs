using eventos_qr.Models;
using Microsoft.AspNetCore.Mvc;

namespace eventos_qr.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel login)
        {
            return RedirectToActionPermanent("Index", "Home");
        }
    }
}
