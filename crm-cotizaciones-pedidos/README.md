# CRM de cotizaciones y pedidos (ASP.NET MVC + SAP Business One)

Aplicación web ASP.NET MVC 5 (C#, AngularJS, jQuery) para que una fuerza de ventas gestione **cotizaciones y pedidos**: alta y actualización de cotizaciones, conversión a pedido, cierre con motivo, consulta de facturas y reportes de ventas mensual/anual. Integra con **SAP Business One** (DI API / SQL) y genera impresiones con Crystal Reports.

Capas principales: `Controllers`, `DataAccess`, `Models`, `Clases`, `Filters`, `Helpers`, `Views`, `Scripts` (AngularJS) y `SQLScripts`.

> **Versión de portafolio (saneada).** Se eliminaron credenciales, servidores, nombres de clientes y de la empresa, logotipos y reportes `.rpt`. Los nombres de cliente fueron sustituidos por `ClientA…`. Configura tus propios valores `YOUR_*` en `Web.config` o usa transformaciones/variables de entorno. No incluye datos reales.
