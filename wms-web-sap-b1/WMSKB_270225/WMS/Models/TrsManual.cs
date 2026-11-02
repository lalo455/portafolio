using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WMS.Models
{
    public class TrsManual : Detalle
    {
        public class Almacen
        {
            public string WhsCode { get; set; }
            public string WhsName { get; set; }
            public string Locked { get; set; }

            public string Tipo { get; set; } //Origen | Destino | Cuarentena
        }
        public class Cliente
        {
            public string CardCode { get; set; }
            public string CardName { get; set; }
        }
        public class ClienteAlmacen
        {
            public string CardCode { get; set; }
            public string CardName { get; set; }
            public string FromWhs { get; set; }
            public string ToWhs { get; set; }
        }
        public class Ubicacion
        {
            public int AbsEntry { get; set; }
            public string BinCode { get; set; }
        }
        public class Traspaso
        {
            public string ClienteSel { get; set; }
            public string AlmacenSel { get; set; }
            public List<Detalle> Lineas { get; set; }
        }

        public class TraspasoCalidad {
            public string ToWhsCode { get; set; }
            public List<Detalle> Lineas { get; set; }
        }

        public class ContenedorOriginal
        {
            public string Code { get; set; }
            public string U_Dispositivo { get; set; }
            public char U_Original { get; set; }

            public char U_OrigDesechable { get; set; }
        }

        public class NumeröParte
        {
            public string Parte { get; set; }
        }
    }
}