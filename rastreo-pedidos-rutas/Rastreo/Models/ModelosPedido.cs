using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rastreo.Models
{
    public class ModelosPedido
    {

    }
    public class SocioDeNegocios
    {
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public int ListNum { get; set; }
        public  string Moneda { get; set; }
        public decimal Discount { get; set; }
        public string DiscInRet { get; set; }
        public string SplName { get; set; }
        public string Sucursal { get; set; }

    }
    public class Articulos
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string VATLiable { get; set; }
        public string NoDiscount { get; set; }
        public string TaxCodeAR { get; set; }
        public string IndirectTax { get; set; }
        public decimal Stock { get; set; }
        public string UnitSize { get; set; }
        public string Price { get; set; }
        public string WhereHose { get; set; }
        public string ItemGroup { get; set; }
        public string CardCode { get; set; }
        public string Substitute { get; set; }
        public decimal Impuesto { get; set; }
    }

    public class TasasDeCambio
    {
        public string RateDate { get; set; }
        public string Currency { get; set; }
        public Decimal Rate { get; set; }
    }

    public class ImpuestoBP
    {
        public string TaxCode { get; set; }
        public Decimal Rate { get; set; }
    }

    public class RespuestaConexionSAP
    {
        public int Codigo { get; set; }
        public string Error { get; set; }
    }

    public class Pedido
    {
        public string UserId { get; set; }
        public string CardCode { get; set; }
        public string DocDate {get;set;}
        public string DocDueDate { get; set; }
        public string NumAtCard { get; set; }
        public string Comentarios { get; set; }
        public string Estado { get; set; }
        public string RefInternas { get; set; }
        public string Moneda { get; set; }
        public string Contacto { get; set; }
        public string DirEntrega { get; set; }
        public string SlpCode { get; set; }

    }

    //public class DetallePedido
    //{
    //    public int IdRowDetalle { get; set; }
    //    public int DocEntry { get; set; }
    //    public string ItemCode { get; set; }
    //    public string Descripcion { get; set; }
    //    public float Cantidad { get; set; }
    //    public string Almacen { get; set; }
    //    public string Comentarios { get; set; }
    //    public string Estado { get; set; }
    //    public float Prcio { get; set; }
    //    public float Tax { get; set; }
    //    public float Total { get; set; }
    //    public int BaseEntry { get; set; }
    //    public string Espe { get; set; }
    //    public string Name { get; set; }
    //    public string EntregaVta { get; set; }
    //    public string EntregaEsti { get; set; }
    //    public string Clave { get; set; }
    //    public string DescLarga { get; set; }
    //    public string DiasEntrega { get; set; }
    //}
    public class DetallePedido
    {
        public string ItemCode { get; set; }
        public string Dscription { get; set; }
        public decimal Cantidad { get; set; }
        public decimal CantidadEditada { get; set; } // Cantidad que se va a entregar
        public decimal Precio { get; set; }
        public decimal TotalLinea { get; set; }
        public decimal IvaLinea { get; set; }
        public string DocEntry { get; set; }
        public string Orden { get; set; }
        public bool Entregado { get; set; }
        public int Linea { get; set; } // Número de línea en el pedido
        public decimal CantidadEntregada { get; internal set; }
        public object CantidadPendiente { get; internal set; }
    }

    public class ConsultaPedido
    {
        public int NoPedido { get; set; }
        public string DocDate { get; set; }
        public string DocDueDate { get; set; }
        public string Cliente { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int QtyPedida { get; set; }
        public int QtyPendiente { get; set; }
        public int QtyEntregada { get; set; }
        public int QtyFacturada { get; set; }
        public string UMVendida { get; set; }
        public decimal Precio { get; set; }
        public decimal Total { get; set; }

    }

    public class ConsultaArticulo
    {
        public string WhsCode { get; set; }
        public string WhsName { get; set; }
        public decimal OnHandQty { get; set; }
        public decimal IsCommited { get; set; }
        public decimal OnOrder { get; set; }
        public decimal Available { get; set; }
    }
    public class PDFObject
    {
        public string streamPdf { get; set; }
    }
}