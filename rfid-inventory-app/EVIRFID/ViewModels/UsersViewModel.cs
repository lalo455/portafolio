using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using EVIRFID.Models;
using EVIRFID.Views;
using EVIRFID.Services;
using Xamarin.CommunityToolkit.ObjectModel;

namespace EVIRFID.ViewModels
{
    public class UsersViewModel : BaseViewModel
    {
        private User _selectedUser;

        public ObservableRangeCollection<User> Users { get; set; }
        public Command LoadUsersCommand { get; }
        public Command AddUserCommand { get;  }
        public Command<User> UserTapped { get; }
        public bool IncludeInactive { get; set; } = false;

        public UsersViewModel()
        {
            Title = "Usuarios";
            Users = new ObservableRangeCollection<User>();

            LoadUsersCommand = new Command(async () => await ExecuteLoadUsersCommand(IncludeInactive));

            UserTapped = new Command<User>(OnUserSelected);

            AddUserCommand = new Command(OnAddUser);

            //Fill the observable collection

        }

        async Task ExecuteLoadUsersCommand(bool includeInactive)
        {
            IsBusy = true;

            Users.Clear();

            try
            {
                foreach(var user in await Repository.GetUsers(true))
                {
                    if (user.activo == 1)
                        if(!Users.Contains(user))
                            Users.Add(user);
                    if(user.activo == 0 && includeInactive)
                        if (!Users.Contains(user))
                            Users.Add(user);
                }

                //Users.AddRange(await Repository.GetUsers(true));

                //if (!includeInactive)
                //{
                //    foreach (var user in Users)
                //    {
                //        if (user.activo == 0)
                //            Users.Remove(user);
                //    }
                //}
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
            SelectedUser = null;
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                OnUserSelected(value);
            }
        }

        private async void OnAddUser(object obj)
        {
            //await Shell.Current.GoToAsync(nameof(NewUserPage));
        }

        async void OnUserSelected(User user)
        {
            if (user == null)
                return;

            // This will push the UserDetailPage onto the navigation stack
            //await Shell.Current.GoToAsync($"{nameof(UserDetailPage)}?{nameof(UserDetailViewModel.UserId)}={user.idUsuario}");
        }
    }
}
