# BasculaAgent — Documentación Técnica Completa

> Servicio Windows/.NET que lee el peso de una báscula Torrey vía puerto serial RS-232 y lo publica en tiempo real a través de MQTT. Diseñado para integrarse con sistemas WMS, portales web, aplicaciones móviles y add-ons de SAP Business One.

---

## Tabla de Contenidos

1. [¿Qué es BasculaAgent?](#1-qué-es-basculaagent)
2. [Arquitectura General](#2-arquitectura-general)
3. [Requisitos del Sistema](#3-requisitos-del-sistema)
4. [Dependencias NuGet](#4-dependencias-nuget)
5. [Estructura del Proyecto](#5-estructura-del-proyecto)
6. [Cómo Funciona el Código (Flujo Completo)](#6-cómo-funciona-el-código-flujo-completo)
   - 6.1 [Program.cs — Punto de entrada](#61-programcs--punto-de-entrada)
   - 6.2 [Worker.cs — Servicio principal](#62-workercs--servicio-principal)
   - 6.3 [Ciclo de polling detallado](#63-ciclo-de-polling-detallado)
   - 6.4 [Detección y conversión de unidades](#64-detección-y-conversión-de-unidades)
   - 6.5 [Gestión del puerto serial](#65-gestión-del-puerto-serial)
   - 6.6 [Gestión del cliente MQTT](#66-gestión-del-cliente-mqtt)
7. [Configuración completa (appsettings.json)](#7-configuración-completa-appsettingsjson)
8. [Instalación y configuración de Mosquitto](#8-instalación-y-configuración-de-mosquitto)
9. [Ejecutar el servicio](#9-ejecutar-el-servicio)
10. [Protocolo de mensajes MQTT](#10-protocolo-de-mensajes-mqtt)
11. [Consumir los datos: ejemplos por tecnología](#11-consumir-los-datos-ejemplos-por-tecnología)
    - 11.1 [JavaScript / Node.js](#111-javascript--nodejs)
    - 11.2 [JavaScript en el navegador (MQTT over WebSockets)](#112-javascript-en-el-navegador-mqtt-over-websockets)
    - 11.3 [Python](#113-python)
    - 11.4 [C# (.NET)](#114-c-net)
    - 11.5 [SAP Business One — Add-on / DI API](#115-sap-business-one--add-on--di-api)
    - 11.6 [SAP Business One — Integration Framework (B1if)](#116-sap-business-one--integration-framework-b1if)
12. [Múltiples básculas](#12-múltiples-básculas)
13. [Troubleshooting](#13-troubleshooting)
14. [Preguntas Frecuentes (FAQ)](#14-preguntas-frecuentes-faq)

---

## 1. ¿Qué es BasculaAgent?

**BasculaAgent** es un _Worker Service_ de .NET 10 que actúa como puente entre una báscula industrial (modelo Torrey con salida RS-232) y cualquier sistema de software moderno.

El servicio:
- Se conecta al **puerto serial** de la báscula (COM3, COM4, etc.) y le envía periódicamente un **comando de polling** (por defecto la letra `P`).
- La báscula responde con el peso actual en texto plano (ej. `"  12.50 kg\r\n"`).
- El agente **parsea** esa respuesta, **convierte el peso a kilogramos** si viene en otra unidad, y lo **publica en un broker MQTT** como un mensaje en un topic configurable.
- Cualquier sistema que se suscriba a ese topic MQTT recibe el peso en tiempo real, sin importar el lenguaje de programación o la plataforma.

### Ventajas del diseño

| Característica | Detalle |
|---|---|
| **Sin recompilación** | Toda la configuración vive en `appsettings.json` |
| **Auto-reconexión** | Se recupera solo si el serial o MQTT se desconectan |
| **Conversión automática** | Libras y onzas se convierten a kilogramos antes de publicar |
| **Multi-consumidor** | Cientos de clientes pueden suscribirse al mismo topic simultáneamente |
| **Ligero** | Un solo proceso, sin base de datos, sin dependencias pesadas |
| **Portable** | Corre como servicio Windows, como consola, o en contenedor Docker |

---

## 2. Arquitectura General

```
┌─────────────────────────────────────────────────────────────────┐
│                        Máquina del Agente                        │
│                                                                   │
│  ┌──────────────┐   RS-232 / USB-Serial   ┌──────────────────┐  │
│  │  Báscula     │ ◄─────────────────────► │                  │  │
│  │  Torrey      │   Comando "P" cada 1s   │  BasculaAgent    │  │
│  │  (hardware)  │   Respuesta: "12.50 kg" │  (.NET Worker)   │  │
│  └──────────────┘                         │                  │  │
│                                           │  - Parseo        │  │
│                                           │  - Conversión    │  │
│                                           │  - Validación    │  │
│                                           └────────┬─────────┘  │
│                                                    │ MQTT TCP    │
│                                           ┌────────▼─────────┐  │
│                                           │  Mosquitto Broker │  │
│                                           │  (localhost:1883) │  │
│                                           └────────┬─────────┘  │
└────────────────────────────────────────────────────┼────────────┘
                                                     │
                    ┌────────────────────────────────┼──────────────────────┐
                    │ Suscriptores MQTT               │                      │
                    ▼                                 ▼                      ▼
            ┌───────────────┐             ┌──────────────────┐   ┌──────────────────┐
            │  App Web      │             │  Python Script   │   │  SAP B1 Add-on   │
            │  (React/Vue)  │             │  (análisis)      │   │  (integración)   │
            └───────────────┘             └──────────────────┘   └──────────────────┘
```

**Flujo resumido:**

```
Báscula ──RS-232──► BasculaAgent ──MQTT──► Broker ──subscribe──► Tu Sistema
```

---

## 3. Requisitos del Sistema

### Hardware
- PC con Windows 10/11 o Windows Server 2019+
- Puerto COM disponible (físico o adaptador USB-Serial)
- Cable serial cruzado (null modem) o directo según la báscula
- Báscula Torrey con protocolo de polling por comando ASCII

### Software
- **.NET 10 SDK o Runtime** — https://dotnet.microsoft.com/download
- **Mosquitto MQTT Broker** — https://mosquitto.org/download/
- Acceso de administrador para instalar servicios de Windows (opcional)

---

## 4. Dependencias NuGet

Declaradas en `BasculaAgent.csproj`:

| Paquete | Versión | Propósito |
|---|---|---|
| `Microsoft.Extensions.Hosting` | 10.0.10 | Infraestructura de Worker Service (.NET Generic Host) |
| `MQTTnet` | 5.2.0.1603 | Cliente MQTT de alto rendimiento para .NET |
| `System.IO.Ports` | 10.0.11 | Comunicación con puerto serial COM |

Restaurar dependencias:
```powershell
dotnet restore
```

---

## 5. Estructura del Proyecto

```
BasculaAgent/
├── BasculaAgent.csproj          # Definición del proyecto: framework, paquetes NuGet
├── Program.cs                   # Punto de entrada; registra el Worker Service
├── Worker.cs                    # Lógica completa: polling serial + publicación MQTT
├── appsettings.json             # Configuración para producción
├── appsettings.Development.json # Sobreescrituras para entorno de desarrollo
└── Properties/
    └── launchSettings.json      # Perfiles de arranque para Visual Studio
```

---

## 6. Cómo Funciona el Código (Flujo Completo)

### 6.1 `Program.cs` — Punto de entrada

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
var host = builder.Build();
host.Run();
```

Estas 4 líneas hacen todo el bootstrapping del _Generic Host_ de .NET:

1. **`Host.CreateApplicationBuilder(args)`** — Crea el host de la aplicación. Automáticamente:
   - Carga `appsettings.json` y `appsettings.{ENVIRONMENT}.json`
   - Configura el sistema de logging (consola, Event Log de Windows, etc.)
   - Establece el entorno (`Development`, `Production`) según la variable de entorno `DOTNET_ENVIRONMENT`

2. **`AddHostedService<Worker>()`** — Registra la clase `Worker` como un servicio en segundo plano. El host la instanciará via inyección de dependencias, pasándole automáticamente `ILogger<Worker>` e `IConfiguration`.

3. **`host.Run()`** — Inicia el host, arranca todos los `IHostedService` registrados, y bloquea el hilo hasta recibir señal de apagado (CTRL+C, `sc stop`, o señal del SO).

---

### 6.2 `Worker.cs` — Servicio principal

`Worker` hereda de `BackgroundService`, que es la clase base de .NET para servicios en segundo plano. Solo requiere implementar el método `ExecuteAsync(CancellationToken)`.

#### Campos y constantes estáticas

```csharp
// Las 3 unidades que el agente entiende (insensible a mayúsculas)
private static readonly HashSet<string> UnidadesSoportadas =
    new(StringComparer.OrdinalIgnoreCase) { "Kilos", "Libras", "Onzas" };

// Factores para convertir cualquier unidad a Kilogramos
private static readonly Dictionary<string, double> FactorAKilos =
    new(StringComparer.OrdinalIgnoreCase)
    {
        { "Kilos",  1.0        },   // 1 kg = 1 kg
        { "Libras", 0.45359237 },   // 1 lb = 0.45359237 kg (exacto, estándar internacional)
        { "Onzas",  0.02834952 }    // 1 oz = 0.02834952 kg
    };

// Regex compilada: elimina cualquier carácter que NO sea dígito o punto decimal
// Ejemplo: "  12.50 kg\r\n" -> "12.50"
private static readonly Regex WeightRegex = new(@"[^\d.]", RegexOptions.Compiled);
```

#### Constructor — Lectura de configuración

El constructor recibe `ILogger<Worker>` e `IConfiguration` por inyección de dependencias y mapea todos los valores del `appsettings.json` a campos privados:

```
"BasculaAgent:SerialPort:PortName"           -> _serialPortName  (default: "COM3")
"BasculaAgent:SerialPort:BaudRate"           -> _baudRate         (default: 9600)
"BasculaAgent:Mqtt:Host"                     -> _mqttHost         (default: "localhost")
"BasculaAgent:Mqtt:Port"                     -> _mqttPort         (default: 1883)
"BasculaAgent:Mqtt:Topic"                    -> _mqttTopic        (default: "wms/basculas/estacion1")
"BasculaAgent:Polling:Command"               -> _pollCommand      (default: "P")
"BasculaAgent:Polling:IntervalSeconds"       -> _pollInterval     (default: 1 segundo)
"BasculaAgent:Polling:ReadDelayMs"           -> _readDelay        (default: 300 ms)
"BasculaAgent:Polling:ReconnectDelaySeconds" -> _reconnectDelay   (default: 5 segundos)
"BasculaAgent:Unidades:UnidadesAceptadas"    -> _unidadesAceptadas (default: ["Kilos"])
```

**Validación de unidades en el constructor:** Si en `appsettings.json` se configura una unidad no reconocida (ej. `"Gramos"`), el agente la ignora y emite un `LogWarning`. Si la lista queda vacía tras la validación, usa `"Kilos"` por defecto para evitar que el servicio arranque sin unidades válidas.

---

### 6.3 Ciclo de polling detallado

`ExecuteAsync` implementa un bucle infinito que se repite mientras el servicio no sea detenido:

```
┌─────────────────────────────────────────────────────────────┐
│                    BUCLE PRINCIPAL (while)                   │
│                                                               │
│  PASO 1: Puerto serial abierto?                              │
│    NO  -> Log Warning -> esperar _reconnectDelay -> volver   │
│    SÍ  -> continuar                                           │
│                                                               │
│  PASO 2: Cliente MQTT conectado?                             │
│    NO  -> Log Warning -> esperar _reconnectDelay -> volver   │
│    SÍ  -> continuar                                           │
│                                                               │
│  PASO 3: PollAndPublishAsync()                               │
│    - Enviar comando "P" al serial                            │
│    - Esperar _readDelay (300ms por defecto)                  │
│    - Leer respuesta del buffer serial                        │
│    - DetectarUnidad() en la respuesta                        │
│    - Validar que la unidad esté en _unidadesAceptadas        │
│    - Limpiar con Regex -> solo números y punto               │
│    - Convertir a kg si no es kilos                           │
│    - PublishWeightAsync() -> MQTT broker                     │
│                                                               │
│  PASO 4: Esperar _pollInterval (1 segundo por defecto)       │
└─────────────────────────────────────────────────────────────┘
```

**¿Por qué hay un delay de 300ms antes de leer?**
La báscula no responde instantáneamente. El comando `P` se envía, y la báscula procesa el peso y forma su cadena de respuesta. El delay de 300ms es el tiempo de espera para que la respuesta completa llegue al buffer del puerto serial antes de llamar a `ReadExisting()`. Este valor es configurable (`ReadDelayMs`).

---

### 6.4 Detección y conversión de unidades

#### Detección (`DetectarUnidad`)

El método analiza el texto crudo de la respuesta buscando palabras clave (insensible a mayúsculas):

| La respuesta contiene... | Unidad detectada |
|---|---|
| `"kg"` o `"kilo"` | `"Kilos"` |
| `"lb"`, `"lbs"` o `"libra"` | `"Libras"` |
| `"oz"` o `"onza"` | `"Onzas"` |
| Nada de lo anterior | `null` (no se detectó unidad) |

> **Caso `null`:** Si la báscula no incluye etiqueta de unidad (respuesta como `"12.50\r\n"`), el agente **no descarta la lectura**. La procesa y publica tal cual, asumiendo que ya viene en la unidad correcta.

#### Validación

Si se detectó una unidad y **no está** en `_unidadesAceptadas` (configurada en `appsettings.json`), la lectura se **descarta silenciosamente** (solo un `LogDebug`). Esto protege contra básculas que puedan enviar unidades no deseadas.

#### Conversión a kilogramos

Si la unidad detectada es `"Libras"` u `"Onzas"`, el valor numérico se multiplica por el factor de conversión:

```
kg = valorOriginal × FactorAKilos[unidadDetectada]
```

El resultado se formatea con **4 decimales** usando `InvariantCulture` (punto como separador decimal, nunca coma) antes de publicarlo en MQTT.

Ejemplo de conversión:
- Respuesta cruda: `"27.55 lbs"`
- Limpieza Regex: `"27.55"`
- Conversión: `27.55 × 0.45359237 = 12.4965`
- Publicado en MQTT: `"12.4965"`

---

### 6.5 Gestión del puerto serial

#### `EnsureSerialPort()`

Se llama al inicio de cada ciclo. Si el puerto ya está abierto, regresa `true` inmediatamente (sin costo). Si no está abierto:

1. Llama a `CloseSerialPort()` para limpiar cualquier instancia anterior con fugas de recursos.
2. Crea un nuevo `SerialPort` con esta configuración **fija** (no configurable por diseño):

| Parámetro | Valor | Razón |
|---|---|---|
| `BaudRate` | Desde config | Configurable; típico 9600 |
| `Parity` | `None` | Estándar RS-232 básico |
| `DataBits` | `8` | Estándar RS-232 básico |
| `StopBits` | `One` | Estándar RS-232 básico |
| `ReadTimeout` | `2000 ms` | Evita que `ReadExisting` bloquee indefinidamente |
| `WriteTimeout` | `2000 ms` | Evita que `Write` bloquee indefinidamente |
| `Encoding` | `ASCII` | Las básculas Torrey transmiten ASCII puro |

3. Llama a `_serialPort.Open()`.
4. Si falla (puerto no existe, en uso, sin permisos), regresa `false` y el bucle principal espera `_reconnectDelay` antes de reintentar.

#### `CloseSerialPort()`

Cierra el puerto **y** llama a `Dispose()` para liberar el handle del SO. Pone `_serialPort = null` para que el recolector de basura pueda actuar. Si el cierre lanza excepción (el puerto ya estaba en mal estado), la captura con `LogWarning` y de todas formas pone null para evitar un state inconsistente.

---

### 6.6 Gestión del cliente MQTT

#### `EnsureMqttConnectedAsync()`

Misma lógica que el serial: si ya está conectado, regresa `true`. Si no:

1. Limpia la instancia anterior con `DisconnectMqttAsync()`.
2. Usa `MqttClientFactory` (MQTTnet 5.x) para crear un cliente nuevo.
3. Construye opciones de conexión:
   - **TCP Server:** `_mqttHost:_mqttPort`
   - **ClientId:** `"BasculaAgent-{nombre_de_la_maquina}"` — único por máquina, evita conflictos si hay varios agentes en la red.
   - **CleanSession:** `true` — no persiste subscripciones entre reconexiones.
   - **KeepAlive:** 30 segundos — el broker detectará si el cliente se desconecta inesperadamente.
4. Verifica que el `ResultCode` sea `Success`. Si el broker rechaza (credenciales incorrectas, ACL, etc.), registra el código de error y regresa `false`.

#### `PublishWeightAsync()`

Construye un mensaje MQTT con:

| Propiedad | Valor | Significado |
|---|---|---|
| `Topic` | Config (`_mqttTopic`) | Dónde se publica |
| `Payload` | Peso en bytes UTF-8 | El dato numérico |
| `QoS` | `AtLeastOnce` (QoS 1) | Entrega garantizada al menos una vez |
| `Retain` | `true` | El broker retiene el último valor; un cliente nuevo recibe el peso actual inmediatamente |

> **¿Por qué `Retain = true`?** Un sistema web que abra la página mientras la báscula ya lleva tiempo publicando necesita saber el peso actual _ahora_, sin esperar al próximo ciclo de polling. El flag `retain` hace que el broker almacene el último mensaje y lo entregue a cualquier nuevo suscriptor.

---

## 7. Configuración completa (`appsettings.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "BasculaAgent": {
    "SerialPort": {
      "PortName": "COM3",
      "BaudRate": 9600
    },
    "Mqtt": {
      "Host": "localhost",
      "Port": 1883,
      "Topic": "wms/basculas/estacion1"
    },
    "Polling": {
      "Command": "P",
      "IntervalSeconds": 1,
      "ReadDelayMs": 300,
      "ReconnectDelaySeconds": 5
    },
    "Unidades": {
      "UnidadesAceptadas": [ "Kilos" ]
    }
  }
}
```

### Referencia de todos los parámetros

#### `Logging.LogLevel`

Controla qué mensajes aparecen en la consola o Event Log de Windows.

| Valor | Muestra |
|---|---|
| `"Trace"` | Todo (muy verboso) |
| `"Debug"` | Respuestas crudas de la báscula, payloads MQTT |
| `"Information"` | Arranque, conexiones, peso leído (recomendado en producción) |
| `"Warning"` | Timeouts, reintentos |
| `"Error"` | Fallos de conexión, errores inesperados |

---

#### `BasculaAgent.SerialPort`

| Clave | Tipo | Default | Descripción |
|---|---|---|---|
| `PortName` | `string` | `"COM3"` | Puerto COM donde está conectada la báscula. En Windows: `"COM3"`, `"COM4"`, etc. |
| `BaudRate` | `int` | `9600` | Velocidad en baudios. Debe coincidir con la configuración física de la báscula. Valores típicos: `9600`, `19200`. |

**¿Cómo saber el COM asignado?**
```powershell
[System.IO.Ports.SerialPort]::GetPortNames()
# O bien: Administrador de dispositivos -> Puertos (COM y LPT)
```

---

#### `BasculaAgent.Mqtt`

| Clave | Tipo | Default | Descripción |
|---|---|---|---|
| `Host` | `string` | `"localhost"` | IP o hostname del broker MQTT. Si Mosquitto está en la misma máquina, usar `"localhost"`. |
| `Port` | `int` | `1883` | Puerto TCP del broker. `1883` es el estándar sin TLS. Con TLS sería `8883`. |
| `Topic` | `string` | `"wms/basculas/estacion1"` | Topic MQTT donde se publican los pesos. Seguir convención jerárquica `área/subsistema/identificador`. |

**Convención de topics sugerida para múltiples básculas:**
```
wms/basculas/estacion1
wms/basculas/estacion2
wms/basculas/recepcion
produccion/linea1/basculas/entrada
```

---

#### `BasculaAgent.Polling`

| Clave | Tipo | Default | Descripción |
|---|---|---|---|
| `Command` | `string` | `"P"` | Carácter o cadena que se envía a la báscula para solicitar el peso. Consultar el manual de la báscula. |
| `IntervalSeconds` | `double` | `1` | Segundos entre cada lectura. `1` = una lectura por segundo. Aumentar si la báscula no soporta alta frecuencia. |
| `ReadDelayMs` | `double` | `300` | Milisegundos de espera entre enviar el comando y leer la respuesta. Aumentar si la báscula es lenta. |
| `ReconnectDelaySeconds` | `double` | `5` | Segundos de espera antes de reintentar conectar al serial o MQTT tras un fallo. |

---

#### `BasculaAgent.Unidades`

| Clave | Tipo | Default | Descripción |
|---|---|---|---|
| `UnidadesAceptadas` | `string[]` | `["Kilos"]` | Lista de unidades válidas. Si la báscula envía una unidad no listada, la lectura se descarta. Valores válidos: `"Kilos"`, `"Libras"`, `"Onzas"`. |

**Ejemplos:**

```json
"UnidadesAceptadas": [ "Kilos" ]
```
Acepta solo kilos. Lectura en libras es descartada.

```json
"UnidadesAceptadas": [ "Kilos", "Libras" ]
```
Acepta kilos y libras. Las libras se convierten a kg antes de publicar.

```json
"UnidadesAceptadas": [ "Kilos", "Libras", "Onzas" ]
```
Acepta todas las unidades. Todas se convierten a kg al publicar.

---

## 8. Instalación y configuración de Mosquitto

Mosquitto es el broker MQTT de código abierto más popular. Es el componente que actúa como intermediario entre el agente y los consumidores.

### 8.1 Descargar e instalar

1. Ir a https://mosquitto.org/download/
2. Descargar el instalador para Windows (`.exe`)
3. Ejecutar el instalador con privilegios de administrador
4. Ruta de instalación por defecto: `C:\Program Files\mosquitto\`

### 8.2 Configuración básica (sin autenticación — red local)

Editar el archivo `C:\Program Files\mosquitto\mosquitto.conf`:

```conf
# Puerto estándar MQTT
listener 1883

# Permitir conexiones anónimas (sin usuario/contraseña)
# SOLO para redes locales internas. NO usar en producción pública.
allow_anonymous true

# Habilitar logging para troubleshooting
log_dest file C:\Program Files\mosquitto\mosquitto.log
log_type error
log_type warning
log_type information
```

### 8.3 Configuración con autenticación (recomendado)

```conf
listener 1883
allow_anonymous false
password_file C:\Program Files\mosquitto\passwd
```

Crear usuario y contraseña:
```powershell
# Ejecutar como administrador
cd "C:\Program Files\mosquitto"
.\mosquitto_passwd.exe -c passwd bascula_user
# Ingresar contraseña cuando se solicite
```

### 8.4 Configuración con WebSockets (para consumir desde el navegador)

```conf
# Puerto MQTT estándar (para apps de servidor)
listener 1883
allow_anonymous true

# Puerto WebSocket (para navegadores web)
listener 9001
protocol websockets
allow_anonymous true
```

### 8.5 Iniciar Mosquitto como servicio de Windows

```powershell
# Instalar como servicio (ejecutar como administrador)
cd "C:\Program Files\mosquitto"
.\mosquitto.exe install

# Iniciar el servicio
net start mosquitto

# Verificar que está corriendo
Get-Service mosquitto

# Configurar inicio automático con Windows
Set-Service -Name mosquitto -StartupType Automatic
```

### 8.6 Verificar que Mosquitto funciona

Abrir dos terminales PowerShell:

**Terminal 1 — Suscribirse a un topic:**
```powershell
cd "C:\Program Files\mosquitto"
.\mosquitto_sub.exe -h localhost -t "wms/basculas/estacion1" -v
```

**Terminal 2 — Publicar un mensaje de prueba:**
```powershell
cd "C:\Program Files\mosquitto"
.\mosquitto_pub.exe -h localhost -t "wms/basculas/estacion1" -m "12.34"
```

Si la Terminal 1 muestra `wms/basculas/estacion1 12.34`, Mosquitto está funcionando correctamente.

---

## 9. Ejecutar el servicio

### 9.1 Como aplicación de consola (desarrollo/pruebas)

```powershell
cd <ruta-local>\BasculaAgent
dotnet run
```

El agente mostrará los logs en la consola en tiempo real. Detener con `CTRL+C`.

### 9.2 Como servicio de Windows (producción)

```powershell
# 1. Publicar la aplicación como ejecutable standalone
dotnet publish -c Release -r win-x64 --self-contained true -o C:\Services\BasculaAgent

# 2. Crear el servicio de Windows
sc.exe create "BasculaAgent" binPath="C:\Services\BasculaAgent\BasculaAgent.exe" start=auto DisplayName="Bascula Agent MQTT"

# 3. Descripción del servicio (opcional)
sc.exe description "BasculaAgent" "Puente serial-MQTT para basculas Torrey."

# 4. Iniciar el servicio
sc.exe start BasculaAgent

# 5. Verificar estado
sc.exe query BasculaAgent
```

**Detener y eliminar el servicio:**
```powershell
sc.exe stop BasculaAgent
sc.exe delete BasculaAgent
```

### 9.3 Variables de entorno

El entorno se controla con la variable `DOTNET_ENVIRONMENT`:

| Valor | Comportamiento |
|---|---|
| `Production` (default) | Carga solo `appsettings.json` |
| `Development` | Carga `appsettings.json` + `appsettings.Development.json` (el segundo sobreescribe al primero) |

```powershell
$env:DOTNET_ENVIRONMENT = "Development"
dotnet run
```

---

## 10. Protocolo de mensajes MQTT

### Topic

```
wms/basculas/estacion1
```
(configurable en `appsettings.json`)

### Payload

El mensaje es un **texto plano UTF-8** con el peso en kilogramos, representado como número decimal con punto (`.`) como separador. **Sin unidad, solo el número.**

#### Ejemplos de payloads

| Situación | Payload publicado |
|---|---|
| Báscula envía `"12.50 kg"` | `12.50` |
| Báscula envía `"27.55 lbs"` | `12.4965` |
| Báscula envía `"435.5 oz"` | `12.3389` |
| Báscula envía `"0.00 kg"` | `0.00` |
| Báscula envía `"12.50"` (sin unidad) | `12.50` |

#### Propiedades del mensaje

| Propiedad | Valor |
|---|---|
| **QoS** | 1 (AtLeastOnce) |
| **Retain** | `true` |
| **Encoding** | UTF-8 |
| **Tipo de dato** | string numérico (parsear con `parseFloat` / `double.Parse`) |

> **Nota sobre `Retain = true`:** Al suscribirse a este topic, el broker entrega **inmediatamente** el último peso publicado, sin esperar al próximo ciclo de polling. Esto es ideal para dashboards que abren la página en cualquier momento.

---

## 11. Consumir los datos: ejemplos por tecnología

### 11.1 JavaScript / Node.js

**Instalar cliente MQTT:**
```bash
npm install mqtt
```

**Suscribirse y recibir el peso:**
```javascript
const mqtt = require('mqtt');

// Conectar al broker
const client = mqtt.connect('mqtt://localhost:1883');

client.on('connect', () => {
  console.log('Conectado al broker MQTT');

  // Suscribirse al topic de la báscula
  client.subscribe('wms/basculas/estacion1', (err) => {
    if (!err) {
      console.log('Suscrito. Esperando datos de la báscula...');
    }
  });
});

// Recibir mensajes
client.on('message', (topic, message) => {
  const pesoKg = parseFloat(message.toString());
  
  if (!isNaN(pesoKg)) {
    console.log(`Peso recibido: ${pesoKg} kg`);
    procesarPeso(topic, pesoKg);
  }
});

function procesarPeso(topic, kg) {
  // Ejemplo: enviar a una API REST
  fetch('https://mi-sistema.com/api/pesos', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      topic,
      peso: kg,
      unidad: 'kg',
      timestamp: new Date().toISOString()
    })
  });
}

client.on('error', (error) => {
  console.error('Error MQTT:', error);
});
```

**Suscribirse a todas las básculas (wildcard):**
```javascript
// '+' coincide con UN nivel del topic
client.subscribe('wms/basculas/+', callback);

// '#' coincide con el topic y TODOS sus sub-niveles
client.subscribe('wms/#', callback);
```

---

### 11.2 JavaScript en el navegador (MQTT over WebSockets)

> **Requisito:** Mosquitto debe tener el listener de WebSockets habilitado en el puerto 9001 (ver sección 8.4).

**Instalar usando CDN (sin npm):**
```html
<script src="https://unpkg.com/mqtt/dist/mqtt.min.js"></script>
```

**Ejemplo completo — Widget de peso en tiempo real:**
```html
<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8">
  <title>Báscula en tiempo real</title>
  <script src="https://unpkg.com/mqtt/dist/mqtt.min.js"></script>
</head>
<body>
  <h1>Peso actual</h1>
  <div id="peso" style="font-size: 3em;">Conectando...</div>
  <div id="timestamp"></div>

  <script>
    // WebSocket: protocol ws:// o wss:// (con TLS)
    const client = mqtt.connect('ws://localhost:9001');

    client.on('connect', () => {
      document.getElementById('peso').textContent = 'Esperando lectura...';
      client.subscribe('wms/basculas/estacion1');
    });

    client.on('message', (topic, message) => {
      const kg = parseFloat(message.toString());
      if (!isNaN(kg)) {
        document.getElementById('peso').textContent = kg.toFixed(2) + ' kg';
        document.getElementById('timestamp').textContent =
          'Última lectura: ' + new Date().toLocaleTimeString();
      }
    });

    client.on('error', (error) => {
      document.getElementById('peso').textContent = 'Error de conexión';
      console.error(error);
    });
  </script>
</body>
</html>
```

**Ejemplo en React:**
```jsx
import { useEffect, useState } from 'react';
import mqtt from 'mqtt';

export default function IndicadorPeso() {
  const [peso, setPeso] = useState(null);
  const [conectado, setConectado] = useState(false);

  useEffect(() => {
    const client = mqtt.connect('ws://localhost:9001');

    client.on('connect', () => {
      setConectado(true);
      client.subscribe('wms/basculas/estacion1');
    });

    client.on('message', (topic, message) => {
      const kg = parseFloat(message.toString());
      if (!isNaN(kg)) setPeso(kg);
    });

    client.on('close', () => setConectado(false));

    // Cleanup al desmontar el componente
    return () => client.end();
  }, []);

  return (
    <div>
      <p>Estado: {conectado ? 'Conectado' : 'Desconectado'}</p>
      <p>Peso: {peso !== null ? peso.toFixed(2) + ' kg' : 'Sin lectura'}</p>
    </div>
  );
}
```

---

### 11.3 Python

**Instalar:**
```bash
pip install paho-mqtt
```

**Suscribirse y recibir peso:**
```python
import paho.mqtt.client as mqtt
from datetime import datetime

BROKER_HOST = "localhost"
BROKER_PORT = 1883
TOPIC = "wms/basculas/estacion1"

def on_connect(client, userdata, flags, rc, properties):
    if rc == 0:
        print("Conectado al broker MQTT")
        client.subscribe(TOPIC)
    else:
        print(f"Error de conexión. Código: {rc}")

def on_message(client, userdata, msg):
    try:
        peso_kg = float(msg.payload.decode('utf-8'))
        timestamp = datetime.now().isoformat()
        print(f"[{timestamp}] Topic: {msg.topic} | Peso: {peso_kg:.4f} kg")
        
        # Aquí puedes guardar en base de datos, llamar una API, etc.
        guardar_lectura(msg.topic, peso_kg, timestamp)
        
    except ValueError:
        print(f"Payload inválido: {msg.payload}")

def guardar_lectura(topic, peso_kg, timestamp):
    # Ejemplo: conectar a PostgreSQL, MySQL, MongoDB, etc.
    print(f"  -> Guardando: {peso_kg} kg @ {timestamp}")

# Crear cliente MQTT v5
client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2)
client.on_connect = on_connect
client.on_message = on_message

client.connect(BROKER_HOST, BROKER_PORT, keepalive=60)

# Bloquea el hilo y procesa mensajes indefinidamente
client.loop_forever()
```

**Suscribirse a todas las básculas con wildcard:**
```python
import paho.mqtt.client as mqtt

def on_message(client, userdata, msg):
    estacion = msg.topic.split('/')[-1]   # "estacion1", "estacion2", etc.
    peso_kg = float(msg.payload.decode())
    print(f"Estación {estacion}: {peso_kg} kg")

client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2)
client.on_connect = lambda c, u, f, rc, p: c.subscribe("wms/basculas/+")
client.on_message = on_message
client.connect("localhost", 1883)
client.loop_forever()
```

---

### 11.4 C# (.NET)

**Agregar paquete NuGet:**
```xml
<PackageReference Include="MQTTnet" Version="5.2.0.1603" />
```

**Suscriptor completo:**
```csharp
using MQTTnet;
using System.Text;

var factory = new MqttClientFactory();
using var client = factory.CreateMqttClient();

var options = new MqttClientOptionsBuilder()
    .WithTcpServer("localhost", 1883)
    .WithClientId("MiSistema-Consumidor")
    .WithCleanSession(true)
    .Build();

// Manejar mensajes recibidos
client.ApplicationMessageReceivedAsync += e =>
{
    var topic = e.ApplicationMessage.Topic;
    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

    if (double.TryParse(payload, 
        System.Globalization.NumberStyles.Float,
        System.Globalization.CultureInfo.InvariantCulture, 
        out double pesoKg))
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {topic}: {pesoKg:F4} kg");
        // ProcesarPeso(topic, pesoKg);
    }

    return Task.CompletedTask;
};

await client.ConnectAsync(options);

// Suscribirse al topic
await client.SubscribeAsync(new MqttTopicFilterBuilder()
    .WithTopic("wms/basculas/estacion1")
    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
    .Build());

Console.WriteLine("Escuchando báscula... CTRL+C para detener.");
Console.ReadLine();

await client.UnsubscribeAsync("wms/basculas/estacion1");
await client.DisconnectAsync();
```

---

### 11.5 SAP Business One — Add-on / DI API

Este ejemplo muestra cómo integrar el peso en un add-on de SAP B1 escrito en C# con DI API, útil para capturar el peso automáticamente al crear una Entrega de Mercancía o al pesar artículos en Recepción.

**Requisitos:**
- SAP Business One SDK (DI API)
- Paquete NuGet `MQTTnet` en el proyecto del add-on

```csharp
using MQTTnet;
using SAPbobsCOM;
using System.Text;

public class BasculaIntegration : IDisposable
{
    private IMqttClient? _mqttClient;
    private double _pesoActual = 0.0;
    private readonly Company _sapCompany;

    public BasculaIntegration(Company sapCompany)
    {
        _sapCompany = sapCompany;
    }

    /// <summary>
    /// Inicia la conexión MQTT y escucha el peso en segundo plano.
    /// Llamar al iniciar el add-on.
    /// </summary>
    public async Task IniciarAsync(
        string mqttHost = "localhost",
        int mqttPort = 1883,
        string topic = "wms/basculas/estacion1")
    {
        var factory = new MqttClientFactory();
        _mqttClient = factory.CreateMqttClient();

        _mqttClient.ApplicationMessageReceivedAsync += msg =>
        {
            var payload = Encoding.UTF8.GetString(msg.ApplicationMessage.PayloadSegment);
            if (double.TryParse(payload,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out double peso))
            {
                _pesoActual = peso;
            }
            return Task.CompletedTask;
        };

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(mqttHost, mqttPort)
            .WithClientId($"SAPB1-Addon-{Environment.MachineName}")
            .Build();

        await _mqttClient.ConnectAsync(options);
        await _mqttClient.SubscribeAsync(topic);
    }

    /// <summary>
    /// Retorna el último peso recibido de la báscula en kilogramos.
    /// </summary>
    public double ObtenerPesoActual() => _pesoActual;

    /// <summary>
    /// Crea una Entrega de Mercancía (Delivery) en SAP B1 con el peso 
    /// de la báscula como cantidad del artículo.
    /// </summary>
    public int CrearEntregaConPeso(string codigoSocio, string codigoArticulo)
    {
        var entrega = (Documents)_sapCompany.GetBusinessObject(
            BoObjectTypes.oDeliveryNotes);

        entrega.CardCode = codigoSocio;

        entrega.Lines.ItemCode = codigoArticulo;
        entrega.Lines.Quantity = _pesoActual;   // Peso de la báscula como cantidad
        entrega.Lines.UoMCode = "KG";           // Unidad de medida en SAP

        int resultado = entrega.Add();

        if (resultado == 0)
        {
            _sapCompany.GetNewObjectCode(out string docEntry);
            Console.WriteLine($"Entrega creada. DocEntry: {docEntry}, Peso: {_pesoActual} kg");
            return int.Parse(docEntry);
        }
        else
        {
            string error = _sapCompany.GetLastErrorDescription();
            throw new Exception($"Error SAP al crear entrega: {error}");
        }
    }

    public void Dispose()
    {
        _mqttClient?.DisconnectAsync().Wait();
        _mqttClient?.Dispose();
    }
}
```

**Uso en el formulario del add-on:**
```csharp
// Al inicializar el add-on
var bascula = new BasculaIntegration(oCompany);
await bascula.IniciarAsync(
    mqttHost: "mqtt.example.local",   // IP del servidor donde corre Mosquitto
    topic: "wms/basculas/estacion1"
);

// En el evento de un botón "Capturar Peso" en el formulario SAP
void btnCapturarPeso_Click()
{
    double peso = bascula.ObtenerPesoActual();
    // Asignar al campo de cantidad en pantalla
    oForm.Items.Item("Quantity").Specific.Value = peso.ToString("F4");
}

// Al crear el documento SAP con el peso
void btnCrearEntrega_Click()
{
    bascula.CrearEntregaConPeso("C0001", "ARTICULO-001");
}
```

---

### 11.6 SAP Business One — Integration Framework (B1if)

B1if puede consumir MQTT mediante un microservicio puente que expone el peso como API REST.

**Microservicio puente MQTT → REST (Node.js):**

```javascript
// servidor-peso.js
const mqtt = require('mqtt');
const express = require('express');

const app = express();
let pesoActual = null;
let ultimaActualizacion = null;

// Suscribirse a la báscula
const mqttClient = mqtt.connect('mqtt://localhost:1883');
mqttClient.on('connect', () => {
  mqttClient.subscribe('wms/basculas/estacion1');
});
mqttClient.on('message', (topic, message) => {
  pesoActual = parseFloat(message.toString());
  ultimaActualizacion = new Date().toISOString();
});

// Exponer como API REST para B1if
app.get('/api/peso/actual', (req, res) => {
  res.json({
    peso: pesoActual,
    unidad: 'kg',
    timestamp: ultimaActualizacion,
    disponible: pesoActual !== null
  });
});

app.listen(3000, () => console.log('API de báscula en puerto 3000'));
```

**Configuración en B1if:**
- **Sender:** HTTP Trigger (o Timer para polling periódico)
- **Receiver:** HTTP Adapter → `GET http://localhost:3000/api/peso/actual`
- **Mapping:** Mapear `$.peso` al campo de cantidad en el Business Object de SAP

---

## 12. Múltiples básculas

Para controlar varias básculas simultáneamente, ejecutar una instancia de BasculaAgent por báscula, cada una con su propio `appsettings.json` apuntando a un COM y un topic diferente.

### Estructura recomendada de carpetas

```
C:\Services\
├── BasculaAgent-Estacion1\
│   ├── BasculaAgent.exe
│   └── appsettings.json  -> COM3, topic: wms/basculas/estacion1
├── BasculaAgent-Estacion2\
│   ├── BasculaAgent.exe
│   └── appsettings.json  -> COM4, topic: wms/basculas/estacion2
└── BasculaAgent-Recepcion\
    ├── BasculaAgent.exe
    └── appsettings.json  -> COM5, topic: wms/basculas/recepcion
```

### Crear los servicios de Windows

```powershell
sc.exe create "BasculaAgent-Est1" binPath="C:\Services\BasculaAgent-Estacion1\BasculaAgent.exe" start=auto
sc.exe create "BasculaAgent-Est2" binPath="C:\Services\BasculaAgent-Estacion2\BasculaAgent.exe" start=auto
sc.exe create "BasculaAgent-Rec"  binPath="C:\Services\BasculaAgent-Recepcion\BasculaAgent.exe"  start=auto

sc.exe start BasculaAgent-Est1
sc.exe start BasculaAgent-Est2
sc.exe start BasculaAgent-Rec
```

### Suscribirse a todas con wildcard

```javascript
// Node.js: recibir de TODAS las básculas con un solo suscriptor
client.subscribe('wms/basculas/+');

client.on('message', (topic, message) => {
  const estacion = topic.split('/').pop(); // "estacion1", "estacion2", "recepcion"
  const peso = parseFloat(message.toString());
  console.log(`${estacion}: ${peso} kg`);
});
```

---

## 13. Troubleshooting

### El servicio arranca pero no lee la báscula

1. **Verificar el puerto COM:**
   ```powershell
   [System.IO.Ports.SerialPort]::GetPortNames()
   ```
   Si `COM3` no aparece, revisar el Administrador de dispositivos.

2. **Verificar que nadie más usa el puerto:** El puerto COM es exclusivo — solo un proceso puede abrirlo simultáneamente.

3. **Verificar BaudRate:** Debe coincidir exactamente con la configuración de la báscula. Consultar el manual de la báscula Torrey.

4. **Aumentar `ReadDelayMs`:** Si los logs muestran respuestas vacías o incompletas, aumentar a `500` o `1000`.

5. **Revisar el comando de polling:** Algunas básculas requieren `"\r"` o `"P\r\n"` en lugar de `"P"`.

### El servicio no publica en MQTT

1. **Verificar que Mosquitto está corriendo:**
   ```powershell
   Get-Service mosquitto
   netstat -an | findstr 1883
   ```

2. **Probar conectividad:**
   ```powershell
   Test-NetConnection -ComputerName localhost -Port 1883
   ```

3. **Revisar el firewall de Windows:**
   ```powershell
   New-NetFirewallRule -DisplayName "MQTT Mosquitto" -Direction Inbound -Protocol TCP -LocalPort 1883 -Action Allow
   ```

4. **Revisar logs de Mosquitto:** `C:\Program Files\mosquitto\mosquitto.log`

### Recibo pesos incorrectos o con decimales raros

- El payload siempre es en **kilogramos** con **punto decimal**.
- Parsear con `parseFloat()` en JS o `double.Parse(value, CultureInfo.InvariantCulture)` en C#.
- **Nunca** usar `Convert.ToDouble()` sin `CultureInfo.InvariantCulture` en C# — puede fallar en sistemas con configuración regional de coma decimal.

### Los logs muestran "Unidad detectada no está en la lista aceptada"

La báscula está enviando una unidad no configurada. Agregarla en `appsettings.json`:
```json
"UnidadesAceptadas": [ "Kilos", "Libras" ]
```

### El servicio no reconecta automáticamente tras desconectar el cable serial

Este es el comportamiento esperado. El agente detectará el error en la próxima iteración del bucle, cerrará el puerto y esperará `ReconnectDelaySeconds` antes de reintentar. Al volver a conectar el cable, el agente lo detectará y abrirá el puerto automáticamente.

---

## 14. Preguntas Frecuentes (FAQ)

**¿El peso siempre llega en kilogramos aunque la báscula mande libras?**  
Sí. El agente convierte automáticamente a kilogramos antes de publicar en MQTT. El payload siempre será en kg.

**¿Qué pasa si la báscula manda `0.00`? ¿Se filtra?**  
No. El agente publica `0.00` tal cual. Si necesitas filtrar lecturas en cero, hazlo en el consumidor.

**¿Puedo tener más de un consumidor suscrito al mismo topic?**  
Sí, MQTT admite múltiples suscriptores simultáneamente. Un sistema web, un script Python y un add-on de SAP pueden recibir el peso al mismo tiempo.

**¿El broker MQTT guarda histórico?**  
Por defecto, Mosquitto solo retiene el **último** mensaje (por el flag `retain`). Para histórico, el consumidor debe persistirlo en una base de datos.

**¿Funciona el agente en Linux o Mac?**  
Sí, con limitaciones: `System.IO.Ports` en Linux requiere permisos sobre `/dev/ttyS*` o `/dev/ttyUSB*`. Cambiar `PortName` a `/dev/ttyUSB0` o similar.

**¿Puedo cambiar la configuración sin reiniciar el servicio?**  
No en la implementación actual. Los valores de `appsettings.json` se leen una sola vez en el constructor. Para aplicar cambios, reiniciar el servicio.

**¿Cómo agrego autenticación MQTT (usuario/contraseña)?**  
Modificar `EnsureMqttConnectedAsync` en `Worker.cs`:
```csharp
var options = new MqttClientOptionsBuilder()
    .WithTcpServer(_mqttHost, _mqttPort)
    .WithCredentials("usuario", "contraseña")  // Agregar esta línea
    .WithClientId(...)
    .Build();
```
Y agregar las claves correspondientes en `appsettings.json`.

---

*Documentación generada para BasculaAgent — .NET 10 Worker Service | MQTTnet 5.x | Báscula Torrey RS-232*
