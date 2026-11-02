// ╔══════════════════════════════════════════════════════════════════════╗
// ║  DatabaseInitializer.cs — Inicialización de Base de Datos SQLite   ║
// ╠══════════════════════════════════════════════════════════════════════╣
// ║                                                                    ║
// ║  📚 ¿QUÉ HACE ESTE ARCHIVO?                                       ║
// ║  Este archivo se encarga de crear las tablas de la base de datos    ║
// ║  y llenarlas con productos de ejemplo. Solo se ejecuta al inicio   ║
// ║  de la aplicación.                                                 ║
// ║                                                                    ║
// ║  🆕 CAMPO "DescripcionIA":                                         ║
// ║  Cada producto tiene un campo especial llamado "DescripcionIA".    ║
// ║  Es un texto compacto con keywords y sinónimos que la IA usa      ║
// ║  para encontrar productos más rápido. En vez de leer 10 columnas, ║
// ║  la IA lee 1 sola cadena con toda la info relevante.              ║
// ║                                                                    ║
// ║  Ejemplo: "coca-cola|refresco|bebida|gaseosa|600ml|$20|popular"   ║
// ║                                                                    ║
// ║  💡 CONCEPTO: Esto es una versión simplificada de "embeddings"     ║
// ║  (vectores semánticos). Los embeddings reales son arrays de        ║
// ║  números como [0.234, -0.891, ...] que la IA compara              ║
// ║  matemáticamente. Para una demo con 50 productos, esta versión    ║
// ║  de texto plano funciona igual de bien y no necesita librerías    ║
// ║  extra.                                                            ║
// ╚══════════════════════════════════════════════════════════════════════╝

using Microsoft.Data.Sqlite;
using Dapper;

namespace TiendaMCP.Server.Data;

/// <summary>
/// Se encarga de crear las tablas e insertar los datos iniciales.
/// Se llama una sola vez al arrancar el servidor MCP.
/// </summary>
public static class DatabaseInitializer
{
    // 📌 La cadena de conexión a SQLite. "Data Source=..." indica el archivo .db
    // SQLite es una base de datos "embebida" — todo vive en un solo archivo.
    public const string ConnectionString = "Data Source=tiendita.db";

    /// <summary>
    /// Crea las tablas si no existen y llena con datos de ejemplo.
    /// </summary>
    public static void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);

        // ─── CREAR TABLAS ─────────────────────────────────────────
        // DROP + CREATE para asegurar el schema actualizado con DescripcionIA
        connection.Execute("DROP TABLE IF EXISTS Carrito;");
        connection.Execute("DROP TABLE IF EXISTS Productos;");

        // 📌 CREATE TABLE define la estructura de cada tabla.
        // - INTEGER PRIMARY KEY AUTOINCREMENT = ID auto-incrementable
        // - TEXT NOT NULL = campo de texto obligatorio
        // - REAL = número decimal (para precios)
        // - BOOLEAN = verdadero/falso (0 o 1 en SQLite)
        // - FOREIGN KEY = referencia a otra tabla (integridad referencial)
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Productos (
                IdProducto INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                Marca TEXT,
                Categoria TEXT,
                Presentacion TEXT,
                Precio REAL NOT NULL,
                CaloriasPorPorcion INTEGER,
                AptoDiabeticos BOOLEAN DEFAULT 0,
                Popularidad INTEGER DEFAULT 50,
                Stock INTEGER DEFAULT 100,
                ImagenUrl TEXT,
                DescripcionIA TEXT  -- 🆕 Campo optimizado para búsqueda por IA
            );

            CREATE TABLE IF NOT EXISTS Carrito (
                IdItem INTEGER PRIMARY KEY AUTOINCREMENT,
                IdProducto INTEGER,
                Cantidad INTEGER,
                FOREIGN KEY(IdProducto) REFERENCES Productos(IdProducto)
            );
        ");

        // ─── INSERTAR DATOS DE EJEMPLO ─────────────────────────────
        // Solo insertamos si la tabla está vacía (COUNT = 0)
        var count = connection.QuerySingle<int>("SELECT COUNT(*) FROM Productos");
        if (count == 0)
        {
            InsertarProductos(connection);
        }
    }

    /// <summary>
    /// Inserta los 50 productos mexicanos con su DescripcionIA pre-generada.
    /// 
    /// 📌 DescripcionIA contiene:
    /// - Nombre y sinónimos populares (ej: "refresco", "gaseosa", "soda")
    /// - Categoría y subcategoría
    /// - Características de salud (apto/no-apto diabéticos, calorías)
    /// - Keywords que un usuario mexicano usaría para buscar
    /// - Precio y presentación
    /// 
    /// Esto permite que la IA encuentre "Coca-Cola" aunque el usuario diga
    /// "dame un refresco" o "quiero una soda".
    /// </summary>
    private static void InsertarProductos(SqliteConnection connection)
    {
        connection.Execute(@"
            INSERT INTO Productos (Nombre, Marca, Categoria, Presentacion, Precio, CaloriasPorPorcion, AptoDiabeticos, Popularidad, Stock, ImagenUrl, DescripcionIA) VALUES 
            -- ═══ BEBIDAS ═══
            ('Coca-Cola Original', 'Coca-Cola', 'Bebidas', '600ml', 20.00, 250, 0, 95, 120, 
             'https://images.openfoodfacts.org/images/products/000/007/500/7614/front_es.40.400.jpg',
             'coca-cola|refresco|gaseosa|soda|bebida|cola|azucarada|600ml|$20|250cal|NO-diabeticos|popular:95|fria|antojo'),

            ('Coca-Cola Sin Azúcar', 'Coca-Cola', 'Bebidas', '600ml', 20.00, 0, 1, 88, 100, 
             'https://images.openfoodfacts.org/images/products/750/105/068/7835/front_es.26.400.jpg',
             'coca-cola-sin-azucar|zero|light|refresco|gaseosa|soda|dieta|bebida|600ml|$20|0cal|SI-diabeticos|popular:88|saludable'),

            ('Coca-Cola Original', 'Coca-Cola', 'Bebidas', '2L', 38.00, 250, 0, 90, 80, 
             'https://images.openfoodfacts.org/images/products/000/007/500/7614/front_es.40.400.jpg',
             'coca-cola|refresco|gaseosa|soda|bebida|cola|familiar|grande|2-litros|2L|$38|250cal|NO-diabeticos|popular:90|fiesta|reunion'),

            ('Coca-Cola Sin Azúcar', 'Coca-Cola', 'Bebidas', '2L', 38.00, 0, 1, 82, 70, 
             'https://images.openfoodfacts.org/images/products/750/105/068/7835/front_es.26.400.jpg',
             'coca-cola-sin-azucar|zero|light|refresco|gaseosa|soda|dieta|familiar|grande|2-litros|2L|$38|0cal|SI-diabeticos|popular:82'),

            ('Pepsi Regular', 'Pepsi', 'Bebidas', '600ml', 18.00, 240, 0, 70, 90, 
             'https://images.openfoodfacts.org/images/products/750/103/560/0080/front_es.5.400.jpg',
             'pepsi|refresco|gaseosa|soda|bebida|cola|600ml|$18|240cal|NO-diabeticos|popular:70|economica|barata'),

            ('Pepsi Black Sin Azúcar', 'Pepsi', 'Bebidas', '600ml', 18.00, 0, 1, 55, 60, 
             'https://images.openfoodfacts.org/images/products/750/103/560/0165/front_es.3.400.jpg',
             'pepsi-black|pepsi-sin-azucar|zero|light|refresco|gaseosa|soda|dieta|600ml|$18|0cal|SI-diabeticos|popular:55'),

            ('Agua Mineral Topo Chico', 'Topo Chico', 'Bebidas', '600ml', 15.00, 0, 1, 85, 150, 
             'https://images.openfoodfacts.org/images/products/002/113/601/0541/front_en.15.400.jpg',
             'topo-chico|agua-mineral|agua-con-gas|sparkling|bebida|hidratacion|600ml|$15|0cal|SI-diabeticos|popular:85|refrescante|natural'),

            ('Agua Natural Epura', 'Epura', 'Bebidas', '1L', 12.00, 0, 1, 80, 200, 
             'https://images.openfoodfacts.org/images/products/750/108/680/1046/front_es.31.400.jpg',
             'epura|agua-natural|agua-pura|agua-simple|bebida|hidratacion|1-litro|1L|$12|0cal|SI-diabeticos|popular:80|basica|esencial'),

            ('Jugo Del Valle Naranja', 'Del Valle', 'Bebidas', '1L', 28.00, 110, 0, 72, 60, 
             'https://images.openfoodfacts.org/images/products/750/105/067/0010/front_es.8.400.jpg',
             'del-valle|jugo|naranja|jugo-de-naranja|bebida|fruta|vitamina-c|1-litro|1L|$28|110cal|NO-diabeticos|popular:72|desayuno'),

            ('Jugo Del Valle Light Naranja', 'Del Valle', 'Bebidas', '1L', 30.00, 35, 1, 45, 40, 
             'https://images.openfoodfacts.org/images/products/750/105/067/3516/front_es.4.400.jpg',
             'del-valle-light|jugo|naranja|jugo-de-naranja|light|bajo-calorias|bebida|fruta|1-litro|1L|$30|35cal|SI-diabeticos|popular:45|saludable'),

            -- ═══ LÁCTEOS ═══
            ('Leche Nutri Leche Clásica', 'Nutri Leche', 'Lácteos', '1L', 24.00, 150, 0, 88, 100, 
             'https://images.openfoodfacts.org/images/products/750/102/054/0666/front_es.63.400.jpg',
             'nutri-leche|leche|leche-entera|lacteo|calcio|proteina|1-litro|1L|$24|150cal|NO-diabeticos|popular:88|basica|desayuno|cereal'),

            ('Leche Alpura Deslactosada', 'Alpura', 'Lácteos', '1L', 30.00, 130, 1, 78, 80, 
             'https://images.openfoodfacts.org/images/products/750/105/590/1425/front_es.47.400.jpg',
             'alpura|leche|deslactosada|sin-lactosa|lacteo|calcio|proteina|1-litro|1L|$30|130cal|SI-diabeticos|popular:78|intolerancia-lactosa|digestion'),

            ('Leche Lala Deslactosada', 'Lala', 'Lácteos', '1L', 29.00, 130, 1, 75, 70, 
             'https://images.openfoodfacts.org/images/products/750/102/056/5911/front_es.22.400.jpg',
             'lala|leche|deslactosada|sin-lactosa|lacteo|calcio|proteina|1-litro|1L|$29|130cal|SI-diabeticos|popular:75|intolerancia-lactosa'),

            ('Leche Santa Clara Entera', 'Santa Clara', 'Lácteos', '1L', 32.00, 160, 0, 65, 50, 
             'https://images.openfoodfacts.org/images/products/750/129/560/0126/front_en.48.400.jpg',
             'santa-clara|leche|entera|premium|lacteo|calcio|proteina|1-litro|1L|$32|160cal|NO-diabeticos|popular:65|alta-calidad'),

            ('Leche Santa Clara Deslactosada', 'Santa Clara', 'Lácteos', '1L', 34.00, 130, 1, 60, 45, 
             'https://images.openfoodfacts.org/images/products/750/129/560/0157/front_es.60.400.jpg',
             'santa-clara|leche|deslactosada|sin-lactosa|premium|lacteo|1-litro|1L|$34|130cal|SI-diabeticos|popular:60|alta-calidad'),

            ('Media Crema Lala', 'Lala', 'Lácteos', '250ml', 18.00, 80, 0, 70, 90, 
             'https://images.openfoodfacts.org/images/products/750/102/051/5299/front_es.36.400.jpg',
             'lala|media-crema|crema|lacteo|cocina|receta|250ml|$18|80cal|NO-diabeticos|popular:70|cocinar|cafe|postre'),

            ('Yogurt Griego Oikos Natural', 'Oikos', 'Lácteos', '150g', 22.00, 90, 1, 60, 40, 
             'https://images.openfoodfacts.org/images/products/750/644/310/1074/front_es.43.400.jpg',
             'oikos|yogurt|griego|natural|lacteo|proteina|snack|150g|$22|90cal|SI-diabeticos|popular:60|saludable|fitness|bajo-calorias'),

            ('Yogurt Griego Yoplait Sin Azúcar', 'Yoplait', 'Lácteos', '150g', 20.00, 70, 1, 55, 45, 
             'https://images.openfoodfacts.org/images/products/750/104/009/5177/front_en.53.400.jpg',
             'yoplait|yogurt|griego|sin-azucar|lacteo|proteina|snack|150g|$20|70cal|SI-diabeticos|popular:55|saludable|fitness|dieta'),

            -- ═══ BOTANAS Y GALLETAS ═══
            ('Galletas Emperador Chocolate', 'Gamesa', 'Botanas', '117g', 22.00, 300, 0, 82, 80, 
             'https://images.openfoodfacts.org/images/products/750/100/066/3729/front_es.7.400.jpg',
             'emperador|galletas|chocolate|gamesa|botana|snack|dulce|antojo|117g|$22|300cal|NO-diabeticos|popular:82|lonche|escuela'),

            ('Galletas Saladitas', 'Gamesa', 'Botanas', '186g', 18.00, 130, 0, 78, 90, 
             'https://images.openfoodfacts.org/images/products/750/100/066/4221/front_es.25.400.jpg',
             'saladitas|galletas|saladas|gamesa|botana|snack|salado|186g|$18|130cal|NO-diabeticos|popular:78|lonche|antojo'),

            ('Doritos Nacho', 'Doritos', 'Botanas', '62g', 22.00, 280, 0, 85, 100, 
             'https://images.openfoodfacts.org/images/products/750/101/112/3588/front_es.30.400.jpg',
             'doritos|nacho|frituras|botana|snack|maiz|queso|antojo|62g|$22|280cal|NO-diabeticos|popular:85|fiesta|reunion'),

            ('Takis Fuego', 'Barcel', 'Botanas', '56g', 18.00, 250, 0, 90, 110, 
             'https://images.openfoodfacts.org/images/products/750/103/042/4536/front_es.14.400.jpg',
             'takis|fuego|picante|chile|limon|barcel|botana|snack|frituras|antojo|56g|$18|250cal|NO-diabeticos|popular:90|fiesta|escuela'),

            ('Salmas Horneadas', 'Sanissimo', 'Botanas', '120g', 28.00, 80, 1, 65, 50, 
             'https://images.openfoodfacts.org/images/products/065/848/000/1101/front_en.39.400.jpg',
             'salmas|sanissimo|horneadas|galletas|botana|snack|saludable|bajo-calorias|120g|$28|80cal|SI-diabeticos|popular:65|dieta|fitness'),

            ('Nueces Mixtas', 'De la Rosa', 'Botanas', '100g', 35.00, 150, 1, 50, 30, 
             'https://images.openfoodfacts.org/images/products/750/117/600/7002/front_es.3.400.jpg',
             'nueces|mixtas|de-la-rosa|frutos-secos|botana|snack|saludable|proteina|100g|$35|150cal|SI-diabeticos|popular:50|natural|energia'),

            ('Rice Cakes con Quinoa', 'OKKO Superfoods', 'Botanas', '120g', 42.00, 60, 1, 40, 25, 
             'https://images.openfoodfacts.org/images/products/750/032/704/7878/front_en.4.400.jpg',
             'rice-cakes|quinoa|okko|superfoods|galletas-de-arroz|botana|snack|saludable|bajo-calorias|120g|$42|60cal|SI-diabeticos|popular:40|fitness|dieta|vegano'),

            ('Brown Rice Cakes', 'OKKO Superfoods', 'Botanas', '120g', 40.00, 55, 1, 38, 20, 
             'https://images.openfoodfacts.org/images/products/750/032/704/7854/front_en.65.400.jpg',
             'brown-rice-cakes|arroz-integral|okko|superfoods|galletas-de-arroz|botana|snack|saludable|bajo-calorias|120g|$40|55cal|SI-diabeticos|popular:38|fitness|integral'),

            -- ═══ PAN Y PANADERÍA ═══
            ('Pan Blanco Bimbo', 'Bimbo', 'Panadería', '680g', 52.00, 140, 0, 92, 60, 
             'https://images.openfoodfacts.org/images/products/750/081/002/9183/front_en.59.400.jpg',
             'bimbo|pan|pan-blanco|pan-de-caja|panaderia|sandwich|680g|$52|140cal|NO-diabeticos|popular:92|basico|desayuno|lonche'),

            ('Pan Integral Bimbo', 'Bimbo', 'Panadería', '680g', 58.00, 120, 1, 75, 50, 
             'https://images.openfoodfacts.org/images/products/750/081/002/2061/front_en.32.400.jpg',
             'bimbo|pan|pan-integral|pan-de-caja|fibra|panaderia|sandwich|saludable|680g|$58|120cal|SI-diabeticos|popular:75|desayuno|dieta'),

            ('Pan Bimbo Cero Cero', 'Bimbo', 'Panadería', '530g', 65.00, 70, 1, 68, 40, 
             'https://images.openfoodfacts.org/images/products/750/103/046/7090/front_en.112.400.jpg',
             'bimbo|cero-cero|pan|sin-azucar|sin-grasa|panaderia|saludable|bajo-calorias|530g|$65|70cal|SI-diabeticos|popular:68|dieta|fitness'),

            ('Pan Tostado Clásico', 'Bimbo', 'Panadería', '288g', 38.00, 110, 0, 60, 45, 
             'https://images.openfoodfacts.org/images/products/750/100/011/1800/front_en.32.400.jpg',
             'bimbo|pan-tostado|tostadas|panaderia|desayuno|288g|$38|110cal|NO-diabeticos|popular:60|crujiente'),

            ('Pan Oroweat 12 Granos', 'Oroweat', 'Panadería', '600g', 72.00, 90, 1, 48, 30, 
             'https://images.openfoodfacts.org/images/products/750/100/037/5295/front_en.62.400.jpg',
             'oroweat|12-granos|multigrano|pan|integral|semillas|premium|panaderia|saludable|600g|$72|90cal|SI-diabeticos|popular:48|fitness|gourmet'),

            ('Medias Noches Bimbo', 'Bimbo', 'Panadería', '8 pzas', 42.00, 160, 0, 70, 55, 
             'https://images.openfoodfacts.org/images/products/750/100/011/1503/front_es.48.400.jpg',
             'bimbo|medias-noches|pan-dulce|hot-dog|panaderia|8-piezas|$42|160cal|NO-diabeticos|popular:70|fiesta|antojo'),

            -- ═══ PASTELERÍA / DULCES ═══
            ('Gansito Marinela', 'Marinela', 'Pastelería', '50g', 16.00, 280, 0, 93, 120, 
             'https://images.openfoodfacts.org/images/products/750/100/015/3107/front_es.48.400.jpg',
             'gansito|marinela|pastelito|chocolate|fresa|dulce|postre|snack|antojo|50g|$16|280cal|NO-diabeticos|popular:93|lonche|escuela|favorito'),

            ('Triki Trakes Marinela', 'Marinela', 'Pastelería', '47g', 14.00, 250, 0, 75, 80, 
             'https://images.openfoodfacts.org/images/products/750/100/014/0855/front_en.36.400.jpg',
             'triki-trakes|marinela|galleta|chocolate|dulce|postre|snack|antojo|47g|$14|250cal|NO-diabeticos|popular:75|lonche|escuela'),

            -- ═══ CEREALES ═══
            ('Cheerios Avena y Más Granos', 'Nestlé', 'Cereales', '480g', 68.00, 110, 1, 72, 40, 
             'https://images.openfoodfacts.org/images/products/750/647/510/1158/front_es.46.400.jpg',
             'cheerios|nestle|cereal|avena|granos|fibra|desayuno|saludable|480g|$68|110cal|SI-diabeticos|popular:72|familia|integral'),

            -- ═══ DESPENSA ═══
            ('Frijoles Negros La Costeña', 'La Costeña', 'Despensa', '560g', 24.00, 120, 1, 88, 100, 
             'https://images.openfoodfacts.org/images/products/750/200/054/5064/front_es.5.400.jpg',
             'la-costena|frijoles|negros|frijoles-negros|legumbre|despensa|basico|proteina|fibra|560g|$24|120cal|SI-diabeticos|popular:88|comida-mexicana|esencial'),

            ('Frijoles Bayos La Costeña', 'La Costeña', 'Despensa', '560g', 24.00, 120, 1, 80, 80, 
             'https://images.openfoodfacts.org/images/products/750/200/054/5057/front_es.4.400.jpg',
             'la-costena|frijoles|bayos|frijoles-bayos|legumbre|despensa|basico|proteina|fibra|560g|$24|120cal|SI-diabeticos|popular:80|comida-mexicana'),

            ('Atún Dolores en Agua', 'Dolores', 'Despensa', '133g', 25.00, 90, 1, 82, 90, 
             'https://images.openfoodfacts.org/images/products/750/204/206/3157/front_es.3.400.jpg',
             'dolores|atun|atun-en-agua|pescado|proteina|despensa|enlatado|saludable|bajo-calorias|133g|$25|90cal|SI-diabeticos|popular:82|dieta|rapido'),

            ('Atún Dolores en Aceite', 'Dolores', 'Despensa', '133g', 26.00, 120, 0, 78, 80, 
             'https://images.openfoodfacts.org/images/products/750/204/206/3164/front_es.3.400.jpg',
             'dolores|atun|atun-en-aceite|pescado|proteina|despensa|enlatado|133g|$26|120cal|NO-diabeticos|popular:78|rapido'),

            ('Arroz Verde Valle', 'Verde Valle', 'Despensa', '900g', 32.00, 160, 0, 85, 70, 
             'https://images.openfoodfacts.org/images/products/750/306/900/0325/front_es.4.400.jpg',
             'verde-valle|arroz|arroz-blanco|grano|despensa|basico|guarnicion|900g|$32|160cal|NO-diabeticos|popular:85|comida-mexicana|esencial'),

            ('Arroz Integral Verde Valle', 'Verde Valle', 'Despensa', '900g', 38.00, 140, 1, 55, 40, 
             'https://images.openfoodfacts.org/images/products/750/306/900/0349/front_es.3.400.jpg',
             'verde-valle|arroz|arroz-integral|grano|fibra|despensa|saludable|900g|$38|140cal|SI-diabeticos|popular:55|dieta|fitness'),

            ('Aceite Nutrioli', 'Nutrioli', 'Despensa', '946ml', 48.00, 120, 1, 90, 60, 
             'https://images.openfoodfacts.org/images/products/750/106/030/0027/front_es.3.400.jpg',
             'nutrioli|aceite|aceite-vegetal|cocina|freir|despensa|basico|946ml|$48|120cal|SI-diabeticos|popular:90|esencial|cocinar'),

            ('Azúcar Morena Zulka', 'Zulka', 'Despensa', '1kg', 35.00, 50, 0, 75, 80, 
             'https://images.openfoodfacts.org/images/products/750/502/800/0023/front_es.3.400.jpg',
             'zulka|azucar|azucar-morena|endulzante|despensa|basico|reposteria|cafe|1kg|$35|50cal|NO-diabeticos|popular:75|cocinar|hornear'),

            ('Splenda Endulzante', 'Splenda', 'Despensa', '100 sobres', 95.00, 0, 1, 60, 25, 
             'https://images.openfoodfacts.org/images/products/072/220/015/0310/front_en.11.400.jpg',
             'splenda|endulzante|sustituto-azucar|sin-azucar|sin-calorias|despensa|cafe|saludable|100-sobres|$95|0cal|SI-diabeticos|popular:60|dieta'),

            -- ═══ SALSAS Y CONDIMENTOS ═══
            ('Salsa Valentina Etiqueta Amarilla', 'Valentina', 'Salsas', '370ml', 18.00, 5, 1, 88, 100, 
             'https://images.openfoodfacts.org/images/products/750/610/035/1038/front_es.3.400.jpg',
             'valentina|salsa|salsa-picante|picante|chile|amarilla|suave|condimento|370ml|$18|5cal|SI-diabeticos|popular:88|mexicana|antojo'),

            ('Salsa Valentina Etiqueta Negra', 'Valentina', 'Salsas', '370ml', 18.00, 5, 1, 82, 90, 
             'https://images.openfoodfacts.org/images/products/750/610/035/1069/front_es.3.400.jpg',
             'valentina|salsa|salsa-picante|picante|chile|negra|extra-picante|condimento|370ml|$18|5cal|SI-diabeticos|popular:82|mexicana|fuerte'),

            ('Mayonesa McCormick', 'McCormick', 'Salsas', '390g', 38.00, 100, 0, 80, 70, 
             'https://images.openfoodfacts.org/images/products/750/702/200/1064/front_es.3.400.jpg',
             'mccormick|mayonesa|mayo|aderezo|condimento|sandwich|ensalada|390g|$38|100cal|NO-diabeticos|popular:80|cocina'),

            ('Knorr Caldo de Pollo', 'Knorr', 'Despensa', '8 cubos', 14.00, 15, 0, 85, 110, 
             'https://images.openfoodfacts.org/images/products/750/100/839/3706/front_es.4.400.jpg',
             'knorr|caldo|caldo-de-pollo|sazonador|condimento|despensa|cocina|sopa|8-cubos|$14|15cal|NO-diabeticos|popular:85|basico|esencial'),

            -- ═══ BARRAS ENERGÉTICAS / SALUDABLES ═══
            ('Barra Nature Valley Avena y Miel', 'Nature Valley', 'Barras', '42g', 22.00, 190, 0, 65, 40, 
             'https://images.openfoodfacts.org/images/products/001/600/014/7861/front_en.44.400.jpg',
             'nature-valley|barra|barra-energetica|avena|miel|snack|granola|energia|42g|$22|190cal|NO-diabeticos|popular:65|fitness|deporte|rapido'),

            -- ═══ NUEVOS PRODUCTOS (FRUTAS/VERDURAS/CARNES) ═══
            ('Limón Colima (Malla)', 'Granel', 'Frutas y Verduras', '500g', 35.00, 15, 1, 95, 60,
             'https://images.openfoodfacts.org/images/products/750/300/113/3745/front_es.3.400.jpg',
             'limon|limones|colima|verdura|fruta|fresco|malla|500g|$35|15cal|SI-diabeticos|popular:95|cocina|mexicana|acido'),

            ('Aguacate Hass (Malla)', 'Granel', 'Frutas y Verduras', '500g', 65.00, 160, 1, 98, 40,
             'https://images.openfoodfacts.org/images/products/070/462/460/2481/front_fr.4.400.jpg',
             'aguacate|palta|hass|verdura|fruta|fresco|malla|500g|$65|160cal|SI-diabeticos|popular:98|guacamole|mexicana|sano'),

            ('Jitomate Saladette', 'Granel', 'Frutas y Verduras', '1kg', 45.00, 18, 1, 90, 80,
             'https://images.openfoodfacts.org/images/products/750/300/113/3752/front_es.3.400.jpg',
             'jitomate|tomate|saladette|rojo|verdura|fresco|1kg|$45|18cal|SI-diabeticos|popular:90|salsa|cocina'),

            ('Tortillas de Maíz Milpa Real', 'Milpa Real', 'Despensa', '1kg', 24.00, 210, 1, 100, 150,
             'https://images.openfoodfacts.org/images/products/750/103/048/0105/front_es.8.400.jpg',
             'tortillas|maiz|milpa-real|comida|mexicana|tacos|1kg|$24|210cal|SI-diabeticos|popular:100|basico|esencial'),

            ('Huevo San Juan (Docena)', 'San Juan', 'Despensa', '12 pzas', 42.00, 70, 1, 96, 80,
             'https://images.openfoodfacts.org/images/products/750/300/113/3714/front_es.3.400.jpg',
             'huevo|huevos|san-juan|docena|blanco|proteina|desayuno|12-piezas|$42|70cal|SI-diabeticos|popular:96|basico'),

            ('Queso Panela Fud', 'Fud', 'Lácteos', '400g', 68.00, 220, 1, 88, 50,
             'https://images.openfoodfacts.org/images/products/750/104/000/1888/front_es.4.400.jpg',
             'queso|panela|fud|fresco|lacteo|proteina|saludable|400g|$68|220cal|SI-diabeticos|popular:88|dieta|ensalada'),

            ('Salchichas de Pavo Fud', 'Fud', 'Embutidos', '500g', 55.00, 150, 0, 85, 90,
             'https://images.openfoodfacts.org/images/products/750/104/000/1178/front_es.4.400.jpg',
             'salchicha|salchichas|pavo|fud|embutido|carne|hot-dog|500g|$55|150cal|NO-diabeticos|popular:85|rapido|desayuno'),

            ('Jamón Virginia de Pavo Fud', 'Fud', 'Embutidos', '250g', 48.00, 90, 1, 90, 70,
             'https://images.openfoodfacts.org/images/products/750/104/000/1109/front_es.3.400.jpg',
             'jamon|pavo|virginia|fud|embutido|carne|sandwich|250g|$48|90cal|SI-diabeticos|popular:90|lonche|escuela'),

            -- ═══ INGREDIENTES PARA RECETAS ═══
            ('Harina para Hot Cakes Pronto', 'Pronto', 'Despensa', '800g', 38.00, 180, 0, 85, 60,
             'https://images.openfoodfacts.org/images/products/750/101/903/1021/front_es.3.400.jpg',
             'harina|hot-cakes|hotcakes|pronto|desayuno|dulce|receta|800g|$38|180cal|NO-diabeticos|popular:85|panqueques'),

            ('Miel de Maple Karo', 'Karo', 'Despensa', '250ml', 42.00, 150, 0, 80, 50,
             'https://images.openfoodfacts.org/images/products/750/100/520/0035/front_es.3.400.jpg',
             'miel|maple|karo|jarabe|dulce|hot-cakes|desayuno|250ml|$42|150cal|NO-diabeticos|popular:80|postre'),

            ('Mantequilla Gloria', 'Gloria', 'Lácteos', '90g', 24.00, 100, 1, 95, 80,
             'https://images.openfoodfacts.org/images/products/750/101/804/4039/front_es.3.400.jpg',
             'mantequilla|gloria|lacteo|grasa|cocina|desayuno|hot-cakes|90g|$24|100cal|SI-diabeticos|popular:95|esencial'),

            ('Carne para Hamburguesas SuKarne', 'SuKarne', 'Carnes', '8 pzas', 95.00, 250, 1, 75, 40,
             'https://images.openfoodfacts.org/images/products/750/229/905/0002/front_es.3.400.jpg',
             'carne|hamburguesa|sukarne|res|parrillada|comida|cena|8-piezas|$95|250cal|SI-diabeticos|popular:75|fiesta'),

            ('Pan para Hamburguesa Bimbo', 'Bimbo', 'Panadería', '8 pzas', 48.00, 150, 0, 85, 60,
             'https://images.openfoodfacts.org/images/products/750/100/011/1404/front_es.3.400.jpg',
             'pan|hamburguesa|bimbo|panaderia|comida|cena|8-piezas|$48|150cal|NO-diabeticos|popular:85|fiesta'),

            ('Pasta para Sopa de Fideo La Moderna', 'La Moderna', 'Despensa', '200g', 12.00, 110, 1, 98, 120,
             'https://images.openfoodfacts.org/images/products/750/101/831/0004/front_es.3.400.jpg',
             'pasta|sopa|fideo|la-moderna|comida|caldo|mexicana|200g|$12|110cal|SI-diabeticos|popular:98|basico|esencial'),

            ('Puré de Tomate Del Fuerte', 'Del Fuerte', 'Despensa', '210g', 14.00, 30, 1, 95, 100,
             'https://images.openfoodfacts.org/images/products/750/100/315/1013/front_es.4.400.jpg',
             'pure|tomate|del-fuerte|salsa|sopa|cocina|despensa|210g|$14|30cal|SI-diabeticos|popular:95|basico|esencial'),

            ('Tostadas Charras', 'Charras', 'Despensa', '350g', 32.00, 110, 1, 88, 70,
             'https://images.openfoodfacts.org/images/products/750/118/312/1001/front_es.4.400.jpg',
             'tostadas|charras|maiz|comida|cena|mexicana|tinga|350g|$32|110cal|SI-diabeticos|popular:88|crujiente'),

            ('Pechuga de Pollo', 'Granel', 'Carnes', '1kg', 110.00, 165, 1, 90, 50,
             'https://images.openfoodfacts.org/images/products/750/300/113/3707/front_es.3.400.jpg',
             'pollo|pechuga|carne|tinga|comida|cena|saludable|proteina|1kg|$110|165cal|SI-diabeticos|popular:90|dieta'),

            ('Cebolla Blanca', 'Granel', 'Frutas y Verduras', '1kg', 28.00, 20, 1, 95, 80,
             'https://images.openfoodfacts.org/images/products/750/300/113/3769/front_es.3.400.jpg',
             'cebolla|blanca|verdura|cocina|comida|mexicana|1kg|$28|20cal|SI-diabeticos|popular:95|basico|esencial'),

            -- ═══ CHILAQUILES, ANTOJITOS Y SALSAS DE COMIDA ═══
            ('Totopos de Maíz Milpa Real', 'Milpa Real', 'Despensa', '280g', 36.00, 140, 1, 95, 80,
             'https://images.openfoodfacts.org/images/products/750/103/048/0112/front_es.3.400.jpg',
             'totopos|totopo|nachos|chilaquiles|milpa-real|maiz|botana|guacamole|despensa|280g|$36|140cal|SI-diabeticos|popular:95|receta|crujiente'),

            ('Salsa Verde Herdez', 'Herdez', 'Salsas', '453g', 26.00, 20, 1, 94, 90,
             'https://images.openfoodfacts.org/images/products/750/100/310/2404/front_es.3.400.jpg',
             'salsa-verde|herdez|salsa|tomate-verde|chile|picante|chilaquiles|enchiladas|salsas|453g|$26|20cal|SI-diabeticos|popular:94|mexicana|receta'),

            ('Salsa Casera Mexicana Herdez', 'Herdez', 'Salsas', '453g', 26.00, 15, 1, 92, 85,
             'https://images.openfoodfacts.org/images/products/750/100/310/2411/front_es.3.400.jpg',
             'salsa-roja|salsa-casera|herdez|jitomate|chile|chilaquiles|huevos|salsas|453g|$26|15cal|SI-diabeticos|popular:92|mexicana|receta'),

            ('Crema Ácida Lala', 'Lala', 'Lácteos', '450ml', 36.00, 110, 0, 96, 75,
             'https://images.openfoodfacts.org/images/products/750/102/051/5282/front_es.4.400.jpg',
             'crema|crema-acida|lala|lacteo|chilaquiles|tacos|tostadas|antojitos|450ml|$36|110cal|NO-diabeticos|popular:96|esencial|receta'),

            ('Queso Cotija Rallado Los Volcanes', 'Los Volcanes', 'Lácteos', '250g', 45.00, 120, 1, 88, 60,
             'https://images.openfoodfacts.org/images/products/750/102/055/1235/front_es.3.400.jpg',
             'queso-cotija|cotija|queso-rallado|los-volcanes|lacteo|chilaquiles|frijoles|antojitos|250g|$45|120cal|SI-diabeticos|popular:88|salado'),

            -- ═══ QUESADILLAS, SINCRONIZADAS Y EMBUTIDOS ═══
            ('Tortillas de Harina Tía Rosa', 'Tía Rosa', 'Panadería', '10 pzas', 28.00, 130, 0, 96, 90,
             'https://images.openfoodfacts.org/images/products/750/100/011/2708/front_es.4.400.jpg',
             'tortillas-de-harina|tia-rosa|tortillinas|quesadillas|sincronizadas|burritos|panaderia|10-piezas|$28|130cal|NO-diabeticos|popular:96|desayuno|cena'),

            ('Queso Oaxaca Lala (Quesillo)', 'Lala', 'Lácteos', '400g', 78.00, 140, 1, 95, 70,
             'https://images.openfoodfacts.org/images/products/750/102/055/0122/front_es.4.400.jpg',
             'queso-oaxaca|quesillo|queso-hebra|lala|lacteo|fundir|quesadillas|sincronizadas|400g|$78|140cal|SI-diabeticos|popular:95|delicioso'),

            ('Jamón de Pierna Fud', 'Fud', 'Embutidos', '250g', 52.00, 100, 1, 92, 80,
             'https://images.openfoodfacts.org/images/products/750/104/000/1123/front_es.3.400.jpg',
             'jamon-de-pierna|jamon|fud|embutidos|carne|sincronizadas|sandwich|torta|250g|$52|100cal|SI-diabeticos|popular:92|lonche'),

            -- ═══ GUISOS, TORTAS Y CONDIMENTOS MEXICANOS ═══
            ('Chiles Chipotles Adobados La Costeña', 'La Costeña', 'Salsas', '220g', 22.00, 30, 1, 93, 100,
             'https://images.openfoodfacts.org/images/products/750/200/054/1011/front_es.3.400.jpg',
             'chile-chipotle|chipotles|adobados|la-costena|tinga|guisado|salsas|picante|220g|$22|30cal|SI-diabeticos|popular:93|comida-mexicana|receta'),

            ('Carne Molida de Res SuKarne', 'SuKarne', 'Carnes', '500g', 85.00, 210, 1, 88, 50,
             'https://images.openfoodfacts.org/images/products/750/229/905/0101/front_es.3.400.jpg',
             'carne-molida|res|sukarne|carnes|picadillo|hamburguesa|albondigas|500g|$85|210cal|SI-diabeticos|popular:88|comida|receta'),

            ('Salsa Catsup Del Monte', 'Del Monte', 'Salsas', '370g', 22.00, 40, 0, 90, 80,
             'https://images.openfoodfacts.org/images/products/750/100/311/1000/front_es.3.400.jpg',
             'catsup|ketchup|del-monte|salsa-tomate|hot-dogs|hamburguesas|papas|salsas|370g|$22|40cal|NO-diabeticos|popular:90|antojo'),

            ('Mostaza Original McCormick', 'McCormick', 'Salsas', '260g', 20.00, 10, 1, 85, 75,
             'https://images.openfoodfacts.org/images/products/750/702/200/1101/front_es.3.400.jpg',
             'mostaza|mccormick|aderezo|hot-dogs|hamburguesas|sandwich|salsas|260g|$20|10cal|SI-diabeticos|popular:85|condimento'),

            ('Queso Amarillo Americano Kraft', 'Kraft', 'Lácteos', '8 pzas', 32.00, 60, 0, 91, 70,
             'https://images.openfoodfacts.org/images/products/750/100/510/2001/front_es.3.400.jpg',
             'queso-amarillo|kraft-singles|americano|queso|hamburguesas|sandwich|hot-dogs|lacteos|8-rebanadas|$32|60cal|NO-diabeticos|popular:91|antojo'),

            ('Pan Bolillo Bimbo', 'Bimbo', 'Panadería', '4 pzas', 26.00, 140, 0, 94, 85,
             'https://images.openfoodfacts.org/images/products/750/100/011/1701/front_es.3.400.jpg',
             'bolillo|telera|bimbo|pan|panaderia|torta|molletes|desayuno|lonche|4-piezas|$26|140cal|NO-diabeticos|popular:94|clasico'),

            ('Chiles Jalapeños en Rajas La Costeña', 'La Costeña', 'Salsas', '380g', 22.00, 15, 1, 92, 90,
             'https://images.openfoodfacts.org/images/products/750/200/054/1042/front_es.3.400.jpg',
             'jalapenos|rajas|chiles-en-vinagre|la-costena|salsas|tortas|sandwiches|picante|380g|$22|15cal|SI-diabeticos|popular:92|mexicano'),

            -- ═══ PASTAS Y COMIDA CASERA ═══
            ('Espagueti La Moderna', 'La Moderna', 'Despensa', '200g', 12.00, 130, 1, 95, 110,
             'https://images.openfoodfacts.org/images/products/750/101/831/0011/front_es.3.400.jpg',
             'espagueti|pasta|la-moderna|despensa|comida|crema|bolonesa|italiana|200g|$12|130cal|SI-diabeticos|popular:95|receta|esencial'),

            ('Queso Parmesano Rallado Kraft', 'Kraft', 'Lácteos', '85g', 48.00, 110, 1, 82, 50,
             'https://images.openfoodfacts.org/images/products/002/100/061/1001/front_es.3.400.jpg',
             'queso-parmesano|parmesano|kraft|lacteos|pasta|espagueti|ensaladas|85g|$48|110cal|SI-diabeticos|popular:82|gourmet'),

            ('Cilantro Fresco (Manojo)', 'Granel', 'Frutas y Verduras', '1 pza', 10.00, 5, 1, 96, 60,
             'https://images.openfoodfacts.org/images/products/750/300/113/3776/front_es.3.400.jpg',
             'cilantro|manojo|verdura|hierba|guacamole|salsas|tacos|fresco|frutas-y-verduras|$10|5cal|SI-diabeticos|popular:96|aroma|esencial'),

            ('Chile Serrano Fresco', 'Granel', 'Frutas y Verduras', '250g', 15.00, 10, 1, 95, 70,
             'https://images.openfoodfacts.org/images/products/750/300/113/3783/front_es.3.400.jpg',
             'chile-serrano|serrano|chile-verde|picante|guacamole|salsas|fresco|frutas-y-verduras|250g|$15|10cal|SI-diabeticos|popular:95|picoso'),

            -- ═══ DESAYUNO, CAFÉ Y ANTOJO DE LA TARDE ═══
            ('Café Soluble Nescafé Clásico', 'Nescafé', 'Despensa', '120g', 65.00, 5, 1, 97, 80,
             'https://images.openfoodfacts.org/images/products/750/105/920/0024/front_es.3.400.jpg',
             'nescafe|cafe|cafe-soluble|clasico|despensa|desayuno|tarde|caliente|120g|$65|5cal|SI-diabeticos|popular:97|esencial|despertar'),

            ('Chocolate en Polvo Abuelita', 'Nestlé', 'Despensa', '320g', 45.00, 120, 0, 94, 80,
             'https://images.openfoodfacts.org/images/products/750/105/920/1014/front_es.3.400.jpg',
             'chocolate-abuelita|abuelita|chocolate|dulce|despensa|leche|caliente|antojo|desayuno|320g|$45|120cal|NO-diabeticos|popular:94|familia'),

            ('Leche Condensada La Lechera', 'Nestlé', 'Despensa', '384g', 34.00, 130, 0, 93, 75,
             'https://images.openfoodfacts.org/images/products/750/105/920/2028/front_es.3.400.jpg',
             'la-lechera|leche-condensada|nestle|dulce|postres|hot-cakes|cafe|despensa|384g|$34|130cal|NO-diabeticos|popular:93|postre'),

            ('Galletas Marías Gamesa', 'Gamesa', 'Botanas', '170g', 18.00, 140, 0, 96, 95,
             'https://images.openfoodfacts.org/images/products/750/100/066/1022/front_es.3.400.jpg',
             'galletas-marias|marias|gamesa|botanas|panaderia|cafe|postre|antojo|170g|$18|140cal|NO-diabeticos|popular:96|clasicas'),

            ('Tocino Ahumado Fud', 'Fud', 'Embutidos', '250g', 68.00, 190, 1, 89, 50,
             'https://images.openfoodfacts.org/images/products/750/104/000/1307/front_es.3.400.jpg',
             'tocino|bacon|fud|ahumado|embutidos|huevos|hamburguesas|desayuno|250g|$68|190cal|SI-diabeticos|popular:89|crujiente')
        ;");
    }
}
