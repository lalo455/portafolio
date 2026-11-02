using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMs.Models
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
        public decimal Credito { get; set; }
        public decimal Saldo { get; set; }
        public string EstatusMetodPago { get; set; }

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
        public decimal Comprometido { get; set; }
        public string UnitSize { get; set; }
        public string Price { get; set; }
        public string PrecioMostrador { get; set; }
        public string WhereHose { get; set; }
        public string ItemGroup { get; set; }
        public string CardCode { get; set; }
        public string Substitute { get; set; }
        public decimal Impuesto { get; set; }
        public int U_VtasPiezas { get; set; }
        //NUEVO
        public string U_Leyenda { get; set; }
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

    public class DetallePedido
    {
        public int IdRowDetalle { get; set; }
        public int DocEntry { get; set; }
        public string ItemCode { get; set; }
        public string Descripcion { get; set; }
        public float Cantidad { get; set; }
        public string Almacen { get; set; }
        public string Comentarios { get; set; }
        public string Estado { get; set; }
        public float Prcio { get; set; }
        public float Tax { get; set; }
        public float Total { get; set; }
        public int BaseEntry { get; set; }
        public string Espe { get; set; }
        public string Name { get; set; }
        public string EntregaVta { get; set; }
        public string EntregaEsti { get; set; }
        public string Clave { get; set; }
        public string DescLarga { get; set; }
        public string DiasEntrega { get; set; }
        

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
        public int DocFactura { get; set; }
        public string FechaCont { get; set; }
        public string Guia { get; set; }
        public string Estado { get; set; }//Facturado,Cancelado,Pendiente
    }
    public class ConsultaFactura { 
        public int NoPedidoSAP { get; set; }
        public string DocDate { get; set; }
        public string FechaCont { get; set;}
        public string Cliente { get; set; }
        public int DocFactura { get; set; }
        public int EntryFactura { get; set; }
        public int DocEntry { get; set; }
        public string Estado { get; set; }
        public float Total { get; set; }
        public float TotalsinIva { get; set; }
        public string rutaQr { get; set; }

    }
    public class ConsultaCotizacion
    {
        public int NoCotizacion { get; set; }
        public string DocStatus { get; set; }
        public string DocDate { get; set; }
        public string Cliente { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int QtyCotizada { get; set; }
        public string UMVendida { get; set; }
        public decimal Precio { get; set; }
        public decimal Descuento1 { get; set; }
        public decimal Descuento2 { get; set; }
        public int LineNum { get; set; }
        public string UnitSize { get; set; }
        public double IVA { get; set; }
        public decimal Total { get; set; }
        public string Direccion { get; set; }
        public string Folio { get; set; }
        public string Comentarios { get; set; }
        public string FechaEntrega { get; set; }
        public string VATLiable { get; set; }
        public string TaxCodeAR { get; set; }
        public string IndirectTax { get; set; }
        public decimal ValorImpuesto { get; set; }
        public string Almacen { get; set; }

        public string WhsCode { get; set; }
        public decimal comprometido { get; set; }
        public string Envio { get; set; }
        public decimal stock { get; set; }
        public string ShipDate { get; set; }
        public string Fol { get; set; }
        public string Tipo { get; set; }
        public string Paqueteria { get; set; }
        public string MetodoPago { get; set; }
        public string FormaPago { get; set; }
        public string CDFI { get; set; }


    }
    public class ConsultaArticulo
    {
        public string WhsCode { get; set; }
        public string WhsName { get; set; }
        public decimal OnHandQty { get; set; }
        public decimal IsCommited { get; set; }
        public decimal OnOrder { get; set; }
        public decimal Available { get; set; }
        public string SalUnitMsr { get; set; }
        public string InvntryUom { get; set; }
        public string Ubicacion { get; set; }
    }
    public class PDFObject
    {
        public string streamPdf { get; set; }
    }

    public class Direccion {
        public string Address { get; set; }
        public string Street {get; set; }
        public string Block {get; set; }
        public string ZipCode {get; set; }
        public string City {get; set; }
        public string Country {get; set; }
        public string State {get; set; }
    }
    public class Inventario {

        public string whsCode { get; set; }
        public string Almacen { get; set; }
        
        public decimal stock { get; set; }
        public decimal comprometido { get; set; }
    }
    public class stockInd
    {

      

        public decimal Stock { get; set; }
    }
    public class VistaPrevia
    {
      
        public string DocNum { get; set; }
    }
    public class FormasPago
    {
        public string PymCode { get; set; }
        public string Descrip { get; set; }

    }
    public class CDFI
    {
        public string Code { get; set; }
        public string Descrip { get; set; }

    }
    public class FormaEnvio
    {
        public string Tipo { get; set; }
        public string Documento { get; set; }

    }
    public class MetodoPago
    {
        public string Tipo { get; set; }
        public string Documento { get; set; }

    }
    public class TipoEncio
    {
        public string Tipo { get; set; }
        public string Documento { get; set; }

    }
    public class Formato
    {
        public string U_PathReport { get; set; }
        public string Documento { get; set; }

    }

    public class FechaFactura
    {
        public string U_Anio { get; set; }
        public string Anio { get; set; }

    }

}