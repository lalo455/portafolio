using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Rastreo.Filters;
using Rastreo.Models;
using Rastreo.Utilerias;
using static Rastreo.Models.ModeloRuta;
using Rastreo.Utilities;
using System.Reflection;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.IO;
namespace Rastreo.Controllers
{
    [AuthorizeRole("R")]
    public class PedidoController : Controller
    {
        // GET: Pedido
        #region Vistas
        public ActionResult PedidosAsigandos()
        {
            try
            {
                var Verificar = ((ClaimsIdentity)User.Identity).Claims.First().Value;
                if (String.IsNullOrEmpty(Verificar))
                {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {

                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Login");
            }
        }
        public ActionResult DetallePedido(string docNum)
        {
            try
            {
                var Verificar = ((ClaimsIdentity)User.Identity).Claims.First().Value;
                if (String.IsNullOrEmpty(Verificar))
                {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    ViewBag.DocNum = docNum; // Pasar el DocNum a la vista
                    return View();
                }
            }
            catch (Exception ex)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult RutasPedidos(string nombreRuta)
        {
            try
            {
                var Verif = ((ClaimsIdentity)User.Identity).Claims.First().Value;
                ViewBag.NombreRuta = nombreRuta;
                if (Verif == "")
                {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Login");
                }
                else
                    return View();
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Pedido.RutasPedidos");
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Login");
            }
        }
        #endregion
     
        #region [Acciones]
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ObtenerOrdenesVentaPorRepartidorYRuta(string ruta, string estatus)
        {
            List<PedidosRuta> ListaPedidosRuta = new List<PedidosRuta>();

            try
            {
                int idRepartidor = Convert.ToInt16(((ClaimsIdentity)User.Identity).Claims.First().Value);
                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();
                    using (SqlCommand comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = "SP_ObtenerOrdenesVentaPorRepartidorYRuta";
                        comandoSQL.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter taxCodeParameter = comandoSQL.CreateParameter();
                        taxCodeParameter.ParameterName = "Repartidor";
                        taxCodeParameter.DbType = System.Data.DbType.String;
                        taxCodeParameter.Value = idRepartidor;
                        comandoSQL.Parameters.Add(taxCodeParameter);

                        SqlParameter userCodeParameter = comandoSQL.CreateParameter();
                        userCodeParameter.ParameterName = "Ruta";
                        userCodeParameter.DbType = DbType.String;
                        userCodeParameter.Value = ruta;
                        comandoSQL.Parameters.Add(userCodeParameter);

                        SqlParameter estatusParameter = comandoSQL.CreateParameter();
                        estatusParameter.ParameterName = "Estatus";
                        estatusParameter.DbType = DbType.String;
                        estatusParameter.Value = estatus;
                        comandoSQL.Parameters.Add(estatusParameter);

                        using (SqlDataReader dataReaderTax = comandoSQL.ExecuteReader())
                        {
                            try
                            {
                                while (dataReaderTax.Read())
                                {
                                    ListaPedidosRuta.Add(new PedidosRuta()
                                    {
                                        DocEntry = !dataReaderTax.IsDBNull(0) ? dataReaderTax[0].ToString() : string.Empty,
                                        DocNum = dataReaderTax.IsDBNull(1) ? string.Empty : dataReaderTax[1].ToString(),
                                        CardCode = dataReaderTax.IsDBNull(2) ? string.Empty : dataReaderTax.GetString(2),
                                        CardName = dataReaderTax.IsDBNull(3) ? string.Empty : dataReaderTax.GetString(3),
                                        CardFName = dataReaderTax.IsDBNull(4) ? string.Empty : dataReaderTax.GetString(4)
                                    }
                                   );
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtility.LogException(ex, "Pedido.ObtenerOrdenesVentaAbiertosPorRepartidorYRuta");
                                return Json(ListaPedidosRuta, JsonRequestBehavior.AllowGet);
                            }
                            finally
                            {
                                if (dataReaderTax != null)
                                {
                                    dataReaderTax.Close();
                                    comandoSQL.Dispose();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Pedido.ObtenerOrdenesVentaAbiertosPorRepartidorYRuta");
            }

            return Json(ListaPedidosRuta, JsonRequestBehavior.AllowGet);
        }




        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ObtenerRutasAsignadasPorRepartidor(string estatus)
        {
            List<RutaAsignada> ListaDeRutas = new List<RutaAsignada>();

            try
            {
                int idRepartidor = Convert.ToInt16(((ClaimsIdentity)User.Identity).Claims.First().Value);
                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();
                    using (SqlCommand comandoSQL = conexionSQL.CreateCommand())
                    {

                        comandoSQL.CommandText = "SP_ObtenerRutasPorRepartidor";
                        comandoSQL.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter taxCodeParameter = comandoSQL.CreateParameter();
                        taxCodeParameter.ParameterName = "Repartidor";
                        taxCodeParameter.DbType = System.Data.DbType.String;
                        taxCodeParameter.Value = idRepartidor;
                        comandoSQL.Parameters.Add(taxCodeParameter);

                        SqlParameter estCodeParameter = comandoSQL.CreateParameter();
                        estCodeParameter.ParameterName = "Estatus";
                        estCodeParameter.DbType = System.Data.DbType.String;
                        estCodeParameter.Value = estatus;
                        comandoSQL.Parameters.Add(estCodeParameter);

                        using (SqlDataReader dataReaderTax = comandoSQL.ExecuteReader())
                        {
                            try
                            {
                                while (dataReaderTax.Read())
                                {
                                    ListaDeRutas.Add(new RutaAsignada()
                                    {
                                        RutaName = dataReaderTax.IsDBNull(0) ? "" : dataReaderTax.GetString(0)
                                    }
                                   );
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerImpuestoDeSAP");
                                return Json(ListaDeRutas, JsonRequestBehavior.AllowGet);
                            }
                            finally
                            {
                                if (dataReaderTax != null)
                                {
                                    dataReaderTax.Close();
                                    comandoSQL.Dispose();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Rutas.ObtenerRutasAsignadasPorRepartidor");
            }

            return Json(ListaDeRutas, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult ObtenerDetallePedido(int docNum)
        {
            List<DetallePedido> detallePedido = new List<DetallePedido>();

            try
            {
                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();
                    using (SqlCommand comandoSQL = new SqlCommand("SP_DetallePedido", conexionSQL))
                    {
                        comandoSQL.CommandType = CommandType.StoredProcedure;
                        comandoSQL.Parameters.AddWithValue("@DocNum", docNum);

                        using (SqlDataReader dataReader = comandoSQL.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                detallePedido.Add(new DetallePedido
                                {
                                    ItemCode = dataReader.IsDBNull(2) ? "" : dataReader.GetString(2),
                                 
                                    Dscription = dataReader.IsDBNull(3) ? "" : dataReader.GetString(3),
                                    Cantidad = dataReader.IsDBNull(4) ? 0 : dataReader.GetDecimal(4),
                                    CantidadEntregada = dataReader.IsDBNull(6) ? 0 : dataReader.GetDecimal(6), // Nueva columna
                                    CantidadPendiente = dataReader.IsDBNull(5) ? 0 : dataReader.GetDecimal(5),
                                    Precio = dataReader.IsDBNull(7) ? 0 : dataReader.GetDecimal(7),
                                    TotalLinea = dataReader.IsDBNull(8) ? 0 : dataReader.GetDecimal(8),
                                    IvaLinea = dataReader.IsDBNull(10) ? 0 : dataReader.GetDecimal(10),
                                    DocEntry = !dataReader.IsDBNull(11) ? dataReader[11].ToString() : string.Empty,
                                    Orden = dataReader.IsDBNull(12) ? "" : dataReader.GetString(12),
                                    Entregado = false // Por defecto, no está entregado
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejar el error
            }

            return Json(detallePedido);
        }

        [HttpPost]
        public JsonResult ActualizarEstadoEntrega(DetallePedido item)
        {
            try
            {
                // Lógica para actualizar el estado de entrega en la base de datos
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        private readonly IntegracionSAPController _integracionSapController;

        // Constructor sin parámetros(necesario para la activación del controlador)
        public PedidoController()
        {
            _integracionSapController = new IntegracionSAPController(); // Inicialización manual
        }

    
      [HttpPost]
        public JsonResult EntregarPedido(int docEntryPedido, List<DetallePedido> detalles)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            var identity = (ClaimsIdentity)User.Identity;
            string idRepartidor = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            Logs log = new Logs();
            log.Info($"Inicio de EntregarPedido. DocEntryPedido: {docEntryPedido}, Repartidor: {idRepartidor}", modulo, metodo);

            try
            {
                // Llamar al controlador de IntegracionSAP para crear la orden de entrega
                var resultado = _integracionSapController.GenerarOrdenDeEntrega(docEntryPedido, detalles, idRepartidor);

                if (resultado.success)
                {
                    log.Info($"Éxito en EntregarPedido. DocEntryPedido: {docEntryPedido}, DocEntryEntrega: {resultado.docEntry}", modulo, metodo);

                    // Generar la URL del PDF del ticket de entrega
                    var pdfUrl = Url.Action("GenerarTicketEntrega", "Pedido", new { docEntry = resultado.docEntry });

                    return Json(new { success = true, docEntry = resultado.docEntry, pdfUrl = pdfUrl });
                }
                else
                {
                    log.Error($"Error en EntregarPedido. Mensaje: {resultado.message}", modulo, metodo);
                    return Json(new { success = false, message = resultado.message });
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, modulo, metodo);
                return Json(new { success = false, message = ex.Message });
            }
            finally
            {
                log.Info($"Fin de EntregarPedido. DocEntryPedido: {docEntryPedido}", modulo, metodo);
            }
        }
        [HttpGet]

        //public ActionResult GenerarTicketEntrega(int docEntry)
        //{
        //    string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
        //    string metodo = MethodBase.GetCurrentMethod().Name;

        //    Logs log = new Logs();
        //    log.Info($"Inicio de GenerarTicketEntrega. DocEntry: {docEntry}", modulo, metodo);

        //    try
        //    {
        //        // Cargar el Reporte
        //        log.Debug("Cargando el reporte de Crystal Reports.", modulo, metodo);
        //        ReportDocument reporte = new ReportDocument();

        //        // Ruta del reporte de Crystal Reports
        //        string rutaReporte = Server.MapPath("~/Reportes/FormatoTicket.rpt");
        //        if (!System.IO.File.Exists(rutaReporte))
        //        {
        //            log.Error($"El archivo de reporte no existe en la ruta: {rutaReporte}", modulo, metodo);
        //            return Json(new { success = false, message = "El archivo de reporte no existe." }, JsonRequestBehavior.AllowGet);
        //        }

        //        reporte.Load(rutaReporte);

        //        // Configurar el parámetro del reporte (DocEntry)
        //        reporte.SetParameterValue("DocKey@", docEntry);

        //        // Configurar la conexión a la base de datos
        //        log.Debug("Configurando la conexión a la base de datos para Crystal Reports.", modulo, metodo);
        //        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString);
        //        foreach (Table table in reporte.Database.Tables)
        //        {
        //            TableLogOnInfo logonInfo = table.LogOnInfo;
        //            logonInfo.ConnectionInfo.ServerName = builder.DataSource;
        //            logonInfo.ConnectionInfo.DatabaseName = builder.InitialCatalog;
        //            logonInfo.ConnectionInfo.UserID = builder.UserID;
        //            logonInfo.ConnectionInfo.Password = builder.Password;
        //            table.ApplyLogOnInfo(logonInfo);
        //        }

        //        // Exportar a Stream como PDF
        //        log.Debug("Exportando el reporte a PDF.", modulo, metodo);
        //        Stream stream = reporte.ExportToStream(ExportFormatType.PortableDocFormat);

        //        // Devolver el archivo PDF
        //        log.Info($"PDF generado correctamente. DocKey@: {docEntry}", modulo, metodo);
        //        return File(stream, "application/pdf", $"TicketEntrega_{docEntry}.pdf");
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex, modulo, metodo);
        //        return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //    finally
        //    {
        //        log.Info($"Fin de GenerarTicketEntrega. DocKey@: {docEntry}", modulo, metodo);
        //    }
        //}
        public ActionResult GenerarTicketEntrega(int docEntry)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;

            Logs log = new Logs();
            log.Info($"Inicio de GenerarTicketEntrega. DocEntry: {docEntry}", modulo, metodo);

            // 1. DECLARAR AFUERA PARA PODER DESTRUIRLO EN EL FINALLY
            ReportDocument reporte = null;

            try
            {
                log.Debug("Cargando el reporte de Crystal Reports.", modulo, metodo);
                reporte = new ReportDocument();

                string rutaReporte = Server.MapPath("~/Reportes/FormatoTicket.rpt");
                if (!System.IO.File.Exists(rutaReporte))
                {
                    log.Error($"El archivo de reporte no existe en la ruta: {rutaReporte}", modulo, metodo);
                    return Json(new { success = false, message = "El archivo de reporte no existe." }, JsonRequestBehavior.AllowGet);
                }

                reporte.Load(rutaReporte);
                reporte.SetParameterValue("DocKey@", docEntry);

                log.Debug("Configurando la conexión a la base de datos para Crystal Reports.", modulo, metodo);
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString);
                foreach (Table table in reporte.Database.Tables)
                {
                    TableLogOnInfo logonInfo = table.LogOnInfo;
                    logonInfo.ConnectionInfo.ServerName = builder.DataSource;
                    logonInfo.ConnectionInfo.DatabaseName = builder.InitialCatalog;
                    logonInfo.ConnectionInfo.UserID = builder.UserID;
                    logonInfo.ConnectionInfo.Password = builder.Password;
                    table.ApplyLogOnInfo(logonInfo);
                }

                log.Debug("Exportando el reporte a PDF.", modulo, metodo);

                // 2. EXPORTAR Y COPIAR A UN MEMORY STREAM INDEPENDIENTE
                using (Stream crystalStream = reporte.ExportToStream(ExportFormatType.PortableDocFormat))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    crystalStream.CopyTo(memoryStream);
                    memoryStream.Position = 0; // Regresar el cursor al inicio para que MVC pueda leerlo

                    log.Info($"PDF generado correctamente. DocKey@: {docEntry}", modulo, metodo);

                    // Retornamos el memoryStream (MVC se encarga de cerrarlo al terminar la descarga)
                    return File(memoryStream, "application/pdf", $"TicketEntrega_{docEntry}.pdf");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, modulo, metodo);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                // 3. ¡AQUÍ MATAMOS LA FUGA DE MEMORIA!
                if (reporte != null)
                {
                    reporte.Close();
                    reporte.Dispose();
                }

                log.Info($"Fin de GenerarTicketEntrega. DocKey@: {docEntry}", modulo, metodo);
            }
        }

    }
    #endregion
}