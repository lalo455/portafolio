using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Rastreo.Controllers
{
    public class ConexionController : Controller
    {
        private Company _company;

        // Constructor para inicializar la conexión con SAP
        public ConexionController()
        {
            _company = new Company
            {
                Server = "YOUR_SERVER",   // Servidor de SAP B1
                CompanyDB = "YOUR_COMPANY_DB", // Base de datos de SAP B1
                DbServerType = BoDataServerTypes.dst_MSSQL2019, // Tipo de servidor
                UserName = "YOUR_SAP_USER",       // Usuario de SAP B1
                DbPassword = "YOUR_DB_PASSWORD",    // Contraseña de SAP B1
                language = BoSuppLangs.ln_Spanish_La, // Idioma de SAP
                UseTrusted = false,            // Autenticación con usuario y contraseña
                DbUserName = "YOUR_SAP_USER",     // Usuario SQL (si es necesario)
                Password = "YOUR_PASSWORD", // Contraseña SQL (si es necesario)
                LicenseServer = "YOUR_SERVER", // Servidor de licencias de SAP
                SLDServer = "YOUR_SERVER", // Servidor de licencias de SAP
            };
        }

        // Acción para conectar a SAP
        public ActionResult ConectarSAP()
        {
            try
            {
                int resultado = _company.Connect();
                if (resultado != 0)
                {
                    string error = _company.GetLastErrorDescription();
                    return Json(new { success = false, message = "Error de conexión: " + error }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = true, message = "Conectado a SAP Business One exitosamente" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Excepción: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DesconectarSAP()
        {
            try
            {
                if (_company.Connected)
                {
                    _company.Disconnect();
                    return Json(new { success = true, message = "Desconectado de SAP Business One" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false, message = "No hay conexión activa con SAP" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Excepción: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_company != null)
            {
                if (_company.Connected)
                {
                    _company.Disconnect();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_company);
                _company = null;
            }
            base.Dispose(disposing);
        }
    }
}