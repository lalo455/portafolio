using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using WmsMovil.Services;
using WmsMovil.Views;

namespace WmsMovil.ViewModels
{
    public partial class MenuViewModel : ObservableObject
    {
        private readonly ScannerOrchestrator _scanner;

        [ObservableProperty] private bool _isRfidMode;
        [ObservableProperty] private int _rfPower;

        public MenuViewModel(ScannerOrchestrator scanner)
        {
            _scanner = scanner;

            // Cargamos la configuración global guardada
            IsRfidMode = GlobalSettings.IsRfidMode;
            RfPower = Preferences.Default.Get("LastRfPower", 15);
        }

        partial void OnRfPowerChanged(int value)
        {
            Preferences.Default.Set("LastRfPower", value);
            // Le avisamos al orquestador que cambie la potencia
            _scanner.SetPower(value);
        }

        partial void OnIsRfidModeChanged(bool value)
        {
            // El orquestador hace el trabajo pesado en segundo plano
            _ = _scanner.PrepararHardwareAsync(value);
        }

        [RelayCommand]
        private void AbrirConfiguracion()
        {
            var popup = new ConfiguracionPopup(this);
            App.Current.MainPage!.ShowPopup(popup);
        }
    }
}