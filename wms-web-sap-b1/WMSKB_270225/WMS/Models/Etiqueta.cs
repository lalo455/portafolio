using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WMS.Models
{
    public class Etiqueta
    {
        public string DistNumber { get; set; }
        public string BinCode { get; set; }
        public int AbsEntry { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal OnHandQty { get; set; }
        public string WhsCode { get; set; }
        public string NewBinCode { get; set; }
        public int NewAbsEntry { get; set; }
        public string Estatus { get; set; } //Estatus del Lote > 0: Liberado | 1: NO accesible | 2: Bloqueado 
        public char Cancelado { get; set; }
        public string Tipo { get; set; } //Variable que indica si el Lote es MP o PT
        public int TurnoEtq { get; set; }

        public string EtqMaster { get; set;} //Registro de código etiqueta master
        public int TurnoMaster { get; set;} 
        public string RegMaster { get; set; } //Registro es de Master previamente
    }
    public class EtiquetaRemision
    {
        public string Area { get; set; } // L o A
        public string Huacal { get; set; }
        public string OrdenVentaSAP { get; set; }
        public string OrdenVenta { get; set; }
        public string OD { get; set; }
        public string LineaPedido { get; set; }
        public string Color { get; set; }
        public string Descripcion { get; set; }
        public int Piezas { get; set; }
    }

    public class ValidacionPedidoRequest
    {
        public string OrdenVentaSAP { get; set; }
        public string NumeroLineaPedido { get; set; }
        public string DescripcionArticulo { get; set; }
        public int NumeroPiezas { get; set; }
        public string Area { get; set; }
    }
}