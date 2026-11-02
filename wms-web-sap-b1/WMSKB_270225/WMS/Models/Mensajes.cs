using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WMS.Models
{
    public class Mensajes
    {
        public int DocEntry { get; set; }
        public int DocNum { get; set; }
        public string Mensaje { get; set; }
    }
    public class MsjArchivo
    {
        public int Codigo { get; set; }
        public string Mensaje { get; set; }
    }

    public class MsjProduccion
    {
        public bool Integrado { get; set; }
        public string DocEntry { get; set; }
        public string DocNum { get; set; }
        public int OF { get; set; }
        public string MsjError { get; set; }

    }

    public class MsjRecibo
    {
        public string DocEntry { get; set; }
        public string DocNum { get; set; }
        public string Mensaje { get; set; }
    }

    public class MensajesTPM
    {
        public int DocEntry { get; set; }
        public int DocNum { get; set; }
        public int Turno1 { get; set; }
        public int Turno2 { get; set; }
        public int Turno3 { get; set; }
        public string Mensaje { get; set; }
    }

    public class MensajesML
    {
        public string CodigoMaster { get; set; }
        public string Mensaje { get; set; }
    }

}