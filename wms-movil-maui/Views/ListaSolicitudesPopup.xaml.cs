using CommunityToolkit.Maui.Views;
using WmsMovil.Models;

namespace WmsMovil.Views
{
    public partial class ListaSolicitudesPopup : Popup
    {
        public ListaSolicitudesPopup(List<SolicitudAbierta> solicitudes)
        {
            InitializeComponent();

            // Le pasamos la lista de solicitudes a la vista
            ListaST.ItemsSource = solicitudes;
        }

        private void OnCerrarClicked(object sender, EventArgs e)
        {
            Close(); // Cierra sin seleccionar nada
        }

        private void OnSolicitudSeleccionada(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is SolicitudAbierta seleccion)
            {
                Close(seleccion); // Cierra y devuelve la solicitud seleccionada
            }
        }
    }
}