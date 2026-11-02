using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using EVIRFID.Services;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Xamarin.CommunityToolkit.Extensions;
using System.Collections.Generic;
using EVIRFID.Models;
using System.Diagnostics;
using System.Linq;
using Xamarin.Essentials;
using System.Threading;
using System.Text;
using Com.Rscja.Deviceapi.Entity;
using Syncfusion.ListView.XForms.Helpers;
using Acr.UserDialogs;

namespace EVIRFID.ViewModels
{
    [QueryProperty(nameof(Title), nameof(Title))]
    public partial class ByItemViewModel : BaseViewModel
	{
		
		//public string PersonId { get; set; }
		public ICommand StartInventoryCommand { get; }
        public ICommand ContinuousReadCommand { get; }
		public ICommand ScanCommand { get; }
        public ICommand ClearEpcListCommand { get; }
        public ICommand SetPowerCommand { get; }
        public ICommand DiscardSelectedCommand { get; }
        public ICommand AssignItemsCommand { get; }

        [ObservableProperty]
		int personId;

        [ObservableProperty]
		bool isScanning = false;

        [ObservableProperty]
        bool isWaiting = false;

        [ObservableProperty]
		int rfPower = 0;

        [ObservableProperty]
        string observaciones;

        [ObservableProperty]
        string cuip;

        [ObservableProperty]
        string tipoSangre;

        public ObservableRangeCollection<Item> epcSeleccionados { get; set; }

        public ObservableRangeCollection<Item> ListaEpcs { get; set; }

		//TODO: detener scan cuando salgo de esta actividad

        public ByItemViewModel()
		{
			StartInventoryCommand = new Command(StartInventory);
			ScanCommand = new AsyncCommand(Scan);
			ClearEpcListCommand = new Command(ClearEpcList);
            DiscardSelectedCommand = new Command(DiscardSelected);
            SetPowerCommand = new AsyncCommand(SetPower);
            AssignItemsCommand = new AsyncCommand(AssignItems);
            ListaEpcs = new ObservableRangeCollection<Item>();
            epcSeleccionados = new ObservableRangeCollection<Item>();
            rfPower = servicioRFID.GetPower();
            CurrentPerson = IgualarPersonas(DataStore.GetCurrentPerson());
        }

        async Task AssignItems()
        {
            string timeStampAsignacion = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            foreach (var item in ListaEpcs)
            {
                if(item.idPersona != CurrentPerson.idPersona && item.idPersona > 0)
                {
                    var result = await UserDialogs.Instance.ConfirmAsync("¿Desea re asignar items de otra persona?", "ATENCIÓN", "SI", "NO");
                    if (!result)
                    {
                        return;
                    } else
                    {
                        Evento evento = new Evento();
                        evento.marcaDeTiempo = timeStampAsignacion;
                        evento.motivo = "Reasignación de items;";
                        evento.accion = String.Format("Chip {0} cambia de persona {1} a persona {2}", item.epc, item.idPersona, CurrentPerson.idPersona);
                        await Repository.InsertEventAsync(evento);
                    }
                }
            }

            if(CurrentPerson.fechaCalzadoEntregado is null && CurrentPerson.calzadoEntregado)
            {
                CurrentPerson.fechaCalzadoEntregado = timeStampAsignacion;
            }

            if (CurrentPerson.fechaCinturonEntregado is null && CurrentPerson.cinturonEntregado)
            {
                CurrentPerson.fechaCinturonEntregado = timeStampAsignacion;
            }

            if (CurrentPerson.fechaPantalonEntregado is null && CurrentPerson.pantalonEntregado)
            {
                CurrentPerson.fechaPantalonEntregado = timeStampAsignacion;
            }

            if (CurrentPerson.fechaKepiEntregado is null && CurrentPerson.kepiEntregado)
            {
                CurrentPerson.fechaKepiEntregado = timeStampAsignacion;
            }

            if (CurrentPerson.fechaImpermeableEntregado is null && CurrentPerson.impermeableEntregado)
            {
                CurrentPerson.fechaImpermeableEntregado = timeStampAsignacion;
            }

            if (CurrentPerson.fechaGorraEntregado is null && CurrentPerson.gorraEntregado)
            {
                CurrentPerson.fechaGorraEntregado = timeStampAsignacion;
            }

            CurrentPerson.fechaUltimaAtencion = timeStampAsignacion;

            CurrentPerson.itemsAsignados = ListaEpcs.Count;

            if (Observaciones is null)
                Observaciones = Preferences.Get("NotaDefecto","");

            //CurrentPerson.observaciones = Observaciones.ToUpper();

            //CurrentPerson.cuip = Cuip.ToUpper();

            //CurrentPerson.tipoSangre = TipoSangre.ToUpper();

            await Repository.UpdatePersonAsync(CurrentPerson);

            StringBuilder clipboardText = new StringBuilder(String.Format("{0}\t{1}\t{2}\t{3}", CurrentPerson.nombre, CurrentPerson.cuip, CurrentPerson.tipoSangre, CurrentPerson.adscripcion));

            int index = 0;

            foreach (Item item in ListaEpcs)
            {
                item.idPersona = CurrentPerson.idPersona;
                item.nombre = CurrentPerson.nombre;
                if(item.fechaAsignacion is null)
                    item.fechaAsignacion = timeStampAsignacion;
                await Repository.UpdateItemOwner(item);
                String linea;
                if(index == 0)
                {
                    //if (AutoNamePrenda && item.prenda == "CHIP DESCONOCIDO")
                    //{
                    //    item.prenda = "CHAMARRA";
                    //    item.talla = "STD";
                    //}
                    linea =
                    String.Format("\t{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}",
                    item.prenda, item.talla, item.epc, timeStampAsignacion, Preferences.Get("Tipo", ""), Preferences.Get("Origen", ""),
                    Preferences.Get("Proveedor", ""), Preferences.Get("Propietario", ""), Observaciones);
                }
                else
                {
                    //if (AutoNamePrenda && index == 1 && item.prenda == "CHIP DESCONOCIDO")
                    //{
                    //    item.prenda = "CAMISOLA";
                    //    item.talla = "STD";
                    //}
                    linea =
                    String.Format("\t\t\t\t{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}",
                    item.prenda, item.talla, item.epc, timeStampAsignacion, Preferences.Get("Tipo", ""), Preferences.Get("Origen", ""),
                    Preferences.Get("Proveedor", ""), Preferences.Get("Propietario", ""), Observaciones);
                }

                if(item.talla == "STD")
                {
                    await Repository.AddItemAsync(item);
                }

                Evento evento = new Evento();
                evento.marcaDeTiempo = timeStampAsignacion;
                evento.motivo = "Asignación de item;";
                evento.accion = String.Format("Chip {0} se asigna a persona {1}", item.epc, CurrentPerson.idPersona);
                await Repository.InsertEventAsync(evento);

                clipboardText.AppendLine(linea);
                index++;
            }
            await Application.Current.MainPage.DisplayToastAsync(String.Format("Ítems asignados {0}",ListaEpcs.Count), 500);
            await Application.Current.MainPage.DisplayToastAsync("Datos copiados al portapapeles", 500);

            await Clipboard.SetTextAsync(clipboardText.ToString());
        }

        async Task SetPower()
		{
			if (servicioRFID.SetPower(RfPower))
			{
				Debug.WriteLine("Potencia nueva ok");
                await Application.Current.MainPage.DisplayToastAsync("Potencia aplicada: " + RfPower, 500);
            } else
			{
                Debug.WriteLine("No se pudo aplicar potencia");
                await Application.Current.MainPage.DisplayToastAsync("No se pudo aplicar potencia. Reinicie aplicación.", 500);
            }
			
        }

        async void DiscardSelected(object obj)
        {
            var eventArgs = obj as Syncfusion.ListView.XForms.ItemTappedEventArgs;
            var badItem = eventArgs.ItemData as Item;
            if(CurrentPerson.idPersona == badItem.idPersona)
            {
                var result = await UserDialogs.Instance.ConfirmAsync("¿Seguro que desea borrar esta asignación?","ATENCIÓN", "SI", "NO");
                if (result)
                {
                    badItem.idPersona = 0;
                    badItem.nombre = null;
                    var res = await Repository.UpdateItemOwner(badItem);
                    ListaEpcs.Remove(badItem);
                }
                else
                {
                    return;
                }
            }
            else if(badItem.prenda == "CHIP DESCONOCIDO")
            {
                var result = await UserDialogs.Instance.ConfirmAsync("¿Desea agregar este ítem a la Base de Datos?", "ATENCIÓN", "SI", "NO");
                if (result)
                {

                    badItem.idPersona = 0;
                    badItem.nombre = null;
                    var res = await Repository.UpdateItemOwner(badItem);
                    ListaEpcs.Remove(badItem);
                    return;
                }
                ListaEpcs.Remove(badItem);
            }
            ListaEpcs.Remove(badItem);
        }

        void ClearEpcList()
		{
			ListaEpcs.Clear();
            OnAppearing();
		}

        async Task Scan()
		{
			IsScanning = !IsScanning;

            if (!IsScanning)
            {
                return;
            }

            bool tid = await Task.Run(() => servicioRFID.setEPCandTIDMode());

            bool inicia = await Task.Run(() => servicioRFID.StartInventoryTag(0, 2, 1));

            Debug.WriteLine("Lectura?{0}", inicia);

            if (inicia)
            {
                while (IsScanning)
                {
                    try
                    {
                        UHFTAGInfo res = await servicioRFID.ReadTagFromBufferAsync();
                        if (res is not null)
                        {
                            IsWaiting = false;
                            if (ListaEpcs.FirstOrDefault(i => i.epc == res.EPC) == null)
                            {
                                Item item = await Repository.GetItemDetailsByEpcAsync(res.EPC);
                                if(item is null && res.EPC.Substring(0,4) == "544C")
                                {
                                    Item desconocido = new Item();
                                    desconocido.epc = res.EPC;
                                    desconocido.prenda = "CHIP DESCONOCIDO";
                                    desconocido.sxtid = res.Tid;
                                    desconocido.talla = "STD";
                                    if(AutoNamePrenda && !ListaEpcs.Any(i => i.talla == "STD"))
                                    {
                                        desconocido.prenda = "CHAMARRA";
                                        if (!string.IsNullOrEmpty(CurrentPerson.adscripcion))
                                        {
                                            // Since adscripcion is not null or empty, proceed with assignment
                                            desconocido.claveCorporacion = CurrentPerson.adscripcion.Length <= 12 ?
                                                                           CurrentPerson.adscripcion :
                                                                           CurrentPerson.adscripcion.Substring(0, 12);
                                        }
                                    }
                                    if(AutoNamePrenda && ListaEpcs.Count(i => i.talla == "STD") == 1)
                                    {
                                        desconocido.prenda = "CAMISOLA";
                                        if (!string.IsNullOrEmpty(CurrentPerson.adscripcion))
                                        {
                                            // Since adscripcion is not null or empty, proceed with assignment
                                            desconocido.claveCorporacion = CurrentPerson.adscripcion.Length <= 12 ?
                                                                           CurrentPerson.adscripcion :
                                                                           CurrentPerson.adscripcion.Substring(0, 12);
                                        }
                                    }
                                    ListaEpcs.Add(desconocido);
                                }
                                else if(item is not null)
                                {
                                    if (item.sxtid is null)
                                    {
                                        item.sxtid = res.Tid;
                                        await Repository.UpdateItemOwner(item);
                                    }
                                    ListaEpcs.Add(item);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Tag no encontrado: {0}",ex.ToString());
                    }
                }
            }
            
            servicioRFID.StopInventory();
        }

		void StartInventory()
		{
			var res = servicioRFID.StartInventoryTag(1,3, 1);
			if (res)
				Console.WriteLine(">>>> Start ok");
        }

        public async void OnAppearing()
		{
            var itemsActuales = await Repository.GetAssignedItemsAsync(CurrentPerson.idPersona);
            ListaEpcs.AddRange(itemsActuales);
        }
	}
}

