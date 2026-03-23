-- =============================================
-- SP: CierreFacturacionMensual
-- Genera cierre mensual consolidando los cierres diarios
-- =============================================

USE FacturacionHN;
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_CierreFacturacionMensual')
    DROP PROCEDURE SP_CierreFacturacionMensual;
GO

CREATE PROCEDURE SP_CierreFacturacionMensual
    @EmpresaId  INT,
    @Anio       INT,
    @Mes        INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Periodo        NVARCHAR(7) = RIGHT('0000' + CAST(@Anio AS VARCHAR(4)), 4) + '-' + RIGHT('00' + CAST(@Mes AS VARCHAR(2)), 2),
            @FechaInicio    DATE = DATEFROMPARTS(@Anio, @Mes, 1),
            @FechaFin       DATE = EOMONTH(DATEFROMPARTS(@Anio, @Mes, 1));

    -- Validar empresa
    IF NOT EXISTS (SELECT 1 FROM Empresas WHERE Id = @EmpresaId AND Activo = 1)
    BEGIN
        RAISERROR('Empresa no encontrada o inactiva.', 16, 1);
        RETURN;
    END

    -- Validar que no exista cierre mensual
    IF EXISTS (SELECT 1 FROM CierresFacturacion WHERE EmpresaId = @EmpresaId AND TipoCierre = 'Mensual' AND Periodo = @Periodo)
    BEGIN
        RAISERROR('Ya existe un cierre mensual para este período.', 16, 1);
        RETURN;
    END

    -- Validar que todos los días con facturas tengan cierre diario
    DECLARE @DiasConFacturas INT, @DiasConCierre INT;

    SELECT @DiasConFacturas = COUNT(DISTINCT CAST(FechaEmision AS DATE))
    FROM Facturas
    WHERE EmpresaId = @EmpresaId
      AND CAST(FechaEmision AS DATE) BETWEEN @FechaInicio AND @FechaFin;

    SELECT @DiasConCierre = COUNT(*)
    FROM CierresFacturacion
    WHERE EmpresaId = @EmpresaId
      AND TipoCierre = 'Diario'
      AND FechaCierre BETWEEN @FechaInicio AND @FechaFin;

    IF @DiasConFacturas > @DiasConCierre
    BEGIN
        RAISERROR('Faltan cierres diarios para completar el cierre mensual. Días con facturas: %d, Cierres diarios: %d.', 16, 1, @DiasConFacturas, @DiasConCierre);
        RETURN;
    END

    IF @DiasConFacturas = 0
    BEGIN
        RAISERROR('No hay facturas en este período.', 16, 1);
        RETURN;
    END

    -- Consolidar desde cierres diarios
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

    SELECT
        @TotalEmitidas  = SUM(TotalFacturasEmitidas),
        @TotalAnuladas  = SUM(TotalFacturasAnuladas),
        @SubTotal       = SUM(MontoSubTotal),
        @Exento         = SUM(MontoExento),
        @Exonerado      = SUM(MontoExonerado),
        @Gravado15      = SUM(MontoGravado15),
        @ISV15          = SUM(MontoISV15),
        @Descuentos     = SUM(MontoDescuentos),
        @Total          = SUM(MontoTotal),
        @FacturaInicial = MIN(NumeroFacturaInicial),
        @FacturaFinal   = MAX(NumeroFacturaFinal)
    FROM CierresFacturacion
    WHERE EmpresaId = @EmpresaId
      AND TipoCierre = 'Diario'
      AND FechaCierre BETWEEN @FechaInicio AND @FechaFin;

    INSERT INTO CierresFacturacion (EmpresaId, TipoCierre, FechaCierre, Periodo,
        TotalFacturasEmitidas, TotalFacturasAnuladas,
        MontoSubTotal, MontoExento, MontoExonerado, MontoGravado15, MontoISV15, MontoDescuentos, MontoTotal,
        NumeroFacturaInicial, NumeroFacturaFinal, FechaGeneracion)
    VALUES (@EmpresaId, 'Mensual', @FechaFin, @Periodo,
        @TotalEmitidas, @TotalAnuladas,
        @SubTotal, @Exento, @Exonerado, @Gravado15, @ISV15, @Descuentos, @Total,
        @FacturaInicial, @FacturaFinal, GETUTCDATE());

    SET @CierreId = SCOPE_IDENTITY();

    -- Retornar cierre generado
    SELECT * FROM CierresFacturacion WHERE Id = @CierreId;
END
GO
