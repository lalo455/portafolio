using Microsoft.Maui.Controls;
using System;
using WmsMovil.ViewModels;

namespace WmsMovil
{
    public partial class MenuPage : ContentPage
    {
        public MenuPage(MenuViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private async void OnTraspasosTapped(object sender, TappedEventArgs e)
        {
            var traspasoPage = Application.Current.MainPage.Handler.MauiContext.Services.GetService<TraspasoPage>();
            await Navigation.PushAsync(traspasoPage);
        }

        // ESTA ES LA FUNCIÓN QUE BUSCA EL XAML
        private void OnLogoutClicked(object sender, EventArgs e)
        {
            var loginPage = Application.Current.MainPage.Handler.MauiContext.Services.GetService<MainPage>();
            Application.Current.MainPage = new NavigationPage(loginPage);
        }
    }
}