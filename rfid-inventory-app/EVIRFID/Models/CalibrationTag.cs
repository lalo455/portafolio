using System;

namespace EVIRFID.Models
{
    public class CalibrationTag
    {
        public string EpcHex { get; set; }
        public string TranslatedValue { get; set; }
        public int RssiValue { get; set; }
        public bool IsInRange { get; set; }
    }
}
