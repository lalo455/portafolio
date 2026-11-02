using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace EVIRFID.ViewModels
{
    public partial class AboutViewModel : BaseViewModel
    {
        [ObservableProperty]
        string version = "Versión:" + VersionTracking.CurrentVersion;

        public AboutViewModel()
        {
            Title = "Acerca de EVIRFIDApp";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://guiarfid.com/politica-de-privacidad"));
        }

        public ICommand OpenWebCommand { get; }

    }
}
