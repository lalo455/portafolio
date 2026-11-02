using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using WmsMovil.Models; // Asegúrate de tener esto

namespace WmsMovil.Views
{
    public partial class AlertasPopup : Popup
    {
        // MIRA AQUÍ: Debe decir MensajeAlerta
        public AlertasPopup(ObservableCollection<MensajeAlerta> alertas)
        {
            InitializeComponent();
            ListaAlertas.ItemsSource = alertas;
        }

        private void OnCerrarClicked(object sender, EventArgs e) => Close("cerrar");
        private void OnLimpiarClicked(object sender, EventArgs e) => Close("limpiar");
    }
}