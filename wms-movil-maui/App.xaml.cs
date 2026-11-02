using WmsMovil.Services;

namespace WmsMovil
{
    public partial class App : Application
    {
        private readonly IRfidService _rfidService;

        // MAUI inyectará el servicio RFID y la pantalla principal automáticamente
        public App(IRfidService rfidService, MainPage mainPage)
        {
            InitializeComponent();

            // Forzamos el modo claro
            Application.Current.UserAppTheme = AppTheme.Light;

            _rfidService = rfidService;

            // Asignamos la pantalla principal que MAUI construyó para nosotros
            MainPage = mainPage;
        }

        protected override void OnResume()
        {
            base.OnResume();
            DeviceDisplay.Current.KeepScreenOn = true;

            for (int i = 0; i < 3; i++)
            {
                if (_rfidService.Init()) break;
                _rfidService.Free();
                Thread.Sleep(100);
            }
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            DeviceDisplay.Current.KeepScreenOn = false;

            try { _rfidService.StopInventory(); } catch { }
            try { _rfidService.Free(); } catch { }
        }
    }
}