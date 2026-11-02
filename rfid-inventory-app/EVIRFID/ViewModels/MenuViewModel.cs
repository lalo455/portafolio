using System;
using System.Windows.Input;
using EVIRFID.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.CommunityToolkit.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using MiniExcelLibs;
using EVIRFID.Models;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;
using System.Threading;
using System.IO;
using Acr.UserDialogs;
using MiniExcelLibs.OpenXml;
using EVIRFID.Services;
using CommunityToolkit.Mvvm.Input;
using Xamarin.CommunityToolkit.Extensions;
using Com.Rscja.Deviceapi.Entity;

namespace EVIRFID.ViewModels
{
    public partial class MenuViewModel : BaseViewModel
    {
        public ICommand GetPowerCommand { get; }
        public ICommand PickPersonsFile { get; }
        public ICommand ExportExcelCommand { get; }
        public ICommand AddByItemCommand { get; }
        public ICommand AddByPersonCommand { get; }
        public ICommand ReleaseCurrentPersonCommand { get; }
        public ObservableRangeCollection<Tag> ListaEpcs { get; set; }
        //epcList guarda los tags leidos
        //public ObservableRangeCollection<string> ListaEpcs { get; set; }
        public List<string> epcList = new List<string>();

        [ObservableProperty]
        Person mostrarElemento;

        [ObservableProperty]
        int cuentaItems = 0;

        [ObservableProperty]
        int cuentaItemsHoy = 0;

        [ObservableProperty]
        int cuentaItemsAsignados = 0;

        [ObservableProperty]
        int cuentaPersonasAtendidas = 0;

        [ObservableProperty]
        int cuentaPersonas = 0;

        [ObservableProperty]
        int cuentaPersonasHoy = 0;

        [ObservableProperty]
        bool isScanning = false;

        [ObservableProperty]
        bool isWaiting = false;

        [ObservableProperty]
        string version = "Versión:" + VersionTracking.CurrentVersion;

        public MenuViewModel()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            Title = "Bienvenido - " + DateTime.Now.ToShortDateString();
            GetPowerCommand = new Command(getPower);
            PickPersonsFile = new AsyncCommand(CargarPersonas);
            AddByItemCommand = new AsyncCommand(AddByItem);
            AddByPersonCommand = new AsyncCommand(AddByPerson);
            ExportExcelCommand = new AsyncCommand(ExportExcel);
            ReleaseCurrentPersonCommand = new Command(LiberarPersona);
            AutoNamePrenda = true;
            ListaEpcs = new ObservableRangeCollection<Tag>();
           

            epcList = new List<string>();
            //MessagingCenter.Subscribe<App>((App)Application.Current, "ReaderReady", (sender) =>
            //{
            //    Potencia = servicioRFID.GetPower();
            //    MessagingCenter.Unsubscribe<App>((App)Application.Current, "ReaderReady");
            //});
        }

        public async void getTotalitems()
        {
            CuentaItems = await Repository.GetTotalItems();
            CuentaItemsHoy = await Repository.GetTodayItems();
            CuentaItemsAsignados = await Repository.GetAssignedItemsCount();
            CuentaPersonas = await Repository.GetTotalPersons();
            CuentaPersonasAtendidas = await Repository.GetAttendedPersons();
            CuentaPersonasHoy = await Repository.GetTodayPersons();
        }

        public void getPower()
        {
            Potencia = servicioRFID.GetPower();
        }

        public async Task ExportExcel()
        {
            UserDialogs.Instance.ShowLoading("Exportando datos...", MaskType.Clear);
            string timeStampEvento = DateTime.Now.ToString("dd_MM_yyyy_HH_mm");
            var personas = await Repository.GetPersonsAsync();
            var items = await Repository.GetItemsAsync();
            var sheets = new Dictionary<string, object>();
            sheets.Add("Elementos", personas);
            sheets.Add("items", items);

            var config = new OpenXmlConfiguration()
            {
                FillMergedCells = true,
                TableStyles = TableStyles.Default
            };

            await MiniExcel.SaveAsAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"BD_Uniformes_{timeStampEvento}.xlsx"), sheets, true, "RFID", ExcelType.XLSX, config, true);

            UserDialogs.Instance.HideLoading();

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Seleccione un app para enviar el archivo",
                File = new ShareFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"BD_Uniformes_{timeStampEvento}.xlsx"))
            });
        }

        [RelayCommand]
        public async Task ExportExcelEventos()
        {
            UserDialogs.Instance.ShowLoading("Exportando eventos...", MaskType.Clear);
            string timeStampEvento = DateTime.Now.ToString("dd_MM_yyyy_HH_mm");
            var eventos = await Repository.GetEventosAsync();
            var sheets = new Dictionary<string, object>();
            sheets.Add("Eventos", eventos);

            var config = new OpenXmlConfiguration()
            {
                FillMergedCells = true,
                TableStyles = TableStyles.Default
            };

            await MiniExcel.SaveAsAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"Eventos_{timeStampEvento}.xlsx"), sheets, true, "RFID", ExcelType.XLSX, config, true);

            UserDialogs.Instance.HideLoading();

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Seleccione un app para enviar el archivo",
                File = new ShareFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"Eventos_{timeStampEvento}.xlsx"))
            });
        }

        [RelayCommand]
        public async Task ExportReport()
        {
            UserDialogs.Instance.ShowLoading("Exportando reporte...", MaskType.Clear);
            string timeStampEvento = DateTime.Now.ToString("dd_MM_yyyy_HH_mm");
            var reporte = await Repository.GetReport();
            reporte.Select(c => { c.TIPO = Preferences.Get("Tipo", ""); return c;}).ToList();
            reporte.Select(c => { c.ORIGEN = Preferences.Get("Origen", ""); return c; }).ToList();
            reporte.Select(c => { c.PROVEEDOR = Preferences.Get("Proveedor", ""); return c; }).ToList();
            reporte.Select(c => { c.PROPIETARIO = Preferences.Get("Propietario", ""); return c; }).ToList();
            var sheets = new Dictionary<string, object>();
            sheets.Add("Reporte", reporte);

            var config = new OpenXmlConfiguration()
            {
                FillMergedCells = true,
                TableStyles = TableStyles.Default
            };

            await MiniExcel.SaveAsAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"EntregaUniformes_{timeStampEvento}.xlsx"), sheets, true, "RFID", ExcelType.XLSX, config, true);

            UserDialogs.Instance.HideLoading();

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Seleccione un app para enviar el archivo",
                File = new ShareFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"EntregaUniformes_{timeStampEvento}.xlsx"))
            });
        }

        public void LiberarPersona()
        {
            DataStore.ReleaseCurrentPerson();
            CurrentPerson = IgualarPersonas(DataStore.GetCurrentPerson());
        }

        public void OnAppearing()
        {
            Title = "Bienvenido - " + DateTime.Now.ToString("d MMMM yyyy");
            CurrentPerson = IgualarPersonas(DataStore.GetCurrentPerson());
            getTotalitems();

            MessagingCenter.Subscribe<App>((App)Application.Current,"ReaderReady", (sender) =>
            {
                Potencia = servicioRFID.GetPower();
                MessagingCenter.Unsubscribe<App>((App)Application.Current, "ReaderReady");
            });

            MessagingCenter.Subscribe<App>((App)Application.Current, "gatillo", async (sender) =>
            {
                try
                {
                    isScanning = !isScanning;

                    if (!isScanning)
                    {
                        await Application.Current.MainPage.DisplayToastAsync($"Termino lectura", 900);
                      
                        ListaEpcs.Clear();

                        epcList.Clear();
                        return;
                    }
                    else {
                        await Application.Current.MainPage.DisplayToastAsync($"Empezo la  lectura", 900);

                    };
                    
                    bool tid = await Task.Run(() => servicioRFID.setEPCandTIDMode());
                    //filtro para quue sean tag solo de la empresa 
                    //bool filtroOk = await Task.Run(() => servicioRFID.SetFilter(2,1,"5440"));

                    bool inicia = await Task.Run(() => servicioRFID.StartInventoryTag(1, 2, 1));

                    Debug.WriteLine("Lectura?{0}", inicia);

                    if (inicia)
                    {
                        while (isScanning)
                        {
                            try
                            {
                                UHFTAGInfo res = await servicioRFID.ReadTagFromBufferAsync();
                                if (res is not null)
                                {
                                    var cuenta = res.Ant;
                                    isWaiting = false;


                                    //si existe en base de datos



                                    //filtro para quue sean tag solo de la empresa 
                                    //if (item is null && res.EPC.Substring(0, 4) == "544C")
                                    //{
                                    //    Item desconocido = new Item();
                                    //    desconocido.epc = res.EPC;
                                    //    desconocido.prenda = "CHIP DESCONOCIDO";
                                    //    desconocido.sxtid = res.Tid;
                                    //    ListaEpcs.Add(desconocido);   
                                    //}
                                    if (res is not null)
                                    {
                                        //if(item.sxtid is null)
                                        //{
                                        //    item.sxtid = res.Tid;
                                        //    await Repository.UpdateItemOwner(item);
                                        //}
                                        int index = checkIsExist(res.EPC);
                                        //ListaEpcs.Add(item);
                                        if (index == -1)
                                        {

                                            ListaEpcs.Add(new Tag { epc = res.EPC });
                                            epcList.Add(res.EPC);

                                            // subir a mosquito el proceso leido
                                          
                                            //Task.Run(() => Mqtt.Publish("Topic","EPC");
                                            //CuentaPersonas = ListaEpcs.Select(x => x.idPersona).Distinct().Count();
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



                    //List<string> epcList = new List<string>();
                    //String codigo = servicioRFID.InventorySingleTag(false);
                    //            await Application.Current.MainPage.DisplayToastAsync($"Lectura:{codigo}", 800);
                    //            await Clipboard.SetTextAsync(codigo);
                }
                catch
                {
                    await Application.Current.MainPage.DisplayToastAsync("Nada que leer", 800);
                }
                 
            });
        }
        
        public void OnDisappearing()
        {
            //MessagingCenter.Unsubscribe<App, string>((App)Application.Current, "InventoryTag");
            MessagingCenter.Unsubscribe<App, string>((App)Application.Current, "gatillo");
        }

        async Task AddByItem()
        {
            UserDialogs.Instance.ShowLoading("Cargando...", MaskType.Black);
            IsBusy = true;
            var route = $"{nameof(InventoryPage)}";
            await Shell.Current.GoToAsync(route);
            UserDialogs.Instance.HideLoading();
        }

        async Task AddByPerson()
        {
            UserDialogs.Instance.ShowLoading("Cargando...",MaskType.Black);
            IsBusy = true;
            //var route = $"{nameof(PersonsPage)}?Title=Seleccione elemento";
            var route = $"{nameof(PersonsPage)}";
            await Shell.Current.GoToAsync(route);
            UserDialogs.Instance.HideLoading();
        }

        async Task CargarPersonas()
        {
            IsBusy = true;
            Debug.WriteLine(">>>>Picker");
            try
            {
                var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    {DevicePlatform.Android, new[]{"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"}}
                });

                var result = await FilePicker.PickAsync(
                    new PickOptions
                    {
                        PickerTitle = "Seleccione archivo Excel",
                        FileTypes = customFileType,
                    });

                if (result != null)
                {
                    Debug.WriteLine($"File Name: {result.FileName}");
                    using (var stream = await result.OpenReadAsync())
                    {
                        var rows = stream.Query<ExcelPerson>();
                        Debug.WriteLine("<<<<< Filas:" + rows.Count<ExcelPerson>());
                    }
                }
                IsBusy = false;
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(">>>Err");
                // The user canceled or something went wrong
            }
            IsBusy = false;
            return;
    }
        public int checkIsExist(string strEPC)
        {
            int existFlag = -1;


            for (int i = 0; i < epcList.Count; i++)
            {
                string tempStr = epcList[i];
                if (strEPC == tempStr)
                {
                    existFlag = i;
                    break;
                }
            }
            return existFlag;
        }

    }
}
