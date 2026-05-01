using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace WebConsultasMedicas.Controllers
{
    [Authorize(Roles = "Administrador")]
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
