using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rastreo.Models;
using System.Security.Claims;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using System.Threading;
using System.Web.Security;
using Rastreo.Utilerias;
using System.Web.UI.WebControls;
using Rastreo.Filters;
using static Rastreo.Models.ModeloRuta;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace Rastreo.Controllers
{

    public class RutasController : Controller
    {
     

        // GET: Menu
        public ActionResult Rutas()
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
        public ActionResult RutasAsignadas(string estatus)
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
        #region [Acciones]
        [HttpGet]
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
                        comandoSQL.Parameters.Add(new SqlParameter("@Repartidor", SqlDbType.VarChar) { Value = idRepartidor });
                        comandoSQL.Parameters.Add(new SqlParameter("@Estatus", SqlDbType.VarChar) { Value = estatus });

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

        #endregion


    }
}