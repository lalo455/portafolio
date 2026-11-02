# SAP Middleware API

API intermedia (ASP.NET Core) que recibe pedidos y entregas por HTTP, los encola como *tickets* en SQL Server (con deduplicación y reintentos) y los procesa de forma asíncrona contra **SAP Business One (DI API)**. Incluye logging estructurado (Serilog) y un script de pruebas de carga (`stress_test.py`).

> Versión de portafolio: credenciales, servidores, nombres de sociedad/artículos y campos definidos por usuario fueron sustituidos por valores genéricos.

## Configuración
Edita `appsettings.json` (o usa variables de entorno / user-secrets) con tus propios valores `YOUR_*`. **Nunca** subas credenciales reales.

## Endpoints
- `POST /api/integracion/pedidos`
- `POST /api/integracion/entregas`
