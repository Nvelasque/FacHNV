-- =============================================
-- Tabla: AutorizacionesReproceso
-- Control gerencial para reabrir cierres
-- =============================================

USE FacturacionHN;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AutorizacionesReproceso')
BEGIN
    CREATE TABLE AutorizacionesReproceso (
        Id                      INT IDENTITY(1,1) PRIMARY KEY,
        EmpresaId               INT             NOT NULL,
        TipoCierre              NVARCHAR(10)    NOT NULL,
        FechaCierre             DATE            NOT NULL,
        Periodo                 NVARCHAR(7)     NULL,
        Motivo                  NVARCHAR(500)   NOT NULL,
        SolicitadoPor           NVARCHAR(100)   NOT NULL,
        FechaSolicitud          DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        Estado                  NVARCHAR(15)    NOT NULL DEFAULT 'Pendiente',
        AprobadoPor             NVARCHAR(100)   NULL,
        FechaResolucion         DATETIME2       NULL,
        ObservacionResolucion   NVARCHAR(500)   NULL,
        CodigoAutorizacion      NVARCHAR(50)    NULL,
        Utilizada               BIT             NOT NULL DEFAULT 0,

        CONSTRAINT FK_Autorizaciones_Empresas FOREIGN KEY (EmpresaId)
            REFERENCES Empresas(Id) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX IX_Auth_CodigoAutorizacion
        ON AutorizacionesReproceso(CodigoAutorizacion)
        WHERE CodigoAutorizacion IS NOT NULL;

    CREATE INDEX IX_Auth_EmpresaId ON AutorizacionesReproceso(EmpresaId);
    CREATE INDEX IX_Auth_Estado ON AutorizacionesReproceso(Estado);

    PRINT 'Tabla AutorizacionesReproceso creada.';
END
GO
