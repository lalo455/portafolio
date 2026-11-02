using System;
using SQLite;

namespace EVIRFID.Models
{
    [Table("event")]
    public class Evento
    {
        [AutoIncrement, PrimaryKey]
        public int idEvento { get; set; }
        public string marcaDeTiempo { get; set; }
        public string motivo { get; set; }
        public string accion { get; set; }
    }
}