using GabImecuApi.Data;
using GabImecuApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GabImecuApi.Controllers;

[ApiController]
[Route("ficha-pastoral")]
public class FichaPastoralController : ControllerBase
{
    private readonly BdGabineteContext _db;

    public FichaPastoralController(BdGabineteContext db) => _db = db;

    /// <summary>Ficha pastoral por nombre de pastor</summary>
    [HttpGet("pastor/{nombre}")]
    public async Task<ActionResult<FichaPastoralResponse>> GetByPastor(string nombre)
    {
        var nombreUpper = nombre.ToUpper();

        var pastor = await _db.TPastors
            .FirstOrDefaultAsync(p => p.Nombre.ToUpper() == nombreUpper)
            ?? await _db.TPastors
            .FirstOrDefaultAsync(p => p.Nombre.ToUpper().Contains(nombreUpper));

        if (pastor == null)
            return NotFound(new { mensaje = "Pastor no encontrado" });

        var pastorId = pastor.IdPastor;
        var response = new FichaPastoralResponse();

        // Info de iglesia
        var iglesia = await _db.TIglesia.FirstOrDefaultAsync(i => i.IdIglesia == pastor.IdIglesia);
        var fechaOrdenacion = await _db.TOrdenacions
            .Where(o => o.IdPastor == pastorId)
            .Select(o => (DateOnly?)o.Fecha)
            .FirstOrDefaultAsync();

        response.IglesiaInfo = new PastorInfoIglesiaDto
        {
            Iglesia = iglesia?.Nombre ?? "",
            Direccion = iglesia?.Direccion ?? "",
            Telefono = iglesia?.Telefono ?? "",
            FechaOrdenacion = fechaOrdenacion.HasValue
                ? fechaOrdenacion.Value.ToDateTime(TimeOnly.MinValue)
                : null
        };

        // Info del pastor
        response.PastorInfo = new PastorInfoPastorDto
        {
            Categorias = new List<string?> { pastor.Categoria },
            Pastores = new List<string> { pastor.Nombre },
            Celulares = new List<string?> { pastor.Celular }
        };

        // Historial
        response.HistorialIglesias = await _db.THistorials
            .Where(h => h.IdPastor == pastorId)
            .Select(h => h.IgPastoreada)
            .ToListAsync();
        response.IniciosHistorial = await _db.THistorials
            .Where(h => h.IdPastor == pastorId)
            .Select(h => (DateTime?)h.FechInic.ToDateTime(TimeOnly.MinValue))
            .ToListAsync();

        // Familia
        response.Familiares = await _db.TFamilia
            .Where(f => f.IdPastor == pastorId)
            .Select(f => f.NombreyApell)
            .ToListAsync();
        response.Edades = await _db.TFamilia
            .Where(f => f.IdPastor == pastorId)
            .Select(f =>
                (f.FechNac.Month > DateTime.Now.Month ||
                 (f.FechNac.Month == DateTime.Now.Month && f.FechNac.Day > DateTime.Now.Day))
                ? DateTime.Now.Year - f.FechNac.Year - 1
                : DateTime.Now.Year - f.FechNac.Year)
            .ToListAsync();

        return Ok(response);
    }

    /// <summary>Ficha pastoral por nombre de iglesia</summary>
    [HttpGet("iglesia/{nombre}")]
    public async Task<ActionResult<FichaPastoralResponse>> GetByIglesia(string nombre)
    {
        var nombreUpper = nombre.ToUpper();

        var iglesia = await _db.TIglesia
            .FirstOrDefaultAsync(i => i.Nombre.ToUpper() == nombreUpper)
            ?? await _db.TIglesia
            .FirstOrDefaultAsync(i => i.Nombre.ToUpper().Contains(nombreUpper));

        if (iglesia == null)
            return NotFound(new { mensaje = "Iglesia no encontrada" });

        var pastores = await _db.TPastors
            .Where(p => p.IdIglesia == iglesia.IdIglesia)
            .ToListAsync();

        var pastorIds = pastores.Select(p => p.IdPastor).ToList();
        var response = new FichaPastoralResponse();

        // Info de iglesia con fecha de ordenación del primer pastor
        var fechaOrdenacion = pastorIds.Any()
            ? await _db.TOrdenacions
                .Where(o => pastorIds.Contains(o.IdPastor))
                .Select(o => (DateOnly?)o.Fecha)
                .FirstOrDefaultAsync()
            : null;

        response.IglesiaInfo = new PastorInfoIglesiaDto
        {
            Iglesia = iglesia.Nombre,
            Direccion = iglesia.Direccion,
            Telefono = iglesia.Telefono,
            FechaOrdenacion = fechaOrdenacion.HasValue
                ? fechaOrdenacion.Value.ToDateTime(TimeOnly.MinValue)
                : null
        };

        // Info de pastores
        response.PastorInfo = new PastorInfoPastorDto
        {
            Categorias = pastores.Select(p => p.Categoria).ToList(),
            Pastores = pastores.Select(p => p.Nombre).ToList(),
            Celulares = pastores.Select(p => p.Celular).ToList()
        };

        // Historial
        response.HistorialIglesias = await _db.THistorials
            .Where(h => pastorIds.Contains(h.IdPastor))
            .Select(h => h.IgPastoreada)
            .ToListAsync();
        response.IniciosHistorial = await _db.THistorials
            .Where(h => pastorIds.Contains(h.IdPastor))
            .Select(h => (DateTime?)h.FechInic.ToDateTime(TimeOnly.MinValue))
            .ToListAsync();

        // Familia
        response.Familiares = await _db.TFamilia
            .Where(f => pastorIds.Contains(f.IdPastor))
            .Select(f => f.NombreyApell)
            .ToListAsync();
        response.Edades = await _db.TFamilia
            .Where(f => pastorIds.Contains(f.IdPastor))
            .Select(f =>
                (f.FechNac.Month > DateTime.Now.Month ||
                 (f.FechNac.Month == DateTime.Now.Month && f.FechNac.Day > DateTime.Now.Day))
                ? DateTime.Now.Year - f.FechNac.Year - 1
                : DateTime.Now.Year - f.FechNac.Year)
            .ToListAsync();

        return Ok(response);
    }
}
