using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionHN.Models;

public class DetalleFactura
{
    public int Id { get; set; }

    public int FacturaId { get; set; }
    public Factura Factura { get; set; } = null!;

    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public int Cantidad { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Descuento { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ISV { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }
}
