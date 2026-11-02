using Android.Webkit;
using Java.Interop;
using EVIRFID.Controls;
using Xamarin.Forms;

namespace EVIRFID.Droid.jsBridge
{
    public class HybridBridge : Java.Lang.Object
    {
        InventoryWebView _formsWebView;

        public HybridBridge(InventoryWebView formsWebView)
        {
            _formsWebView = formsWebView;
        }

        [JavascriptInterface]
        [Export("SetUserFromWMS")]
        public void SetUserFromWMS(string usuario)
        {
            Android.Util.Log.Info("WMS", $"Usuario recibido desde JS: {usuario}");

            // Llama al método del WebView en el proyecto compartido
            _formsWebView?.RecibirUsuario(usuario);
            // ENVÍA DIRECTO A App.xaml.cs
            MessagingCenter.Send<object, string>(this, "SetUserFromWMS", usuario);
        }
    }
}
