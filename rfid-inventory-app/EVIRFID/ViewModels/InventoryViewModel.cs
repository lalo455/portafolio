using Com.Rscja.Deviceapi.Entity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EVIRFID.Models;
using EVIRFID.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace EVIRFID.ViewModels
{
    public partial class InventoryViewModel : BaseViewModel
    {
        // Colección mostrada en la pantalla
        public ObservableRangeCollection<Tag> ListaEpcs { get; set; }

        // Fuente de cancelación para parar el procesamiento en segundo plano
        private CancellationTokenSource cts;

        // Set para manejo de EPCs (El HashSet evita duplicados de forma nativa y ultra rápida)
        private readonly HashSet<string> epcSet = new HashSet<string>();
        private DateTime _horaInicioEscaneo;

        [ObservableProperty]
        int rfPower = 0;

        [ObservableProperty]
        bool isScanning = false;

        [ObservableProperty]
        private bool showPowerPanel = false;

        [ObservableProperty]
        public int powerToSet = 15; 

        [ObservableProperty]
        public int rfRssi = 50;

        [ObservableProperty]
        public int rfQValue = 2;

        [ObservableProperty]
        public int rfSession = 1;


        // ---- COMANDOS ----
        public IAsyncCommand TogglePowerPanelCommand { get; }
        public IAsyncCommand SavePowerCommand { get; }
        public IAsyncCommand CancelPowerCommand { get; }

        public ICommand ScanCommand { get; }
        public ICommand ClearEpcListCommand { get; }
        public ICommand ReintentarEnvioCommand { get; }
        public IAsyncCommand GoToCalibrationCommand { get; }

        public InventoryViewModel()
        {
            Title = "Inventario";

            // Comandos
            ScanCommand = new Command(OnGatillazo); 
            ClearEpcListCommand = new Command(ClearEpcList);
            ReintentarEnvioCommand = new AsyncCommand(ReintentarEnvioAsync);
            GoToCalibrationCommand = new AsyncCommand(GoToCalibrationAsync);

            TogglePowerPanelCommand = new AsyncCommand(TogglePowerPanel);
            SavePowerCommand = new AsyncCommand(SavePowerAsync);
            CancelPowerCommand = new AsyncCommand(CancelPowerAsync);

            ListaEpcs = new ObservableRangeCollection<Tag>();
         

           
            // Si es la primera vez que instalan la app, usará 15 por defecto
            rfPower = Preferences.Get("LastRfPower", 15);
            rfRssi = Preferences.Get("LastRfRssi", 50);
            rfQValue = Preferences.Get("LastRfQValue", 2);
            rfSession = Preferences.Get("LastRfSession", 1);

            // Fuerza el valor al rango permitido (1 a 30) por seguridad
            rfPower = Math.Min(Math.Max(rfPower, 1), 30);
            powerToSet = rfPower;
          
 
          
        }

        private Task TogglePowerPanel()
        {
            showPowerPanel = !showPowerPanel;
            if (showPowerPanel) powerToSet = rfPower; // Sincroniza el valor actual
            return Task.CompletedTask;
        }

        private Task CancelPowerAsync()
        {
            showPowerPanel = false;
            powerToSet = rfPower;
            return Task.CompletedTask;
        }

        private async Task SavePowerAsync()
        {
            try
            {
                showPowerPanel = false;
                bool ok = await Task.Run(() => servicioRFID.SetPower(powerToSet));

                if (ok)
                {
                    rfPower = powerToSet;
                    await Application.Current.MainPage.DisplayToastAsync($"Potencia aplicada: {powerToSet}", 1200);
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No se pudo aplicar la potencia al lector.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error aplicando potencia: {ex}");
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        public bool ApplySettings(int newPower, int newRssi, int newQValue, int newSession)
        {
          
            bool exito = servicioRFID.SetPower(newPower);

            if (exito)
            {
                rfPower = newPower;
                rfRssi = newRssi;
                rfQValue = newQValue;
                rfSession = newSession;

                // Guardar permanentemente 
                Preferences.Set("LastRfPower", newPower);
                Preferences.Set("LastRfRssi", newRssi);
                Preferences.Set("LastRfQValue", newQValue);
                Preferences.Set("LastRfSession", newSession);
            }

            return exito;
        }

        private async Task GoToCalibrationAsync()
        {
            if (Application.Current.MainPage != null)
            {
                await Application.Current.MainPage.Navigation.PushModalAsync(new EVIRFID.Views.CalibrationPage());
            }
        }

        public async void OnGatillazo()
        {
            if (IsScanning)
            {
                await StopScanAsync();
            }
            else
            {
                await StartScanAsync();
            }
        }

        void ClearEpcList()
        {
            ListaEpcs.Clear();
            epcSet.Clear();
        }

        private async Task StartScanAsync()
        {
            if (IsScanning) return;

            IsScanning = true;
            _horaInicioEscaneo = DateTime.Now;

            DependencyService.Get<IAudioService>()?.PlayBeep(isNewTag: false);

            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayToastAsync("Empezando lectura offline", 900);
            });

            MessagingCenter.Send<App>((App)Application.Current, "ScanStarted");

            epcSet.Clear();
            ListaEpcs.Clear();
            cts = new CancellationTokenSource();

            bool powerOk = await Task.Run(() => servicioRFID.SetPower(rfPower));
            if (!powerOk)
            {
                IsScanning = false;
                return;
            }

            await Task.Run(() => servicioRFID.setEPCandTIDMode());
            await Task.Run(() => servicioRFID.StopInventory());
            await Task.Delay(200);

            bool started = await Task.Run(() => servicioRFID.StartInventoryTag(1, rfQValue, rfSession));
            if (!started)
            {
                IsScanning = false;
                Device.BeginInvokeOnMainThread(async () => {
                    await Application.Current.MainPage.DisplayAlert("Error", "Fallo de hardware. Reinicie la app.", "OK");
                });
                return;
            }

            _ = Task.Run(async () =>
            {
                while (IsScanning && !cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        UHFTAGInfo res = await servicioRFID.ReadTagFromBufferAsync();

                       
                        if (res is not null && !string.IsNullOrEmpty(res.EPC))
                        {
                            int rssiValue = 0;
                            if (float.TryParse(res.Rssi, out float rawRssi))
                            {
                                rssiValue = (int)Math.Max(0, 100 + rawRssi);
                            }

                            if (rssiValue >= rfRssi)
                            {
                                if (epcSet.Add(res.EPC)) // Acumula en memoria RAM y evita duplicados
                                {
                                    DependencyService.Get<IAudioService>()?.PlayBeep(isNewTag: true);
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        ListaEpcs.Add(new Tag { epc = res.EPC });
                                    });
                                }
                            }
                        }
                    }
                    catch
                    {
                      
                    }
                }
            });
        }

        private async Task StopScanAsync()
        {
            if (!IsScanning) return;

            // Seguro anti-gatillazo corto (Asegura al menos 4 segundos de lectura)
            var tiempoTranscurrido = (DateTime.Now - _horaInicioEscaneo).TotalSeconds;
            if (tiempoTranscurrido < 4.0)
            {
                await Task.Delay((int)((4.0 - tiempoTranscurrido) * 1000));
            }

            IsScanning = false;
            MessagingCenter.Send<App>((App)Application.Current, "ScanStopped");

            DependencyService.Get<IAudioService>()?.PlayBeep(isNewTag: false);

            // Apagar lector
            await Task.Run(() => { try { servicioRFID.StopInventory(); } catch { } });
            cts?.Cancel();

            // --- ENVÍO EN BLOQUE (BATCH) AL WMS ---
            if (epcSet.Count > 0)
            {
                Device.BeginInvokeOnMainThread(async () => {
                    await Application.Current.MainPage.DisplayToastAsync("Sincronizando con WMS...", 1000);
                });

                bool exito = await ((App)Application.Current).SendBatchAsync(epcSet.ToList());

                if (exito)
                {
                    Device.BeginInvokeOnMainThread(async () => {
                        await Application.Current.MainPage.DisplayToastAsync("¡Lectura enviada con éxito!", 1500);
                        ListaEpcs.Clear();
                        epcSet.Clear();
                    });
                }
                else
                {
                    // Si falla, los datos SE QUEDAN en memoria y en pantalla para poder reintentar
                    Device.BeginInvokeOnMainThread(async () => {
                        await Application.Current.MainPage.DisplayAlert(
                            "Sin Conexión",
                            "No se pudieron enviar los tags. Acérquese a un área con Wi-Fi y presione 'Reintentar'.",
                            "Entendido"
                        );
                    });
                }
            }
        }

        private async Task ReintentarEnvioAsync()
        {
            if (epcSet.Count == 0) return;

            await Application.Current.MainPage.DisplayToastAsync("Reintentando envío...", 1000);

            bool exito = await ((App)Application.Current).SendBatchAsync(epcSet.ToList());

            if (exito)
            {
                await Application.Current.MainPage.DisplayToastAsync("¡Lectura enviada con éxito!", 1500);
                Device.BeginInvokeOnMainThread(() => {
                    ListaEpcs.Clear();
                    epcSet.Clear();
                });
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Sigue sin haber conexión al servidor. Revise su red.", "OK");
            }
        }

        public void Cleanup()
        {
            
            MessagingCenter.Unsubscribe<App>(this, "gatillazo");
        }
    }
}