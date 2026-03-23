-- =============================================
-- SP: CrearFactura
-- Crea factura completa en una sola transacción
-- Recibe detalles como JSON para un solo round-trip
-- =============================================

USE FacturacionHN;
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_CrearFactura')
    DROP PROCEDURE SP_CrearFactura;
GO

CREATE PROCEDURE SP_CrearFactura
    @EmpresaId                          INT,
    @ClienteId                          INT,
    @Modalidad                          NVARCHAR(20) = 'Autoimpresor',
    @NumeroOrdenCompraExenta            NVARCHAR(50) = NULL,
    @NumeroConstanciaRegistroExonerados NVARCHAR(50) = NULL,
    @NumeroRegistroSAG                  NVARCHAR(50) = NULL,
    @DetallesJson                       NVARCHAR(MAX),  -- [{"productoId":1,"cantidad":2,"descuento":0}, ...]
    @FacturaId                          INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @CAIId              INT,
            @SucursalCodigo     NVARCHAR(20),
            @PuntoEmisionCodigo NVARCHAR(20),
            @TipoDocumento      NVARCHAR(2),
            @Correlativo        INT,
            @NumeroFactura      NVARCHAR(20),
            @EsExonerado        BIT = 0,
            @TasaISV            DECIMAL(18,2) = 0.15;

    -- Validar empresa
    IF NOT EXISTS (SELECT 1 FROM Empresas WHERE Id = @EmpresaId AND Activo = 1)
    BEGIN
        RAISERROR('Empresa no encontrada o inactiva.', 16, 1);
        RETURN;
    END

    -- Validar cliente pertenece a la empresa
    IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Id = @ClienteId AND EmpresaId = @EmpresaId AND Activo = 1)
    BEGIN
        RAISERROR('Cliente no encontrado para esta empresa.', 16, 1);
        RETURN;
    END

    -- Determinar si es exonerado
    IF @NumeroOrdenCompraExenta IS NOT NULL AND LEN(@NumeroOrdenCompraExenta) > 0
        SET @EsExonerado = 1;

    -- Validar que no exista cierre diario para hoy
    IF EXISTS (
        SELECT 1 FROM CierresFacturacion
        WHERE EmpresaId = @EmpresaId
          AND TipoCierre = 'Diario'
          AND FechaCierre = CAST(GETUTCDATE() AS DATE)
    )
    BEGIN
        RAISERROR('No se puede facturar: ya existe un cierre diario para la fecha de hoy.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;

    -- Obtener CAI activo con bloqueo para evitar duplicados en concurrencia
    SELECT TOP 1
        @CAIId = Id,
        @SucursalCodigo = SucursalCodigo,
        @PuntoEmisionCodigo = PuntoEmisionCodigo,
        @TipoDocumento = TipoDocumento,
        @Correlativo = CorrelativoActual + 1
    FROM CAIs WITH (UPDLOCK)
    WHERE EmpresaId = @EmpresaId
      AND Activo = 1
      AND FechaLimiteEmision >= GETUTCDATE()
      AND TipoDocumento = '01'
    ORDER BY Id;

    IF @CAIId IS NULL
    BEGIN
        ROLLBACK;
        RAISERROR('No hay CAI activo disponible para esta empresa.', 16, 1);
        RETURN;
    END

    -- Validar que el correlativo no exceda el rango final
    DECLARE @RangoFinalNum INT;
    SELECT @RangoFinalNum = CAST(RIGHT(RangoFinal, 8) AS INT) FROM CAIs WHERE Id = @CAIId;
    IF @Correlativo > @RangoFinalNum
    BEGIN
        -- Desactivar CAI agotado
        UPDATE CAIs SET Activo = 0 WHERE Id = @CAIId;
        ROLLBACK;
        RAISERROR('El CAI ha agotado su rango de numeración. Se desactivó automáticamente. Registre un nuevo CAI.', 16, 1);
        RETURN;
    END

    -- Generar número correlativo SAR: NNN-NNN-NN-NNNNNNNN
    SET @NumeroFactura = @SucursalCodigo + '-' + @PuntoEmisionCodigo + '-' + @TipoDocumento + '-' + RIGHT('00000000' + CAST(@Correlativo AS VARCHAR(8)), 8);

    -- Parsear detalles del JSON a tabla temporal
    SELECT
        ProductoId,
        Cantidad,
        Descuento
    INTO #Detalles
    FROM OPENJSON(@DetallesJson)
    WITH (
        ProductoId  INT             '$.productoId',
        Cantidad    INT             '$.cantidad',
        Descuento   DECIMAL(18,2)   '$.descuento'
    );

    -- Validar que todos los productos existen y pertenecen a la empresa
    IF EXISTS (
        SELECT 1 FROM #Detalles d
        LEFT JOIN Productos p ON p.Id = d.ProductoId AND p.EmpresaId = @EmpresaId AND p.Activo = 1
        WHERE p.Id IS NULL
    )
    BEGIN
        ROLLBACK;
        RAISERROR('Uno o más productos no encontrados para esta empresa.', 16, 1);
        RETURN;
    END

    -- Calcular líneas de detalle
    SELECT
        d.ProductoId,
        d.Cantidad,
        p.Precio AS PrecioUnitario,
        d.Descuento,
        (p.Precio * d.Cantidad) - d.Descuento AS LineaBase,
        CASE
            WHEN @EsExonerado = 1 THEN 0
            WHEN p.GravadoISV = 1 THEN ROUND(((p.Precio * d.Cantidad) - d.Descuento) * @TasaISV, 2)
            ELSE 0
        END AS LineaISV,
        p.GravadoISV
    INTO #Calculado
    FROM #Detalles d
    INNER JOIN Productos p ON p.Id = d.ProductoId;

    -- Calcular totales
    DECLARE @SubTotal           DECIMAL(18,2) = (SELECT SUM(PrecioUnitario * Cantidad) FROM #Calculado),
            @DescuentoTotal     DECIMAL(18,2) = (SELECT SUM(Descuento) FROM #Calculado),
            @ImporteExento      DECIMAL(18,2) = (SELECT ISNULL(SUM(LineaBase), 0) FROM #Calculado WHERE @EsExonerado = 0 AND GravadoISV = 0),
            @ImporteExonerado   DECIMAL(18,2) = (SELECT CASE WHEN @EsExonerado = 1 THEN ISNULL(SUM(LineaBase), 0) ELSE 0 END FROM #Calculado),
            @ImporteGravado15   DECIMAL(18,2) = (SELECT CASE WHEN @EsExonerado = 0 THEN ISNULL(SUM(LineaBase), 0) ELSE 0 END FROM #Calculado WHERE GravadoISV = 1),
            @ISV15              DECIMAL(18,2) = (SELECT ISNULL(SUM(LineaISV), 0) FROM #Calculado),
            @Total              DECIMAL(18,2);

    SET @Total = @SubTotal - @DescuentoTotal + @ISV15;

    -- Insertar factura
    INSERT INTO Facturas (EmpresaId, NumeroFactura, CAIId, ClienteId, FechaEmision, Modalidad,
        SubTotal, ImporteExento, ImporteExonerado, ImporteGravado15, ISV15, Descuento, Total, Estado,
        NumeroOrdenCompraExenta, NumeroConstanciaRegistroExonerados, NumeroRegistroSAG)
    VALUES (@EmpresaId, @NumeroFactura, @CAIId, @ClienteId, GETUTCDATE(), @Modalidad,
        @SubTotal, @ImporteExento, @ImporteExonerado, @ImporteGravado15, @ISV15, @DescuentoTotal, @Total, 'Emitida',
        @NumeroOrdenCompraExenta, @NumeroConstanciaRegistroExonerados, @NumeroRegistroSAG);

    SET @FacturaId = SCOPE_IDENTITY();

    -- Insertar detalles
    INSERT INTO DetalleFacturas (FacturaId, ProductoId, Cantidad, PrecioUnitario, Descuento, SubTotal, ISV, Total)
    SELECT
        @FacturaId,
        ProductoId,
        Cantidad,
        PrecioUnitario,
        Descuento,
        LineaBase,
        LineaISV,
        LineaBase + LineaISV
    FROM #Calculado;

    -- Actualizar correlativo del CAI
    UPDATE CAIs SET CorrelativoActual = @Correlativo WHERE Id = @CAIId;

    COMMIT;

    -- Retornar factura completa
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

    -- Retornar detalles
    SELECT
        df.Id, df.Cantidad, df.PrecioUnitario, df.Descuento, df.SubTotal, df.ISV, df.Total,
        p.Codigo AS ProductoCodigo, p.Descripcion AS ProductoDescripcion
    FROM DetalleFacturas df
    INNER JOIN Productos p ON p.Id = df.ProductoId
    WHERE df.FacturaId = @FacturaId;

    DROP TABLE #Detalles;
    DROP TABLE #Calculado;
END
GO
