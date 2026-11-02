using System;
using System.Collections.Generic;
using EVIRFID.ViewModels;
using Xamarin.Forms;

namespace EVIRFID.Views
{
    public partial class ImportPage : ContentPage
    {
        ImportViewModel _viewModel;

        public ImportPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new ImportViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}

