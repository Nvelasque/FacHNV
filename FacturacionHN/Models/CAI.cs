using System.ComponentModel.DataAnnotations;

namespace FacturacionHN.Models;

/// <summary>
/// Clave de Autorización de Impresión emitida por el SAR de Honduras.
/// </summary>
public class CAI
{
    public int Id { get; set; }

    [Required, MaxLength(37)]
    public string NumeroCai { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string RangoInicial { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string RangoFinal { get; set; } = string.Empty;

    public int CorrelativoActual { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public DateTime FechaLimiteEmision { get; set; }

    [Required, MaxLength(20)]
    public string SucursalCodigo { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string PuntoEmisionCodigo { get; set; } = string.Empty;

    [Required, MaxLength(2)]
    public string TipoDocumento { get; set; } = "01"; // 01 = Factura

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
