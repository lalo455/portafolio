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
using MiniExcelLibs;
using System.IO;
using System.Text;
using EVIRFID.Views;

namespace EVIRFID.ViewModels
{
	public partial class PersonManagerViewModel : BaseViewModel
	{
        public ObservableRangeCollection<Person> Personas { get; }

        [ObservableProperty]
        public int pagina;
        [ObservableProperty]
        public Person selectedPerson;
        [ObservableProperty]
        public string filtro;

        public PersonManagerViewModel()
		{
            Title = "Gestión de Personal";
            Personas = new ObservableRangeCollection<Person>();
        }

        public async void OnAppearing()
        {
            IsBusy = true;
            SelectedPerson = null;
            await LoadPersonas();
        }

        async Task LoadPersonas()
        {
            Pagina += 1;
            IsBusy = true;
            Personas.Clear();
            try
            {
                var personas = await Repository.GetPersonsAsync();
                Personas.AddRange(personas.Skip(Pagina * 20).Take(20));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AZM:" + ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task ShowPersonDetails()
        {
            if (SelectedPerson == null)
                return;

            var route = $"{nameof(PersonDetailPage)}?{nameof(PersonDetailViewModel.PersonId)}={SelectedPerson.idPersona}";
            await Shell.Current.GoToAsync(route);
        }

        [RelayCommand]
        async Task CopyPerson()
        {
            string textoClipboard = $"{SelectedPerson.nombre}\t{SelectedPerson.adscripcion}\t{SelectedPerson.cuip}\t" +
                $"{SelectedPerson.tipoSangre}\t{SelectedPerson.itemsAsignados}\t{SelectedPerson.fechaCalzadoEntregado}\t" +
                $"{SelectedPerson.fechaCinturonEntregado}\t{SelectedPerson.fechaKepiEntregado}\t{SelectedPerson.fechaPantalonEntregado}\t" +
                $"{SelectedPerson.fechaImpermeableEntregado}\t{SelectedPerson.fechaUltimaAtencion}\t{SelectedPerson.observaciones}";

            await Clipboard.SetTextAsync(textoClipboard);

            await Application.Current.MainPage.DisplayToastAsync("Detalles copiados", 500);
        }

        [RelayCommand]
        async Task LoadFilteredPersons()
        {
            IsBusy = true;
            Personas.Clear();
            try
            {
                string filtroLimpio = Filtro.Replace("  ", " ").Trim();
                var personas = await Repository.GetFilteredPersonsAsync(filtroLimpio.ToUpper());
                await Application.Current.MainPage.DisplayToastAsync("Cargando " + personas.Count() + " resultados", 50);
                Personas.AddRange(personas);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AZM:" + ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async void AddPerson(object obj)
        {
            await Shell.Current.GoToAsync(nameof(NewPersonPage));
        }
    }
}