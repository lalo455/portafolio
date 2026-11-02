using System;
using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Xml;
using Newtonsoft.Json.Linq;
using System.IO;
using SAPbobsCOM;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using System.Threading;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Cryptography;
using CRMs.Models;
using CRMs.Utilerias;
using CRMs.Helpers;
using CRMs.Clases;
using System.Globalization;

namespace CRMs.Controllers
{
    [Authorize]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class IntegracionSAPController : Controller
    {

        public Company oCompany = null;

        public ConexionSAP modeloConexionSAP()
        {
            ConexionSAP modelo = null;
            
            try
            {
               string cadenaConexionSAP = ConfigurationManager.ConnectionStrings["ConexionSAPDI"].ConnectionString;
                String[] substrings = cadenaConexionSAP.Split(';');
                modelo = new ConexionSAP()
                {

                    Server = substrings[0].Split('=')[0] == "Server" ? substrings[0].Split('=')[1] : "",
                    Database = substrings[1].Split('=')[0] == "Database" ? substrings[1].Split('=')[1] : "",
                    UserId = substrings[2].Split('=')[0] == "UserId" ? substrings[2].Split('=')[1] : "",
                    Password = substrings[3].Split('=')[0] == "Password" ? substrings[3].Split('=')[1] : "",
                    UserIdSAP = substrings[4].Split('=')[0] == "UserIdSAP" ? substrings[4].Split('=')[1] : "",
                    PasswordSAP = substrings[5].Split('=')[0] == "PasswordSAP" ? substrings[5].Split('=')[1] : "",
                    ServerType = substrings[6].Split('=')[0] == "ServerType" ? substrings[6].Split('=')[1] : "",
                    LicenseServer = substrings[7].Split('=')[0] == "LicenseServer" ? substrings[7].Split('=')[1] : ""
                };
                return modelo;
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "IntegracionSAP.modeloConexionSAP");
                ConexionSAP modelo1 = new ConexionSAP();
                modelo1.Server = "NoServer";
                return modelo1;
            }      
        }

        public RespuestaConexionSAP ConexionDI()
        {
            int valorConexion = 10101; //no conectado
            string error = "";
            RespuestaConexionSAP respConexion = null;
            
            try
            {
                ConexionSAP connectionSAP = modeloConexionSAP();
                oCompany = new SAPbobsCOM.Company();

                if(connectionSAP.Server == "NoServer")
                {
                    respConexion = new RespuestaConexionSAP() { Codigo = -10, Error = "NO-DIAPI: No se obtuvieron los Datos de Conexión a SAP." };
                    return respConexion;
                }

                oCompany.Server = $"{connectionSAP.Server}";
                oCompany.DbServerType = connectionSAP.ServerType == "MSSQL2017" ? BoDataServerTypes.dst_MSSQL2017 : BoDataServerTypes.dst_MSSQL2019;
                oCompany.CompanyDB = connectionSAP.Database; 
                oCompany.SLDServer = connectionSAP.LicenseServer; 
                oCompany.UserName = connectionSAP.UserIdSAP; 
                oCompany.Password = connectionSAP.PasswordSAP; 
                oCompany.DbUserName = connectionSAP.UserId; 
                oCompany.DbPassword = connectionSAP.Password; 
                oCompany.language = BoSuppLangs.ln_Spanish_La;

                valorConexion = oCompany.Connect();
                
                if(valorConexion != 0)
                {
                    error = oCompany.GetLastErrorDescription();
                    respConexion = new RespuestaConexionSAP() { Codigo = valorConexion, Error = error };
                    ExceptionUtility.LogMessageDIAPI(connectionSAP.UserIdSAP, (oCompany.GetLastErrorCode()).ToString(), oCompany.GetLastErrorDescription(),"NA");
                }
                else
                {
                    respConexion = new RespuestaConexionSAP() { Codigo = valorConexion, Error = error};
                }
                
                return respConexion;

            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "IntegracionSAP.ConexionDI");
                respConexion = new RespuestaConexionSAP() { Codigo = valorConexion, Error = error };

                return respConexion;     
            }
        }

        public ActionResult IntegrarOrden(string jsonPedido)
        {
            //string userIDweb = User.Identity.GetUserId().ToString();
            var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            int respuestaPedido = 20202;
            string error = "";
            string docEntryPedido = "nointegrado";
            double totalDescuento = 0;
            DateTime docDate = DateTime.Now;
            RespuestaConexionSAP respSAP = null;
            string tipoDoc = "";
            string token = "";

            try
            {
                dynamic order = JsonConvert.DeserializeObject(jsonPedido);
                JArray lineas = (JArray)order.DocumentLines;
                //DateTime docDueDate = Convert.ToDateTime(order.DocDueDate);
                string[] formatos = { "dd-MM-yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "dd-MM-yyyyTHH:mm:ss", "dd-MM-yyyy HH:mm:ss" };
                DateTime docDueDate = DateTime.ParseExact(
                    (string)order.DocDueDate,
                    formatos,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None
                );
                string userIDweb = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();

                if (string.IsNullOrEmpty(userIDweb))
                {
                    return Json(new { codigo = -33, mensaje = "Se agotó el tiempo de Sesión" }, JsonRequestBehavior.AllowGet);
                }

                respSAP = ConexionDI();
                
                if (respSAP.Codigo != 0)// si no hay conexion
                {
                    error = oCompany.GetLastErrorDescription();
                    return Json(new { codigo = respSAP.Codigo, mensaje = respSAP.Error }, JsonRequestBehavior.AllowGet);
                }
                else//crear el pedido
                {
                    SAPbobsCOM.Documents pedido = null;
                    //validar orden o cotizacion
                    if (order.DocBase == "Pedido")
                    {
                        pedido = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(BoObjectTypes.oOrders);
                        tipoDoc = "Pedido";
                    }
                    else if (order.DocBase == "Cotizacion")
                    {
                        pedido = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(BoObjectTypes.oQuotations);
                        tipoDoc = "Cotizacion";
                    }                    
                    
                    pedido.CardCode = order.CardCode;
                    pedido.DocDate = docDate;
                    pedido.DocDueDate = docDueDate;
                    pedido.Comments = order.Comments;
                    pedido.NumAtCard = Convert.ToString(order.U_NumPed);
                    //pedido.UserFields.Fields.Item("U_NumPedido").Value = Convert.ToString(order.U_NumPed);
                    //pedido.DiscountPercent = order.Discount;
                    pedido.UserFields.Fields.Item("U_UserPortal").Value = userIDweb;
                    pedido.ShipToCode = order.Address;
                    pedido.UserFields.Fields.Item("U_Envio").Value = order.U_envio;
                    pedido.UserFields.Fields.Item("U_PAQUETERIA").Value = order.U_Paquetería;
                    if (order.DocBase == "Pedido")
                    {
                        pedido.UserFields.Fields.Item("U_MetPago").Value = order.MetodoPago;
                        pedido.PaymentMethod = order.FormaPago;
                        pedido.UserFields.Fields.Item("U_UsoCFDI").Value = order.CDFI;
                    }
                    

                    for (int i = 0; i < lineas.Count; i++)
                    {
                        //DateTime ItemDueDate = Convert.ToDateTime(order.DocumentLines[i].DiasEntrega);
                        DateTime ItemDueDate = DateTime.ParseExact(
                             (string)order.DocumentLines[i].DiasEntrega,
                             formatos,
                             CultureInfo.InvariantCulture,
                             DateTimeStyles.None
                         );
                        pedido.Lines.ItemCode = order.DocumentLines[i].ItemCode;

                        double desc1 = 100 - Convert.ToDouble(order.Discount);
                        double desc2 = 100 - Convert.ToDouble(order.DocumentLines[i].PrtgDiscount);
                        double promDesc = (desc1 * desc2) / 100;
                        totalDescuento = 100 - promDesc;
                        pedido.Lines.UnitPrice = order.DocumentLines[i].PrecioUnitario;
                        pedido.Lines.UserFields.Fields.Item("U_Desc1").Value = Convert.ToDouble(order.Discount);
                        pedido.Lines.UserFields.Fields.Item("U_Desc2").Value = Convert.ToDouble(order.DocumentLines[i].PrtgDiscount);
                        pedido.Lines.DiscountPercent = totalDescuento;//order.Discount;
                        pedido.Lines.Quantity = order.DocumentLines[i].Quantity;
                        pedido.Lines.VatGroup = order.DocumentLines[i].VatGroup;
                        pedido.Lines.WarehouseCode = order.DocumentLines[i].WarehouseCode;
                        pedido.Lines.ShipDate = ItemDueDate;
                        if (order.DocBase == "Cotizacion")
                        {
                            pedido.Lines.UnitPrice = order.DocumentLines[i].PrecioUnitario;
                        }
                        //pedido.Lines.Currency = order.DocumentLines[i].Currency == "$" ? "MXP" : order.DocumentLines[i].Currency;
                        pedido.Lines.Add();
                    }
                    if (pedido.Add() != 0)
                    {
                        docEntryPedido = oCompany.GetLastErrorDescription();
                        ExceptionUtility.LogMessageDIAPI(userIDweb, (oCompany.GetLastErrorCode()).ToString(), oCompany.GetLastErrorDescription(), Convert.ToString(order.U_NumPed));
                        //oCompany.Disconnect();
                    }
                    else
                    {
                        respuestaPedido = 0;
                        docEntryPedido = oCompany.GetNewObjectKey();
                        docEntryPedido = Convert.ToString(ReturnDocNum(docEntryPedido, tipoDoc)); //Obtiene DocNum de acuerdo al DocEntry del pedido generado

                        token = CryptoHelper.EncriptarConIV(docEntryPedido, TimeSpan.FromMinutes(5));
                    }

                    //oCompany.Disconnect();
                    return Json(new { codigo = respuestaPedido, mensaje = docEntryPedido, tokenP = token }, JsonRequestBehavior.AllowGet);
                }    
            }
            catch (Exception ex)
            {
                if (oCompany != null)
                {
                    if (oCompany.Connected)
                        oCompany.Disconnect();
                    Marshal.ReleaseComObject(oCompany);
                    oCompany = null;
                }
                //oCompany.Disconnect();
                ExceptionUtility.LogException(ex, "IntegracionSAP.IntegrarOrden");
                return Json(new { codigo = oCompany.GetLastErrorCode(), mensaje = oCompany.GetLastErrorDescription() });
            }
            finally
            {
                if (oCompany != null)
                {
                    if (oCompany.Connected)
                    {
                        oCompany.Disconnect();
                    }
                        
                    Marshal.ReleaseComObject(oCompany);
                    oCompany = null;
                }
            }
                
        }
        
        public ActionResult ActualizarOrden(string jsonPedido)
        {
            //string userIDweb = User.Identity.GetUserId().ToString();
            var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            int respuestaPedido = 20202;
            string error = "";
            string docEntryPedido = "nointegrado";
            double totalDescuento = 0;
            DateTime docDate = DateTime.Now;
            RespuestaConexionSAP respSAP = null;

            try
            {
                dynamic order = JsonConvert.DeserializeObject(jsonPedido);
                JArray lineas = (JArray)order.DocumentLines;
                //DateTime docDueDate = Convert.ToDateTime(order.DocDueDate);
                string[] formatos = {
                    "dd-MM-yyyy", "dd/MM/yyyy",
                    "yyyy-MM-dd", "yyyy/MM/dd",
                    "yyyy-MM-ddTHH:mm:ss", "dd-MM-yyyyTHH:mm:ss",
                    "dd-MM-yyyy HH:mm:ss", "yyyy-MM-dd HH:mm:ss"
                };

                DateTime docDueDate;
                if (!DateTime.TryParseExact(
                        (string)order.DocDueDate,
                        formatos,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out docDueDate))
                {
                    throw new FormatException("Formato inválido en DocDueDate: " + order.DocDueDate);
                }

                string userIDweb = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();

                if (string.IsNullOrEmpty(userIDweb))
                {
                    return Json(new { codigo = -33, mensaje = "Se agotó el tiempo de Sesión" }, JsonRequestBehavior.AllowGet);
                }

                respSAP = ConexionDI();

                if (respSAP.Codigo != 0)// si no hay conexion
                {
                    error = oCompany.GetLastErrorDescription();
                    return Json(new { codigo = respSAP.Codigo, mensaje = respSAP.Error }, JsonRequestBehavior.AllowGet);
                }
                else//crear el pedido
                {
                    SAPbobsCOM.Documents pedido = null;
                    pedido = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(BoObjectTypes.oQuotations);
                    pedido.GetByKey(order.DocNumber);
                    pedido.DocDueDate = docDueDate;
                    //Recorrido de las lineas del documento de la cotización
                    for (int j = 0; j < lineas.Count; j++)
					{
						//Seteo de la linea actual de la cotizacón
                        if (Convert.ToInt32(order.DocumentLines[j].LineNum) !=-1)
                        {
					        for(int i = 0; i < pedido.Lines.Count; i++)
						    {
						        pedido.Lines.SetCurrentLine(i);
							    if (pedido.Lines.LineNum == Convert.ToInt32(order.DocumentLines[j].LineNum)) //Find your line number that you want update.
							    {
                                    if (order.DocumentLines[j].Delete == true)
                                    {
                                        pedido.Lines.Delete();
                                    }
                                    else
                                    {
								        double desc1 = 100 - Convert.ToDouble(order.Discount);
								        double desc2 = 100 - Convert.ToDouble(order.DocumentLines[j].PrtgDiscount);
								        double promDesc = (desc1 * desc2) / 100;
								        totalDescuento = 100 - promDesc;
								
								        pedido.Lines.Price = order.DocumentLines[j].PrecioUnitario;
								        pedido.Lines.UserFields.Fields.Item("U_Desc1").Value = Convert.ToDouble(order.Discount);
								        pedido.Lines.UserFields.Fields.Item("U_Desc2").Value = Convert.ToDouble(order.DocumentLines[j].PrtgDiscount);
								        //pedido.Lines.DiscountPercent = totalDescuento;//order.Discount;
								        pedido.Lines.Quantity = order.DocumentLines[j].Quantity;
								        pedido.Lines.VatGroup = order.DocumentLines[j].VatGroup;
								        pedido.Lines.WarehouseCode = order.DocumentLines[j].WarehouseCode;
                                        pedido.Lines.UnitPrice = order.DocumentLines[i].PrecioUnitario;

                                    }
                                }
                            }
                        }
                        else
                        {
                            double desc1 = 100 - Convert.ToDouble(order.Discount);
                            double desc2 = 100 - Convert.ToDouble(order.DocumentLines[j].PrtgDiscount);
                            double promDesc = (desc1 * desc2) / 100;
                            totalDescuento = 100 - promDesc;

                            pedido.Lines.Add();
                            pedido.Lines.ItemCode = order.DocumentLines[j].ItemCode;
                            pedido.Lines.Price = order.DocumentLines[j].PrecioUnitario;
                            pedido.Lines.UserFields.Fields.Item("U_Desc1").Value = Convert.ToDouble(order.Discount);
                            pedido.Lines.UserFields.Fields.Item("U_Desc2").Value = Convert.ToDouble(order.DocumentLines[j].PrtgDiscount);
                            //pedido.Lines.DiscountPercent = totalDescuento;//order.Discount;
                            pedido.Lines.Quantity = order.DocumentLines[j].Quantity;
                            pedido.Lines.VatGroup = order.DocumentLines[j].VatGroup;
                            pedido.Lines.WarehouseCode = order.DocumentLines[j].WarehouseCode;
                            pedido.Lines.UnitPrice = order.DocumentLines[j].PrecioUnitario;

                        }

                    }					
					
                    if (pedido.Update() != 0)
                    {
                        docEntryPedido = oCompany.GetLastErrorDescription();
                        ExceptionUtility.LogMessageDIAPI(userIDweb, (oCompany.GetLastErrorCode()).ToString(), oCompany.GetLastErrorDescription(), Convert.ToString(order.U_NumPed));
                    }
                    else
                    {
                        respuestaPedido = 0;
                        docEntryPedido = pedido.DocNum.ToString();
                        docEntryPedido = Convert.ToString(ReturnDocNum(docEntryPedido, "Cotizacion"));
                    }
                    //oCompany.Disconnect();
                    return Json(new { codigo = respuestaPedido, mensaje = docEntryPedido }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                if (oCompany != null)
                {
                    if (oCompany.Connected)
                        oCompany.Disconnect();
                    Marshal.ReleaseComObject(oCompany);
                    oCompany = null;
                }
                //oCompany.Disconnect();
                ExceptionUtility.LogException(ex, "IntegracionSAP.IntegrarOrden");
                return Json(new { codigo = oCompany.GetLastErrorCode(), mensaje = oCompany.GetLastErrorDescription() });
            }
            finally
            {
                if (oCompany != null)
                {
                    if (oCompany.Connected)
                    {
                        oCompany.Disconnect();
                    }

                    Marshal.ReleaseComObject(oCompany);
                    oCompany = null;
                }
            }

        }
       
        public ActionResult CerrarOrden(int DocEntry, string Motivo)
        {
            //string userIDweb = User.Identity.GetUserId().ToString();
            var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            int respuestaPedido = 20202;
            string error = "";
            string docEntryPedido = "nointegrado";
            RespuestaConexionSAP respSAP = null;

            try
            {
                //dynamic order = JsonConvert.DeserializeObject(jsonPedido);
                string userIDweb = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();

                if (string.IsNullOrEmpty(userIDweb))
                {
                    return Json(new { codigo = -33, mensaje = "Se agotó el tiempo de Sesión" }, JsonRequestBehavior.AllowGet);
                }

                respSAP = ConexionDI();

                if (respSAP.Codigo != 0)// si no hay conexion
                {
                    error = oCompany.GetLastErrorDescription();
                    return Json(new { codigo = respSAP.Codigo, mensaje = respSAP.Error }, JsonRequestBehavior.AllowGet);
                }
                else//crear el pedido
                {
                    SAPbobsCOM.Documents pedido = null;
                    //validar orden o cotizacion
                    pedido = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(BoObjectTypes.oQuotations);
                    pedido.GetByKey(DocEntry);
                    pedido.UserFields.Fields.Item("U_MotivoCerrado").Value = Convert.ToString(Motivo);
                    //pedido.DiscountPercent = order.Discount;
                    if (pedido.Update() == 0)
                    {
                        if (pedido.Close() == 0)
                        {
                            respuestaPedido = 0;
                            docEntryPedido = oCompany.GetNewObjectKey();
                            docEntryPedido = Convert.ToString(ReturnDocNum(docEntryPedido, "Cotizacion"));
                        }
                        else
                        {
                            docEntryPedido = oCompany.GetLastErrorDescription();
                            ExceptionUtility.LogMessageDIAPI(userIDweb, (oCompany.GetLastErrorCode()).ToString(), oCompany.GetLastErrorDescription(), Convert.ToString(DocEntry));
                            //oCompany.Disconnect();                        
                        }
                    }
                    else
                    {
                        docEntryPedido = oCompany.GetLastErrorDescription();
                        ExceptionUtility.LogMessageDIAPI(userIDweb, (oCompany.GetLastErrorCode()).ToString(), oCompany.GetLastErrorDescription(), Convert.ToString(DocEntry));
                        //oCompany.Disconnect();                        
                    }
                    //oCompany.Disconnect();
                    return Json(new { codigo = respuestaPedido, mensaje = docEntryPedido }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                if (oCompany != null)
                {
                    if (oCompany.Connected)
                        oCompany.Disconnect();
                    Marshal.ReleaseComObject(oCompany);
                    oCompany = null;
                }
                //oCompany.Disconnect();
                ExceptionUtility.LogException(ex, "IntegracionSAP.IntegrarOrden");
                return Json(new { codigo = oCompany.GetLastErrorCode(), mensaje = oCompany.GetLastErrorDescription() });
            }
            finally
            {
                if (oCompany != null)
                {
                    if (oCompany.Connected)
                    {
                        oCompany.Disconnect();
                    }

                    Marshal.ReleaseComObject(oCompany);
                    oCompany = null;
                }
            }

        }
        
        public ActionResult ConvertirCotizacionAPedido(string jsonPedido)
        {
            string userIDweb = User.Identity.GetUserId().ToString();
           
            int respuestaPedido = 20202;
            string error = "";
            string docEntryPedido = "nointegrado";
            double totalDescuento = 0;
            DateTime docDate = DateTime.Now;
            RespuestaConexionSAP respSAP = null;
            try
            {
                dynamic order = JsonConvert.DeserializeObject(jsonPedido);
                JArray lineas = (JArray)order.DocumentLines;
                //DateTime docDueDate = Convert.ToDateTime(order.DocDueDate);
                string[] formatos = {
                    "dd-MM-yyyy", "dd/MM/yyyy",
                    "yyyy-MM-dd", "yyyy/MM/dd",
                    "yyyy-MM-ddTHH:mm:ss", "dd-MM-yyyyTHH:mm:ss",
                    "dd-MM-yyyy HH:mm:ss", "yyyy-MM-dd HH:mm:ss"
                };

                DateTime docDueDate;
                if (!DateTime.TryParseExact(
                        (string)order.DocDueDate,
                        formatos,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out docDueDate))
                {
                    throw new FormatException("Formato inválido en DocDueDate: " + order.DocDueDate);
                }
                //userIDweb = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();

                if (string.IsNullOrEmpty(userIDweb))
                {
                    return Json(new { codigo = -33, mensaje = "Se agotó el tiempo de Sesión" }, JsonRequestBehavior.AllowGet);
                }

                respSAP = ConexionDI();

                if (respSAP.Codigo != 0)// si no hay conexion
                {
                    error = oCompany.GetLastErrorDescription();
                    return Json(new { codigo = respSAP.Codigo, mensaje = respSAP.Error }, JsonRequestBehavior.AllowGet);
                }
                else//crear el pedido
                {
                    //if (oCompany.InTransaction)
                    //    oCompany.EndTransaction(BoWfTransOpt.wf_RollBack);
                    //else
                    //    oCompany.StartTransaction();

                        SAPbobsCOM.Documents pedido = null;
                        //validar orden o cotizacion
          
                        pedido = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(BoObjectTypes.oOrders);
              
                         pedido.CardCode = order.CardCode;
                        pedido.DocDate = docDate;
                        pedido.DocDueDate = docDueDate;
                        pedido.Comments = order.Comments;
                        pedido.NumAtCard = Convert.ToString(order.U_NumPed);
                        //pedido.UserFields.Fields.Item("U_NumPedido").Value = Convert.ToString(order.U_NumPed);
                        pedido.DiscountPercent = order.Discount;
                        pedido.UserFields.Fields.Item("U_UserPortal").Value = userIDweb;
                        pedido.ShipToCode = order.Address;
                        pedido.UserFields.Fields.Item("U_Envio").Value = order.U_envio;
                        pedido.UserFields.Fields.Item("U_MetPago").Value = order.MetodoPago;
                        pedido.PaymentMethod = order.FormaPago;
                        pedido.UserFields.Fields.Item("U_UsoCFDI").Value =order.CDFI;
                    

                    for (int i = 0; i < lineas.Count; i++)
                        {
                            pedido.Lines.ItemCode = order.DocumentLines[i].ItemCode;

                            double desc1 = 100 - Convert.ToDouble(order.Discount);
                            double desc2 = 100 - Convert.ToDouble(order.DocumentLines[i].PrtgDiscount);
                            double promDesc = (desc1 * desc2) / 100;
                            totalDescuento = 100 - promDesc;
                            pedido.Lines.Price = order.DocumentLines[i].PrecioUnitario;
                            pedido.Lines.UserFields.Fields.Item("U_Desc1").Value = Convert.ToDouble(order.Discount);
                            pedido.Lines.UserFields.Fields.Item("U_Desc2").Value = Convert.ToDouble(order.DocumentLines[i].PrtgDiscount);
                            pedido.Lines.DiscountPercent = totalDescuento;//order.Discount;
                            pedido.Lines.Quantity = order.DocumentLines[i].Quantity;
                            pedido.Lines.VatGroup = order.DocumentLines[i].VatGroup;
                            pedido.Lines.WarehouseCode = order.DocumentLines[i].WarehouseCode;
                            pedido.Lines.BaseEntry = Convert.ToInt32(order.DocNumber);
                            pedido.Lines.BaseLine = Convert.ToInt32(order.DocumentLines[i].LineNum);
                            pedido.Lines.BaseType = 23;
                            //pedido.Lines.Currency = order.DocumentLines[i].Currency == "$" ? "MXP" : order.DocumentLines[i].Currency;
                            pedido.Lines.Add();
                        }
                        if (pedido.Add() != 0)
                        {
                            docEntryPedido = oCompany.GetLastErrorDescription();
                            ExceptionUtility.LogMessageDIAPI(userIDweb, (oCompany.GetLastErrorCode()).ToString(), oCompany.GetLastErrorDescription(), Convert.ToString(order.U_NumPed));
                            oCompany.EndTransaction(BoWfTransOpt.wf_RollBack);
                            //oCompany.Disconnect();
                        }
                        else
                        {
                            respuestaPedido = 0;
                            docEntryPedido = oCompany.GetNewObjectKey();
                            docEntryPedido = Convert.ToString(ReturnDocNum(docEntryPedido, "Pedido"));
                            if (!CerrarCotizacionConvertidaAPedido(Convert.ToInt32(order.DocNumber)))
                            {
                                docEntryPedido = docEntryPedido + $"| Error Cotizacion Cerrado:{oCompany.GetLastErrorDescription()}. Favor de Cerrar la cotizacion {order.DocNumber} manualmente.";
                                ExceptionUtility.LogMessageDIAPI(userIDweb, (oCompany.GetLastErrorCode()).ToString(), oCompany.GetLastErrorDescription(), Convert.ToString(order.U_NumPed));
                                //oCompany.Disconnect();
                                oCompany.EndTransaction(BoWfTransOpt.wf_RollBack);
                            }
                        else
                        {
                            //oCompany.EndTransaction(BoWfTransOpt.wf_Commit);
                            //docEntryPedido += $"; Cotizacion Cerrada con exito: {order.DocNumber}";
                        }
                    }
                    //oCompany.Disconnect();
                    return Json(new { codigo = respuestaPedido, mensaje = docEntryPedido + $"; Cotizacion Cerrada con exito: {order.DocNumber}", docEntry = docEntryPedido }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                if (oCompany != null)
                {
                    if (oCompany.Connected)
                        oCompany.Disconnect();
                    Marshal.ReleaseComObject(oCompany);
                    oCompany = null;
                }
                //oCompany.Disconnect();
                ExceptionUtility.LogException(ex, "IntegracionSAP.IntegrarOrden");
                return Json(new { codigo = oCompany.GetLastErrorCode(), mensaje = oCompany.GetLastErrorDescription() });
            }
            finally
            {
                if (oCompany != null)
                {
                    if (oCompany.Connected)
                    {
                        oCompany.Disconnect();
                    }

                    Marshal.ReleaseComObject(oCompany);
                    oCompany = null;
                }
            }

        }
        
        public bool CerrarCotizacionConvertidaAPedido(int DocEntry)
        {
            //string userIDweb = User.Identity.GetUserId().ToString();
            var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string docEntryPedido = "nointegrado";
            try
            {
                //dynamic order = JsonConvert.DeserializeObject(jsonPedido);
                string userIDweb = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
                    SAPbobsCOM.Documents pedido = null;
                    //validar orden o cotizacion
                    pedido = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(BoObjectTypes.oQuotations);
                    pedido.GetByKey(DocEntry);
                    
                    if (pedido.Update() == 0)
                    {
                        if (pedido.Close() == 0)
                        {
                            docEntryPedido = oCompany.GetNewObjectKey();
                            docEntryPedido = Convert.ToString(docEntryPedido);
                        }
                        else
                        {
                            docEntryPedido = oCompany.GetLastErrorDescription();
                            ExceptionUtility.LogMessageDIAPI(userIDweb, (oCompany.GetLastErrorCode()).ToString(), oCompany.GetLastErrorDescription(), Convert.ToString(DocEntry));
                            //oCompany.Disconnect();                        
                        }
                    }
                    else
                    {
                        docEntryPedido = oCompany.GetLastErrorDescription();
                        ExceptionUtility.LogMessageDIAPI(userIDweb, (oCompany.GetLastErrorCode()).ToString(), oCompany.GetLastErrorDescription(), Convert.ToString(DocEntry));
                        //oCompany.Disconnect();                        
                    }
                    //oCompany.Disconnect();
                    return true;
            }
            catch (Exception ex)
            {
                if (oCompany != null)
                {
                    if (oCompany.Connected)
                        oCompany.Disconnect();
                    Marshal.ReleaseComObject(oCompany);
                    oCompany = null;
                }
                //oCompany.Disconnect();
                ExceptionUtility.LogException(ex, "IntegracionSAP.IntegrarOrden");
                return false;
            }
        }
        
        public int ReturnDocNum(string DocEntry,string TipoDocumento)
        {
            int DocNum = 0;
            SAPbobsCOM.Recordset oDocNum = null;
            try
            {
                if (TipoDocumento == "Pedido")
                {
                    oDocNum = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                    oDocNum.DoQuery("SELECT DocNum FROM ORDR WHERE DocEntry =" + int.Parse(DocEntry));
                }
                else {
                    oDocNum = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                    oDocNum.DoQuery("SELECT DocNum FROM OQUT WHERE DocEntry =" + int.Parse(DocEntry));
                }

                while (!oDocNum.EoF)
                {
                    DocNum = int.Parse(oDocNum.Fields.Item("DocNum").Value.ToString());
                    oDocNum.MoveNext();
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "IntegracionSAP.ReturnDocNum");
            }
            
            return DocNum;
        }
        
        public FileResult GenerarImpresion(string fi, string ff, string e)
        {
            
            FileStream fss = null;
            //string WebUserId = User.Identity.GetUserId();
            try
            {
                var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                Impresion i = new Impresion();
                string WebUserId = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
                var stream = i.GenerarImpresion(fi, ff, e, WebUserId);
                int buffLength = 2048;
                byte[] buff = new byte[buffLength];
                int contentLen;
                
                var LocalDirectory = Path.Combine(Path.GetTempPath(), "Etiqueta.pdf");
                
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
                ExceptionUtility.LogException(ex, "IntegracionSAP.GenerarImpresion");
            }

            return File(fss, "application/pdf");
        }

        public FileResult ImprimirPedido(string SO, string T, string modo = "inline")
        {
            try
            {
                ExceptionUtility.FullInfo("", "Imprimir Pedido - Token: " + SO, "Demo", 0, "IntegracionSap", "ImprimirPedido");

                var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                string WebUserId = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();

                string desencriptado = CryptoHelper.DesencriptarConIV(SO);
                int EntrySAP = int.Parse(desencriptado);

                Impresion i = new Impresion();
                var stream = i.GenerarImpresion2(EntrySAP, T, WebUserId);

                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", $"{modo}; filename=Pedido_" + EntrySAP + ".pdf");

                return new FileStreamResult(memoryStream, "application/pdf");
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "IntegracionSAP.ImprimirPedido");

                byte[] errorBytes = System.Text.Encoding.ASCII.GetBytes("Error al generar el PDF.");
                return File(new MemoryStream(errorBytes), "application/pdf", "Error.pdf");
            }
        }

        public FileResult ImprimirCotizacion(string docEntrySAP, string tipo, string modo = "inline")
        {
            try
            {
                ExceptionUtility.FullInfo("", "Imprimir Cotizacion - Token: " + docEntrySAP, "Demo", 0, "IntegracionSap", "ImprimirCotizacion");

                var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                string WebUserId = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();

                string desencriptado = CryptoHelper.DesencriptarConIV(docEntrySAP);
                int EntrySAP = int.Parse(desencriptado);

                Impresion i = new Impresion();
                var stream = i.GenerarImpresion3(EntrySAP, tipo, WebUserId);

                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", $"{modo}; filename=Cotizacion_" + EntrySAP + ".pdf");

                return new FileStreamResult(memoryStream, "application/pdf");
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "IntegracionSAP.ImprimirCotizacion"); 

                byte[] errorBytes = System.Text.Encoding.ASCII.GetBytes("Error al generar el PDF de cotización.");
                return File(new MemoryStream(errorBytes), "application/pdf", "Error.pdf");
            }
        }

        public FileResult ImprimirEstadoPedido(int DocNum)
        {
            FileStream fss = null;

            try
            {
                Impresion i = new Impresion();
                var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                string Vendedor = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
                var stream = i.GenerarImpresionEstadoPedido(DocNum, Vendedor);
                int buffLength = 2048;
                byte[] buff = new byte[buffLength];
                int contentLen;

                var LocalDirectory = Path.Combine(Path.GetTempPath(), "EstadoPedido.pdf");
                                
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
                ExceptionUtility.LogException(ex, "IntegracionSAP.ImprimirEstadoPedido");
            }

            return File(fss, "application/pdf");
        }
        
        public FileResult ImprimirEstadoCotizacion(int DocNum)
        {
            FileStream fss = null;

            try
            {
                Impresion i = new Impresion();
                var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                string Vendedor = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
                var stream = i.GenerarImpresionEstadoCotizacion(DocNum, Vendedor);
                int buffLength = 2048;
                byte[] buff = new byte[buffLength];
                int contentLen;

                var LocalDirectory = Path.Combine(Path.GetTempPath(), "EstadoCotizacion.pdf");

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
                ExceptionUtility.LogException(ex, "IntegracionSAP.ImprimirEstadoCotizacion");
            }

            return File(fss, "application/pdf");
        }

        public ActionResult EstadoSesion()
        {
            bool sesion = true;
            try
            {
                var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                string SlpCode = Identity.Claims.Where(c => c.Type == ClaimTypes.Actor).Select(c => c.Value).SingleOrDefault();
                

                if (string.IsNullOrEmpty(SlpCode))
                {
                    sesion = false;
                    
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "IntegracionSAP.EstadoSesion");
            }
            return Json(sesion, JsonRequestBehavior.AllowGet);

        }
        
        public FileResult RegresarStringBase64(string Obj)
        {
            dynamic streamPdf = Obj;
            //var bytes = Convert.FromBase64String(streamPdf.Replace("data:application/pdf;base64,", ""));
            var bytes = Convert.FromBase64String("");
            var stream = new MemoryStream(bytes);
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;

            var LocalDirectory = Path.Combine(Path.GetTempPath(), "Etiqueta.pdf");
            
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
            FileStream fss = new FileStream(LocalDirectory, FileMode.Open, FileAccess.Read);
            return File(fss, "application/pdf");
        }
        
        public FileResult ImprimirEstadoFactura(int DocNum,string RutaQr)
        {
            FileStream fss = null;

            try
            {
                Impresion i = new Impresion();
                var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                string Vendedor = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
                var stream = i.GenerarImpresionEstadoFactura(DocNum, RutaQr, Vendedor);
                int buffLength = 2048;
                byte[] buff = new byte[buffLength];
                int contentLen;

                var LocalDirectory = Path.Combine(Path.GetTempPath(), "EstadoFactura.pdf");

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
                ExceptionUtility.LogException(ex, "IntegracionSAP.ImprimirEstadoFactura");
            }

            return File(fss, "application/pdf");
        }

        //****************************************************************************
        //****************************************************************************

        public FileResult ReimprimePedido(int docEntrySAP, string tipo, string modo = "inline")
        {
            try
            {
                ExceptionUtility.FullInfo("", "Re-Imprimir Pedido: " + docEntrySAP, "Demo", 0, "IntegracionSap", "ReimprimePedido");

                var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                string WebUserId = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();

                Impresion i = new Impresion();
                var stream = i.GenerarImpresion2(docEntrySAP, tipo, WebUserId);

                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", $"{modo}; filename=Pedido_" + docEntrySAP + ".pdf");

                return new FileStreamResult(memoryStream, "application/pdf");
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "IntegracionSAP.ReimprimePedido");

                byte[] errorBytes = System.Text.Encoding.ASCII.GetBytes("Error al generar el PDF.");
                return File(new MemoryStream(errorBytes), "application/pdf", "Error.pdf");
            }
        }

        public FileResult ReimprimeCotizacion(int docEntrySAP, string tipo, string modo = "inline")
        {
            try
            {
                ExceptionUtility.FullInfo("", "Re-Imprimir Cotizacion: " + docEntrySAP, "Demo", 0, "IntegracionSap", "ReimprimeCotizacion");

                var Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                string WebUserId = Identity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();

                Impresion i = new Impresion();
                var stream = i.GenerarImpresion3(docEntrySAP, tipo, WebUserId);

                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", $"{modo}; filename=Cotizacion_" + docEntrySAP + ".pdf");

                return new FileStreamResult(memoryStream, "application/pdf");

            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "IntegracionSAP.ReimprimeCotizacion");

                byte[] errorBytes = System.Text.Encoding.ASCII.GetBytes("Error al generar el reimpresion PDF de cotizacion.");
                return File(new MemoryStream(errorBytes), "application/pdf", "Error.pdf");
            }
        }
    }
}