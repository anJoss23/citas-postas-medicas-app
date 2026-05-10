using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models;
using WebConsultasMedicas.Security;

namespace WebConsultasMedicas.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Paciente"))
                {
                    return RedirectToAction("Buscar", "Portal");
                }

                if (User.IsInRole("Medico"))
                {
                    return RedirectToAction("MisCitas", "MedicoPortal");
                }

                return RedirectToAction("Inicio", "Admin");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string emailAddress, string password, bool rememberSession)
        {
            if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Ingresa correo y contraseña.";
                return View();
            }

            var email = emailAddress.Trim().ToLowerInvariant();
            var passwordHash = PasswordHasher.Sha256Hex(password);

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Correo.ToLower() == email);

            if (usuario is null || !usuario.Estado || usuario.ClaveHash != passwordHash)
            {
                TempData["Error"] = "Credenciales inválidas.";
                return View();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, usuario.Correo),
                new(ClaimTypes.Email, usuario.Correo),
                new(ClaimTypes.Role, usuario.Rol.Nombre)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = rememberSession,
                    AllowRefresh = true,
                    IssuedUtc = DateTimeOffset.UtcNow,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            if (string.Equals(usuario.Rol.Nombre, "Paciente", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Buscar", "Portal");
            }

            if (string.Equals(usuario.Rol.Nombre, "Medico", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("MisCitas", "MedicoPortal");
            }

            return RedirectToAction("Inicio", "Admin");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        public IActionResult Denied()
        {
            ViewData["Title"] = "Acceso denegado";
            return View();
        }

        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Buscar", "Portal");
            }
            ViewData["Title"] = "Registro de paciente";
            return View(new PatientRegisterViewModel
            {
                FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18))
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(PatientRegisterViewModel model)
        {
            ViewData["Title"] = "Registro de paciente";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = model.Correo.Trim().ToLowerInvariant();
            var existsEmail = await _context.Usuarios.AnyAsync(u => u.Correo.ToLower() == email);
            if (existsEmail)
            {
                ModelState.AddModelError(nameof(PatientRegisterViewModel.Correo), "El correo ya está registrado.");
                return View(model);
            }

            var existsDni = await _context.Pacientes.AnyAsync(p => p.DNI == model.DNI);
            if (existsDni)
            {
                ModelState.AddModelError(nameof(PatientRegisterViewModel.DNI), "El DNI ya está registrado.");
                return View(model);
            }

            var existsSis = await _context.Pacientes.AnyAsync(p => p.NumeroSIS == model.NumeroSIS);
            if (existsSis)
            {
                ModelState.AddModelError(nameof(PatientRegisterViewModel.NumeroSIS), "El número SIS ya está registrado.");
                return View(model);
            }

            var pacienteRolId = await _context.Roles
                .Where(r => r.Nombre == "Paciente")
                .Select(r => r.IdRol)
                .FirstOrDefaultAsync();

            if (pacienteRolId == 0)
            {
                TempData["Error"] = "No existe el rol Paciente en la base de datos.";
                return View(model);
            }

            var usuario = new Usuario
            {
                Correo = email,
                ClaveHash = PasswordHasher.Sha256Hex(model.Password),
                IdRol = pacienteRolId,
                Estado = true,
                FechaRegistro = DateTime.Now
            };

            var paciente = new Paciente
            {
                Usuario = usuario,
                DNI = model.DNI,
                Nombres = model.Nombres,
                ApellidoPaterno = model.ApellidoPaterno,
                ApellidoMaterno = model.ApellidoMaterno,
                FechaNacimiento = model.FechaNacimiento,
                Sexo = model.Sexo,
                Telefono = model.Telefono,
                Direccion = model.Direccion,
                NumeroSIS = model.NumeroSIS,
                Estado = true
            };

            _context.Pacientes.Add(paciente);
            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Registro exitoso. Ya puedes iniciar sesión.";
                return RedirectToAction(nameof(Login));
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "No se pudo completar el registro. Verifica que el correo/DNI/SIS no estén duplicados.";
                return View(model);
            }
        }
    }
}
