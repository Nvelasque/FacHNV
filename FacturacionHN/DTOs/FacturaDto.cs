namespace FacturacionHN.DTOs;

public record FacturaDto(
    int Id,
    string NumeroFactura,
    string Modalidad,
    // Datos del emisor (Art. 10)
    string EmisorRazonSocial,
    string EmisorRTN,
    string? EmisorNombreComercial,
    string EmisorDireccion,
    string? EmisorTelefono,
    string? EmisorCorreo,
    // CAI y rango
    string NumeroCai,
    string RangoAutorizado,
    DateTime FechaLimiteEmision,
    // Cliente
    string ClienteNombre,
    string ClienteRTN,
    // Montos
    DateTime FechaEmision,
    decimal SubTotal,
    decimal ImporteExento,
    decimal ImporteExonerado,
    decimal ImporteGravado15,
    decimal ISV15,
    decimal Descuento,
    decimal Total,
    string Estado,
    // Exoneración
    string? NumeroOrdenCompraExenta,
    string? NumeroConstanciaRegistroExonerados,
    string? NumeroRegistroSAG,
    // Colores de la empresa
    string ColorPrimario,
    string ColorSecundario,
    List<DetalleFacturaDto> Detalles
);

public record DetalleFacturaDto(
    int Id, string ProductoCodigo, string ProductoDescripcion,
    int Cantidad, decimal PrecioUnitario, decimal Descuento,
    decimal SubTotal, decimal ISV, decimal Total
);

public record CrearFacturaDto(
    int EmpresaId,
    int ClienteId,
    string? Modalidad, // Autoimpresor o Imprenta (default: Autoimpresor)
    // Campos opcionales para exonerados
    string? NumeroOrdenCompraExenta,
    string? NumeroConstanciaRegistroExonerados,
    string? NumeroRegistroSAG,
    List<CrearDetalleDto> Detalles
);

public record CrearDetalleDto(int ProductoId, int Cantidad, decimal Descuento);
