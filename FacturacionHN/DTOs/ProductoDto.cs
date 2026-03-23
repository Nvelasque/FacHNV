namespace FacturacionHN.DTOs;

public record ProductoDto(int Id, int EmpresaId, string Codigo, string Descripcion, decimal Precio, bool GravadoISV, bool Activo);

public record CrearProductoDto(int EmpresaId, string Codigo, string Descripcion, decimal Precio, bool GravadoISV);

public record ActualizarProductoDto(string Descripcion, decimal Precio, bool GravadoISV, bool Activo);
