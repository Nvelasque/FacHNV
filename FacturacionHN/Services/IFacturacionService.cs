using FacturacionHN.DTOs;

namespace FacturacionHN.Services;

public interface IFacturacionService
{
    Task<FacturaDto> CrearFacturaAsync(CrearFacturaDto dto);
    Task<FacturaDto?> ObtenerFacturaAsync(int empresaId, int id);
    Task<List<FacturaDto>> ListarFacturasAsync(int empresaId, DateTime? desde, DateTime? hasta);
    Task<FacturaDto> AnularFacturaAsync(int empresaId, int id);
    Task<CierreDto> CierreDiarioAsync(int empresaId, DateTime fecha);
    Task<CierreDto> CierreMensualAsync(int empresaId, int anio, int mes);
    Task<List<CierreDto>> ListarCierresAsync(int empresaId, string? tipoCierre);
    Task<AutorizacionDto> SolicitarReprocesoAsync(int empresaId, SolicitarReprocesoDto dto);
    Task<AutorizacionDto> AprobarReprocesoAsync(int empresaId, int autorizacionId, AprobarReprocesoDto dto);
    Task<AutorizacionDto> RechazarReprocesoAsync(int empresaId, int autorizacionId, RechazarReprocesoDto dto);
    Task<List<AutorizacionDto>> ListarAutorizacionesAsync(int empresaId, string? estado);
    Task ReabrirCierreDiarioAsync(int empresaId, DateTime fecha, string codigoAutorizacion);
    Task ReabrirCierreMensualAsync(int empresaId, int anio, int mes, string codigoAutorizacion);
}
