using System.ComponentModel;
using Xamarin.Forms;
using EVIRFID.ViewModels;

namespace EVIRFID.Views
{
    public partial class PersonDetailPage : ContentPage
    {
        public PersonDetailPage()
        {
            InitializeComponent();
            BindingContext = new PersonDetailViewModel();
        }
    }
}
