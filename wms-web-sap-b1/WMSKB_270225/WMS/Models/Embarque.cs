using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WMS.Models
{
    public class Embarque
    {
        public class ASN
        {
            public int LineId { get; set; }
            public string U_LicPlate { get; set; }
            public string U_CardCode { get; set; }
            public int DocEntry { get; set; }
            public string U_ItemSN { get; set; }
            public double U_QtyDisp { get; set; }
            public int U_EtiDesde { get; set; }
            public int U_EtiHasta { get; set; }
            public int U_Quantity { get; set; }
            public string EtiLeidas { get; set; }
            public string EtiExterno { get; set; }
        }
        public class NParte
        {
            public int Encontrado { get; set; }
            public string Parte { get; set; }
        }

        public class Socios {
            public string CardCode { get; set; }
            public string CardName { get; set; }
        }
    }
}