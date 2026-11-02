using EVIRFID.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EVIRFID.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalibrationPage : ContentPage
    {
        CalibrationViewModel _viewModel;

        public CalibrationPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new CalibrationViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe<App>(this, "gatillazo", async (_) => { await _viewModel.OnGatillazo(); });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<App>(this, "gatillazo");
            _viewModel.Cleanup();
        }
    }
}
