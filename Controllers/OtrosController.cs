using GabImecuApi.Data;
using GabImecuApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GabImecuApi.Controllers;

[ApiController]
[Route("[controller]")]
public class OtrosController : ControllerBase
{
    private readonly BdGabineteContext _db;

    public OtrosController(BdGabineteContext db) => _db = db;

    /// <summary>Organismos supeditados</summary>
    [HttpGet("supeditados")]
    public async Task<ActionResult<List<TSupeditado>>> GetSupeditados()
    {
        var result = await _db.TSupeditados
            .OrderByDescending(x => x.Organismo)
            .ToListAsync();
        return Ok(result);
    }

    /// <summary>Pastores fuera del sistema</summary>
    [HttpGet("fuera-sistema")]
    public async Task<ActionResult<List<TOutside>>> GetFueraSistema()
    {
        var result = await _db.TOutsides
            .OrderByDescending(x => x.FechSalida)
            .ToListAsync();
        return Ok(result);
    }

    /// <summary>Pastores por fecha de ordenación</summary>
    [HttpGet("por-fecha-ordenacion")]
    public async Task<ActionResult<List<PastorReporteConFechaDto>>> GetPorFechaOrdenacion(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta)
    {
        var desdeDate = DateOnly.FromDateTime(desde);
        var hastaDate = DateOnly.FromDateTime(hasta);
        var result = await (from o in _db.TOrdenacions
                            join p in _db.TPastors on o.IdPastor equals p.IdPastor
                            join i in _db.TIglesia on p.IdIglesia equals i.IdIglesia
                            where o.Fecha >= desdeDate && o.Fecha <= hastaDate
                            orderby i.Distrito, o.Fecha
                            select new PastorReporteConFechaDto
                            {
                                Distrito = i.Distrito,
                                Iglesia = i.Nombre,
                                Categoria = p.Categoria ?? string.Empty,
                                Pastor = p.Nombre,
                                Fecha = o.Fecha
                            }).ToListAsync();

        return Ok(result);
    }

    /// <summary>Pastores por fecha de nombramiento (inicio en historial)</summary>
    [HttpGet("por-fecha-nombramiento")]
    public async Task<ActionResult<List<PastorReporteConFechaDto>>> GetPorFechaNombramiento(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta)
    {
        var desdeDate = DateOnly.FromDateTime(desde);
        var hastaDate = DateOnly.FromDateTime(hasta);
        var result = await (from h in _db.THistorials
                            join p in _db.TPastors on h.IdPastor equals p.IdPastor
                            join i in _db.TIglesia on p.IdIglesia equals i.IdIglesia
                            where h.FechInic >= desdeDate && h.FechInic <= hastaDate
                            orderby i.Distrito, i.Nombre, h.FechInic
                            select new PastorReporteConFechaDto
                            {
                                Distrito = i.Distrito,
                                Iglesia = i.Nombre,
                                Categoria = p.Categoria ?? string.Empty,
                                Pastor = p.Nombre,
                                Fecha = h.FechInic
                            }).ToListAsync();

        return Ok(result);
    }
}
