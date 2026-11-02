# Guia de Arquitectura y Codigo Fuente

Esta guia proporciona los lineamientos tecnicos y de arquitectura que rigen el diseño y desarrollo de la aplicacion. Esta destinada a capacitar a nuevos desarrolladores sobre la estructura del codebase y sus estandares de implementacion.

## 1. Organizacion de Capas

El proyecto esta estructurado como una aplicacion monolítica bajo ASP.NET MVC en .NET Framework. Aunque reside en un unico proyecto fisico (`Rastreo.csproj`), las capas se encuentran delimitadas logicamente mediante directorios y namespaces especificos:

### Capa de Presentacion (Views & Scripts)
- **Vistas Razor (.cshtml):** Ubicadas en el directorio `/Views/`. Estan organizadas por controlador y manejan la estructura base HTML de las interfaces.
- **Logica del Cliente (Scripts):** Localizada en `/Scripts/`. Implementa el dinamismo de la interfaz mediante controladores AngularJS separados por modulo de negocio (ej. `/Scripts/Pedidos/`, `/Scripts/CorteCaja/`).

### Capa de Controladores (Controllers)
- Ubicada en `/Controllers/`. Actua como punto de entrada de las peticiones HTTP y orquestador entre las peticiones frontend AngularJS y los servicios backend.

### Capa de Logica de Negocio y Modelos (Models & Clases)
- **Modelos de Datos y DTOs:** Ubicados en `/Models/`. Contiene clases que representan tanto la informacion del negocio como las estructuras directas de tablas de SAP Business One (ejemplo: `ORDR` para ordenes de venta y `OQUT` para cotizaciones).
- **Control transaccional ERP:** Encapsulado principalmente en `IntegracionSAPController.cs`, que maneja el ciclo de integracion dinamica con la DI API de SAP.

### Capa de Acceso a Datos (DataAccess)
- Ubicada en el directorio `/DataAccess/` y apoyada por clases utilitarias como `/Models/Consultas.cs`. El sistema emplea dos estrategias clave:
  1. **ADO.NET Nativo:** Uso explicito de `SqlConnection`, `SqlCommand` y lectores `SqlDataReader` para invocar procedimientos almacenados de base de datos de manera directa y optimizada.
  2. **Entity Framework 6:** Utilizado para persistir configuraciones locales de base de datos a traves de DbContext y mapeos ORM nativos de C#.

---

## 2. Controladores y Flujo de Backend

### Ciclo de vida de las peticiones HTTP:
1. **Origen:** La aplicacion cliente AngularJS envia una peticion asincrona usando el servicio `$http` hacia un endpoint del backend (ej. `/Pedido/EntregarPedido`).
2. **Autenticacion y OWIN:** La peticion es interceptada por el pipeline de middleware OWIN configurado en `Startup.cs` para validar la cookie de autenticacion activa y poblar los claims del usuario.
3. **Filtros de Acceso:** Se ejecutan los filtros globales y especificos de accion. Los controladores estan decorados con atributos personalizados como `[AuthorizeRole("R")]` (definidos en `/Filters/`) para validar el rol web asignado.
4. **Ejecucion en Controlador:** El controlador recibe los parametros (mapeados a ViewModels o DTOs como `DetallePedido`), e inicia las llamadas de negocio hacia SQL Server o la conexion COM a SAP Business One.
5. **Respuesta:** El resultado es empaquetado en un formato JSON a traves de un `JsonResult` (o devuelto en forma de flujo binario PDF usando `FileResult` para reportes) y retornado al cliente con el codigo de estado HTTP correspondiente.

### Responsabilidades de los controladores principales:
- **LoginController:** Procesa la autenticacion de credenciales contra SAP y la base de datos local mediante `SP_LoginRutas`. Gestiona la persistencia de cookies OWIN de sesion.
- **RutasController:** Provee los endpoints de lectura para la gestion, asignacion y control logistico de las rutas de entrega.
- **PedidoController:** Controla el ciclo de vida de los pedidos asignados a los repartidores, carga su detalle y gestiona las solicitudes de entrega.
- **CorteCajaController:** Gestiona el cierre financiero del repartidor, persistiendo la conciliacion monetaria de la ruta en formato JSON en base de datos.
- **IntegracionSAPController:** Encapsula de forma exclusiva la conexion DI API no administrada con SAP Business One, controlando la creacion transaccional de ordenes de entrega.

---

## 3. Frontend y Comunicacion AJAX

### Estructura del JavaScript y manipulacion del DOM:
El frontend esta construido sobre **AngularJS 1.x**. La aplicacion evita la manipulacion directa del DOM mediante jQuery (a menos de ser estrictamente necesario para integracion de librerias de terceros). Todo el dinamismo se realiza a traves del binding de datos bidireccional (`ng-model`, `ng-repeat`, `ng-show`).

- **Controladores Angular:** Cada vista Razor asocia su contenedor principal a un controlador AngularJS (ej. `ng-controller="pedidoController"`).
- **Componentes Visuales:** Se hace uso de librerias como **SweetAlert2 (Swal)** para el despliegue de ventanas de confirmacion de procesos criticos (ej. entregar pedidos) y alertas de error en la interfaz de usuario de manera no bloqueante.

### Mecanismos de comunicacion asincrona:
- **JsonResult:** El frontend realiza llamadas asincronas via `$http.post` enviando y recibiendo payloads serializados en JSON. El backend procesa las solicitudes y retorna respuestas rapidas para actualizar el estado del `$scope` de Angular en el cliente.
- **Uso de $timeout:** Al actualizar arreglos y propiedades del `$scope` que se renderizan inmediatamente en la vista (por ejemplo, al recalcular subtotales de pedidos), se debe encapsular la logica en un wrapper `$timeout` de Angular. Esto garantiza que el framework ejecute el ciclo de digestion (`$digest`) y renderice los datos actualizados.
- **Generacion y Descarga de Archivos:** Para los reportes y tickets Crystal Reports, el backend procesa el archivo PDF y devuelve un flujo binario. El cliente AngularJS abre la ruta del archivo mediante `window.open(pdfUrl, '_blank')` para su despliegue y posterior impresion sin interrupcion de la aplicacion de una sola pagina.

---

## 4. Estandares del Codebase

Para mantener la consistencia y escalabilidad del codebase, todos los desarrolladores deben seguir los siguientes estandares de nomenclatura y directrices:

### Convenciones de nomenclatura:
- **Clases en C# / Controladores / Servicios:** Usar PascalCase (ejemplo: `PedidoController`, `IntegracionSAPController`).
- **Nombres de Tablas de SAP:** Aquellos DTOs que mapeen directamente tablas del SDK de SAP Business One deben conservar el nombre nativo de la tabla SAP (ejemplo: `ORDR`, `RDR1`, `OQUT`, `QUT1`).
- **Controladores y Servicios AngularJS:** Usar camelCase para variables locales, funciones y asignaciones al `$scope` (ejemplo: `cargarPedido`, `verificarArticulosMarcados`).
- **Endpoints y Rutas HTTP:** Deben mapearse a las acciones de los controladores MVC en formato PascalCase (ejemplo: `/Pedido/ObtenerDetallePedido`).

### Directrices para nuevas implementaciones:

#### Manejo de Conexiones SQL:
Todo acceso a base de datos mediante ADO.NET debe declararse dentro de bloques `using` para garantizar que la conexion se cierre de manera automatica y evite el agotamiento del pool de conexiones:
```csharp
using (var conexion = new SqlConnection(cadenaConexion))
{
    conexion.Open();
    // Logica de base de datos
}
```

#### Prevencion de Fugas de Memoria en SAP DI API:
Al interactuar con objetos de tipo COM de SAP Business One, se debe asegurar su liberacion manual en el bloque `finally` del metodo:
```csharp
Documents pedido = null;
try
{
    pedido = (Documents)company.GetBusinessObject(BoObjectTypes.oOrders);
    // Logica con pedido
}
finally
{
    if (pedido != null)
    {
        Marshal.ReleaseComObject(pedido);
    }
}
```

#### Prevencion de Fugas de Memoria en Crystal Reports:
Toda creacion de reportes PDF locales a traves del motor de Crystal Reports debe liberar el archivo cargado y descartar la instancia del objeto de reporte:
```csharp
ReportDocument reporte = null;
try
{
    reporte = new ReportDocument();
    reporte.Load(rutaReporte);
    // Exportacion del reporte
}
finally
{
    if (reporte != null)
    {
        reporte.Close();
        reporte.Dispose();
    }
}
```

#### Control de Errores:
No se permiten excepciones silenciosas en el backend. Toda excepcion controlada debe ser capturada en un bloque `try-catch` y registrada inmediatamente llamando a `ExceptionUtility.LogException` o al servicio de logs del sistema.
