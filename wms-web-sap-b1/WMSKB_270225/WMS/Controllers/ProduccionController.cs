using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using WMS.Models;
using WMS.Logic.Comun;
using WMS.Logic.Stores;
using System.Threading;
using System.Data;
using SAPbobsCOM;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using System.Configuration;
using static WMS.Models.Produccion;

namespace WMS.Controllers
{
    public class ProduccionController : Controller
    {
        // GET: Produccion
        StoreProcedure SP;
        Obtener Ob;
        Integrar Crear;
        Utilities.Log Logs;

        public ProduccionController() { 
            SP = new StoreProcedure();
            Ob = new Obtener();
            Crear = new Integrar();
            Logs = new Utilities.Log();
        }

        /*
         * Sección de vistas
        */
        public ActionResult Produccion()
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
        public ActionResult ReciboProduccion() {
            try
            {
                if (String.IsNullOrEmpty(((ClaimsIdentity)User.Identity).Claims.First().Value)) {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Login", "Login");
                }
                else
                    return View();
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Login", "Login");
            }
        }
        public ActionResult EmisionReciboProduccion()
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
        
        //********************************************************
        //********************************************************
        
        [HttpGet]
        public ActionResult ObtenerDocumentos(string Accion, int Documento) 
        {
            List<Produccion.OrdenFabricacion> ListaOF = new List<Produccion.OrdenFabricacion>();
            List<Produccion.DetalleOrden> DetalleOF = new List<Produccion.DetalleOrden>();
            List<Produccion.DetalleSolicitud> DetalleST = new List<Produccion.DetalleSolicitud>();

            switch (Accion)
            {
                //Listar OF | desactivada
                case "OF": 
                    if (SP.ObtenerLists(ref ListaOF, Accion, -1))
                    {
                        if (ListaOF.Count > 0)
                        {
                            return Json(new { codigo = 0, mensaje = ListaOF }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { codigo = -1 }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { codigo = -2 }, JsonRequestBehavior.AllowGet);
                    }                        
                    //break;

                //Detalle de OF
                case "DO": 
                    if (SP.ObtenerLists(ref DetalleOF, Accion, Documento))
                    {
                        if (DetalleOF.Count > 0)
                        {
                            return Json(new { codigo = 0, mensaje = DetalleOF }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { codigo = -1 }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { codigo = -1 }, JsonRequestBehavior.AllowGet);
                    }                        
                    //break;

                //Detalle de ST
                case "SO":
                    if (SP.ObtenerLists(ref DetalleST, Accion, Documento))
                    {
                        if (DetalleST.Count > 0)
                        {
                            return Json(new { codigo = 0, mensaje = DetalleST }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { codigo = -1 }, JsonRequestBehavior.AllowGet);
                        }                            
                    }
                    else
                    {
                        return Json(new { codigo = -2 }, JsonRequestBehavior.AllowGet);
                    }                        
                    //break;
            }

            return Json(new { codigo = -2, mensaje = "Error durante el proceso"}, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public ActionResult EnvioProduccion(Produccion.Documento Lineas, Produccion.Datos IAdicional)
        {
            Mensajes msj = new Mensajes();
            if (Crear.EnvioProduccion(ref msj, Lineas, IAdicional))
                return Json(new { codigo = 0, mensaje = msj.DocNum }, JsonRequestBehavior.DenyGet);
            else
                return Json(new { codigo = -1, mensaje = msj.Mensaje}, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// Metodo opcional para generar traspaso a produdcion con varios lotes con el mismo artículo
        /// Modulo Envio a Produccion
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult TraspasoAProduccion(Produccion.Documento Lineas, Produccion.Datos IAdicional) 
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();

            Mensajes msj = new Mensajes();
            Guid g;
            g = Guid.NewGuid();

            Logs.FullInfo("Modulo: Envio a Produccion - Inicio", "", Usuario, 1, "", ""); //Log Informativo
            Logs.FullInfo("", "Etiquetas a Procesar", Usuario, 0, modulo, metodo); //Log Informativo
            
            try
            {
                if (SP.GuardarRegistrosProduccion(Lineas, IAdicional, g.ToString()))
                {
                    if (Crear.TraspasoAProduccion(ref msj, IAdicional, Lineas.NumST, Lineas.NumOrdr, g.ToString()))
                    {
                        Logs.FullInfo("", "Traspaso Realizado: DocNum => " + msj.DocNum + "--- DocEntry => " + msj.DocEntry, Usuario, 0, modulo, metodo); //Log Informativo
                        return Json(new { codigo = 0, mensaje = msj.DocNum }, JsonRequestBehavior.DenyGet);
                    }
                    else
                    {
                        Logs.FullInfo("", "No se realizó Traspaso: " + msj.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo
                        return Json(new { codigo = -1, mensaje = msj.Mensaje }, JsonRequestBehavior.DenyGet);
                        //return Json(new { codigo = 0, mensaje = "Pruebas"});
                    }
                }
                else
                {
                    Logs.FullInfo("", "Error al guardar los registros", Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = -1, mensaje = "Error al guardar los registros" });
                }                    
            }
            catch (Exception ex)
            {
                Logs.Registrar("Envio a Produccion NO Realizado", ex.Message, modulo, metodo);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = "Error al guardar los registros" });
            }
            finally
            {
                Logs.FullInfo("Modulo: Envio a Produccion - Fin", "", Usuario, 2, "", ""); //Log Informativo
            }
        }
        
        [HttpPost]
        public ActionResult ValidarProd(string QR) {
            List<Produccion.ValidacionProd> lineas = new List<Produccion.ValidacionProd>();
            if (Ob.ObtenerValidacionProduccion(ref lineas, QR.Split('&')[0], QR.Split('&')[1]))
            {
                if (lineas.Count > 0)
                    return Json(new { codigo = 0, mensaje = lineas }, JsonRequestBehavior.DenyGet);
                else
                    return Json(new { codigo = -1, mensaje = "No se encontro informacion del lote: " + QR.Split('&')[0] }, JsonRequestBehavior.DenyGet);
            }
            else
                return Json(new { codigo = -1, mensaje = "Error al recuperar información del lote: " + QR.Split('&')[0] }, JsonRequestBehavior.DenyGet);
        }
        
        //Validación de Producción
        [HttpPost]
        public ActionResult ActualizarOrden(List<Produccion.ValidacionProd> ListaAct) 
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();

            Logs.FullInfo("Modulo: Validación de Produccion - Inicio", "", Usuario, 1, "", ""); //Log Informativo
            Logs.FullInfo("", "Etiquetas a Procesar", Usuario, 0, modulo, metodo); //Log Informativo

            string msj = string.Empty;
            ClaimsPrincipal iden = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string usr = iden.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault() ;

            try
            {
                if (Ob.ActualizarDtsOrden(ref msj, ListaAct, usr))
                {
                    Logs.FullInfo("", "Validacion de Producción Realizada Correctamente", Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = 0, mensaje = "Actualizacion realizada correctamente" }, JsonRequestBehavior.DenyGet);
                }
                else
                {
                    Logs.FullInfo("", "NO se realizo validacion de Producción, " + msj, Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = -1, mensaje = msj }, JsonRequestBehavior.DenyGet);
                }                    
            }
            catch (Exception ex)
            {
                Logs.Registrar("Validación de Produccion NO Realizada", ex.Message, modulo, metodo);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo

                return Json(new { codigo = -1, mensaje = msj }, JsonRequestBehavior.DenyGet);
            }
            finally
            {
                Logs.FullInfo("Modulo: Validación de Produccion - Fin", "", Usuario, 2, "", ""); //Log Informativo
            }            
        }
                
        [HttpPost]
        public ActionResult ObtenerBCode(string BCode, string Warehouse) 
        {
            List<Comun.Location> Ubicaciones = new List<Comun.Location>();
            if (Ob.UbicacionesLista(Warehouse, BCode, ref Ubicaciones))
            {
                if (Ubicaciones.Count > 0)
                    return Json(new { codigo = 0, mensaje = Ubicaciones }, JsonRequestBehavior.DenyGet);
                else 
                    return Json(new { codigo = -2, mensaje = "La ubicación: " + BCode + " del almacén: " + Warehouse + " NO EXISTE"}, JsonRequestBehavior.DenyGet);
            }
            else
                return Json(new { codigo = -1, mensaje = "Error al recuperar información de la ubicación: " + BCode }, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public ActionResult EstatusEtiqueta(string Lote) //Verificar el estatus de la etiqueta, YA NO SE PERMITEN ETIQUETAS T - IUO 6/11/2020
        {
            string valor = string.Empty;
            List<InicialEtiqueta> Lista = new List<InicialEtiqueta>();
            if (!Lote.StartsWith(ConfigurationManager.AppSettings["TRL"])) //Si la etiqueta es de Traspaleo T NO SE PROCESA - IUO 6/11/2020
            {
                //Si la etiqueta es diferente de T00000 se puede leer y procesar
                if (Ob.EstadoEtiqueta(ref Lista, Lote.Split('&')[0]))
                {
                    if (Lista.Count > 1) //Existe mas de un resultado
                    {
                        return Json(new { codigo = -1, mensaje = "Existe más de un Registro de Etiqueta." }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        foreach (InicialEtiqueta etq in Lista)
                        {
                            if (Lote.StartsWith(ConfigurationManager.AppSettings["TRLU"])) //Si la etiqueta es de Traspaleo U se Procesa - Nuevo Cambio Traspaleo etiquetas U - IUO 6/11/2020
                            {
                                switch (etq.Estatus)
                                {
                                    case "L":
                                        return Json(new { codigo = 0, mensaje = "" }, JsonRequestBehavior.DenyGet);
                                    default:
                                        return Json(new { codigo = -2, mensaje = "La etiqueta de traspaleo no tiene el estatus de Liberado" }, JsonRequestBehavior.DenyGet);
                                }
                            }
                            else
                            {
                                switch (etq.Estatus)
                                {
                                    case "R":
                                        return Json(new { codigo = -2, mensaje = "La etiqueta Ya ha sido REGISTRADA" }, JsonRequestBehavior.DenyGet);
                                    case "G":
                                        return Json(new { codigo = 0, mensaje = Lista }, JsonRequestBehavior.DenyGet);
                                    case "L":
                                        return Json(new { codigo = -2, mensaje = "La etiqueta YA FUE LIBERADA" }, JsonRequestBehavior.DenyGet);
                                    case "E":
                                        return Json(new { codigo = -2, mensaje = "Las etiquetas embarcadas NO estan PERMITIDAS" }, JsonRequestBehavior.DenyGet);
                                    default:
                                        return Json(new { codigo = -1, mensaje = "El estatus de la etiqueta no pudo ser verificada" }, JsonRequestBehavior.DenyGet);
                                }
                            }
                        }
                        return Json(new { codigo = 0, mensaje = Lista }, JsonRequestBehavior.DenyGet);
                    }
                }
                else
                {
                    return Json(new { codigo = -1, mensaje = "Error al obtener estado de la etiqueta" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { codigo = -2, mensaje = "La etiqueta NO es permitida" }, JsonRequestBehavior.DenyGet);
            }                
        }

        /*Actualizacion 12-01-2021*/
        [HttpPost]
        public ActionResult TurnoFechaEtiqueta(string Lote, int Accion) //Obtener Turno y Fecha del Lote Leido
        {
            switch (Accion)
            {
                case 1: //Turno
                    string TurnoR = string.Empty;
                    if (Ob.TurnoEtq(Lote.Split('&')[0], ref TurnoR))
                    {
                        return Json(new { codigo = 0, mensaje = TurnoR }, JsonRequestBehavior.DenyGet);
                    }
                    else
                        return Json(new { codigo = -1, mensaje = "Error al recuperar Turno de la etiqueta: " + Lote.Split('&')[0] }, JsonRequestBehavior.DenyGet);

                case 2: //Fecha
                    string FechaR = string.Empty;
                    if (Ob.FechaEtq(Lote.Split('&')[0], ref FechaR))
                    {
                        return Json(new { codigo = 0, mensaje = FechaR }, JsonRequestBehavior.DenyGet);
                    }
                    else
                        return Json(new { codigo = -1, mensaje = "Error al recuperar Fecha de la etiqueta: " + Lote.Split('&')[0] }, JsonRequestBehavior.DenyGet);

                default:
                    return Json(new { codigo = -1, mensaje = "El Turno y Fecha de la etiqueta no pudo ser verificada" }, JsonRequestBehavior.DenyGet);
            }            
        }
        /**************************/

        [HttpPost]
        public ActionResult GrabarLectura(string Etiqueta, int Nlectura)
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();

            try
            {
                if (Nlectura == 1)
                {
                    Logs.FullInfo(" <<<<<<<<<<<<<<<<<<<<<<<<<<<<>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>", "", Usuario, 1, "", ""); //Log Informativo
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
            finally
            {
                //Logs.FullInfo(" L E C T U R A S  O R I G I N A L E S", "", Usuario, 4, "", ""); //Log Informativo
                //Logs.FullInfo(" <<<<<<<<<<<<<<<<<<<<<<<<<<<<>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>", "", Usuario, 2, "", ""); //Log Informativo
            }
        }

        [HttpPost]
        public ActionResult CrearEmisionRecibo(List<Produccion.EmisionRecibo> Lineas, string CodigoGuia, List<Produccion.EmisionRecibo> RegMaster)
        {
            #region Variables
            string result = string.Empty, msjSbo = string.Empty, resultT = string.Empty;
            List<MsjProduccion> mensajes = new List<MsjProduccion>();
            string lotesTT = string.Empty;
            Mensajes mensaje = new Mensajes();
            MsjRecibo msjTraspaleo = new MsjRecibo();
            Guid g;
            g = Guid.NewGuid();
            bool integrar = true; //Se puede proceder a integrar | Todos los artículos de la OF tienen Lotes a Consumir
            string msgErrConsumo;
            
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();
            string CodeGuia = CodigoGuia + Usuario;

            var Stock = 0.0;
            var QtyRequerida = 0.0;
            var QtyDiferencia = 0.0;
            var QtyLotes = 0.0;
            var Procede = "Y";

            List<Produccion.EmisionRecibo> regParciales = new List<Produccion.EmisionRecibo>();
            List<EtqMaster.ListaMaster> regMaster = new List<EtqMaster.ListaMaster>();
            #endregion

            Logs.FullInfo("Modulo: Recibo de Produccion - Inicio", "", Usuario, 1, "", "");
            try
            {
                //Procesar Etiquetas - Debe haber etiquetas de PT
                //if (SP.GuardarProduccion(Lineas.Where(x => x.DistNumber.StartsWith("L")).ToList(), g.ToString(), ref result))
                //**************************************************************************
                //**************************************************************************
                //Validar si hay registros master a procesar
                if (RegMaster != null && RegMaster.Count > 0)
                {
                    Logs.FullInfo("", "Proceso de registros Individuales y de Master", Usuario, 0, modulo, metodo); //Log Informativo
                    //Obtener solo registros leidos tipo I: Individual
                    foreach (var reg in Lineas)
                    {
                        if (reg.TipoLectura == "I")
                        {
                            regParciales.Add(new Produccion.EmisionRecibo()
                            {
                                DistNumber = reg.DistNumber,
                                ItemCode = reg.ItemCode,
                                OnHandQty = reg.OnHandQty,
                                OF = reg.OF,
                                BinCode = reg.BinCode,
                                AbsEntry = reg.AbsEntry,
                                Warehouse = reg.Warehouse,
                                U_TurnoRecibo = reg.U_TurnoRecibo,
                                U_FechaRecibo = reg.U_FechaRecibo,
                                EtqMaster = reg.EtqMaster,
                                TipoLectura = reg.TipoLectura,
                                TipoReg = reg.TipoReg
                            });
                        }

                        //Lista de Master a Liberar
                        if (reg.TipoLectura == "M")
                        {
                            regMaster.Add(new EtqMaster.ListaMaster()
                            {
                                Codigo = reg.DistNumber,
                                Articulo = reg.ItemCode,
                                OrdenFab = reg.OF
                            });
                        }
                    }

                    //Borrar los registros recibos
                    Lineas.Clear();

                    //Agregar registros Individuales y registros de Master authors.AddRange(duplicateAuthors);
                    Lineas.AddRange(regParciales);
                    Lineas.AddRange(RegMaster);
                }
                //**************************************************************************
                //**************************************************************************

                if (SP.GuardarProduccion(Lineas, g.ToString(), ref result))
                {
                    //Emision y Recibo de Produccion
                    Logs.FullInfo("", "Se obtienen Ordenes de Fabricación a Procesar, SE PROCESA E INTEGRA POR ORDEN DE FABRICACION", Usuario, 0, modulo, metodo); 

                    //Por cada OF que se obtenga de las etiquetas leidas, se obtiene su Consumo (Detalle de OF)
                    foreach (Produccion.DocumentoAProduccion produccion in Ob.ObtenerDocumentoAProduccion(g.ToString(), "LOF", 0)) 
                    {
                        string CodeGuiaER = CodigoGuia + produccion.DocEntry + produccion.DocNum + produccion.ItemCode + produccion.Warehouse + produccion.AbsEntry;
                        //Antes de Realizar el proceso, se verificará que el Codigo de Guia en Emision y Recibo de Prouccion (OIGE.U_WMS_CodeGuid y OIGN.U_WMS_CodeGuid) de SAP ---- IUrizar 17-06-2021
                        string DocNum = string.Empty;
                        integrar = true;
                        msgErrConsumo = "";

                        if (Ob.GetDocNum(ref DocNum, CodeGuiaER, "Produccion"))
                        {
                            switch (DocNum)
                            {
                                case null: //No existe - Se puede procesar la información
                                case "": //No existe - Se puede procesar la información

                                    Logs.FullInfo(" >>>>>> Procesamiento de la Orden de Fabricacion: " + produccion.DocNum + " | Codigo de Guia: " + CodeGuiaER + " | STATUS [C=Canceled, L=Closed, P=Planned, R=Released] : " + produccion.StatusOF + " >>>>>>", "", Usuario, 3, "", "");

                                    //Verificar STATUS de la OF | Solo se pueden procesar OF Liberadas (R = Released)
                                    if (produccion.StatusOF == "R")
                                    {
                                        msgErrConsumo = "Verificar los siguientes artículos:";
                                        //Se obtiene el Detalle de la OF y por cada linea se Obtiene el Lote a Consumir de los Articulos (ItemType = 4)
                                        List<Produccion.ConsumoOrdenFabricacion> detalleOf = Ob.ObtenerConsumoPorOrden(g.ToString(), produccion.DocNum, produccion.Quantity);
                                        for (int d = 0; d < detalleOf.Count; d++)
                                        {

                                            if (detalleOf[d].ItemType == 4) //Por cada Articulo, se obtienen los lotes a consumir
                                            {
                                                //Validar si el artículo y almacén solo estan en 1 linea
                                                if (ValidaItemWhs(detalleOf, detalleOf[d].ItemCode, detalleOf[d].Warehouse, detalleOf[d].LineNum))
                                                {
                                                    //Console.WriteLine("Item duplicado");
                                                    integrar = false;
                                                    msgErrConsumo = msgErrConsumo + "<br /> " + detalleOf[d].ItemCode + " con Almacén " + detalleOf[d].Warehouse + " , se encuentra repetido en la OF";
                                                    break;
                                                }
                                                else
                                                {
                                                    //Validar si el artículo es administrado por lotes
                                                    if (detalleOf[d].ManBtchNum == "Y")
                                                    {
                                                        //Logs.FullInfo(" >>>>>> Procesamiento del Articulo: " + detalleOf[d].ItemCode + " >>>>>>", "", Usuario, 3, "", "");
                                                        //Logs.FullInfo("", "Articulo: " + detalleOf[d].ItemCode, Usuario, 0, modulo, metodo);
                                                        detalleOf[d].Batch = Ob.ObtenerLotesAConsumir(g.ToString(), detalleOf[d].ItemCode, detalleOf[d].Warehouse, produccion.Quantity, produccion.DocNum);

                                                        //Validar que artículo tenga Lotes a consumir
                                                        if (detalleOf[d].Batch.Count == 0)
                                                        {
                                                            integrar = false;
                                                            msgErrConsumo = msgErrConsumo + "<br /> " + detalleOf[d].ItemCode + ", no cuenta con lotes liberados a consumir en " + detalleOf[d].Warehouse;
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            Procede = detalleOf[d].Batch[0].Procede;
                                                            Stock = detalleOf[d].Batch[0].Stock;
                                                            QtyRequerida = detalleOf[d].Batch[0].QtyRequerida;
                                                            QtyDiferencia = detalleOf[d].Batch[0].QtyDiferencia;
                                                            QtyLotes = detalleOf[d].Batch[0].QtyLotes;

                                                            Logs.FullInfo("", "Consumo: " + detalleOf[d].ItemCode + " - Requerido: " + QtyRequerida + " - Disponible Lotes: " + QtyLotes + " - Faltante: " + QtyDiferencia + " en " + detalleOf[d].Warehouse + " | PROCEDE OF: " + Procede, Usuario, 0, modulo, metodo);
                                                            
                                                            if (Procede == "N")
                                                            {
                                                                integrar = false;
                                                                msgErrConsumo = msgErrConsumo + "<br /> " + detalleOf[d].ItemCode + " - Requerido: " + QtyRequerida + " - Disponible Lotes: " + QtyLotes + " en " + detalleOf[d].Warehouse;
                                                                break;
                                                            }

                                                        }
                                                        //Logs.FullInfo(" <<<<< Procesamiento del Articulo: " + detalleOf[d].ItemCode + " <<<<< ", "", Usuario, 4, "", "");
                                                    }
                                                    else
                                                    {
                                                        integrar = false;
                                                        msgErrConsumo = msgErrConsumo + "<br /> " + detalleOf[d].ItemCode + ", no es administrado por lotes";
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {   //La Maquina o Recurso no se procesa, solo se agrega a la lista del Detalle de OF
                                                //Logs.FullInfo("", "Recurso: " + detalleOf[d].ItemCode, Usuario, 0, modulo, metodo);
                                                detalleOf[d].Batch = new List<Produccion.LoteConsumo>();
                                            }

                                        } //FIN DEL RECORRIDO DETALLE OF

                                        if (integrar)
                                        {
                                            //Una vez que se tiene la informacion de OF, Detalle de OF y Lotes a Consumir por Articulo del Detalle de OF, se envía a la Integración de de SAP
                                            Logs.FullInfo("", "Envio a integracion de Informacion de la Orden de Fabricacion: " + produccion.DocNum + " ************* ", Usuario, 0, modulo, metodo);
                                            Crear.Aproduccion(ref mensaje, ref mensajes, produccion, detalleOf, g.ToString(), Ob.ObtenerDocumentoAProduccion(g.ToString(), "EPM", produccion.DocNum), Ob.ObtenerMaquina(produccion.DocNum), CodeGuiaER, regMaster);
                                        }
                                        else
                                        {
                                            //OF sin lotes a consumir | Lotes a consumir insuficientes
                                            Logs.FullInfo("", "OF " + produccion.DocNum + " no procesada, Consumo insuficiente | " + msgErrConsumo, Usuario, 0, modulo, metodo); //Log Informativo

                                            mensajes.Add(new MsjProduccion
                                            {
                                                DocEntry = "-1",
                                                DocNum = "-1",
                                                Integrado = false,
                                                OF = produccion.DocNum,
                                                MsjError = msgErrConsumo
                                            });
                                        }
                                    }
                                    else
                                    {
                                        //OF no Liberada
                                        Logs.FullInfo("", "OF " + produccion.DocNum + " no procesada, No se encuentra Liberada", Usuario, 0, modulo, metodo); //Log Informativo

                                        mensajes.Add(new MsjProduccion
                                        {
                                            DocEntry = "-1",
                                            DocNum = "-1",
                                            Integrado = false,
                                            OF = produccion.DocNum,
                                            MsjError = "El estatus de la orden de trabajo debe ser Liberada, verificar antes de proceder."
                                        });
                                    }

                                    Logs.FullInfo(" <<<<< Procesamiento de la Orden de Fabricacion " + produccion.DocNum + " <<<<< ", "", Usuario, 4, "", "");                                    
                                    break;

                                default: //Existe - NO se procesa, se sale del proceso y se muestra el Documento
                                    string[] dc = DocNum.Split('&');
                                    Logs.FullInfo("",   "Obtenido por Verificacion de CodeGuia: " + CodeGuiaER + 
                                                        " --- Documento de Emision Creado: " + dc[0] + 
                                                        " --- Recibo de Produccion Creado: " + dc[1]
                                                        , Usuario, 0, modulo, metodo); //Log Informativo

                                    mensajes.Add(new MsjProduccion
                                    {
                                        DocEntry = "Emision Num. Doc. : " + dc[0] + " - Recibo de Produccion Num. " + dc[1],
                                        DocNum = "Emision Num. Doc. : " + dc[0] + " - Recibo de Produccion Num. " + dc[1],
                                        Integrado = true,
                                        OF = produccion.DocNum,
                                        MsjError = string.Empty
                                    });
                                    break;
                            }
                        }
                        else
                        {
                            Logs.FullInfo("", "Error Verificación CodeGuia => Mensaje ::: " + DocNum, Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = -1, mensaje = "Error Verificación CodeGuia => Mensaje ::: " + DocNum });
                        }
                    }
                    Logs.FullInfo("Etiquetas PT", "", Usuario, 4, modulo, metodo); //Log Informativo   
                    return Json(new { codigo = 0, mensaje, documentos = mensajes, traspaleo = lotesTT }, JsonRequestBehavior.DenyGet);
                }
                else
                {
                    Logs.FullInfo("", "Borrador NO creado, las etiquetas no fueron creadas en el Borrador (Produccion)", Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = -1, mensaje = result }, JsonRequestBehavior.DenyGet);
                }                    
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo

                return Json(new { codigo = -1, mensaje = ex.Message }, JsonRequestBehavior.DenyGet);
            }
            finally
            {
                Logs.FullInfo("Modulo: Recibo de Produccion - Fin", "", Usuario, 2, "", ""); //Log Informativo
            }
        }

        //Verificar registros duplicados en OF
        public bool ValidaItemWhs(List<Produccion.ConsumoOrdenFabricacion> detalleOf, string ItemCode, string WhsCode, int LineNum) {
            var respuesta = false;
            foreach (var reg in detalleOf) {
                //Validar registros distintos al actual validado
                if (reg.LineNum != LineNum && (reg.ItemCode == ItemCode && reg.Warehouse == WhsCode)) {
                    respuesta = true;
                    break;
                }
            }
            
            return respuesta;
        }

        [HttpPost]
        public ActionResult Fifo(string Qr, List<Etiqueta> lineas)
        {
            //int i = -1;
            List<Fifo> listaF = new List<Fifo>();
            if (lineas == null)
                lineas = new List<Etiqueta>();

            if (Ob.obtenerFIFOLote(ref listaF, Qr.Split('&')[0], Qr.Split('&')[1], lineas))
            {
                if (listaF.Count == 1 || listaF.Count == 0)
                    return Json(new { codigo = 0, mensaje = true }, JsonRequestBehavior.DenyGet);
                else
                    return Json(new { codio = -1, mensaje = $"Existe el lote {listaF[0].DistNumber} con fecha: {listaF[0].InDate}" }, JsonRequestBehavior.DenyGet);
                //if (i == 1)
                //    return Json(new { codigo = 0, mensaje = true }, JsonRequestBehavior.DenyGet);
                //else
                //    return Json(new { codio = -1, mensaje = "Existe un lote con una fecha más antigua"}, JsonRequestBehavior.DenyGet);
            }
            else
                return Json(new { codigo = -1, mensaje = "Error al obtener lote por FIFO" }, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public ActionResult GuardarBorrador(List<Produccion.EmisionRecibo> Lineas, string guid, List<Produccion.EmisionRecibo> RegMaster)
        {
            string response = string.Empty;
            int totalOk = 0;
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();

            Logs.FullInfo("Modulo: Recibo de Produccion ::: G U A R D A R   B O R R A D O R - Inicio", "", Usuario, 1, "", ""); //Log Informativo
            try
            {
                if (Lineas == null || Lineas.Count == 0)
                {
                    Logs.FullInfo("", "No se leyeron Etiquetas", Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = -1, mensaje = $"Para guardar un borrador debe leer al menos una etiqueta" }, JsonRequestBehavior.DenyGet);
                }
                else
                {
                    //**************************************************************************
                    //Agregar registros de Master a Lista a guardar en Borrador
                    //**************************************************************************
                    if (RegMaster != null && RegMaster.Count > 0) 
                    {
                        Lineas.AddRange(RegMaster);
                    }             
                    //**************************************************************************
                    //**************************************************************************

                    Logs.FullInfo("", "\r\n\t Etiquetas a Procesar", Usuario, 0, modulo, metodo); //Log Informativo
                    foreach (Produccion.EmisionRecibo etq in Lineas)
                    {
                        Logs.FullInfo("", "Etiqueta: " + etq.DistNumber, Usuario, 0, modulo, metodo); //Log Informativo
                    }

                    if (SP.GuardarBorrador(Lineas, guid, ref response, ref totalOk))
                    {
                        if (response.Length > 0)
                        {

                            return Json(new { codigo = -2, mensaje = "Se Guardaron: " + totalOk + " Etiquetas." + response }, JsonRequestBehavior.DenyGet);
                        }
                        else
                        {
                            return Json(new { codigo = 0, mensaje = "Borrador Guardado Completo" }, JsonRequestBehavior.DenyGet);
                        }
                    }
                    else
                    {
                        return Json(new { codigo = -1, mensaje = $"Error: {response}" }, JsonRequestBehavior.DenyGet);
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo

                return Json(new { codigo = -1, mensaje = ex.Message }, JsonRequestBehavior.DenyGet);
            }
            finally
            {
                Logs.FullInfo("Modulo: Recibo de Produccion ::: G U A R D A R   B O R R A D O R - Fin", "", Usuario, 2, "", ""); //Log Informativo
            }
        }

        [HttpPost]
        public ActionResult BorrarRBorrador(Produccion.EmisionRecibo registro)
        {
            string msj = string.Empty;
            if (SP.EliminarRegistroBorrador(ref msj, registro))
            {
                return Json(new { codigo = 0, mensaje = "Registro eliminado de Borrador" }, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(new { codigo = -1, mensaje = $"Error: {msj}" }, JsonRequestBehavior.DenyGet);
            }
        }

        [HttpPost]
        public ActionResult BorrarTBorrador(List<Produccion.EmisionRecibo> Lineas)
        {
            int response = SP.EliminarTBorrador(Lineas);
            if (response == 0)
                return Json(new { codigo = 0, mensaje = "Borrador eliminado" }, JsonRequestBehavior.DenyGet);
            else
                return Json(new { codigo = -1, mensaje = $"Surgio un error durante el proceso, intente una vez más. Registros eliminados: {response} de {Lineas.Count}" });
        }

        [HttpGet]
        public ActionResult ValidaEtiqueta(string Lote) //Verificar si la etiqueta existe en un borrador - IUO 30/08/2020
        {
            List<BorradorEtiqueta> Lista = new List<BorradorEtiqueta>();
            if (Ob.EtqBorrador(ref Lista, Lote.Split('&')[0], Lote.Split('&')[1]))
            {
                return Json(new { codigo = 0, mensaje = Lista }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { codigo = -1, mensaje = "Error al obtener informacion de la etiqueta" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Turnos() {
            List<Produccion.Turnos> Turnos = new List<Produccion.Turnos>();
           
            switch (SP.Turnos(ref Turnos))
            {
                case true:
                    if (Turnos.Count > 0) {
                        return Json(new { codigo = 0, mensaje = Turnos, turno = int.Parse(Ob.ObtenerTurno()) }, JsonRequestBehavior.AllowGet); 
                    }
                    else
                        return Json(new { codigo = -1, mensaje = "No se recuperaron registros de turnos" }, JsonRequestBehavior.AllowGet);
                case false:
                default:
                    return Json(new { codigo = -1, mensaje = "No se pudierón recuperar registros de los turnos" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult RecuperarBorrador(string type, int order, int turno) 
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();

            Logs.FullInfo("Modulo: Recibo de Produccion ::: R E C U P E R A R   B O R R A D O R - Inicio", "", Usuario, 1, "", ""); //Log Informativo

            try
            {
                switch (type)
                {
                    case "L"://Listar Borradores del usuario
                        Logs.FullInfo("", "***** Recuperar Lista de Borradores *****", Usuario, 0, modulo, metodo); //Log Informativo

                        List<Produccion.Borrador> listaBorrador = new List<Produccion.Borrador>();
                        if (Ob.ListaBorradores(turno, ref listaBorrador))
                        {
                            if (listaBorrador.Count > 0)
                            {
                                Logs.FullInfo("", "Lista de Borradores: Recuperacion Exitosa", Usuario, 0, modulo, metodo); //Log Informativo
                                return Json(new { codigo = 0, mensaje = listaBorrador }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                Logs.FullInfo("", "Lista de Borradores: No existen registros guardados", Usuario, 0, modulo, metodo); //Log Informativo
                                return Json(new { codigo = -1, mensaje = $"No existen registros guardados" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            Logs.FullInfo("", "Error al recuperar la lista de Borradores", Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = -1, mensaje = $"Error al recuperar la lista de Borradores" }, JsonRequestBehavior.AllowGet);
                        }

                    case "R"://Recuperar Borrador seleccionado
                        Logs.FullInfo("", " <<<<< Recuperar Etiquetas del Borrador >>>>> ", Usuario, 0, modulo, metodo); //Log Informativo

                        List<Produccion.EmisionRecibo> Lineas = new List<Produccion.EmisionRecibo>();
                        string almacenSel = string.Empty;
                        if (Ob.RecuperarBorrador(order, turno, ref Lineas))
                        {
                            if (Lineas.Count > 0)
                            {
                                Logs.FullInfo("", "Recuperar Etiquetas: Recuperacion Exitosa", Usuario, 0, modulo, metodo); //Log Informativo
                                return Json(new { codigo = 0, mensaje = Lineas }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                Logs.FullInfo("", "Recuperar Etiquetas: No se recuperarón etiquetas de la OF " + order + " en el Turno " + turno, Usuario, 0, modulo, metodo); //Log Informativo
                                return Json(new { codigo = -1, mensaje = "No se recuperarón etiquetas de la OF " + order + " en el Turno " + turno }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            Logs.FullInfo("", "Error al recuperar etiquetas guardadas", Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = -2, mensaje = "Error al recuperar etiquetas guardadas" }, JsonRequestBehavior.AllowGet);
                        }

                    default:
                        Logs.FullInfo("", "No se reconoce el tipo de acción indicado", Usuario, 0, modulo, metodo); //Log Informativo
                        return Json(new { codigo = -2, mensaje = $"No se reconoce el tipo de acción indicado" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo

                return Json(new { codigo = -1, mensaje = ex.Message }, JsonRequestBehavior.DenyGet);
            }
            finally
            {
                Logs.FullInfo("Modulo: Recibo de Produccion ::: R E C U P E R A R   B O R R A D O R - ", "", Usuario, 2, "", ""); //Log Informativo
            }
        }

    }
}