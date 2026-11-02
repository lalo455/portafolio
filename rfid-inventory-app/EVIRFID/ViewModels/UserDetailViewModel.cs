using System;
using System.Diagnostics;
using System.Threading.Tasks;
using EVIRFID.Models;
using Xamarin.Forms;

namespace EVIRFID.ViewModels
{
    //[QueryProperty(nameof(UserId), nameof(UserId))]
    public class UserDetailViewModel : BaseViewModel
    {
        private string userId;
        private string text;
        private string description;
        public string Id { get; set; }

        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }

        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        public string ItemId
        {
            get
            {
                return userId;
            }
            set
            {
                userId = value;
                LoadUserId(value);
            }
        }        

        public async void LoadUserId(string itemId)
        {
            try
            {
                //var user = await DataStore.GetUserAsync(userId);
                //Id = user.Id;
                //Text = user.Text;
                //Description = user.Description;
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to Load Item");
            }
        }
    }
}

