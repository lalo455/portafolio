using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WMS.Logic.Comun;
using WMS.Models;

namespace WMS.Controllers
{
    public class ImpresionController : Controller
    {
        Utilities.Log Logs = new Utilities.Log();
        Obtener Ob = new Obtener();
        private SAP modeloSAp;
        string Cadena;

        public Stream GenerarImpresion(string Codigo)
        {
            try
            {
                SAP param = modeloConexionWms();
                string Server = $"{param.Server}";
                string Base = param.Database;
                string UserDB = param.UserId;
                string Pass = param.Password;

                string Ruta = "";
                string pathEtq = ConfigurationManager.AppSettings["RutaMaster"]; //Obtener la Ruta de la Etiqueta a Imprimir

                //Existe la Ruta
                if (pathEtq != null || pathEtq != "")
                {
                    Ruta = pathEtq;
                }

                string respuesta = string.Empty;
                ReportDocument Reporte = new ReportDocument();
                Reporte.Load(Ruta);
                Reporte.Refresh();
                Reporte.SetParameterValue("@CodigoMaster", Codigo);

                OdbcConnectionStringBuilder odbcConnection = new OdbcConnectionStringBuilder
                {
                    ConnectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["WmsSQL"])
                };

                CrystalReportsUtilerias.RecurseAndRemap(Reporte, Server, Base, UserDB, Pass);
                return Reporte.ExportToStream(ExportFormatType.PortableDocFormat);
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes("Se generó un error al momento de procesar el documento PDF.");
                MemoryStream memroryS = new MemoryStream(bytes);
                return memroryS;
            }
        }

        public SAP modeloConexionWms()
        {
            try
            {
                Cadena = ConfigurationManager.ConnectionStrings["WmsSQL"].ConnectionString;
                String[] substrings = Cadena.Split(';');
                modeloSAp = new SAP()
                {
                    Server = substrings[0].Split('=')[0] == "Data Source" ? substrings[0].Split('=')[1] : "",
                    Database = substrings[1].Split('=')[0] == "Initial Catalog" ? substrings[1].Split('=')[1] : "",
                    UserId = substrings[2].Split('=')[0] == "User Id" ? substrings[2].Split('=')[1] : "",
                    Password = substrings[3].Split('=')[0] == "Password" ? substrings[3].Split('=')[1] : ""
                };
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
            }
            return modeloSAp;
        }
    }
}