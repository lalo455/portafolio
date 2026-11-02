using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WMS.Models
{
    public class Calidad : Detalle
    {
        public class Preliminar
        {
            public char Estatus { get; set; }
            public List<DetalleC> Lineas { get; set; }
        }
    }
}