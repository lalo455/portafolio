using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WMS.Models
{
    public class Traspaleo
    {
        public string U_ItemCode { get; set; }
        public string U_ItemName { get; set; }
        public string Name { get; set; }
        public char U_stImp { get; set; }
        public char U_stEti { get; set; }
        public char U_stCal { get; set; }
        public char U_Cancel { get; set; }
        public double U_QtyEti { get; set; }
        public string U_Dispositivo { get; set; }
        public int U_OF { get; set; }
        public DateTime U_GenDate { get; set; }
        public string WhsCode { get; set; }
    }

    public class TablaEvt
    {
        public int Etqs { get; set; }
        public string U_Etiqueta { get; set; }

        public TablaEvt() {
            Etqs = 0;
            U_Etiqueta = "";
        }
    }
}