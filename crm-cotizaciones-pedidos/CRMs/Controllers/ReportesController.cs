using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using CRMs.Models;
using System.Security.Claims;
using System.Threading;
using CRMs.Utilerias;
using System.Web.Util;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Runtime.InteropServices.ComTypes;
using System.IO;
using Microsoft.Ajax.Utilities;
using System.Globalization;
using CRMs.Filters;

namespace CRMs.Controllers
{
    [Authorize]
    [NoCache]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class ReportesController : Controller
    {
        public ActionResult Inventario()
        {
            try
            {
                var Verif = ((ClaimsIdentity)User.Identity).Claims.First().Value;
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
                ExceptionUtility.LogException(ex, "Reportes.Inventario");
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult InventarioRC()
        {
            try
            {
                var Verif = ((ClaimsIdentity)User.Identity).Claims.First().Value;
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
                ExceptionUtility.LogException(ex, "Reportes.InventarioRC");
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult Reimpresion()
        {
            try
            {
                var Verif = ((ClaimsIdentity)User.Identity).Claims.First().Value;
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
                ExceptionUtility.LogException(ex, "Reportes.Reimpresion");
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult Compras()
        {
            try
            {
                var Verif = ((ClaimsIdentity)User.Identity).Claims.First().Value;
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
                ExceptionUtility.LogException(ex, "Reportes.Compras");
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult Ventas()
        {
            try
            {
                var Verif = ((ClaimsIdentity)User.Identity).Claims.First().Value;
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
                ExceptionUtility.LogException(ex, "Reportes.Ventas");
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Login");
            }
        }

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
                string UserId = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();

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
                                        DocDate = datosPedido.IsDBNull(1) ? "" : docDate.ToString("dd/MM/yyyy"),
                                        DocDueDate = datosPedido.IsDBNull(2) ? "" : docDue.ToString("dd/MM/yyyy"),
                                        Cliente = datosPedido.IsDBNull(3) ? "" : Convert.ToString(datosPedido.GetValue(3)),
                                        ItemCode = datosPedido.IsDBNull(4) ? "" : Convert.ToString(datosPedido.GetValue(4)),
                                        ItemName = datosPedido.IsDBNull(5) ? "" : Convert.ToString(datosPedido.GetValue(5)),
                                        QtyPedida = datosPedido.IsDBNull(6) ? 0 : Convert.ToInt16(datosPedido.GetValue(6)),
                                        QtyPendiente = datosPedido.IsDBNull(7) ? 0 : Convert.ToInt16(datosPedido.GetValue(7)),
                                        QtyEntregada = datosPedido.IsDBNull(8) ? 0 : Convert.ToInt16(datosPedido.GetValue(8)),
                                        QtyFacturada = datosPedido.IsDBNull(9) ? 0 : Convert.ToInt16(datosPedido.GetValue(9)),
                                        UMVendida = datosPedido.IsDBNull(10) ? "" : Convert.ToString(datosPedido.GetValue(10)),
                                        Precio = datosPedido.IsDBNull(11) ? 0 : Convert.ToDecimal(datosPedido.GetValue(11)),
                                        Total = datosPedido.IsDBNull(12) ? 0 : Convert.ToDecimal(datosPedido.GetValue(12)),
                                        DocFactura = datosPedido.IsDBNull(13) ? 0 : Convert.ToInt32(datosPedido.GetValue(13)),
                                        FechaCont = datosPedido.IsDBNull(14) ? "" : Convert.ToDateTime(datosPedido.GetValue(14)).ToString("dd/MM/yyyy"),
                                        Guia = datosPedido.IsDBNull(15) ? "" : Convert.ToString(datosPedido.GetValue(15)),
                                        Estado = datosPedido.IsDBNull(16) ? "" : Convert.ToString(datosPedido.GetValue(16)),
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
        public ActionResult ConsultarCotizaciones(int DocNum, string UserCode, string Type, string DateOrder, string Item,int opcion)
        {
            List<ConsultaCotizacion> ListaPedidos = new List<ConsultaCotizacion>();
            SqlConnection conexionPedido = null;
            SqlCommand comandoPedido = null;
            SqlDataReader datosPedido = null;
            DateTime docDate = new DateTime();
            //var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;

            try
            {               
                var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                string UserId = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
                if (UserCode.Length > 0 || UserCode.IsNullOrWhiteSpace())
                {
                    UserId = UserCode;
                }

                if (string.IsNullOrEmpty(UserId))
                {
                    ListaPedidos.Add(new ConsultaCotizacion()
                    {
                        NoCotizacion = -33,
                        DocDate = "nodate",
                        Cliente = "Tiempo de Sesión agotado.",
                        ItemCode = "noitem",
                        ItemName = "noname",
                        QtyCotizada = 0,
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
                            comandoPedido.CommandText = "SP_Consulta_Cotizaciones";
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

                            SqlParameter opcionParameter = comandoPedido.CreateParameter();
                            opcionParameter.ParameterName = "Opcion";
                            opcionParameter.DbType = System.Data.DbType.Int32;
                            opcionParameter.Value = opcion;
                            comandoPedido.Parameters.Add(opcionParameter);

                            using (datosPedido = comandoPedido.ExecuteReader())
                            {
                                while (datosPedido.Read())
                                {
                                    docDate = Convert.ToDateTime(datosPedido.GetValue(2));
                                    ListaPedidos.Add(new ConsultaCotizacion()
                                    {
                                        NoCotizacion = datosPedido.IsDBNull(0) ? 0 : Convert.ToInt32(datosPedido.GetValue(0)),
                                        DocStatus = datosPedido.IsDBNull(1) ? "" : Convert.ToString(datosPedido.GetValue(1)),
                                        DocDate = datosPedido.IsDBNull(2) ? "" : docDate.ToString("yyyy/MM/dd"),
                                        Cliente = datosPedido.IsDBNull(3) ? "" : Convert.ToString(datosPedido.GetValue(3)),
                                        ItemCode = datosPedido.IsDBNull(4) ? "" : Convert.ToString(datosPedido.GetValue(4)),
                                        ItemName = datosPedido.IsDBNull(5) ? "" : Convert.ToString(datosPedido.GetValue(5)),
                                        QtyCotizada = datosPedido.IsDBNull(6) ? 0 : Convert.ToInt16(datosPedido.GetValue(6)),
                                        UMVendida = datosPedido.IsDBNull(7) ? "" : Convert.ToString(datosPedido.GetValue(7)),
                                        Precio = datosPedido.IsDBNull(8) ? 0 : Convert.ToDecimal(datosPedido.GetValue(8)),
                                        Total = datosPedido.IsDBNull(9) ? 0 : Convert.ToDecimal(datosPedido.GetValue(9)),
                                        Direccion = datosPedido.IsDBNull(10) ? "" : Convert.ToString(datosPedido.GetValue(10)),
                                        Folio = datosPedido.IsDBNull(11) ? "" : Convert.ToString(datosPedido.GetValue(11)),
                                        Comentarios = datosPedido.IsDBNull(12) ? "" : Convert.ToString(datosPedido.GetValue(12)),
                                        FechaEntrega = datosPedido.IsDBNull(13) ? "" : Convert.ToDateTime(datosPedido.GetValue(13)).ToString("yyyy/MM/dd"),
                                        Descuento1 = datosPedido.IsDBNull(14) ? 0 : Convert.ToDecimal(datosPedido.GetValue(14)),
                                        Descuento2 = datosPedido.IsDBNull(15) ? 0 : Convert.ToDecimal(datosPedido.GetValue(15)),
                                        LineNum = datosPedido.IsDBNull(16) ? 0 : Convert.ToInt32(datosPedido.GetValue(16)),
                                        UnitSize = datosPedido.IsDBNull(17) ? "" : Convert.ToString(datosPedido.GetValue(17)),
                                        VATLiable = datosPedido.IsDBNull(18) ? "" : datosPedido.GetString(18),
                                        TaxCodeAR = datosPedido.IsDBNull(19) ? "" : datosPedido.GetString(19),
                                        IndirectTax = datosPedido.IsDBNull(20) ? "" : datosPedido.GetString(20),
                                        ValorImpuesto = datosPedido.IsDBNull(21) ? 0 : Convert.ToDecimal(datosPedido.GetValue(21)),
                                        WhsCode = datosPedido.IsDBNull(22) ? "" : Convert.ToString(datosPedido.GetValue(22)),
                                        Almacen = datosPedido.IsDBNull(23) ? "" : Convert.ToString(datosPedido.GetValue(23)),
                                        Envio = datosPedido.IsDBNull(24) ? "" : Convert.ToString(datosPedido.GetValue(24)),
                                        stock = datosPedido.IsDBNull(25) ? 0 : Convert.ToDecimal(datosPedido.GetValue(25)),
                                        ShipDate = datosPedido.IsDBNull(26) ? "" : Convert.ToDateTime(datosPedido.GetValue(26)).ToString("yyyy/MM/dd"),
                                        comprometido = datosPedido.IsDBNull(27) ? 0 : Convert.ToDecimal(datosPedido.GetValue(27)),
                                        Fol = datosPedido.IsDBNull(28) ? "" : Convert.ToString(datosPedido.GetValue(28)),
                                        Paqueteria = datosPedido.IsDBNull(29) ? "" : Convert.ToString(datosPedido.GetValue(29)),
                                        MetodoPago = datosPedido.IsDBNull(30) ? "" : Convert.ToString(datosPedido.GetValue(30)),
                                        FormaPago = datosPedido.IsDBNull(31) ? "" : Convert.ToString(datosPedido.GetValue(31)),
                                        CDFI = datosPedido.IsDBNull(32) ? "" : Convert.ToString(datosPedido.GetValue(32)),

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
        public ActionResult ConsultarVentasPorUsuario()
        {
            ConsultaVentas consultaVentas = new ConsultaVentas();
            SqlConnection conexionPedido = null;
            SqlCommand comandoPedido = null;
            SqlDataReader datosPedido = null;
            try
            {
                var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                string UserId = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();

                if (string.IsNullOrEmpty(UserId))
                {
                    throw new HttpException("Usuario no encontrado");
                    // return Json(ListaPedidos, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    using (conexionPedido = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                    {
                        conexionPedido.Open();
                        using (comandoPedido = conexionPedido.CreateCommand())
                        {
                            comandoPedido.CommandText = "SP_Consulta_Ventas";
                            comandoPedido.CommandType = System.Data.CommandType.StoredProcedure;
                            SqlParameter userCodeParameter = comandoPedido.CreateParameter();
                            userCodeParameter.ParameterName = "UserId";
                            userCodeParameter.DbType = DbType.String;
                            userCodeParameter.Value = UserId;
                            comandoPedido.Parameters.Add(userCodeParameter);
                            using (datosPedido = comandoPedido.ExecuteReader())
                            {

                                List<ConsultaVentasAnio> ventasAnio = new List<ConsultaVentasAnio>();
                                while (datosPedido.Read())
                                {
                                    ConsultaVentasAnio venta = new ConsultaVentasAnio()
                                    {
                                        Factura = datosPedido.IsDBNull(0) ? 0 : Convert.ToInt32(datosPedido.GetValue(0)),
                                        Pedido = datosPedido.IsDBNull(1) ? 0 : Convert.ToInt32(datosPedido.GetValue(1)),
                                        FechaFactura = datosPedido.IsDBNull(2) ? "" : Convert.ToDateTime(datosPedido.GetValue(2)).ToString("dd/MM/yyyy"),
                                        VencimientoFactura = datosPedido.IsDBNull(3) ? "" : Convert.ToDateTime(datosPedido.GetValue(3)).ToString("dd/MM/yyyy"),
                                        CodigoCliente = datosPedido.IsDBNull(4) && datosPedido.IsDBNull(5) ? "" : $"{Convert.ToString(datosPedido.GetValue(4))} - {Convert.ToString(datosPedido.GetValue(5))}",
                                        ImporteTotal = datosPedido.IsDBNull(6) ? 0 : Convert.ToDecimal(datosPedido.GetValue(6)),
                                        MesFactura = datosPedido.IsDBNull(7) ? 0 : Convert.ToInt32(datosPedido.GetValue(7))
                                    };

                                    ventasAnio.Add(venta);
                                }

                                var ventasFiltradas = ventasAnio.Where(x => x.MesFactura == DateTime.Now.Month).ToList();


                                List<ConsultaVentasMes> ventasMes = new List<ConsultaVentasMes>();
                                foreach (var venta in ventasFiltradas)
                                {
                                    ConsultaVentasMes ventaMes = new ConsultaVentasMes
                                    {
                                        Factura = venta.Factura,
                                        Pedido = venta.Pedido,
                                        FechaFactura = venta.FechaFactura,
                                        VencimientoFactura = venta.VencimientoFactura,
                                        CodigoCliente = venta.CodigoCliente,
                                        ImporteTotal = venta.ImporteTotal,
                                    };

                                    ventasMes.Add(ventaMes);
                                }


                                consultaVentas.VentasMes = ventasMes;
                                consultaVentas.VentasAnio = ventasAnio;

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

            return Json(consultaVentas, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ObtenerInventarioDeArticulo(string Grupo, string PorItem, string PorDesc, string ItemCode, string rol, string bd)
        {
            List<ConsultaArticulo> ListaArticulo = new List<ConsultaArticulo>();
            SqlConnection conexionInventario = null;
            SqlCommand comandoInventario = null;
            SqlDataReader datosInventario = null;
            string codigoVendedor = "";

            string SlpCode = "";

            try
            {
                var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                SlpCode = Identity.Claims.Where(c => c.Type == ClaimTypes.Actor).Select(c => c.Value).SingleOrDefault();
                codigoVendedor = User.Identity.Name;


                using (conexionInventario = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionInventario.Open();
                    using (comandoInventario = conexionInventario.CreateCommand())
                    {
                        comandoInventario.CommandText = "SP_InventarioPorArticulo";
                        comandoInventario.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter cardCodeParameter = comandoInventario.CreateParameter();
                        cardCodeParameter.ParameterName = "CardCode";
                        cardCodeParameter.DbType = System.Data.DbType.String;
                        cardCodeParameter.Value = SlpCode;
                        comandoInventario.Parameters.Add(cardCodeParameter);

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
                        SqlParameter whsCodeParameter = comandoInventario.CreateParameter();
                        whsCodeParameter.ParameterName = "Rol";
                        whsCodeParameter.DbType = System.Data.DbType.String;
                        whsCodeParameter.Value = rol;
                        comandoInventario.Parameters.Add(whsCodeParameter);

                        SqlParameter whsbd = comandoInventario.CreateParameter();
                        whsbd.ParameterName = "bd";
                        whsbd.DbType = System.Data.DbType.String;
                        whsbd.Value = bd;
                        comandoInventario.Parameters.Add(whsbd);


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
                                    Available = datosInventario.IsDBNull(5) ? -1 : Convert.ToDecimal(datosInventario.GetValue(5)),
                                    SalUnitMsr = datosInventario.IsDBNull(6) ? "" : Convert.ToString(datosInventario.GetValue(6)),
                                    InvntryUom = datosInventario.IsDBNull(7) ? "" : Convert.ToString(datosInventario.GetValue(7)),
                                    Ubicacion = datosInventario.IsDBNull(8) ? "" : Convert.ToString(datosInventario.GetValue(8)),
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

                if (conexionInventario != null)
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
        public ActionResult ObtenerPorcentajeVentas()
        {
            List<PorcentajeVentas> PorcentajeVenta = new List<PorcentajeVentas>();
            SqlConnection conexionInventario = null;
            SqlCommand comandoInventario = null;
            SqlDataReader datosInventario = null;
            var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string UserId = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();

            try
            {
                using (conexionInventario = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionInventario.Open();
                    using (comandoInventario = conexionInventario.CreateCommand())
                    {
                        comandoInventario.CommandText = "SP_ObtenerPorcentajeVentas";
                        comandoInventario.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter userCodeParameter = comandoInventario.CreateParameter();
                        userCodeParameter.ParameterName = "@Usuario";
                        userCodeParameter.DbType = DbType.String;
                        userCodeParameter.Value = UserId;
                        comandoInventario.Parameters.Add(userCodeParameter);



                        using (datosInventario = comandoInventario.ExecuteReader())
                        {
                            while (datosInventario.Read())
                            {
                                PorcentajeVenta.Add(new PorcentajeVentas()
                                {

                                    PorcentajeVenta = datosInventario.IsDBNull(0) ? 0 : Convert.ToDecimal(datosInventario.GetValue(0)),

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

                if (conexionInventario != null)
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

            return Json(PorcentajeVenta);

        }

        public ActionResult VistaPreviaOrden(string jsonPedido, string g)
        {
            dynamic order = JsonConvert.DeserializeObject(jsonPedido);
            JArray lineas = (JArray)order.DocumentLines;
            List<VistaPrevia> vista = new List<VistaPrevia>();
            SqlConnection conexionVista = null;
            SqlCommand comandoVista = null;
            SqlDataReader datosVista = null;
            var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string UserId = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();

            try
            {
                for (int i = 0; i < lineas.Count;)
                {
                    using (conexionVista = new SqlConnection(ConfigurationManager.ConnectionStrings["CRMSp"].ConnectionString))
                    {
                        conexionVista.Open();
                        using (comandoVista = conexionVista.CreateCommand())
                        {
                            comandoVista.CommandText = "CRM_InsertarRegistroVistaP";
                            comandoVista.CommandType = System.Data.CommandType.StoredProcedure;

                            // DocDate
                            SqlParameter DocDate = comandoVista.CreateParameter();
                            DocDate.ParameterName = "@DocDate";
                            DocDate.DbType = DbType.DateTime;

                            DateTime fechaDoc;
                            if (DateTime.TryParseExact(Convert.ToString(order.DocDate),
                                                       "dd-MM-yyyy",
                                                       CultureInfo.InvariantCulture,
                                                       DateTimeStyles.None,
                                                       out fechaDoc))
                            {
                                DocDate.Value = fechaDoc;
                            }
                            else
                            {
                                DocDate.Value = DBNull.Value;
                            }
                            comandoVista.Parameters.Add(DocDate);

                            // DocDueDate
                            SqlParameter DocDueDate = comandoVista.CreateParameter();
                            DocDueDate.ParameterName = "@DocDueDate";
                            DocDueDate.DbType = DbType.DateTime;

                            DateTime fechaVenc;
                            if (DateTime.TryParseExact(Convert.ToString(order.DocDueDate),
                                                       "dd-MM-yyyy",
                                                       CultureInfo.InvariantCulture,
                                                       DateTimeStyles.None,
                                                       out fechaVenc))
                            {
                                DocDueDate.Value = fechaVenc;
                            }
                            else
                            {
                                DocDueDate.Value = DBNull.Value;
                            }
                            comandoVista.Parameters.Add(DocDueDate);

                            SqlParameter CardCode = comandoVista.CreateParameter();
                            CardCode.ParameterName = "@CardCode";
                            CardCode.DbType = DbType.String;
                            CardCode.Value = order.CardCode;
                            comandoVista.Parameters.Add(CardCode);

                            SqlParameter CardName = comandoVista.CreateParameter();
                            CardName.ParameterName = "@CardName";
                            CardName.DbType = DbType.String;
                            CardName.Value = order.CardName;
                            comandoVista.Parameters.Add(CardName);

                            SqlParameter Comments = comandoVista.CreateParameter();
                            Comments.ParameterName = "@Comments";
                            Comments.DbType = DbType.String;
                            Comments.Value = order.Comments;
                            comandoVista.Parameters.Add(Comments);

                            SqlParameter SalesPersonCode = comandoVista.CreateParameter();
                            SalesPersonCode.ParameterName = "@SalesPersonCode";
                            SalesPersonCode.DbType = DbType.Double;
                            SalesPersonCode.Value = order.SalesPersonCode;
                            comandoVista.Parameters.Add(SalesPersonCode);

                            SqlParameter Address = comandoVista.CreateParameter();
                            Address.ParameterName = "@Address";
                            Address.DbType = DbType.String;
                            Address.Value = order.Address;
                            comandoVista.Parameters.Add(Address);

                            SqlParameter JournalMemo = comandoVista.CreateParameter();
                            JournalMemo.ParameterName = "@JournalMemo";
                            JournalMemo.DbType = DbType.String;
                            JournalMemo.Value = order.JournalMemo;
                            comandoVista.Parameters.Add(JournalMemo);

                            SqlParameter DocumentStatus = comandoVista.CreateParameter();
                            DocumentStatus.ParameterName = "@DocumentStatus";
                            DocumentStatus.DbType = DbType.String;
                            DocumentStatus.Value = order.DocumentStatus;
                            comandoVista.Parameters.Add(DocumentStatus);

                            SqlParameter DocType = comandoVista.CreateParameter();
                            DocType.ParameterName = "@DocType";
                            DocType.DbType = DbType.String;
                            DocType.Value = order.DocType;
                            comandoVista.Parameters.Add(DocType);

                            SqlParameter Discount = comandoVista.CreateParameter();
                            Discount.ParameterName = "@Discount";
                            Discount.DbType = DbType.Double;
                            Discount.Value = order.Discount;
                            comandoVista.Parameters.Add(Discount);

                            SqlParameter DocBase = comandoVista.CreateParameter();
                            DocBase.ParameterName = "@DocBase";
                            DocBase.DbType = DbType.String;
                            DocBase.Value = order.DocBase;
                            comandoVista.Parameters.Add(DocBase);

                            // DocDueDateCot
                            SqlParameter DocDueDateCot = comandoVista.CreateParameter();
                            DocDueDateCot.ParameterName = "@DocDueDateCot";
                            DocDueDateCot.DbType = DbType.DateTime;

                            DateTime fechaCot;
                            if (DateTime.TryParseExact(Convert.ToString(order.DocDueDateCot),
                                                       "dd-MM-yyyy",
                                                       CultureInfo.InvariantCulture,
                                                       DateTimeStyles.None,
                                                       out fechaCot))
                            {
                                DocDueDateCot.Value = fechaCot;
                            }
                            else
                            {
                                DocDueDateCot.Value = DBNull.Value;
                            }
                            comandoVista.Parameters.Add(DocDueDateCot);

                            SqlParameter U_envio = comandoVista.CreateParameter();
                            U_envio.ParameterName = "@U_envio";
                            U_envio.DbType = DbType.String;
                            U_envio.Value = order.U_envio;
                            comandoVista.Parameters.Add(U_envio);

                            SqlParameter ItemCode = comandoVista.CreateParameter();
                            ItemCode.ParameterName = "@ItemCode";
                            ItemCode.DbType = DbType.String;
                            ItemCode.Value = order.DocumentLines[i].ItemCode;
                            comandoVista.Parameters.Add(ItemCode);

                            SqlParameter ItemDescription = comandoVista.CreateParameter();
                            ItemDescription.ParameterName = "@ItemDescription";
                            ItemDescription.DbType = DbType.String;
                            ItemDescription.Value = order.DocumentLines[i].ItemDescription;
                            comandoVista.Parameters.Add(ItemDescription);

                            // DiasEntrega (en líneas)
                            SqlParameter DiasEntrega = comandoVista.CreateParameter();
                            DiasEntrega.ParameterName = "@DiasEntrega";
                            DiasEntrega.DbType = DbType.DateTime;

                            DateTime fechaLinea;
                            if (DateTime.TryParseExact(Convert.ToString(order.DocumentLines[i].DiasEntrega),
                                                       "dd-MM-yyyy",
                                                       CultureInfo.InvariantCulture,
                                                       DateTimeStyles.None,
                                                       out fechaLinea))
                            {
                                DiasEntrega.Value = fechaLinea;
                            }
                            else
                            {
                                DiasEntrega.Value = DBNull.Value;
                            }
                            comandoVista.Parameters.Add(DiasEntrega);

                            SqlParameter Precio = comandoVista.CreateParameter();
                            Precio.ParameterName = "@Precio";
                            Precio.DbType = DbType.Double;
                            Precio.Value = order.DocumentLines[i].Precio;
                            comandoVista.Parameters.Add(Precio);

                            SqlParameter PrecioLinea = comandoVista.CreateParameter();
                            PrecioLinea.ParameterName = "@PrecioLinea";
                            PrecioLinea.DbType = DbType.Double;
                            PrecioLinea.Value = order.DocumentLines[i].PrecioLinea;
                            comandoVista.Parameters.Add(PrecioLinea);

                            SqlParameter PrecioUnitario = comandoVista.CreateParameter();
                            PrecioUnitario.ParameterName = "@PrecioUnitario";
                            PrecioUnitario.DbType = DbType.String;
                            PrecioUnitario.Value = order.DocumentLines[i].PrecioUnitario;
                            comandoVista.Parameters.Add(PrecioUnitario);

                            SqlParameter Quantity = comandoVista.CreateParameter();
                            Quantity.ParameterName = "@Quantity";
                            Quantity.DbType = DbType.Double;
                            Quantity.Value = order.DocumentLines[i].Quantity;
                            comandoVista.Parameters.Add(Quantity);

                            SqlParameter Currency = comandoVista.CreateParameter();
                            Currency.ParameterName = "@Currency";
                            Currency.DbType = DbType.String;
                            Currency.Value = order.DocumentLines[i].Currency;
                            comandoVista.Parameters.Add(Currency);

                            SqlParameter PrtgDiscount = comandoVista.CreateParameter();
                            PrtgDiscount.ParameterName = "@PrtgDiscount";
                            PrtgDiscount.DbType = DbType.Double;
                            PrtgDiscount.Value = order.DocumentLines[i].PrtgDiscount;
                            comandoVista.Parameters.Add(PrtgDiscount);

                            SqlParameter WarehouseCode = comandoVista.CreateParameter();
                            WarehouseCode.ParameterName = "@WarehouseCode";
                            WarehouseCode.DbType = DbType.String;
                            WarehouseCode.Value = order.DocumentLines[i].WarehouseCode;
                            comandoVista.Parameters.Add(WarehouseCode);

                            SqlParameter MetrosExtra = comandoVista.CreateParameter();
                            MetrosExtra.ParameterName = "@MetrosExtra";
                            MetrosExtra.DbType = DbType.String;
                            MetrosExtra.Value = order.DocumentLines[i].MetrosExtra;
                            comandoVista.Parameters.Add(MetrosExtra);


                            SqlParameter LineStatus = comandoVista.CreateParameter();
                            LineStatus.ParameterName = "@LineStatus";
                            LineStatus.DbType = DbType.String;
                            LineStatus.Value = order.DocumentLines[i].LineStatus;
                            comandoVista.Parameters.Add(LineStatus);

                            SqlParameter LineNum = comandoVista.CreateParameter();
                            LineNum.ParameterName = "@LineNum";
                            LineNum.DbType = DbType.String;
                            LineNum.Value = order.DocumentLines[i].LineNum;
                            comandoVista.Parameters.Add(LineNum);

                            SqlParameter Apuntador = comandoVista.CreateParameter();
                            Apuntador.ParameterName = "@Apuntador";
                            Apuntador.DbType = DbType.String;
                            Apuntador.Value = i;
                            comandoVista.Parameters.Add(Apuntador);
                            SqlParameter id = comandoVista.CreateParameter();
                            id.ParameterName = "@id";
                            id.DbType = DbType.String;
                            id.Value = g;
                            comandoVista.Parameters.Add(id);

                            using (datosVista = comandoVista.ExecuteReader())
                            {
                                vista.Add(new VistaPrevia()
                                {
                                    DocNum = "Y"
                                });
                            }
                            if (vista.Count > 0)
                            {
                                i++;
                            }
                            else
                            {
                                datosVista = null;

                            }
                            if (datosVista != null)
                            {
                                datosVista.Close();
                            }
                        }
                        if (comandoVista != null)
                        {
                            comandoVista.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Reportes.GuardarVistaPrevia");

                if (datosVista != null)
                {
                    datosVista.Close();
                    comandoVista.Dispose();
                }

                if (conexionVista != null)
                {
                    if (conexionVista.State != ConnectionState.Closed)
                    {
                        conexionVista.Close();
                        conexionVista.Dispose();
                    }
                }
            }
            finally
            {
                if (datosVista != null)
                {
                    datosVista.Close();
                    comandoVista.Dispose();
                }

                if (conexionVista != null)
                {
                    if (conexionVista.State != ConnectionState.Closed)
                    {
                        conexionVista.Close();
                        conexionVista.Dispose();
                    }
                }
            }

            return Json(vista);
        }

        public ActionResult ImprimirVistaP(string id) {
            FileStream fss = null;

            try
            {
                Impresion i = new Impresion();
                var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                string Vendedor = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
                var stream = i.GenerarImpresionVistaP(id);
                int buffLength = 2048;
                byte[] buff = new byte[buffLength];
                int contentLen;

                var LocalDirectory = Path.Combine(Path.GetTempPath(), "VistaPrevia.pdf");

                FileStream fs = null;
                using (fs = new FileStream(LocalDirectory, FileMode.Create, FileAccess.Write, FileShare.None))
                using (stream)
                {
                    contentLen = stream.Read(buff, 0, buffLength);
                    while (contentLen != 0)
                    {
                        fs.Write(buff, 0, contentLen);
                        contentLen = stream.Read(buff, 0, buffLength);
                    }
                }
                i = null;

                fss = new FileStream(LocalDirectory, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "ReportesController.ImprimirVistaP");
            }

            return File(fss, "application/pdf");
        }

        public ActionResult ConsultarFacturas(int DocNum, string UserCode, string Type, string DateOrder, string Item,string Year, string Mes)
        {
            List<ConsultaFactura> ListaFacturas = new List<ConsultaFactura>();
            SqlConnection conexionPedido = null;
            SqlCommand comandoPedido = null;
            SqlDataReader datosPedido = null;
          
            //var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            try
            {
                if (UserCode=="")
                {
                    var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                    UserCode = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
                }            

                if (string.IsNullOrEmpty(UserCode))
                {
                    ListaFacturas.Add(new ConsultaFactura()
                    {
                        NoPedidoSAP = -33,
                        DocDate = "nodate",
                        FechaCont = "nodate",
                        Cliente = "Tiempo de Sesión agotado.",
                       
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
                            comandoPedido.CommandText = "SP_Consulta_Facturas";
                            comandoPedido.CommandType = System.Data.CommandType.StoredProcedure;

                            SqlParameter docNumParameter = comandoPedido.CreateParameter();
                            docNumParameter.ParameterName = "DocNum";
                            docNumParameter.DbType = System.Data.DbType.Int32;
                            docNumParameter.Value = DocNum;
                            comandoPedido.Parameters.Add(docNumParameter);

                            SqlParameter userCodeParameter = comandoPedido.CreateParameter();
                            userCodeParameter.ParameterName = "UserId";
                            userCodeParameter.DbType = DbType.String;
                            userCodeParameter.Value = UserCode;
                            comandoPedido.Parameters.Add(userCodeParameter);

                            SqlParameter TypeParameter = comandoPedido.CreateParameter();
                            TypeParameter.ParameterName = "Tipo";
                            TypeParameter.DbType = DbType.String;
                            TypeParameter.Value = "Todos";
                            comandoPedido.Parameters.Add(TypeParameter);

                            SqlParameter FechaParameter = comandoPedido.CreateParameter();
                            FechaParameter.ParameterName = "Fecha";
                            FechaParameter.DbType = DbType.String;
                            FechaParameter.Value = "";
                            comandoPedido.Parameters.Add(FechaParameter);
                            SqlParameter yearParameter = comandoPedido.CreateParameter();
                            yearParameter.ParameterName = "Year";
                            yearParameter.DbType = DbType.String;
                            yearParameter.Value = Year;
                            comandoPedido.Parameters.Add(yearParameter);
                            SqlParameter mesParameter = comandoPedido.CreateParameter();
                            mesParameter.ParameterName = "Mes";
                            mesParameter.DbType = DbType.String;
                            mesParameter.Value = Mes;
                            comandoPedido.Parameters.Add(mesParameter);

                            using (datosPedido = comandoPedido.ExecuteReader())
                            {
                                while (datosPedido.Read())
                                {
                                    ListaFacturas.Add(new ConsultaFactura()
                                    {
                                        NoPedidoSAP = datosPedido.IsDBNull(0) ? 0 : Convert.ToInt32(datosPedido.GetValue(0)),
                                        //DocDate = datosPedido.IsDBNull(1) ? "" : Convert.ToDateTime(datosPedido.GetValue(1)).ToString("yyyy/MM/dd"),
                                        Cliente = datosPedido.IsDBNull(1) ? "" : Convert.ToString(datosPedido.GetValue(1)),
                                        DocFactura = datosPedido.IsDBNull(2) ? 0 : Convert.ToInt32(datosPedido.GetValue(2)),
                                        EntryFactura = datosPedido.IsDBNull(3) ? 0 : Convert.ToInt32(datosPedido.GetValue(3)),
                                        FechaCont = datosPedido.IsDBNull(4) ? "" : Convert.ToDateTime(datosPedido.GetValue(4)).ToString("dd/MM/yyyy"),
                                        Estado = datosPedido.IsDBNull(5) ? "" : Convert.ToString(datosPedido.GetValue(5)),
                                        DocEntry = datosPedido.IsDBNull(6) ? 0 : Convert.ToInt32(datosPedido.GetValue(6)),
                                        TotalsinIva = (float)(datosPedido.IsDBNull(7) ? 0 : Convert.ToDecimal(datosPedido.GetValue(7))),
                                        Total = (float)(datosPedido.IsDBNull(8) ? 0 : Convert.ToDecimal(datosPedido.GetValue(8))),
                                      
                                        rutaQr = datosPedido.IsDBNull(9) ? "" : Convert.ToString(datosPedido.GetValue(9)),
                                       
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
                ExceptionUtility.LogException(ex, "Reportes.ConsultarFacturas");

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

            return Json(ListaFacturas, JsonRequestBehavior.AllowGet);
        }

    }
}
