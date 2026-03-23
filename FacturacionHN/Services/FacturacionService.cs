using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FacturacionHN.Data;
using FacturacionHN.DTOs;
using FacturacionHN.Models;

namespace FacturacionHN.Services;

public class FacturacionService : IFacturacionService
{
    private readonly FacturacionDbContext _db;
    public FacturacionService(FacturacionDbContext db) => _db = db;

    public async Task<FacturaDto> CrearFacturaAsync(CrearFacturaDto dto)
    {
        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SP_CrearFactura";
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add(new SqlParameter("@EmpresaId", dto.EmpresaId));
        cmd.Parameters.Add(new SqlParameter("@ClienteId", dto.ClienteId));
        cmd.Parameters.Add(new SqlParameter("@Modalidad", dto.Modalidad ?? "Autoimpresor"));
        cmd.Parameters.Add(new SqlParameter("@NumeroOrdenCompraExenta", (object?)dto.NumeroOrdenCompraExenta ?? DBNull.Value));
        cmd.Parameters.Add(new SqlParameter("@NumeroConstanciaRegistroExonerados", (object?)dto.NumeroConstanciaRegistroExonerados ?? DBNull.Value));
        cmd.Parameters.Add(new SqlParameter("@NumeroRegistroSAG", (object?)dto.NumeroRegistroSAG ?? DBNull.Value));
        var detallesJson = System.Text.Json.JsonSerializer.Serialize(
            dto.Detalles.Select(d => new { productoId = d.ProductoId, cantidad = d.Cantidad, descuento = d.Descuento }));
        cmd.Parameters.Add(new SqlParameter("@DetallesJson", detallesJson));
        var outParam = new SqlParameter("@FacturaId", SqlDbType.Int) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(outParam);
        try
        {
            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                throw new InvalidOperationException("No se pudo crear la factura.");
            var factura = MapFacturaFromReader(reader);
            await reader.NextResultAsync();
            var detalles = new List<DetalleFacturaDto>();
            while (await reader.ReadAsync())
            {
                detalles.Add(new DetalleFacturaDto(
                    reader.GetInt32(reader.GetOrdinal("Id")),
                    reader.GetString(reader.GetOrdinal("ProductoCodigo")),
                    reader.GetString(reader.GetOrdinal("ProductoDescripcion")),
                    reader.GetInt32(reader.GetOrdinal("Cantidad")),
                    reader.GetDecimal(reader.GetOrdinal("PrecioUnitario")),
                    reader.GetDecimal(reader.GetOrdinal("Descuento")),
                    reader.GetDecimal(reader.GetOrdinal("SubTotal")),
                    reader.GetDecimal(reader.GetOrdinal("ISV")),
                    reader.GetDecimal(reader.GetOrdinal("Total"))));
            }
            return factura with { Detalles = detalles };
        }
        catch (SqlException ex) { throw new InvalidOperationException(ex.Message); }
        finally { await conn.CloseAsync(); }
    }

    public async Task<FacturaDto> AnularFacturaAsync(int empresaId, int id)
    {
        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SP_AnularFactura";
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add(new SqlParameter("@EmpresaId", empresaId));
        cmd.Parameters.Add(new SqlParameter("@FacturaId", id));
        try
        {
            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                throw new InvalidOperationException("Factura no encontrada.");
            var factura = MapFacturaFromReader(reader);
            var detalles = await _db.DetalleFacturas
                .Include(d => d.Producto)
                .Where(d => d.FacturaId == id)
                .Select(d => new DetalleFacturaDto(
                    d.Id, d.Producto.Codigo, d.Producto.Descripcion,
                    d.Cantidad, d.PrecioUnitario, d.Descuento,
                    d.SubTotal, d.ISV, d.Total))
                .ToListAsync();
            return factura with { Detalles = detalles };
        }
        catch (SqlException ex) { throw new InvalidOperationException(ex.Message); }
        finally { await conn.CloseAsync(); }
    }

    public async Task<FacturaDto?> ObtenerFacturaAsync(int empresaId, int id)
    {
        var f = await _db.Facturas
            .Include(f => f.Empresa).Include(f => f.CAI)
            .Include(f => f.Cliente)
            .Include(f => f.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(f => f.Id == id && f.EmpresaId == empresaId);
        return f is null ? null : MapFacturaToDto(f);
    }

    public async Task<List<FacturaDto>> ListarFacturasAsync(int empresaId, DateTime? desde, DateTime? hasta)
    {
        var query = _db.Facturas
            .Include(f => f.Empresa).Include(f => f.CAI)
            .Include(f => f.Cliente)
            .Include(f => f.Detalles).ThenInclude(d => d.Producto)
            .Where(f => f.EmpresaId == empresaId);
        if (desde.HasValue) query = query.Where(f => f.FechaEmision >= desde.Value);
        if (hasta.HasValue) query = query.Where(f => f.FechaEmision <= hasta.Value);
        var facturas = await query.OrderByDescending(f => f.FechaEmision).ToListAsync();
        return facturas.Select(MapFacturaToDto).ToList();
    }

    public async Task<CierreDto> CierreDiarioAsync(int empresaId, DateTime fecha)
    {
        var fechaCierre = fecha.Date;
        var existente = await _db.CierresFacturacion
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.TipoCierre == "Diario" && c.FechaCierre == fechaCierre);
        if (existente != null)
            throw new InvalidOperationException($"Ya existe un cierre diario para {fechaCierre:yyyy-MM-dd}.");
        var facturas = await _db.Facturas
            .Where(f => f.EmpresaId == empresaId && f.FechaEmision.Date == fechaCierre)
            .ToListAsync();
        if (!facturas.Any())
            throw new InvalidOperationException($"No hay facturas para cerrar en {fechaCierre:yyyy-MM-dd}.");
        var emitidas = facturas.Where(f => f.Estado == "Emitida").ToList();
        var cierre = new CierreFacturacion
        {
            EmpresaId = empresaId, TipoCierre = "Diario", FechaCierre = fechaCierre,
            TotalFacturasEmitidas = emitidas.Count,
            TotalFacturasAnuladas = facturas.Count(f => f.Estado == "Anulada"),
            MontoSubTotal = emitidas.Sum(f => f.SubTotal),
            MontoExento = emitidas.Sum(f => f.ImporteExento),
            MontoExonerado = emitidas.Sum(f => f.ImporteExonerado),
            MontoGravado15 = emitidas.Sum(f => f.ImporteGravado15),
            MontoISV15 = emitidas.Sum(f => f.ISV15),
            MontoDescuentos = emitidas.Sum(f => f.Descuento),
            MontoTotal = emitidas.Sum(f => f.Total),
            NumeroFacturaInicial = facturas.OrderBy(f => f.NumeroFactura).First().NumeroFactura,
            NumeroFacturaFinal = facturas.OrderBy(f => f.NumeroFactura).Last().NumeroFactura,
            FechaGeneracion = DateTime.UtcNow
        };
        _db.CierresFacturacion.Add(cierre);
        await _db.SaveChangesAsync();
        return MapCierreToDto(cierre);
    }

    public async Task<CierreDto> CierreMensualAsync(int empresaId, int anio, int mes)
    {
        var periodo = $"{anio}-{mes:D2}";
        var primerDia = new DateTime(anio, mes, 1);
        var ultimoDia = primerDia.AddMonths(1).AddDays(-1);
        var existente = await _db.CierresFacturacion
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.TipoCierre == "Mensual" && c.Periodo == periodo);
        if (existente != null)
            throw new InvalidOperationException($"Ya existe un cierre mensual para {periodo}.");
        var diasConFacturas = await _db.Facturas
            .Where(f => f.EmpresaId == empresaId && f.FechaEmision >= primerDia && f.FechaEmision <= ultimoDia)
            .Select(f => f.FechaEmision.Date).Distinct().ToListAsync();
        if (!diasConFacturas.Any())
            throw new InvalidOperationException($"No hay facturas en el periodo {periodo}.");
        var diasConCierre = await _db.CierresFacturacion
            .Where(c => c.EmpresaId == empresaId && c.TipoCierre == "Diario"
                && c.FechaCierre >= primerDia && c.FechaCierre <= ultimoDia)
            .Select(c => c.FechaCierre.Date).ToListAsync();
        var diasSinCierre = diasConFacturas.Except(diasConCierre).OrderBy(d => d).ToList();
        if (diasSinCierre.Any())
            throw new InvalidOperationException(
                $"Faltan cierres diarios para: {string.Join(", ", diasSinCierre.Select(d => d.ToString("yyyy-MM-dd")))}");
        var cierresDiarios = await _db.CierresFacturacion
            .Where(c => c.EmpresaId == empresaId && c.TipoCierre == "Diario"
                && c.FechaCierre >= primerDia && c.FechaCierre <= ultimoDia)
            .ToListAsync();
        var cierre = new CierreFacturacion
        {
            EmpresaId = empresaId, TipoCierre = "Mensual", FechaCierre = ultimoDia, Periodo = periodo,
            TotalFacturasEmitidas = cierresDiarios.Sum(c => c.TotalFacturasEmitidas),
            TotalFacturasAnuladas = cierresDiarios.Sum(c => c.TotalFacturasAnuladas),
            MontoSubTotal = cierresDiarios.Sum(c => c.MontoSubTotal),
            MontoExento = cierresDiarios.Sum(c => c.MontoExento),
            MontoExonerado = cierresDiarios.Sum(c => c.MontoExonerado),
            MontoGravado15 = cierresDiarios.Sum(c => c.MontoGravado15),
            MontoISV15 = cierresDiarios.Sum(c => c.MontoISV15),
            MontoDescuentos = cierresDiarios.Sum(c => c.MontoDescuentos),
            MontoTotal = cierresDiarios.Sum(c => c.MontoTotal),
            NumeroFacturaInicial = cierresDiarios.OrderBy(c => c.FechaCierre).First().NumeroFacturaInicial,
            NumeroFacturaFinal = cierresDiarios.OrderBy(c => c.FechaCierre).Last().NumeroFacturaFinal,
            FechaGeneracion = DateTime.UtcNow
        };
        _db.CierresFacturacion.Add(cierre);
        await _db.SaveChangesAsync();
        return MapCierreToDto(cierre);
    }

    public async Task<List<CierreDto>> ListarCierresAsync(int empresaId, string? tipoCierre)
    {
        var query = _db.CierresFacturacion.Where(c => c.EmpresaId == empresaId);
        if (!string.IsNullOrEmpty(tipoCierre))
            query = query.Where(c => c.TipoCierre == tipoCierre);
        var cierres = await query.OrderByDescending(c => c.FechaCierre).ToListAsync();
        return cierres.Select(MapCierreToDto).ToList();
    }

    private static FacturaDto MapFacturaFromReader(System.Data.Common.DbDataReader r) => new(
        Id: r.GetInt32(r.GetOrdinal("Id")),
        NumeroFactura: r.GetString(r.GetOrdinal("NumeroFactura")),
        Modalidad: r.GetString(r.GetOrdinal("Modalidad")),
        EmisorRazonSocial: r.GetString(r.GetOrdinal("EmisorRazonSocial")),
        EmisorRTN: r.GetString(r.GetOrdinal("EmisorRTN")),
        EmisorNombreComercial: r.IsDBNull(r.GetOrdinal("EmisorNombreComercial")) ? null : r.GetString(r.GetOrdinal("EmisorNombreComercial")),
        EmisorDireccion: r.GetString(r.GetOrdinal("EmisorDireccion")),
        EmisorTelefono: r.IsDBNull(r.GetOrdinal("EmisorTelefono")) ? null : r.GetString(r.GetOrdinal("EmisorTelefono")),
        EmisorCorreo: r.IsDBNull(r.GetOrdinal("EmisorCorreo")) ? null : r.GetString(r.GetOrdinal("EmisorCorreo")),
        NumeroCai: r.GetString(r.GetOrdinal("NumeroCai")),
        RangoAutorizado: r.GetString(r.GetOrdinal("RangoAutorizado")),
        FechaLimiteEmision: r.GetDateTime(r.GetOrdinal("FechaLimiteEmision")),
        ClienteNombre: r.GetString(r.GetOrdinal("ClienteNombre")),
        ClienteRTN: r.GetString(r.GetOrdinal("ClienteRTN")),
        FechaEmision: r.GetDateTime(r.GetOrdinal("FechaEmision")),
        SubTotal: r.GetDecimal(r.GetOrdinal("SubTotal")),
        ImporteExento: r.GetDecimal(r.GetOrdinal("ImporteExento")),
        ImporteExonerado: r.GetDecimal(r.GetOrdinal("ImporteExonerado")),
        ImporteGravado15: r.GetDecimal(r.GetOrdinal("ImporteGravado15")),
        ISV15: r.GetDecimal(r.GetOrdinal("ISV15")),
        Descuento: r.GetDecimal(r.GetOrdinal("Descuento")),
        Total: r.GetDecimal(r.GetOrdinal("Total")),
        Estado: r.GetString(r.GetOrdinal("Estado")),
        NumeroOrdenCompraExenta: r.IsDBNull(r.GetOrdinal("NumeroOrdenCompraExenta")) ? null : r.GetString(r.GetOrdinal("NumeroOrdenCompraExenta")),
        NumeroConstanciaRegistroExonerados: r.IsDBNull(r.GetOrdinal("NumeroConstanciaRegistroExonerados")) ? null : r.GetString(r.GetOrdinal("NumeroConstanciaRegistroExonerados")),
        NumeroRegistroSAG: r.IsDBNull(r.GetOrdinal("NumeroRegistroSAG")) ? null : r.GetString(r.GetOrdinal("NumeroRegistroSAG")),
        ColorPrimario: "#1B5E20",
        ColorSecundario: "#E8F5E9",
        Detalles: new List<DetalleFacturaDto>()
    );

    private static FacturaDto MapFacturaToDto(Factura f) => new(
        Id: f.Id, NumeroFactura: f.NumeroFactura, Modalidad: f.Modalidad,
        EmisorRazonSocial: f.Empresa.RazonSocial, EmisorRTN: f.Empresa.RTN,
        EmisorNombreComercial: f.Empresa.NombreComercial,
        EmisorDireccion: f.Empresa.DireccionCasaMatriz,
        EmisorTelefono: f.Empresa.Telefono, EmisorCorreo: f.Empresa.Correo,
        NumeroCai: f.CAI.NumeroCai,
        RangoAutorizado: $"{f.CAI.RangoInicial} a {f.CAI.RangoFinal}",
        FechaLimiteEmision: f.CAI.FechaLimiteEmision,
        ClienteNombre: f.Cliente.Nombre, ClienteRTN: f.Cliente.RTN,
        FechaEmision: f.FechaEmision, SubTotal: f.SubTotal,
        ImporteExento: f.ImporteExento, ImporteExonerado: f.ImporteExonerado,
        ImporteGravado15: f.ImporteGravado15, ISV15: f.ISV15,
        Descuento: f.Descuento, Total: f.Total, Estado: f.Estado,
        NumeroOrdenCompraExenta: f.NumeroOrdenCompraExenta,
        NumeroConstanciaRegistroExonerados: f.NumeroConstanciaRegistroExonerados,
        NumeroRegistroSAG: f.NumeroRegistroSAG,
        ColorPrimario: f.Empresa.ColorPrimario,
        ColorSecundario: f.Empresa.ColorSecundario,
        Detalles: f.Detalles.Select(d => new DetalleFacturaDto(
            d.Id, d.Producto.Codigo, d.Producto.Descripcion,
            d.Cantidad, d.PrecioUnitario, d.Descuento,
            d.SubTotal, d.ISV, d.Total)).ToList()
    );

    private static CierreDto MapCierreToDto(CierreFacturacion c) => new(
        c.Id, c.EmpresaId, c.TipoCierre, c.FechaCierre, c.Periodo,
        c.TotalFacturasEmitidas, c.TotalFacturasAnuladas,
        c.MontoSubTotal, c.MontoExento, c.MontoExonerado,
        c.MontoGravado15, c.MontoISV15, c.MontoDescuentos, c.MontoTotal,
        c.NumeroFacturaInicial, c.NumeroFacturaFinal, c.FechaGeneracion
    );
}
