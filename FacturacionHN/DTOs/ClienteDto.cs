namespace FacturacionHN.DTOs;

public record ClienteDto(int Id, int EmpresaId, string RTN, string Nombre, string? Direccion, string? Correo, string? Telefono, bool Activo);

public record CrearClienteDto(int EmpresaId, string RTN, string Nombre, string? Direccion, string? Correo, string? Telefono);

public record ActualizarClienteDto(string Nombre, string? Direccion, string? Correo, string? Telefono, bool Activo);
