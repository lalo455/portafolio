using WmsMovil.ViewModels;

namespace WmsMovil
{
    public partial class MainPage : ContentPage
    {
        // MAUI inyectará el LoginViewModel mágicamente aquí
        public MainPage(LoginViewModel viewModel)
        {
            InitializeComponent();

            // Asignamos el cerebro a la pantalla
            BindingContext = viewModel;
        }
    }
}