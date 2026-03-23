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

    // --- Autorizaciones de Reproceso ---

    [HttpPost("cierres/reproceso/solicitar")]
    public async Task<ActionResult<AutorizacionDto>> SolicitarReproceso(int empresaId, SolicitarReprocesoDto dto)
    {
        try { return Ok(await _service.SolicitarReprocesoAsync(empresaId, dto)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("cierres/reproceso/{autorizacionId}/aprobar")]
    public async Task<ActionResult<AutorizacionDto>> AprobarReproceso(int empresaId, int autorizacionId, AprobarReprocesoDto dto)
    {
        try { return Ok(await _service.AprobarReprocesoAsync(empresaId, autorizacionId, dto)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("cierres/reproceso/{autorizacionId}/rechazar")]
    public async Task<ActionResult<AutorizacionDto>> RechazarReproceso(int empresaId, int autorizacionId, RechazarReprocesoDto dto)
    {
        try { return Ok(await _service.RechazarReprocesoAsync(empresaId, autorizacionId, dto)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet("cierres/reproceso")]
    public async Task<ActionResult<List<AutorizacionDto>>> ListarAutorizaciones(int empresaId, [FromQuery] string? estado)
        => Ok(await _service.ListarAutorizacionesAsync(empresaId, estado));

    [HttpDelete("cierres/diario")]
    public async Task<IActionResult> ReabrirCierreDiario(int empresaId, ReabrirConAutorizacionDto request, [FromQuery] DateTime fecha)
    {
        try
        {
            await _service.ReabrirCierreDiarioAsync(empresaId, fecha, request.CodigoAutorizacion);
            return Ok(new { mensaje = $"Cierre diario {fecha:yyyy-MM-dd} reabierto con autorización." });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpDelete("cierres/mensual")]
    public async Task<IActionResult> ReabrirCierreMensual(int empresaId, ReabrirConAutorizacionDto request, [FromQuery] int anio, [FromQuery] int mes)
    {
        try
        {
            await _service.ReabrirCierreMensualAsync(empresaId, anio, mes, request.CodigoAutorizacion);
            return Ok(new { mensaje = $"Cierre mensual {anio}-{mes:D2} reabierto con autorización." });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}
