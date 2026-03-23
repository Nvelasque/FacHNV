using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacturacionHN.Models;

public class Producto
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Descripcion { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Precio { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public bool GravadoISV { get; set; } = true;

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
