using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionHN.Models;

public class AutorizacionReproceso
{
    public int Id { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    /// <summary>Diario o Mensual</summary>
    [Required, MaxLength(10)]
    public string TipoCierre { get; set; } = string.Empty;

    /// <summary>Fecha del cierre diario o último día del mes</summary>
    public DateTime FechaCierre { get; set; }

    /// <summary>YYYY-MM para mensual</summary>
    [MaxLength(7)]
    public string? Periodo { get; set; }

    [Required, MaxLength(500)]
    public string Motivo { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string SolicitadoPor { get; set; } = string.Empty;

    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

    /// <summary>Pendiente, Aprobada, Rechazada</summary>
    [Required, MaxLength(15)]
    public string Estado { get; set; } = "Pendiente";

    [MaxLength(100)]
    public string? AprobadoPor { get; set; }

    public DateTime? FechaResolucion { get; set; }

    [MaxLength(500)]
    public string? ObservacionResolucion { get; set; }

    /// <summary>Código único generado al aprobar, requerido para ejecutar el reproceso</summary>
    [MaxLength(50)]
    public string? CodigoAutorizacion { get; set; }

    /// <summary>Si ya se usó el código para reabrir</summary>
    public bool Utilizada { get; set; } = false;
}
