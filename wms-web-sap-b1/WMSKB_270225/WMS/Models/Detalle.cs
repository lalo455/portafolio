using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WMS.Models
{
    public class Detalle
    {
        public string DistNumber { get; set; }
        public string BinCode { get; set; }
        public int AbsEntry { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public double OnHandQty { get; set; }
        public string WhsCode { get; set; }
        public string NewBinCode { get; set; }
        public int NewAbsEntry { get; set; }
        public string Dispositivo { get; set; }
        public int NumDispositivos { get; set; }
        public double PesoBruto { get; set; }
        public double PesoNeto { get; set; }
        public string TipoLectura { get; set; }
    }
    public class DetalleR {
        public string DistNumber { get; set; }
        public string BinCode { get; set; }
        public int AbsEntry { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public double OnHandQty { get; set; }
        public string WhsCode { get; set; }
        public int Tarima { get; set; }
        public string Dispositivo { get; set; }
        public int NumDispositivos { get; set; }
        public double PesoBruto { get; set; }
        public double PesoNeto { get; set; }
        public string NewBinCode { get; set; }
        public int NewAbsEntry { get; set; }

        public int LineNum { get; set; } // Linea del Pedido relacionado

        public string TipoLote { get; set; } //Tipo de Lote MP | PT
        public double QtyEmbarque { get; set; } //Cantidad a Embarcar
        public double QtySAP { get; set; } //Cantidad de Lote GLOBAL SAP
        public string GlobalBatch { get; set; } //Lte Global SAP de la PT Individual
        public int NumEtq { get; set; } //Numero de Etiqueta

        public string TipoLectura { get; set; }
    }
    public class DetalleC
    {
        //public string DistNumber { get; set; }
        //public string BinCode { get; set; }
        //public int AbsEntry { get; set; }
        //public string ItemCode { get; set; }
        //public string ItemName { get; set; }
        //public double OnHandQty { get; set; }
        //public string WhsCode { get; set; }
        //public string Tipo { get; set; }
    }
}