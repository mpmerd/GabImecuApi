using GabImecuApi.Data;
using GabImecuApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GabImecuApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly BdGabineteContext _db;

    public AuthController(BdGabineteContext db) => _db = db;

    /// <summary>Validar credenciales de usuario</summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _db.TLogins
            .FirstOrDefaultAsync(u => u.Usuario == request.Usuario
                                   && u.Contraseña == request.Contraseña);

        if (user == null)
            return Ok(new LoginResponse
            {
                Success = false,
                Mensaje = "Usuario o contraseña incorrectos"
            });

        return Ok(new LoginResponse
        {
            Success = true,
            Usuario = user.Usuario,
            EscrituraYLectura = user.EscrituraYLectura,
            Mensaje = "Autenticación exitosa"
        });
    }
}

public class LoginRequest
{
    public string Usuario { get; set; } = string.Empty;
    public string Contraseña { get; set; } = string.Empty;
}
