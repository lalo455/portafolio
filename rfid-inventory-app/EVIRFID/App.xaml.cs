using System;
using System.Reflection;
using Xamarin.Forms;
using EVIRFID.Services;
using EVIRFID.Views;
using System.IO;
using Xamarin.Essentials;
using System.Diagnostics;
using System.Collections.Generic;
using Xamarin.MQTT;
using System.Threading.Tasks;

namespace EVIRFID
{
    public partial class App : Application
    {
        private readonly IRfidService rfidService;

        // Ruta de Base de Datos
        public static string DbPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "evirfid.db3");

        // Credenciales MQTT
        //string ServerName = "api.example.com";
        string ServerName = "10.0.0.1";
        int Port = 1883;
        string ClientID = "WMSListener_";
        string Username = "";
        string Password = "";

        public App()
        {
            VersionTracking.Track();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_SYNCFUSION_LICENSE_KEY");
            DevExpress.XamarinForms.Popup.Initializer.Init();
            DevExpress.XamarinForms.CollectionView.Initializer.Init();
            DevExpress.XamarinForms.Editors.Initializer.Init();
            DevExpress.XamarinForms.DataForm.Initializer.Init();

            InitializeComponent();

            // -----------------------------------------------------------------
            // AL RECIBIR EL USUARIO DEL WEBVIEW, SOLO LO GUARDAMOS EN MEMORIA
            // -----------------------------------------------------------------
            MessagingCenter.Subscribe<object, string>(this, "SetUserFromWMS", (sender, user) =>
            {
                if (!string.IsNullOrWhiteSpace(user))
                {
                    string safeUser = user.Trim().ToLowerInvariant();
                    Preferences.Set("LastMqttUser", safeUser);
                    Debug.WriteLine($"[App] Usuario guardado para MQTT Batch: {safeUser}");
                }
            });

            // Create preferences if they don't exist
            if (!Preferences.ContainsKey("LecturaContinua")) Preferences.Set("LecturaContinua", true);
            if (!Preferences.ContainsKey("SoloEvi")) Preferences.Set("SoloEvi", true);
            if (!Preferences.ContainsKey("Tipo")) Preferences.Set("Tipo", "RFID");
            if (!Preferences.ContainsKey("Origen")) Preferences.Set("Origen", "Mexicano");
            if (!Preferences.ContainsKey("Proveedor")) Preferences.Set("Proveedor", "RFID 2023");
            if (!Preferences.ContainsKey("Propietario")) Preferences.Set("Propietario", "GUÍA RFID");
            if (!Preferences.ContainsKey("NotaDefecto")) Preferences.Set("NotaDefecto", "Todos los derechos reservados, 2023");

            // Primera ejecución
            if (VersionTracking.IsFirstLaunchEver)
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
                using (Stream stream = assembly.GetManifestResourceStream("EVIRFID.evirfid.db3"))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        File.WriteAllBytes(DbPath, memoryStream.ToArray());
                    }
                }
            }

            DependencyService.Register<MockDataStore>();
            DependencyService.Register<IRfidService>();

            MainPage = new InventoryPage();

            rfidService = DependencyService.Get<IRfidService>();
        }

        // -------------------- MOTOR MOSQUITTO (BATCH) --------------------
        public async Task<bool> SendBatchAsync(List<string> epcs)
        {
            try
            {
                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                    return false;

                // Recupera el usuario exacto que mandó el WebView
                string user = Preferences.Get("LastMqttUser", "temporal").ToLowerInvariant();
                var topicList = new List<string> { user };

                Debug.WriteLine($"Enviando {epcs.Count} tags al WMS en el canal: {user}");

                // TRUCO VITAL: Agregamos un ID aleatorio al ClientID. 
                // Esto fuerza a Mosquitto a darnos una sesión nueva y limpia al instante.
                string randomId = Guid.NewGuid().ToString().Substring(0, 6);
                var mqttConn = new MqttConnection(ServerName, Port, ClientID + randomId, Username, Password, topicList);

                var connectMethod = mqttConn.GetType().GetMethod("CreateMQTTConnection");
                if (connectMethod != null)
                {
                    var result = connectMethod.Invoke(mqttConn, null);
                    if (result is Task t) await t;
                }

                // Enviar el bloque completo
                foreach (var epc in epcs)
                {
                    mqttConn.SendMessageToTopic(user, epc);
                    await Task.Delay(10); // Micro-pausa para no ahogar el socket
                }

                // Enviar mensaje de FIN
                mqttConn.SendMessageToTopic(user, "4556494349544946494e303035");
                await Task.Delay(500); // Dar tiempo a que el último paquete salga de la antena de red

                // Desconectar limpiamente
                try
                {
                    var disconnectMethod = mqttConn.GetType().GetMethod("Disconnect");
                    if (disconnectMethod != null) disconnectMethod.Invoke(mqttConn, null);
                }
                catch { }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en envío Batch: {ex.Message}");
                return false;
            }
        }

        // -------------------- CICLO DE VIDA (LIMPIO) --------------------
        protected override void OnStart()
        {
            Xamarin.Essentials.DeviceDisplay.KeepScreenOn = true;
            try { rfidService.StopInventory(); } catch { }
            InitializeReader();
        }

        protected override void OnSleep()
        {
            //  apagar la pantalla para ahorrar batería si la minimizan
            Xamarin.Essentials.DeviceDisplay.KeepScreenOn = false;
            try { rfidService.StopInventory(); } catch { }
            try { rfidService.Free(); } catch { }
        }

        protected override void OnResume()
        {
            Xamarin.Essentials.DeviceDisplay.KeepScreenOn = true;
            InitializeReader();
        }

        private void InitializeReader()
        {
            for (int i = 0; i < 3; i++)
            {
                if (rfidService.Init()) break;
                rfidService.Free();
            }
            MessagingCenter.Send<App>(this, "ReaderReady");
        }
    }
}