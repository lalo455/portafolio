using System;
using EVIRFID.Services;
namespace EVIRFID.Models
{
	public class RfidReading
	{
		public string epc { get; set; }
		public string pwr { get; set; }
        public string tid { get; set; }
        public string prenda { get; set; }
        public string idItem { get; set; }
        public string claveCorporacion { get; set; }
        public string talla { get; set; }
        public string asignacion { get; set; }
    }
}

