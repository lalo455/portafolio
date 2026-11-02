using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WMS.Models
{
    public class TPM
    {
        public class Comun
        {
            //Maquinas
            public string Recurso { get; set; }

            //Datos OF
            public string DocNum { get; set; }
            public string DocEntry { get; set; }
            public string Almacen { get; set; }
            public string Status { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string Maquina { get; set; }
            public string Molde { get; set; }

            //Tipo de TPM
            public string IdS { get; set; }
            public string TipoS { get; set; }

            public string Categoria { get; set; }
            public string Usuario { get; set; }
            public string Correo { get; set; }
        }

        public class Detalle
        {
            public string NumOrdr { get; set; }
            public string DocNum { get; set; }
            public string DocEntry { get; set; }
            public string Almacen { get; set; }
            public string Maquina { get; set; }
            public string Molde { get; set; }
            public int IdOperador { get; set; }
            public string Operador { get; set; }
            public string Fecha { get; set; }
            public int Turno { get; set; }
            public double Total { get; set; }
            public string TipoTPM { get; set; }
            public string DescTPM { get; set; }
            public int Tiempo { get; set; }
            public string DistNumber { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string Usuario { get; set; }
            public int LineNum { get; set; }

            public string SDocEntry { get; set; }
            public int LineId { get; set; }
            public int VisOrder { get; set; }

            public string Categoria { get; set; }
            public string Correo { get; set; }
        }

        public class DetalleRegistros
        {
            public string DocNum { get; set; }
            public string DocEntry { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string Maquina { get; set; }
            public string Fecha { get; set; }
            public int Turno { get; set; }
            public int Tiempo { get; set; }
            public int Estimado { get; set; }
            public string TipoTPM { get; set; }
            public string DescTPM { get; set; }
            public string TpmStatus { get; set; }
            public string Categoria { get; set; }

            public string SDocEntry { get; set; }
            public int LibNum { get; set; }
            public int LineId { get; set; }
            public int VisOrder { get; set; }
        }
    }
}