using Rastreo.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Rastreo.Utilerias;
using static Rastreo.Models.ModeloRuta;
using System.Data;


namespace Rastreo.Controllers
{
    public class LogicaPedidoController : Controller
    {
        
   
        ///////////////////////Parte de Rastreo/////////////////////////////

        public ActionResult ObtenerRutasYRepartidores()
        {
            List<RutaRepartidor> ListaRutasRepartidores = new List<RutaRepartidor>();
            try
            {
                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();
                    using (SqlCommand comandoSQL = new SqlCommand("SP_ObtenerRutasYRepartidores", conexionSQL))
                    {
                        comandoSQL.CommandType = CommandType.StoredProcedure;
                        using (SqlDataReader dataReader = comandoSQL.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                ListaRutasRepartidores.Add(new RutaRepartidor()
                                {
                                    RutaCode = dataReader.IsDBNull(0) ? "" : dataReader.GetString(0),  // Código de la ruta
                                    RutaName = dataReader.IsDBNull(1) ? "" : dataReader.GetString(1),  // Nombre de la ruta (U_Ruta)
                                    RepartidorCode = dataReader.IsDBNull(2) ? "" : dataReader.GetString(2),
                                    RepartidorName = dataReader.IsDBNull(3) ? "" : dataReader.GetString(3),
                                    Estatus = dataReader.IsDBNull(4) ? "" : dataReader.GetString(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerRutasYRepartidores");
            }

            return Json(ListaRutasRepartidores, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GuardarRutas(List<CambioRuta> cambios)
        {
            try
            {
                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();
                    foreach (var cambio in cambios)
                    {
                        using (SqlCommand comandoSQL = new SqlCommand("RutasGuardado", conexionSQL))
                        {
                            comandoSQL.CommandType = CommandType.StoredProcedure;
                            comandoSQL.Parameters.AddWithValue("@RepartidorCode", cambio.RepartidorCode);
                            comandoSQL.Parameters.AddWithValue("@RutaName", cambio.RutaName);
                            comandoSQL.Parameters.AddWithValue("@Estatus", cambio.Estatus);
                            comandoSQL.ExecuteNonQuery();
                        }
                    }
                }

                return Json(new { success = true, message = "Cambios realizados con éxito." });
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "LogicaPedido.GuardarRutas");
                return Json(new { success = false, message = "Error al guardar los cambios: " + ex.Message });
            }
        }
    }
}