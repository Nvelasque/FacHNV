-- =============================================
-- Tablas del Sistema de Facturación Honduras (SAR)
-- Multi-tenant: cada empresa tiene datos aislados
-- =============================================

USE FacturacionHN;
GO

-- =============================================
-- Tabla: Empresas (Datos del Emisor - Art. 10)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Empresas')
BEGIN
    CREATE TABLE Empresas (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        RazonSocial         NVARCHAR(200)   NOT NULL,
        RTN                 NVARCHAR(14)    NOT NULL,
        NombreComercial     NVARCHAR(200)   NULL,
        DireccionCasaMatriz NVARCHAR(300)   NOT NULL,
        Telefono            NVARCHAR(20)    NULL,
        Correo              NVARCHAR(100)   NULL,
        Activo              BIT             NOT NULL DEFAULT 1,
        ColorPrimario       NVARCHAR(7)     NOT NULL DEFAULT '#1B5E20',
        ColorSecundario     NVARCHAR(7)     NOT NULL DEFAULT '#E8F5E9',
        LogoUrl             NVARCHAR(500)   NULL
    );

    CREATE UNIQUE INDEX IX_Empresas_RTN ON Empresas(RTN);
END
GO

-- =============================================
-- Tabla: Clientes (aislados por EmpresaId)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Clientes')
BEGIN
    CREATE TABLE Clientes (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        EmpresaId       INT             NOT NULL,
        RTN             NVARCHAR(14)    NOT NULL,
        Nombre          NVARCHAR(200)   NOT NULL,
        Direccion       NVARCHAR(300)   NULL,
        Correo          NVARCHAR(100)   NULL,
        Telefono        NVARCHAR(20)    NULL,
        Activo          BIT             NOT NULL DEFAULT 1,
        FechaCreacion   DATETIME2       NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT FK_Clientes_Empresas FOREIGN KEY (EmpresaId)
            REFERENCES Empresas(Id) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX IX_Clientes_Empresa_RTN ON Clientes(EmpresaId, RTN);
    CREATE INDEX IX_Clientes_EmpresaId ON Clientes(EmpresaId);
END
GO

-- =============================================
-- Tabla: Productos (aislados por EmpresaId)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Productos')
BEGIN
    CREATE TABLE Productos (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        EmpresaId       INT             NOT NULL,
        Codigo          NVARCHAR(50)    NOT NULL,
        Descripcion     NVARCHAR(200)   NOT NULL,
        Precio          DECIMAL(18,2)   NOT NULL,
        GravadoISV      BIT             NOT NULL DEFAULT 1,
        Activo          BIT             NOT NULL DEFAULT 1,
        FechaCreacion   DATETIME2       NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT FK_Productos_Empresas FOREIGN KEY (EmpresaId)
            REFERENCES Empresas(Id) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX IX_Productos_Empresa_Codigo ON Productos(EmpresaId, Codigo);
    CREATE INDEX IX_Productos_EmpresaId ON Productos(EmpresaId);
END
GO

-- =============================================
-- Tabla: CAIs (aislados por EmpresaId)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CAIs')
BEGIN
    CREATE TABLE CAIs (
        Id                      INT IDENTITY(1,1) PRIMARY KEY,
        EmpresaId               INT             NOT NULL,
        NumeroCai               NVARCHAR(37)    NOT NULL,
        RangoInicial            NVARCHAR(20)    NOT NULL,
        RangoFinal              NVARCHAR(20)    NOT NULL,
        CorrelativoActual       INT             NOT NULL DEFAULT 0,
        FechaLimiteEmision      DATETIME2       NOT NULL,
        SucursalCodigo          NVARCHAR(20)    NOT NULL,
        PuntoEmisionCodigo      NVARCHAR(20)    NOT NULL,
        TipoDocumento           NVARCHAR(2)     NOT NULL DEFAULT '01',
        Activo                  BIT             NOT NULL DEFAULT 1,
        FechaCreacion           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT FK_CAIs_Empresas FOREIGN KEY (EmpresaId)
            REFERENCES Empresas(Id) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX IX_CAIs_NumeroCai ON CAIs(NumeroCai);
    CREATE INDEX IX_CAIs_EmpresaId ON CAIs(EmpresaId);
END
GO

-- =============================================
-- Tabla: Facturas (aisladas por EmpresaId)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Facturas')
BEGIN
    CREATE TABLE Facturas (
        Id                                  INT IDENTITY(1,1) PRIMARY KEY,
        EmpresaId                           INT             NOT NULL,
        NumeroFactura                       NVARCHAR(20)    NOT NULL,
        CAIId                               INT             NOT NULL,
        ClienteId                           INT             NOT NULL,
        FechaEmision                        DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        Modalidad                           NVARCHAR(20)    NOT NULL DEFAULT 'Autoimpresor',
        SubTotal                            DECIMAL(18,2)   NOT NULL DEFAULT 0,
        ImporteExento                       DECIMAL(18,2)   NOT NULL DEFAULT 0,
        ImporteExonerado                    DECIMAL(18,2)   NOT NULL DEFAULT 0,
        ImporteGravado15                    DECIMAL(18,2)   NOT NULL DEFAULT 0,
        ISV15                               DECIMAL(18,2)   NOT NULL DEFAULT 0,
        Descuento                           DECIMAL(18,2)   NOT NULL DEFAULT 0,
        Total                               DECIMAL(18,2)   NOT NULL DEFAULT 0,
        Estado                              NVARCHAR(20)    NOT NULL DEFAULT 'Emitida',
        NumeroOrdenCompraExenta             NVARCHAR(50)    NULL,
        NumeroConstanciaRegistroExonerados  NVARCHAR(50)    NULL,
        NumeroRegistroSAG                   NVARCHAR(50)    NULL,

        CONSTRAINT FK_Facturas_Empresas FOREIGN KEY (EmpresaId)
            REFERENCES Empresas(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Facturas_CAIs FOREIGN KEY (CAIId)
            REFERENCES CAIs(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Facturas_Clientes FOREIGN KEY (ClienteId)
            REFERENCES Clientes(Id) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX IX_Facturas_NumeroFactura ON Facturas(NumeroFactura);
    CREATE INDEX IX_Facturas_EmpresaId ON Facturas(EmpresaId);
    CREATE INDEX IX_Facturas_CAIId ON Facturas(CAIId);
    CREATE INDEX IX_Facturas_ClienteId ON Facturas(ClienteId);
    CREATE INDEX IX_Facturas_FechaEmision ON Facturas(FechaEmision);
END
GO

-- =============================================
-- Tabla: DetalleFacturas
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DetalleFacturas')
BEGIN
    CREATE TABLE DetalleFacturas (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        FacturaId           INT             NOT NULL,
        ProductoId          INT             NOT NULL,
        Cantidad            INT             NOT NULL,
        PrecioUnitario      DECIMAL(18,2)   NOT NULL,
        Descuento           DECIMAL(18,2)   NOT NULL DEFAULT 0,
        SubTotal            DECIMAL(18,2)   NOT NULL DEFAULT 0,
        ISV                 DECIMAL(18,2)   NOT NULL DEFAULT 0,
        Total               DECIMAL(18,2)   NOT NULL DEFAULT 0,

        CONSTRAINT FK_DetalleFacturas_Facturas FOREIGN KEY (FacturaId)
            REFERENCES Facturas(Id) ON DELETE CASCADE,
        CONSTRAINT FK_DetalleFacturas_Productos FOREIGN KEY (ProductoId)
            REFERENCES Productos(Id) ON DELETE NO ACTION
    );

    CREATE INDEX IX_DetalleFacturas_FacturaId ON DetalleFacturas(FacturaId);
    CREATE INDEX IX_DetalleFacturas_ProductoId ON DetalleFacturas(ProductoId);
END
GO

-- =============================================
-- Tabla: CierresFacturacion
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CierresFacturacion')
BEGIN
    CREATE TABLE CierresFacturacion (
        Id                      INT IDENTITY(1,1) PRIMARY KEY,
        EmpresaId               INT             NOT NULL,
        TipoCierre              NVARCHAR(10)    NOT NULL, -- Diario, Mensual
        FechaCierre             DATE            NOT NULL,
        Periodo                 NVARCHAR(7)     NULL,     -- YYYY-MM para mensual
        TotalFacturasEmitidas   INT             NOT NULL DEFAULT 0,
        TotalFacturasAnuladas   INT             NOT NULL DEFAULT 0,
        MontoSubTotal           DECIMAL(18,2)   NOT NULL DEFAULT 0,
        MontoExento             DECIMAL(18,2)   NOT NULL DEFAULT 0,
        MontoExonerado          DECIMAL(18,2)   NOT NULL DEFAULT 0,
        MontoGravado15          DECIMAL(18,2)   NOT NULL DEFAULT 0,
        MontoISV15              DECIMAL(18,2)   NOT NULL DEFAULT 0,
        MontoDescuentos         DECIMAL(18,2)   NOT NULL DEFAULT 0,
        MontoTotal              DECIMAL(18,2)   NOT NULL DEFAULT 0,
        NumeroFacturaInicial    NVARCHAR(20)    NULL,
        NumeroFacturaFinal      NVARCHAR(20)    NULL,
        FechaGeneracion         DATETIME2       NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT FK_Cierres_Empresas FOREIGN KEY (EmpresaId)
            REFERENCES Empresas(Id) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX IX_Cierres_Empresa_Tipo_Fecha ON CierresFacturacion(EmpresaId, TipoCierre, FechaCierre);
    CREATE INDEX IX_Cierres_EmpresaId ON CierresFacturacion(EmpresaId);
END
GO
