using System;
using SQLite;

namespace EVIRFID.Models
{
    [Table("usuario")]
    public class User
    {
        [AutoIncrement, PrimaryKey]
        public int idUsuario { get; set; }
        public string nombre { get; set; }
        public string pinPassword { get; set; }
        public int idAvatar { get; set; }
        public int activo { get; set; }
        public string ultimoLogin { get; set; }
        public byte[] imagen { get; set; }
    }
}
