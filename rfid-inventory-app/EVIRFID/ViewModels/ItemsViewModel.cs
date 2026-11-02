//using System;
//using System.Collections.ObjectModel;
//using System.Diagnostics;
//using System.Threading.Tasks;

//using Xamarin.Forms;
//using EVIRFID.Services;
//using EVIRFID.Models;
//using EVIRFID.Views;
//using Xamarin.CommunityToolkit.ObjectModel;
//using CommunityToolkit.Mvvm.ComponentModel;
//using System.Linq;
//using Xamarin.CommunityToolkit.Extensions;
//using Acr.UserDialogs;

//namespace EVIRFID.ViewModels
//{
//    public partial class ItemsViewModel : BaseViewModel
//    {
//        private Item _selectedItem;

//        public ObservableRangeCollection<Item> Items { get; set; }
//        public Command LoadItemsCommand { get; }
//        public Command AddItemCommand { get;  }
//        public Command MassiveAddItemCommand { get; }
//        public Command<Item> ItemTapped { get; }
//        public Command LoadFilteredItemsCommand { get; }
//        public Command LinkItemToCurrentPersonCommand { get; }
//        public Command UnlinkItemCommand { get; }
//        public Command ShowItemDetailsCommand { get; }

//        //public Services.IDataStore<Item> DataStore => DependencyService.Get<Services.IDataStore<Item>>();

//        [ObservableProperty]
//        public int page;

//        [ObservableProperty]
//        public bool isRefreshing = false;

//        [ObservableProperty]
//        public string filtro;

//        public ItemsViewModel()
//        {
//            Title = "Ítems";
//            Items = new ObservableRangeCollection<Item>();
//            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
//            LoadFilteredItemsCommand = new Command(async () => await LoadFilteredItems());
//            LinkItemToCurrentPersonCommand = new Command(async() => await LinkItemToCurrentPerson());
//            UnlinkItemCommand = new Command(async () => await UnlinkItem());
//            ShowItemDetailsCommand = new Command(async () => await ShowItemDetails());

//            ItemTapped = new Command<Item>(OnItemSelected);

//            AddItemCommand = new Command(OnAddItem);

//            MassiveAddItemCommand = new Command(OnMassiveAddItemCommand);

//            //MessagingCenter.Subscribe<App>((App)Application.Current, "gatillo", async (sender) => {
//            //    Filtro = servicioRFID.InventorySingleTag();
//            //    await LoadFilteredItems();
//            //    await Application.Current.MainPage.DisplayToastAsync("Gatillo", 300);
//            //});
//            page = -1;
//        }

//        async Task UnlinkItem()
//        {
//            if (SelectedItem is null)
//                return;

//            var result = await UserDialogs.Instance.ConfirmAsync("¿Seguro que desea borrar la asignación?", "ATENCIÓN", "SI", "NO");
//            if (result)
//            {
//                SelectedItem.idPersona = 0;
//                SelectedItem.nombre = null;
//                SelectedItem.fechaAsignacion = null;
//                if (await Repository.UpdateItemOwner(SelectedItem) == 1)
//                {
//                    await Application.Current.MainPage.DisplayToastAsync("Elemento deasignado", 300);
//                    page = -1;
//                    await ExecuteLoadItemsCommand();
//                }
//                else
//                {
//                    await Application.Current.MainPage.DisplayToastAsync("Error al asignar", 300);
//                }
//            }
//        }

//        async Task ShowItemDetails()
//        {
//            if (SelectedItem == null)
//                return;

//            // This will push the ItemDetailPage onto the navigation stack
//            var route = $"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={SelectedItem.idItem}";
//            await Shell.Current.GoToAsync(route);
//        }

//        async Task LinkItemToCurrentPerson()
//        {
//            SelectedItem.idPersona = CurrentPerson.idPersona;
//            if(await Repository.UpdateItemOwner(SelectedItem) == 1)
//            {
//                await Application.Current.MainPage.DisplayToastAsync("Asignación ok", 300);
//                page = -1;
//                await ExecuteLoadItemsCommand();
//            } else
//            {
//                await Application.Current.MainPage.DisplayToastAsync("Error al asignar", 300);
//            }
//        }

//        async Task scan()
//        {
//            bool resultado = true;
            
//            if (resultado)
//                await Application.Current.MainPage.DisplayToastAsync("Leyendo ok", 200);
//        }

//        async Task ExecuteLoadItemsCommand()
//        {
//            page += 1;
//            Debug.WriteLine(">>>>>" + page);
//            IsBusy = true;
//            Items.Clear();
//            try
//            {
//                //Llenar items
//                var objetos = await Repository.GetItemsAsync();
//                Items.AddRange(objetos.Skip(page * 20).Take(20));
//                isRefreshing = false;
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine("AZM:" + ex);
//            }
//            finally
//            {
//                IsBusy = false;
//            }
//            //------
//        }

//        async Task LoadFilteredItems()
//        {
//            IsBusy = true;
//            Items.Clear();
//            try
//            {
//                if(Filtro == "" || Filtro is null)
//                {
//                    page = -1;
//                    SelectedItem = null;
//                    await ExecuteLoadItemsCommand();
//                    CurrentPerson = IgualarPersonas(DataStore.GetCurrentPerson());
//                    return;
//                }
//                string filtroLimpio = Filtro.Replace("  ", " ").Trim();
//                var items = await Repository.GetFilteredItemsAsync(filtroLimpio.ToUpper());
//                await Application.Current.MainPage.DisplayToastAsync("Cargando " + items.Count() + " resultados", 300);
//                Items.AddRange(items);
//                isRefreshing = false;
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine("AZM:" + ex);
//            }
//            finally
//            {
//                IsBusy = false;
//            }
//        }

//        public async void OnAppearing()
//        {
//            MessagingCenter.Subscribe<App>((App)Application.Current, "gatillo", async (sender) => {
//                Filtro = servicioRFID.InventorySingleTag();
//                await LoadFilteredItems();
//                await Application.Current.MainPage.DisplayToastAsync("Gatillo", 300);
//            });
//            IsBusy = true;
//            SelectedItem = null;
//            await ExecuteLoadItemsCommand();
//            CurrentPerson = IgualarPersonas(DataStore.GetCurrentPerson());
//        }

//        public Item SelectedItem
//        {
//            get => _selectedItem;
//            set
//            {
//                SetProperty(ref _selectedItem, value);
//                OnItemSelected(value);
//            }
//        }

//        private async void OnAddItem(object obj)
//        {
//            if(Filtro is null || Filtro.Substring(0,4) != "544C" || Filtro.Length != 24 || Items.Count > 0)
//            {
//                await Application.Current.MainPage.DisplayToastAsync("Primero debe leer un chip que no esté en la Base de Datos y que corresponda a EVI", 3000);
//                return;
//            }
//            await Shell.Current.GoToAsync($"{nameof(NewItemPage)}?{nameof(Filtro)}={Filtro}");
//        }

//        private async void OnMassiveAddItemCommand(object obj)
//        {
            
//            await Application.Current.MainPage.DisplayToastAsync("Alta masiva de items - EVI", 2000);
                
//            await Shell.Current.GoToAsync($"{nameof(MassiveAddPage)}");
//        }

//        async void OnItemSelected(Item item)
//        {
            
//        }

//        public void OnDisappearing()
//        {
//            MessagingCenter.Unsubscribe<App, string>((App)Application.Current, "InventoryTag");
//            MessagingCenter.Unsubscribe<App, string>((App)Application.Current, "gatillo");
//        }
//    }
//}
