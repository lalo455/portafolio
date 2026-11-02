using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WMS.Models
{
    public class EtqMaster
    {
        public string Codigo { get; set; }
        public string MasterEtq { get; set; }
        public string Articulo { get; set; }
        public int Turno { get; set; } //Turno de la Etiqueta Master
        public string Estatus { get; set; }
        public string Bloqueado { get; set; }
        public string Cancelado { get; set; }
        public string Calidad { get; set; }
        public double MasterOnHandQty { get; set; }

        public string EtqLote { get; set; }
        public string EtqLoteArticulo { get; set; }
        public double EtqLoteOnHandQty { get; set; }
        public double EtqLoteStdPack { get; set; }
        public string EtqLoteEstatus { get; set; } //L: Liberado | G: Generado | T: Traspaleado | R: Envidado a Produccion | E: Embarcado
        public string EtqLoteBloqueado { get; set; } //0: Liberado | 1: No accesible | 2: Bloqueado
        public string EtqLoteCalidad { get; set; } 
        public string EtqLoteCancelado { get; set; } //N: No | Y: Si
        public int EtqLoteTurno { get; set; } //Turno de la etiqueta individual del lote
        public string Mwms { get; set; }

        public string Lote { get; set; }
        public string LoteArticulo { get; set; }
        public string LoteEstatus { get; set; } //0: Liberado | 1: No accesible | 2: Bloqueado


        public class Detalle
        {
            public string TipoReg { get; set; } //1: Informacion Master | 2: Detalle de etiquetas | 3: Detalle de Lotes

            public string Codigo { get; set; }
            public string MasterEtq { get; set; }
            public string Articulo { get; set; }
            public int Turno { get; set; } //Turno de la Etiqueta Master
            public string Estatus { get; set; }
            public string Bloqueado { get; set; }
            public string Cancelado { get; set; }
            public string Calidad { get; set; }
            public double MasterOnHandQty { get; set; }

            public string EtqLote { get; set; }
            public double EtqLoteOnHandQty { get; set; }
            public double EtqLoteStdPack { get; set; }
            public string EtqLoteEstatus { get; set; }  //L: Liberado | G: Generado | T: Traspaleado | R: Envidado a Produccion | E: Embarcado
            public string EtqLoteCalidad { get; set; }
            public string EtqLoteCancelado { get; set; } //N: No | Y: Si
            public int EtqLoteTurno { get; set; } //Turno de la etiqueta individual del lote
            public string EtqLoteFecha { get; set; } //Fecha generación de la etiqueta individual del lote

            public string Lote { get; set; }
            public string BinCode { get; set; }
            public int AbsEntry { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }            
            public decimal OnHandQty { get; set; }
            public string WhsCode { get; set; }
            public string NewBinCode { get; set; }
            public int NewAbsEntry { get; set; }
            public string LoteEstatus { get; set; } //0: Liberado | 1: No accesible | 2: Bloqueado
            public string Tipo { get; set; }
            public string EtqMaster { get; set; }            
            public string Dispositivo { get; set; }

            //Datos de Recuento
            public double Ctd_SAP { get; set; }
            public double Ctd_Ctda { get; set; }

            //Datos Liberar
            public int OrdenFab { get; set; }
            public string Usuario { get; set; }
            public string FechaCreacion { get; set; }
        }

        public class ListaMaster {
            public string Codigo { get; set; }
            public string MasterEtq { get; set; }
            public string Articulo { get; set; }
            public double MasterOnHandQty { get; set; }
            public string WhsCode { get; set; }
            public int OrdenFab { get; set; }
        }
    
    }
}