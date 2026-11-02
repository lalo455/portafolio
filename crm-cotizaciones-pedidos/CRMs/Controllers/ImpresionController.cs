using CRMs.Clases;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Configuration;
using System.Data.Odbc;
using System.IO;
using System.Web.Mvc;
using CRMs.Utilerias;
using CRMs.Models;
using SAPbobsCOM;
using System.Data.SqlClient;
using Microsoft.Owin.BuilderProperties;
using Newtonsoft.Json;
using System.Web.Helpers;
using Microsoft.SqlServer.Server;
using System.Runtime.Remoting.Messaging;
using CRMs.Filters;

namespace CRMs.Controllers
{
    [Authorize]
    [NoCache]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class Impresion : Controller
    {
       
        public Stream GenerarImpresion(string fInicio, string fFinal, string pEstado, string user)
        {
            ReportDocument Reporte = null;
            try
            {
                Reporte = new ReportDocument();
                string Ruta = ConfigurationManager.AppSettings["RptPath"];
                string userIDweb = string.Copy(user);
                string Server = ConfigurationManager.AppSettings["Server"];
                string Base = ConfigurationManager.AppSettings["DataBase"];
                string Pass = ConfigurationManager.AppSettings["Password"];
                string UserDB = ConfigurationManager.AppSettings["UserId"];
                string respuesta = string.Empty;
                Random Num = new Random();
                int numRan = Num.Next(0, 100);

                Reporte.Load(Ruta + "ReportePedidos.rpt");
                Reporte.Refresh();
                Reporte.SetParameterValue("@dateFrom", fInicio);
                Reporte.SetParameterValue("@dateTo", fFinal);
                Reporte.SetParameterValue("@DocStatus", pEstado);
                Reporte.SetParameterValue("@UserPortal", userIDweb);
                OdbcConnectionStringBuilder odbcConnection = new OdbcConnectionStringBuilder
                {
                    ConnectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["dbCRM"])
                };
                CrystalReportsUtilerias.RecurseAndRemap(Reporte, Server, Base, UserDB, Pass);

                return Reporte.ExportToStream(ExportFormatType.PortableDocFormat);

            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Impresion.GenerarImpresion");
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes("Se generó un error al momento de procesar el documento PDF.");
                MemoryStream memroryS = new MemoryStream(bytes);
                return memroryS;
            }
            finally
            { 
                Reporte.Close();
                Reporte.Dispose();
            }
        }

        public Stream GenerarImpresion2(int DocEntry, string Tipo , string User)
        {
            ReportDocument Reporte = null;
            try
            {
                ExceptionUtility.FullInfo("", "Imprimir DocEntry Pedido: " + DocEntry, "Demo", 0, "Impresion", "GenerarImpresion2");

                string Ruta = ConfigurationManager.AppSettings["RptPath"];
                string Server = ConfigurationManager.AppSettings["Server"];
                string Base = ConfigurationManager.AppSettings["DataBase"];
                string Pass = ConfigurationManager.AppSettings["Password"];
                string UserDB = ConfigurationManager.AppSettings["UserId"];
                string respuesta = string.Empty;

                Reporte = new ReportDocument();
                Reporte.Load(Ruta + "RptPedido.rpt");
                Reporte.Refresh();
                Reporte.SetParameterValue("@DocKey", DocEntry);
                Reporte.SetParameterValue("@Tipo", Tipo);
                Reporte.SetParameterValue("@User", User);
                OdbcConnectionStringBuilder odbcConnection = new OdbcConnectionStringBuilder
                {
                    ConnectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["dbCRM"])
                };
                CrystalReportsUtilerias.RecurseAndRemap(Reporte, Server, Base, UserDB, Pass);

                return Reporte.ExportToStream(ExportFormatType.PortableDocFormat);
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Impresion.GenerarImpresion2");
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes("Se generó un error al momento de procesar el documento PDF.");
                MemoryStream memroryS = new MemoryStream(bytes);
                return memroryS;
            }
            finally
            {
                Reporte.Close();
                Reporte.Dispose();
            }
        }

        public Stream GenerarImpresion3(int DocEntry, string Tipo, string User)
        {
            ReportDocument Reporte = null;
            try
            {
                ExceptionUtility.FullInfo("", "Imprimir DocEntry Cotizacion: " + DocEntry, "Demo", 0, "Impresion", "GenerarImpresion3");

                string Ruta = ConfigurationManager.AppSettings["RptPath"];
                string Server = ConfigurationManager.AppSettings["Server"];
                string Base = ConfigurationManager.AppSettings["DataBase"];
                string Pass = ConfigurationManager.AppSettings["Password"];
                string UserDB = ConfigurationManager.AppSettings["UserId"];
                string respuesta = string.Empty;
               
                Reporte = new ReportDocument();
                Reporte.Load(Ruta + "RptCotizacion.rpt");
                Reporte.Refresh();
                Reporte.SetParameterValue("@DocKey", DocEntry);
                Reporte.SetParameterValue("@Tipo", Tipo);
                Reporte.SetParameterValue("@User", User);

                OdbcConnectionStringBuilder odbcConnection = new OdbcConnectionStringBuilder
                {
                    ConnectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["dbCRM"])
                };
                CrystalReportsUtilerias.RecurseAndRemap(Reporte, Server, Base, UserDB, Pass);

                return Reporte.ExportToStream(ExportFormatType.PortableDocFormat);
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Impresion.GenerarImpresion3");
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes("Se generó un error al momento de procesar el documento PDF de la Cotizacion.");
                MemoryStream memroryS = new MemoryStream(bytes);
                return memroryS;
            }
            finally
            {
                Reporte.Close();
                Reporte.Dispose();
            }
        }
        
        public Stream GenerarImpresionVistaP(string id)
        {
            ReportDocument Reporte = null;
            try
            {

                string Ruta = ConfigurationManager.AppSettings["RptPath"];
                string Server = ConfigurationManager.AppSettings["Server"];
                string Base = ConfigurationManager.AppSettings["DataBase1"];
                string Pass = ConfigurationManager.AppSettings["Password"];
                string UserDB = ConfigurationManager.AppSettings["UserId"];
                string respuesta = string.Empty;

                Reporte = new ReportDocument();
                Reporte.Load(Ruta + "RptPedidoVistaP.rpt");

                Reporte.Refresh();
                Reporte.SetParameterValue("@id", id);
     
                OdbcConnectionStringBuilder odbcConnection = new OdbcConnectionStringBuilder
                {
                    ConnectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["CRMSp"])
                };
                CrystalReportsUtilerias.RecurseAndRemap(Reporte, Server, Base, UserDB, Pass);

                return Reporte.ExportToStream(ExportFormatType.PortableDocFormat);
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Impresion.GenerarImpresionVistaP");
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes("Se generó un error al momento de procesar el documento PDF de la Vista Previa.");
                MemoryStream memroryS = new MemoryStream(bytes);
                return memroryS;
            }
            finally
            {
                Reporte.Close();
                Reporte.Dispose();
            }
        }


        public Stream GenerarImpresionEstadoPedido(int DocEntry, string User)
        {
            // ReportDocument Reporte = null;
            ReportDocument Reporte = null;
            try
            {
                string Ruta = ConfigurationManager.AppSettings["RptPath"];
                string Server = ConfigurationManager.AppSettings["Server"];
                string Base = ConfigurationManager.AppSettings["DataBase"];
                string Pass = ConfigurationManager.AppSettings["Password"];
                string UserDB = ConfigurationManager.AppSettings["UserId"];
                string respuesta = string.Empty;


                Reporte = new ReportDocument();
                Reporte.Load(Ruta + "DetalleEstadoPedido.rpt");
                Reporte.Refresh();
                Reporte.SetParameterValue("@DocNum",DocEntry);
                Reporte.SetParameterValue("@User",User);

                OdbcConnectionStringBuilder odbcConnection = new OdbcConnectionStringBuilder
                {
                    ConnectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["dbCRM"])
                };
                CrystalReportsUtilerias.RecurseAndRemap(Reporte, Server, Base, UserDB, Pass);

                return Reporte.ExportToStream(ExportFormatType.PortableDocFormat);
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Impresion.GenerarImpresionEstadoPedido");
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes("Se generó un error al momento de procesar el documento PDF.");
                MemoryStream memroryS = new MemoryStream(bytes);
                return memroryS;
            }
            finally
            { 
                Reporte.Close();
                Reporte.Dispose();
            }
        }
        
        public Stream GenerarImpresionEstadoCotizacion(int DocEntry, string User)
        {
            // ReportDocument Reporte = null;
            ReportDocument Reporte = null;
            try
            {
                string Ruta = ConfigurationManager.AppSettings["RptPath"];
                string Server = ConfigurationManager.AppSettings["Server"];
                string Base = ConfigurationManager.AppSettings["DataBase"];
                string Pass = ConfigurationManager.AppSettings["Password"];
                string UserDB = ConfigurationManager.AppSettings["UserId"];
                string respuesta = string.Empty;


                Reporte = new ReportDocument();
                Reporte.Load(Ruta + "RptCotizacion.rpt");
                Reporte.Refresh();
                Reporte.SetParameterValue("@DocKey", DocEntry);
                Reporte.SetParameterValue("@Tipo", "DocNum");

                OdbcConnectionStringBuilder odbcConnection = new OdbcConnectionStringBuilder
                {
                    ConnectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["dbCRM"])
                };
                CrystalReportsUtilerias.RecurseAndRemap(Reporte, Server, Base, UserDB, Pass);

                return Reporte.ExportToStream(ExportFormatType.PortableDocFormat);
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Impresion.GenerarImpresionEstadoCotizacion");
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes("Se generó un error al momento de procesar el documento PDF.");
                MemoryStream memroryS = new MemoryStream(bytes);
                return memroryS;
            }
            finally
            {
                Reporte.Close();
                Reporte.Dispose();
            }
        }

        public Stream GenerarImpresionEstadoFactura(int DocEntry, string RutaQr, string Vendedor)
        {
            // ReportDocument Reporte = null;
            ReportDocument Reporte = null;
            try
            {
               
                string apPath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
                string CodigoFormato = ConfigurationManager.AppSettings["CodigoFormato"];
                string Server = ConfigurationManager.AppSettings["Server"];
                string Base = ConfigurationManager.AppSettings["DataBase"];
                string Pass = ConfigurationManager.AppSettings["Password"];
                string UserDB = ConfigurationManager.AppSettings["UserId"];
                string respuesta = string.Empty;
                //LogicaPedidoController r = new LogicaPedidoController();
              
                ///////////////////////////////////////////////////////////////////////////////////pedidos////////////////////////////////////////////////////////////////////////////////////////////////
                string Ruta = ConfigurationManager.AppSettings["RptPath1"];
                //try
                //{
                //    using (SqlConnection conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))

                //    {
                //        conexionSQL.Open();
                //        using (SqlCommand comandoSQL = conexionSQL.CreateCommand())
                //        {
                //            comandoSQL.CommandText = $"Exec [dbo].[SP_Obtener_Formato] @Tipo='{CodigoFormato}'";
                //            comandoSQL.CommandType = System.Data.CommandType.Text;

                //            using (SqlDataReader reader = comandoSQL.ExecuteReader())
                //            {

                //                while (reader.Read())
                //                {
                //                    Ruta = reader.GetString(0);

                //                }
                //            }
                //        }

                //    }
                //}
                //catch (Exception ex)
                //{
                //    ExceptionUtility.LogException(ex, "ListarMetodoPago");
                //    throw;
                //}




                //se usa para acceder a la solucion con direcion del pool y asi hacer acceso a la virtual
                //string respuestaFina = System.Web.Hosting.HostingEnvironment.MapPath("~//YOUR_SERVER/b1_shr/Reportes/ECFDI_FE_4.0 CLIENTB CAT.rpt");
                ExceptionUtility.FullInfo("Integración", "", Vendedor, 3, "", "");
                ExceptionUtility.FullInfo("", Ruta, Vendedor, 0, "ReporteCotizacion", "ReporteCotizacion");

                Reporte = new ReportDocument();
        
                Reporte.Load(Ruta);
                Reporte.Refresh();
                Reporte.SetParameterValue("@DocEntry", DocEntry);
                Reporte.SetParameterValue("@ImagePath", RutaQr);
                Reporte.SetParameterValue("@Table", "INV");
                Reporte.SetParameterValue("@TableAddon", "@ECFD_EINV");

                OdbcConnectionStringBuilder odbcConnection = new OdbcConnectionStringBuilder
                {
                    ConnectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["dbCRM"])
                };
                CrystalReportsUtilerias.RecurseAndRemap(Reporte, Server, Base, UserDB, Pass);

                return Reporte.ExportToStream(ExportFormatType.PortableDocFormat);
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Impresion.GenerarImpresionEstadoFactura");
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes("Se generó un error al momento de procesar el documento PDF.");
                MemoryStream memroryS = new MemoryStream(bytes);
                return memroryS;
            }
            finally
            {
                Reporte.Close();
                Reporte.Dispose();
            }
        }

    }
}