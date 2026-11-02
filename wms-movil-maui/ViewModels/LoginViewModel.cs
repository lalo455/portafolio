using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using WmsMovil.Models;
using WmsMovil.Services;

namespace WmsMovil.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string _usuario = string.Empty; 

        [ObservableProperty]
        private string _password = string.Empty; 
        [ObservableProperty]
        private bool _isPasswordHidden = true; // Por defecto la contraseña está oculta

       
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string _versionApp = "v4.0.0 (MAUI Nativo)";

        public LoginViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }
        // Comando para el botón del "Ojo"
        [RelayCommand]
        private void TogglePasswordVisibility()
        {
            IsPasswordHidden = !IsPasswordHidden;
        }
        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Usuario) || string.IsNullOrWhiteSpace(Password))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Ingrese usuario y contraseña", "OK");
                return;
            }

            IsBusy = true;

            try
            {
                // El Request Body exacto de la documentación
                var loginData = new
                {
                    usuario = this.Usuario,
                    password = this.Password
                };

                // Llamamos a la API esperando un LoginResponseData
                var response = await _apiService.PostAsync<object, LoginResponseData>("api/Auth/Login", loginData);

                if (response != null && response.Codigo == 0)
                {
                    // ¡LOGIN EXITOSO! Guardamos los datos del usuario en el dispositivo
                    Preferences.Default.Set("User_CODE", response.Data.User_CODE);
                    Preferences.Default.Set("U_NAME", response.Data.U_NAME);
                    Preferences.Default.Set("Warehouse", response.Data.Warehouse);

                 
                    // Redirigimos al Menú Principal envuelto en un NavigationPage (Para tener la barra superior y botón "Atrás")
                    var menuPage = Application.Current.MainPage.Handler.MauiContext.Services.GetService<MenuPage>();
                    Application.Current.MainPage = new NavigationPage(menuPage);
                }
                else
                {
                    // Mostramos el error devuelto por la API (-1)
                    await App.Current.MainPage.DisplayAlert("Acceso Denegado", response?.Mensaje ?? "Error desconocido", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}