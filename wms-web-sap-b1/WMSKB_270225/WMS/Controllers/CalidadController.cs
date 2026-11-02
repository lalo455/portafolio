using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using WMS.Logic.Comun;
using WMS.Logic.Stores;
using WMS.Models;
using static WMS.Models.Produccion;

namespace WMS.Controllers
{
    public class CalidadController : Controller
    {
        // GET: Calidad
        Obtener Ob;
        Integrar IntSAP;
        StoreProcedure Sp;
        Integrar sap;
        Utilities.Log Logs = new Utilities.Log();

        public CalidadController() {
            Ob = new Obtener();
            IntSAP = new Integrar();
            Sp = new StoreProcedure();
            sap = new Integrar();
        }

        public ActionResult Calidad()
        {
            try
            {
                var Verificar = ((ClaimsIdentity)User.Identity).Claims.First().Value;
                if (String.IsNullOrEmpty(Verificar))
                {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Login", "Login");
            }
        }        
        
        //Calidad: Calidad
        [HttpPost]
        public ActionResult ObtenerAlmacenes(string TipoWhs, string WhsCode)
        {
            List<TrsManual.Almacen> ListAlm = new List<TrsManual.Almacen>();

            if (Ob.ObtenerAlm(ref ListAlm, WhsCode, TipoWhs))
            {
                if (ListAlm.Count > 0)
                {
                    return Json(new { codigo = 0, mensaje = ListAlm }, JsonRequestBehavior.DenyGet);
                }
                else
                {
                    return Json(new { codigo = -1, mensaje = "" }, JsonRequestBehavior.DenyGet);
                }
            }
            else
            {
                return Json(new { codigo = -2, mensaje = "" }, JsonRequestBehavior.DenyGet);
            }
        }

        public ActionResult GrabarLectura(string Etiqueta, int Nlectura)
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();

            try
            {
                if (Nlectura == 1)
                {
                    Logs.FullInfo(" <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" +
                        "\r\n <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>"
                        , "", Usuario, 1, "", ""); //Log Informativo
                }

                Logs.FullInfo("", " L E C T U R A  O R I G I N A L: " + Etiqueta, Usuario, 0, modulo, metodo); //Log Informativo

                return Json(new { codigo = 0, mensaje = "Etiqueta grabada" }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo

                return Json(new { codigo = -1, mensaje = ex.Message }, JsonRequestBehavior.DenyGet);
            }
        }

        public ActionResult Actualizar(string Tipo, List<DetalleR> ListaBloq, string Estatus, string Destino, string Origen, List<DetalleR> RegMaster) 
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();

            string msj = string.Empty;
            Guid g;
            g = Guid.NewGuid();
            Company sbo = null;
            string msjUpdate = string.Empty, msjSbo = string.Empty;
            ClaimsPrincipal iden = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string usr = iden.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

            Logs.FullInfo("Modulo: Calidad - Inicio", "", Usuario, 1, "", ""); //Log Informativo
            Logs.FullInfo("", "Etiquetas a Procesar con el Status [X: Bloquear | O: Liberar] : " + Estatus, Usuario, 0, modulo, metodo); //Log Informativo

            //**************************************************************************
            //**************************************************************************
            List<DetalleR> regParciales = new List<DetalleR>();
            //Validar si hay registros master a procesar
            if (RegMaster != null && RegMaster.Count > 0)
            {
                Logs.FullInfo("", "Proceso de registros Individuales y de Master", Usuario, 0, modulo, metodo); //Log Informativo
                //Obtener solo registros leidos tipo I: Individual
                foreach (var reg in ListaBloq)
                {
                    if (reg.TipoLectura == "I")
                    {
                        regParciales.Add(new DetalleR()
                        {
                            DistNumber = reg.DistNumber,
                            BinCode = reg.BinCode,
                            AbsEntry = reg.AbsEntry,
                            ItemCode = reg.ItemCode,
                            ItemName = reg.ItemName,
                            OnHandQty = reg.OnHandQty,
                            WhsCode = reg.WhsCode,
                            NewBinCode = reg.NewBinCode,
                            NewAbsEntry = reg.NewAbsEntry,
                            Dispositivo = reg.Dispositivo,
                            NumDispositivos = reg.NumDispositivos,
                            PesoBruto = reg.PesoBruto,
                            PesoNeto = reg.PesoNeto
                        });
                    }
                }

                //Borrar los registros recibos
                ListaBloq.Clear();

                //Agregar registros Individuales y registros de Master authors.AddRange(duplicateAuthors);
                ListaBloq.AddRange(regParciales);
                ListaBloq.AddRange(RegMaster);
            }
            //**************************************************************************
            //**************************************************************************

            foreach (DetalleR etq in ListaBloq)
            {
                Logs.FullInfo("", "Etiqueta: " + etq.DistNumber, Usuario, 0, modulo, metodo); //Log Informativo
            }
            
            try
            {
                sbo = sap.sboConect(ref msjSbo);
                if (sbo != null && msjSbo.Length == 0)
                {
                    //Validar el tipo 
                    //******************************************
                    // Tipo1: Actualizacion y Traspaso
                    // Tipo2: Solo Actualizacion
                    // Tipo3: Solo Traspaso en el BLOQUEO
                    //******************************************
                    switch (Tipo)
                    {
                        case "Tipo1":
                            //Actualizacion y Traspaso
                            if (Sp.GuardarRegistroCalidad(ListaBloq, g.ToString(), Estatus, Destino))
                            {
                                sbo.StartTransaction();
                                Mensajes response = new Mensajes();
                                if (Ob.ActualizarSAP(ref msjUpdate, ListaBloq, Estatus, sbo))
                                {
                                    Logs.FullInfo("", "Actualizacion aplicada correctamente: " + response.DocNum, Usuario, 0, modulo, metodo); //Log Informativo
                                    if (IntSAP.TraspasoCalidad(sbo, ref response, g.ToString(), ListaBloq, Estatus, Destino, Origen))
                                    {
                                        sbo.EndTransaction(BoWfTransOpt.wf_Commit);
                                        //Sp.RemoverRegistros("Movimiento", g.ToString());
                                        Logs.FullInfo("", "Traspaso Realizado: DocNum => " + response.DocNum + "- DocEntry => " + response.DocEntry, Usuario, 0, modulo, metodo); //Log Informativo
                                        return Json(new { codigo = 0, mensaje = "Actualizacion aplicada correctamente: " + response.DocNum }, JsonRequestBehavior.DenyGet);
                                    }
                                    else
                                    {
                                        Logs.FullInfo("", "Error: No se Realizó Traspaso: " + response.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo

                                        if (sbo.InTransaction) 
                                        {
                                            sbo.EndTransaction(BoWfTransOpt.wf_RollBack);
                                        }
                                        return Json(new { codigo = -1, mensaje = response.Mensaje }, JsonRequestBehavior.DenyGet);
                                    }
                                }
                                else
                                {
                                    //sbo.EndTransaction(BoWfTransOpt.wf_RollBack);
                                    Logs.FullInfo("", "Error al hacer la actualización: " + msjUpdate, Usuario, 0, modulo, metodo); //Log Informativo

                                    if (sbo.InTransaction)
                                    {
                                        sbo.EndTransaction(BoWfTransOpt.wf_RollBack);
                                    }
                                    return Json(new { codigo = -1, mensaje = msjUpdate }, JsonRequestBehavior.DenyGet);
                                }
                            }
                            else
                            {
                                Logs.FullInfo("", "Error al guardar los registros para calidad", Usuario, 0, modulo, metodo); //Log Informativo
                                return Json(new { codigo = -1, mensaje = "Error al guardar los registros para calidad" }, JsonRequestBehavior.DenyGet);
                            }
                            //break;

                        case "Tipo2":
                            //SOLO Actualizacion
                            if (Sp.GuardarRegistroCalidad(ListaBloq, g.ToString(), Estatus, Destino))
                            {
                                string msjUpd = string.Empty;
                                Mensajes response = new Mensajes();
                                sbo.StartTransaction();
                                if (Ob.ActualizarSAP(ref msjUpd, ListaBloq, Estatus, sbo))
                                {
                                    sbo.EndTransaction(BoWfTransOpt.wf_Commit);
                                    //Sp.RemoverRegistros("Movimiento", g.ToString());

                                    Logs.FullInfo("", "Actualizacion aplicada correctamente", Usuario, 0, modulo, metodo); //Log Informativo
                                    return Json(new { codigo = 0, mensaje = "Actualizacion aplicada correctamente" }, JsonRequestBehavior.DenyGet);
                                }
                                else
                                {
                                    Logs.FullInfo("", "Error al hacer la actualización: " + msjUpd, Usuario, 0, modulo, metodo); //Log Informativo

                                    if (sbo.InTransaction)
                                    {
                                        sbo.EndTransaction(BoWfTransOpt.wf_RollBack);
                                    }
                                    return Json(new { codigo = -1, mensaje = "Error al hacer la actualización" }, JsonRequestBehavior.DenyGet);
                                }
                            }
                            else
                            {
                                Logs.FullInfo("", "Error al guardar los registros para calidad", Usuario, 0, modulo, metodo); //Log Informativo
                                return Json(new { codigo = -1, mensaje = "Error al guardar los registros para calidad" }, JsonRequestBehavior.DenyGet);
                            }
                            //break;

                        case "Tipo3":
                            //SOLO Traspaso en el Bloqueo
                            if (Sp.GuardarRegistroCalidad(ListaBloq, g.ToString(), Estatus, Destino))
                            {
                                string msjUpd = string.Empty;
                                Mensajes response = new Mensajes();
                                sbo.StartTransaction();
                                if (IntSAP.TraspasoCalidad(sbo, ref response, g.ToString(), ListaBloq, Estatus, Destino, Origen))
                                {
                                    sbo.EndTransaction(BoWfTransOpt.wf_Commit);
                                    //Sp.RemoverRegistros("Movimiento", g.ToString());

                                    Logs.FullInfo("", "Traspaso Realizado: DocNum => " + response.DocNum + "- DocEntry => " + response.DocEntry, Usuario, 0, modulo, metodo); //Log Informativo
                                    return Json(new { codigo = 0, mensaje = "Traspaso Realizado: DocNum => " + response.DocNum }, JsonRequestBehavior.DenyGet);
                                }
                                else
                                {
                                    //sbo.EndTransaction(BoWfTransOpt.wf_RollBack);
                                    Logs.FullInfo("", "Error: No se Realizó Traspaso: " + response.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo

                                    if (sbo.InTransaction)
                                    {
                                        sbo.EndTransaction(BoWfTransOpt.wf_RollBack);
                                    }
                                    return Json(new { codigo = -1, mensaje = response.Mensaje }, JsonRequestBehavior.DenyGet);
                                }
                            }
                            else
                            {
                                Logs.FullInfo("", "Error al guardar los registros para calidad", Usuario, 0, modulo, metodo); //Log Informativo
                                return Json(new { codigo = -1, mensaje = "Error al guardar los registros para calidad" }, JsonRequestBehavior.DenyGet);
                            }
                        //break;

                        default:
                            Logs.FullInfo("", "Error: Estatus Desconocido", Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = -1, mensaje = "Estatus desconocido" }, JsonRequestBehavior.DenyGet);
                    }
                }
                else
                {
                    Logs.FullInfo("", "Error en Conexion a SAP: -- " + msjSbo, Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = -1, mensaje = msjSbo }, JsonRequestBehavior.DenyGet);
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar("Error en Actualizacion de Etiquetas: ", ex.Message, modulo, metodo);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = ex.Message }, JsonRequestBehavior.DenyGet);
            }
            finally 
            {
                if (sbo != null)
                {
                    if (sbo.Connected)
                    {
                        sbo.Disconnect();
                        Marshal.ReleaseComObject(sbo);
                        sbo = null;
                    }

                    Logs.FullInfo("", "Conexion a SAP Cerrada", Usuario, 0, modulo, metodo); //Log Informativo
                }

                Logs.FullInfo("Modulo: Calidad - Fin", "", Usuario, 2, "", ""); //Log Informativo
            }
        }
                
    }
}