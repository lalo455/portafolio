using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WMS.Models
{
    public class Acceso
    {
        public class Credenciales
        {
            public string Usuario { get; set; }
            public string Password { get; set; }
        }

        public class UsuarioLogeado
        {
            public string UserId { get; set; }  // Coincide con "Code" en la base de datos
            public string U_Password { get; set; }  // Coincide con "U_U_PassWord" en la base de datos
        }

        public class Datos
        {
            public static string User { get; set; }
            public static string UserId { get; set; }
        }
    }
}