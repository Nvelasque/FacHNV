using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionHN.Models;

public class Factura
{
    public int Id { get; set; }

    /// <summary>Numeración correlativa 16 dígitos: NNN-NNN-NN-NNNNNNNN</summary>
    [Required, MaxLength(20)]
    public string NumeroFactura { get; set; } = string.Empty;

    public int CAIId { get; set; }
    public CAI CAI { get; set; } = null!;

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;

    /// <summary>Modalidad: Autoimpresor, Imprenta</summary>
    [Required, MaxLength(20)]
    public string Modalidad { get; set; } = "Autoimpresor";

    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ImporteExento { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ImporteExonerado { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ImporteGravado15 { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ISV15 { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Descuento { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    [MaxLength(20)]
    public string Estado { get; set; } = "Emitida"; // Emitida, Anulada

    // --- Campos para clientes exonerados ---
    [MaxLength(50)]
    public string? NumeroOrdenCompraExenta { get; set; }

    [MaxLength(50)]
    public string? NumeroConstanciaRegistroExonerados { get; set; }

    [MaxLength(50)]
    public string? NumeroRegistroSAG { get; set; }

    public ICollection<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();
}
