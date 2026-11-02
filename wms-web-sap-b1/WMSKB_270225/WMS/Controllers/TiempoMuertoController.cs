using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using WMS.Logic.Comun;
using WMS.Logic.Stores;
using WMS.Models;
using WMS.Utilities;
using static WMS.Models.Produccion;

namespace WMS.Controllers
{
    public class TiempoMuertoController : Controller
    {
        StoreProcedure SP;
        Obtener Ob;
        Integrar Crear;
        Utilities.Log Logs;

        public TiempoMuertoController()
        {
            SP = new StoreProcedure();
            Ob = new Obtener();
            Crear = new Integrar();
            Logs = new Utilities.Log();
        }

        // GET: TiempoMuerto
        public ActionResult RegistroTM()
        {
            return View();
        }

        public ActionResult EdicionTM()
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
            List<TPM.Comun> RespOF = new List<TPM.Comun>();

            switch (Ob.ValidaOFTPM(ref RespOF, NumOF))
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
            List<TPM.Comun> Maquinas = new List<TPM.Comun>();

            switch (Ob.MaquinasTPM(ref Maquinas))
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
            List<TPM.Comun> RespMaquina = new List<TPM.Comun>();

            switch (Ob.ValidaMaqTPM(ref RespMaquina, Maquina))
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
        public ActionResult TipoTPM()
        {
            List<TPM.Comun> ListaTPM = new List<TPM.Comun>();

            switch (Ob.TiposTPM(ref ListaTPM))
            {
                case true:
                    if (ListaTPM.Count > 0)
                    {
                        return Json(new { codigo = 0, mensaje = ListaTPM }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { codigo = -1, mensaje = "No se recuperaron registros de Tipos de TPM" }, JsonRequestBehavior.AllowGet);
                    }

                case false:
                default:
                    return Json(new { codigo = -2, mensaje = "Error al recuperar registros de Tipos de TPM" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult RegistrosTPM(string DocEntry, string Maquina, int Turno, string Fecha)
        {
            List<TPM.DetalleRegistros> ListTPM = new List<TPM.DetalleRegistros>();

            switch (Ob.ListaTPMOF(ref ListTPM, DocEntry, Maquina, Turno, Fecha))
            {
                case true:
                    if (ListTPM.Count > 0)
                    {
                        return Json(new { codigo = 0, mensaje = ListTPM }, JsonRequestBehavior.AllowGet);
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

        //**************************************************
        [HttpPost]
        public ActionResult CrearTPM(List<TPM.Detalle> Lineas)
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();

            Logs.FullInfo("Modulo: TPM - Inicio", "", Usuario, 1, "", ""); //Log Informativo
            try
            {
                Logs.FullInfo("Información a Procesar", "", Usuario, 4, "", ""); //Log Informativo
                Mensajes msj = new Mensajes();
                Guid g;
                g = Guid.NewGuid();
                if (SP.GuardarRegistrosTPM(Lineas, g.ToString())) //Guarda Registro en Tabla Movimiento
                {
                    //Integración en SAP              
                    if (Crear.RegistroTPM(ref msj, g.ToString()))
                    {
                        Logs.FullInfo("", "Integración: TPM registrado", Usuario, 0, modulo, metodo); //Log Informativo

                        return Json(new { codigo = 0, mensaje = "Datos de TPM registrados correctamente" }, JsonRequestBehavior.DenyGet); //Muestra Documento Creado
                    }
                    else
                    {
                        Logs.Registrar("Integración: TPM registrado NO Creado", msj.Mensaje, modulo, metodo);
                        Logs.FullInfo("", "Integración: TPM registrado NO Creado: " + msj.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo
                        return Json(new { codigo = -1, mensaje = "Error al registrar datos de TPM, " + msj.Mensaje }, JsonRequestBehavior.DenyGet); //No se genero Documento
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
                Logs.Registrar("Registro de TPM NO Creado", ex.Message, modulo, metodo);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = "Error en el registro de TPM" }, JsonRequestBehavior.DenyGet);
            }
            finally
            {
                Logs.FullInfo("Modulo: TPM - Fin", "", Usuario, 2, "", ""); //Log Informativo
            }
        }
        
        //*****************************************************
        [HttpGet]
        public ActionResult ListaRegistros(string NumOF)
        {
            List<TPM.DetalleRegistros> Registros = new List<TPM.DetalleRegistros>();

            switch (Ob.ListaRegTPM(ref Registros, NumOF))
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
        public ActionResult EliminarTPM(List<TPM.Detalle> Lineas)
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();

            Logs.FullInfo("Modulo: Editar TPM - Inicio", "", Usuario, 1, "", ""); //Log Informativo
            try
            {
                Logs.FullInfo("Información a Procesar", "", Usuario, 4, "", ""); //Log Informativo
                Mensajes msj = new Mensajes();
                Guid g;
                g = Guid.NewGuid();
                if (SP.GuardarRegistrosEliminarTPM(Lineas, g.ToString())) //Guarda Registro en Tabla Movimiento
                {
                    //Integración en SAP                 
                    if (Crear.EliminarRegistroTPM(ref msj, g.ToString()))
                    {
                        Logs.FullInfo("", "Integración: TPM eliminado", Usuario, 0, modulo, metodo); //Log Informativo
                        return Json(new { codigo = 0, mensaje = "Datos de TPM eliminados correctamente" }, JsonRequestBehavior.DenyGet); //Muestra Documento Creado
                    }
                    else
                    {
                        Logs.Registrar("Integración: TPM eliminado NO Creado", msj.Mensaje, modulo, metodo);
                        Logs.FullInfo("", "Integración: TPM eliminado NO Creado: " + msj.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo
                        return Json(new { codigo = -1, mensaje = "Error al eliminar datos de TPM, " + msj.Mensaje }, JsonRequestBehavior.DenyGet); //No se genero Documento
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
                Logs.Registrar("Registro de TPM NO Editado", ex.Message, modulo, metodo);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = "Error al editar TPM" }, JsonRequestBehavior.DenyGet);
            }
            finally
            {
                Logs.FullInfo("Modulo: Editar TPM - Fin", "", Usuario, 2, "", ""); //Log Informativo
            }
        }

        //*****************************************************

    }
}