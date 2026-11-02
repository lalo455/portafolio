//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Windows.Input;
//using CommunityToolkit.Mvvm.ComponentModel;
//using EVIRFID.Models;
//using Xamarin.CommunityToolkit.Extensions;
//using Xamarin.Forms;
//using EVIRFID.Services;

//namespace EVIRFID.ViewModels
//{
//    [QueryProperty(nameof(Filtro), nameof(Filtro))]
//    public partial class NewItemViewModel : BaseViewModel
//    {
//        private string text;
        
//        private string description;

//        [ObservableProperty]
//        string filtro;

//        [ObservableProperty]
//        string prenda;

//        [ObservableProperty]
//        string talla;

//        public NewItemViewModel()
//        {
//            SaveCommand = new Command(OnSave, ValidateSave);
//            CancelCommand = new Command(OnCancel);
//            this.PropertyChanged += 
//                (_, __) => SaveCommand.ChangeCanExecute();
//        }

//        private bool ValidateSave()
//        {
//            return !String.IsNullOrWhiteSpace(Prenda)
//                && !String.IsNullOrWhiteSpace(Talla);
//        }

//        public string Text
//        {
//            get => text;
//            set
//            {
//                SetProperty(ref text, value);
//            } 
//        }

//        public string Description
//        {
//            get => description;
//            set => SetProperty(ref description, value);
//        }

//        public Command SaveCommand { get; }
//        public Command CancelCommand { get; }

//        private async void OnCancel()
//        {
//            await Shell.Current.GoToAsync("..");
//        }

//        private async void OnSave()
//        {
//            Item newItem = new()
//            {
//                prenda = Prenda.ToUpper(),
//                epc = Filtro.ToUpper(),
//                talla = Talla.ToUpper()
//            };

//            await Repository.AddItemAsync(newItem);

//            string timeStampEvento = DateTime.Now.ToString("dd_MM_yyyy_HH_mm");
//            Evento evento = new Evento();
//            evento.marcaDeTiempo = timeStampEvento;
//            evento.motivo = "Nuevo ítem;";
//            evento.accion = String.Format("Se da de alta el chip {0}", Filtro);
//            await Repository.InsertEventAsync(evento);

//            // This will pop the current page off the navigation stack
//            await Shell.Current.GoToAsync("..");
//        }
//    }
//}

