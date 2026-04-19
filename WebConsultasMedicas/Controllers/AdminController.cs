using Microsoft.AspNetCore.Mvc;

namespace WebConsultasMedicas.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Inicio()
        {
            ViewData["Title"] = "Inicio";
            return View();
        }

        public IActionResult Dashboard()
        {
            return RedirectToAction(nameof(Inicio));
        }
    }
}
