-- =============================================
-- Migración: Agregar campos de personalización visual a Empresas
-- Ejecutar sobre base de datos existente
-- =============================================

USE FacturacionHN;
GO

-- Agregar ColorPrimario
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Empresas') AND name = 'ColorPrimario')
BEGIN
    ALTER TABLE Empresas ADD ColorPrimario NVARCHAR(7) NOT NULL DEFAULT '#1B5E20';
    PRINT 'Columna ColorPrimario agregada.';
END
GO

-- Agregar ColorSecundario
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Empresas') AND name = 'ColorSecundario')
BEGIN
    ALTER TABLE Empresas ADD ColorSecundario NVARCHAR(7) NOT NULL DEFAULT '#E8F5E9';
    PRINT 'Columna ColorSecundario agregada.';
END
GO

-- Agregar LogoUrl
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Empresas') AND name = 'LogoUrl')
BEGIN
    ALTER TABLE Empresas ADD LogoUrl NVARCHAR(500) NULL;
    PRINT 'Columna LogoUrl agregada.';
END
GO

-- Asignar colores diferentes a cada empresa existente
UPDATE Empresas SET ColorPrimario = '#1B5E20', ColorSecundario = '#E8F5E9' WHERE Id = 1;
UPDATE Empresas SET ColorPrimario = '#0D47A1', ColorSecundario = '#E3F2FD' WHERE Id = 2;
GO

PRINT 'Migración completada.';
GO
