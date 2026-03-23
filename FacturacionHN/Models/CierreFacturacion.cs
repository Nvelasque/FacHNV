using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionHN.Models;

public class CierreFacturacion
{
    public int Id { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    /// <summary>Diario o Mensual</summary>
    [Required, MaxLength(10)]
    public string TipoCierre { get; set; } = string.Empty;

    public DateTime FechaCierre { get; set; }

    /// <summary>Para cierre mensual: año-mes (ej: 2026-03)</summary>
    [MaxLength(7)]
    public string? Periodo { get; set; }

    public int TotalFacturasEmitidas { get; set; }
    public int TotalFacturasAnuladas { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoSubTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoExento { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoExonerado { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoGravado15 { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoISV15 { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoDescuentos { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoTotal { get; set; }

    [MaxLength(20)]
    public string? NumeroFacturaInicial { get; set; }

    [MaxLength(20)]
    public string? NumeroFacturaFinal { get; set; }

    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;
}
