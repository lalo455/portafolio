# Tienda MCP – Asistente de voz con Model Context Protocol

Demo de comercio conversacional: un servidor **MCP** (.NET, SQLite + Dapper) expone herramientas de inventario y ventas; una **Web API** actúa como cliente MCP y conversa con **Google Gemini**; un front-end web con **Web Speech API** permite hablar con la tienda.

## Componentes
- `TiendaMCP.Server` – servidor MCP con las herramientas de la tienda y la inicialización de la BD.
- `TiendaMCP.WebApi` – orquestador: cliente MCP + Gemini, memoria conversacional.
- `TiendaAsistenteVoz` – front-end (HTML/CSS/JS).
- `docs/` – documentación de arquitectura.

## Configuración
La API key de Gemini **no** se incluye. Configúrala fuera del repositorio:
```
dotnet user-secrets set "Gemini:ApiKey" "<TU_API_KEY>" --project TiendaMCP.WebApi
```
La base SQLite se genera automáticamente al iniciar el servidor.
