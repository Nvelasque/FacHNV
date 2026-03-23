using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FacturacionHN.Data;
using FacturacionHN.DTOs;
using FacturacionHN.Models;

namespace FacturacionHN.Controllers;

[ApiController]
[Route("api/empresas/{empresaId}/clientes")]
public class ClientesController : ControllerBase
{
    private readonly FacturacionDbContext _db;
    public ClientesController(FacturacionDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<ClienteDto>>> GetAll(int empresaId)
    {
        var clientes = await _db.Clientes
            .Where(c => c.EmpresaId == empresaId)
            .Select(c => new ClienteDto(c.Id, c.EmpresaId, c.RTN, c.Nombre, c.Direccion, c.Correo, c.Telefono, c.Activo))
            .ToListAsync();
        return Ok(clientes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClienteDto>> GetById(int empresaId, int id)
    {
        var c = await _db.Clientes.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);
        if (c is null) return NotFound();
        return Ok(new ClienteDto(c.Id, c.EmpresaId, c.RTN, c.Nombre, c.Direccion, c.Correo, c.Telefono, c.Activo));
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Create(int empresaId, CrearClienteDto dto)
    {
        if (dto.EmpresaId != empresaId) return BadRequest(new { error = "EmpresaId no coincide con la ruta." });

        var cliente = new Cliente
        {
            EmpresaId = empresaId, RTN = dto.RTN, Nombre = dto.Nombre,
            Direccion = dto.Direccion, Correo = dto.Correo, Telefono = dto.Telefono
        };
        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();
        var result = new ClienteDto(cliente.Id, cliente.EmpresaId, cliente.RTN, cliente.Nombre, cliente.Direccion, cliente.Correo, cliente.Telefono, cliente.Activo);
        return CreatedAtAction(nameof(GetById), new { empresaId, id = cliente.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ClienteDto>> Update(int empresaId, int id, ActualizarClienteDto dto)
    {
        var c = await _db.Clientes.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);
        if (c is null) return NotFound();
        c.Nombre = dto.Nombre; c.Direccion = dto.Direccion; c.Correo = dto.Correo; c.Telefono = dto.Telefono; c.Activo = dto.Activo;
        await _db.SaveChangesAsync();
        return Ok(new ClienteDto(c.Id, c.EmpresaId, c.RTN, c.Nombre, c.Direccion, c.Correo, c.Telefono, c.Activo));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int empresaId, int id)
    {
        var c = await _db.Clientes.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);
        if (c is null) return NotFound();
        c.Activo = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
