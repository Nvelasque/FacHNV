# 🇭🇳 API de Facturación Honduras (SAR)

API REST desarrollada en .NET 9 para gestión de facturación fiscal conforme al Reglamento del SAR de Honduras.

## Características

- Multi-tenant: múltiples empresas aisladas en una sola instancia
- Numeración correlativa SAR de 16 dígitos (NNN-NNN-NN-NNNNNNNN)
- Gestión de CAI con control de rangos y vencimiento
- Cálculo automático de ISV 15% por producto (gravado/exento)
- Soporte para clientes exonerados (Art. 10 y 11 del Reglamento SAR)
- Modalidades: Autoimpresor e Imprenta
- Generación de PDF con colores personalizados por empresa
- Stored Procedures para operaciones críticas (creación y anulación)
- Cierres de facturación diarios y mensuales
- Colección de Postman incluida para pruebas

## Requisitos

- .NET 9 SDK
- SQL Server 2019+ (o SQL Server Express)
- Visual Studio 2022+ o VS Code

## Instalación

### 1. Clonar el repositorio

```bash
git clone https://github.com/Nvelasque/FacHNV.git
cd FacHNV/FacturacionHN
```

### 2. Configurar la base de datos

Ejecutar los scripts SQL en orden desde SQL Server Management Studio (SSMS):

```
Scripts/01_CreateDatabase.sql    -- Crea la base de datos FacturacionHN
Scripts/02_CreateTables.sql      -- Crea todas las tablas con índices y FKs
Scripts/03_SeedData.sql          -- Inserta datos de prueba (2 empresas)
Scripts/04_SP_CrearFactura.sql   -- SP para crear facturas
Scripts/05_SP_AnularFactura.sql  -- SP para anular facturas
Scripts/06_SP_CierreDiario.sql   -- SP para cierre diario
Scripts/07_SP_CierreMensual.sql  -- SP para cierre mensual
Scripts/08_Migracion_ColoresEmpresa.sql -- Agrega campos de color a Empresas
```

### 3. Configurar el connection string

Crear el archivo `appsettings.Development.json` (no se sube a git):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=FacturacionHN;User Id=TU_USUARIO;Password=TU_PASSWORD;TrustServerCertificate=true"
  }
}
```

### 4. Ejecutar la aplicación

```bash
dotnet run
```

O desde Visual Studio: F5 (perfil `https`).

La API estará disponible en: `https://localhost:7063/swagger`

## Guía de Uso - Endpoints

Todas las rutas de facturación son multi-tenant y requieren el `empresaId` en la URL.

### Empresas

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/empresas` | Listar todas las empresas |
| GET | `/api/empresas/{id}` | Obtener empresa por ID |
| POST | `/api/empresas` | Crear empresa |
| PUT | `/api/empresas/{id}` | Actualizar empresa |

**Crear empresa:**
```json
POST /api/empresas
{
  "razonSocial": "Mi Empresa S.A. de C.V.",
  "rtn": "08019000999999",
  "nombreComercial": "MiEmpresa",
  "direccionCasaMatriz": "Col. Kennedy, Tegucigalpa",
  "telefono": "2232-1111",
  "correo": "info@miempresa.hn",
  "colorPrimario": "#1B5E20",
  "colorSecundario": "#E8F5E9"
}
```

### Clientes

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/empresas/{empresaId}/clientes` | Listar clientes |
| GET | `/api/empresas/{empresaId}/clientes/{id}` | Obtener cliente |
| POST | `/api/empresas/{empresaId}/clientes` | Crear cliente |

**Crear cliente:**
```json
POST /api/empresas/1/clientes
{
  "rtn": "08011990123456",
  "nombre": "Cliente Ejemplo S.A.",
  "direccion": "Barrio El Centro, SPS",
  "correo": "cliente@ejemplo.com",
  "telefono": "2553-0000"
}
```

### Productos

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/empresas/{empresaId}/productos` | Listar productos |
| GET | `/api/empresas/{empresaId}/productos/{id}` | Obtener producto |
| POST | `/api/empresas/{empresaId}/productos` | Crear producto |

**Crear producto:**
```json
POST /api/empresas/1/productos
{
  "codigo": "PROD-010",
  "descripcion": "Monitor 24 pulgadas",
  "precio": 5500.00,
  "gravadoISV": true
}
```

> `gravadoISV: true` = se aplica ISV 15%. `gravadoISV: false` = exento de ISV.

### CAIs (Clave de Autorización de Impresión)

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/empresas/{empresaId}/cais` | Listar CAIs |
| POST | `/api/empresas/{empresaId}/cais` | Crear CAI |

**Crear CAI:**
```json
POST /api/empresas/1/cais
{
  "numeroCai": "A1B2C3-NUEVO-CAI-EJEMPLO-123456-Z9",
  "rangoInicial": "000-001-01-00000001",
  "rangoFinal": "000-001-01-00500000",
  "fechaLimiteEmision": "2027-12-31",
  "sucursalCodigo": "000",
  "puntoEmisionCodigo": "001",
  "tipoDocumento": "01"
}
```

> Cuando un CAI se agota o vence, desactivarlo (`activo: false`) y crear uno nuevo.

### Facturas

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/empresas/{empresaId}/facturas` | Listar facturas |
| GET | `/api/empresas/{empresaId}/facturas/{id}` | Obtener factura |
| POST | `/api/empresas/{empresaId}/facturas` | Crear factura (via SP) |
| PUT | `/api/empresas/{empresaId}/facturas/{id}/anular` | Anular factura (via SP) |
| GET | `/api/empresas/{empresaId}/facturas/{id}/pdf` | Descargar PDF |

**Crear factura normal (con ISV):**
```json
POST /api/empresas/1/facturas
{
  "empresaId": 1,
  "clienteId": 1,
  "modalidad": "Autoimpresor",
  "detalles": [
    { "productoId": 1, "cantidad": 2, "descuento": 0 },
    { "productoId": 2, "cantidad": 3, "descuento": 50 }
  ]
}
```

**Crear factura con cliente exonerado:**
```json
POST /api/empresas/1/facturas
{
  "empresaId": 1,
  "clienteId": 1,
  "modalidad": "Autoimpresor",
  "numeroOrdenCompraExenta": "OCE-2026-001",
  "numeroConstanciaRegistroExonerados": "CRE-2026-001",
  "numeroRegistroSAG": "SAG-2026-001",
  "detalles": [
    { "productoId": 1, "cantidad": 1, "descuento": 0 }
  ]
}
```

> Cuando se incluye `numeroOrdenCompraExenta`, el ISV se calcula en 0 para todos los productos.

**Anular factura:**
```
PUT /api/empresas/1/facturas/1/anular
```

> No se puede anular si ya existe un cierre diario para esa fecha.

**Descargar PDF:**
```
GET /api/empresas/1/facturas/1/pdf
```

### Cierres de Facturación

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/empresas/{empresaId}/facturas/cierres/diario` | Cierre diario |
| POST | `/api/empresas/{empresaId}/facturas/cierres/mensual` | Cierre mensual |
| GET | `/api/empresas/{empresaId}/facturas/cierres?tipo=Diario` | Listar cierres |

**Cierre diario:**
```json
POST /api/empresas/1/facturas/cierres/diario
{
  "fecha": "2026-03-23"
}
```

**Cierre mensual:**
```json
POST /api/empresas/1/facturas/cierres/mensual
{
  "anio": 2026,
  "mes": 3
}
```

> El cierre mensual requiere que todos los días con facturas del mes tengan cierre diario.

## Flujo Completo de Uso

1. **Crear empresa** con datos del emisor (razón social, RTN, dirección, colores)
2. **Crear CAI** asignado por el SAR con rango de numeración
3. **Crear clientes** con RTN
4. **Crear productos** indicando si son gravados (`gravadoISV: true`) o exentos
5. **Crear facturas** → el sistema asigna número correlativo automáticamente
6. **Descargar PDF** de la factura generada
7. **Cierre diario** al final del día
8. **Cierre mensual** al final del mes (consolida los cierres diarios)

## Estructura del Proyecto

```
FacturacionHN/
├── Controllers/          # Controladores REST
├── Data/                 # DbContext (Entity Framework Core)
├── DTOs/                 # Data Transfer Objects
├── Models/               # Entidades del dominio
├── Services/             # Lógica de negocio
├── Scripts/              # Scripts SQL (tablas, SPs, migraciones)
├── Postman/              # Colección de Postman para pruebas
└── Program.cs            # Configuración de la aplicación
```

## Tecnologías

- .NET 9 / ASP.NET Core Web API
- Entity Framework Core 9 (SQL Server)
- QuestPDF (generación de facturas en PDF)
- Stored Procedures para operaciones críticas
- Swagger/OpenAPI para documentación interactiva

## Postman

Importar el archivo `Postman/FacturacionHN.postman_collection.json` en Postman.
La colección incluye todos los endpoints organizados por módulo con ejemplos listos para usar.

## Licencia

MIT
