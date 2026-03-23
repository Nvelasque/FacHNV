using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FacturacionHN.Data;
using FacturacionHN.DTOs;
using FacturacionHN.Models;

namespace FacturacionHN.Controllers;

[ApiController]
[Route("api/empresas/{empresaId}/productos")]
public class ProductosController : ControllerBase
{
    private readonly FacturacionDbContext _db;
    public ProductosController(FacturacionDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<ProductoDto>>> GetAll(int empresaId)
    {
        var productos = await _db.Productos
            .Where(p => p.EmpresaId == empresaId)
            .Select(p => new ProductoDto(p.Id, p.EmpresaId, p.Codigo, p.Descripcion, p.Precio, p.GravadoISV, p.Activo))
            .ToListAsync();
        return Ok(productos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductoDto>> GetById(int empresaId, int id)
    {
        var p = await _db.Productos.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);
        if (p is null) return NotFound();
        return Ok(new ProductoDto(p.Id, p.EmpresaId, p.Codigo, p.Descripcion, p.Precio, p.GravadoISV, p.Activo));
    }

    [HttpPost]
    public async Task<ActionResult<ProductoDto>> Create(int empresaId, CrearProductoDto dto)
    {
        if (dto.EmpresaId != empresaId) return BadRequest(new { error = "EmpresaId no coincide con la ruta." });

        var producto = new Producto
        {
            EmpresaId = empresaId, Codigo = dto.Codigo, Descripcion = dto.Descripcion,
            Precio = dto.Precio, GravadoISV = dto.GravadoISV
        };
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();
        var result = new ProductoDto(producto.Id, producto.EmpresaId, producto.Codigo, producto.Descripcion, producto.Precio, producto.GravadoISV, producto.Activo);
        return CreatedAtAction(nameof(GetById), new { empresaId, id = producto.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductoDto>> Update(int empresaId, int id, ActualizarProductoDto dto)
    {
        var p = await _db.Productos.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);
        if (p is null) return NotFound();
        p.Descripcion = dto.Descripcion; p.Precio = dto.Precio; p.GravadoISV = dto.GravadoISV; p.Activo = dto.Activo;
        await _db.SaveChangesAsync();
        return Ok(new ProductoDto(p.Id, p.EmpresaId, p.Codigo, p.Descripcion, p.Precio, p.GravadoISV, p.Activo));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int empresaId, int id)
    {
        var p = await _db.Productos.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);
        if (p is null) return NotFound();
        p.Activo = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
