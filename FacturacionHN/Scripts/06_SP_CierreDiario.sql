-- =============================================
-- SP: CierreFacturacionDiario
-- Genera cierre del día con resumen de facturación
-- =============================================

USE FacturacionHN;
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_CierreFacturacionDiario')
    DROP PROCEDURE SP_CierreFacturacionDiario;
GO

CREATE PROCEDURE SP_CierreFacturacionDiario
    @EmpresaId  INT,
    @Fecha      DATE  -- Fecha del cierre (ej: '2026-03-23')
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- Validar empresa
    IF NOT EXISTS (SELECT 1 FROM Empresas WHERE Id = @EmpresaId AND Activo = 1)
    BEGIN
        RAISERROR('Empresa no encontrada o inactiva.', 16, 1);
        RETURN;
    END

    -- Validar que no exista cierre para esa fecha
    IF EXISTS (SELECT 1 FROM CierresFacturacion WHERE EmpresaId = @EmpresaId AND TipoCierre = 'Diario' AND FechaCierre = @Fecha)
    BEGIN
        RAISERROR('Ya existe un cierre diario para esta fecha.', 16, 1);
        RETURN;
    END

    -- Validar que existan facturas para cerrar
    IF NOT EXISTS (
        SELECT 1 FROM Facturas
        WHERE EmpresaId = @EmpresaId AND CAST(FechaEmision AS DATE) = @Fecha
    )
    BEGIN
        RAISERROR('No hay facturas para cerrar en esta fecha.', 16, 1);
        RETURN;
    END

    DECLARE @TotalEmitidas      INT,
            @TotalAnuladas      INT,
            @SubTotal           DECIMAL(18,2),
            @Exento             DECIMAL(18,2),
            @Exonerado          DECIMAL(18,2),
            @Gravado15          DECIMAL(18,2),
            @ISV15              DECIMAL(18,2),
            @Descuentos         DECIMAL(18,2),
            @Total              DECIMAL(18,2),
            @FacturaInicial     NVARCHAR(20),
            @FacturaFinal       NVARCHAR(20),
            @CierreId           INT;

    -- Calcular resumen (solo facturas emitidas, no anuladas)
    SELECT
        @TotalEmitidas  = COUNT(CASE WHEN Estado = 'Emitida' THEN 1 END),
        @TotalAnuladas  = COUNT(CASE WHEN Estado = 'Anulada' THEN 1 END),
        @SubTotal       = ISNULL(SUM(CASE WHEN Estado = 'Emitida' THEN SubTotal ELSE 0 END), 0),
        @Exento         = ISNULL(SUM(CASE WHEN Estado = 'Emitida' THEN ImporteExento ELSE 0 END), 0),
        @Exonerado      = ISNULL(SUM(CASE WHEN Estado = 'Emitida' THEN ImporteExonerado ELSE 0 END), 0),
        @Gravado15      = ISNULL(SUM(CASE WHEN Estado = 'Emitida' THEN ImporteGravado15 ELSE 0 END), 0),
        @ISV15          = ISNULL(SUM(CASE WHEN Estado = 'Emitida' THEN ISV15 ELSE 0 END), 0),
        @Descuentos     = ISNULL(SUM(CASE WHEN Estado = 'Emitida' THEN Descuento ELSE 0 END), 0),
        @Total          = ISNULL(SUM(CASE WHEN Estado = 'Emitida' THEN Total ELSE 0 END), 0)
    FROM Facturas
    WHERE EmpresaId = @EmpresaId AND CAST(FechaEmision AS DATE) = @Fecha;

    -- Rango de facturas del día
    SELECT @FacturaInicial = MIN(NumeroFactura), @FacturaFinal = MAX(NumeroFactura)
    FROM Facturas
    WHERE EmpresaId = @EmpresaId AND CAST(FechaEmision AS DATE) = @Fecha;

    INSERT INTO CierresFacturacion (EmpresaId, TipoCierre, FechaCierre, Periodo,
        TotalFacturasEmitidas, TotalFacturasAnuladas,
        MontoSubTotal, MontoExento, MontoExonerado, MontoGravado15, MontoISV15, MontoDescuentos, MontoTotal,
        NumeroFacturaInicial, NumeroFacturaFinal, FechaGeneracion)
    VALUES (@EmpresaId, 'Diario', @Fecha, NULL,
        @TotalEmitidas, @TotalAnuladas,
        @SubTotal, @Exento, @Exonerado, @Gravado15, @ISV15, @Descuentos, @Total,
        @FacturaInicial, @FacturaFinal, GETUTCDATE());

    SET @CierreId = SCOPE_IDENTITY();

    -- Retornar cierre generado
    SELECT * FROM CierresFacturacion WHERE Id = @CierreId;
END
GO
