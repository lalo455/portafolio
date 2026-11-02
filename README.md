# Portafolio — Jose Eduardo Ramirez Mendoza

Proyectos de desarrollo .NET: integraciones con SAP Business One, WMS, apps móviles (Xamarin/MAUI), IoT (MQTT/RFID) e IA (MCP + Gemini).

> Todos los proyectos son **versiones de portafolio saneadas**: credenciales, servidores, licencias, llaves de cifrado y datos de clientes/empresa fueron reemplazados por valores genéricos (`YOUR_*`). Configura los tuyos propios para ejecutarlos.

| Proyecto | Descripción | Tecnologías |
|---|---|---|
| [sap-middleware-api](sap-middleware-api/) | API intermedia que encola pedidos/entregas y los procesa contra SAP B1 (DI API) con reintentos | ASP.NET Core, SQL Server, Hangfire, SAP DI API |
| [wms-web-sap-b1](wms-web-sap-b1/) | WMS web: recepciones, traspasos, surtido, producción, etiquetas y recuento integrado con SAP B1 | ASP.NET MVC, Dapper, AngularJS |
| [wms-movil-maui](wms-movil-maui/) | App de almacén para terminales de mano con escaneo de código de barras/RFID | .NET MAUI, MVVM |
| [crm-cotizaciones-pedidos](crm-cotizaciones-pedidos/) | CRM de fuerza de ventas: cotizaciones, pedidos y facturas | ASP.NET MVC 5, AngularJS, SQL Server |
| [rastreo-pedidos-rutas](rastreo-pedidos-rutas/) | Gestión logística: asignación de pedidos a rutas, seguimiento y corte de caja | ASP.NET MVC 5, AngularJS, Crystal Reports |
| [rfid-inventory-app](rfid-inventory-app/) | Inventario con lectores RFID UHF, SQLite local y MQTT | Xamarin.Forms, MVVM, SQLite |
| [bascula-agent-mqtt](bascula-agent-mqtt/) | Servicio que lee una báscula por RS-232 y publica el peso por MQTT | .NET Worker Service, MQTTnet |
| [tienda-mcp-asistente-voz](tienda-mcp-asistente-voz/) | Comercio conversacional por voz: servidor MCP + cliente con Google Gemini | .NET, MCP, SQLite, Web Speech API |
| [encuesta-webapp-clean-architecture](encuesta-webapp-clean-architecture/) | App de encuestas con Clean Architecture, SOLID y DRY | ASP.NET MVC, Dapper, SQL Server |
| [ping-webapp-netcore](ping-webapp-netcore/) | Monitor de conectividad (ping) con API y vista en tiempo real | ASP.NET Core MVC |
