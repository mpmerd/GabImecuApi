using GabImecuApi.Data;
using GabImecuApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GabImecuApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsultasController : ControllerBase
{
    private readonly BdGabineteContext _db;

    public ConsultasController(BdGabineteContext db) => _db = db;

    /// <summary>Consultas generales: pastores con su iglesia, filtrable</summary>
    [HttpGet("pastores-iglesias")]
    public async Task<ActionResult<List<PastorIglesiaDto>>> GetPastoresIglesias(
        [FromQuery] string? categoria = null,
        [FromQuery] string? distrito = null)
    {
        var query = from i in _db.TIglesia
                    join p in _db.TPastors on i.IdIglesia equals p.IdIglesia
                    orderby i.Distrito, i.Nombre
                    select new PastorIglesiaDto
                    {
                        Distrito = i.Distrito,
                        Iglesia = i.Nombre,
                        Direccion = i.Direccion,
                        Telefono = i.Telefono,
                        Categoria = p.Categoria,
                        Pastor = p.Nombre,
                        Celular = p.Celular
                    };

        if (!string.IsNullOrEmpty(categoria))
            query = query.Where(x => x.Categoria == categoria);
        if (!string.IsNullOrEmpty(distrito))
            query = query.Where(x => x.Distrito == distrito);

        return Ok(await query.ToListAsync());
    }

    /// <summary>Miembros del Gabinete (Obispos + Superintendentes)</summary>
    [HttpGet("gabinete")]
    public async Task<ActionResult<List<PastorReporteDto>>> GetGabinete()
    {
        var obispos = from p in _db.TPastors
                      join i in _db.TIglesia on p.IdIglesia equals i.IdIglesia
                      where p.Categoria == "Obispo"
                      select new PastorReporteDto
                      {
                          Distrito = i.Distrito,
                          Iglesia = i.Nombre,
                          Categoria = p.Categoria,
                          Pastor = p.Nombre
                      };

        var supers = from p in _db.TPastors
                     join i in _db.TIglesia on p.IdIglesia equals i.IdIglesia
                     join s in _db.TSuperintendentes on p.IdPastor equals s.IdPastoral
                     select new PastorReporteDto
                     {
                         Distrito = i.Distrito,
                         Iglesia = i.Nombre,
                         Categoria = p.Categoria,
                         Pastor = p.Nombre
                     };

        var result = await obispos.Union(supers)
            .OrderBy(x => x.Categoria)
            .ThenBy(x => x.Distrito)
            .ToListAsync();

        return Ok(result);
    }

    /// <summary>Nombramientos oficiales (todos los pastores con iglesia, distrito y categoria)</summary>
    [HttpGet("nombramientos-oficiales")]
    public async Task<ActionResult<List<PastorReporteDto>>> GetNombramientosOficiales()
    {
        var result = await (from i in _db.TIglesia
                            join d in _db.TDistritos on i.Distrito equals d.Distrito
                            join p in _db.TPastors on i.IdIglesia equals p.IdIglesia
                            join c in _db.TCategoria on p.Categoria equals c.Categoria into pc
                            from c in pc.DefaultIfEmpty()
                            orderby i.Distrito, i.Nombre, p.Nombre
                            select new PastorReporteDto
                            {
                                Distrito = i.Distrito,
                                Iglesia = i.Nombre,
                                Categoria = p.Categoria ?? string.Empty,
                                Pastor = p.Nombre
                            }).ToListAsync();

        return Ok(result);
    }

    /// <summary>Pastores sin planilla (sin registro en T_familia)</summary>
    [HttpGet("sin-planillas")]
    public async Task<ActionResult<List<PastorReporteDto>>> GetSinPlanillas()
    {
        var subquery = _db.TFamilia.Select(f => f.IdPastor).Distinct();

        var result = await (from i in _db.TIglesia
                            join p in _db.TPastors on i.IdIglesia equals p.IdIglesia
                            where !subquery.Contains(p.IdPastor)
                                  && p.Categoria != null && p.Categoria != ""
                            orderby i.Distrito, i.Nombre
                            select new PastorReporteDto
                            {
                                Distrito = i.Distrito,
                                Iglesia = i.Nombre,
                                Categoria = p.Categoria,
                                Pastor = p.Nombre
                            }).ToListAsync();

        return Ok(result);
    }
}
