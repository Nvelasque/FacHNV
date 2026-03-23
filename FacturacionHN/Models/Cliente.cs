using System.ComponentModel.DataAnnotations;

namespace FacturacionHN.Models;

public class Cliente
{
    public int Id { get; set; }

    [Required, MaxLength(14)]
    public string RTN { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Direccion { get; set; }

    [MaxLength(100)]
    public string? Correo { get; set; }

    [MaxLength(20)]
    public string? Telefono { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
