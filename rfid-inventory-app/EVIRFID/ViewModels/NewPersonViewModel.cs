using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using EVIRFID.Models;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;
using EVIRFID.Services;

namespace EVIRFID.ViewModels
{
    [QueryProperty(nameof(Filtro), nameof(Filtro))]
    public partial class NewPersonViewModel : BaseViewModel
    {
        private string text;
        
        private string description;

        [ObservableProperty]
        string filtro;

        [ObservableProperty]
        string nombre;

        [ObservableProperty]
        string adscripcion;

        [ObservableProperty]
        string cuip;

        [ObservableProperty]
        string sangre;

        public NewPersonViewModel()
        {
            SaveCommand = new Command(OnSave, ValidateSave);
            CancelCommand = new Command(OnCancel);
            this.PropertyChanged += 
                (_, __) => SaveCommand.ChangeCanExecute();
        }

        private bool ValidateSave()
        {
            return !String.IsNullOrWhiteSpace(Nombre)
                && !String.IsNullOrWhiteSpace(Cuip)
                && !String.IsNullOrWhiteSpace(Adscripcion)
                && !String.IsNullOrWhiteSpace(Sangre);
        }

        public string Text
        {
            get => text;
            set
            {
                SetProperty(ref text, value);
            } 
        }

        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        public Command SaveCommand { get; }
        public Command CancelCommand { get; }

        private async void OnCancel()
        {
            await Shell.Current.GoToAsync("..");
        }

        private async void OnSave()
        {
            Person newPerson = new()
            {
                nombre = Nombre.ToUpper(),
                adscripcion = Adscripcion.ToUpper(),
                cuip = Cuip.ToUpper(),
                tipoSangre = Sangre.ToUpper()
            };

            await Repository.AddPersonAsync(newPerson);

            string timeStampEvento = DateTime.Now.ToString("dd_MM_yyyy_HH_mm");
            Evento evento = new Evento();
            evento.marcaDeTiempo = timeStampEvento;
            evento.motivo = "Nueva Persona;";
            evento.accion = String.Format("Se da de alta la persona con CUIP {0}", Cuip);
            await Repository.InsertEventAsync(evento);

            // This will pop the current page off the navigation stack
            await Shell.Current.GoToAsync("..");
        }
    }
}

