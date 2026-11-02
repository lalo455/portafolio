using System;
using SQLite;

namespace EVIRFID.Models
{
    [Table("persona")]
    public class Person
    {
        [AutoIncrement, PrimaryKey]
        public int idPersona { get; set; }
        public string nombre { get; set; }
        public string adscripcion { get; set; }
        public string cuip { get; set; }
        public string iniciales
        {
            get
            {
                string initials = "";
                for (int i = 1; i < this.nombre.Length - 1; i++)
                    if (this.nombre[i] == ' ' && initials.Length<3)
                        initials += Char.ToUpper(this.nombre[i + 1]);
                return initials;
            }
        }
        public string fechaCalzadoEntregado { get; set; }
        public string fechaCinturonEntregado { get; set; }
        public string fechaKepiEntregado { get; set; }
        public string fechaPantalonEntregado { get; set; }
        public string fechaImpermeableEntregado { get; set; }
        public string fechaGorraEntregado { get; set; }
        public string tipoSangre { get; set; }
        public bool calzadoEntregado { get; set; }
        public bool cinturonEntregado { get; set; }
        public bool kepiEntregado { get; set; }
        public bool pantalonEntregado { get; set; }
        public bool impermeableEntregado { get; set; }
        public bool gorraEntregado { get; set; }
        public int itemsAsignados { get; set; }
        public string fechaUltimaAtencion { get; set; }
        public string observaciones { get; set; }
    }

    public class ExcelPerson
    {
        public string NOMBRE { get; set; }
        public string ADSCRIPCION { get; set; }
        public string CUIP { get; set; }
        public string TIPOSANGRE { get; set; }
    }
}
