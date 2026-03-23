using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FacturacionHN.Data;
using FacturacionHN.DTOs;
using FacturacionHN.Models;

namespace FacturacionHN.Controllers;

[ApiController]
[Route("api/empresas/{empresaId}/cais")]
public class CAIsController : ControllerBase
{
    private readonly FacturacionDbContext _db;
    public CAIsController(FacturacionDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<CAIDto>>> GetAll(int empresaId)
    {
        var cais = await _db.CAIs
            .Where(c => c.EmpresaId == empresaId)
            .Select(c => new CAIDto(c.Id, c.EmpresaId, c.NumeroCai, c.RangoInicial, c.RangoFinal, c.CorrelativoActual,
                c.FechaLimiteEmision, c.SucursalCodigo, c.PuntoEmisionCodigo, c.TipoDocumento, c.Activo))
            .ToListAsync();
        return Ok(cais);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CAIDto>> GetById(int empresaId, int id)
    {
        var c = await _db.CAIs.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);
        if (c is null) return NotFound();
        return Ok(new CAIDto(c.Id, c.EmpresaId, c.NumeroCai, c.RangoInicial, c.RangoFinal, c.CorrelativoActual,
            c.FechaLimiteEmision, c.SucursalCodigo, c.PuntoEmisionCodigo, c.TipoDocumento, c.Activo));
    }

    [HttpPost]
    public async Task<ActionResult<CAIDto>> Create(int empresaId, CrearCAIDto dto)
    {
        if (dto.EmpresaId != empresaId) return BadRequest(new { error = "EmpresaId no coincide con la ruta." });

        var cai = new CAI
        {
            EmpresaId = empresaId, NumeroCai = dto.NumeroCai, RangoInicial = dto.RangoInicial,
            RangoFinal = dto.RangoFinal, FechaLimiteEmision = dto.FechaLimiteEmision,
            SucursalCodigo = dto.SucursalCodigo, PuntoEmisionCodigo = dto.PuntoEmisionCodigo,
            TipoDocumento = dto.TipoDocumento
        };
        _db.CAIs.Add(cai);
        await _db.SaveChangesAsync();
        var result = new CAIDto(cai.Id, cai.EmpresaId, cai.NumeroCai, cai.RangoInicial, cai.RangoFinal, cai.CorrelativoActual,
            cai.FechaLimiteEmision, cai.SucursalCodigo, cai.PuntoEmisionCodigo, cai.TipoDocumento, cai.Activo);
        return CreatedAtAction(nameof(GetById), new { empresaId, id = cai.Id }, result);
    }

    [HttpPut("{id}/desactivar")]
    public async Task<IActionResult> Desactivar(int empresaId, int id)
    {
        var c = await _db.CAIs.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);
        if (c is null) return NotFound();
        c.Activo = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
