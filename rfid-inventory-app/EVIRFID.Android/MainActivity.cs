
using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Android;
using EVIRFID.Droid.Services;
using Android.Views;
using Xamarin.Forms;
using EVIRFID.Views;
using Acr.UserDialogs;
using System.Threading.Tasks;
using Android.Media;
using Xamarin.Essentials;
using Android.Content;
using System.Windows.Markup;
using Com.Rscja.Team.Qcom.Deviceapi;

namespace EVIRFID.Droid
{
    [Activity(Label = "EVIRFIDApp", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public  class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        
        private bool isDestory;
        public bool isBound=false;
        public bool isConfigurationChnage=false;
        private DateTime tiempoInicioGatillo;
        private const int tiempoMinimoMs = 3000;
        protected override void OnCreate(Bundle savedInstanceState)
        {

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                if (!(CheckPermissionGranted(Manifest.Permission.ReadExternalStorage) && !CheckPermissionGranted(Manifest.Permission.WriteExternalStorage)))
                {
                    RequestPermission();
                }
            }

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_SYNCFUSION_LICENSE_KEY");

            base.OnCreate(savedInstanceState);
            // después de inicializar
     
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Syncfusion.XForms.Android.PopupLayout.SfPopupLayoutRenderer.Init();
            UserDialogs.Init(this);
            LoadApplication(new App());
          

            // Suscribir al mensaje de "gatillo"
            MessagingCenter.Subscribe<App>((App)Xamarin.Forms.Application.Current, "gatillo", (sender) =>
            {
                // Este bloque se ejecutará cuando se reciba el mensaje "gatillo"
                System.Console.WriteLine("¡Gatillo recibido en MainActivity!");
                // Puedes realizar la acción que desees aquí (por ejemplo, cambiar el UI, abrir un fragmento, etc.)
            });
            MessagingCenter.Subscribe<App>(
                this,
                "ScanStarted",
                (sender) =>
                {
                    BeepCorto();
                    VibrarCorto();
                }
            );

            MessagingCenter.Subscribe<App>(
                this,
                "ScanStopped",
                (sender) =>
                {
                    BeepLargo();
                    VibrarLargo();
                }
            );

        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void RequestPermission()
        {
            //ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage }, 0);
        }

        public bool CheckPermissionGranted(string Permissions)
        {
            // Check if the permission is already available.
            if (ActivityCompat.CheckSelfPermission(this, Permissions) != Permission.Granted)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            if (e.KeyCode == Android.Views.Keycode.VideoApp5 &&
                e.Action == KeyEventActions.Down &&
                e.RepeatCount == 0)
            {
                MessagingCenter.Send<App>(
                    (App)Xamarin.Forms.Application.Current,
                    "gatillazo"
                );

                return true;
            }

            return base.DispatchKeyEvent(e);
        }
        void VibrarCorto()
        {
            try
            {
                var vibrator = (Vibrator)GetSystemService(VibratorService);
                if (vibrator?.HasVibrator == true)
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        vibrator.Vibrate(
                            VibrationEffect.CreateOneShot(80, VibrationEffect.DefaultAmplitude)
                        );
                    }
                    else
                    {
#pragma warning disable CS0618
                        vibrator.Vibrate(80);
#pragma warning restore CS0618
                    }
                }
            }
            catch (Exception ex)
            {
                Android.Util.Log.Warn("VIBRATION", ex.ToString());
            }
        }

        void VibrarLargo()
        {
            try
            {
                var vibrator = (Vibrator)GetSystemService(VibratorService);
                if (vibrator?.HasVibrator == true)
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        vibrator.Vibrate(
                            VibrationEffect.CreateOneShot(250, VibrationEffect.DefaultAmplitude)
                        );
                    }
                    else
                    {
#pragma warning disable CS0618
                        vibrator.Vibrate(250);
#pragma warning restore CS0618
                    }
                }
            }
            catch (Exception ex)
            {
                Android.Util.Log.Warn("VIBRATION", ex.ToString());
            }
        }

        void BeepCorto()
        {
            ToneGenerator tone = new ToneGenerator(Stream.System, 80);
            tone.StartTone(Tone.PropBeep, 100);
        }

        void BeepLargo()
        {
            ToneGenerator tone = new ToneGenerator(Stream.System, 80);
            tone.StartTone(Tone.PropBeep, 300);
        }
        protected override void OnStart()
        {
            base.OnStart();
          
          
        }





    }
}