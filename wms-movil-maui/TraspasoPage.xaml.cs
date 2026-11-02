using WmsMovil.ViewModels;

namespace WmsMovil
{
    public partial class TraspasoPage : ContentPage
    {
        public TraspasoPage(ViewModels.TraspasoViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        // ESTA FUNCIÓN SOLO VA AQUÍ
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}