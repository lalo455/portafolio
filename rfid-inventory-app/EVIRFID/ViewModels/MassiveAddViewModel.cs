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
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using Acr.UserDialogs;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using Xamarin.Essentials;

namespace EVIRFID.ViewModels
{
	public partial class MassiveAddViewModel : BaseViewModel
	{
		public ObservableRangeCollection<InventoryRecord> RegistroInventario { get; set; }

        public ObservableRangeCollection<Item> ListaEpcs { get; set; }

        [ObservableProperty]
		public string descripcionEvento;

        [ObservableProperty]
        public string fechaEvento;

        [ObservableProperty]
        public string nombrePrendaNueva;

        [ObservableProperty]
        public string claveCorporacionNueva;

        [ObservableProperty]
        public string tallaPrendaNueva;

        [ObservableProperty]
        int rfPower = 0;

        [ObservableProperty]
        bool isScanning = false;

        [ObservableProperty]
        bool isWaiting = false;

        [ObservableProperty]
        int cuentaChips = 0;

        public ICommand ScanCommand { get; }
        public ICommand ClearEventCommand { get; }
        public ICommand SetPowerCommand { get; }
        public ICommand ClearEpcListCommand { get; }

        public MassiveAddViewModel()
		{
			Title = "Alta Masiva de Prendas";
            ScanCommand = new AsyncCommand(Scan);
            ClearEpcListCommand = new Command(ClearEpcList);
            SetPowerCommand = new AsyncCommand(SetPower);
            ListaEpcs = new ObservableRangeCollection<Item>();
            rfPower = servicioRFID.GetPower();
        }

        [RelayCommand]
        async Task ExportarInventory()
        {
            if(IsScanning)
                await Scan();

            if(ListaEpcs.Count == 0)
            {
                await Application.Current.MainPage.DisplayToastAsync("No hay prendas que cargar", 4000);
                return;
            }

            if(NombrePrendaNueva is null || TallaPrendaNueva is null || ClaveCorporacionNueva is null)
            {
                await Application.Current.MainPage.DisplayToastAsync("Debe seleccionar Prenda, Talla y Corporación", 4000);
                return;
            }

            UserDialogs.Instance.ShowLoading($"Cargando {CuentaChips} prendas...", MaskType.Clear);
            //Carga en BBDD
            foreach(Item item in ListaEpcs)
            {
                try
                {
                    item.claveCorporacion = ClaveCorporacionNueva;
                    item.prenda = NombrePrendaNueva;
                    item.talla = TallaPrendaNueva;
                    await Repository.AddItemAsync(item);
                }
                catch
                {
                    await Application.Current.MainPage.DisplayToastAsync("Error de carga en Base de Datos", 1000);
                }
            }

            //Fichero de Reporte
            string timeStampEvento = DateTime.Now.ToString("dd_MM_yyyy_HH_mm");
            var sheets = new Dictionary<string, object>
            {
                { $"AltaMasiva_{timeStampEvento}", ListaEpcs }
            };

            var config = new OpenXmlConfiguration()
            {
                FillMergedCells = true,
                TableStyles = TableStyles.Default
            };

            await MiniExcel.SaveAsAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"AltaMasiva_{timeStampEvento}.xlsx"), sheets, true, "CargaPrendas", ExcelType.XLSX, config, true);

            UserDialogs.Instance.HideLoading();

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Seleccione un app para enviar el archivo",
                File = new ShareFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"AltaMasiva_{timeStampEvento}.xlsx"))
            });

            Evento evento = new Evento();
            evento.marcaDeTiempo = timeStampEvento;
            evento.motivo = "CierreAltaMasiva;";
            evento.accion = String.Format("Chips leídos: {0}", CuentaChips);
            await Repository.InsertEventAsync(evento);
        }

        async Task SetPower()
        {
            if (servicioRFID.SetPower(RfPower))
            {
                await Application.Current.MainPage.DisplayToastAsync("Potencia aplicada: " + RfPower, 500);
            }
            else
            {
                await Application.Current.MainPage.DisplayToastAsync("No se pudo aplicar potencia. Reinicie aplicación.", 500);
            }
        }

        void ClearEpcList()
        {
            ListaEpcs.Clear();
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
                            if (ListaEpcs.FirstOrDefault(i => i.epc == res.EPC) == null) //El tag no ha sido leído previamente
                            {
                                Item item = await Repository.GetItemDetailsByEpcAsync(res.EPC);
                                if (item is null && res.EPC.Substring(0, 4) == "544C") //El tag no está en la BBDD y es EVI
                                {
                                    Item desconocido = new Item();
                                    desconocido.epc = res.EPC;
                                    desconocido.prenda = NombrePrendaNueva;
                                    desconocido.sxtid = res.Tid;
                                    desconocido.talla = TallaPrendaNueva;
                                    desconocido.claveCorporacion = ClaveCorporacionNueva;
                                    ListaEpcs.Add(desconocido);
                                    CuentaChips = ListaEpcs.Count;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Tag no encontrado: {0}", ex.ToString());
                    }
                }
            }
            servicioRFID.StopInventory();
        }
    }
}