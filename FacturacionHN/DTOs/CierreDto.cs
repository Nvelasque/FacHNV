namespace FacturacionHN.DTOs;

public record CierreDto(
    int Id, int EmpresaId, string TipoCierre, DateTime FechaCierre, string? Periodo,
    int TotalFacturasEmitidas, int TotalFacturasAnuladas,
    decimal MontoSubTotal, decimal MontoExento, decimal MontoExonerado,
    decimal MontoGravado15, decimal MontoISV15, decimal MontoDescuentos, decimal MontoTotal,
    string? NumeroFacturaInicial, string? NumeroFacturaFinal, DateTime FechaGeneracion
);

public record CierreDiarioRequest(DateTime Fecha);

public record CierreMensualRequest(int Anio, int Mes);
