using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using WMS.Logic.Comun;
using WMS.Utilities;
using WMS.Models;
using System.Runtime.ConstrainedExecution;
using WMS.Logic.Stores;
using System.Configuration;
using System.Reflection;
using System.Threading;
using static WMS.Models.Produccion;
using System.IO;
using CrystalDecisions.CrystalReports.Engine;

namespace WMS.Controllers
{
    public class EtiquetaMasterController : Controller
    {
        Obtener Ob;
        Log Logs;
        Integrar IntSAP;
        StoreProcedure SP;
        ImpresionController Impresion;

        public EtiquetaMasterController()
        {
            Ob = new Obtener();
            Logs = new Log();
            IntSAP = new Integrar();
            SP = new StoreProcedure();
            Impresion = new ImpresionController();
        }

        // GET: EtiquetaMaster
        public ActionResult EtiquetaMaster()
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

        //**********************************************
        [HttpGet]
        public ActionResult ObtenerPrefijo()
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();
            string prefijo = "";

            //Logs.FullInfo("Obtener Prefijo", "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                if (Ob.Prefijo(ref prefijo))
                {
                    switch (prefijo)
                    {
                        case null:
                        case "":
                            Logs.FullInfo("", "No se ha definido prefijo de Etiqueta Master", Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = -2, mensaje = "No se ha definido prefijo de Etiqueta Master" }, JsonRequestBehavior.AllowGet);

                        default:
                            Logs.FullInfo("", "Prefijo de Etiqueta Master: " + prefijo, Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = 0, mensaje = prefijo }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    Logs.FullInfo("", "Error al obtener información del Prefijo de Etiqueta Master: " + prefijo, Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = -2, mensaje = "Error al obtener información del Prefijo de Etiqueta Master" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = "ERROR: Mensaje: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            //finally
            //{
            //    Logs.FullInfo("Obtener Prefijo", "", Usuario, 4, "", ""); //Log Informativo
            //}
        }

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
        }

        [HttpGet]
        public ActionResult VerificarEtq(string Lote)
        {
            string StEti = string.Empty;
            List<InicialEtiqueta> Lista = new List<InicialEtiqueta>();
            if (Ob.EstadoEtiqueta(ref Lista, Lote))
            {
                if (Lista.Count == 0)
                {
                    return Json(new { codigo = -2, mensaje = "No se obtuvo información del estatus de etiqueta leída" }, JsonRequestBehavior.AllowGet);
                }
                else if (Lista.Count > 1) 
                {
                    return Json(new { codigo = -1, mensaje = "Existe más de un registro de etiqueta leída, verificar y volver a leer" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    switch (Lista[0].Estatus)
                    {
                        case "L":
                            return Json(new { codigo = 0, mensaje = "" }, JsonRequestBehavior.AllowGet);

                        case "X":
                            return Json(new { codigo = -2, mensaje = "La etiqueta leída esta BLOQUEADA" }, JsonRequestBehavior.AllowGet);

                        case "T": //Estatus de estiqueta 
                            return Json(new { codigo = -2, mensaje = "La etiqueta leída fue utilizada en TRASPALEO" }, JsonRequestBehavior.AllowGet);
                        
                        case "E": //Estatus de estiqueta 
                            return Json(new { codigo = -2, mensaje = "La etiqueta leída fue EMBARCADA" }, JsonRequestBehavior.AllowGet);

                        case null:
                        case "":
                            return Json(new { codigo = -2, mensaje = "No se obtuvo información del estatus de etiqueta leída" }, JsonRequestBehavior.AllowGet);

                        default:
                            return Json(new { codigo = -2, mensaje = "La etiqueta existe, no ha sido LIBERADA" }, JsonRequestBehavior.AllowGet);
                    }
                    //return Json(new { codigo = 0, mensaje = Lista }, JsonRequestBehavior.DenyGet);
                }
            }
            else
            {
                return Json(new { codigo = -1, mensaje = "Error al obtener información de la etiqueta" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ObtenerLote(string Lote, string Articulo)
        {
            List<Etiqueta> Lotes = new List<Etiqueta>();
            if (Ob.MasterLoteInd(ref Lotes, Lote, Articulo))
            {
                switch (Lotes.Count)
                {
                    case 0:
                        return Json(new { codigo = -1, mensaje = "No se recuperó informacion de la etiqueta" }, JsonRequestBehavior.AllowGet);

                    default:
                        return Json(new { codigo = 0, mensaje = Lotes }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { codigo = -1, mensaje = "Error al obtener información de la etiqueta" }, JsonRequestBehavior.AllowGet);
            }                
        }

        //Generación de Etiqueta Master
        [HttpPost]
        public ActionResult GenerarMaster(List<Etiqueta> ListaEtq)
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();

            Guid g;
            g = Guid.NewGuid();
            MensajesML msj = new MensajesML();

            Logs.FullInfo("Modulo: Generacion de Etiqueta Master - Inicio", "", Usuario, 1, "", ""); //Log Informativo
            if (SP.GuardarRegistrosMaster(ListaEtq, "Crear", g.ToString())) //Guarda Registro en Tabla Movimiento
            {
                //Creación de Master
                if (IntSAP.RegistroMaster(ref msj, g.ToString()))
                {
                    Logs.FullInfo("", "Master generada: " + msj.CodigoMaster + " - " + msj.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = 0, mensaje = msj.Mensaje, mlcodigo = msj.CodigoMaster }, JsonRequestBehavior.DenyGet); //Muestra Documento Creado
                }
                else
                {
                    Logs.Registrar("Master no generada: ", msj.Mensaje, modulo, metodo);
                    Logs.FullInfo("", "Error al generar master: " + msj.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo                    
                    return Json(new { codigo = -1, mensaje = msj.Mensaje }, JsonRequestBehavior.DenyGet); //No se genero Documento
                }
            }
            else
            {
                Logs.FullInfo("", "Error al guardar los registros para etiqueta master", Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = "Error al guardar los registros para etiqueta master" }, JsonRequestBehavior.DenyGet);
            }
        }

        //**********************************************
        //Listar Etiquetas Master disponibles para editar
        [HttpGet]
        public ActionResult ListarMaster()
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();
            List<EtqMaster> Datos = new List<EtqMaster>();

            //Logs.FullInfo("Listar Etiquetas Master validas", "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                if (Ob.DetalleMaster(ref Datos, "", "Listar"))
                {
                    switch (Datos.Count)
                    {
                        case 0:
                            Logs.FullInfo("", "No hay registros de etiquetas master", Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = -1, mensaje = "No hay registros de etiquetas master" }, JsonRequestBehavior.AllowGet);

                        default:
                            Logs.FullInfo("", "Etiquetas Master encontradas: " + Datos.Count, Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = 0, mensaje = Datos }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    Logs.FullInfo("", "Error al obtener información de etiquetas master", Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = -1, mensaje = "Error al obtener información de etiquetas master" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = "ERROR: Mensaje: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            //finally
            //{
            //    Logs.FullInfo("Listas Etiquetas Master validas", "", Usuario, 4, "", ""); //Log Informativo
            //}
        }

        //Obtener información de Etiquetas Master para editar
        [HttpGet]
        public ActionResult ObtenerMaster(string master)
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();
            List<EtqMaster> Datos = new List<EtqMaster>();

            //Logs.FullInfo("Obtener información de Etiqueta Master " + master, "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                if (Ob.DetalleMaster(ref Datos, master, "Cargar"))
                {
                    switch (Datos.Count)
                    {
                        case 0:
                            Logs.FullInfo("", "No hay registros de la etiquetas master " + master, Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = -1, mensaje = "No hay registros de la etiquetas master " + master }, JsonRequestBehavior.AllowGet);

                        default:
                            Logs.FullInfo("", "Registros de Etiqueta Master: " + Datos.Count, Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = 0, mensaje = Datos }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    Logs.FullInfo("", "Error al obtener información de etiquetas master", Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = -1, mensaje = "Error al obtener información de etiquetas master" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = "ERROR: Mensaje: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            //finally
            //{
            //    Logs.FullInfo("Obtener información de Etiquetas Master " + master, "", Usuario, 4, "", ""); //Log Informativo
            //}
        }

        //Eliminar registro de Master
        [HttpPost]
        public ActionResult EliminarLoteM(string loteReg, string articuloReg, double cantidadReg, string master, double cantidad)
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();
            string msj = string.Empty;
            int codigo = 0;

            Logs.FullInfo("Eliminar Lote " + loteReg + " de Etiqueta Master " + master, "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                SP.EliminarRegM(ref msj, ref codigo, loteReg, articuloReg, cantidadReg, master, cantidad);
                if (codigo == 0)
                {
                    Logs.FullInfo("", "Master actualizada, lote eliminado", Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = 0, mensaje = "Master actualizada, lote eliminado" }, JsonRequestBehavior.DenyGet); 
                }
                else
                {
                    Logs.FullInfo("", msj, Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = -1, mensaje = msj }, JsonRequestBehavior.DenyGet);
                }
            }
            catch (Exception ex)
            {
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = "ERROR: Mensaje: " + ex.Message }, JsonRequestBehavior.DenyGet);
            }
            finally
            {
                Logs.FullInfo("Eliminar Lote " + loteReg + " de Etiqueta Master " + master, "", Usuario, 4, "", ""); //Log Informativo
            }
        }

        //Actualizar Etiqueta Master
        [HttpPost]
        public ActionResult ActualizarMaster(List<Etiqueta> ListaEtq)
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();

            Guid g;
            g = Guid.NewGuid();
            MensajesML msj = new MensajesML();

            Logs.FullInfo("Modulo: Actualizar Etiqueta Master - Inicio", "", Usuario, 1, "", ""); //Log Informativo
            if (SP.GuardarRegistrosMaster(ListaEtq, "Editar", g.ToString())) //Guarda Registro en Tabla Movimiento
            {
                //Creación de Master
                if (IntSAP.EditarMaster(ref msj, g.ToString()))
                {
                    Logs.FullInfo("", "Master actualizada: " + msj.CodigoMaster + " - " + msj.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = 0, mensaje = msj.Mensaje, mlcodigo = msj.CodigoMaster }, JsonRequestBehavior.DenyGet); //Muestra Documento Creado
                }
                else
                {
                    Logs.Registrar("Master no actualizada: ", msj.Mensaje, modulo, metodo);
                    Logs.FullInfo("", "Error al actualizar master: " + msj.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo                    
                    return Json(new { codigo = -1, mensaje = msj.Mensaje }, JsonRequestBehavior.DenyGet); //No se genero Documento
                }
            }
            else
            {
                Logs.FullInfo("", "Error al guardar los registros para actualizar etiqueta master", Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = "Error al guardar los registros para actualizar etiqueta master" }, JsonRequestBehavior.DenyGet);
            }
        }

        //Impresión de Etiqueta
        public FileResult ImprimirEtqMaster(string Codigo)
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();
            
            Logs.FullInfo("", "Impresión de Etiqueta Master: " + Codigo, Usuario, 0, modulo, metodo);
            
            FileStream fss = null;
            try
            {
                //*********************************************************************************************
                //Carpeta para almacenar Etiquetas Master creadas
                string rutaImprMaster = ConfigurationManager.AppSettings["RutaPDFMaster"];

                //Verificar si la carpeta Existe
                if (!Directory.Exists($"{rutaImprMaster}"))
                {
                    //Crear la carpeta si no existe
                    Directory.CreateDirectory($"{rutaImprMaster}");
                }

                var LocalDirectory = $"{rutaImprMaster}\\EtiquetaMaster_{Codigo}.pdf";
                Logs.FullInfo("", "FileStream LocalDirectory: " + LocalDirectory, Usuario, 0, modulo, metodo);
                //*********************************************************************************************
                //*********************************************************************************************
                var stream = Impresion.GenerarImpresion(Codigo);
                int buffLength = 2048;
                byte[] buff = new byte[buffLength];
                int contentLen;
                //*********************************************************************************************
                //*********************************************************************************************
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

                Impresion = null;
                fss = new FileStream(LocalDirectory, FileMode.Open, FileAccess.Read);

            }
            catch (Exception ex)
            {
                Logs.FullInfo("Error en la Impresión de Etiqueta Master: " + ex.Message, "", Usuario, 2, "", ""); //Log Informativo
            }

            return File(fss, "application/pdf");
        }

        //*****************************************************************************
        //Obtener información de Etiquetas Master para procesar en Módulos [Traspaso]
        [HttpGet]
        public ActionResult ObtenerMasterDetalle(string QR, string Origen)
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();
            List<EtqMaster.Detalle> Datos = new List<EtqMaster.Detalle>();
            string msj = string.Empty;

            //Logs.FullInfo("Obtener información de Etiqueta Master a procesar", "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                if (Ob.DatosMaster(ref Datos, ref msj, QR, Origen))
                {
                    switch (Datos.Count)
                    {
                        case 0:
                            Logs.FullInfo("", "No se econtró información de Etiqueta Master  " + QR, Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = -1, mensaje = "No se obtuvo información de Etiqueta Master, verificar en SAP"}, JsonRequestBehavior.AllowGet);

                        default:
                            Logs.FullInfo("", "Registros de Etiqueta Master: " + Datos.Count, Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = 0, mensaje = Datos }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    Logs.FullInfo("", "Error al obtener información de la etiqueta master " + QR, Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = -1, mensaje = "Error al obtener información de Etiqueta Master, " + msj }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logs.FullInfo("", "ERROR: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = "ERROR: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            //finally
            //{
            //    Logs.FullInfo("Obtener información de Etiquetas Master a procesar", "", Usuario, 4, "", ""); //Log Informativo
            //}
        }

        //Listar Etiquetas Master disponibles para editar
        [HttpGet]
        public ActionResult ObtenerListaMaster(string QR)
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();
            List<EtqMaster.ListaMaster> Datos = new List<EtqMaster.ListaMaster>();

            //Logs.FullInfo("Listar Ubicaciones de Etiqueta Master", "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                if (Ob.ListarMaster(ref Datos, QR))
                {
                    switch (Datos.Count)
                    {
                        case 0:
                            Logs.FullInfo("", "No hay registros de etiquetas master", Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = -1, mensaje = "No se obtuvieron registros de la etiquetas master" }, JsonRequestBehavior.AllowGet);

                        default:
                            Logs.FullInfo("", "Registros de Master encontradas: " + Datos.Count, Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = 0, mensaje = Datos }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    Logs.FullInfo("", "Error al obtener información de etiqueta master", Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = -1, mensaje = "Error al obtener información de etiqueta master" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = "ERROR: Mensaje: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            //finally
            //{
            //    Logs.FullInfo("Listas Ubicaciones Etiqueta Master", "", Usuario, 4, "", ""); //Log Informativo
            //}
        }

        //*****************************************************************************
        [HttpGet]
        public ActionResult ObtenerMasterLiberar(string Codigo, string Articulo, int Orden)
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();
            List<EtqMaster.Detalle> Datos = new List<EtqMaster.Detalle>();
            string msj = string.Empty;

            //Logs.FullInfo("Obtener información de Etiqueta Master a procesar", "", Usuario, 3, "", ""); //Log Informativo
            try
            {
                if (Ob.InfoMasterLiberar(ref Datos, ref msj, Codigo, Articulo, Orden))
                {
                    switch (Datos.Count)
                    {
                        case 0:
                            Logs.FullInfo("", "No se econtró información de Etiqueta Master  " + Codigo, Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = -1, mensaje = "No se obtuvo información de Etiqueta Master, verificar en SAP" }, JsonRequestBehavior.AllowGet);

                        default:
                            Logs.FullInfo("", "Registros de Etiqueta Master: " + Datos.Count, Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = 0, mensaje = Datos }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    Logs.FullInfo("", "Error al obtener información de la etiqueta master " + Codigo, Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = -1, mensaje = "Error al obtener información de Etiqueta Master, " + msj }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logs.FullInfo("", "ERROR: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = "ERROR: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            //finally
            //{
            //    Logs.FullInfo("Obtener información de Etiquetas Master a procesar", "", Usuario, 4, "", ""); //Log Informativo
            //}
        }


    }
}