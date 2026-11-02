using System;
using System.Collections.Generic;
using System.Text;

namespace EVIRFID
{
    public interface IStartService
    {
        void StartForegroundServiceCompat();
        void StopForegroundServiceCompat();

    }
}