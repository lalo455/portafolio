using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using Rastreo.Models;
using System.Security.Claims;
using System.Threading;
using Rastreo.Utilerias;

namespace Rastreo.Controllers
{
    public class ReportesController : Controller
    {
        
        [HttpPost]
        public ActionResult ConsultarPedidos(int DocNum, string UserCode, string Type, string DateOrder, string Item)
        {
            List<ConsultaPedido> ListaPedidos = new List<ConsultaPedido>();
            SqlConnection conexionPedido = null;
            SqlCommand comandoPedido = null;
            SqlDataReader datosPedido = null;
            DateTime docDate = new DateTime();
            DateTime docDue = new DateTime();
            //var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            

            try
            {
                var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                string UserId = Datos.CardCode;//Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();

                if (string.IsNullOrEmpty(UserId))
                {
                    ListaPedidos.Add(new ConsultaPedido()
                    {
                        NoPedido = -33,
                        DocDate = "nodate",
                        DocDueDate = "nodate",
                        Cliente = "Tiempo de Sesión agotado.",
                        ItemCode = "noitem",
                        ItemName = "noname",
                        QtyPedida = 0,
                        QtyPendiente = 0,
                        QtyEntregada = 0,
                        QtyFacturada = 0,
                        UMVendida = "nounit",
                        Precio = 0,
                        Total = 0
                    });

                   // return Json(ListaPedidos, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    using (conexionPedido = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                    {
                        conexionPedido.Open();
                        using (comandoPedido = conexionPedido.CreateCommand())
                        {
                            comandoPedido.CommandText = "SP_Consulta_Pedidos";
                            comandoPedido.CommandType = System.Data.CommandType.StoredProcedure;

                            SqlParameter docNumParameter = comandoPedido.CreateParameter();
                            docNumParameter.ParameterName = "DocNum";
                            docNumParameter.DbType = System.Data.DbType.Int32;
                            docNumParameter.Value = DocNum;
                            comandoPedido.Parameters.Add(docNumParameter);

                            SqlParameter userCodeParameter = comandoPedido.CreateParameter();
                            userCodeParameter.ParameterName = "UserId";
                            userCodeParameter.DbType = DbType.String;
                            userCodeParameter.Value = UserId;
                            comandoPedido.Parameters.Add(userCodeParameter);

                            SqlParameter typeParameter = comandoPedido.CreateParameter();
                            typeParameter.ParameterName = "Tipo";
                            typeParameter.DbType = DbType.String;
                            typeParameter.Value = Type;
                            comandoPedido.Parameters.Add(typeParameter);

                            SqlParameter dateOrderParameter = comandoPedido.CreateParameter();
                            dateOrderParameter.ParameterName = "Fecha";
                            dateOrderParameter.DbType = DbType.DateTime;
                            dateOrderParameter.Value = Type == "Fecha" ? DateTime.Parse(DateOrder) : DateTime.Now;
                            comandoPedido.Parameters.Add(dateOrderParameter);

                            SqlParameter itemParameter = comandoPedido.CreateParameter();
                            itemParameter.ParameterName = "Articulo";
                            itemParameter.DbType = DbType.String;
                            itemParameter.Value = Item;
                            comandoPedido.Parameters.Add(itemParameter);

                            using (datosPedido = comandoPedido.ExecuteReader())
                            {
                                while (datosPedido.Read())
                                {
                                    docDate = Convert.ToDateTime(datosPedido.GetValue(1));
                                    docDue = Convert.ToDateTime(datosPedido.GetValue(2));

                                    ListaPedidos.Add(new ConsultaPedido()
                                    {
                                        NoPedido = datosPedido.IsDBNull(0) ? 0 : Convert.ToInt32(datosPedido.GetValue(0)),
                                        DocDate = datosPedido.IsDBNull(1) ? "" : docDate.ToString("yyyy/MM/dd"),
                                        DocDueDate = datosPedido.IsDBNull(2) ? "" : docDue.ToString("yyyy/MM/dd"),
                                        Cliente = datosPedido.IsDBNull(3) ? "" : Convert.ToString(datosPedido.GetValue(3)),
                                        ItemCode = datosPedido.IsDBNull(4) ? "" : Convert.ToString(datosPedido.GetValue(4)),
                                        ItemName = datosPedido.IsDBNull(5) ? "" : Convert.ToString(datosPedido.GetValue(5)),
                                        QtyPedida = datosPedido.IsDBNull(6) ? 0 : Convert.ToInt16(datosPedido.GetValue(6)),
                                        QtyPendiente = datosPedido.IsDBNull(7) ? 0 : Convert.ToInt16(datosPedido.GetValue(7)),
                                        QtyEntregada = datosPedido.IsDBNull(8) ? 0 : Convert.ToInt16(datosPedido.GetValue(8)),
                                        QtyFacturada = datosPedido.IsDBNull(9) ? 0 : Convert.ToInt16(datosPedido.GetValue(9)),
                                        UMVendida = datosPedido.IsDBNull(10) ? "" : Convert.ToString(datosPedido.GetValue(10)),
                                        Precio = datosPedido.IsDBNull(11) ? 0 : Convert.ToDecimal(datosPedido.GetValue(11)),
                                        Total = datosPedido.IsDBNull(12) ? 0 : Convert.ToDecimal(datosPedido.GetValue(12))
                                    });
                                }
                            }
                            if (datosPedido != null)
                            {
                                datosPedido.Close();
                            }
                        }
                        if (comandoPedido != null)
                        {
                            comandoPedido.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Reportes.ConsultarPedidos");

                if(datosPedido != null)
                {
                    datosPedido.Close();
                    comandoPedido.Dispose();
                }
                
                if(conexionPedido != null)
                {
                    if(conexionPedido.State != ConnectionState.Closed)
                    {
                        conexionPedido.Close();
                        conexionPedido.Dispose();
                    }
                }
            }
            finally
            {
                if (datosPedido != null)
                {
                    datosPedido.Close();
                    comandoPedido.Dispose();
                }

                if (conexionPedido != null)
                {
                    if (conexionPedido.State != ConnectionState.Closed)
                    {
                        conexionPedido.Close();
                        conexionPedido.Dispose();
                    }
                }
            }

            return Json(ListaPedidos, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ObtenerInventarioDeArticulo(string Grupo, string PorItem, string PorDesc, string ItemCode)
        {
            List<ConsultaArticulo> ListaArticulo = new List<ConsultaArticulo>();
            SqlConnection conexionInventario = null;
            SqlCommand comandoInventario = null;
            SqlDataReader datosInventario = null;


            try
            {
                using (conexionInventario = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionInventario.Open();
                    using (comandoInventario = conexionInventario.CreateCommand())
                    {
                        comandoInventario.CommandText = "SP_InventarioPorArticulo";
                        comandoInventario.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter grupoParameter = comandoInventario.CreateParameter();
                        grupoParameter.ParameterName = "Grupo";
                        grupoParameter.DbType = System.Data.DbType.Int16;
                        grupoParameter.Value = Convert.ToInt16(Grupo);
                        comandoInventario.Parameters.Add(grupoParameter);

                        SqlParameter porItemParameter = comandoInventario.CreateParameter();
                        porItemParameter.ParameterName = "ContieneItmCod";
                        porItemParameter.DbType = DbType.String;
                        porItemParameter.Value = PorItem;
                        comandoInventario.Parameters.Add(porItemParameter);

                        SqlParameter porDescParameter = comandoInventario.CreateParameter();
                        porDescParameter.ParameterName = "ContieneDesc";
                        porDescParameter.DbType = DbType.String;
                        porDescParameter.Value = PorDesc;
                        comandoInventario.Parameters.Add(porDescParameter);

                        SqlParameter itemCodeParameter = comandoInventario.CreateParameter();
                        itemCodeParameter.ParameterName = "ItemCode";
                        itemCodeParameter.DbType = DbType.String;
                        itemCodeParameter.Value = ItemCode;
                        comandoInventario.Parameters.Add(itemCodeParameter);


                        using (datosInventario = comandoInventario.ExecuteReader())
                        {
                            while (datosInventario.Read())
                            {
                                ListaArticulo.Add(new ConsultaArticulo()
                                {
                                    WhsCode = datosInventario.IsDBNull(0) ? "" : Convert.ToString(datosInventario.GetValue(0)),
                                    WhsName = datosInventario.IsDBNull(1) ? "" : Convert.ToString(datosInventario.GetValue(1)),
                                    OnHandQty = datosInventario.IsDBNull(2) ? -1 : Convert.ToDecimal(datosInventario.GetValue(2)),
                                    IsCommited = datosInventario.IsDBNull(3) ? -1 : Convert.ToDecimal(datosInventario.GetValue(3)),
                                    OnOrder = datosInventario.IsDBNull(4) ? -1 : Convert.ToDecimal(datosInventario.GetValue(4)),
                                    Available = datosInventario.IsDBNull(5) ? -1 : Convert.ToDecimal(datosInventario.GetValue(5))
                                });
                            }
                        }
                        if (datosInventario != null)
                        {
                            datosInventario.Close();
                        }
                    }
                    if (comandoInventario != null)
                    {
                        comandoInventario.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Reportes.ObtenerInventarioDeArticulo");

                if (datosInventario != null)
                {
                    datosInventario.Close();
                    comandoInventario.Dispose();
                }

                if(conexionInventario != null)
                {
                    if (conexionInventario.State != ConnectionState.Closed)
                    {
                        conexionInventario.Close();
                        conexionInventario.Dispose();
                    }
                }  
            }
            finally
            {
                if (datosInventario != null)
                {
                    datosInventario.Close();
                    comandoInventario.Dispose();
                }

                if (conexionInventario != null)
                {
                    if (conexionInventario.State != ConnectionState.Closed)
                    {
                        conexionInventario.Close();
                        conexionInventario.Dispose();
                    }
                }
            }

            return Json(ListaArticulo);

        }
    }
}