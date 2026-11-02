using Android.Media;
using Stream = Android.Media.Stream;

namespace WmsMovil.Platforms.Android.Services
{
    public static class AudioService
    {
        public static void PlayBeep(bool isStart)
        {
            // Tono corto para Iniciar, Tono largo/diferente para Detener
            var toneType = isStart ? Tone.PropBeep : Tone.PropBeep2;
            int durationMs = isStart ? 100 : 300;

            new ToneGenerator(Stream.System, 80).StartTone(toneType, durationMs);
        }
    }
}