using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rastreo.Models
{
    public class ModelosConsultas
    {
        public class UsuarioAutenticado
        {
            public string IdEmpleadoSAP { get; set; }
            public string UsuarioWeb { get; set; }
            public string PasswordWeb { get; set; }
            public string NombreEmpleado { get; set; }
            public bool AutenticacionCorrecta { get; set; }
            public bool UsuarioNoExiste { get; set; }
            public bool ContraseñaInvalida { get; set; }
            public bool validFor { get; set; }
            public List<string> Acciones { get; set; }
            public string RolWeb { get; set; }

            //public UsuarioAutenticado()
            //{
            //    IdEmpleadoSAP = "-1";
            //    AutenticacionCorrecta = false;
            //    Acciones = new List<string>();
            //}
        }
    }
}