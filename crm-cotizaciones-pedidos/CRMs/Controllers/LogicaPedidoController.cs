using CRMs.Models;
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
using CRMs.Utilerias;
using SAPbobsCOM;
using System.Reflection.Emit;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Drawing;
using Microsoft.AspNet.Identity;
using System.Data;
using CRMs.Filters;
using CRMs.Helpers;


namespace CRMs.Controllers
{
    [Authorize]
    [NoCache]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class LogicaPedidoController : Controller
    {
        
        public ActionResult ObtenerSociosDeNegocios(string opcion)
        {
            List<SocioDeNegocios> ListaSocioNegocios = new List<SocioDeNegocios>();
            string codigoVendedor = "";
            
            string SlpCode = "";

            try
            {
                var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                SlpCode = Identity.Claims.Where(c => c.Type == ClaimTypes.Actor).Select(c => c.Value).SingleOrDefault();
                codigoVendedor = User.Identity.Name;

                if (string.IsNullOrEmpty(SlpCode))
                {
                    ListaSocioNegocios.Add(new SocioDeNegocios()
                    {
                        CardCode = "-33",
                        CardName = "Tiempo de Sesión agotado.",
                        ListNum = 0,
                        Moneda = "",
                        Discount = 0,
                        DiscInRet = ""
                    });
                    return Json(ListaSocioNegocios, JsonRequestBehavior.AllowGet);
                }

                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();

                    using (var comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = "SP_ObtenerSN";
                        comandoSQL.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter compradorParameter = comandoSQL.CreateParameter();
                        compradorParameter.ParameterName = "Vendedor";
                        compradorParameter.DbType = System.Data.DbType.String;
                        compradorParameter.Value = SlpCode;
                        comandoSQL.Parameters.Add(compradorParameter);

                        SqlParameter tipoParameter = comandoSQL.CreateParameter();
                        tipoParameter.ParameterName = "Opcion";
                        tipoParameter.DbType = System.Data.DbType.String;
                        tipoParameter.Value = opcion;
                        comandoSQL.Parameters.Add(tipoParameter);

                        using (SqlDataReader dataReaderSN = comandoSQL.ExecuteReader())
                        {
                            try
                            {
                                while (dataReaderSN.Read())
                                {
                                    ListaSocioNegocios.Add(new SocioDeNegocios()
                                    {
                                        CardCode = dataReaderSN.IsDBNull(0) ? "" : dataReaderSN.GetString(0),
                                        CardName = dataReaderSN.IsDBNull(1) ? "" : dataReaderSN.GetString(1),
                                        ListNum = dataReaderSN.IsDBNull(2) ? -1 : dataReaderSN.GetInt16(2),
                                        Moneda = dataReaderSN.IsDBNull(3) ? "" : dataReaderSN.GetString(3),
                                        Discount = dataReaderSN.IsDBNull(4) ? 0 : dataReaderSN.GetDecimal(4),
                                        DiscInRet = dataReaderSN.IsDBNull(5) ? "" : dataReaderSN.GetString(5),
                                        Saldo = dataReaderSN.IsDBNull(6) ? 0 : dataReaderSN.GetDecimal(6),
                                        Credito = dataReaderSN.IsDBNull(7) ? 0 : dataReaderSN.GetDecimal(7),
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerSociosDeNegocios");
                                return Json(ListaSocioNegocios, JsonRequestBehavior.AllowGet);
                            }
                            finally
                            {
                                if (dataReaderSN != null)
                                {
                                    dataReaderSN.Close();
                                    comandoSQL.Dispose();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerSociosDeNegocios");
            }

            return Json(ListaSocioNegocios, JsonRequestBehavior.AllowGet);
        }// fin ObtenerSociosDeNegocios

        public ActionResult ObtenerAlmacen()
        {
            string warehouse = "NoNWHS";

            try
            {
                warehouse = ConfigurationManager.AppSettings["WHS"];
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerAlmacen");
            }
            
            return Json(warehouse, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult ObtenerArticulosdeSAP2(string cardCode, string rol,string itemCode, string itemName, string bd)
        {
            List<Articulos> ListaDeArticulos = new List<Articulos>();
            string codigoVendedor = "";

            string SlpCode = "";

            try
            {
                var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                SlpCode = Identity.Claims.Where(c => c.Type == ClaimTypes.Actor).Select(c => c.Value).SingleOrDefault();
                codigoVendedor = User.Identity.Name;

               
                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();

                    using (var comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = "SP_ObtenerArticulosdeSAP";
                        comandoSQL.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter cardCodeParameter = comandoSQL.CreateParameter();
                        cardCodeParameter.ParameterName = "CardCode";
                        cardCodeParameter.DbType = System.Data.DbType.String;
                        cardCodeParameter.Value = SlpCode;
                        comandoSQL.Parameters.Add(cardCodeParameter);

                        SqlParameter pricelistParameter = comandoSQL.CreateParameter();
                        pricelistParameter.ParameterName = "PriceList";
                        pricelistParameter.DbType = System.Data.DbType.String;
                        pricelistParameter.Value = "";
                        comandoSQL.Parameters.Add(pricelistParameter);

                        SqlParameter whsCodeParameter = comandoSQL.CreateParameter();
                        whsCodeParameter.ParameterName = "Rol";
                        whsCodeParameter.DbType = System.Data.DbType.String;
                        whsCodeParameter.Value = rol;
                        comandoSQL.Parameters.Add(whsCodeParameter);

                        SqlParameter wmsItemCode = comandoSQL.CreateParameter();
                        wmsItemCode.ParameterName = "Codigo";
                        wmsItemCode.DbType = System.Data.DbType.String;
                        wmsItemCode.Value = itemCode;
                        comandoSQL.Parameters.Add(wmsItemCode);
                        SqlParameter wmsItemName = comandoSQL.CreateParameter();
                        wmsItemName.ParameterName = "Articulo";
                        wmsItemName.DbType = System.Data.DbType.String;
                        wmsItemName.Value = itemName;
                        comandoSQL.Parameters.Add(wmsItemName);
                        SqlParameter wmsBD = comandoSQL.CreateParameter();
                        wmsBD.ParameterName = "bd";
                        wmsBD.DbType = System.Data.DbType.String;
                        wmsBD.Value = bd;
                        comandoSQL.Parameters.Add(wmsBD);

                        using (SqlDataReader dataReaderITEM = comandoSQL.ExecuteReader())
                        {
                            try
                            {
                                while (dataReaderITEM.Read())
                                {
                                    ListaDeArticulos.Add(new Articulos()
                                    {
                                        ItemCode = dataReaderITEM.IsDBNull(0) ? "" : dataReaderITEM.GetString(0),
                                        ItemName = dataReaderITEM.IsDBNull(1) ? "" : dataReaderITEM.GetString(1),
                                   
                                    }
                                   );
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerArticulosdeSAP");
                                return Json(ListaDeArticulos, JsonRequestBehavior.AllowGet);
                            }
                            finally
                            {
                                if (dataReaderITEM != null)
                                {
                                    dataReaderITEM.Close();
                                    comandoSQL.Dispose();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerArticulosdeSAP");
            }

            return Json(ListaDeArticulos, JsonRequestBehavior.AllowGet);
        }// fin ObtenerArticulosdeSAP
        
        public ActionResult ObtenerArticulosdeSAP(string cardCode, string whsCode, int priceList,string descuento )
        {
            List<Articulos> ListaDeArticulos = new List<Articulos>();

            try
            {
                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();

                    using (var comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = "SP_ObtenerArticulos";
                        comandoSQL.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter cardCodeParameter = comandoSQL.CreateParameter();
                        cardCodeParameter.ParameterName = "CardCode";
                        cardCodeParameter.DbType = System.Data.DbType.String;
                        cardCodeParameter.Value = cardCode;
                        comandoSQL.Parameters.Add(cardCodeParameter);

                        SqlParameter whsCodeParameter = comandoSQL.CreateParameter();
                        whsCodeParameter.ParameterName = "WareHouse";
                        whsCodeParameter.DbType = System.Data.DbType.String;
                        whsCodeParameter.Value = whsCode;
                        comandoSQL.Parameters.Add(whsCodeParameter);

                        SqlParameter pricelistParameter = comandoSQL.CreateParameter();
                        pricelistParameter.ParameterName = "PriceList";
                        pricelistParameter.DbType = System.Data.DbType.Int16;
                        pricelistParameter.Value = priceList;
                        comandoSQL.Parameters.Add(pricelistParameter);

                        SqlParameter descuentoParameter = comandoSQL.CreateParameter();
                        descuentoParameter.ParameterName = "descuento";
                        descuentoParameter.DbType = System.Data.DbType.String;
                        descuentoParameter.Value = descuento;
                        comandoSQL.Parameters.Add(descuentoParameter);



                        using (SqlDataReader dataReaderITEM = comandoSQL.ExecuteReader())
                        {
                            try
                            {
                                while (dataReaderITEM.Read())
                                {
                                    ListaDeArticulos.Add(new Articulos()
                                    {
                                        ItemCode = dataReaderITEM.IsDBNull(0) ? "" : dataReaderITEM.GetString(0),
                                        ItemName = dataReaderITEM.IsDBNull(1) ? "" : dataReaderITEM.GetString(1),
                                        VATLiable = dataReaderITEM.IsDBNull(2) ? "" : dataReaderITEM.GetString(2),
                                        NoDiscount = dataReaderITEM.IsDBNull(3) ? "" : dataReaderITEM.GetString(3),
                                        TaxCodeAR = dataReaderITEM.IsDBNull(4) ? "" : dataReaderITEM.GetString(4),
                                        IndirectTax = dataReaderITEM.IsDBNull(5) ? "" : dataReaderITEM.GetString(5),
                                        Stock = dataReaderITEM.IsDBNull(6) ? 0 : dataReaderITEM.GetDecimal(6),
                                        UnitSize = dataReaderITEM.IsDBNull(7) ? "" : dataReaderITEM.GetString(7),
                                        Price = dataReaderITEM.IsDBNull(8) ? "" : dataReaderITEM.GetString(8),
                                        ItemGroup = dataReaderITEM.IsDBNull(9) ? "" : dataReaderITEM.GetString(9),
                                        CardCode = dataReaderITEM.IsDBNull(10) ? "" : dataReaderITEM.GetString(10),
                                        Substitute = dataReaderITEM.IsDBNull(11) ? "" : dataReaderITEM.GetString(11),
                                        PrecioMostrador = dataReaderITEM.IsDBNull(12) ? "" : dataReaderITEM.GetString(12),
                                        U_VtasPiezas = dataReaderITEM.IsDBNull(13) ? 0 : dataReaderITEM.GetInt32(13),

                                    }
                                   );
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerArticulosdeSAP");
                                return Json(ListaDeArticulos, JsonRequestBehavior.AllowGet);
                            }
                            finally
                            {
                                if (dataReaderITEM != null)
                                {
                                    dataReaderITEM.Close();
                                    comandoSQL.Dispose();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerArticulosdeSAP");
            }

            return Json(ListaDeArticulos, JsonRequestBehavior.AllowGet);
        }// fin ObtenerArticulosdeSAP
        
        [HttpPost]
        public ActionResult ObtenerArticulosdeSAPN(string cardCode, string whsCode, int priceList, string descuento, string filtro)
        {
            List<Articulos> ListaDeArticulos = new List<Articulos>();

            try
            {
                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();

                    using (var comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = "SP_ObtenerArticulosN";
                        comandoSQL.CommandType = CommandType.StoredProcedure;

                        comandoSQL.Parameters.AddWithValue("CardCode", cardCode);
                        comandoSQL.Parameters.AddWithValue("WareHouse", whsCode);
                        comandoSQL.Parameters.AddWithValue("PriceList", priceList);
                        comandoSQL.Parameters.AddWithValue("descuento", descuento);
                        comandoSQL.Parameters.AddWithValue("SearchTerm", string.IsNullOrEmpty(filtro) ? (object)DBNull.Value : filtro);

                        using (var dataReader = comandoSQL.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                ListaDeArticulos.Add(new Articulos
                                {
                                    ItemCode = dataReader.IsDBNull(0) ? "" : dataReader.GetString(0),
                                    ItemName = dataReader.IsDBNull(1) ? "" : dataReader.GetString(1),
                                    VATLiable = dataReader.IsDBNull(2) ? "" : dataReader.GetString(2),
                                    NoDiscount = dataReader.IsDBNull(3) ? "" : dataReader.GetString(3),
                                    TaxCodeAR = dataReader.IsDBNull(4) ? "" : dataReader.GetString(4),
                                    IndirectTax = dataReader.IsDBNull(5) ? "" : dataReader.GetString(5),
                                    Stock = dataReader.IsDBNull(6) ? 0 : dataReader.GetDecimal(6),
                                    UnitSize = dataReader.IsDBNull(7) ? "" : dataReader.GetString(7),
                                    Price = dataReader.IsDBNull(8) ? "" : dataReader.GetString(8),
                                    ItemGroup = dataReader.IsDBNull(9) ? "" : dataReader.GetString(9),
                                    CardCode = dataReader.IsDBNull(10) ? "" : dataReader.GetString(10),
                                    Substitute = dataReader.IsDBNull(11) ? "" : dataReader.GetString(11),
                                    PrecioMostrador = dataReader.IsDBNull(12) ? "" : dataReader.GetString(12),
                                    U_VtasPiezas = dataReader.IsDBNull(13) ? 0 : dataReader.GetInt32(13),
                                    //NUEVO
                                    U_Leyenda = dataReader.IsDBNull(14) ? "" : dataReader.GetString(14)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerArticulosdeSAPN");
            }

            return Json(ListaDeArticulos, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ObtenerArticulosSAPVendedor(string cardCode, string whsCode, int priceList)
        {
            List<Articulos> ListaDeArticulos = new List<Articulos>();

            try
            {
                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();

                    using (var comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = "SP_ObtenerArticulosVendedor";
                        comandoSQL.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter cardCodeParameter = comandoSQL.CreateParameter();
                        cardCodeParameter.ParameterName = "CardCode";
                        cardCodeParameter.DbType = System.Data.DbType.String;
                        cardCodeParameter.Value = cardCode;
                        comandoSQL.Parameters.Add(cardCodeParameter);

                        SqlParameter whsCodeParameter = comandoSQL.CreateParameter();
                        whsCodeParameter.ParameterName = "WareHouse";
                        whsCodeParameter.DbType = System.Data.DbType.String;
                        whsCodeParameter.Value = whsCode;
                        comandoSQL.Parameters.Add(whsCodeParameter);

                        SqlParameter pricelistParameter = comandoSQL.CreateParameter();
                        pricelistParameter.ParameterName = "PriceList";
                        pricelistParameter.DbType = System.Data.DbType.Int16;
                        pricelistParameter.Value = priceList;
                        comandoSQL.Parameters.Add(pricelistParameter);

                        

                        using (SqlDataReader dataReaderITEM = comandoSQL.ExecuteReader())
                        {
                            try
                            {
                                while (dataReaderITEM.Read())
                                {
                                    ListaDeArticulos.Add(new Articulos()
                                    {
                                        ItemCode = dataReaderITEM.IsDBNull(0) ? "" : dataReaderITEM.GetString(0),
                                        ItemName = dataReaderITEM.IsDBNull(1) ? "" : dataReaderITEM.GetString(1),
                                        VATLiable = dataReaderITEM.IsDBNull(2) ? "" : dataReaderITEM.GetString(2),
                                        NoDiscount = dataReaderITEM.IsDBNull(3) ? "" : dataReaderITEM.GetString(3),
                                        TaxCodeAR = dataReaderITEM.IsDBNull(4) ? "" : dataReaderITEM.GetString(4),
                                        IndirectTax = dataReaderITEM.IsDBNull(5) ? "" : dataReaderITEM.GetString(5),
                                        Stock = dataReaderITEM.IsDBNull(6) ? 0 : dataReaderITEM.GetDecimal(6),
                                      
                                        UnitSize = dataReaderITEM.IsDBNull(7) ? "" : dataReaderITEM.GetString(7),
                                        Price = dataReaderITEM.IsDBNull(8) ? "" : dataReaderITEM.GetString(8),
                                        ItemGroup = dataReaderITEM.IsDBNull(9) ? "" : dataReaderITEM.GetString(9),
                                        CardCode = dataReaderITEM.IsDBNull(10) ? "" : dataReaderITEM.GetString(10),
                                        Substitute = dataReaderITEM.IsDBNull(11) ? "" : dataReaderITEM.GetString(11),
                                        U_VtasPiezas = dataReaderITEM.IsDBNull(12) ? 0 : dataReaderITEM.GetInt32(12),
                                        //WhereHose = dataReaderITEM.IsDBNull(13) ? "" : dataReaderITEM.GetString(13),
                                        //nuevo
                                        U_Leyenda = dataReaderITEM.IsDBNull(14) ? "" : dataReaderITEM.GetString(14)
                                    } 
                                   );
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerArticulosdeSAP");
                                return Json(ListaDeArticulos, JsonRequestBehavior.AllowGet);
                            }
                            finally
                            {
                                if (dataReaderITEM != null)
                                {
                                    dataReaderITEM.Close();
                                    comandoSQL.Dispose();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerArticulosdeSAP");
            }

            return Json(ListaDeArticulos, JsonRequestBehavior.AllowGet);
        }// fin ObtenerArticulosdeSAP

        public ActionResult GuardarPedidoEnLocal(Pedido pedido)
        {
            List<Articulos> ListaDeArticulos = new List<Articulos>();

            try
            {
                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();

                    using (var comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = "SP_InsertarPedido";
                        comandoSQL.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter cardCodeParameter = comandoSQL.CreateParameter();
                        cardCodeParameter.ParameterName = "CardCode";
                        cardCodeParameter.DbType = System.Data.DbType.String;
                        cardCodeParameter.Value = pedido.CardCode;
                        comandoSQL.Parameters.Add(cardCodeParameter);

                        SqlParameter fechaCreacion = comandoSQL.CreateParameter();
                        fechaCreacion.ParameterName = "FechaCreacion";
                        fechaCreacion.DbType = System.Data.DbType.String;
                        fechaCreacion.Value = pedido.DocDate;
                        comandoSQL.Parameters.Add(fechaCreacion);

                        SqlParameter whsCodeParameter = comandoSQL.CreateParameter();
                        whsCodeParameter.ParameterName = "FechaEntrega";
                        whsCodeParameter.DbType = System.Data.DbType.String;
                        whsCodeParameter.Value = pedido.DocDueDate;
                        comandoSQL.Parameters.Add(whsCodeParameter);

                        using (SqlDataReader dataReaderITEM = comandoSQL.ExecuteReader())
                        {
                            try
                            {
                                while (dataReaderITEM.Read())
                                {
                                    ListaDeArticulos.Add(new Articulos()
                                    {
                                        ItemCode = dataReaderITEM.IsDBNull(0) ? "" : dataReaderITEM.GetString(0),
                                        ItemName = dataReaderITEM.IsDBNull(1) ? "" : dataReaderITEM.GetString(1),
                                        VATLiable = dataReaderITEM.IsDBNull(2) ? "" : dataReaderITEM.GetString(2),
                                        NoDiscount = dataReaderITEM.IsDBNull(3) ? "" : dataReaderITEM.GetString(3),
                                        TaxCodeAR = dataReaderITEM.IsDBNull(4) ? "" : dataReaderITEM.GetString(4),
                                        IndirectTax = dataReaderITEM.IsDBNull(5) ? "" : dataReaderITEM.GetString(5),
                                        Stock = dataReaderITEM.IsDBNull(6) ? 0 : dataReaderITEM.GetDecimal(6),
                                        UnitSize = dataReaderITEM.IsDBNull(7) ? "" : dataReaderITEM.GetString(7),
                                        Price = dataReaderITEM.IsDBNull(8) ? "" : dataReaderITEM.GetString(8),
                                        WhereHose = dataReaderITEM.IsDBNull(9) ? "" : dataReaderITEM.GetString(9),
                                        ItemGroup = dataReaderITEM.IsDBNull(10) ? "" : dataReaderITEM.GetString(10),
                                        CardCode = dataReaderITEM.IsDBNull(11) ? "" : dataReaderITEM.GetString(11),
                                        Substitute = dataReaderITEM.IsDBNull(12) ? "" : dataReaderITEM.GetString(12)
                                    }
                                   );
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtility.LogException(ex, "LogicaPedido.GuardarPedidoEnLocal");
                                return Json(ListaDeArticulos, JsonRequestBehavior.AllowGet);
                            }
                            finally
                            {
                                if (dataReaderITEM != null)
                                {
                                    dataReaderITEM.Close();
                                    comandoSQL.Dispose();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "LogicaPedido.GuardarPedidoEnLocal");
            }

            return Json(ListaDeArticulos, JsonRequestBehavior.AllowGet);
        }// fin GuardarPedidoEnLocal

        //Metodo para obtener el impuesto correspondiente
        public ActionResult ObtenerImpuestoDeSAP(string TaxCode)
        {
            decimal valorImpuesto=-1;

            try
            {
                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();
                    using (SqlCommand comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = "SP_ObtenerImpuesto";
                        comandoSQL.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter taxCodeParameter = comandoSQL.CreateParameter();
                        taxCodeParameter.ParameterName = "codigoImpuesto";
                        taxCodeParameter.DbType = System.Data.DbType.String;
                        taxCodeParameter.Value = TaxCode;
                        comandoSQL.Parameters.Add(taxCodeParameter);

                        using (SqlDataReader dataReaderTax = comandoSQL.ExecuteReader())
                        {
                            try
                            {
                                while (dataReaderTax.Read())
                                {
                                    valorImpuesto = dataReaderTax.GetDecimal(0);
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerImpuestoDeSAP");
                                return Json(valorImpuesto, JsonRequestBehavior.AllowGet);
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
                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerImpuestoDeSAP");
            }
            
            return Json(valorImpuesto, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ObtenerCodigoImpuestoBP(string CardCode)
        {
            string TaxCodeBP = string.Empty;
            ImpuestoBP taxBP = null;

            try
            {
                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();
                    using (SqlCommand comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = "SP_ObtenerTaxCodeBP";
                        comandoSQL.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter cardCodeParameter = comandoSQL.CreateParameter();
                        cardCodeParameter.ParameterName = "CardCode";
                        cardCodeParameter.DbType = System.Data.DbType.String;
                        cardCodeParameter.Value = CardCode;
                        comandoSQL.Parameters.Add(cardCodeParameter);

                        using (SqlDataReader dataReaderTax = comandoSQL.ExecuteReader())
                        {
                            try
                            {
                                while (dataReaderTax.Read())
                                {
                                    //TaxCodeBP = dataReaderTax.GetString(0);
                                    taxBP = new ImpuestoBP()
                                    {
                                        TaxCode = dataReaderTax.GetString(0),
                                        Rate = dataReaderTax.GetDecimal(1)
                                    };
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerCodigoImpuestoBP");
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
                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerCodigoImpuestoBP");
                ImpuestoBP taxBP1 = new ImpuestoBP();
                taxBP1.Rate = -1;
                taxBP1.TaxCode = "NoNCode";
                return Json(taxBP, JsonRequestBehavior.AllowGet);
            }

            return Json(taxBP, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ObtenerImpuestoDefault()
        {
            dynamic valorJson = null;

            try
            {
                string TaxCodeImpuestoDefault = ConfigurationManager.AppSettings["DefaultTaxCodeBP"];

                valorJson = ObtenerImpuestoDeSAP(TaxCodeImpuestoDefault);
                return Json(valorJson.Data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerImpuestoDefault");
                valorJson = -1;
                return Json(valorJson, JsonRequestBehavior.AllowGet);
            }  
        }
        
        public ActionResult ObtenerTasasDeCambio( string fecha)
        {
            List<TasasDeCambio> ListaTasasDeCambio = new List<TasasDeCambio>();

            try
            {
                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();

                    using (var comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = "SP_ObtenerTasasDeCambio";
                        comandoSQL.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter fechaParameter = comandoSQL.CreateParameter();
                        fechaParameter.ParameterName = "fechaDeHoy";
                        fechaParameter.DbType = System.Data.DbType.String;
                        fechaParameter.Value = fecha;
                        comandoSQL.Parameters.Add(fechaParameter);

                        using (SqlDataReader dataRedarRate = comandoSQL.ExecuteReader())
                        {
                            try
                            {
                                while (dataRedarRate.Read())
                                {
                                    ListaTasasDeCambio.Add(new TasasDeCambio()
                                    {
                                        RateDate = dataRedarRate.GetDateTime(0).ToShortDateString(),
                                        Currency = dataRedarRate.IsDBNull(1) ? "" : dataRedarRate.GetString(1),
                                        Rate = dataRedarRate.IsDBNull(2) ? 0 : dataRedarRate.GetDecimal(2)
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerTasasDeCambio");
                                return Json(ListaTasasDeCambio, JsonRequestBehavior.AllowGet);
                            }
                            finally
                            {
                                if (dataRedarRate != null)
                                {
                                    dataRedarRate.Close();
                                    comandoSQL.Dispose();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerTasasDeCambio");
            }

            return Json(ListaTasasDeCambio, JsonRequestBehavior.AllowGet);
        }// fin ObtenerSociosDeNegocios
        
        [HttpPost]
        public ActionResult ListarDirecciones(string cardcode) {
            List<Direccion> _direcciones = new List<Direccion>();
            try
            {
                using (SqlConnection conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();
                    using (SqlCommand comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = $"select ISNULL(Address, '')  Address, ISNULL(Street,'')  Street, ISNULL(Block,'')  Block, ISNULL(ZipCode,'')  ZipCode, ISNULL(City, '')  City, ISNULL(Country,'') Country, ISNULL(State,'') State from CRD1 where CardCode = '{cardcode}' AND AdresType = 'S'";
                        comandoSQL.CommandType = System.Data.CommandType.Text;

                        using (SqlDataReader reader = comandoSQL.ExecuteReader())
                        {
                          
                            while (reader.Read())
                            {
                                _direcciones.Add(new Direccion()
                                {
                                    Address = reader.GetString(0),
                                    Street =  reader.GetString(1),
                                    Block =   reader.GetString(2),
                                    ZipCode = reader.GetString(3),
                                    City =    reader.GetString(4),
                                    Country = reader.GetString(5),
                                    State = reader.GetString(6)
                                });
                            }
                        }
                    }
                }
                return Json(new { codigo = 0, mensaje = _direcciones}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Listar direcciones");
                throw;
            }
        }
        
        public ActionResult DatosIniciales()
        {
            string codigoVendedor = "";
            string SlpCode = "";
            string Rol = "";

            try
            {
                var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                SlpCode = Identity.Claims.Where(c => c.Type == ClaimTypes.Actor).Select(c => c.Value).SingleOrDefault();
                codigoVendedor = User.Identity.Name;

                Rol = Identity.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).SingleOrDefault(); //Rol

                return Json(Rol, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerSociosDeNegocios");
                return Json(Rol, JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult ObtenerClienteDeNegocios()
        {
            SocioDeNegocios ListaSocioNegocios = new SocioDeNegocios();
            string codigoVendedor = "";

            string SlpCode = "";

            try
            {
                var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                SlpCode = Identity.Claims.Where(c => c.Type == ClaimTypes.Actor).Select(c => c.Value).SingleOrDefault();
                codigoVendedor = User.Identity.Name;

                if (string.IsNullOrEmpty(SlpCode))
                {
                    ListaSocioNegocios=(new SocioDeNegocios()
                    {
                        CardCode = "-33",
                        CardName = "Tiempo de Sesión agotado.",
                        ListNum = 0,
                        Moneda = "",
                        Discount = 0,
                        DiscInRet = ""
                    });
                    return Json(ListaSocioNegocios, JsonRequestBehavior.AllowGet);
                }

                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();

                    using (var comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = "SP_ObtenerSNCliente";
                        comandoSQL.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter compradorParameter = comandoSQL.CreateParameter();
                        compradorParameter.ParameterName = "Cliente";
                        compradorParameter.DbType = System.Data.DbType.String;
                        compradorParameter.Value = SlpCode;
                        comandoSQL.Parameters.Add(compradorParameter);

                        using (SqlDataReader dataReaderSN = comandoSQL.ExecuteReader())
                        {
                            try
                            {
                                while (dataReaderSN.Read())
                                {
                                    ListaSocioNegocios=(new SocioDeNegocios()
                                    {
                                        CardCode = dataReaderSN.IsDBNull(0) ? "" : dataReaderSN.GetString(0),
                                        CardName = dataReaderSN.IsDBNull(1) ? "" : dataReaderSN.GetString(1),
                                        ListNum = dataReaderSN.IsDBNull(2) ? -1 : dataReaderSN.GetInt16(2),
                                        Moneda = dataReaderSN.IsDBNull(3) ? "" : dataReaderSN.GetString(3),
                                        Discount = dataReaderSN.IsDBNull(4) ? 0 : dataReaderSN.GetDecimal(4),
                                        DiscInRet = dataReaderSN.IsDBNull(5) ? "" : dataReaderSN.GetString(5),
                                        Saldo = dataReaderSN.IsDBNull(6) ? 0 : dataReaderSN.GetDecimal(6),
                                        Credito = dataReaderSN.IsDBNull(7) ? 0 : dataReaderSN.GetDecimal(7),
                                        EstatusMetodPago = dataReaderSN.IsDBNull(7) ? "" : dataReaderSN.GetString(8)

                                    });
                                }
                            }
                            
                            catch (Exception ex)
                            {
                                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerSociosDeNegocios");
                                return Json(ListaSocioNegocios, JsonRequestBehavior.AllowGet);
                            }
                            finally
                            {
                                if (dataReaderSN != null)
                                {
                                    dataReaderSN.Close();
                                    comandoSQL.Dispose();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerSociosDeNegocios");
            }

            return Json(ListaSocioNegocios, JsonRequestBehavior.AllowGet);
        }// fin ObtenerSociosDeNegocios

        public ActionResult ObtenerStockalamcen(string cardCode, string itemcode,string rol)
        {
            List<Inventario> ListaDeAlmacenStock = new List<Inventario>();

            try
            {
                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();

                    using (var comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = "SP_ObtenerStockAlmacen";
                        comandoSQL.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter cardCodeParameter = comandoSQL.CreateParameter();
                        cardCodeParameter.ParameterName = "CardCode";
                        cardCodeParameter.DbType = System.Data.DbType.String;
                        cardCodeParameter.Value = cardCode;
                        comandoSQL.Parameters.Add(cardCodeParameter);

                        SqlParameter pricelistParameter = comandoSQL.CreateParameter();
                        pricelistParameter.ParameterName = "ItemCode";
                        pricelistParameter.DbType = System.Data.DbType.String;
                        pricelistParameter.Value = itemcode;
                        comandoSQL.Parameters.Add(pricelistParameter);

                        SqlParameter rolParameter = comandoSQL.CreateParameter();
                        rolParameter.ParameterName = "Rol";
                        rolParameter.DbType = System.Data.DbType.String;
                        rolParameter.Value = rol;
                        comandoSQL.Parameters.Add(rolParameter);

                        using (SqlDataReader dataReaderITEMS = comandoSQL.ExecuteReader())
                        {
                            try
                            {
                                while (dataReaderITEMS.Read())
                                {
                                    ListaDeAlmacenStock.Add(new Inventario()
                                    {
                                        whsCode = dataReaderITEMS.IsDBNull(0) ? "" : dataReaderITEMS.GetString(0),
                                        Almacen = dataReaderITEMS.IsDBNull(1) ? "" : dataReaderITEMS.GetString(1),
                                       
                                        stock = dataReaderITEMS.IsDBNull(2) ? 0 : dataReaderITEMS.GetDecimal(2),
                                        comprometido = dataReaderITEMS.IsDBNull(3) ? 0 : dataReaderITEMS.GetDecimal(3),

                                    }
                                   );
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerArticulosdeSAP");
                                return Json(ListaDeAlmacenStock, JsonRequestBehavior.AllowGet);
                            }
                            finally
                            {
                                if (dataReaderITEMS != null)
                                {
                                    dataReaderITEMS.Close();
                                    comandoSQL.Dispose();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerArticulosdeSAP");
            }

            return Json(ListaDeAlmacenStock, JsonRequestBehavior.AllowGet);
        }// fin ObtenerArticulosdeSAP
        
        public ActionResult ObtenerStockalamcenInd( string itemcode, string almacen)
        {
            List<Inventario> ListaDeAlmacenStock = new List<Inventario>();

            try
            {
                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();

                    using (var comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = "SP_ObtenerStockAlmacenInd";
                        comandoSQL.CommandType = System.Data.CommandType.StoredProcedure;

                       

                        SqlParameter pricelistParameter = comandoSQL.CreateParameter();
                        pricelistParameter.ParameterName = "ItemCode";
                        pricelistParameter.DbType = System.Data.DbType.String;
                        pricelistParameter.Value = itemcode;
                        comandoSQL.Parameters.Add(pricelistParameter);

                        SqlParameter almacenParameter = comandoSQL.CreateParameter();
                        almacenParameter.ParameterName = "Almacen";
                        almacenParameter.DbType = System.Data.DbType.String;
                        almacenParameter.Value = almacen;
                        comandoSQL.Parameters.Add(almacenParameter);

                        using (SqlDataReader dataReaderITEMS = comandoSQL.ExecuteReader())
                        {
                            try
                            {
                                while (dataReaderITEMS.Read())
                                {
                                    ListaDeAlmacenStock.Add(new Inventario()
                                    {


                                        whsCode = dataReaderITEMS.IsDBNull(0) ? "" : dataReaderITEMS.GetString(0),
                                        Almacen = dataReaderITEMS.IsDBNull(1) ? "" : dataReaderITEMS.GetString(1),

                                        stock = dataReaderITEMS.IsDBNull(2) ? 0 : dataReaderITEMS.GetDecimal(2),


                                    }
                                   );
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtility.LogException(ex, "LogicaPedido.SP_ObtenerStockAlmacenInd");
                                return Json(ListaDeAlmacenStock, JsonRequestBehavior.AllowGet);
                            }
                            finally
                            {
                                if (dataReaderITEMS != null)
                                {
                                    dataReaderITEMS.Close();
                                    comandoSQL.Dispose();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerArticulosdeSAP");
            }

            return Json(ListaDeAlmacenStock, JsonRequestBehavior.AllowGet);
        }// fin ObtenerArticulosdeSAP

        public ActionResult ListarFormasPago(string cardcode)
        {
            List<FormasPago> _formapago = new List<FormasPago>();
            try
            {
                using (SqlConnection conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();
                    using (SqlCommand comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = $"select ISNULL(C.PymCode,''),ISNULL(O.DEscript, '') from CRD2 C  inner join OPYM O on C.PymCode = O.PayMethCod where C.CardCode = '{cardcode}'  AND(PymCode = '03' OR PymCode = '99')";
                        comandoSQL.CommandType = System.Data.CommandType.Text;

                        using (SqlDataReader reader = comandoSQL.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                _formapago.Add(new FormasPago()
                                {
                                    PymCode = reader.GetString(0),
                                    Descrip = reader.GetString(1),

                                });
                            }
                        }
                    }
                }
                return Json(new { codigo = 0, mensaje = _formapago }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Listar direcciones");
                throw;
            }
        }

        public ActionResult ListarCDFI()
        {
            List<CDFI> _listaCDFI = new List<CDFI>();
            try
            {
                using (SqlConnection conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();
                    using (SqlCommand comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = $"SELECT * FROM  [dbo].[@ECFD_CUSC] where  Code='G01'OR  Code='G03' ";
                        comandoSQL.CommandType = System.Data.CommandType.Text;

                        using (SqlDataReader reader = comandoSQL.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                _listaCDFI.Add(new CDFI()
                                {
                                    Code = reader.GetString(0),
                                    Descrip = reader.GetString(1),

                                });
                            }
                        }
                    }
                }
                return Json(new { codigo = 0, mensaje = _listaCDFI }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Listar direcciones");
                throw;
            }
        }

        public ActionResult ListarFormaEnvio()
        {
            List<FormaEnvio> _listaFormaEnvio= new List<FormaEnvio>();
            try
            {
                using (SqlConnection conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();
                    using (SqlCommand comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = $"select FldValue,Descr From UFD1 where FieldID = 49 and TableID = 'ORDR'";
                        comandoSQL.CommandType = System.Data.CommandType.Text;

                        using (SqlDataReader reader = comandoSQL.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                _listaFormaEnvio.Add(new FormaEnvio()
                                {
                                    Tipo = reader.GetString(0),
                                    Documento = reader.GetString(1),

                                });
                            }
                        }
                    }
                }
                return Json(new { codigo = 0, mensaje = _listaFormaEnvio }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Listar direcciones");
                throw;
            }
        }

        public ActionResult ListarTipoEnvio()
        {
            List<MetodoPago> _listaTipoEnvio = new List<MetodoPago>();
            try
            {
                using (SqlConnection conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();
                    using (SqlCommand comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = $"select FldValue,Descr From UFD1 where FieldID = 45 and TableID = 'ORDR'";
                        comandoSQL.CommandType = System.Data.CommandType.Text;

                        using (SqlDataReader reader = comandoSQL.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                _listaTipoEnvio.Add(new MetodoPago()
                                {
                                    Tipo = reader.GetString(0),
                                    Documento = reader.GetString(1),

                                });
                            }
                        }
                    }
                }
                return Json(new { codigo = 0, mensaje = _listaTipoEnvio }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Listar TipoEnvio");
                throw;
            }
        }
        
        public ActionResult ListarMetodoPago()
        {
            List<TipoEncio> _listaMetodoPago = new List<TipoEncio>();
            try
            {
                using (SqlConnection conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();
                    using (SqlCommand comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = $"select  FldValue,Descr From UFD1 where FieldID = 30 and TableID = 'ORDR'";
                        comandoSQL.CommandType = System.Data.CommandType.Text;

                        using (SqlDataReader reader = comandoSQL.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                _listaMetodoPago.Add(new TipoEncio()
                                {
                                    Tipo = reader.GetString(0),
                                    Documento = reader.GetString(1),

                                });
                            }
                        }
                    }
                }
                return Json(new { codigo = 0, mensaje = _listaMetodoPago }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "ListarMetodoPago");
                throw;
            }
        }

        public ActionResult RutaFormato(string CodigoFormato)
        {
            string U_PathReport = null;
            Formato _formato = new Formato();
            try
            {
                using (SqlConnection conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();
                    using (SqlCommand comandoSQL = conexionSQL.CreateCommand())
                    {
                        comandoSQL.CommandText = $"Exec [dbo].[SP_Obtener_Formato] @Tipo='{CodigoFormato}'";
                        comandoSQL.CommandType = System.Data.CommandType.Text;

                        using (SqlDataReader reader = comandoSQL.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                U_PathReport = reader.GetString(0);
                              
                            }
                        }
                    }
                }
                return View(Convert.ToInt64(U_PathReport));
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "ListarMetodoPago");
                throw;
            }
        }

        //******************************************************************************
        //******************************************************************************
        public ActionResult ValidarDocCliente(string tipoDoc, string DocNum, string tipo, string Cliente)
        {
            string DocEntry = string.Empty;
            string token = string.Empty;
            int code = -1;
            string msj = string.Empty;

            try
            {
                using (var conexionSQL = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                {
                    conexionSQL.Open();

                    using (var comandoSQL = conexionSQL.CreateCommand())
                    {
                        //exec SP_ValidaDocCliente @TipoDoc = 'order', @DocNum = '89287', @Tipo = 'DocNum', @CardCode = 'C0036'
                        comandoSQL.CommandText = "SP_ValidaDocCliente";
                        comandoSQL.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter TipoDocParameter = comandoSQL.CreateParameter();
                        TipoDocParameter.ParameterName = "TipoDoc";
                        TipoDocParameter.DbType = System.Data.DbType.String;
                        TipoDocParameter.Value = tipoDoc;
                        comandoSQL.Parameters.Add(TipoDocParameter);

                        SqlParameter docnumParameter = comandoSQL.CreateParameter();
                        docnumParameter.ParameterName = "DocNum";
                        docnumParameter.DbType = System.Data.DbType.String;
                        docnumParameter.Value = DocNum;
                        comandoSQL.Parameters.Add(docnumParameter);

                        SqlParameter TipoParameter = comandoSQL.CreateParameter();
                        TipoParameter.ParameterName = "Tipo";
                        TipoParameter.DbType = System.Data.DbType.String;
                        TipoParameter.Value = tipo;
                        comandoSQL.Parameters.Add(TipoParameter);

                        SqlParameter cardCodeParameter = comandoSQL.CreateParameter();
                        cardCodeParameter.ParameterName = "CardCode";
                        cardCodeParameter.DbType = System.Data.DbType.String;
                        cardCodeParameter.Value = Cliente;
                        comandoSQL.Parameters.Add(cardCodeParameter);

                        using (SqlDataReader dataReader = comandoSQL.ExecuteReader())
                        {
                            try
                            {
                                while (dataReader.Read())
                                {
                                    DocEntry = dataReader.IsDBNull(0) ? "" : dataReader.GetValue(0).ToString();
                                }

                                if (DocEntry != string.Empty) {
                                    code = 0;
                                    token = CryptoHelper.EncriptarConIV(DocEntry, TimeSpan.FromMinutes(5));
                                }
                                else
                                {
                                    code = -2;
                                }
                            }
                            catch (Exception ex)
                            {
                                msj = ex.Message.ToString();
                                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerArticulosdeSAP");
                                //return Json(ListaDeAlmacenStock, JsonRequestBehavior.AllowGet);
                            }
                            finally
                            {
                                if (dataReader != null)
                                {
                                    dataReader.Close();
                                    comandoSQL.Dispose();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msj = ex.Message.ToString();
                ExceptionUtility.LogException(ex, "LogicaPedido.ObtenerArticulosdeSAP");
            }

            return Json(new { codigo = code, mensaje = msj, tokenP = token }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ListarAniosFactura(string UserId)
        {
            List<FechaFactura> listaAnios = new List<FechaFactura>();
            try
            {
                using (SqlConnection conexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionSAPSQL"].ConnectionString))
                using (SqlCommand comando = new SqlCommand("SP_Consulta_AniosFacturaPorUsuario", conexion))
                {
                    if (UserId == "")
                    {
                        var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                        UserId = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
                    }

                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@UserId", UserId);

                    conexion.Open();
                    using (SqlDataReader reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string anio = reader.GetInt32(0).ToString();
                            listaAnios.Add(new FechaFactura
                            {
                                U_Anio = anio,
                                Anio = anio
                            });
                        }
                    }
                }

                return Json(new { codigo = 0, mensaje = listaAnios }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "ListarAniosFactura");
                return Json(new { codigo = -1, mensaje = "Error al obtener los años de facturas" }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}