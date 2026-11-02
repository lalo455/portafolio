using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WMS.Logic.Comun;
using WMS.Logic.Stores;
using WMS.Models;
using static WMS.Models.Produccion;

namespace WMS.Controllers
{
    public class ScrapController : Controller
    {
        StoreProcedure SP;
        Obtener Ob;
        Integrar Crear;
        Utilities.Log Logs;

        public ScrapController()
        {
            SP = new StoreProcedure();
            Ob = new Obtener();
            Crear = new Integrar();
            Logs = new Utilities.Log();
        }

        // GET: Scrap
        public ActionResult RegistroScrap()
        {
            return View();
        }

        public ActionResult EdicionScrap()
        {
            return View();
        }

        //***************************************************
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

        //*************************************************
        [HttpGet]
        public ActionResult Turnos()
        {
            List<Turnos> Turnos = new List<Turnos>();
            switch (SP.Turnos(ref Turnos))
            {
                case true:
                    if (Turnos.Count > 0)
                    {
                        return Json(new { codigo = 0, mensaje = Turnos, turno = int.Parse(Ob.ObtenerTurno()) }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { codigo = -1, mensaje = "No se recuperaron registros de turnos" }, JsonRequestBehavior.AllowGet);
                    }
                        
                case false:
                default:
                    return Json(new { codigo = -2, mensaje = "Error al recuperar registros de los turnos" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult HorasTurno(string Turno)
        {
            string HrIni = string.Empty;
            string HrFin = string.Empty;
            string msgError = string.Empty;

            switch (SP.HrTurno(ref HrIni, ref HrFin, ref msgError, Turno))
            {
                case true:
                    if (HrIni != "" && HrFin != "")
                    {
                        return Json(new { codigo = 0, mensaje = "", HoraIni = HrIni, HoraFin = HrFin }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        if (HrIni == "")
                        {
                            return Json(new { codigo = -1, mensaje = "No se recuperó Hora Inicial del Turno " + Turno + ", verificar" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { codigo = -1, mensaje = "No se recuperó Hora Final del Turno " + Turno + ", verificar" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                case false:
                default:
                    return Json(new { codigo = -2, mensaje = "Error al recuperar Horarios del Turno " + Turno + ", " + msgError }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ListaOperadores()
        {
            List<Operadores> Operadores = new List<Operadores>();
            switch (SP.Operadores(ref Operadores))
            {
                case true:
                    if (Operadores.Count > 0)
                    {
                        return Json(new { codigo = 0, mensaje = Operadores }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { codigo = -1, mensaje = "No se recuperaron registros de operadores" }, JsonRequestBehavior.AllowGet);
                    }

                case false:
                default:
                    return Json(new { codigo = -2, mensaje = "Error al recuperar registros de los operadores" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ValidaOF(string NumOF)
        {
            List<Scrap.Comun> RespOF = new List<Scrap.Comun>();

            switch (Ob.ValidaOF(ref RespOF, NumOF))
            {
                case true:
                    if (RespOF.Count > 0)
                    {
                        return Json(new { codigo = 0, mensaje = RespOF }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { codigo = -1 }, JsonRequestBehavior.AllowGet);
                    }

                case false:
                default:
                    return Json(new { codigo = -2 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Maquinas()
        {
            List<Scrap.Comun> Maquinas = new List<Scrap.Comun>();

            switch (Ob.Maquinas(ref Maquinas))
            {
                case true:
                    if (Maquinas.Count > 0)
                    {
                        return Json(new { codigo = 0, mensaje = Maquinas }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { codigo = -1, mensaje = "No se recuperaron registros de Maquinas" }, JsonRequestBehavior.AllowGet);
                    }                        
                
                case false:
                default:
                    return Json(new { codigo = -2, mensaje = "Error al recuperar registros de los Maquinas" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ValidaMaquina(string Maquina)
        {
            List<Scrap.Comun> RespMaquina = new List<Scrap.Comun>();

            switch (Ob.ValidaMaq(ref RespMaquina, Maquina))
            {
                case true:
                    if (RespMaquina.Count > 0)
                    {
                        return Json(new { codigo = 0, mensaje = RespMaquina }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { codigo = -1 }, JsonRequestBehavior.AllowGet);
                    }

                case false:
                default:
                    return Json(new { codigo = -2 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult TipoScrap()
        {
            List<Scrap.Comun> ListaScrap = new List<Scrap.Comun>();

            switch (Ob.TiposScrap(ref ListaScrap))
            {
                case true:
                    if (ListaScrap.Count > 0)
                    {
                        return Json(new { codigo = 0, mensaje = ListaScrap }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { codigo = -1, mensaje = "No se recuperaron registros de Tipos de Scrap" }, JsonRequestBehavior.AllowGet);
                    }

                case false:
                default:
                    return Json(new { codigo = -2, mensaje = "Error al recuperar registros de Tipos de Scrap" }, JsonRequestBehavior.AllowGet);
            }
        }

        //**************************************************
        [HttpPost]
        public ActionResult CrearScrap(List<Scrap.Detalle> Lineas)
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();
            
            Logs.FullInfo("Modulo: Scrap - Inicio", "", Usuario, 1, "", ""); //Log Informativo
            try
            {                
                Logs.FullInfo("Información a Procesar", "", Usuario, 4, "", ""); //Log Informativo
                Mensajes msj = new Mensajes();
                Guid g;
                g = Guid.NewGuid();
                if (SP.GuardarRegistrosScrap(Lineas, g.ToString())) //Guarda Registro en Tabla Movimiento
                {
                    //Integración en SAP              
                    if (Crear.RegistroScrap(ref msj, g.ToString()))
                    {
                        Logs.FullInfo("", "Integración: Scrap registrado", Usuario, 0, modulo, metodo); //Log Informativo
                        return Json(new { codigo = 0, mensaje = "Datos de Scrap registrados correctamente" }, JsonRequestBehavior.DenyGet); //Muestra Documento Creado
                    }
                    else
                    {
                        Logs.Registrar("Integración: Scrap registrado NO Creado", msj.Mensaje, modulo, metodo);
                        Logs.FullInfo("", "Integración: Scrap registrado NO Creado: " + msj.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo
                        return Json(new { codigo = -1, mensaje = "Error al registrar datos de Scrap, " + msj.Mensaje }, JsonRequestBehavior.DenyGet); //No se genero Documento
                    }
                }
                else
                {
                    Logs.FullInfo("", "Error al guardar los registros de scrap", Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = -1, mensaje = "Error al guardar los registros de scrap" }, JsonRequestBehavior.DenyGet);
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar("Registro de Scrap NO Creado", ex.Message, modulo, metodo);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = "Error en el registro de Scrap" }, JsonRequestBehavior.DenyGet);
            }
            finally
            {
                Logs.FullInfo("Modulo: Scrap - Fin", "", Usuario, 2, "", ""); //Log Informativo
            }
        }

        //*****************************************************
        [HttpGet]
        public ActionResult ListaRegistros(string NumOF)
        {
            List<Scrap.DetalleRegistros> Registros = new List<Scrap.DetalleRegistros>();

            switch (Ob.ListaRegScrap(ref Registros, NumOF))
            {
                case true:
                    if (Registros.Count > 0)
                    {
                        return Json(new { codigo = 0, mensaje = Registros }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { codigo = -1, mensaje = "No se recuperaron registros de la Orden " + NumOF }, JsonRequestBehavior.AllowGet);
                    }

                case false:
                default:
                    return Json(new { codigo = -2, mensaje = "Error al recuperar registros de la Orden " + NumOF }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EliminarScrap(List<Scrap.Detalle> Lineas)
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();

            Logs.FullInfo("Modulo: Editar Scrap - Inicio", "", Usuario, 1, "", ""); //Log Informativo
            try
            {
                Logs.FullInfo("Información a Procesar", "", Usuario, 4, "", ""); //Log Informativo
                Mensajes msj = new Mensajes();
                Guid g;
                g = Guid.NewGuid();
                if (SP.GuardarRegistrosEliminarScrap(Lineas, g.ToString())) //Guarda Registro en Tabla Movimiento
                {
                    //Integración en SAP                 
                    if (Crear.EliminarRegistroScrap(ref msj, g.ToString()))
                    {
                        Logs.FullInfo("", "Integración: Scrap eliminado", Usuario, 0, modulo, metodo); //Log Informativo
                        return Json(new { codigo = 0, mensaje = "Datos de Scrap eliminados correctamente" }, JsonRequestBehavior.DenyGet); //Muestra Documento Creado
                    }
                    else
                    {
                        Logs.Registrar("Integración: Scrap eliminado NO Creado", msj.Mensaje, modulo, metodo);
                        Logs.FullInfo("", "Integración: Scrap eliminado NO Creado: " + msj.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo
                        return Json(new { codigo = -1, mensaje = "Error al eliminar datos de Scrap, " + msj.Mensaje }, JsonRequestBehavior.DenyGet); //No se genero Documento
                    }
                    
                }
                else
                {
                    Logs.FullInfo("", "Error al guardar los registros de scrap", Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = -1, mensaje = "Error al guardar los registros de scrap" }, JsonRequestBehavior.DenyGet);
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar("Registro de Scrap NO Editado", ex.Message, modulo, metodo);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = "Error al editar Scrap" }, JsonRequestBehavior.DenyGet);
            }
            finally
            {
                Logs.FullInfo("Modulo: Editar Scrap - Fin", "", Usuario, 2, "", ""); //Log Informativo
            }
        }

        //*****************************************************










    }
}