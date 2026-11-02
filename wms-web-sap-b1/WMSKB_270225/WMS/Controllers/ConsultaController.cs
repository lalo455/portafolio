using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using WMS.Logic.Comun;
using WMS.Logic.Stores;
using WMS.Models;
using WMS.Utilities;

namespace WMS.Controllers
{
    public class ConsultaController : Controller
    {
        StoreProcedure SP;
        Obtener Ob;
        Log Logs;

        public ConsultaController()
        {
            SP = new StoreProcedure();
            Ob = new Obtener();
            Logs = new Log();
        }

        // GET: Consulta
        public ActionResult Consulta()
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
        public ActionResult VerificarEtq(string QR)
        {
            string StEti = string.Empty;
            List<InicialEtiqueta> Lista = new List<InicialEtiqueta>();

            //Si la etiqueta es diferente de T00000 se puede leer y procesar
            if (Ob.EstadoEtiqueta(ref Lista, QR.Split('&')[0]))
            {
                if (Lista.Count > 1) //Existe mas de un resultado
                {
                    return Json(new { codigo = -1, mensaje = "Existe más de un Registro de Etiqueta." }, JsonRequestBehavior.DenyGet);
                }
                else
                {
                    foreach (InicialEtiqueta etq in Lista)
                    {
                        //Verificacion de Etiqueta de Traspaleo - se toma del WebConfig
                        //Acepta etiquetas T0000 y U0000
                        //Se agrega el parametro de etiquetas traspaleo tipo U
                        if (QR.StartsWith(ConfigurationManager.AppSettings["TRL"]) || QR.StartsWith(ConfigurationManager.AppSettings["TRLU"]))
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
                                case "L":
                                    return Json(new { codigo = 0, mensaje = "" }, JsonRequestBehavior.DenyGet);
                                
                                case "X":
                                    return Json(new { codigo = -2, mensaje = "La etiqueta leída esta BLOQUEADA" }, JsonRequestBehavior.DenyGet);
                                
                                case "T": //Estatus de estiqueta 
                                    return Json(new { codigo = -2, mensaje = "La etiqueta leída FUE USADA PARA TRASPALEO" }, JsonRequestBehavior.DenyGet);
                                
                                case null:
                                case "":
                                    List<Etiqueta> Lotes = new List<Etiqueta>();
                                    if (Ob.Lotes(QR, ref Lotes))
                                    {
                                        if (Lotes.Count > 0)
                                            return Json(new { codigo = 0, mensaje = "" }, JsonRequestBehavior.DenyGet);
                                        else
                                            return Json(new { codigo = -2, mensaje = "La etiqueta leída NO EXISTE" }, JsonRequestBehavior.DenyGet);
                                    }
                                    else
                                        return Json(new { codigo = -1, mensaje = "Error al obtener información de la etiqueta" }, JsonRequestBehavior.DenyGet);
                                
                                default:
                                    return Json(new { codigo = -1, mensaje = "La etiqueta existe pero NO HA SIDO LIBERADA" }, JsonRequestBehavior.DenyGet);
                            }
                        }
                    }
                    return Json(new { codigo = 0, mensaje = Lista }, JsonRequestBehavior.DenyGet);
                }
            }
            else
            {
                return Json(new { codigo = -1, mensaje = StEti }, JsonRequestBehavior.DenyGet);
            }
        }

        [HttpPost]
        public ActionResult ObtenerLote(string Lectura, string Tipo)
        {
            List<Etiqueta> Registros = new List<Etiqueta>();
            bool Procede = false;
            string MsgError = string.Empty;

            switch (Tipo)
            {
                case "Atm":
                    if (Ob.Lotes(Lectura, ref Registros))
                    {
                        Procede = true;                      
                    }
                    else {
                        MsgError = "Error al obtener información de la etiqueta";
                    }                        
                    break;

                case "Manual":
                    if (Ob.ConsultaIL(Lectura, ref Registros))
                    {
                        Procede = true;
                    }
                    else
                    {
                        MsgError = "Error al obtener información del código ingresado";
                    }
                    break ;
            }

            if (Procede)
            {
                switch (Registros.Count)
                {
                    case 0:
                        return Json(new { codigo = -1, mensaje = "No se encontró informacion" }, JsonRequestBehavior.AllowGet);

                    default:
                        return Json(new { codigo = 0, mensaje = Registros }, JsonRequestBehavior.AllowGet);
                }
            }
            else 
            {
                return Json(new { codigo = -2, mensaje = MsgError }, JsonRequestBehavior.AllowGet);
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
        }

    }
}