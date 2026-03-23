namespace FacturacionHN.DTOs;

public record AutorizacionDto(
    int Id, int EmpresaId, string TipoCierre, DateTime FechaCierre, string? Periodo,
    string Motivo, string SolicitadoPor, DateTime FechaSolicitud,
    string Estado, string? AprobadoPor, DateTime? FechaResolucion,
    string? ObservacionResolucion, string? CodigoAutorizacion, bool Utilizada
);

public record SolicitarReprocesoDto(
    string TipoCierre,   // Diario o Mensual
    DateTime? Fecha,     // Para diario
    int? Anio,           // Para mensual
    int? Mes,            // Para mensual
    string Motivo,
    string SolicitadoPor
);

public record AprobarReprocesoDto(
    string AprobadoPor,
    string? Observacion
);

public record RechazarReprocesoDto(
    string AprobadoPor,
    string? Observacion
);

public record ReabrirConAutorizacionDto(
    string CodigoAutorizacion
);
