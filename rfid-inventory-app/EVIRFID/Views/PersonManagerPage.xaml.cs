using System;
using System.Collections.Generic;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using EVIRFID.ViewModels;

namespace EVIRFID.Views
{	
	public partial class PersonManagerPage : ContentPage
	{
        PersonManagerViewModel _viewModel;

		public PersonManagerPage()
		{
			InitializeComponent ();
            BindingContext = _viewModel = new PersonManagerViewModel();
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        void TextEdit_Completed(System.Object sender, System.EventArgs e)
        {
            _viewModel.LoadFilteredPersonsCommand.Execute(null);
        }

        void filterText_TextChanged(System.Object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
        }
    }
}

