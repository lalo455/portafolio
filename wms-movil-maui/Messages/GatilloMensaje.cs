using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WmsMovil.Messages
{
    // Este mensaje viajará por toda la app llevando un "true" (presionado) o "false" (soltado)
    public class GatilloMensaje : ValueChangedMessage<bool>
    {
        public GatilloMensaje(bool presionado) : base(presionado) { }
    }
}