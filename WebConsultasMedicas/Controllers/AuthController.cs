using Microsoft.AspNetCore.Mvc;

namespace WebConsultasMedicas.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string emailAddress, string password)
        {
            return RedirectToAction("Dashboard", "Admin");
        }
    }
}
