// ╔══════════════════════════════════════════════════════════════════════╗
// ║  Program.cs — WebApi: Cliente MCP + Gemini Single-Turn Action     ║
// ╠══════════════════════════════════════════════════════════════════════╣
// ║                                                                    ║
// ║  🚀 PUERTO: 5020                                                   ║
// ╚══════════════════════════════════════════════════════════════════════╝

using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5020");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// HttpClient con timeout de 25 segundos para Gemini
builder.Services.AddHttpClient("gemini", client =>
{
    client.Timeout = TimeSpan.FromSeconds(40);
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();
app.UseCors("AllowAll");

var geminiApiKey = app.Configuration["Gemini:ApiKey"]
    ?? throw new Exception("❌ Falta Gemini:ApiKey en appsettings.json");
var geminiModel = app.Configuration["Gemini:Model"] ?? "gemini-2.0-flash-lite";
var mcpServerUrl = app.Configuration["McpServer:Url"] ?? "http://localhost:5010";

var geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{geminiModel}:generateContent?key={geminiApiKey}";

// ─── HELPER: Crear cliente MCP ────────────────────────────────────
async Task<McpClient> CrearClienteMcp()
{
    var transport = new HttpClientTransport(new HttpClientTransportOptions
    {
        Endpoint = new Uri($"{mcpServerUrl}/mcp"),
        Name = "TiendaWebApi"
    });
    return await McpClient.CreateAsync(transport);
}

async Task<string> LlamarHerramientaMcp(McpClient client, string toolName, Dictionary<string, object?>? args = null)
{
    var result = await client.CallToolAsync(toolName, args);
    return string.Join("\n", result.Content
        .Where(c => c.Type == "text")
        .Select(c => ((dynamic)c).Text ?? ""));
}

// ╔══════════════════════════════════════════════════════════════════╗
// ║  ENDPOINT: POST /api/chat — SINGLE-TURN ACTION                  ║
// ╚══════════════════════════════════════════════════════════════════╝
app.MapPost("/api/chat", async (MensajeChat mensaje, IHttpClientFactory httpFactory) =>
{
    Console.WriteLine($"[LOG - CHAT] Recibido: '{mensaje.Mensaje}'");

    // ─── UNA sola conexión MCP para todo el request ────────────────
    await using var mcpClient = await CrearClienteMcp();

    // PASO 1: Obtener contexto compacto de la tienda
    Console.WriteLine("[LOG - CHAT] Obteniendo contexto MCP...");
    string contextoTienda;
    try
    {
        contextoTienda = await LlamarHerramientaMcp(mcpClient, "obtener_contexto_tienda");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR - MCP] {ex.Message}");
        return Results.Problem("No se pudo conectar al servidor MCP. ¿Está corriendo en puerto 5010?");
    }

    // PASO 2: Construir prompt y personalidad de Don Lalo para Gemini
    var systemInstruction = new
    {
        parts = new[]
        {
            new { text = @"Eres 'Don Lalo', el querido tendero y asistente de voz estrella de 'La Tiendita del Barrio' en México.
Conoces a todos los vecinos, tienes una vibra súper alegre, servicial y eres un maestro de la cocina casera mexicana y el mandado.

PERSONALIDAD Y VOZ:
- Hablas con calidez mexicana, entusiasta y respetuosa (ej: '¡Qué onda vecino!', '¡Trato hecho!', '¡Uff, eso te va a quedar de rechupete!', '¡Con gusto güerito!').
- Tus respuestas son para leerse en voz alta: fluidas, con chispa, sin asteriscos de markdown, sin listas con guiones y sin emojis excesivos.
- Longitud ideal: 2 a 4 oraciones bien armadas. Da gusto escucharte.
- Si te saludan o preguntan cómo estás, saluda con alegría y pregunta qué se va a cocinar hoy o qué hace falta en la alacena.

EXPERTO EN COMIDA Y RECETAS MEXICANAS:
- Si el cliente tiene hambre, no sabe qué comer o pide una receta, sugiérele un platillo con sabor casero usando productos de tu tienda.
  Ejemplos típicos:
  * Chilaquiles verdes o rojos con pollo deshebrado, crema ácida Lala y queso cotija.
  * Sincronizadas doraditas con tortillas de harina Tía Rosa, queso Oaxaca derretido y jamón Fud.
  * Guacamole botanero con aguacate Hass, jitomate, cebolla, cilantro fresco, chile serrano y totopos Milpa Real.
  * Tinga de pollo en tostadas Charras con chipotle, puré y cremita.
  * Hot dogs de tiendita con medias noches, salchicha de pavo, cátsup, mostaza y mayonesa.
  * Hamburguesas con queso amarillo Kraft, tocino crujiente, jitomate y cebolla.
  * Espagueti a la crema con mantequilla Gloria y queso parmesano.
  * Hot cakes de domingo con harina Pronto, mantequilla, miel de maple y leche fría.
  * Huevitos con frijoles refritos, tortillas de maíz calientes y salsa casera.
  * Ensalada de atún con mayonesa, jitomate, limón y galletas saladitas.
- Ofrécete a armar el paquete o combo y echarle los ingredientes directo al carrito.
- Cuando el cliente pida agregar una receta o diga 'échamelos', 'agrega los ingredientes' o 'trato hecho', AGREGA TODOS los ingredientes del combo usando múltiples acciones 'agregar_carrito' en una sola respuesta.
- Si buscan opciones saludables o para diabéticos, filtra por los tags 'APT-DIAB' o calorías (<100cal).
- NUNCA inventes productos: utiliza ÚNICAMENTE los productos reales presentes en el catálogo con sus IDs exactos.
- Al confirmar una compra, felicita al cliente y genera un número de orden ORD-XXXXX.

IMPORTANTE — MOSTRAR PRODUCTOS EN PANTALLA:
- Si el cliente pide buscar o ver productos generales, dile una frase amable (ej: '¡Claro vecino! Aquí en pantalla te muestro las opciones.') e incluye los IDs en 'productos_relevantes' para que el catálogo web se filtre en vivo.

RESPONDE EXCLUSIVAMENTE CON JSON VÁLIDO (sin bloques de código markdown fuera):
{
  ""respuesta"": ""Texto de Don Lalo para voz (natural, sin asteriscos ni listas)"",
  ""acciones"": [
    { ""tipo"": ""agregar_carrito"", ""parametros"": { ""id_producto"": 1, ""cantidad"": 1 } }
  ],
  ""productos_relevantes"": [1, 3]
}

TIPOS DE ACCIONES:
- 'agregar_carrito': { ""id_producto"": int, ""cantidad"": int } (¡puedes devolver múltiples para combos de recetas!).
- 'eliminar_carrito': { ""id_item"": int } (usa el [Item#X] del carrito actual).
- 'confirmar_orden': sin parámetros (cuando el cliente confirme que ya va a pagar o finalizar).
- Si solo platica o consulta: acciones = []." }
        }
    };

    // Construir historial multi-turn para contexto conversacional
    var contentsList = new List<object>();

    if (mensaje.Historial != null && mensaje.Historial.Count > 0)
    {
        // Tomar hasta los últimos 6 turnos para mantener velocidad y memoria
        var turnosRecientes = mensaje.Historial.TakeLast(6);
        foreach (var t in turnosRecientes)
        {
            if (string.IsNullOrWhiteSpace(t.Text)) continue;
            contentsList.Add(new
            {
                role = t.Role == "model" ? "model" : "user",
                parts = new[] { new { text = t.Text } }
            });
        }
    }

    var promptActual = $"[ESTADO ACTUAL DE LA TIENDA Y CATÁLOGO]\n{contextoTienda}\n\nMENSAJE DEL CLIENTE: {mensaje.Mensaje}";
    contentsList.Add(new
    {
        role = "user",
        parts = new[] { new { text = promptActual } }
    });

    var requestBody = new
    {
        system_instruction = systemInstruction,
        contents = contentsList,
        generation_config = new
        {
            response_mime_type = "application/json",
            temperature = 0.6,
            max_output_tokens = 800  // Espacio suficiente para múltiples acciones de recetas
        }
    };

    var jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    // PASO 3: Llamar a Gemini con timeout configurado
    Console.WriteLine("[LOG - CHAT] Llamando a Gemini...");
    var httpClient = httpFactory.CreateClient("gemini");

    string responseText;
    try
    {
        var jsonContent = JsonSerializer.Serialize(requestBody, jsonOptions);
        var response = await httpClient.PostAsync(
            geminiUrl,
            new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json"));

        responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[ERROR - GEMINI] {response.StatusCode}: {responseText}");
            return Results.Problem($"Error de Gemini API ({response.StatusCode}): {responseText}");
        }
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine("[ERROR - GEMINI] Timeout (25s)");
        return Results.Ok(new
        {
            respuesta = "Lo siento, tardé mucho en responder. ¿Podrías repetirlo de forma más breve?",
            productosActualizados = (object?)null
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR - GEMINI] {ex.Message}");
        return Results.Problem($"Error al contactar Gemini: {ex.Message}");
    }

    Console.WriteLine("[LOG - CHAT] ✅ Respuesta de Gemini recibida");

    // PASO 4: Parsear respuesta de Gemini
    var geminiResponse = JsonNode.Parse(responseText);
    var finalText = geminiResponse?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.GetValue<string>();

    if (string.IsNullOrEmpty(finalText))
    {
        return Results.Ok(new
        {
            respuesta = "Lo siento, no pude procesar tu solicitud. ¿Podrías intentar de nuevo?",
            productosActualizados = (object?)null
        });
    }

    string respuestaTexto;
    int[]? productosRelevantes = null;
    bool ordenConfirmada = false;

    try
    {
        var actionResponse = JsonNode.Parse(finalText);
        respuestaTexto = actionResponse?["respuesta"]?.GetValue<string>()
            ?? "No pude entender la respuesta.";

        var prodArray = actionResponse?["productos_relevantes"]?.AsArray();
        if (prodArray != null && prodArray.Count > 0)
        {
            productosRelevantes = prodArray
                .Where(p => p != null)
                .Select(p => p!.GetValue<int>())
                .ToArray();
        }

        // PASO 5: Ejecutar acciones (reutilizando la misma conexión MCP)
        var acciones = actionResponse?["acciones"]?.AsArray();
        if (acciones != null && acciones.Count > 0)
        {
            Console.WriteLine($"[LOG - CHAT] Ejecutando {acciones.Count} acción(es)...");
            foreach (var accion in acciones)
            {
                var tipo = accion?["tipo"]?.GetValue<string>();
                var parametros = accion?["parametros"];

                if (string.IsNullOrEmpty(tipo) || tipo == "ninguna") continue;

                try
                {
                    switch (tipo)
                    {
                        case "agregar_carrito":
                            var idProd = parametros?["id_producto"]?.GetValue<int>() ?? 0;
                            var cantidad = parametros?["cantidad"]?.GetValue<int>() ?? 1;
                            var resAdd = await LlamarHerramientaMcp(mcpClient, "agregar_carrito",
                                new Dictionary<string, object?> { ["id_producto"] = idProd, ["cantidad"] = cantidad });
                            Console.WriteLine($"[LOG - MCP] agregar_carrito({idProd}, {cantidad}): {resAdd}");
                            break;

                        case "eliminar_carrito":
                            var idItem = parametros?["id_item"]?.GetValue<int>() ?? 0;
                            var resDel = await LlamarHerramientaMcp(mcpClient, "eliminar_carrito",
                                new Dictionary<string, object?> { ["id_item"] = idItem });
                            Console.WriteLine($"[LOG - MCP] eliminar_carrito({idItem}): {resDel}");
                            break;

                        case "confirmar_orden":
                            var resOrden = await LlamarHerramientaMcp(mcpClient, "confirmar_orden");
                            Console.WriteLine($"[LOG - MCP] confirmar_orden: {resOrden}");
                            ordenConfirmada = !resOrden.Contains("Error");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error ejecutando acción '{tipo}': {ex.Message}");
                }
            }
        }
    }
    catch (JsonException)
    {
        // Gemini devolvió texto plano en vez de JSON (raro con response_mime_type)
        respuestaTexto = finalText;
    }

    // PASO 6: Si hay productos relevantes, devolverlos para actualizar la vista
    object? productosActualizados = null;
    if (productosRelevantes != null && productosRelevantes.Length > 0)
    {
        try
        {
            var searchResult = await LlamarHerramientaMcp(mcpClient, "obtener_contexto_tienda");
            productosActualizados = ParsearProductosDesdeContexto(searchResult, null, productosRelevantes);
        }
        catch { /* Si falla, el frontend usa su caché */ }
    }

    return Results.Ok(new { respuesta = respuestaTexto, productosActualizados, ordenConfirmada });
});

// ╔══════════════════════════════════════════════════════════════════╗
// ║  ENDPOINTS REST                                                 ║
// ╚══════════════════════════════════════════════════════════════════╝

// Todos los productos (para caché inicial del frontend)
app.MapGet("/api/productos", async (string? busqueda) =>
{
    Console.WriteLine($"[LOG - GET PRODUCTOS] Buscando: '{busqueda}'");
    try
    {
        await using var mcpClient = await CrearClienteMcp();
        var contexto = await LlamarHerramientaMcp(mcpClient, "obtener_contexto_tienda");
        return Results.Ok(ParsearProductosDesdeContexto(contexto, busqueda, null));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error en /api/productos: {ex.Message}");
        return Results.Problem("Error al conectar con el servidor MCP. ¿Está corriendo en puerto 5010?");
    }
});

// Agregar al carrito
app.MapPost("/api/carrito", async (ItemCarrito item) =>
{
    try
    {
        await using var mcpClient = await CrearClienteMcp();
        var resultado = await LlamarHerramientaMcp(mcpClient, "agregar_carrito",
            new Dictionary<string, object?> { ["id_producto"] = item.IdProducto, ["cantidad"] = item.Cantidad });

        if (resultado.Contains("Error"))
            return Results.BadRequest(new { mensaje = resultado });

        return Results.Ok(new { mensaje = resultado });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error MCP: {ex.Message}");
    }
});

// Ver el carrito
app.MapGet("/api/carrito", async () =>
{
    try
    {
        await using var mcpClient = await CrearClienteMcp();
        var contexto = await LlamarHerramientaMcp(mcpClient, "obtener_contexto_tienda");
        return Results.Ok(ParsearCarritoDesdeContexto(contexto));
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error MCP: {ex.Message}");
    }
});

// Eliminar del carrito
app.MapDelete("/api/carrito/{idItem}", async (int idItem) =>
{
    try
    {
        await using var mcpClient = await CrearClienteMcp();
        var resultado = await LlamarHerramientaMcp(mcpClient, "eliminar_carrito",
            new Dictionary<string, object?> { ["id_item"] = idItem });

        if (resultado.Contains("Error"))
            return Results.NotFound(new { mensaje = resultado });

        return Results.Ok(new { mensaje = resultado });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error MCP: {ex.Message}");
    }
});

// ╔══════════════════════════════════════════════════════════════════╗
// ║  PARSERS                                                        ║
// ╚══════════════════════════════════════════════════════════════════╝

List<object> ParsearProductosDesdeContexto(string contexto, string? busqueda, int[]? soloIds)
{
    var productos = new List<object>();
    var lines = contexto.Split('\n');
    var enProductos = false;

    foreach (var line in lines)
    {
        if (line.Contains("=== CATÁLOGO DE PRODUCTOS ===")) { enProductos = true; continue; }
        if (line.Contains("=== CARRITO ACTUAL ===")) break;
        if (!enProductos || !line.StartsWith("[")) continue;

        try
        {
            var idEnd = line.IndexOf(']');
            var id = int.Parse(line[1..idEnd]);

            // Filtrar por IDs específicos si se proporcionaron
            if (soloIds != null && !soloIds.Contains(id)) continue;

            var parts = line[(idEnd + 2)..].Split('|');
            if (parts.Length < 9) continue; // Now we expect 9 or 10 parts

            var nombre = parts[0].Trim();
            var marca = parts[1].Trim();
            var categoria = parts[2].Trim();
            var presentacion = parts[3].Trim();

            var precioStr = parts[4].Trim().TrimStart('$');
            if (!decimal.TryParse(precioStr, out var precio)) continue;

            var calStr = parts[5].Trim().Replace("cal", "");
            if (!int.TryParse(calStr, out var calorias)) continue;

            var aptoDiab = parts[6].Trim() == "APT-DIAB";
            var stockStr = parts[7].Trim().Replace("Stock:", "");
            if (!int.TryParse(stockStr, out var stock)) continue;

            var imagenUrl = parts[8].Trim().Replace("Img:", "");

            // Filtrar por búsqueda de texto
            if (!string.IsNullOrEmpty(busqueda))
            {
                var searchLower = busqueda.ToLower();
                // Buscar también en Tags (DescripcionIA) si está disponible
                var tagsStr = parts.Length > 9 ? parts[9].ToLower() : "";
                if (!nombre.ToLower().Contains(searchLower) &&
                    !marca.ToLower().Contains(searchLower) &&
                    !categoria.ToLower().Contains(searchLower) &&
                    !tagsStr.Contains(searchLower))
                    continue;
            }

            productos.Add(new
            {
                IdProducto = id,
                Nombre = nombre,
                Marca = marca,
                Categoria = categoria,
                Presentacion = presentacion,
                Precio = precio,
                CaloriasPorPorcion = calorias,
                AptoDiabeticos = aptoDiab ? 1 : 0,
                Stock = stock,
                ImagenUrl = imagenUrl
            });
        }
        catch { continue; }
    }

    return productos;
}

List<object> ParsearCarritoDesdeContexto(string contexto)
{
    var items = new List<object>();
    var lines = contexto.Split('\n');
    var enCarrito = false;

    foreach (var line in lines)
    {
        if (line.Contains("=== CARRITO ACTUAL ===")) { enCarrito = true; continue; }
        if (!enCarrito || !line.StartsWith("[Item#")) continue;
        if (line.Contains("(vacío)") || line.StartsWith("TOTAL:")) continue;

        try
        {
            var itemIdEnd = line.IndexOf(']');
            var idStr = line["[Item#".Length..itemIdEnd];
            var idItem = int.Parse(idStr);

            var resto = line[(itemIdEnd + 2)..];
            var cantEnd = resto.IndexOf('x');
            if (!int.TryParse(resto[..cantEnd], out var cantidad)) continue;

            var partes = resto[(cantEnd + 2)..].Split('|');
            if (partes.Length < 3) continue;

            var nombreMarca = partes[0].Trim();
            var precioStr = partes[1].Trim().Replace("$", "").Replace("c/u", "").Trim();
            var subtotalStr = partes[2].Trim().Replace("Subtotal:", "").Replace("$", "").Trim();

            var parenOpen = nombreMarca.LastIndexOf('(');
            var nombre = parenOpen > 0 ? nombreMarca[..parenOpen].Trim() : nombreMarca;
            var marca = parenOpen > 0 ? nombreMarca[(parenOpen + 1)..].TrimEnd(')').Trim() : "";

            if (!decimal.TryParse(precioStr, out var precio)) continue;
            if (!decimal.TryParse(subtotalStr, out var subtotal)) continue;

            items.Add(new
            {
                IdItem = idItem,
                Nombre = nombre,
                Marca = marca,
                Presentacion = "",
                Precio = precio,
                Cantidad = cantidad,
                Subtotal = subtotal,
                ImagenUrl = ""
            });
        }
        catch { continue; }
    }

    return items;
}

// ─── HEALTH CHECK ─────────────────────────────────────────────────
app.MapGet("/", () => Results.Ok(new
{
    servicio = "🔌 La Tiendita del Barrio — WebApi",
    estado = "✅ Activo",
    modelo = geminiModel,
    mcpServer = mcpServerUrl,
    endpoints = new
    {
        chat = "POST /api/chat",
        productos = "GET /api/productos",
        carrito = "GET|POST /api/carrito",
        eliminar = "DELETE /api/carrito/{id}"
    }
}));

Console.WriteLine("╔══════════════════════════════════════════════════╗");
Console.WriteLine("║  🔌 WebApi — La Tiendita del Barrio             ║");
Console.WriteLine("║  📡 Puerto: 5020                                 ║");
Console.WriteLine("║  🧠 Patrón: Single-Turn Action                   ║");
Console.WriteLine($"║  🤖 Modelo: {geminiModel.PadRight(37)}║");
Console.WriteLine($"║  🔗 MCP Server: {mcpServerUrl.PadRight(33)}║");
Console.WriteLine("╚══════════════════════════════════════════════════╝");

app.Run();

public record MensajeChat(string Mensaje, List<TurnoHistorial>? Historial = null);
public record TurnoHistorial(string Role, string Text);
public record ItemCarrito(int IdProducto, int Cantidad);
