using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebConsultasMedicas.Data;

namespace WebConsultasMedicas.Hubs;

[Authorize]
public class CitasHub : Hub
{
    private readonly ApplicationDbContext _context;

    public CitasHub(ApplicationDbContext context)
    {
        _context = context;
    }

    public override async Task OnConnectedAsync()
    {
        if (Context.User?.IsInRole("Medico") == true)
        {
            var userIdClaim = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var idUsuario))
            {
                var idMedico = await _context.Medicos.AsNoTracking()
                    .Where(m => m.IdUsuario == idUsuario)
                    .Select(m => m.IdMedico)
                    .FirstOrDefaultAsync();

                if (idMedico > 0)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, GroupForMedico(idMedico));
                }
            }
        }

        await base.OnConnectedAsync();
    }

    public static string GroupForMedico(int idMedico) => $"medico:{idMedico}";
}

