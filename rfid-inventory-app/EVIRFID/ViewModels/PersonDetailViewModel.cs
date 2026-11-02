using System;
using System.Diagnostics;
using System.Threading.Tasks;
using EVIRFID.Models;
using Xamarin.Forms;
using EVIRFID.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.Input;
using Acr.UserDialogs;
using Xamarin.CommunityToolkit.Extensions;

namespace EVIRFID.ViewModels
{
    [QueryProperty(nameof(PersonId), nameof(PersonId))]
    public partial class PersonDetailViewModel : BaseViewModel
    {

        [ObservableProperty]
        Person currentPerson;

        [ObservableProperty]
        string cveCorp;

        private int personId;

        public int PersonId
        {
            get
            {
                return personId;
            }
            set
            {
                personId = value;
                LoadPersonId(value);
            }
        }

        public async void LoadPersonId(int itemId)
        {
            try
            {
                CurrentPerson = await Repository.GetPersonById(PersonId);
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to Load Person");
            }
        }

        [RelayCommand]
        async Task UpdatePerson()
        {
            var result = await UserDialogs.Instance.ConfirmAsync("¿Seguro que desea actualizar?", "ATENCIÓN", "SI", "NO");
            if (result)
            {
                await Repository.UpdatePersonAsync(CurrentPerson);
                string timeStampEvento = DateTime.Now.ToString("dd_MM_yyyy_HH_mm");
                Evento evento = new Evento();
                evento.marcaDeTiempo = timeStampEvento;
                evento.motivo = "Modificación de Persona;";
                evento.accion = String.Format("Se modifica la información atribuída a la persona", CurrentPerson.idPersona);
                await Repository.InsertEventAsync(evento);
                await Application.Current.MainPage.DisplayToastAsync("Se actualizó el detalle del elemento.", 1000);
            }
        }
    }
}

