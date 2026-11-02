using Com.Rscja.Deviceapi.Entity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EVIRFID.Models;
using EVIRFID.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace EVIRFID.ViewModels
{
    public partial class CalibrationViewModel : BaseViewModel
    {
        public ObservableRangeCollection<CalibrationTag> ListaEpcs { get; set; }

        private CancellationTokenSource cts;
        private readonly HashSet<string> epcSet = new HashSet<string>();
        private DateTime _horaInicioEscaneo;

        [ObservableProperty]
        bool isScanning = false;

        [ObservableProperty]
        int currentRfPower = 15;

        [ObservableProperty]
        int currentRfRssi = 50;

        [ObservableProperty]
        int currentRfQValue = 2;

        [ObservableProperty]
        int currentRfSession = 1;

        public ICommand ScanCommand { get; }
        public ICommand ClearListCommand { get; }
        public ICommand CloseCommand { get; }

        public CalibrationViewModel()
        {
            Title = "Calibración y Pruebas";
            ListaEpcs = new ObservableRangeCollection<CalibrationTag>();
            
            ScanCommand = new AsyncCommand(OnGatillazo);
            ClearListCommand = new Command(ClearList);
            CloseCommand = new AsyncCommand(CloseAsync);

            // Cargar configuración de potencia y RSSI
            CurrentRfPower = Preferences.Get("LastRfPower", 15);
            CurrentRfRssi = Preferences.Get("LastRfRssi", 50);
            CurrentRfQValue = Preferences.Get("LastRfQValue", 2);
            CurrentRfSession = Preferences.Get("LastRfSession", 1);
        }

        public async Task OnGatillazo()
        {
            if (IsScanning)
                await StopScanAsync();
            else
                await StartScanAsync();
        }

        private async Task CloseAsync()
        {
            if (Application.Current.MainPage != null)
            {
                await Application.Current.MainPage.Navigation.PopModalAsync();
            }
        }

        private void ClearList()
        {
            ListaEpcs.Clear();
            epcSet.Clear();
        }

        private string HexToString(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return "";
            try
            {
                byte[] bytes = new byte[hex.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }
                // Filtramos caracteres no imprimibles para que no se vea feo
                return new string(Encoding.ASCII.GetString(bytes).Where(c => !char.IsControl(c)).ToArray());
            }
            catch
            {
                return "Inválido";
            }
        }

        private async Task StartScanAsync()
        {
            if (IsScanning) return;

            IsScanning = true;
            _horaInicioEscaneo = DateTime.Now;

            DependencyService.Get<IAudioService>()?.PlayBeep(isNewTag: false);

            epcSet.Clear();
            ListaEpcs.Clear();
            cts = new CancellationTokenSource();

            bool powerOk = await Task.Run(() => servicioRFID.SetPower(CurrentRfPower));
            if (!powerOk)
            {
                IsScanning = false;
                return;
            }

            await Task.Run(() => servicioRFID.setEPCandTIDMode());
            await Task.Run(() => servicioRFID.StopInventory());
            await Task.Delay(200);

            bool started = await Task.Run(() => servicioRFID.StartInventoryTag(1, CurrentRfQValue, CurrentRfSession));
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
                            if (epcSet.Add(res.EPC))
                            {
                                DependencyService.Get<IAudioService>()?.PlayBeep(isNewTag: true);
                                int rssiValue = 0;
                                if (float.TryParse(res.Rssi, out float rawRssi))
                                {
                                    // Mapeo: -100 a 0 -> 0 a 100
                                    rssiValue = (int)Math.Max(0, 100 + rawRssi);
                                }

                                bool inRange = rssiValue >= CurrentRfRssi;
                                string translated = HexToString(res.EPC);

                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    ListaEpcs.Add(new CalibrationTag 
                                    { 
                                        EpcHex = res.EPC, 
                                        TranslatedValue = translated,
                                        RssiValue = rssiValue,
                                        IsInRange = inRange
                                    });
                                });
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

            var tiempoTranscurrido = (DateTime.Now - _horaInicioEscaneo).TotalSeconds;
            if (tiempoTranscurrido < 1.0)
            {
                await Task.Delay(TimeSpan.FromSeconds(4.0 - tiempoTranscurrido));
            }

            IsScanning = false;
            cts?.Cancel();
            DependencyService.Get<IAudioService>()?.PlayBeep(isNewTag: false);
            await Task.Run(() => { try { servicioRFID.StopInventory(); } catch { } });
        }

        public void Cleanup()
        {
            MessagingCenter.Unsubscribe<App>(this, "gatillazo");
        }
    }
}
