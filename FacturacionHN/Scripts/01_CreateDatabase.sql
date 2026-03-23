-- =============================================
-- Script de creación de Base de Datos
-- Sistema de Facturación Honduras (SAR)
-- =============================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'FacturacionHN')
BEGIN
    CREATE DATABASE FacturacionHN;
END
GO

USE FacturacionHN;
GO
