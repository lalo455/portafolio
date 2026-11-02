using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using Syncfusion.XForms.PopupLayout;
using EVIRFID.ViewModels;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace EVIRFID.Views
{
    public partial class ByItemPage : ContentPage
    {
        ByItemViewModel _viewModel;

        public string PersonaId { get; set; }

        public ByItemPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new ByItemViewModel();
            int.TryParse(PersonaId, out var numpersona);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
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
            MessagingCenter.Unsubscribe<App, string>((App)Application.Current,"Gatillo");
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

        void ScanBtn_Clicked(System.Object sender, System.EventArgs e)
        {
            if (!(bool)Preferences.Get("LecturaContinua", true))
            {
                ScanBtn.IsChecked = false;
                return;
            }

            SwitchScanLabels();
        }

        void PwrBtn_Clicked(System.Object sender, System.EventArgs e)
        {
            popupLayout.Show();
        }

        void SfButton_Clicked(System.Object sender, System.EventArgs e)
        {
            //Aplicar nueva potencia
        }

        void SelectAllBtn_Clicked(System.Object sender, System.EventArgs e)
        {
            listViewEPC.SelectAll();
        }

        void SelectNoneBtn_Clicked(System.Object sender, System.EventArgs e)
        {
            listViewEPC.SelectedItems.Clear();
        }

        void chkContinuo_StateChanged(System.Object sender, Syncfusion.XForms.Buttons.StateChangedEventArgs e)
        {
            //Preferences.Set("LecturaContinua", (bool)chkContinuo.IsChecked);
        }

        void chkFiltroEvi_StateChanged(System.Object sender, Syncfusion.XForms.Buttons.StateChangedEventArgs e)
        {
            
        }

        void chkTodos_StateChanged(System.Object sender, Syncfusion.XForms.Buttons.StateChangedEventArgs e)
        {
            if(e.IsChecked.HasValue && e.IsChecked.Value)
            {
                chkCalzado.IsChecked = true;
                chkCinturon.IsChecked = true;
                chkKepi.IsChecked = true;
                chkPantalon.IsChecked = true;
                chkImpermeable.IsChecked = true;
                chkGorra.IsChecked = true;
                chkTodos.Text = "Ninguno";
            } else
            {
                chkCalzado.IsChecked = false;
                chkCinturon.IsChecked = false;
                chkKepi.IsChecked = false;
                chkPantalon.IsChecked = false;
                chkImpermeable.IsChecked = false;
                chkGorra.IsChecked = false;
                chkTodos.Text = "Todos";
            }
        }

        void ToolbarItem_Clicked(System.Object sender, System.EventArgs e)
        {
            assignLayout.Show();
        }
    }
}

