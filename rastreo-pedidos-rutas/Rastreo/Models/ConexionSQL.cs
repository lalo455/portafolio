using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Rastreo.Models
{
    public class ConexionSQL
    {
        public string Cadena()
        {
            string TipoBD = string.Empty;
            string ReturnCadena = string.Empty;
            ReturnCadena = "Data Source=" + ConfigurationManager.AppSettings["Server"] + ";Initial Catalog=" + ConfigurationManager.AppSettings["DataBase"] + ";User Id=" + ConfigurationManager.AppSettings["UserId"] + ";Password=" + ConfigurationManager.AppSettings["Password"];
            return ReturnCadena;
        }
    }
}