using Microsoft.AspNetCore.Mvc;
using FacturacionHN.DTOs;
using FacturacionHN.Services;

namespace FacturacionHN.Controllers;

[ApiController]
[Route("api/empresas/{empresaId}/facturas")]
public class FacturasController : ControllerBase
{
    private readonly IFacturacionService _service;
    private readonly FacturaPdfService _pdfService;

    public FacturasController(IFacturacionService service, FacturaPdfService pdfService)
    {
        _service = service;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<ActionResult<List<FacturaDto>>> GetAll(int empresaId, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        => Ok(await _service.ListarFacturasAsync(empresaId, desde, hasta));

    [HttpGet("{id}")]
    public async Task<ActionResult<FacturaDto>> GetById(int empresaId, int id)
    {
        var factura = await _service.ObtenerFacturaAsync(empresaId, id);
        return factura is null ? NotFound() : Ok(factura);
    }

    [HttpPost]
    public async Task<ActionResult<FacturaDto>> Create(int empresaId, CrearFacturaDto dto)
    {
        if (dto.EmpresaId != empresaId) return BadRequest(new { error = "EmpresaId no coincide." });
        try
        {
            var factura = await _service.CrearFacturaAsync(dto);
            return CreatedAtAction(nameof(GetById), new { empresaId, id = factura.Id }, factura);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("{id}/anular")]
    public async Task<ActionResult<FacturaDto>> Anular(int empresaId, int id)
    {
        try
        {
            var factura = await _service.AnularFacturaAsync(empresaId, id);
            return Ok(factura);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> DescargarPdf(int empresaId, int id)
    {
        var factura = await _service.ObtenerFacturaAsync(empresaId, id);
        if (factura is null) return NotFound();
        var pdfBytes = _pdfService.GenerarPdf(factura);
        return File(pdfBytes, "application/pdf", $"Factura_{factura.NumeroFactura}.pdf");
    }

    // --- Cierres de Facturación ---

    [HttpPost("cierres/diario")]
    public async Task<ActionResult<CierreDto>> CierreDiario(int empresaId, CierreDiarioRequest request)
    {
        try
        {
            var cierre = await _service.CierreDiarioAsync(empresaId, request.Fecha);
            return Ok(cierre);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("cierres/mensual")]
    public async Task<ActionResult<CierreDto>> CierreMensual(int empresaId, CierreMensualRequest request)
    {
        try
        {
            var cierre = await _service.CierreMensualAsync(empresaId, request.Anio, request.Mes);
            return Ok(cierre);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet("cierres")]
    public async Task<ActionResult<List<CierreDto>>> ListarCierres(int empresaId, [FromQuery] string? tipo)
        => Ok(await _service.ListarCierresAsync(empresaId, tipo));
}
