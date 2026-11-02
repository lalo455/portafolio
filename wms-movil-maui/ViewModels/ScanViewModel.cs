using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts; // Para los Toasts
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Storage;
using WmsMovil.Messages;
using WmsMovil.Services;
using System.Diagnostics;

#if ANDROID
using WmsMovil.Platforms.Android.Services; // Para el Beep
#endif

namespace WmsMovil.ViewModels
{
    public partial class ScanViewModel : ObservableObject
    {
        private readonly IRfidService _rfidService;

        private bool _isScanning = false;
        private HashSet<string> _uniqueTags = new();
        private List<string> _tagsPendientesDeUI = new();
        private readonly object _uiLock = new object();

        public ObservableCollection<string> ScannedTags { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsQrMode))]
        private bool _isRfidMode = true;

        // Este método  se ejecuta cada vez que el Switch cambia
        // Este método se ejecuta cada vez que mueves el Switch en la pantalla
        partial void OnIsRfidModeChanged(bool value)
        {
            GlobalSettings.IsRfidMode = value;

            if (value) // MODO RFID
            {
                _rfidService.Init();

#if ANDROID
                // Le decimos a la Handheld que PAUSE el láser rojo
                var intent = new Android.Content.Intent("com.rscja.android.action.BARCODE_SCAN_PAUSE");
                Android.App.Application.Context.SendBroadcast(intent);
#endif
                EstadoUI = "Modo RFID Listo.";
            }
            else // MODO QR
            {
                if (_isScanning) DetenerLectura();
                _rfidService.Free();

#if ANDROID
                // Le decimos a la Handheld que REANUDE el láser rojo
                var intent = new Android.Content.Intent("com.rscja.android.action.BARCODE_SCAN_RESUME");
                Android.App.Application.Context.SendBroadcast(intent);
#endif
                EstadoUI = "Modo QR Listo. Apunte y dispare.";
            }
        }
        // Propiedad inversa para mostrar/ocultar cosas del QR
        public bool IsQrMode => !IsRfidMode;

        [ObservableProperty]
        private string _estadoUI = "Listo para escanear";

        [ObservableProperty]
        private int _rfPower;

        // Propiedad para capturar el texto del QR
        [ObservableProperty]
        private string _qrText;

        public ScanViewModel(IRfidService rfidService)
        {
            _rfidService = rfidService;
            RfPower = Preferences.Default.Get("LastRfPower", 15);

            WeakReferenceMessenger.Default.Register<GatilloMensaje>(this, (r, m) =>
            {
                // El gatillo físico SOLO controla el RFID. El QR se controla con el láser nativo de la Handheld.
                if (IsRfidMode)
                {
                    if (m.Value) IniciarLectura();
                    else DetenerLectura();
                }
            });
        }

        partial void OnRfPowerChanged(int value)
        {
            Preferences.Default.Set("LastRfPower", value);
            _rfidService.SetPower(value);
        }

        private async void IniciarLectura()
        {
            if (_isScanning) return;

            if (!_rfidService.isPowerOn() && !_rfidService.Init())
            {
                EstadoUI = "Error: No se pudo conectar con el módulo RFID.";
                return;
            }

            _isScanning = true;
            EstadoUI = "Antena RFID Encendida...";

            // UX: Vibración, Sonido y Toast
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
#if ANDROID
            AudioService.PlayBeep(true);
#endif
            await Toast.Make("Lectura iniciada", ToastDuration.Short).Show();

            _uniqueTags.Clear();
            ScannedTags.Clear();
            lock (_uiLock) { _tagsPendientesDeUI.Clear(); }

            _rfidService.SetPower(RfPower);
            _rfidService.setEPCandTIDMode();
            _rfidService.StopInventory();
            await Task.Delay(200);

            if (!_rfidService.StartInventoryTag(1, 2))
            {
                EstadoUI = "Fallo de hardware al iniciar inventario.";
                _isScanning = false;
                return;
            }

            _ = LeerBufferRfidAsync();
            _ = ActualizarPantallaAsync();
        }

        private async void DetenerLectura()
        {
            if (!_isScanning) return;
            _isScanning = false;

            _rfidService.StopInventory();
            EstadoUI = $"RFID Detenido. Total leídos: {_uniqueTags.Count}";

            // UX: Vibración larga, Sonido y Toast
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
#if ANDROID
            AudioService.PlayBeep(false);
#endif
            await Toast.Make("Lectura terminada", ToastDuration.Short).Show();
        }

        // --- LÓGICA DEL CÓDIGO QR ---
        // Este método se dispara cuando el láser lee el QR y manda el "Enter"
        [RelayCommand]
        private async Task QrScannedAsync()
        {
            if (string.IsNullOrWhiteSpace(QrText)) return;

            // UX: Sonido de éxito
#if ANDROID
            AudioService.PlayBeep(true);
#endif

            // Agregamos el QR a la lista
            ScannedTags.Insert(0, $"[QR] {QrText}");
            EstadoUI = "QR Leído correctamente";

            // Limpiamos la caja de texto para el siguiente escaneo
            QrText = string.Empty;
        }

        // ... (Deja los métodos LeerBufferRfidAsync, ActualizarPantallaAsync y Limpiar exactamente igual que antes) ...
        private async Task LeerBufferRfidAsync()
        {
            await Task.Run(async () =>
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
                                lock (_uiLock) { _tagsPendientesDeUI.Add(tag.EPC); }
                            }
                        }
                    }
                    catch { }
                }
            });
        }

        private async Task ActualizarPantallaAsync()
        {
            while (_isScanning)
            {
                await Task.Delay(500);
                List<string> tagsParaDibujar = new();
                lock (_uiLock)
                {
                    if (_tagsPendientesDeUI.Count > 0)
                    {
                        tagsParaDibujar.AddRange(_tagsPendientesDeUI);
                        _tagsPendientesDeUI.Clear();
                    }
                }
                if (tagsParaDibujar.Count > 0)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        foreach (var epc in tagsParaDibujar) ScannedTags.Insert(0, epc);
                        EstadoUI = $"Leyendo... ({_uniqueTags.Count} tags únicos)";
                    });
                }
            }
        }

        [RelayCommand]
        private void Limpiar()
        {
            _uniqueTags.Clear();
            ScannedTags.Clear();
            lock (_uiLock) { _tagsPendientesDeUI.Clear(); }
            EstadoUI = "Memoria limpiada";
        }
    }
}