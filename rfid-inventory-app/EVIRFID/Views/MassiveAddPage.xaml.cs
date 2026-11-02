using System;
using System.Collections.Generic;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using EVIRFID.ViewModels;

namespace EVIRFID.Views
{	
	public partial class MassiveAddPage : ContentPage
	{
        MassiveAddViewModel _viewModel;

		public MassiveAddPage()
		{
			InitializeComponent ();
            BindingContext = _viewModel = new MassiveAddViewModel();
        }

        void ScanBtn_Clicked(System.Object sender, System.EventArgs e)
        {
            if (!(bool)Preferences.Get("LecturaContinua", true))
            {
                ScanBtn.IsChecked = false;
                return;
            }

            SwitchScanLabels();
        }

        void SwitchScanLabels()
        {
            if (ScanBtn.Text == "Leer")
            {
                ScanBtn.IsChecked = true;
                ScanBtn.Text = "Leyendo...";
            }
            else
            {
                ScanBtn.IsChecked = false;
                ScanBtn.Text = "Leer";
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //chkContinuo.IsChecked = Preferences.Get("LecturaContinua",true);
            //chkFiltroEvi.IsChecked = Preferences.Get("SoloEvi", true);
            MessagingCenter.Subscribe<App>((App)Application.Current, "gatillo", async (sender) => {
                if (!(bool)Preferences.Get("LecturaContinua", true))
                {
                    ScanBtn.IsChecked = false;
                    return;
                }
                SwitchScanLabels();
                _viewModel.ScanCommand.Execute(null);
                await Application.Current.MainPage.DisplayToastAsync("Gatillo", 200);
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<App, string>((App)Application.Current, "Gatillo");
        }

        void PwrBtn_Clicked(System.Object sender, System.EventArgs e)
        {
            popupLayout.Show();
        }

        void Entry_Focused(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        {
        }
    }
}

