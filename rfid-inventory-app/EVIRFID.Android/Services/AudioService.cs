using Android.Media;
using EVIRFID.Droid.Services;
using EVIRFID.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioService))]
namespace EVIRFID.Droid.Services
{
    public class AudioService : IAudioService
    {
        private ToneGenerator _toneGenerator;

        public AudioService()
        {
            // Initializes the tone generator on the notification stream
            _toneGenerator = new ToneGenerator(Stream.Notification, 100); 
        }

        public void PlayBeep(bool isNewTag = false)
        {
            if (isNewTag)
            {
                // High pitch, very short beep for new tag detection
                _toneGenerator.StartTone(Tone.PropBeep, 40); 
            }
            else
            {
                // Different pitch, slightly longer for start/stop scan
                _toneGenerator.StartTone(Tone.PropAck, 150); 
            }
        }
    }
}
