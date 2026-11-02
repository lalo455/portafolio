using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.XamarinForms.Popup;
using Syncfusion.XForms.PopupLayout;
using EVIRFID.Services;
using EVIRFID.ViewModels;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EVIRFID.Views
{
    public partial class MenuPage : ContentPage
    {
        MenuViewModel _viewModel;
    
        public MenuPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new MenuViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<App>((App)Application.Current, "gatillo");
        }

        void LinkByPerson(System.Object sender, System.EventArgs e)
        {
            //popupLayout.Show();
        }

        //void LoadFromFile(System.Object sender, System.EventArgs e)
        //{
        //    loadfromfilepopup.IsOpen = true;
        //}

        //void DismissPopup(System.Object sender, System.EventArgs e)
        //{
        //    loadfromfilepopup.IsOpen = false;
        //}

        void FreePerson_Clicked(System.Object sender, System.EventArgs e)
        {

        }
     

    }
}