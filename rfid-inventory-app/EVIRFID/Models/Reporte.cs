using System;
using SQLite;

namespace EVIRFID.Models
{
    [Table("reporte")]
    public class Reporte
    {
        [AutoIncrement, PrimaryKey]
        public int NO { get; set; }
        public string FOLIO { get; set; }
        public string NOMBRE { get; set; }
        public string DESCRIPCION { get; set; }
        public string TALLA { get; set; }
        public string CUIP { get; set; }
        public string tipoSangre { get; set; }
        public string ADSCRIPCION { get; set; }
        public string CHIP { get; set; }
        public string FECHA { get; set; }
        public string observaciones { get; set; }
        public string TIPO { get; set; }
        public string ORIGEN { get; set; }
        public string PROVEEDOR { get; set; }
        public string PROPIETARIO { get; set; }
    }
}