namespace GabImecuApi.Models;

// ====== ENTIDADES (mapean 1:1 con tablas) ======

public class TPastor
{
    public int IdPastor { get; set; }
    public int? IdIglesia { get; set; }
    public string? Categoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Celular { get; set; }
}

public class TIglesia
{
    public int IdIglesia { get; set; }
    public string Distrito { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
}

public class TDistrito
{
    public int IdDistrito { get; set; }
    public string Distrito { get; set; } = string.Empty;
}

public class TCategoria
{
    public int IdCategoria { get; set; }
    public string Categoria { get; set; } = string.Empty;
}

public class TFamilia
{
    public int IdFamilia { get; set; }
    public int IdPastor { get; set; }
    public string NombreyApell { get; set; } = string.Empty;
    public DateTime FechNac { get; set; }
}

public class THistorial
{
    public int IdHistorial { get; set; }
    public int IdPastor { get; set; }
    public string IgPastoreada { get; set; } = string.Empty;
    public DateOnly FechInic { get; set; }
}

public class TOrdenacion
{
    public int IdOrdenacion { get; set; }
    public int IdPastor { get; set; }
    public DateOnly Fecha { get; set; }
}

public class TObispo
{
    public int IdObispo { get; set; }
    public int IdDist { get; set; }
    public int IdPastor { get; set; }
}

public class TSuperintendente
{
    public int IdSuperintendente { get; set; }
    public int IdDist { get; set; }
    public int IdPastoral { get; set; }
}

public class TOutside
{
    public string? UltimaIgl { get; set; }
    public string? Categoria { get; set; }
    public string NombreyApell { get; set; } = string.Empty;
    public string? Celular { get; set; }
    public string? Causa { get; set; }
    public DateTime? FechSalida { get; set; }
}

public class TSupeditado
{
    public int IdSupeditado { get; set; }
    public string? Organismo { get; set; }
    public string? Nombre { get; set; }
    public string? Categoria { get; set; }
}

public class TLogin
{
    public int IdUser { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Contraseña { get; set; } = string.Empty;
    public bool EscrituraYLectura { get; set; }
}

// ====== DTOs de respuesta ======

public class PastorIglesiaDto
{
    public string Distrito { get; set; } = string.Empty;
    public string Iglesia { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Categoria { get; set; }
    public string Pastor { get; set; } = string.Empty;
    public string? Celular { get; set; }
}

public class PastorReporteDto
{
    public string? Distrito { get; set; }
    public string? Iglesia { get; set; }
    public string? Categoria { get; set; }
    public string Pastor { get; set; } = string.Empty;
}

public class PastorReporteConFechaDto
{
    public string? Distrito { get; set; }
    public string? Iglesia { get; set; }
    public string? Categoria { get; set; }
    public string Pastor { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
}

public class FichaPastoralResponse
{
    public PastorInfoIglesiaDto? IglesiaInfo { get; set; }
    public PastorInfoPastorDto? PastorInfo { get; set; }
    public List<string> HistorialIglesias { get; set; } = new();
    public List<DateTime?> IniciosHistorial { get; set; } = new();
    public List<string> Familiares { get; set; } = new();
    public List<int> Edades { get; set; } = new();
}

public class PastorInfoIglesiaDto
{
    public string? Iglesia { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public DateTime? FechaOrdenacion { get; set; }
}

public class PastorInfoPastorDto
{
    public List<string?> Categorias { get; set; } = new();
    public List<string> Pastores { get; set; } = new();
    public List<string?> Celulares { get; set; } = new();
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string? Usuario { get; set; }
    public bool EscrituraYLectura { get; set; }
    public string? Mensaje { get; set; }
}

public class ConsultaIaRequest
{
    public string Sql { get; set; } = string.Empty;
}

// ====== Schema DTOs para MCP ======

public class SchemaResponse
{
    public List<SchemaTableDto> Tables { get; set; } = new();
}

public class SchemaTableDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<SchemaColumnDto> Columns { get; set; } = new();
    public List<SchemaForeignKeyDto> ForeignKeys { get; set; } = new();
}

public class SchemaColumnDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
}

public class SchemaForeignKeyDto
{
    public string ColumnName { get; set; } = string.Empty;
    public string ReferencedTable { get; set; } = string.Empty;
    public string ReferencedColumn { get; set; } = string.Empty;
}
