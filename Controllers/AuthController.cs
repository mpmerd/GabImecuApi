using GabImecuApi.Data;
using GabImecuApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GabImecuApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly BdGabineteContext _db;
    private readonly string _connectionString;

    public AuthController(BdGabineteContext db, IConfiguration config)
    {
        _db = db;
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionString no configurado");
    }

    /// <summary>
    /// Valida credenciales de usuario y registra el acceso
    /// usando el stored procedure sp_RegistrarAccesoLogin.
    /// Devuelve: Success, Usuario, EscrituraYLectura, Mensaje.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Usuario)
            || string.IsNullOrWhiteSpace(request.Contraseña))
        {
            return Ok(new LoginResponse
            {
                Success = false,
                Mensaje = "Usuario y contraseña son requeridos"
            });
        }

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("sp_RegistrarAccesoLogin", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@Usuario", request.Usuario);
            command.Parameters.AddWithValue("@Contraseña", request.Contraseña);
            command.Parameters.AddWithValue("@Origen", request.Origen ?? "API");

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                int resultado = reader.GetInt32(0);
                string mensaje = reader.GetString(1);
                bool escriturayLectura = reader.GetBoolean(2);

                return Ok(new LoginResponse
                {
                    Success = resultado == 1,
                    Usuario = resultado == 1 ? request.Usuario : null,
                    EscrituraYLectura = escriturayLectura,
                    Mensaje = mensaje
                });
            }

            return Ok(new LoginResponse
            {
                Success = false,
                Mensaje = "Error al procesar la respuesta del servidor"
            });
        }
        catch (SqlException sqlEx)
        {
            return Ok(new LoginResponse
            {
                Success = false,
                Mensaje = $"Error de conexión a la base de datos: {sqlEx.Message}"
            });
        }
        catch (Exception ex)
        {
            return Ok(new LoginResponse
            {
                Success = false,
                Mensaje = $"Error inesperado: {ex.Message}"
            });
        }
    }
}

public class LoginRequest
{
    public string Usuario { get; set; } = string.Empty;
    public string Contraseña { get; set; } = string.Empty;
    public string? Origen { get; set; }
}
