//using System;
//using System.Diagnostics;
//using System.Threading.Tasks;
//using EVIRFID.Models;
//using Xamarin.Forms;
//using EVIRFID.Services;
//using CommunityToolkit.Mvvm.ComponentModel;
//using System.Linq;
//using System.Text;
//using CommunityToolkit.Mvvm.Input;
//using Acr.UserDialogs;
//using Xamarin.CommunityToolkit.Extensions;

//namespace EVIRFID.ViewModels
//{
//    [QueryProperty(nameof(ItemId), nameof(ItemId))]
//    public partial class ItemDetailViewModel : BaseViewModel
//    {

//        [ObservableProperty]
//        Item currentItem;

//        [ObservableProperty]
//        string cveCorp;

//        private int itemId;

//        public int ItemId
//        {
//            get
//            {
//                return itemId;
//            }
//            set
//            {
//                itemId = value;
//                LoadItemId(value);
//            }
//        }

//        public async void LoadItemId(int itemId)
//        {
//            try
//            {
//                CurrentItem = await Repository.GetItemById(ItemId);
//                //CveCorp =Encoding.ASCII.GetString(StringToByteArray(CurrentItem.epc.Substring(0, 16)));
//                CveCorp = "";
//            }
//            catch (Exception)
//            {
//                Debug.WriteLine("Failed to Load Item");
//            }
//        }

//        public static byte[] StringToByteArray(string hex)
//        {
//            return Enumerable.Range(0, hex.Length)
//                             .Where(x => x % 2 == 0)
//                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
//                             .ToArray();
//        }

//        [RelayCommand]
//        async Task UpdateItem()
//        {
//            var result = await UserDialogs.Instance.ConfirmAsync("¿Seguro que desea actualizar?", "ATENCIÓN", "SI", "NO");
//            if (result)
//            {
//                await Repository.UpdateItemOwner(CurrentItem);
//                string timeStampEvento = DateTime.Now.ToString("dd_MM_yyyy_HH_mm");
//                Evento evento = new Evento();
//                evento.marcaDeTiempo = timeStampEvento;
//                evento.motivo = "Modificación de ítem;";
//                evento.accion = String.Format("Se modifica la información atribuída al chip {0}", CurrentItem.epc);
//                await Repository.InsertEventAsync(evento);
//                await Application.Current.MainPage.DisplayToastAsync("Se actualizó el detalle del ítem.", 1000);
//            }
//        }
//    }
//}

