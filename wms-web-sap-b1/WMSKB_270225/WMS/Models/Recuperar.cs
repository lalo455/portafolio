using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WMS.Models
{
    public class Recuperar
    {
        public class Line
        {
            public int LineNum { get; set; }
            public string ItemCode { get; set; }
            public string Desde { get; set; }
            public string Hasta { get; set; }
            public int UbDestino { get; set; }
            public double Qty { get; set; }
            public string Lotes { get; set; }
            public int Tarima { get; set; }
            public string Dispositivo { get; set; }
            public int NumDispositivos { get; set; }
            public double PesoBruto { get; set; }
            public double PesoNeto { get; set; }

            //Actualización de Documento Preliminar Numero de OC y Posicion de OC - 13-01-2021
            public string OC { get; set; } 
            public string LineOC { get; set; }

            public string RemSubstitute { set; get; }
            public string RemPurchase { set; get; }
            public string RemEmpaquePallet { set; get; }
        }

        public class Batch
        {
            public int BaseLine { get; set; }
            public int LineNumLote { get; set; }
            public string Lote { get; set; }
            public double QtyLote { get; set; }
        }

        public class BinLocation
        {
            public int BaseLine { get; set; }
            public int BaseLineLote { get; set; }
            public int UbOrigen { get; set; }
            public double QtyUbOrigen { get; set; }

            public int UbDestino { get; set; }
            public double QtyUbDestino { get; set; }
        }

        //Se agrega para el Traspaleo por Numero de Parte
        public class Partes
        { 
            public string NoParte { get; set; }
        }

    }
}