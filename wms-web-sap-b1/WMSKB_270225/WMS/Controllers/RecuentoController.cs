using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using WMS.Models;
using WMS.Logic.Comun;
using WMS.Logic.Stores;
using static WMS.Models.Recuento;
using System.Reflection;

namespace WMS.Controllers
{
    public class RecuentoController : Controller
    {
        // GET: Recuento
        Obtener Ob = new Obtener();
        Integrar Int = new Integrar();
        Utilities.Log Logs = new Utilities.Log();
        
        public ActionResult RecuentoInv()
        {
            try
            {
                if (String.IsNullOrEmpty(( (ClaimsIdentity)User.Identity).Claims.First().Value))
                {
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
        
        [HttpGet]
        public ActionResult ObtenerAlms() {
            List<TrsManual.Almacen> ListaAlm = new List<TrsManual.Almacen>();
            if (Ob.ObtenerAlm(ref ListaAlm, "", "Todos"))
                return Json(new { codigo = 0, mensaje = ListaAlm }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { codigo = -1, mensaje = "No se recuperaron almacenes"}, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ListaUbicaciones(string WhsCode)
        {
            List<Comun.Location> Locations = new List<Comun.Location>();
            if (Ob.UbicacionesLista(WhsCode, "", ref Locations))
            {
                if (Locations.Count > 0)
                {
                    return Json(new { codigo = 0, mensaje = Locations }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { codigo = -1, mensaje = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { codigo = -2, mensaje = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult LoteRecuento(string WhsCode, string Batch, string Item)
        {
            List<Recuento.LtsRecuento> ListaBt = new List<LtsRecuento>();
            if (Ob.LotesRecuento(ref ListaBt, WhsCode, Batch, Item))
            {
                if (ListaBt.Count > 0)
                {
                    return Json(new { codigo = 0, mensaje = ListaBt });
                }
                else
                {
                    return Json(new { codigo = -1, mensaje = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { codigo = -2, mensaje = "" });
            }
        }

        [HttpPost]
        public ActionResult CrearTxt(List<LtsRecuento> lineas, string WhsCode, int NumRec) 
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;

            MsjArchivo msj = new MsjArchivo();

            Logs.FullInfo("Modulo: Recuento de Inventario - Inicio", "", Usuario, 1, "", ""); //Log Informativo
            Logs.FullInfo("", "Etiquetas a Procesar --- Almacen: " + WhsCode + ", --- No. Recuento: " + NumRec, Usuario, 0, modulo, metodo); //Log Informativo
            //---
            foreach (LtsRecuento etq in lineas)
            {
                Logs.FullInfo("", "Etiqueta: " + etq.DistNumber, Usuario, 0, modulo, metodo); //Log Informativo
            }
            //-----
            try
            {
                if (Int.CrearArchivo(ref msj, WhsCode, NumRec, lineas))
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
                Logs.FullInfo("Modulo: Recuento de Inventario - Fin", "", Usuario, 2, "", ""); //Log Informativo
            }            
        }
                
    }
}