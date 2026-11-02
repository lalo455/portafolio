using System;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;
using EVIRFID.ViewModels;

namespace EVIRFID.Views
{
    public partial class InventoryPage : ContentPage
    {
        InventoryViewModel _viewModel;
        bool _isInitializing = false;

        public InventoryPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new InventoryViewModel();

            // Sincronizar UI con la potencia REAL del hardware, no hardcodear en 1
            PowerSlider.Maximum = 30;
            PowerSlider.Minimum = 1;

            // _viewModel.RfPower ya trae el valor real del lector desde su constructor
            PowerSlider.Value = _viewModel.RfPower;
            PowerLabel.Text = $"Potencia: {_viewModel.RfPower}";
            
            RssiSlider.Value = _viewModel.RfRssi;
            RssiLabel.Text = $"Valor: {_viewModel.RfRssi}";
            UpdateRssiUI(_viewModel.RfRssi);

            QValueSlider.Value = _viewModel.RfQValue;
            UpdateQValueUI(_viewModel.RfQValue);
        }

        private void OnPowerButtonClicked(object sender, EventArgs e)
        {
            // Al abrir el popup, nos aseguramos de que el slider muestre la potencia actual
            PowerSlider.Value = _viewModel.RfPower;
            RssiSlider.Value = _viewModel.RfRssi;
            QValueSlider.Value = _viewModel.RfQValue;
            
            _isInitializing = true;
            SessionPicker.SelectedIndex = _viewModel.RfSession;
            UpdateSessionDescription(_viewModel.RfSession);
            _isInitializing = false;
            
            PowerPopup.IsVisible = true;
        }

        private void OnSliderChanged(object sender, ValueChangedEventArgs e)
        {
            int power = (int)Math.Round(e.NewValue);
            PowerLabel.Text = $"Valor: {power} dBm";
            
            if (power <= 10)
                PowerDescLabel.Text = "Alcance corto (ej: mostrador / escritorio)";
            else if (power <= 20)
                PowerDescLabel.Text = "Alcance medio (ej: racks / pasillos)";
            else
                PowerDescLabel.Text = "Alcance máximo (ej: almacén abierto)";
        }
        
        private void OnRssiSliderChanged(object sender, ValueChangedEventArgs e)
        {
            int rssi = (int)Math.Round(e.NewValue);
            RssiLabel.Text = $"Valor: {rssi}";
            UpdateRssiUI(rssi);
        }

        private void UpdateRssiUI(int rssi)
        {
            if (rssi <= 20)
                RssiDescLabel.Text = "Filtro bajo: Lee todo a su alrededor";
            else if (rssi <= 60)
                RssiDescLabel.Text = "Filtro medio: Ignora las más lejanas";
            else
                RssiDescLabel.Text = "Filtro alto: Solo lee las muy cercanas";
        }

        private void OnQValueSliderChanged(object sender, ValueChangedEventArgs e)
        {
            int qValue = (int)Math.Round(e.NewValue);
            UpdateQValueUI(qValue);
        }

        private void UpdateQValueUI(int qValue)
        {
            QValueLabel.Text = $"Valor Q: {qValue}";
            int estimatedTags = (int)Math.Pow(2, qValue);
            QValueDescLabel.Text = $"Recomendado para: ~{estimatedTags} etiquetas simultáneas";
        }

        private void OnSessionPickerChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return;
            
            int selectedSession = SessionPicker.SelectedIndex;
            if(selectedSession < 0) selectedSession = 1;

            UpdateSessionDescription(selectedSession);

            int power = (int)Math.Round(PowerSlider.Value);
            int rssi = (int)Math.Round(RssiSlider.Value);
            int qValue = (int)Math.Round(QValueSlider.Value);
            
            bool result = _viewModel.ApplySettings(power, rssi, qValue, selectedSession);
            if (!result)
            {
                // Fallback o advertencia (opcional)
            }
        }

        private void UpdateSessionDescription(int session)
        {
            if (SessionDescLabel == null) return;
            switch(session)
            {
                case 0:
                    SessionDescLabel.Text = "Lectura continua (S0). Las etiquetas no duermen. Útil para pocas etiquetas.";
                    break;
                case 1:
                    SessionDescLabel.Text = "Inventario estándar (S1). Etiquetas leídas duermen un tiempo. Ideal para la mayoría de los casos.";
                    break;
                case 2:
                    SessionDescLabel.Text = "Inventario denso (S2). Etiquetas leídas duermen más. Bueno para áreas muy pobladas.";
                    break;
                case 3:
                    SessionDescLabel.Text = "Inventario especial (S3). Similar a S2, se usa en casos específicos.";
                    break;
                default:
                    SessionDescLabel.Text = "S1 duerme etiquetas leídas para descubrir nuevas.";
                    break;
            }
        }

        private async void OnApplyPowerClicked(object sender, EventArgs e)
        {
            int newPower = (int)Math.Round(PowerSlider.Value);
            int newRssi = (int)Math.Round(RssiSlider.Value);
            int newQValue = (int)Math.Round(QValueSlider.Value);
            int newSession = SessionPicker.SelectedIndex;

            // Llamamos al método del ViewModel para que hable con el hardware y guarde ajustes
            bool result = _viewModel.ApplySettings(newPower, newRssi, newQValue, newSession);

            if (!result)
            {
                await DisplayAlert("Error", "No se pudo aplicar la configuración al lector", "OK");
                return;
            }

            await Application.Current.MainPage.DisplayToastAsync($"Ajustes guardados", 900);

            PowerPopup.IsVisible = false;
            Console.WriteLine($"[RFID] Nueva potencia aplicada: {newPower}, RSSI: {newRssi}");
        }

        private void OnCancelPowerClicked(object sender, EventArgs e)
        {
            PowerPopup.IsVisible = false;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe<App>(this, "gatillazo", (_) => { _viewModel.OnGatillazo(); });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<App>(this, "gatillazo");
        }
    }
}