using System.Collections.Generic;

namespace Rastreo.Models
{
    public class Cabecera
    {
        public string Folio { get; set; }
        public string Ruta { get; set; }
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public string Liquido { get; set; }
        public int Repartidor { get; set; }
        public decimal TotalPiezasContado { get; set; }
        public decimal TotalPiezasCredito { get; set; }
        public decimal TotalImporteContado { get; set; }
        public decimal TotalImporteCredito { get; set; }
        public bool EsRegistrado { get; set; } = false;
    }

    public class Detalle
    {
        public int DocEntry { get; set; }
        public int DocNum { get; set; }
        public int LineNum { get; set; }
        public string CardName { get; set; }
        public string CardCode { get; set; }
        public decimal PiezasTotales { get; set; }
        public decimal DocTotal { get; set; }
        public string TipoPago { get; set; }
        public decimal PiezasContado { get; set; }
        public decimal PiezasCredito { get; set; }
        public decimal ImporteContado { get; set; }
        public decimal ImporteCredito { get; set; }
    }

    public class CorteCajaModel
    {
        public string Cabecera { get; set; }
        public Cabecera Encabezado { get; set; }
        public List<Detalle> Detalle { get; set; }
    }
}