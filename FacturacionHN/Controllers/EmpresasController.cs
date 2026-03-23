using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FacturacionHN.Data;
using FacturacionHN.DTOs;
using FacturacionHN.Models;

namespace FacturacionHN.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmpresasController : ControllerBase
{
    private readonly FacturacionDbContext _db;
    public EmpresasController(FacturacionDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<EmpresaDto>>> GetAll()
    {
        var empresas = await _db.Empresas
            .Select(e => new EmpresaDto(e.Id, e.RazonSocial, e.RTN, e.NombreComercial,
                e.DireccionCasaMatriz, e.Telefono, e.Correo, e.Activo,
                e.ColorPrimario, e.ColorSecundario, e.LogoUrl))
            .ToListAsync();
        return Ok(empresas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmpresaDto>> GetById(int id)
    {
        var e = await _db.Empresas.FindAsync(id);
        if (e is null) return NotFound();
        return Ok(MapToDto(e));
    }

    [HttpPost]
    public async Task<ActionResult<EmpresaDto>> Create(CrearEmpresaDto dto)
    {
        var empresa = new Empresa
        {
            RazonSocial = dto.RazonSocial, RTN = dto.RTN,
            NombreComercial = dto.NombreComercial,
            DireccionCasaMatriz = dto.DireccionCasaMatriz,
            Telefono = dto.Telefono, Correo = dto.Correo,
            ColorPrimario = dto.ColorPrimario ?? "#1B5E20",
            ColorSecundario = dto.ColorSecundario ?? "#E8F5E9",
            LogoUrl = dto.LogoUrl
        };
        _db.Empresas.Add(empresa);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = empresa.Id }, MapToDto(empresa));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EmpresaDto>> Update(int id, CrearEmpresaDto dto)
    {
        var empresa = await _db.Empresas.FindAsync(id);
        if (empresa is null) return NotFound();
        empresa.RazonSocial = dto.RazonSocial;
        empresa.RTN = dto.RTN;
        empresa.NombreComercial = dto.NombreComercial;
        empresa.DireccionCasaMatriz = dto.DireccionCasaMatriz;
        empresa.Telefono = dto.Telefono;
        empresa.Correo = dto.Correo;
        empresa.ColorPrimario = dto.ColorPrimario ?? empresa.ColorPrimario;
        empresa.ColorSecundario = dto.ColorSecundario ?? empresa.ColorSecundario;
        empresa.LogoUrl = dto.LogoUrl;
        await _db.SaveChangesAsync();
        return Ok(MapToDto(empresa));
    }

    private static EmpresaDto MapToDto(Empresa e) => new(
        e.Id, e.RazonSocial, e.RTN, e.NombreComercial,
        e.DireccionCasaMatriz, e.Telefono, e.Correo, e.Activo,
        e.ColorPrimario, e.ColorSecundario, e.LogoUrl);
}
