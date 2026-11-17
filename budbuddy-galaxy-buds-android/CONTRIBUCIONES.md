# BudBuddy — fork con mejoras para Galaxy Buds en teléfonos no Samsung

Fork de [BudBuddy](https://github.com/BenEgeDeniz/BudBuddy) (GPL-3.0, por Ege Deniz), una app Android que controla
auriculares Samsung Galaxy Buds por Bluetooth SPP en teléfonos que no son Samsung y recupera funciones que
normalmente solo da Galaxy Wearable. Uso diario: **Galaxy Buds4 Pro + Google Pixel 10 (Android 17)**.

**Stack:** Kotlin, Jetpack Compose (Material 3), Coroutines/Flow, Glance (widgets), Bluetooth RFCOMM/SPP,
Companion Device Manager, kotlinx.serialization, Gradle (KTS), R8.

## Aportaciones de este fork

### Idioma
- Traducción completa al **español** (`values-es`, ~600 cadenas) y selector de idioma.
- Se agregó `locales_config` para el idioma por app de Android 13+, y `es` a `resourceConfigurations`.
  Sin esto, el build descartaba la traducción.
- Textos que estaban escritos directo en el código se pasaron a recursos.

### Conexión y estabilidad
- **Reconexión automática** con backoff cuando se cae el enlace SPP, sin pelear con una desconexión manual.
- **Carrera de reconexión corregida:** cada conexión usa su propio socket con un contador de generación, así un
  hilo viejo bloqueado en `read()` ya no puede cerrar el socket nuevo.
- Al reconectar se limpia el estado de deduplicación de ANC/EQ y se vacía la cola de paquetes. Antes la regla
  activa no se volvía a aplicar.
- **Verificación CRC16** de los paquetes entrantes. Es adaptativa: se exige solo después de validar el primer
  CRC del enlace.
- Los receivers ya no truenan con `ForegroundServiceStartNotAllowedException` (Android 12+).
- Se quitó el permiso `BLUETOOTH_SCAN`, que no se usaba, y se corrigió una fuga del motor TTS.

### Funciones nuevas
- **Companion Device Manager:** la app se asocia con los Buds para iniciar el servicio cuando aparecen
  (`CompanionDeviceService`), abrir actividades desde segundo plano y evitar las restricciones de batería.
- **Gemini con gestos de cabeza**, usando el IMU de los Buds. El asistente se abre con varios métodos de respaldo.
- **EQ automático por género y BPM:** el género sale de iTunes y el tempo de la API pública de Deezer. Hay
  9 grupos de géneros editables y funciona con cualquier reproductor (YouTube Music, Spotify…).
- **Control de ruido según la red:** con Wi-Fi (casa/lugar seguro) un modo, con datos móviles otro.
  No necesita permiso de ubicación.
- **Música al hablar:** cuando la detección de voz activa Transparencia, la app pausa o baja el volumen
  (a elección del usuario) y lo restaura al terminar.
- **Bienestar:** aviso de batería baja por auricular (notificación y voz) y recordatorio para estirar el cuello
  basado en el movimiento de la cabeza.
- **Widget "Batería y atajos"** en Glance: batería de I/D/estuche, Gemini y un botón para el EQ automático.
- **Integración con Tasker** por intents, desactivada por defecto.

### Calidad
- Tests unitarios del clasificador de géneros; se corrigió un test roto del proyecto original.
- Firma de release mediante `keystore.properties`, que no se versiona.

## Compilar
```bash
./gradlew :app:assembleGithubDebug          # APK de depuración
./gradlew :app:assembleGithubRelease        # requiere keystore.properties (ver app/build.gradle.kts)
./gradlew :app:testGithubDebugUnitTest
```
Si Play Protect bloquea la app por los permisos sensibles (acceso a notificaciones), instálala por ADB:
`adb install -r app-github-release.apk`.

Licencia: GPL-3.0, igual que el proyecto original (ver `LICENSE`).
