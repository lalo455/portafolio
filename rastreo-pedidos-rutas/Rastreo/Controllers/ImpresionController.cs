//using Rastreo.Clases;
//using CrystalDecisions.CrystalReports.Engine;
//using CrystalDecisions.Shared;
//using System;
//using System.Configuration;
//using System.Data.Odbc;
//using System.IO;
//using System.Web.Mvc;
//using Rastreo.Utilerias;


//namespace Rastreo.Controllers
//{
//    public class Impresion : Controller
//    {
       
//        public Stream GenerarImpresion(string fInicio, string fFinal, string pEstado, string user)
//        {
           
//            try
//            {
//                ReportDocument Reporte = new ReportDocument();
//                string Ruta = ConfigurationManager.AppSettings["RptPath"];
//                string userIDweb = string.Copy(user);
//                string Server = ConfigurationManager.AppSettings["Server"];
//                string Base = ConfigurationManager.AppSettings["DataBase"];
//                string Pass = ConfigurationManager.AppSettings["Password"];
//                string UserDB = ConfigurationManager.AppSettings["UserId"];
//                string respuesta = string.Empty;
//                Random Num = new Random();
//                int numRan = Num.Next(0, 100);

//                Reporte.Load(Ruta + "ReportePedidos.rpt");
//                Reporte.Refresh();
//                Reporte.SetParameterValue("@dateFrom", fInicio);
//                Reporte.SetParameterValue("@dateTo", fFinal);
//                Reporte.SetParameterValue("@DocStatus", pEstado);
//                Reporte.SetParameterValue("@UserPortal", userIDweb);
//                OdbcConnectionStringBuilder odbcConnection = new OdbcConnectionStringBuilder
//                {
//                    ConnectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["dbCRM"])
//                };
//                CrystalReportsUtilerias.RecurseAndRemap(Reporte, Server, Base, UserDB, Pass);

//                return Reporte.ExportToStream(ExportFormatType.PortableDocFormat);

//            }
//            catch(Exception ex)
//            {
//                ExceptionUtility.LogException(ex, "Impresion.GenerarImpresion");
//                byte[] bytes = System.Text.Encoding.ASCII.GetBytes("Se generó un error al momento de procesar el documento PDF.");
//                MemoryStream memroryS = new MemoryStream(bytes);
//                return memroryS;
//            }
//        }

//        public Stream GenerarImpresion2(int DocEntry, string Tipo)
//        {
            
//            try
//            {
                
//                string Ruta = ConfigurationManager.AppSettings["RptPath"];
//                string Server = ConfigurationManager.AppSettings["Server"];
//                string Base = ConfigurationManager.AppSettings["DataBase"];
//                string Pass = ConfigurationManager.AppSettings["Password"];
//                string UserDB = ConfigurationManager.AppSettings["UserId"];
//                string respuesta = string.Empty;

//                ReportDocument Reporte = new ReportDocument();

//                Reporte.Load(Ruta + "RptPedido.rpt");
//                Reporte.Refresh();
//                Reporte.SetParameterValue("DocKey", DocEntry);
//                Reporte.SetParameterValue("Tipo", Tipo );
                
//                OdbcConnectionStringBuilder odbcConnection = new OdbcConnectionStringBuilder
//                {   
//                    ConnectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["dbCRM"])
//                };
//                CrystalReportsUtilerias.RecurseAndRemap(Reporte, Server, Base, UserDB, Pass);

//                return Reporte.ExportToStream(ExportFormatType.PortableDocFormat);
//            }
//            catch (Exception ex)
//            {
//                ExceptionUtility.LogException(ex, "Impresion.GenerarImpresion2");
//                byte[] bytes = System.Text.Encoding.ASCII.GetBytes("Se generó un error al momento de procesar el documento PDF.");
//                MemoryStream memroryS = new MemoryStream(bytes);
//                return memroryS;
//            }
//        }


//        public Stream GenerarImpresionEstadoPedido(int DocEntry, string User)
//        {
//           // ReportDocument Reporte = null;
//            try
//            {
//                string Ruta = ConfigurationManager.AppSettings["RptPath"];
//                string Server = ConfigurationManager.AppSettings["Server"];
//                string Base = ConfigurationManager.AppSettings["DataBase"];
//                string Pass = ConfigurationManager.AppSettings["Password"];
//                string UserDB = ConfigurationManager.AppSettings["UserId"];
//                string respuesta = string.Empty;

//                ReportDocument Reporte = new ReportDocument();

//                Reporte.Load(Ruta + "DetalleEstadoPedido.rpt");
//                Reporte.Refresh();
//                Reporte.SetParameterValue("DocNum", DocEntry);
//                Reporte.SetParameterValue("User", User);

//                OdbcConnectionStringBuilder odbcConnection = new OdbcConnectionStringBuilder
//                {
//                    ConnectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["dbCRM"])
//                };
//                CrystalReportsUtilerias.RecurseAndRemap(Reporte, Server, Base, UserDB, Pass);

//                return Reporte.ExportToStream(ExportFormatType.PortableDocFormat);
//            }
//            catch (Exception ex)
//            {
//                ExceptionUtility.LogException(ex, "Impresion.GenerarImpresionEstadoPedido");
//                byte[] bytes = System.Text.Encoding.ASCII.GetBytes("Se generó un error al momento de procesar el documento PDF.");
//                MemoryStream memroryS = new MemoryStream(bytes);
//                return memroryS;
//            }         
//        }


//    }
//}