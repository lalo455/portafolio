using Xamarin.Forms;

namespace EVIRFID.Controls
{
    public class InventoryWebView : WebView
    {
        // Llamado desde el HybridBridge
        public void RecibirUsuario(string usuario)
        {
            System.Diagnostics.Debug.WriteLine(
                $"*** Usuario recibido del WebView: {usuario}"
            );

            MessagingCenter.Send<object, string>(
                this,
                "SetUserFromWMS",
                usuario
            );
        }
    }
}
