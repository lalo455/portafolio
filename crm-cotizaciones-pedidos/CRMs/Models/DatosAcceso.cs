using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentValidation;

namespace CRMs.Models
{
    public class DatosAcceso
    {
        public string Usuario { get; set; }
        public string Password { get; set; }
        
        public class Validador : AbstractValidator<DatosAcceso>
        {

            public Validador()
            {                
                RuleFor(u => u.Usuario).NotNull().WithMessage("Ingrese un Usuario");
                RuleFor(u => u.Password).NotNull().WithMessage("Ingrese la Password");
            }
        }

        public class UsuarioLogeado
        {
            public string UsuarioWeb { get; set; }
            public string PasswordWeb { get; set; }
            public string SlpName { get; set; }
            public int SlpCode { get; set; }
            public bool AutenticacionCorrecta { get; set; }
            public bool UsuarioNoExiste { get; set; }
            public bool ContraseñaInvalida { get; set; }
            public bool UsuarioInactivo { get; set; }
            public string NombreInspector { get; set; }
        }
    }
    public class CredencialesRecuperadas
    {
        public string UsuarioS { get; set; }
        public string PasswordS { get; set; }
    }
}