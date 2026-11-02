using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using MQTTnet;
using MQTTnet.Protocol;

namespace BasculaAgent;

/// <summary>
/// Worker Service que realiza polling a una báscula Torrey vía puerto serial
/// y publica el peso leído (normalizado a kilos) en un broker MQTT.
/// Toda la configuración se carga desde appsettings.json sin necesidad de recompilar.
/// </summary>
public class Worker : BackgroundService
{
    // ──────────────────────────────────────────────────────────────
    //  Unidades soportadas
    // ──────────────────────────────────────────────────────────────
    private static readonly HashSet<string> UnidadesSoportadas =
        new(StringComparer.OrdinalIgnoreCase) { "Kilos", "Libras", "Onzas" };

    // Factores de conversión → Kilogramos
    private static readonly Dictionary<string, double> FactorAKilos =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "Kilos",  1.0        },
            { "Libras", 0.45359237 },
            { "Onzas",  0.02834952 }
        };

    // Regex compilada: conserva únicamente dígitos y punto decimal
    private static readonly Regex WeightRegex = new(@"[^\d.]", RegexOptions.Compiled);

    // ──────────────────────────────────────────────────────────────
    //  Configuración (cargada desde appsettings.json)
    // ──────────────────────────────────────────────────────────────
    private readonly string   _serialPortName;
    private readonly int      _baudRate;
    private readonly string   _mqttHost;
    private readonly int      _mqttPort;
    private readonly string   _mqttTopic;
    private readonly string   _pollCommand;
    private readonly TimeSpan _pollInterval;
    private readonly TimeSpan _readDelay;
    private readonly TimeSpan _reconnectDelay;

    /// <summary>
    /// Conjunto de unidades que se aceptarán de la báscula.
    /// Si la báscula envía una unidad no listada aquí, la lectura se descarta.
    /// Default: solo Kilos.
    /// </summary>
    private readonly HashSet<string> _unidadesAceptadas;

    // ──────────────────────────────────────────────────────────────
    //  Estado interno
    // ──────────────────────────────────────────────────────────────
    private readonly ILogger<Worker> _logger;
    private SerialPort?  _serialPort;
    private IMqttClient? _mqttClient;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;

        // — Serial —
        _serialPortName = configuration["BasculaAgent:SerialPort:PortName"] ?? "COM3";
        _baudRate       = configuration.GetValue<int>("BasculaAgent:SerialPort:BaudRate", 9600);

        // — MQTT —
        _mqttHost  = configuration["BasculaAgent:Mqtt:Host"] ?? "localhost";
        _mqttPort  = configuration.GetValue<int>("BasculaAgent:Mqtt:Port", 1883);
        _mqttTopic = configuration["BasculaAgent:Mqtt:Topic"] ?? "wms/basculas/estacion1";

        // — Polling —
        _pollCommand    = configuration["BasculaAgent:Polling:Command"] ?? "P";
        _pollInterval   = TimeSpan.FromSeconds(configuration.GetValue<double>("BasculaAgent:Polling:IntervalSeconds",    1));
        _readDelay      = TimeSpan.FromMilliseconds(configuration.GetValue<double>("BasculaAgent:Polling:ReadDelayMs",   300));
        _reconnectDelay = TimeSpan.FromSeconds(configuration.GetValue<double>("BasculaAgent:Polling:ReconnectDelaySeconds", 5));

        // — Unidades —
        var unidadesConfig = configuration
            .GetSection("BasculaAgent:Unidades:UnidadesAceptadas")
            .Get<string[]>();

        // Por defecto solo Kilos; se validan contra las unidades soportadas
        _unidadesAceptadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (unidadesConfig is { Length: > 0 })
        {
            foreach (var u in unidadesConfig)
            {
                if (UnidadesSoportadas.Contains(u))
                    _unidadesAceptadas.Add(u);
                else
                    _logger.LogWarning("Unidad '{unidad}' no soportada en configuración. Se ignorará. Valores válidos: {validos}",
                        u, string.Join(", ", UnidadesSoportadas));
            }
        }

        if (_unidadesAceptadas.Count == 0)
        {
            _logger.LogWarning("No se configuraron unidades válidas. Se usará 'Kilos' por default.");
            _unidadesAceptadas.Add("Kilos");
        }
    }

    // ──────────────────────────────────────────────────────────────
    //  Ciclo principal
    // ──────────────────────────────────────────────────────────────
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "BasculaAgent iniciado. Puerto: {port} | Topic: {topic} | Unidades aceptadas: {unidades}",
            _serialPortName, _mqttTopic, string.Join(", ", _unidadesAceptadas));

        while (!stoppingToken.IsCancellationRequested)
        {
            // 1. Asegurar que el puerto serial esté abierto
            if (!EnsureSerialPort())
            {
                _logger.LogWarning("No se pudo abrir {port}. Reintentando en {delay}s…",
                    _serialPortName, _reconnectDelay.TotalSeconds);
                await SafeDelay(_reconnectDelay, stoppingToken);
                continue;
            }

            // 2. Asegurar que el cliente MQTT esté conectado
            if (!await EnsureMqttConnectedAsync(stoppingToken))
            {
                _logger.LogWarning("No se pudo conectar al broker MQTT. Reintentando en {delay}s…",
                    _reconnectDelay.TotalSeconds);
                await SafeDelay(_reconnectDelay, stoppingToken);
                continue;
            }

            // 3. Polling y publicación
            await PollAndPublishAsync(stoppingToken);

            // 4. Esperar el intervalo antes del siguiente ciclo
            await SafeDelay(_pollInterval, stoppingToken);
        }

        // Limpieza al detener el servicio
        _logger.LogInformation("BasculaAgent detenido. Liberando recursos…");
        CloseSerialPort();
        await DisconnectMqttAsync();
    }

    // ──────────────────────────────────────────────────────────────
    //  Polling y publicación
    // ──────────────────────────────────────────────────────────────
    private async Task PollAndPublishAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Enviar comando de polling a la báscula
            _serialPort!.Write(_pollCommand);
            _logger.LogDebug("Comando '{cmd}' enviado a {port}", _pollCommand, _serialPortName);

            // Esperar que la báscula responda
            await Task.Delay(_readDelay, stoppingToken);

            // Leer lo que haya en el buffer
            string rawResponse = _serialPort.ReadExisting();
            _logger.LogDebug("Respuesta cruda: '{raw}'", rawResponse);

            // ── Detección de unidad ────────────────────────────────
            // La báscula Torrey suele responder algo como "  12.50 kg\r\n"
            // Intentamos detectar la unidad en la respuesta para validarla.
            string? unidadDetectada = DetectarUnidad(rawResponse);

            if (unidadDetectada is not null && !_unidadesAceptadas.Contains(unidadDetectada))
            {
                _logger.LogDebug(
                    "Unidad detectada '{unidad}' no está en la lista de unidades aceptadas. Lectura descartada.",
                    unidadDetectada);
                return;
            }

            // Limpiar con Regex: sólo números y punto decimal
            string cleanWeight = WeightRegex.Replace(rawResponse, string.Empty);

            if (string.IsNullOrWhiteSpace(cleanWeight))
            {
                _logger.LogDebug("Respuesta descartada (vacía tras limpieza): '{raw}'", rawResponse);
                return;
            }

            // ── Conversión a Kilos ─────────────────────────────────
            // Si la báscula envió en otra unidad, convertimos a kg antes de publicar.
            string weightParaPublicar = cleanWeight;

            if (unidadDetectada is not null &&
                !string.Equals(unidadDetectada, "Kilos", StringComparison.OrdinalIgnoreCase) &&
                double.TryParse(cleanWeight, System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture, out double valorOriginal))
            {
                double valorKg = valorOriginal * FactorAKilos[unidadDetectada];
                weightParaPublicar = valorKg.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
                _logger.LogDebug("Conversión: {orig} {unidad} → {kg} kg", cleanWeight, unidadDetectada, weightParaPublicar);
            }

            _logger.LogInformation("Peso leído: {weight} kg", weightParaPublicar);

            // Publicar en el broker MQTT
            await PublishWeightAsync(weightParaPublicar, stoppingToken);
        }
        catch (TimeoutException tex)
        {
            _logger.LogWarning(tex,
                "Timeout leyendo {port}. La báscula puede estar inactiva.", _serialPortName);
        }
        catch (InvalidOperationException iex)
        {
            // Puerto cerrado inesperadamente (cable desconectado, por ejemplo)
            _logger.LogError(iex,
                "Puerto serial {port} cerrado inesperadamente. Se intentará reconectar.", _serialPortName);
            CloseSerialPort();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex,
                "Error inesperado durante el polling en {port}.", _serialPortName);
            CloseSerialPort();
        }
    }

    /// <summary>
    /// Intenta detectar la unidad de medida en la respuesta cruda de la báscula.
    /// Busca patrones como "kg", "lb", "oz" (insensible a mayúsculas).
    /// Retorna null si no se detecta ninguna unidad conocida.
    /// </summary>
    private static string? DetectarUnidad(string rawResponse)
    {
        if (rawResponse.Contains("kg", StringComparison.OrdinalIgnoreCase) ||
            rawResponse.Contains("kilo", StringComparison.OrdinalIgnoreCase))
            return "Kilos";

        if (rawResponse.Contains("lb", StringComparison.OrdinalIgnoreCase) ||
            rawResponse.Contains("lbs", StringComparison.OrdinalIgnoreCase) ||
            rawResponse.Contains("libra", StringComparison.OrdinalIgnoreCase))
            return "Libras";

        if (rawResponse.Contains("oz", StringComparison.OrdinalIgnoreCase) ||
            rawResponse.Contains("onza", StringComparison.OrdinalIgnoreCase))
            return "Onzas";

        return null; // La báscula no incluye etiqueta de unidad en la respuesta
    }

    // ──────────────────────────────────────────────────────────────
    //  Publicación MQTT
    // ──────────────────────────────────────────────────────────────
    private async Task PublishWeightAsync(string weight, CancellationToken stoppingToken)
    {
        try
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(_mqttTopic)
                .WithPayload(Encoding.UTF8.GetBytes(weight))
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(true)
                .Build();

            await _mqttClient!.PublishAsync(message, stoppingToken);
            _logger.LogDebug("Publicado en '{topic}': {weight}", _mqttTopic, weight);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex,
                "Error publicando en MQTT. Se intentará reconectar en el siguiente ciclo.");
            // Forzar reconexión del cliente MQTT en la próxima iteración
            await DisconnectMqttAsync();
        }
    }

    // ──────────────────────────────────────────────────────────────
    //  Gestión del puerto serial
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Garantiza que el puerto serial esté abierto y listo.
    /// </summary>
    /// <returns>True si el puerto está disponible; false en caso de error.</returns>
    private bool EnsureSerialPort()
    {
        if (_serialPort?.IsOpen == true)
            return true;

        try
        {
            CloseSerialPort(); // Liberar instancia anterior si existe

            _serialPort = new SerialPort(_serialPortName)
            {
                BaudRate     = _baudRate,
                Parity       = Parity.None,
                DataBits     = 8,
                StopBits     = StopBits.One,
                ReadTimeout  = 2000, // ms — evita bloqueo indefinido
                WriteTimeout = 2000, // ms
                Encoding     = Encoding.ASCII
            };

            _serialPort.Open();
            _logger.LogInformation("Puerto serial {port} abierto correctamente.", _serialPortName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo abrir el puerto serial {port}.", _serialPortName);
            CloseSerialPort();
            return false;
        }
    }

    private void CloseSerialPort()
    {
        try
        {
            if (_serialPort is not null)
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();

                _serialPort.Dispose();
                _serialPort = null;
                _logger.LogInformation("Puerto serial cerrado y liberado.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al cerrar el puerto serial.");
            _serialPort = null;
        }
    }

    // ──────────────────────────────────────────────────────────────
    //  Gestión del cliente MQTT  (MQTTnet 5.x)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Garantiza que el cliente MQTT esté conectado al broker.
    /// </summary>
    /// <returns>True si la conexión está activa; false en caso de error.</returns>
    private async Task<bool> EnsureMqttConnectedAsync(CancellationToken stoppingToken)
    {
        if (_mqttClient?.IsConnected == true)
            return true;

        try
        {
            await DisconnectMqttAsync(); // Limpiar instancia anterior

            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_mqttHost, _mqttPort)
                .WithClientId($"BasculaAgent-{Environment.MachineName}")
                .WithCleanSession(true)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
                .Build();

            var result = await _mqttClient.ConnectAsync(options, stoppingToken);

            if (result.ResultCode == MqttClientConnectResultCode.Success)
            {
                _logger.LogInformation(
                    "Conectado al broker MQTT en {host}:{port}.", _mqttHost, _mqttPort);
                return true;
            }

            _logger.LogError(
                "El broker MQTT rechazó la conexión. Código: {code}", result.ResultCode);
            return false;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex,
                "No se pudo conectar al broker MQTT en {host}:{port}.", _mqttHost, _mqttPort);
            await DisconnectMqttAsync();
            return false;
        }
    }

    private async Task DisconnectMqttAsync()
    {
        try
        {
            if (_mqttClient is not null)
            {
                if (_mqttClient.IsConnected)
                {
                    await _mqttClient.DisconnectAsync();
                    _logger.LogInformation("Cliente MQTT desconectado.");
                }

                _mqttClient.Dispose();
                _mqttClient = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al desconectar el cliente MQTT.");
            _mqttClient = null;
        }
    }

    // ──────────────────────────────────────────────────────────────
    //  Utilidades
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Task.Delay que absorbe <see cref="OperationCanceledException"/> para evitar
    /// que el servicio crashee cuando es detenido durante una espera.
    /// </summary>
    private static async Task SafeDelay(TimeSpan delay, CancellationToken ct)
    {
        try
        {
            await Task.Delay(delay, ct);
        }
        catch (OperationCanceledException)
        {
            // Detenido mientras esperaba: comportamiento normal, no propagar.
        }
    }
}
