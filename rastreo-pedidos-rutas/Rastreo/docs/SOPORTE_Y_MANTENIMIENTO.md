# Manual de Operaciones y Soporte

## 1. Infraestructura y Configuracion

El correcto funcionamiento de la aplicacion depende estrictamente de la configuracion del archivo Web.config y de las dependencias externas integradas en el servidor web IIS.

### Variables criticas requeridas en el Web.config:

#### Cadenas de Conexion (connectionStrings):
- **ConexionSAPSQL:** Establece la conexion con el motor de base de datos de SAP Business One (SQL Server). Es utilizada para consultas de lectura de rutas y pedidos mediante procedimientos almacenados.
- **ConexionSAPDI:** Cadena de conexion estructurada para la inicializacion de la DI API (SAPbobsCOM). Requiere definicion de parametros del servidor SQL, base de datos de la compañia, credenciales de base de datos y credenciales de usuario SAP (manager).
- **dbCRM:** Conexion directa a la base de datos de control local de pedidos YOUR_DATABASE.

#### Configuraciones de Aplicacion (appSettings):
- **Server / DataBase / UserId / Password:** Credenciales duplicadas para la conexion a la base de datos del negocio.
- **RptPath:** Ruta fisica absoluta del servidor donde residen las plantillas de reportes de Crystal Reports (ejemplo: `C:\CRM\Reportes\`).
- **RutaLog:** Ubicacion fisica absoluta del archivo de logs general (ejemplo: `C:\RastreoLog\Log.txt`).
- **RutaFullLog:** Carpeta destinada para la escritura de logs detallados del proceso de integracion SAP.
- **RutaContenedor / RutaContenedorR:** Rutas del sistema de archivos requeridas para el almacenamiento temporal de archivos de transferencias y entregas.

### Dependencias de servidor y middleware:
- **Internet Information Services (IIS):** Requiere un pool de aplicaciones configurado en .NET CLR Version v4.0 con arquitectura de 32 bits (x86) habilitada si la version instalada de la DI API de SAP es de 32 bits.
- **OWIN Middleware (Microsoft.Owin):** Administra el pipeline de autenticacion basada en cookies (`CookieAuthenticationOptions`) con un tiempo de expiracion absoluto en servidor de 1 hora.
- **ASP.NET Identity:** Gestiona los claims de usuario autenticado contra los datos de empleados de SAP obtenidos mediante base de datos.
- **SAP Business One DI API (SAPbobsCOM.dll):** Dependencia de codigo no administrado (COM) instalada y registrada en el servidor IIS. Requiere conectividad directa y sin bloqueos al License Server en el puerto TCP 40000.
- **Crystal Reports Runtime:** Es mandatario contar con la version del runtime compatible con la arquitectura del IIS AppPool para compilar y generar los tickets en PDF de manera local.

## 2. Diagnostico de Fallas (Troubleshooting)

### Flujo de excepciones:
- **Control Global de Errores:** La aplicacion delega la interceptacion basica al filtro global `HandleErrorAttribute` registrado en `FilterConfig.cs`. No existe un manejador global centralizado en `Global.asax.cs` para excepciones no atrapadas.
- **Manejador Custom (ExceptionUtility):** Los metodos estaticos `ExceptionUtility.LogException` y `ExceptionUtility.LogMessageDIAPI` capturan y persisten excepciones directamente al archivo plano definido en la llave `RutaLog`. Captura el tipo de excepcion interna, stack trace, origen y mensajes especificos del motor DI API de SAP.
- **Monitoreo de Procesos (Logs):** La clase `Rastreo.Utilities.Logs` escribe en tiempo de ejecucion las trazas log de niveles DEBUG, INFO, WARNING y ERROR usando bloques `StreamWriter` concurrentes.

### Matriz de Errores Comunes:

| Sintoma | Causa Raiz | Accion Correctiva |
| :--- | :--- | :--- |
| Error DI-API en integracion al generar entrega (Rollback transaccional). | El SAP License Server esta inaccesible, o las credenciales especificadas en ConexionSAPDI son incorrectas o estan bloqueadas. | Validar que el puerto TCP 40000 este abierto hacia el servidor de licencias. Verificar vigencia de la contraseña del usuario SAP configurado. |
| Crash del Pool de Aplicaciones (W3WP) o incremento sostenido y critico de consumo de RAM. | Fuga de memoria por falta de liberacion de objetos COM de SAP (`Marshal.ReleaseComObject`) o instancias no liberadas de Crystal Reports (`ReportDocument`). | Monitorear el Pool. Verificar en el codigo fuente que todo objeto COM de SAP y objeto de reporte este contenido en bloques `finally` con metodos de liberacion y desecho explicitos. |
| Redireccion automatica y repetitiva a la pantalla de Login tras loguearse con exito. | Ocurre una excepcion no manejada dentro de la sesion activa. Los controladores principales (`RutasController`, `PedidoController`) tienen catches que fuerzan el `SignOut` inmediato ante fallos. | Revisar inmediatamente el archivo log configurado en `RutaLog` para identificar la excepcion exacta que dispara el cierre de sesion forzado. |
| Falla al cargar reportes / tickets PDF. | La ruta especificada en `RptPath` o `RutaReporteOrdenEntrega` es incorrecta o la cuenta del IIS AppPool no tiene privilegios de lectura sobre el directorio fisico de reportes. | Validar la existencia fisica del archivo `.rpt` correspondiente y otorgar permisos de lectura al usuario del Pool de Aplicaciones (ej. `IIS_IUSRS`). |
| Bloqueo transaccional de Base de Datos SQL (Deadlocks o Timeouts). | Falta de liberacion de conexiones SQL o de cursores abiertos en procedimientos almacenados personalizados de control logistico. | Forzar la terminacion de procesos huerfanos ejecutando la herramienta utilitaria `SQLCancelSession.exe` incluida en el root del proyecto. |

## 3. Seguridad Operacional

El sistema implementa medidas estrictas para mitigar el uso de estados inconsistentes y proteger los datos transaccionales contra accesos no autorizados:

- **Caducidad Automatica de Sesion (OWIN):** Administrada en el servidor por cookies de autenticacion de OWIN con duracion configurada de 1 hora.
- **Cierre Automatico por Inactividad en Cliente:** Se fuerza un cierre de sesion incondicional mediante JavaScript en `Menu.cshtml` despues de 5 horas continuas (18,000,000 milisegundos), redirigiendo al endpoint `/Login/Logout`.
- **Cierres Forzados ante Excepciones:** Para evitar que un usuario opere bajo un estado inconsistente de base de datos o de sesion tras ocurrir un fallo, los controladores principales implementan una directiva catch estricta: ante cualquier error capturado, se ejecuta incondicionalmente el metodo `HttpContext.GetOwinContext().Authentication.SignOut()` destruyendo los tokens del pipeline de OWIN y redirigiendo al usuario a la pantalla de login.

## 4. Monitoreo

Para garantizar la alta disponibilidad del sistema, el equipo de operaciones debe establecer alertas basadas en los siguientes puntos criticos:

- **Fugas de Objetos COM de SAP:** Asegurar que toda instancia de objeto SAP (`Company`, `Documents`, `Items`) sea liberada en memoria mediante `Marshal.ReleaseComObject` en el bloque `finally`. El crecimiento no controlado del consumo de memoria no administrada por el proceso IIS es indicativo de objetos huerfanos.
- **Fugas de Recursos de Crystal Reports:** El motor de Crystal Reports (`ReportDocument`) es propenso a saturar la memoria virtual. Es mandatorio validar que los metodos `reporte.Close()` y `reporte.Dispose()` se ejecuten de manera determinista en todos los endpoints que exportan PDFs (por ejemplo en `PedidoController.GenerarTicketEntrega`).
- **Agotamiento del Pool de Conexiones SQL:** Validar a nivel de base de datos las conexiones inactivas originadas por el sitio web. Se debe comprobar que todo bloque de acceso a datos que use `SqlConnection` emplee sentencias `using` para retornar las conexiones al Pool del backend de .NET de forma inmediata.
