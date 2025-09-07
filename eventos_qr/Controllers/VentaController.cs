using Microsoft.AspNetCore.Mvc;

namespace eventos_qr.Controllers
{
    public class VentaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Crear()
        {
            return View();
        }
    }
}
