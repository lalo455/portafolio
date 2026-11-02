using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rastreo.Models
{
    public class ConexionSAP
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string UserIdSAP { get; set; }
        public string PasswordSAP { get; set; }
        public string ServerType { get; set; }
        public string LicenseServer { get; set; }
    }
}