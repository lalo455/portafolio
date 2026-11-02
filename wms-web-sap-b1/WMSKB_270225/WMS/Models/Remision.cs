using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WMS.Models
{
    public class Remision : Detalle
    {
        public class Preliminar
        {
            public string ClienteSel { get; set; }
            public int Pedido { get; set; }
            public string Referencia { get; set; }
            public List<DetalleR> Lineas { get; set; }
        }

        public class Pedido
        {
            public int DocEntry { get; set; }
            public int DocNum { get; set; }
            public string NumAtCard { get; set; }
            public string DocDate { get; set; }
        }
        
        public class DetallePedido
        {
            public int LineNum { get; set; }
            public string ItemCode { get; set; }
            public double Quantity { get; set; }
        }

        //Actualizacion 19-01-2021
        public class ContenedorOriginalR
        {
            public string Code { get; set; }
            public string U_Dispositivo { get; set; }
            public char U_Original { get; set; }

            public char U_OrigDesechable { get; set; }
        }

        public class NumeröParteR
        {
            public string Parte { get; set; }
        }

        public class RemAdicional
        {
            public string RemSubstitute { set; get; }
            public string RemPurchase { set; get; }
            public string RemEmpaquePallet { set; get; }
        }
    }

    public class LineasR
    {
        public string ItemCode { get; set; }
        public double Quantity { get; set; }
        public string U_Etis_Emb2 { get; set; }
        public string U_Dispositivo { get; set; }
        public int U_Cant_Disp { get; set; }
        public double U_PesoBruto { get; set; }
        public double U_PesoNeto { get; set; }
        public List<LotesR> Lotes { get; set; }
        public List<UbicacionR> Ubicaciones { get; set; }

        public LineasR() {
            Lotes = new List<LotesR>();
            Ubicaciones = new List<UbicacionR>();
        }
    }

    public class LotesR
    {
        public int BaseLineNumber { get; set; }
        public string BatchNumber { get; set; }
        public string Location { get; set; }
        public double Quantity { get; set; }
    }

    public class UbicacionR
    {
        public int BinAbsEntry { get; set; }
        public double Quantity { get; set; }
        public int SerialAndBatchNumbersBaseLine { get; set; }
        public int BaseLineNumber { get; set; }
    }
    public class PedidoArquitectura
    {
        public string OrdenVentaSAP { get; set; }
        public string NumeroLineaPedido { get; set; }
        public string DescripcionArticulo { get; set; }
        public int NumeroPiezas { get; set; }
        public string Area { get; set; }
        public string Cliente { get; set; }
        public string Articulo { get; set; }

    }
    // Modelo para el elemento principal
    public class PedidoPrincipal
    {
        public string OrdenVenta { get; set; }
        public string Huacal { get; set; }
        public string Area { get; set; }
        public string Articulo { get; set; } // Inicialmente vacío
        public int Piezas { get; set; } // Se llenará después
        public string Cliente { get; set; } // Se llenará después
        public List<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }

    // Modelo para los detalles
    public class DetallePedido
    {
        public string Huacal { get; set; }
        public string Descripcion { get; set; }
        public int Piezas { get; set; }
    }
}