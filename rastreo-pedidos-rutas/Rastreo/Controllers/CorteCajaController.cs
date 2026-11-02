#region [Usings]
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Newtonsoft.Json;
using Rastreo.Models;
using Rastreo.Utilerias;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
#endregion

namespace Rastreo.Controllers
{
    public class CorteCajaController : Controller
    {
        #region [Vistas]
        public ActionResult CorteCaja(string estatus, string fecha)
        {
            try
            {
                var claimsIdentity = User.Identity as ClaimsIdentity;
                var Verif = claimsIdentity?.Claims.FirstOrDefault()?.Value;

                if (string.IsNullOrEmpty(Verif))
                {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Login");
                }

                return View();
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "CorteCaja.CorteCaja");
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Login");
            }
        }

        #endregion


        #region [Acciones]
        [HttpGet]
        public ActionResult ObtenerInfoCorteCajaPorRuta(string fecha)
        {
            CorteCajaModel corteCaja = new CorteCajaModel();
            try
            {
                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();

                    var fechaDate = fecha.Split('-');
                    int idRepartidor = Convert.ToInt16(((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value);

                    using (SqlCommand comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = "SP_ObtenerDatosCorteCaja";
                        comandoSQL.CommandType = System.Data.CommandType.StoredProcedure;
                        comandoSQL.Parameters.Add(new SqlParameter("@Fecha", SqlDbType.VarChar) { Value = $"{fechaDate[2]}{fechaDate[1]}{fechaDate[0]}" });
                        comandoSQL.Parameters.Add(new SqlParameter("@Repartidor", SqlDbType.VarChar) { Value = idRepartidor });

                        using (SqlDataReader dataReaderTax = comandoSQL.ExecuteReader())
                        {
                            try
                            {
                                while (dataReaderTax.Read())
                                {
                                    var jsonResult = dataReaderTax.IsDBNull(0) ? string.Empty : dataReaderTax.GetString(0);
                                    if (jsonResult != string.Empty)
                                    {
                                        corteCaja = JsonConvert.DeserializeObject<CorteCajaModel>(jsonResult);
                                        corteCaja.Encabezado = JsonConvert.DeserializeObject<Cabecera>(corteCaja.Cabecera);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtility.LogException(ex, "CorteCaja.ObtenerInfoCorteCajaPorRuta");
                                return Json(corteCaja, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "CorteCaja.ObtenerInfoCorteCajaPorRuta");
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

            return Json(corteCaja, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GuardarCorteCaja(CorteCajaModel model)
        {
            try
            {
                model.Encabezado.Repartidor = Convert.ToInt16(((ClaimsIdentity)User.Identity).Claims.First().Value);
                string jsonData = JsonConvert.SerializeObject(model);

                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();
                    using (SqlCommand cmd = new SqlCommand("SP_InsertarCorteCaja", conexionSQL))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@json", SqlDbType.NVarChar) { Value = jsonData });

                        cmd.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "CorteCaja.ObtenerInfoCorteCajaPorRuta");
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

            return Json("Corte de caja guardado exitosamente.", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ExportarCorteCaja(string fecha)
        {
            try
            {
                CorteCajaModel corteCaja = new CorteCajaModel();
                var f3 = fecha.Split('-');
                fecha = $"{f3[2]}-{f3[1]}-{f3[0]}";

                if (!DateTime.TryParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaParsed))
                {
                    return Json(new { success = false, message = "Formato de fecha inválido." }, JsonRequestBehavior.AllowGet);
                }

                var f = new DateTime(Convert.ToInt16(f3[2]), Convert.ToInt16(f3[1]), Convert.ToInt16(f3[0]));
                var x = ((ClaimsIdentity)User.Identity).Claims;

                var idRepartidor = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.PrimarySid).Value;
                DataSet ds = ObtenerDatos(f, idRepartidor);
                if (ds == null || ds.Tables[0].Rows.Count == 0)
                {
                    return Content("No se encontraron datos para generar el reporte.");
                }

                // Cargar el Reporte
                ReportDocument reporte = new ReportDocument();
                string rutaReporte = Server.MapPath("~/Reportes/FormatoCorteCaja.rpt");
                reporte.Load(rutaReporte);
                reporte.SetDataSource(ds);
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

                // Exportar a Stream como PDF
                Stream stream = reporte.ExportToStream(ExportFormatType.PortableDocFormat);
                return File(stream, "application/pdf", $"CorteCaja_{fecha:yyyyMMdd}.pdf");

            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "CorteCaja.ObtenerInfoCorteCajaPorRuta");
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        private DataSet ObtenerDatos(DateTime fecha, string idRepartidor)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
            {
                con.Open();
                DataSet ds = new DataSet();

                // 1. LE QUITAMOS EL "AND IdRuta = @IdRuta" A LA CONSULTA
                using (SqlDataAdapter daComando = new SqlDataAdapter(
                    "SELECT * FROM VIEW_EviRastreo_CorteCaja WHERE Fecha = @Fecha AND IdRepartidor = @IdRepartidor", con))
                {
                    daComando.SelectCommand.Parameters.Add("@Fecha", SqlDbType.Date).Value = fecha;
                    daComando.SelectCommand.Parameters.Add("@IdRepartidor", SqlDbType.VarChar).Value = idRepartidor;

                    // 2. BORRAMOS LA LÍNEA QUE AGREGABA EL PARÁMETRO @IdRuta
                    // daComando.SelectCommand.Parameters.Add("@IdRuta", SqlDbType.VarChar).Value = idRuta;

                    DataTable dtComando = new DataTable();
                    daComando.Fill(dtComando);
                    ds.Tables.Add(dtComando);
                    ds.Tables[0].TableName = "Comando";
                }

                // Obtener datos de "OADM" (Esto se queda igual)
                using (SqlDataAdapter daEmpresa = new SqlDataAdapter(
                    "SELECT CompnyName AS Empresa, CompnyAddr AS DireccionEmpresa FROM OADM", con))
                {
                    DataTable dtEmpresa = new DataTable();
                    daEmpresa.Fill(dtEmpresa);
                    ds.Tables.Add(dtEmpresa);
                    ds.Tables[1].TableName = "DatosEmpresa";
                }

                return ds;
            }
        }

        #region [Modulo Administrador]

        [HttpGet]
      
        public ActionResult CorteCajaAdmin()
        {
            try
            {
                var claimsIdentity = User.Identity as ClaimsIdentity;
                var Verif = claimsIdentity?.Claims.FirstOrDefault()?.Value;

                if (string.IsNullOrEmpty(Verif))
                {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Login");
                }

                return View();
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "CorteCaja.CorteCajaAdmin");
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpGet]
        public ActionResult ObtenerRepartidores()
        {
            try
            {
                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT Code AS IdRepartidor, U_SlpName AS Nombre FROM [@USRS_REPARTIDORES] WHERE U_Rol = 'R'", conexionSQL))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var repartidores = new System.Collections.Generic.List<object>();
                            while (reader.Read())
                            {
                                repartidores.Add(new
                                {
                                    IdRepartidor = reader["IdRepartidor"].ToString(),
                                    Nombre = reader["Nombre"].ToString()
                                });
                            }
                            return Json(repartidores, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "CorteCaja.ObtenerRepartidores");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
       
        public ActionResult ExportarCorteCajaAdmin(string fecha, string idRepartidor)
        {
            try
            {
                var f3 = fecha.Split('-');
                fecha = $"{f3[2]}-{f3[1]}-{f3[0]}";

                if (!DateTime.TryParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaParsed))
                {
                    return Json(new { success = false, message = "Formato de fecha inválido." }, JsonRequestBehavior.AllowGet);
                }

                var f = new DateTime(Convert.ToInt16(f3[2]), Convert.ToInt16(f3[1]), Convert.ToInt16(f3[0]));

                DataSet ds = ObtenerDatos(f, idRepartidor);
                if (ds == null || ds.Tables[0].Rows.Count == 0)
                {
                    return Content("No se encontraron datos para generar el reporte.");
                }

                // Cargar el Reporte
                ReportDocument reporte = new ReportDocument();
                string rutaReporte = Server.MapPath("~/Reportes/FormatoCorteCaja.rpt");
                reporte.Load(rutaReporte);
                reporte.SetDataSource(ds);
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

                Stream stream = reporte.ExportToStream(ExportFormatType.PortableDocFormat);
                return File(stream, "application/pdf", $"CorteCajaAdmin_{idRepartidor}_{fecha:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "CorteCaja.ExportarCorteCajaAdmin");
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #endregion

    }
}
