using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMs.Models
{
    public class ConsultaVentas
    {
        public List<ConsultaVentasMes> VentasMes { get; set; }
        public List<ConsultaVentasAnio> VentasAnio { get; set; }
    }
}