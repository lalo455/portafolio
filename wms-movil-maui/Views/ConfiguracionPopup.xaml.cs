using CommunityToolkit.Maui.Views; // Agrega esto

namespace WmsMovil.Views
{
    public partial class ConfiguracionPopup : Popup // Cambia ContentView por Popup
    {
        public ConfiguracionPopup(object viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel; // Le pasamos el cerebro de Traspasos
        }

        private void OnCerrarClicked(object sender, EventArgs e)
        {
            Close(); // Cierra el popup
        }
    }
}