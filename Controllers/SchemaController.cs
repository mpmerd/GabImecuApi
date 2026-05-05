using GabImecuApi.Data;
using GabImecuApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GabImecuApi.Controllers;

[ApiController]
[Route("[controller]")]
public class SchemaController : ControllerBase
{
    private readonly BdGabineteContext _db;

    // Tablas y columnas sensibles que no se exponen
    private static readonly HashSet<string> TablasProhibidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "T_login", "T_logs"
    };

    private static readonly HashSet<string> ColumnasProhibidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "Contraseña", "Password", "Contrasena", "Clave", "Pass"
    };

    // Descripciones curadas por tabla
    private static readonly Dictionary<string, string> Descripciones = new(StringComparer.OrdinalIgnoreCase)
    {
        ["T_pastor"] = "Datos principales de cada pastor activo: nombre, categoría, iglesia asignada y celular. IdIglesia NULL significa que el pastor está inactivo.",
        ["T_iglesia"] = "Iglesias de la IMECU. Cada iglesia pertenece a un distrito (texto libre, no FK).",
        ["T_familia"] = "Familiares de los pastores (esposa e hijos). Cada registro se vincula a IdPastor.",
        ["T_historial"] = "Historial de iglesias pastoreadas por cada pastor junto con la fecha de inicio.",
        ["T_categoria"] = "Categorías válidas del ministerio: Misionero, PLC, PL, PS, PSA, PP, PI, Obispo.",
        ["T_distrito"] = "Distritos de la IMECU en Cuba. Cada distrito tiene un superintendente activo.",
        ["T_ordenacion"] = "Fecha en que cada pastor fue ordenado como PI (Presbítero Itinerante).",
        ["T_outside"] = "Pastores que han salido del sistema activo: última iglesia, causa y fecha de salida.",
        ["T_obispo"] = "Pastores que sirven en la iglesia del obispo (iglesia con múltiples pastores).",
        ["T_superintendente"] = "Superintendentes de distrito. IdDistrito es FK a T_distrito.",
        ["T_supeditados"] = "Personal de organismos supeditados a la IMECU (Seminario, Hogar de Ancianos, etc.).",
        ["T_cambioscategorias"] = "Historial de cambios de categoría de cada pastor."
    };

    public SchemaController(BdGabineteContext db) => _db = db;

    /// <summary>Devuelve el esquema de la BD (tablas, columnas, tipos y FKs) para uso del módulo MCP/IA.</summary>
    [HttpGet]
    public async Task<ActionResult<SchemaResponse>> GetSchema()
    {
        var connectionString = _db.Database.GetConnectionString();
        var response = new SchemaResponse();

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Obtener tablas (excluyendo las prohibidas)
        var tablesQuery = """
            SELECT TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
            ORDER BY TABLE_NAME
            """;

        var tableNames = new List<string>();
        using (var cmd = new SqlCommand(tablesQuery, connection))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var name = reader.GetString(0);
                if (!TablasProhibidas.Contains(name))
                    tableNames.Add(name);
            }
        }

        foreach (var tableName in tableNames)
        {
            var table = new SchemaTableDto
            {
                Name = tableName,
                Description = Descripciones.TryGetValue(tableName, out var desc) ? desc : string.Empty
            };

            // Columnas
            var columnsQuery = $"""
                SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = @table
                ORDER BY ORDINAL_POSITION
                """;

            using (var cmd = new SqlCommand(columnsQuery, connection))
            {
                cmd.Parameters.AddWithValue("@table", tableName);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var colName = reader.GetString(0);
                    if (ColumnasProhibidas.Contains(colName))
                        continue;

                    table.Columns.Add(new SchemaColumnDto
                    {
                        Name = colName,
                        Type = reader.GetString(1),
                        IsNullable = reader.GetString(2) == "YES"
                    });
                }
            }

            // Foreign Keys
            var fkQuery = """
                SELECT
                    kcu.COLUMN_NAME,
                    ccu.TABLE_NAME  AS ReferencedTable,
                    ccu.COLUMN_NAME AS ReferencedColumn
                FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
                    ON rc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
                JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu
                    ON rc.UNIQUE_CONSTRAINT_NAME = ccu.CONSTRAINT_NAME
                WHERE kcu.TABLE_NAME = @table
                """;

            using (var cmd = new SqlCommand(fkQuery, connection))
            {
                cmd.Parameters.AddWithValue("@table", tableName);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var referencedTable = reader.GetString(1);
                    if (TablasProhibidas.Contains(referencedTable))
                        continue;

                    table.ForeignKeys.Add(new SchemaForeignKeyDto
                    {
                        ColumnName = reader.GetString(0),
                        ReferencedTable = referencedTable,
                        ReferencedColumn = reader.GetString(2)
                    });
                }
            }

            response.Tables.Add(table);
        }

        return Ok(response);
    }
}
