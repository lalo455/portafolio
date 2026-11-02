# [RastreoWeb]

## 1. Resumen Funcional

El sistema es una solucion web para la gestion logistica, asignacion y rastreo de rutas de entrega de productos  y su integracion transaccional en tiempo real con el sistema ERP SAP Business One. Permite automatizar el ciclo de distribucion desde la asignacion de la carga hasta la liquidacion final en caja.

### Modulos principales detectados en el codigo:
- **Modulo de Autenticacion y Acceso (LoginController):** Manejo de sesiones y cookies de aplicacion mediante OWIN Middleware y ASP.NET Identity, con validacion de credenciales contra base de datos a traves de procedimientos almacenados.
- **Modulo de Rutas y Logistica (RutasController):** Panel de administracion para la asignacion, control y consulta de rutas de entrega asignadas a los choferes o repartidores.
- **Modulo de Pedidos y Entregas (PedidoController, LogicaPedidoController):** Interfaz para consultar los detalles de pedidos, modificar cantidades de entrega en sitio y registrar la confirmacion del servicio.
- **Modulo de Integracion SAP (IntegracionSAPController):** Sincronizacion directa mediante DI API (SAPbobsCOM). Se encarga de la creacion automatica de Ordenes de Entrega (Delivery Notes) y de la actualizacion o cierre de los pedidos de venta (Sales Orders) en la base de datos de SAP Business One bajo control transaccional (Commit / Rollback).
- **Modulo de Corte de Caja (CorteCajaController, CorteDeCajaController):** Conciliacion financiera de los montos cobrados en ruta al finalizar la jornada de entregas.
- **Modulo de Impresion y Reportes (ImpresionController, ReportesController):** Generacion y descarga en formato PDF de tickets de entrega y formatos de corte de caja a traves de la integracion con Crystal Reports.

### Tipos de usuarios o roles:
- **Administrador (Rol A):** Acceso completo al panel de rutas para gestionar la asignacion y control logistico de los pedidos de venta.
- **Repartidor (Rol R):** Acceso limitado desde dispositivos de entrega para visualizar pedidos asignados, confirmar cantidades entregadas, imprimir tickets y registrar el corte de caja.

## 2. Stack Tecnologico

- **Version de .NET Framework:** .NET Framework 4.8.1
- **Base de Datos:** Microsoft SQL Server (Database: YOUR_DATABASE), conectado mediante System.Data.SqlClient y SAP Business One.
- **Librerias Backend Clave:**
  - Entity Framework 6.5.1 (Mapeo objeto-relacional y acceso a datos)
  - SAPbobsCOM - SAP Business One DI API (Integracion y creacion de documentos en el ERP)
  - Microsoft.AspNet.Identity.Core 2.2.4 (Gestion de identidad de usuario)
  - Microsoft.Owin.Security.Cookies 4.2.2 (Manejo de sesion persistente mediante cookies)
  - FluentValidation 11.11.0 (Validaciones de modelos y credenciales de entrada)
  - Crystal Reports - CrystalDecisions.CrystalReports.Engine (Motor para la generacion de reportes PDF)
  - Newtonsoft.Json 13.0.3 (Serializacion y deserializacion de objetos de negocio)
- **Librerias Frontend Clave:**
  - AngularJS 1.x (Estructura de aplicacion y bindings en vistas)
  - Bootstrap 5.3.3 (Hojas de estilo y diseño responsivo)
  - jQuery 3.7.1
  - jsPDF (Generacion dinamica de documentos PDF en el navegador)
  - FontAwesome 6.0.0 (Iconos de interfaz)

## 3. Entorno de Desarrollo (Getting Started)

Siga las siguientes instrucciones para configurar y compilar la aplicacion de manera local:

### 1. Restaurar Paquetes NuGet
Abra la solucion en Visual Studio o ejecute el siguiente comando desde la Consola de Administrador de Paquetes NuGet para descargar las dependencias listadas en packages.config:
```bash
nuget restore Rastreo.sln
```

### 2. Configuracion de Web.config Local
Configure las credenciales e inicializadores en el archivo Web.config en la raiz del proyecto:
- **Cadenas de conexion (connectionStrings):**
  - **ConexionSAPSQL:** Cadena de conexion al servidor de base de datos SQL Server del ERP.
  - **ConexionSAPDI:** Parametros especificos para la conexion a la DI API de SAP Business One (LicenseServer, CompanyDB, DbServerType, UserName, Password).
  - **dbCRM:** Conexion directa a la base de datos de control local de pedidos YOUR_DATABASE.
- **Parametros de aplicacion (appSettings):** Ensure local physical folders exist for the following keys:
  - **RptPath:** Ruta fisica donde se ubican las plantillas .rpt de Crystal Reports (ejemplo: `C:\CRM\Reportes\`).
  - **RutaLog:** Archivo de registro de errores del sistema (ejemplo: `C:\RastreoLog\Log.txt`).
  - **RutaFullLog:** Directorio para almacenamiento de logs extensos.
  - **RutaContenedor / RutaContenedorR:** Directorios locales para la transferencia y procesamiento de entregas.

### 3. IIS Express / Ejecucion Local
- Establezca el proyecto Rastreo como proyecto de inicio en Visual Studio.
- Seleccione el servidor de ejecucion **IIS Express**.
- Presione **F5** o seleccione Depurar para iniciar la compilacion y desplegar el sitio localmente. El navegador abrira de manera automatica el puerto asignado en el archivo Rastreo.csproj.

## 4. Ecosistema de Documentacion

- [SOPORTE_Y_MANTENIMIENTO.md](./docs/SOPORTE_Y_MANTENIMIENTO.md) - Operaciones, IIS y Troubleshooting.
- [DESARROLLO_Y_ARQUITECTURA.md](./docs/DESARROLLO_Y_ARQUITECTURA.md) - Arquitectura, Controladores y Frontend.
