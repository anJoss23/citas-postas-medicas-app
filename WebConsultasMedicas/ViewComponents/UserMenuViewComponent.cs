using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebConsultasMedicas.Data;

namespace WebConsultasMedicas.ViewComponents;

public class UserMenuViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public UserMenuViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var principal = HttpContext.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return View(new UserMenuVm { DisplayName = "Usuario", DisplayRole = "Invitado" });
        }

        var role = principal.FindFirstValue(ClaimTypes.Role) ?? "Usuario";
        var displayRole = role switch
        {
            "Administrador" => "Administrador",
            "Paciente" => "Paciente",
            "Medico" => "Médico",
            _ => role
        };

        var displayName = principal.Identity?.Name ?? "Usuario";

        var idClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(idClaim, out var idUsuario))
        {
            if (role == "Paciente")
            {
                var p = await _context.Pacientes.AsNoTracking()
                    .Where(x => x.IdUsuario == idUsuario)
                    .Select(x => new { x.Nombres, x.ApellidoPaterno, x.ApellidoMaterno })
                    .FirstOrDefaultAsync();
                if (p is not null)
                {
                    displayName = $"{p.ApellidoPaterno} {p.ApellidoMaterno}, {p.Nombres}";
                }
            }
            else if (role == "Medico")
            {
                var m = await _context.Medicos.AsNoTracking()
                    .Where(x => x.IdUsuario == idUsuario)
                    .Select(x => new { x.Nombres, x.ApellidoPaterno, x.ApellidoMaterno })
                    .FirstOrDefaultAsync();
                if (m is not null)
                {
                    displayName = $"{m.ApellidoPaterno} {m.ApellidoMaterno}, {m.Nombres}";
                }
            }
        }

        return View(new UserMenuVm { DisplayName = displayName, DisplayRole = displayRole });
    }
}

public class UserMenuVm
{
    public string DisplayName { get; set; } = "Usuario";
    public string DisplayRole { get; set; } = "Usuario";
}

