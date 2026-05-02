using GabImecuApi.Data;
using GabImecuApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GabImecuApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogosController : ControllerBase
{
    private readonly BdGabineteContext _db;

    public CatalogosController(BdGabineteContext db) => _db = db;

    [HttpGet("categorias")]
    public async Task<ActionResult<List<string>>> GetCategorias()
    {
        var categorias = await _db.TPastors
            .Select(p => p.Categoria)
            .Where(c => c != null)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
        return Ok(categorias);
    }

    [HttpGet("distritos")]
    public async Task<ActionResult<List<string>>> GetDistritos()
    {
        var distritos = await _db.TIglesia
            .Select(i => i.Distrito)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
        return Ok(distritos);
    }

    [HttpGet("pastores-nombres")]
    public async Task<ActionResult<List<string>>> GetPastoresNombres()
    {
        var pastores = await _db.TPastors
            .Select(p => p.Nombre)
            .OrderBy(n => n)
            .ToListAsync();
        return Ok(pastores);
    }

    [HttpGet("iglesias-nombres")]
    public async Task<ActionResult<List<string>>> GetIglesiasNombres()
    {
        var iglesias = await _db.TIglesia
            .Select(i => i.Nombre)
            .OrderBy(n => n)
            .ToListAsync();
        return Ok(iglesias);
    }
}
