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
}
