using System;

namespace EVIRFID.Services
{
    public interface IAudioService
    {
        void PlayBeep(bool isNewTag = false);
    }
}
