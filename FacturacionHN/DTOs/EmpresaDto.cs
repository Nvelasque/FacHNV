namespace FacturacionHN.DTOs;

public record EmpresaDto(int Id, string RazonSocial, string RTN, string? NombreComercial,
    string DireccionCasaMatriz, string? Telefono, string? Correo, bool Activo,
    string ColorPrimario, string ColorSecundario, string? LogoUrl);

public record CrearEmpresaDto(string RazonSocial, string RTN, string? NombreComercial,
    string DireccionCasaMatriz, string? Telefono, string? Correo,
    string? ColorPrimario, string? ColorSecundario, string? LogoUrl);
