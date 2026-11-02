using Android.Content;
using Android.Webkit;
using EVIRFID.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using EVIRFID.Droid.jsBridge;

[assembly: ExportRenderer(typeof(InventoryWebView), typeof(EVIRFID.Droid.Renderers.InventoryWebViewRenderer))]
namespace EVIRFID.Droid.Renderers
{
    public class InventoryWebViewRenderer : WebViewRenderer
    {
        public InventoryWebViewRenderer(Context ctx) : base(ctx) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.WebView> e)
        {
            base.OnElementChanged(e);

            if (Control != null && e.NewElement != null)
            {
                Control.Settings.JavaScriptEnabled = true;
                Control.Settings.DomStorageEnabled = true;

                Control.AddJavascriptInterface(
                    new HybridBridge((InventoryWebView)e.NewElement),
                    "AndroidBridge"
                );
            }
        }
    }
}
