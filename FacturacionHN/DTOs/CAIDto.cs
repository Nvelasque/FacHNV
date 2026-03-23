namespace FacturacionHN.DTOs;

public record CAIDto(int Id, int EmpresaId, string NumeroCai, string RangoInicial, string RangoFinal, int CorrelativoActual,
    DateTime FechaLimiteEmision, string SucursalCodigo, string PuntoEmisionCodigo, string TipoDocumento, bool Activo);

public record CrearCAIDto(int EmpresaId, string NumeroCai, string RangoInicial, string RangoFinal,
    DateTime FechaLimiteEmision, string SucursalCodigo, string PuntoEmisionCodigo, string TipoDocumento);
