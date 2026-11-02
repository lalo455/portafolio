using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using EVIRFID.Models;
using EVIRFID.Views;
using EVIRFID.ViewModels;
using System.Collections.ObjectModel;

namespace EVIRFID.Views
{
    public partial class UsersPage : ContentPage
    {
        UsersViewModel _viewModel;
        

        public UsersPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new UsersViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        public string NumeroUsuario = "Hello";

        void soloActivos_Toggled(System.Object sender, Xamarin.Forms.ToggledEventArgs e)
        {
        }
    }
}
