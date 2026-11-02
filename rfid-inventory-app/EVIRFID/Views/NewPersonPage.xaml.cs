using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using EVIRFID.Models;
using EVIRFID.ViewModels;

namespace EVIRFID.Views
{
    public partial class NewPersonPage : ContentPage
    {
        public Person Person { get; set; }

        public NewPersonPage()
        {
            InitializeComponent();
            BindingContext = new NewPersonViewModel();
        }
    }
}
