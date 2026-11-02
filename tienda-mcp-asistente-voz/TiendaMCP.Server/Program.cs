// ╔══════════════════════════════════════════════════════════════════════╗
// ║  Program.cs — Servidor MCP de La Tiendita del Barrio              ║
// ╠══════════════════════════════════════════════════════════════════════╣
// ║                                                                    ║
// ║  📚 ¿QUÉ ES UN SERVIDOR MCP?                                      ║
// ║                                                                    ║
// ║  MCP (Model Context Protocol) es un estándar creado por Anthropic  ║
// ║  y adoptado por Microsoft, Google, y otras empresas de IA.         ║
// ║  Es como un "USB universal" para IA — cualquier modelo que        ║
// ║  hable MCP puede conectarse a este servidor y usar sus            ║
// ║  herramientas automáticamente.                                     ║
// ║                                                                    ║
// ║  Este servidor expone las herramientas de la tienda (buscar,       ║
// ║  agregar al carrito, etc.) usando el protocolo MCP sobre HTTP.     ║
// ║  Se comunica via SSE (Server-Sent Events) y JSON-RPC.             ║
// ║                                                                    ║
// ║  📌 CONCEPTOS CLAVE:                                               ║
// ║  - AddMcpServer()          → Registra el servidor MCP              ║
// ║  - WithHttpTransport()     → Usa HTTP/SSE como transporte          ║
// ║  - WithToolsFromAssembly() → Descubre herramientas con             ║
// ║                              [McpServerTool] automáticamente       ║
// ║  - MapMcp()                → Crea los endpoints /sse y /messages   ║
// ║                                                                    ║
// ║  🚀 PUERTO: 5010                                                   ║
// ╚══════════════════════════════════════════════════════════════════════╝

using TiendaMCP.Server.Data;

var builder = WebApplication.CreateBuilder(args);

// ─── CONFIGURAR PUERTO ────────────────────────────────────────────
// Usamos el puerto 5010 para el servidor MCP.
// Así no choca con el WebApi (5020) ni el Frontend (5030).
builder.WebHost.UseUrls("http://localhost:5010");

// ─── REGISTRAR SERVIDOR MCP ──────────────────────────────────────
// 📌 AddMcpServer() registra los servicios del protocolo MCP.
// 📌 WithHttpTransport() dice que la comunicación será via HTTP/SSE.
//    (La alternativa es Stdio, que se usa cuando el servidor es una
//    app de consola que lee de stdin/stdout — como con Claude Desktop).
// 📌 WithToolsFromAssembly() escanea todo el proyecto buscando clases
//    con [McpServerToolType] y métodos con [McpServerTool]. Los registra
//    automáticamente sin que tengas que listarlos uno por uno.
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// ─── INICIALIZAR BASE DE DATOS ────────────────────────────────────
// Creamos las tablas y llenamos con datos de ejemplo al arrancar.
// Esto solo se ejecuta una vez al inicio del servidor.
DatabaseInitializer.Initialize();

// ─── MAPEAR ENDPOINTS MCP ─────────────────────────────────────────
// 📌 MapMcp() crea automáticamente los endpoints necesarios para
// el protocolo MCP:
//   - POST /mcp  → Para recibir mensajes JSON-RPC del cliente
//   - GET /mcp/sse → Para el stream de Server-Sent Events
//
// El cliente MCP (en el WebApi) se conecta a estos endpoints
app.MapMcp("/mcp");

// ─── ENDPOINT DE SALUD ────────────────────────────────────────────
// Un endpoint simple para verificar que el servidor está corriendo.
// Útil para debugging y health checks.
app.MapGet("/", () => Results.Ok(new 
{ 
    servicio = "🏪 La Tiendita del Barrio — Servidor MCP",
    estado = "✅ Activo",
    protocolo = "MCP (Model Context Protocol)",
    transporte = "HTTP/SSE",
    endpoints = new { mcp = "/mcp", health = "/" }
}));

Console.WriteLine("╔══════════════════════════════════════════════════╗");
Console.WriteLine("║  🏪 Servidor MCP — La Tiendita del Barrio       ║");
Console.WriteLine("║  📡 Puerto: 5010                                 ║");
Console.WriteLine("║  🔗 MCP Endpoint: http://localhost:5010/mcp      ║");
Console.WriteLine("║  ❤️  Health Check: http://localhost:5010/         ║");
Console.WriteLine("╚══════════════════════════════════════════════════╝");

app.Run();
