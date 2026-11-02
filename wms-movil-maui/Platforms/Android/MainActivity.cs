using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using CommunityToolkit.Mvvm.Messaging;
using WmsMovil.Messages;

namespace WmsMovil
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, Icon = "@mipmap/app_icon", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
  
    public class MainActivity : MauiAppCompatActivity
    {
        // Variable para controlar el estado en el Modo 1 (Toggle)
        private bool _isScanning = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            if (e.KeyCode == Keycode.VideoApp5 || (int)e.KeyCode == 280 || (int)e.KeyCode == 139)
            {
                if (GlobalSettings.IsRfidMode)
                {
                    // Solo mandamos la señal cuando se APRIETA el botón (sin guardar estados aquí)
                    if (e.Action == KeyEventActions.Down && e.RepeatCount == 0)
                    {
                        WeakReferenceMessenger.Default.Send(new GatilloMensaje(true));
                    }
                    return true; // Bloqueamos el botón físico para el láser
                }
            }
            return base.DispatchKeyEvent(e);
        }
    }
}