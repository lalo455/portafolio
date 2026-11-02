using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WMS.Models
{
    public class Menu
    {
        public class Modulos
        {
            public string Traspasos { get; set; }
            public string Remision { get; set; }
            public string Env_Produccion { get; set; }
            public string Validacion_Prod { get; set; }
            public string Recuento_Inv { get; set; }
            public string Val_de_Embarque { get; set; }
            public string Calidad { get; set; }
            public string Traspaleo { get; set; }
            public string Recibo_Prod { get; set; }
            public string Etq_Master { get; set; }
            public string Scrap { get; set; }
            public string TiempoMuerto { get; set; }
            public string Consulta { get; set; }
        }
    }
}