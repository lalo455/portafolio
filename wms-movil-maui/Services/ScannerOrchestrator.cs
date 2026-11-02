using System.Diagnostics;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Storage;
#if ANDROID
using WmsMovil.Platforms.Android.Services; // Para el Beep
#endif

namespace WmsMovil.Services
{
    public class ScannerOrchestrator
    {
        private readonly IRfidService _rfidService;
        private bool _isScanning = false;

        private HashSet<string> _uniqueTags = new();
        private List<string> _tagsPendientes = new();
        private readonly object _lock = new object();

        // Evento al que las pantallas se van a suscribir para recibir los tags
        public event Action<List<string>> OnTagsUpdated;
        public event Action<string> OnStatusChanged;
        public event Action<List<string>> OnScanFinished;
        public ScannerOrchestrator(IRfidService rfidService)
        {
            _rfidService = rfidService;
        }

        // Se llama cuando el usuario cambia el Switch a RFID o QR
        public async Task PrepararHardwareAsync(bool isRfidMode)
        {
            GlobalSettings.IsRfidMode = isRfidMode;

            await Task.Run(() => // Lo hacemos en segundo plano para no trabar la UI
            {
                if (isRfidMode)
                {
                    _rfidService.Init();
                    OnStatusChanged?.Invoke("Modo RFID Listo");
                }
                else
                {
                    if (_isScanning) Detener();
                    _rfidService.Free();
                    OnStatusChanged?.Invoke("Modo QR Listo");
                }
            });
        }

        // Se llama cuando se aprieta el gatillo
        public void ToggleScanner()
        {
            if (_isScanning) Detener();
            else Iniciar();
        }

        private async void Iniciar()
        {
            if (_isScanning) return;

            if (!_rfidService.isPowerOn() && !_rfidService.Init())
            {
                _ = Toast.Make("Error de conexión RFID", ToastDuration.Long).Show();
                return;
            }

            _isScanning = true;
            _uniqueTags.Clear();
            lock (_lock) { _tagsPendientes.Clear(); }

            // UX: Vibración y Beep de Inicio
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
#if ANDROID
            AudioService.PlayBeep(true);
#endif

            int power = Preferences.Default.Get("LastRfPower", 15);
            _rfidService.SetPower(power);
            _rfidService.setEPCandTIDMode();
            _rfidService.StopInventory();
            await Task.Delay(100);

            if (!_rfidService.StartInventoryTag(1, 2))
            {
                _isScanning = false;
                OnStatusChanged?.Invoke("Fallo al iniciar antena");
                return;
            }

            OnStatusChanged?.Invoke("Escaneando RFID...");
            _ = LeerBufferAsync();
            _ = DespacharTagsAsync();
        }

        private void Detener()
        {
            if (!_isScanning) return;
            _isScanning = false;
            _rfidService.StopInventory();

            // UX: Vibración y Beep de Fin
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
#if ANDROID
            AudioService.PlayBeep(false);
#endif
            OnStatusChanged?.Invoke($"Detenido. Total: {_uniqueTags.Count}");
            // Le pasamos a la pantalla todos los tags que recolectamos en este gatillazo
            MainThread.BeginInvokeOnMainThread(() => OnScanFinished?.Invoke(_uniqueTags.ToList()));
        }

        private async Task LeerBufferAsync()
        {
            await Task.Run(() =>
            {
                while (_isScanning)
                {
                    try
                    {
                        var tag = _rfidService.ReadTagFromBuffer();
                        if (tag != null && !string.IsNullOrWhiteSpace(tag.EPC))
                        {
                            if (_uniqueTags.Add(tag.EPC))
                            {
                                lock (_lock) { _tagsPendientes.Add(tag.EPC); }
                            }
                        }
                    }
                    catch { }
                }
            });
        }

        private async Task DespacharTagsAsync()
        {
            while (_isScanning)
            {
                await Task.Delay(500); // Entregamos tags cada medio segundo
                List<string> lote = new();
                lock (_lock)
                {
                    if (_tagsPendientes.Count > 0)
                    {
                        lote.AddRange(_tagsPendientes);
                        _tagsPendientes.Clear();
                    }
                }

                if (lote.Count > 0)
                {
                    // Le enviamos los tags nuevos a la pantalla que esté activa
                    MainThread.BeginInvokeOnMainThread(() => OnTagsUpdated?.Invoke(lote));
                }
            }
        }
        public void SetPower(int power)
        {
            _rfidService.SetPower(power);
        }
        public void LimpiarMemoria()
        {
            _uniqueTags.Clear();
            lock (_lock) { _tagsPendientes.Clear(); }
        }
    }
}