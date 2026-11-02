using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Storage;
using WmsMovil.Messages;
using WmsMovil.Models;
using WmsMovil.Services;
using WmsMovil.Views;

namespace WmsMovil.ViewModels
{
    public partial class TraspasoViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly ScannerOrchestrator _scanner;

        [ObservableProperty] private ObservableCollection<Cliente> _clientes = new();
        [ObservableProperty] private Cliente _clienteSeleccionado;
        [ObservableProperty] private ObservableCollection<Almacen> _almacenes = new();
        [ObservableProperty] private Almacen _almacenOrigen;
        [ObservableProperty] private Almacen _almacenDestino;
        [ObservableProperty] private string _solicitudTraslado = string.Empty;
        [ObservableProperty] private bool _lecturaPorRack;
        [ObservableProperty] private int _totalEscaneados = 0;

     

    

        [ObservableProperty] private string _codigoEscaneado = string.Empty;
        [ObservableProperty] private string _estadoUI = "Listo";
        // Propiedad que lee la configuración global para mostrar/ocultar el cuadro verde
        public bool IsQrMode => !GlobalSettings.IsRfidMode;

        public ObservableCollection<LoteValidado> LotesEscaneados { get; } = new();
        // --- ALERTAS ---
        public ObservableCollection<MensajeAlerta> ListaAlertas { get; } = new();
        [ObservableProperty][NotifyPropertyChangedFor(nameof(TieneAlertas))] private int _totalAlertas = 0;
        public bool TieneAlertas => TotalAlertas > 0;
        public TraspasoViewModel(ApiService apiService, ScannerOrchestrator scanner)
        {
            _apiService = apiService;
            _scanner = scanner;

          

            _scanner.OnScanFinished += ProcesarGatillazoAsync;
            _scanner.OnStatusChanged += (status) => EstadoUI = status;

            WeakReferenceMessenger.Default.Register<GatilloMensaje>(this, (r, m) =>
            {
                if (GlobalSettings.IsRfidMode) _scanner.ToggleScanner();
            });

            // LLAMAMOS A LA API AL ABRIR LA PANTALLA
            _ = CargarCatalogosAsync();
        }
       
        private async Task CargarCatalogosAsync()
        {
            EstadoUI = "Cargando catálogos...";

            // Llamamos a la API (Endpoint de tu documentación)
            var response = await _apiService.GetAsync<CatalogosResponseData>("api/TransferCatalogs/InitData");

            if (response != null && response.Codigo == 0 && response.Data != null)
            {
                Clientes = new ObservableCollection<Cliente>(response.Data.Clientes);
                Almacenes = new ObservableCollection<Almacen>(response.Data.Almacenes);
                EstadoUI = "Catálogos cargados.";
            }
            else
            {
                await App.Current.MainPage!.DisplayAlert("Error", "No se pudieron cargar los catálogos.", "OK");
                EstadoUI = "Error de red.";
            }
        }

        // REGLA DE UI: Autoseleccionar almacenes al cambiar el cliente
        partial void OnClienteSeleccionadoChanged(Cliente value)
        {
            if (value != null)
            {
                if (!string.IsNullOrEmpty(value.FromWhs))
                    AlmacenOrigen = Almacenes.FirstOrDefault(a => a.WhsCode == value.FromWhs);

                if (!string.IsNullOrEmpty(value.ToWhs))
                    AlmacenDestino = Almacenes.FirstOrDefault(a => a.WhsCode == value.ToWhs);
            }
        }

        [RelayCommand]
        private async Task BuscarSTAsync()
        {
            if (string.IsNullOrWhiteSpace(SolicitudTraslado)) return;

            EstadoUI = "Buscando Solicitud...";

            // Llamamos a la API (Endpoint de tu documentación)
            var response = await _apiService.GetAsync<List<LineaST>>($"api/TransferCatalogs/SolicitudTraslado/{SolicitudTraslado}");

            if (response != null && response.Codigo == 0 && response.Data != null && response.Data.Count > 0)
            {
                var primeraLinea = response.Data[0];

                // REGLA DE UI: Autoseleccionar almacenes de la ST
                AlmacenOrigen = Almacenes.FirstOrDefault(a => a.WhsCode == primeraLinea.FromWhsCod);
                AlmacenDestino = Almacenes.FirstOrDefault(a => a.WhsCode == primeraLinea.ToWhsCode);

                // Guardamos el DocEntry en memoria (como pediste en la documentación)
                Preferences.Default.Set("ST_DocEntry_Actual", primeraLinea.DocEntry);

                await App.Current.MainPage!.DisplayAlert("Éxito", $"Solicitud {SolicitudTraslado} cargada correctamente.", "OK");
                EstadoUI = "Solicitud cargada.";
            }
            else
            {
                await App.Current.MainPage!.DisplayAlert("Error", "No se encontró la solicitud o no tiene líneas abiertas.", "OK");
                EstadoUI = "Solicitud no encontrada.";
            }
        }
        [RelayCommand]
        private async Task ListarSolicitudesAsync()
        {
            if (ClienteSeleccionado == null || string.IsNullOrWhiteSpace(ClienteSeleccionado.CardCode))
            {
                await App.Current.MainPage!.DisplayAlert("Aviso", "Primero seleccione un cliente.", "OK");
                return;
            }

            EstadoUI = "Buscando pedidos abiertos...";

            var response = await _apiService.GetAsync<List<SolicitudAbierta>>($"api/TransferCatalogs/SolicitudesAbiertas/{ClienteSeleccionado.CardCode}");

            if (response != null && response.Codigo == 0 && response.Data != null)
            {
                EstadoUI = "Listo";

                // Abrimos el Popup y esperamos a que el usuario seleccione uno
                var popup = new ListaSolicitudesPopup(response.Data);
                var resultado = await App.Current.MainPage!.ShowPopupAsync(popup);

                if (resultado is SolicitudAbierta seleccion)
                {
                    // Si seleccionó uno, lo ponemos en el input y llamamos a Buscar automáticamente
                    SolicitudTraslado = seleccion.DocNum.ToString();
                    await BuscarSTAsync();
                }
            }
            else if (response?.Codigo == -2)
            {
                await App.Current.MainPage!.DisplayAlert("Info", response.Mensaje, "OK");
                EstadoUI = "Sin pedidos.";
            }
            else
            {
                await App.Current.MainPage!.DisplayAlert("Error", response?.Mensaje ?? "Error de red", "OK");
                EstadoUI = "Error de red.";
            }
        }
        private async void ProcesarGatillazoAsync(List<string> tagsCrudos)
        {
            if (tagsCrudos == null || tagsCrudos.Count == 0) return;

            if (AlmacenOrigen == null || AlmacenDestino == null)
            {
                await App.Current.MainPage!.DisplayAlert("Aviso", "Seleccione almacenes origen y destino antes de escanear.", "OK");
                return;
            }

            EstadoUI = "Consultando SAP...";

            // Usamos el nuevo TraspasoRequest
            var request = new TraspasoRequest
            {
                userName = Microsoft.Maui.Storage.Preferences.Default.Get("User_CODE", "WMS"),
                solTr = string.IsNullOrWhiteSpace(SolicitudTraslado) ? "N" : "Y",
                clienteSel = ClienteSeleccionado?.CardCode ?? "",
                whsCodeOrigen = AlmacenOrigen.WhsCode,
                whsCodeDestino = AlmacenDestino.WhsCode,
                ubicacionDestino = "",
                entryST = Microsoft.Maui.Storage.Preferences.Default.Get("ST_DocEntry_Actual", 0),
                tagsEscaneados = tagsCrudos
            };

            // Usamos la nueva respuesta ValidacionLecturaResponse
            var response = await _apiService.PostAsync<TraspasoRequest, ValidacionLecturaResponse>("api/SyncIntegration/Traspaso", request);

            if (response != null && response.Codigo == 0 && response.Data != null)
            {
                foreach (var lote in response.Data.Lotes)
                {
                    if (!LotesEscaneados.Any(l => l.DistNumber == lote.DistNumber))
                    {
                        LotesEscaneados.Insert(0, lote);
                    }
                }
                TotalEscaneados = LotesEscaneados.Count;
                ActualizarColoresCebra();

                if (response.Data.Alertas != null && response.Data.Alertas.Count > 0)
                {
                    ListaAlertas.Clear();
                    foreach (var alerta in response.Data.Alertas) ListaAlertas.Add(alerta);
                    TotalAlertas = ListaAlertas.Count;
                    OnPropertyChanged(nameof(TieneAlertas));

                    _ = AbrirAlertasAsync();
                }

                EstadoUI = "Validación terminada.";
            }
            else
            {
                await App.Current.MainPage!.DisplayAlert("Error", response?.Mensaje ?? "Error de red", "OK");
                EstadoUI = "Error en validación.";
            }
        }
        private void ActualizarColoresCebra()
        {
            bool esGris = false;

            foreach (var lote in LotesEscaneados)
            {
                if (!string.IsNullOrEmpty(lote.CodigoTarima))
                {
                    lote.ColorFondo = Color.FromArgb("#FFF9C4"); // Amarillo Master
                }
                else
                {
                    // CAMBIAMOS EL GRIS A UNO MÁS OSCURO (#E5E8E8)
                    lote.ColorFondo = esGris ? Color.FromArgb("#E5E8E8") : Colors.White;
                    esGris = !esGris;
                }
            }
        }

        [RelayCommand]

        private void ProcesarCodigo()
        {
            if (string.IsNullOrWhiteSpace(CodigoEscaneado)) return;

            // En lugar de simularlo, lo mandamos a la API para que lo valide en SAP
            ProcesarGatillazoAsync(new List<string> { CodigoEscaneado });

            // Limpiamos el cuadro para el siguiente escaneo
            CodigoEscaneado = string.Empty;
        }

        [RelayCommand]
        private void BorrarLote(LoteValidado lote) // <-- Cambia a LoteValidado
        {
            if (lote != null && LotesEscaneados.Contains(lote))
            {
                LotesEscaneados.Remove(lote);
                TotalEscaneados = LotesEscaneados.Count;
                ActualizarColoresCebra();
            }
        }
        
        [RelayCommand]
        private async Task AbrirAlertasAsync()
        {
            // Le pasamos ListaAlertas (que ahora es de tipo MensajeAlerta)
            var popup = new AlertasPopup(ListaAlertas);
            var resultado = await App.Current.MainPage!.ShowPopupAsync(popup);

            if (resultado?.ToString() == "limpiar")
            {
                ListaAlertas.Clear();
                TotalAlertas = 0;
                OnPropertyChanged(nameof(TieneAlertas)); // Oculta el botón rojo
            }
        }

        [RelayCommand]
        private async Task GenerarMovimientoAsync()
        {
            if (LotesEscaneados.Count == 0) return;

            // REGLA DE UI: No permitir guardar si hay Master sin confirmar (Amarillos)
            if (LotesEscaneados.Any(l => !string.IsNullOrEmpty(l.CodigoTarima) /* Aquí agregarás tu lógica de Status == false después */))
            {
                // await App.Current.MainPage!.DisplayAlert("Aviso", "Hay lotes Master sin confirmar físicamente.", "OK");
                // return;
            }

            EstadoUI = "Procesando en SAP...";

            // 1. Armamos el JSON de la FASE 2
            var request = new
            {
                wmsOrigin = "MAUI_APP",
                userName = Microsoft.Maui.Storage.Preferences.Default.Get("User_CODE", "WMS"),
                payload = new
                {
                    Accion = "TraspasoManual",
                    SolTr = string.IsNullOrWhiteSpace(SolicitudTraslado) ? "N" : "Y",
                    ClienteSel = ClienteSeleccionado?.CardCode ?? "",
                    WhsCodeOrigen = AlmacenOrigen?.WhsCode ?? "",
                    WhsCodeDestino = AlmacenDestino?.WhsCode ?? "",
                    EntryST = Microsoft.Maui.Storage.Preferences.Default.Get("ST_DocEntry_Actual", 0),
                    LotesFinales = LotesEscaneados.ToList() // Mandamos los objetos completos
                }
            };

            // 2. Llamamos a la API (Integration)
            var response = await _apiService.PostAsync<object, dynamic>("api/Integration", request);

            if (response != null && response.Codigo == 0) // HTTP 202 Accepted
            {
                await App.Current.MainPage!.DisplayAlert("Éxito", "Movimiento encolado en SAP exitosamente.", "OK");

                // Limpiamos la pantalla
                LotesEscaneados.Clear();
                ListaAlertas.Clear();
                TotalEscaneados = 0;
                TotalAlertas = 0;
                EstadoUI = "Listo";
            }
            else
            {
                await App.Current.MainPage!.DisplayAlert("Error SAP", response?.Mensaje ?? "Error de integración", "OK");
                EstadoUI = "Error en SAP.";
            }
        }



    }
}