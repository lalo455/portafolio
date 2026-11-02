using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using WMS.Logic.Comun;
using WMS.Models;

namespace WMS.Controllers
{
    public class TraspaleoController : Controller
    {
        Obtener Ob = new Obtener();
        Utilities.Log Logs = new Utilities.Log();
        Integrar Int = new Integrar();

        // GET: Traspaleo
        public ActionResult Traspaleo()
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


        [HttpPost]
        public ActionResult Etiqueta(string QR) //Obtenemos Informacion de Etiqueta
        { 
            List<Traspaleo> lista = new List<Traspaleo>();
            if (Ob.InformacionTraspaleo(ref lista, QR))
            {
                if (lista.Count > 0)
                {
                    if (lista[0].U_Cancel == 'Y') //Se valida si la Etiqueta esta CANCELADA 
                    {
                        if (lista[0].U_stEti == 'T') //Si la etiqueta cancelada tiene el Status T 
                        {
                            return Json(new { codigo = -2, mensaje = "La etiqueta fue aplicada en un proceso de TRASPALEO" }, JsonRequestBehavior.DenyGet);
                        }
                        else
                        {
                            return Json(new { codigo = -1, mensaje = "La etiqueta esta CANCELADA" }, JsonRequestBehavior.DenyGet);
                        }
                    }
                    else if (lista[0].U_stEti == 'G') //Si la etiqueta esta ACTIVA -- Se verifica el estatus de la Etiqueta
                    {
                        return Json(new { codigo = -1, mensaje = "La etiqueta NO ha sido LIBERADA" }, JsonRequestBehavior.DenyGet);
                    }
                    else if (lista[0].U_stEti == 'T') //Si la etiqueta esta ACTIVA -- Se verifica el estatus de la Etiqueta
                    {
                        return Json(new { codigo = -2, mensaje = "La etiqueta ya tiene aplicada un proceso de TRASPALEO" }, JsonRequestBehavior.DenyGet);
                    }
                    else if (lista[0].U_stEti == 'E') //Si la etiqueta esta ACTIVA -- Se verifica el estatus de la Etiqueta
                    {
                        return Json(new { codigo = -2, mensaje = "La etiqueta ya tiene aplicada un proceso de REMISION" }, JsonRequestBehavior.DenyGet);
                    }
                    else //Si la etiqueta esta ACTIVA y LIBERADA -- Se procesa la información de la etiqueta leida
                    {
                        return Json(new { codigo = 0, mensaje = lista }, JsonRequestBehavior.DenyGet);
                    }
                }
                else
                {
                    //return Json(new { codigo = -1, mensaje = "No se recupero informacion para la etiqueta: " + QR }, JsonRequestBehavior.DenyGet);
                    return Json(new { codigo = -1, mensaje = "Validar Lote: " + QR + ", verificar Lote LIBERADO y con EXISTENCIA en Almacen." }, JsonRequestBehavior.DenyGet);
                }
            }
            else
            {
                return Json(new { codigo = -1, mensaje = "Error al recuperar información de la etiqueta: " + QR }, JsonRequestBehavior.DenyGet);
            }
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
            finally
            {
                //Logs.FullInfo(" L E C T U R A S  O R I G I N A L E S", "", Usuario, 4, "", ""); //Log Informativo
                //Logs.FullInfo(" <<<<<<<<<<<<<<<<<<<<<<<<<<<<>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>", "", Usuario, 2, "", ""); //Log Informativo
            }
        }



        [HttpPost]
        public ActionResult Crear(List<Traspaleo> Lineas) //Crear el Archivo TXT con los Lotes leidos 
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();
            //string result = string.Empty;
            MsjArchivo msj = new MsjArchivo();

            Logs.FullInfo("Modulo: Traspaleo - Inicio", "", Usuario, 1, "", ""); //Log Informativo
            Logs.FullInfo("", "Etiquetas a Procesar", Usuario, 0, modulo, metodo); //Log Informativo
            //---
            foreach (Traspaleo etq in Lineas)
            {
                Logs.FullInfo("", "Etiqueta: " + etq.Name, Usuario, 0, modulo, metodo); //Log Informativo
            }
            //-----

            /*
             * ---Este ya no se utiliza, se creará archivo txt en lugar de guardar las etiquetas -- 14-01-2021
            try
            {
                if (Ob.EtiquetasTraspaleo(Lineas, ref msj, EtiquetasIN(Lineas)))
                {
                    switch (result)
                    {
                        case "":
                        case null:
                            Logs.FullInfo("", "Registros de traspaleo generados correctamente", Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = 0, mensaje = "Registros de traspaleo generados correctamente" }, JsonRequestBehavior.DenyGet);
                        default:
                            Logs.FullInfo("", "Error en el Registro: " + result, Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = -1, mensaje = result }, JsonRequestBehavior.DenyGet);
                    }
                }
                else
                {
                    Logs.FullInfo("", "Error al guardar Traspaleo --- " + result, Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = -1, mensaje = result }, JsonRequestBehavior.DenyGet);
                }                    
            }
            catch (Exception ex)
            {
                Logs.Registrar("Traspaleo NO Realizado", ex.Message, modulo, metodo);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = result }, JsonRequestBehavior.DenyGet);
            }
            */

            //Actuallizacion para creación de archivo txt de etiquetas Traspaleo -- 14-01-2021
            try
            {
                if (Int.CrearArchivoTraspaleo(Lineas, ref msj, EtiquetasIN(Lineas)))
                {
                    Logs.FullInfo("", "Documento creado con exito, " + msj.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = 0, mensaje = msj.Mensaje }, JsonRequestBehavior.DenyGet);
                }
                else
                {
                    Logs.FullInfo("", "Error en la creación de Documento, " + msj.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = -1, mensaje = msj.Mensaje }, JsonRequestBehavior.DenyGet);
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar("Error en Procesamiento de Etiquetas: ", ex.Message, modulo, metodo);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = ex.Message }, JsonRequestBehavior.DenyGet);
            }
            finally
            {
                Logs.FullInfo("Modulo: Traspaleo - Fin", "", Usuario, 2, "", ""); //Log Informativo
            }

        }

        public string EtiquetasIN(List<Traspaleo> Lineas) //Listar las etiquetas a Procesar
        {
            string cadena = string.Empty;
            for (int i = 0; i < Lineas.Count; i++) 
            {
                if (string.IsNullOrEmpty(cadena)) 
                    cadena = $"''{Lineas[i].Name}''";
                else 
                    cadena += $",''{Lineas[i].Name}''";
            }
            return cadena;
        }

    }
}