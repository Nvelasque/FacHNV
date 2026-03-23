-- =============================================
-- Datos de prueba Multi-tenant
-- Dos empresas con datos completamente aislados
-- =============================================

USE FacturacionHN;
GO

-- =============================================
-- EMPRESA 1
-- =============================================
INSERT INTO Empresas (RazonSocial, RTN, NombreComercial, DireccionCasaMatriz, Telefono, Correo, ColorPrimario, ColorSecundario)
VALUES ('Tecnología Catracha S.A. de C.V.', '08019000123456', 'TecCatracha', 'Col. Palmira, Blvd. Morazán, Tegucigalpa', '2232-0000', 'facturacion@teccatracha.hn', '#1B5E20', '#E8F5E9');

-- =============================================
-- EMPRESA 2
-- =============================================
INSERT INTO Empresas (RazonSocial, RTN, NombreComercial, DireccionCasaMatriz, Telefono, Correo, ColorPrimario, ColorSecundario)
VALUES ('Comercial Sampedrana S.A.', '05019500654321', 'ComSampedrana', 'Barrio Guamilito, 3ra Avenida, San Pedro Sula', '2553-0000', 'facturacion@comsampedrana.hn', '#0D47A1', '#E3F2FD');
GO

-- Clientes de Empresa 1
INSERT INTO Clientes (EmpresaId, RTN, Nombre, Direccion, Correo, Telefono)
VALUES
    (1, '08011985123456', 'Distribuidora Honduras S.A.', 'Col. Palmira, Tegucigalpa', 'info@distribuidorahn.com', '2232-5678'),
    (1, '05021990654321', 'Comercial El Progreso', 'Barrio El Centro, San Pedro Sula', 'ventas@comercialep.com', '2553-1234'),
    (1, '01011980111111', 'Consumidor Final', NULL, NULL, NULL);

-- Clientes de Empresa 2
INSERT INTO Clientes (EmpresaId, RTN, Nombre, Direccion, Correo, Telefono)
VALUES
    (2, '08011985123456', 'Importadora del Norte', 'Col. Los Andes, San Pedro Sula', 'compras@importnorte.hn', '2557-1111'),
    (2, '07031992222222', 'Ferretería La Ceiba', 'Barrio La Isla, La Ceiba', 'ventas@ferrceiba.hn', '2443-2222');
GO

-- Productos de Empresa 1
INSERT INTO Productos (EmpresaId, Codigo, Descripcion, Precio, GravadoISV)
VALUES
    (1, 'PROD-001', 'Laptop HP 15 pulgadas',          18500.00, 1),
    (1, 'PROD-002', 'Mouse inalámbrico Logitech',       450.00, 1),
    (1, 'PROD-003', 'Teclado mecánico RGB',              850.00, 1),
    (1, 'PROD-004', 'Cable HDMI 2m',                     150.00, 1),
    (1, 'PROD-005', 'Servicio de consultoría (exento)', 5000.00, 0);

-- Productos de Empresa 2
INSERT INTO Productos (EmpresaId, Codigo, Descripcion, Precio, GravadoISV)
VALUES
    (2, 'FER-001', 'Martillo Stanley',       250.00, 1),
    (2, 'FER-002', 'Taladro Bosch 500W',    3200.00, 1),
    (2, 'FER-003', 'Cemento Bijao (bolsa)',   220.00, 1);
GO

-- CAI de Empresa 1
INSERT INTO CAIs (EmpresaId, NumeroCai, RangoInicial, RangoFinal, CorrelativoActual, FechaLimiteEmision, SucursalCodigo, PuntoEmisionCodigo, TipoDocumento)
VALUES (1, 'A1B2C3-D4E5F6-G7H8I9-J0K1L2-M3N4O5-P6', '000-001-01-00000001', '000-001-01-00500000', 0, '2027-12-31', '000', '001', '01');

-- CAI de Empresa 2
INSERT INTO CAIs (EmpresaId, NumeroCai, RangoInicial, RangoFinal, CorrelativoActual, FechaLimiteEmision, SucursalCodigo, PuntoEmisionCodigo, TipoDocumento)
VALUES (2, 'X9Y8Z7-W6V5U4-T3S2R1-Q0P9O8-N7M6L5-K4', '001-001-01-00000001', '001-001-01-00500000', 0, '2027-12-31', '001', '001', '01');
GO

PRINT 'Datos de prueba multi-tenant insertados correctamente.';
GO
