using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WMS.Models
{
    public class Recuento
    {
        public class Almacen
        {
            public string WhsCode { get; set; }
            public string WhsName { get; set; }
            public string Locked { get; set; }
        }
        public class LtsRecuento
        {
            public string DistNumber { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public double OnHandQty { get; set; }
            public string WhsCodeSAP { get; set; }
            public string BinCodeSAP { get; set; }
            public int AbsEntrySAP { get; set; }
            public string WhsCode { get; set; }
            public string BinCode { get; set; }
            public int AbsEntry { get; set; }
            public double Ctd_SAP { get; set; }
            public double Ctd_Ctda { get; set; }
            public string Tipo { get; set; }
        }
    }
}