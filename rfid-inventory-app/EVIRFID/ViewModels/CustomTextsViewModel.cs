using System;
using Xamarin.CommunityToolkit.ObjectModel;
using EVIRFID.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.CommunityToolkit.Extensions;
using System.Threading.Tasks;
using System.Diagnostics;
using Com.Rscja.Deviceapi.Entity;
using EVIRFID.Services;
using System.Linq;
using Xamarin.Essentials;
using CommunityToolkit.Mvvm.Input;
using Acr.UserDialogs;
using System.Threading;

namespace EVIRFID.ViewModels
{
	public partial class CustomTextsViewModel : BaseViewModel
	{
		[ObservableProperty]
		string tipo;
        [ObservableProperty]
        string origen;
        [ObservableProperty]
        string proveedor;
        [ObservableProperty]
        string propietario;
        [ObservableProperty]
        string observacion;

        public CustomTextsViewModel()
		{
            Tipo = Preferences.Get("Tipo", "Estatal");
            Origen = Preferences.Get("Origen", "Nacional");
            Proveedor = Preferences.Get("Proveedor", "Confecciones Finas SA");
            Propietario = Preferences.Get("Propietario", "Comisión Estatal de Seguridad de Tlaxcala");
            Observacion = Preferences.Get("NotaDefecto", "Dotación Completa");
        }

        [RelayCommand]
        private async void SaveParams()
        {
            UserDialogs.Instance.ShowLoading("Guardando...", MaskType.Clear);
            Preferences.Set("Tipo", Tipo);
            Preferences.Set("Origen", Origen);
            Preferences.Set("Proveedor", Proveedor);
            Preferences.Set("Propietario", Propietario);
            Preferences.Set("NotaDefecto", Observacion);
            await Task.Delay(1000);
            UserDialogs.Instance.HideLoading();
            
        }
    }
}