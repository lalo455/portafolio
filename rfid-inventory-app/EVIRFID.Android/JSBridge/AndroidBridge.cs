using Android.Webkit;
using Java.Interop;
using System.Diagnostics;

namespace EVIRFID.Droid.JSBridge
{
    public class AndroidBridge : Java.Lang.Object
    {
        [JavascriptInterface]
        [Export("SetUserFromWMS")]
        public void SetUserFromWMS(string user)
        {
            Debug.WriteLine($"*** Usuario recibido desde WMS: {user} ***");

            Xamarin.Forms.MessagingCenter.Send<object, string>(this, "UsuarioWMS", user);
        }
    }
}
