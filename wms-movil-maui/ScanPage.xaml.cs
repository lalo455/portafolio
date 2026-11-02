using WmsMovil.ViewModels;

namespace WmsMovil
{
    public partial class ScanPage : ContentPage
    {
        public ScanPage(ScanViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;

            // Escuchamos cuando el ViewModel cambia de modo
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ScanViewModel.IsQrMode))
                {
                    if (viewModel.IsQrMode)
                    {
                        // Le damos medio segundo a la UI para dibujarse y forzamos el teclado/foco
                        Task.Delay(500).ContinueWith(_ =>
                            MainThread.BeginInvokeOnMainThread(() => QrEntry.Focus()));
                    }
                }
            };
        }
    }
}