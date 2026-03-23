using System.ComponentModel.DataAnnotations;

namespace FacturacionHN.Models;

/// <summary>
/// Datos del emisor según Art. 10 del Reglamento SAR.
/// </summary>
public class Empresa
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string RazonSocial { get; set; } = string.Empty;

    [Required, MaxLength(14)]
    public string RTN { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? NombreComercial { get; set; }

    [Required, MaxLength(300)]
    public string DireccionCasaMatriz { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(100)]
    public string? Correo { get; set; }

    public bool Activo { get; set; } = true;

    // --- Personalización visual del PDF ---
    /// <summary>Color primario en hex (encabezados, bordes). Ej: #1B5E20</summary>
    [MaxLength(7)]
    public string ColorPrimario { get; set; } = "#1B5E20";

    /// <summary>Color secundario en hex (fondos alternos). Ej: #E8F5E9</summary>
    [MaxLength(7)]
    public string ColorSecundario { get; set; } = "#E8F5E9";

    /// <summary>URL o ruta del logo de la empresa (opcional)</summary>
    [MaxLength(500)]
    public string? LogoUrl { get; set; }
}
