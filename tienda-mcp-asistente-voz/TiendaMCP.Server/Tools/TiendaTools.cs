// ╔══════════════════════════════════════════════════════════════════════╗
// ║  TiendaTools.cs — Herramientas MCP de La Tiendita del Barrio      ║
// ╠══════════════════════════════════════════════════════════════════════╣
// ║                                                                    ║
// ║  📚 ¿QUÉ SON LAS "HERRAMIENTAS MCP" (MCP Tools)?                  ║
// ║                                                                    ║
// ║  Son funciones que tu servidor expone al mundo exterior usando     ║
// ║  el protocolo MCP. Cualquier IA o aplicación que hable MCP puede  ║
// ║  descubrir estas herramientas automáticamente y llamarlas.         ║
// ║                                                                    ║
// ║  Es como un menú de restaurante:                                   ║
// ║  - El MENÚ (lista de herramientas) dice qué puedes pedir          ║
// ║  - El MESERO (protocolo MCP) lleva tu pedido a la cocina          ║
// ║  - La COCINA (estas funciones) prepara lo que pediste             ║
// ║  - El CLIENTE (la IA/Gemini) lee el menú y decide qué pedir      ║
// ║                                                                    ║
// ║  🔑 DECORADORES IMPORTANTES:                                       ║
// ║  [McpServerToolType] → Marca la clase como contenedor de tools    ║
// ║  [McpServerTool]     → Marca un método como herramienta MCP       ║
// ║  [Description()]     → Describe qué hace la herramienta (la IA   ║
// ║                         lee esto para saber cuándo usarla)        ║
// ╚══════════════════════════════════════════════════════════════════════╝

using ModelContextProtocol.Server;
using Microsoft.Data.Sqlite;
using Dapper;
using System.ComponentModel;

namespace TiendaMCP.Server.Tools;

// 📌 [McpServerToolType] le dice al SDK de MCP:
// "Esta clase contiene herramientas. Escanéala y registra sus métodos."
// El SDK las descubre automáticamente con WithToolsFromAssembly().
[McpServerToolType]
public class TiendaTools
{
    // ╔══════════════════════════════════════════════════════════════╗
    // ║  HERRAMIENTA 1: buscar_producto                             ║
    // ╠══════════════════════════════════════════════════════════════╣
    // ║  Busca productos en el catálogo por nombre, marca, o        ║
    // ║  categoría. También busca en el campo DescripcionIA para    ║
    // ║  encontrar por sinónimos (ej: "soda" → "Coca-Cola").        ║
    // ╚══════════════════════════════════════════════════════════════╝
    [McpServerTool, Description(
        "Busca productos en el catálogo de la tienda por nombre, marca, categoría o sinónimos. " +
        "Úsala siempre que necesites consultar qué productos hay disponibles. " +
        "El término puede ser: nombre (Coca-Cola), marca (Bimbo), categoría (Bebidas, Lácteos, Botanas, Panadería, Despensa, Salsas, Barras, Pastelería, Cereales), " +
        "o una característica (saludable, diabéticos, bajo-calorías).")]
    public static string buscar_producto(
        [Description("Término de búsqueda: nombre, marca, categoría, o característica del producto")]
        string nombre)
    {
        using var connection = new SqliteConnection(Data.DatabaseInitializer.ConnectionString);

        // 📌 Buscamos en múltiples columnas + DescripcionIA para máxima cobertura.
        // LIKE '%término%' busca el texto en cualquier posición del campo.
        // DescripcionIA contiene sinónimos, así que si el usuario dice "soda",
        // encontrará Coca-Cola porque su DescripcionIA incluye "|soda|".
        var productos = connection.Query(
            @"SELECT IdProducto, Nombre, Marca, Categoria, Presentacion, 
                     Precio, CaloriasPorPorcion, AptoDiabeticos, Popularidad, Stock, ImagenUrl
              FROM Productos 
              WHERE Nombre LIKE @Busqueda 
                 OR Marca LIKE @Busqueda 
                 OR Categoria LIKE @Busqueda
                 OR DescripcionIA LIKE @Busqueda
              ORDER BY Popularidad DESC",
            new { Busqueda = $"%{nombre}%" }).ToList();

        if (productos.Count == 0)
            return "No se encontraron productos con ese término de búsqueda.";

        // 📌 Devolvemos los resultados como texto legible.
        // MCP devuelve strings, no objetos complejos — la IA los interpreta.
        var resultado = $"Se encontraron {productos.Count} producto(s):\n\n";
        foreach (var p in productos)
        {
            resultado += $"- ID:{((dynamic)p).IdProducto} | {((dynamic)p).Nombre} ({((dynamic)p).Marca}) " +
                         $"| {((dynamic)p).Presentacion} | ${((dynamic)p).Precio:F2} " +
                         $"| {((dynamic)p).CaloriasPorPorcion}cal " +
                         $"| {(((dynamic)p).AptoDiabeticos == 1 ? "✅ Apto diabéticos" : "❌ No apto diabéticos")} " +
                         $"| Stock: {((dynamic)p).Stock}\n";
        }

        return resultado;
    }

    // ╔══════════════════════════════════════════════════════════════╗
    // ║  HERRAMIENTA 2: agregar_carrito                             ║
    // ╠══════════════════════════════════════════════════════════════╣
    // ║  Agrega un producto al carrito del usuario.                  ║
    // ║  Valida que haya stock suficiente antes de agregar.          ║
    // ║  Reduce el stock del producto automáticamente.               ║
    // ╚══════════════════════════════════════════════════════════════╝
    [McpServerTool, Description(
        "Agrega un producto al carrito. Si el producto ya está en el carrito, actualiza la cantidad " +
        "en vez de duplicarlo. Necesitas el IdProducto y la cantidad deseada.")]
    public static string agregar_carrito(
        [Description("El IdProducto del producto a agregar (número entero obtenido de buscar_producto)")]
        int id_producto,
        [Description("Cantidad de unidades a agregar al carrito (mínimo 1)")]
        int cantidad = 1)
    {
        using var connection = new SqliteConnection(Data.DatabaseInitializer.ConnectionString);

        var producto = connection.QueryFirstOrDefault(
            "SELECT IdProducto, Nombre, Marca, Presentacion, Precio, Stock FROM Productos WHERE IdProducto = @Id",
            new { Id = id_producto });

        if (producto == null)
            return $"Error: No se encontró el producto con ID {id_producto}.";

        var stock = (int)((dynamic)producto).Stock;
        if (stock < cantidad)
            return $"Error: Stock insuficiente para {((dynamic)producto).Nombre}. Solo quedan {stock} unidades.";

        // ─── UPSERT: si ya existe en el carrito, suma la cantidad ────────
        var itemExistente = connection.QueryFirstOrDefault(
            "SELECT IdItem, Cantidad FROM Carrito WHERE IdProducto = @IdProducto",
            new { IdProducto = id_producto });

        if (itemExistente != null)
        {
            // Ya está en carrito → actualizar cantidad
            connection.Execute(
                "UPDATE Carrito SET Cantidad = Cantidad + @Cantidad WHERE IdItem = @IdItem",
                new { Cantidad = cantidad, ((dynamic)itemExistente).IdItem });
        }
        else
        {
            // No está → insertar nuevo
            connection.Execute(
                "INSERT INTO Carrito (IdProducto, Cantidad) VALUES (@IdProducto, @Cantidad)",
                new { IdProducto = id_producto, Cantidad = cantidad });
        }

        // Descontar stock
        connection.Execute(
            "UPDATE Productos SET Stock = Stock - @Cantidad WHERE IdProducto = @Id",
            new { Cantidad = cantidad, Id = id_producto });

        // Limpiar si la cantidad bajó a 0 o menos
        connection.Execute("DELETE FROM Carrito WHERE Cantidad <= 0");

        string accion = itemExistente != null ? "Cantidad actualizada en carrito" : "Agregado al carrito";
        return $"✅ {accion}: {cantidad}x {((dynamic)producto).Nombre} ({((dynamic)producto).Marca}) " +
               $"| {((dynamic)producto).Presentacion} | ${((dynamic)producto).Precio:F2} c/u " +
               $"| Subtotal: ${(decimal)((dynamic)producto).Precio * cantidad:F2}";
    }

    // ╔══════════════════════════════════════════════════════════════╗
    // ║  HERRAMIENTA: confirmar_orden                               ║
    // ╠══════════════════════════════════════════════════════════════╣
    // ║  Vacía el carrito después de confirmar la compra y          ║
    // ║  genera un número de orden. No restaura stock (la orden      ║
    // ║  fue procesada).                                            ║
    // ╚══════════════════════════════════════════════════════════════╝
    [McpServerTool, Description(
        "Confirma la compra y vacía el carrito. Genera un número de orden ORD-XXXXX. " +
        "Úsala SOLO cuando el usuario confirme explícitamente que quiere finalizar su compra.")]
    public static string confirmar_orden()
    {
        using var connection = new SqliteConnection(Data.DatabaseInitializer.ConnectionString);

        var items = connection.Query(
            @"SELECT c.IdItem, c.IdProducto, c.Cantidad, p.Precio, p.Nombre
              FROM Carrito c INNER JOIN Productos p ON c.IdProducto = p.IdProducto").ToList();

        if (items.Count == 0)
            return "Error: El carrito está vacío. No hay nada que ordenar.";

        decimal total = items.Sum(i => (decimal)((dynamic)i).Precio * (int)((dynamic)i).Cantidad);
        int numOrden = new Random().Next(10000, 99999);

        // Vaciar carrito (el stock ya se descontó al agregar)
        connection.Execute("DELETE FROM Carrito");

        return $"✅ Orden ORD-{numOrden} confirmada. Total: ${total:F2} MXN. " +
               $"Pago contra entrega. ¡Gracias por tu compra!";
    }

    // ╔══════════════════════════════════════════════════════════════╗
    // ║  HERRAMIENTA 3: ver_carrito                                  ║
    // ╠══════════════════════════════════════════════════════════════╣
    // ║  Muestra todos los artículos en el carrito con precios,      ║
    // ║  cantidades, subtotales y el total a pagar.                  ║
    // ╚══════════════════════════════════════════════════════════════╝
    [McpServerTool, Description(
        "Muestra todos los artículos que el usuario tiene en su carrito de compras, " +
        "con nombres, precios, cantidades, subtotales y el total a pagar.")]
    public static string ver_carrito()
    {
        using var connection = new SqliteConnection(Data.DatabaseInitializer.ConnectionString);

        // 📌 JOIN entre Carrito y Productos para obtener la info completa.
        // INNER JOIN = solo devuelve filas donde hay coincidencia en ambas tablas.
        var items = connection.Query(
            @"SELECT c.IdItem, p.IdProducto, p.Nombre, p.Marca, p.Presentacion, 
                     p.Precio, c.Cantidad, (p.Precio * c.Cantidad) as Subtotal 
              FROM Carrito c
              INNER JOIN Productos p ON c.IdProducto = p.IdProducto").ToList();

        if (items.Count == 0)
            return "🛒 El carrito está vacío. ¡Agrega productos para comenzar!";

        var resultado = "🛒 Carrito de compras:\n\n";
        decimal total = 0;

        foreach (var item in items)
        {
            var subtotal = (decimal)((dynamic)item).Subtotal;
            total += subtotal;
            resultado += $"- [Item #{((dynamic)item).IdItem}] {((dynamic)item).Cantidad}x {((dynamic)item).Nombre} " +
                         $"({((dynamic)item).Marca}) | {((dynamic)item).Presentacion} " +
                         $"| ${((dynamic)item).Precio:F2} c/u | Subtotal: ${subtotal:F2}\n";
        }

        resultado += $"\n💰 TOTAL A PAGAR: ${total:F2} MXN";
        resultado += $"\n📦 Total de artículos: {items.Count}";
        resultado += "\n💳 Método de pago: Contra entrega únicamente";

        return resultado;
    }

    // ╔══════════════════════════════════════════════════════════════╗
    // ║  HERRAMIENTA 4: eliminar_carrito                             ║
    // ╠══════════════════════════════════════════════════════════════╣
    // ║  Elimina un artículo del carrito usando su IdItem.           ║
    // ║  Restaura el stock del producto automáticamente.             ║
    // ╚══════════════════════════════════════════════════════════════╝
    [McpServerTool, Description(
        "Elimina un artículo específico del carrito de compras usando su IdItem " +
        "(el identificador de la línea del carrito, obtenido de ver_carrito). " +
        "Restaura el stock del producto automáticamente.")]
    public static string eliminar_carrito(
        [Description("El IdItem del artículo en el carrito a eliminar (obtenido de ver_carrito)")]
        int id_item)
    {
        using var connection = new SqliteConnection(Data.DatabaseInitializer.ConnectionString);

        // 📌 Primero obtenemos la info del item para restaurar el stock
        var itemInfo = connection.QueryFirstOrDefault(
            "SELECT c.IdProducto, c.Cantidad FROM Carrito c WHERE c.IdItem = @IdItem",
            new { IdItem = id_item });

        if (itemInfo == null)
            return $"Error: No se encontró el artículo #{id_item} en el carrito.";

        // 📌 Eliminamos del carrito y restauramos el stock
        connection.Execute("DELETE FROM Carrito WHERE IdItem = @IdItem",
            new { IdItem = id_item });

        connection.Execute(
            "UPDATE Productos SET Stock = Stock + @Cantidad WHERE IdProducto = @IdProducto",
            new { ((dynamic)itemInfo).Cantidad, ((dynamic)itemInfo).IdProducto });

        return $"✅ Artículo #{id_item} eliminado del carrito exitosamente. Stock restaurado.";
    }

    // ╔══════════════════════════════════════════════════════════════╗
    // ║  HERRAMIENTA: consultar_receta                               ║
    // ╠══════════════════════════════════════════════════════════════╣
    // ║  Devuelve los ingredientes necesarios para platillos         ║
    // ║  mexicanos típicos, con sus IDs exactos de la tienda,        ║
    // ║  precios y el combo estimado.                                ║
    // ╚══════════════════════════════════════════════════════════════╝
    [McpServerTool, Description(
        "Consulta una receta o combo de comida mexicana disponible con ingredientes de la tienda. " +
        "Platillos disponibles: chilaquiles (verdes o rojos), sincronizadas, guacamole, hot dogs, " +
        "hamburguesas, tinga de pollo, espagueti a la crema, hot cakes, ensalada de atún, " +
        "huevos al gusto con frijoles. Devuelve los IDs exactos de producto para agregarlos al carrito.")]
    public static string consultar_receta(
        [Description("Nombre del platillo o antojo (ej: 'chilaquiles', 'sincronizadas', 'guacamole', 'tinga', 'espagueti', 'hot cakes', 'hot dogs')")]
        string platillo)
    {
        using var connection = new SqliteConnection(Data.DatabaseInitializer.ConnectionString);

        var term = (platillo ?? "").ToLower();

        // Mapeo de platillo a palabras clave de búsqueda de productos
        string titulo = "";
        string descripcion = "";
        List<string> searchKeys = new();

        if (term.Contains("chilaquil"))
        {
            titulo = "🍳 Chilaquiles con Pollo y Crema";
            descripcion = "El clásico desayuno mexicano: totopos crujientes bañados en salsa con pollo deshebrado, crema y queso cotija.";
            searchKeys = new() { "Totopos de Maíz", "Salsa Verde Herdez", "Pechuga de Pollo", "Crema Ácida Lala", "Queso Cotija Rallado" };
        }
        else if (term.Contains("sincronizada") || term.Contains("quesadilla"))
        {
            titulo = "🧀 Sincronizadas Doraditas";
            descripcion = "Tortillinas de harina rellenas de queso Oaxaca derretido y jamón de pierna.";
            searchKeys = new() { "Tortillas de Harina Tía Rosa", "Queso Oaxaca Lala", "Jamón de Pierna Fud" };
        }
        else if (term.Contains("guacamol") || term.Contains("botana"))
        {
            titulo = "🥑 Guacamole Botanero Tradicional";
            descripcion = "Aguacate fresco machacado con jitomate, cebolla, cilantro, limón y serrano, acompañado de totopos.";
            searchKeys = new() { "Aguacate Hass", "Jitomate Saladette", "Cebolla Blanca", "Cilantro Fresco", "Chile Serrano", "Limón Colima", "Totopos de Maíz" };
        }
        else if (term.Contains("hot dog") || term.Contains("dogo"))
        {
            titulo = "🌭 Hot Dogs de Tiendita";
            descripcion = "Medias noches calientitas con salchicha de pavo, cátsup, mostaza y mayonesa.";
            searchKeys = new() { "Medias Noches Bimbo", "Salchichas de Pavo Fud", "Salsa Catsup Del Monte", "Mostaza Original McCormick", "Mayonesa McCormick" };
        }
        else if (term.Contains("hamburguesa"))
        {
            titulo = "🍔 Hamburguesas con Queso y Tocino";
            descripcion = "Carne a la plancha con queso amarillo derretido, tocino crujiente, jitomate y cebolla fresca.";
            searchKeys = new() { "Carne para Hamburguesas SuKarne", "Pan para Hamburguesa Bimbo", "Queso Amarillo Americano Kraft", "Tocino Ahumado Fud", "Jitomate Saladette" };
        }
        else if (term.Contains("tinga"))
        {
            titulo = "🍗 Tinga de Pollo sobre Tostadas";
            descripcion = "Pollo deshebrado guisado con cebolla, puré de tomate y chipotle sobre tostadas crujientes con crema.";
            searchKeys = new() { "Pechuga de Pollo", "Cebolla Blanca", "Puré de Tomate Del Fuerte", "Chiles Chipotles Adobados", "Tostadas Charras", "Crema Ácida Lala" };
        }
        else if (term.Contains("espagueti") || term.Contains("pasta"))
        {
            titulo = "🍝 Espagueti a la Crema y Mantequilla";
            descripcion = "Pasta cremosa suave con mantequilla Gloria, media crema y queso parmesano espolvoreado.";
            searchKeys = new() { "Espagueti La Moderna", "Media Crema Lala", "Mantequilla Gloria", "Queso Parmesano Rallado Kraft" };
        }
        else if (term.Contains("hot cake") || term.Contains("panqueque"))
        {
            titulo = "🥞 Hot Cakes Esponjosos de Domingo";
            descripcion = "Hot cakes recién hechos con mantequilla derretida, miel de maple y acompañados con leche fría.";
            searchKeys = new() { "Harina para Hot Cakes Pronto", "Leche Nutri Leche Clásica", "Mantequilla Gloria", "Miel de Maple Karo", "Huevo San Juan" };
        }
        else if (term.Contains("atun") || term.Contains("ensalada"))
        {
            titulo = "🥗 Ensalada de Atún Práctica";
            descripcion = "Atún con mayonesa, jitomate picado, toque de limón y galletas saladitas crujientes.";
            searchKeys = new() { "Atún Dolores en Agua", "Mayonesa McCormick", "Galletas Saladitas", "Jitomate Saladette", "Limón Colima" };
        }
        else
        {
            // Default: Desayuno con huevos
            titulo = "🍳 Huevitos al Gusto con Frijolitos";
            descripcion = "Huevos frescos acompañados de frijoles refritos, tortillas de maíz calientes y salsa casera.";
            searchKeys = new() { "Huevo San Juan", "Frijoles Negros La Costeña", "Tortillas de Maíz Milpa Real", "Salsa Casera Mexicana Herdez" };
        }

        var resultado = $"📖 Receta sugerida: {titulo}\n{descripcion}\n\n🛒 Ingredientes disponibles en la tienda:\n";
        decimal totalCombo = 0;
        List<int> idsProductos = new();

        foreach (var key in searchKeys)
        {
            var p = connection.QueryFirstOrDefault(
                @"SELECT IdProducto, Nombre, Marca, Presentacion, Precio, Stock 
                  FROM Productos 
                  WHERE Nombre LIKE @K OR DescripcionIA LIKE @K 
                  LIMIT 1",
                new { K = $"%{key}%" });

            if (p != null)
            {
                int id = (int)((dynamic)p).IdProducto;
                decimal precio = (decimal)((dynamic)p).Precio;
                totalCombo += precio;
                idsProductos.Add(id);
                resultado += $"- [ID:{id}] {((dynamic)p).Nombre} ({((dynamic)p).Presentacion}) — ${precio:F2}\n";
            }
        }

        resultado += $"\n💰 Total estimado del combo: ${totalCombo:F2} MXN";
        resultado += $"\n💡 Para agregar todos estos ingredientes al carrito usa los IDs: [{string.Join(", ", idsProductos)}].";

        return resultado;
    }

    // ╔══════════════════════════════════════════════════════════════╗
    // ║  HERRAMIENTA 5: obtener_contexto_tienda                      ║
    // ╠══════════════════════════════════════════════════════════════╣
    // ║  Devuelve el catálogo completo + carrito en formato compacto ║
    // ║  para el WebApi (Single-Turn Action).                        ║
    // ╚══════════════════════════════════════════════════════════════╝
    [McpServerTool, Description(
        "Obtiene el contexto completo de la tienda: catálogo de productos con DescripcionIA " +
        "y el carrito actual del usuario. Útil para darle toda la información al modelo de IA " +
        "en una sola interacción (Single-Turn Action).")]
    public static string obtener_contexto_tienda()
    {
        using var connection = new SqliteConnection(Data.DatabaseInitializer.ConnectionString);

        // Catálogo compacto: solo los campos esenciales para la IA
        // No enviamos la DescripcionIA completa — usamos solo los primeros 4 tags
        var productos = connection.Query(
            @"SELECT IdProducto, Nombre, Marca, Categoria, Presentacion, 
                     Precio, CaloriasPorPorcion, AptoDiabeticos, Popularidad, Stock, ImagenUrl, DescripcionIA
              FROM Productos 
              ORDER BY Categoria, Popularidad DESC").ToList();

        var carrito = connection.Query(
            @"SELECT c.IdItem, p.IdProducto, p.Nombre, p.Marca, p.Presentacion, 
                     p.Precio, c.Cantidad, (p.Precio * c.Cantidad) as Subtotal 
              FROM Carrito c
              INNER JOIN Productos p ON c.IdProducto = p.IdProducto").ToList();

        // Formato compacto para el WebApi (parseo por línea)
        var contexto = "=== CATÁLOGO DE PRODUCTOS ===\n";
        foreach (var p in productos)
        {
            // Abreviar DescripcionIA — solo primeros 5 tags para reducir tokens
            string descFull = (string)(((dynamic)p).DescripcionIA ?? "");
            string tags = string.Join("|", descFull.Split('|').Take(5));
            string url = (string)(((dynamic)p).ImagenUrl ?? "");

            contexto += $"[{((dynamic)p).IdProducto}] {((dynamic)p).Nombre} | {((dynamic)p).Marca} " +
                        $"| {((dynamic)p).Categoria} | {((dynamic)p).Presentacion} " +
                        $"| ${((dynamic)p).Precio:F2} | {((dynamic)p).CaloriasPorPorcion}cal " +
                        $"| {(((dynamic)p).AptoDiabeticos == 1 ? "APT-DIAB" : "NO-DIAB")} " +
                        $"| Stock:{((dynamic)p).Stock} " +
                        $"| Img:{url} " +
                        $"| Tags:{tags}\n";
        }

        contexto += "\n=== CARRITO ACTUAL ===\n";
        if (carrito.Count == 0)
        {
            contexto += "(vacío)\n";
        }
        else
        {
            decimal total = 0;
            foreach (var item in carrito)
            {
                var subtotal = (decimal)((dynamic)item).Subtotal;
                total += subtotal;
                contexto += $"[Item#{((dynamic)item).IdItem}] {((dynamic)item).Cantidad}x " +
                            $"{((dynamic)item).Nombre} ({((dynamic)item).Marca}) " +
                            $"| ${((dynamic)item).Precio:F2} c/u | Subtotal: ${subtotal:F2}\n";
            }
            contexto += $"TOTAL: ${total:F2} MXN\n";
        }

        return contexto;
    }
}
