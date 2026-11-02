using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevExpress.XamarinForms.Core.Internal;
using EVIRFID.Models;
using EVIRFID.Services;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.CommunityToolkit.Extensions;
using EVIRFID.Views;
using Xamarin.Essentials;
using Acr.UserDialogs;

namespace EVIRFID.ViewModels
{
    [QueryProperty(nameof(Title), nameof(Title))]
    public partial class PersonsViewModel : BaseViewModel
    {
		private Person _selectedPerson;

		public ObservableRangeCollection<Person> Personas { get; }

		public ICommand LoadPersonsCommand { get; }
        public ICommand LoadFilteredPersonsCommand { get; }
        public ICommand ViewPersonDetails { get; }
        public ICommand AddTagsToPersonCommand { get; }

        public PersonsViewModel()
		{
			Title = "Busca";
			Personas = new ObservableRangeCollection<Person>();
			LoadPersonsCommand = new AsyncCommand(LoadPersons);
            LoadFilteredPersonsCommand = new AsyncCommand(LoadFilteredPersons);
            AddTagsToPersonCommand = new Command(AddTagsToPerson);
        }

		[ObservableProperty]
		public Person selectedPerson;

        [ObservableProperty]
        public int page;

        [ObservableProperty]
        public bool isRefreshing = false;

		[ObservableProperty]
		public ImageSource iconobuscar = "info.png";

        [ObservableProperty]
        public string filtro;

        [ObservableProperty]
        public int cuentaPersonal;

        public async void AddTagsToPerson()
        {
            bool ok = FijarElemento();
            if (ok)
            {
                UserDialogs.Instance.ShowLoading("Cargando...", MaskType.Clear);
           
           
                UserDialogs.Instance.HideLoading();
            }
        }

        public bool FijarElemento()
        {
            if(SelectedPerson != null)
            {
                DataStore.SetCurrentPerson(SelectedPerson);
                CurrentPerson = IgualarPersonas(DataStore.GetCurrentPerson());
                return true;
            } else
            {
                Application.Current.MainPage.DisplayToastAsync("Seleccione a un elemento para fijarlo", 1000);
                return false;
            }
        }


        async Task LoadPersons()
		{
			page += 1;
			Debug.WriteLine(">>>>>" + page);
			IsBusy = true;
			Personas.Clear();
			try
			{
				//Llenar Personas
				var personas = await Repository.GetPersonsAsync();
				Personas.AddRange(personas.Skip(page * 20).Take(20));
                isRefreshing = false;
            } catch (Exception ex)
			{
                Debug.WriteLine("AZM:" + ex);
            }
			finally
			{
				IsBusy = false;
			}
		}

		async Task LoadFilteredPersons()
		{
            IsBusy = true;
            Personas.Clear();
            try
            {
                string filtroLimpio = Filtro.Replace("  ", " ").Trim();
                var personas = await Repository.GetFilteredPersonsAsync(filtroLimpio.ToUpper());
                await Application.Current.MainPage.DisplayToastAsync("Cargando " + personas.Count() + " resultados", 200);
                Personas.AddRange(personas);
                isRefreshing = false;
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

        public async void OnAppearing()
        {
            IsBusy = true;
            selectedPerson = null;
			await LoadPersons();
            CurrentPerson = DataStore.GetCurrentPerson();
        }

        [RelayCommand]
        async Task NuevoElemento()
        {
            await Shell.Current.GoToAsync(nameof(NewPersonPage));
        }
    }
}

