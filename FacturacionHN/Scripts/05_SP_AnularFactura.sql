-- =============================================
-- SP: AnularFactura
-- Anula una factura validando empresa y estado
-- =============================================

USE FacturacionHN;
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_AnularFactura')
    DROP PROCEDURE SP_AnularFactura;
GO

CREATE PROCEDURE SP_AnularFactura
    @EmpresaId  INT,
    @FacturaId  INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validar que la factura existe y pertenece a la empresa
    IF NOT EXISTS (SELECT 1 FROM Facturas WHERE Id = @FacturaId AND EmpresaId = @EmpresaId)
    BEGIN
        RAISERROR('Factura no encontrada para esta empresa.', 16, 1);
        RETURN;
    END

    -- Validar que no esté ya anulada
    IF EXISTS (SELECT 1 FROM Facturas WHERE Id = @FacturaId AND Estado = 'Anulada')
    BEGIN
        RAISERROR('La factura ya se encuentra anulada.', 16, 1);
        RETURN;
    END

    -- Validar que no pertenezca a un cierre ya generado
    DECLARE @FechaEmision DATETIME2;
    SELECT @FechaEmision = FechaEmision FROM Facturas WHERE Id = @FacturaId;

    IF EXISTS (
        SELECT 1 FROM CierresFacturacion
        WHERE EmpresaId = @EmpresaId
          AND TipoCierre = 'Diario'
          AND FechaCierre = CAST(@FechaEmision AS DATE)
    )
    BEGIN
        RAISERROR('No se puede anular: ya existe un cierre diario para esta fecha.', 16, 1);
        RETURN;
    END

    UPDATE Facturas SET Estado = 'Anulada' WHERE Id = @FacturaId;

    -- Retornar factura actualizada
    SELECT
        f.Id, f.NumeroFactura, f.Modalidad, f.FechaEmision, f.Estado,
        f.SubTotal, f.ImporteExento, f.ImporteExonerado, f.ImporteGravado15,
        f.ISV15, f.Descuento, f.Total,
        f.NumeroOrdenCompraExenta, f.NumeroConstanciaRegistroExonerados, f.NumeroRegistroSAG,
        e.RazonSocial AS EmisorRazonSocial, e.RTN AS EmisorRTN, e.NombreComercial AS EmisorNombreComercial,
        e.DireccionCasaMatriz AS EmisorDireccion, e.Telefono AS EmisorTelefono, e.Correo AS EmisorCorreo,
        c.NumeroCai, c.RangoInicial + ' a ' + c.RangoFinal AS RangoAutorizado, c.FechaLimiteEmision,
        cl.Nombre AS ClienteNombre, cl.RTN AS ClienteRTN
    FROM Facturas f
    INNER JOIN Empresas e ON e.Id = f.EmpresaId
    INNER JOIN CAIs c ON c.Id = f.CAIId
    INNER JOIN Clientes cl ON cl.Id = f.ClienteId
    WHERE f.Id = @FacturaId;
END
GO
