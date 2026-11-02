using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rastreo.Models
{
    public class ModeloRuta
    {
        public class Ruta
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }
        public class Repartidor
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }
        public class RutaAsignada
        {
            public string RepartidorCode { get; set; }
            public string RutaCode { get; set; }
            public string RutaName { get; set; }
            public string Estatus { get; set; }
        }
        public class RutaRepartidor
        {
            public string RutaCode { get; set; }  // Código de la ruta
            public string RutaName { get; set; }  // Nombre de la ruta (U_Ruta)
            public string RepartidorCode { get; set; }
            public string RepartidorName { get; set; }
            public string Estatus { get; set; }
        }
        // Clase para representar los cambios
        public class CambioRuta
        {
            public string RepartidorCode { get; set; }
            public string RutaName { get; set; }
            public string Estatus { get; set; }  // Y o N
        }

        public class PedidosRuta
        {
            public string RutaName { get; set; }
            public string DocEntry { get; set; }
            public string DocNum { get; set; }

            public string CardCode { get; set; }

            public string CardName { get; set; }

            public string CardFName { get; set; }
        }

    }
}