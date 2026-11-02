using System;
using System.Collections.Generic;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using EVIRFID.ViewModels;

namespace EVIRFID.Views
{	
	public partial class CustomTextsPage : ContentPage
	{
        CustomTextsViewModel _viewModel;

		public CustomTextsPage()
		{
			InitializeComponent ();
            BindingContext = _viewModel = new CustomTextsViewModel();
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }
    }
}

