using System;
using SQLite;

namespace EVIRFID.Models
{
    [Table("item")]
    public class Item
    {
        [AutoIncrement, PrimaryKey]
        public int idItem { get; set; }
        public string prenda { get; set; }
        public string epc { get; set; }
        public string sxtid { get; set; }
        public string claveCorporacion { get; set; }
        public string talla { get; set; }
        public int idPersona { get; set; }
        public string nombre { get; set; }
        public string fechaAsignacion { get; set; }
    }
    public class Epc
    {
        public string epc { get; set; }


    }

}