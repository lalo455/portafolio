using System;
using System.Collections.Generic;
using DevExpress.Core;
using EVIRFID.ViewModels;
using Xamarin.Forms;

namespace EVIRFID.Views
{	
	public partial class PersonsPage : ContentPage
	{
		PersonsViewModel _viewModel;

        public PersonsPage ()
		{
			InitializeComponent ();
			BindingContext = _viewModel = new PersonsViewModel();
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        void TextEdit_Completed(System.Object sender, System.EventArgs e)
        {
            _viewModel.LoadFilteredPersonsCommand.Execute(null);
        }
    }
}

