using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMs.Models
{
    public class ConsultaVentasAnio
    {
        public int Factura { get; set; }
        public int Pedido { get; set; }
        public string FechaFactura { get; set; }
        public string VencimientoFactura { get; set; }
        public string CodigoCliente { get; set; }
        public decimal ImporteTotal { get; set; }
        public int MesFactura { get; set; }
    }
    public class PorcentajeVentas
    {
        
        public decimal PorcentajeVenta { get; set; }
        
    }

}