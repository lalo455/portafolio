using System;
using System.Collections.Generic;
using EVIRFID.ViewModels;
using EVIRFID.Views;
using Xamarin.Forms;

namespace EVIRFID
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
            Routing.RegisterRoute(nameof(PersonsPage), typeof(PersonsPage));

            Routing.RegisterRoute(nameof(UsersPage), typeof(UsersPage));

            Routing.RegisterRoute(nameof(InventoryPage), typeof(InventoryPage));
            Routing.RegisterRoute(nameof(CustomTextsPage), typeof(CustomTextsPage));
            Routing.RegisterRoute(nameof(PersonManagerPage), typeof(PersonManagerPage));
            Routing.RegisterRoute(nameof(PersonDetailPage), typeof(PersonDetailPage));
            Routing.RegisterRoute(nameof(NewPersonPage), typeof(NewPersonPage));
            Routing.RegisterRoute(nameof(CalibrationPage), typeof(CalibrationPage));
          
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}

