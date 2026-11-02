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
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Login", "Login");
            }
        }
        [HttpPost]
        public ActionResult ObtenerDocumentos(string Param, int Param2) {
            List<Produccion.OrdenFabricacion> ListaOF = new List<Produccion.OrdenFabricacion>();
            List<Produccion.DetalleOrden> LinOF = new List<Produccion.DetalleOrden>();
            List<Produccion.DetalleSolicitud> LinSolicitud = new List<Produccion.DetalleSolicitud>();
            switch (Param)
            {
                case "OF":
                    if (SP.ObtenerLists(ref ListaOF, Param, -1))
                        return Json(new { codigo = 0, mensaje = ListaOF }, JsonRequestBehavior.DenyGet);
                    else
                        return Json(new { codigo = -1, mensaje = "Error al recuperar las ordenes de fabricación"}, JsonRequestBehavior.DenyGet);
                case "DO":
                    if (SP.ObtenerLists(ref LinOF, Param, Param2))
                        return Json(new { codigo = 0, mensaje = LinOF }, JsonRequestBehavior.DenyGet);
                    else
                        return Json(new { codigo = -1, mensaje = "No se recuperaron lineas para la Orden: "}, JsonRequestBehavior.DenyGet);
                case "SO":
                    if (SP.ObtenerLists(ref LinSolicitud, Param, Param2))
                    {
                        if (LinSolicitud.Count > 0) 
                            return Json(new { codigo = 0, mensaje = LinSolicitud }, JsonRequestBehavior.DenyGet);
                        else
                            return Json(new { codigo = -1, mensaje = "No se recuperaron articulos para la solicitud" }, JsonRequestBehavior.DenyGet);
                    }
                    else
                        return Json(new { codigo = -1, mensaje = "No se recuperó informacion para la solicitud: " }, JsonRequestBehavior.DenyGet);
                //default:
                //    break;
            }
            return Json(new { codigo = -1, mensaje = "Error durante el proceso"}, JsonRequestBehavior.DenyGet);
        }
        [HttpPost]
        public ActionResult Comparar(int NOrden, int NSolicitud) {
            List<Produccion.Comparacion> Comparacion = new List<Produccion.Comparacion>();
            if (SP.Comparacion(ref Comparacion, NOrden, NSolicitud))
            {
                foreach (Produccion.Comparacion C in Comparacion) {
                    if (C.Coinciden == "No")
                        return Json(new { codigo = -1, mensaje = $"Los artículos de la orden: { NOrden } con la solicitud: { NSolicitud } NO COINCIDEN"}, JsonRequestBehavior.DenyGet);
                    if (C.Coinciden == "-1")
                        return Json(new { codigo = -1, mensaje = $"La solicitud: { NSolicitud } no contiene el mismo número de artículos"}, JsonRequestBehavior.DenyGet);
                }
                return Json(new { codigo = 0, mensaje = "Documento cargado exitosamente" });
            }
            else
                return Json(new { codigo = -1, mensaje = $"Error al comparar la Orden de Fabricación: { NOrden } con la Solicitud: { NSolicitud }" }, JsonRequestBehavior.DenyGet);
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
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult TraspasoAProduccion(Produccion.Documento Lineas, Produccion.Datos IAdicional) {
            Mensajes msj = new Mensajes();
            Guid g;
            g = Guid.NewGuid();
            if (SP.GuardarRegistrosProduccion(Lineas, IAdicional, g.ToString()))
            {
                if (Crear.TraspasoAProduccion(ref msj, IAdicional, Lineas.NumST, Lineas.NumOrdr, g.ToString()))
                    return Json(new { codigo = 0, mensaje = msj.DocNum}, JsonRequestBehavior.DenyGet);
                else
                    return Json(new { codigo = -1, mensaje = msj.Mensaje }, JsonRequestBehavior.DenyGet);
                //return Json(new { codigo = 0, mensaje = "Pruebas"});
            }
            else
                return Json(new { codigo = -1, mensaje = "Error al guardar los registros"});

        }
        //[HttpPost]
        //public ActionResult DetalleOrden(string Orden)
        //{
        //    //List<Produccion.EstatusOrden> Lineas = new List<Produccion.EstatusOrden>();
        //    //if (Ob.ObtenerDetalleOrden(ref Lineas, Orden))
        //    //    if (Lineas.Count > 0)
        //    //        return Json(new { codigo = 0, mensaje = Lineas }, JsonRequestBehavior.DenyGet);
        //    //    else
        //    //        return Json(new { codigo = -1, mensaje = "No se encontro información del documento: " + Orden }, JsonRequestBehavior.DenyGet);
        //    //else
        //    //    return Json(new { codigo = -1, mensaje = "No se encontro información del documento: " + Orden }, JsonRequestBehavior.DenyGet);
        //}
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
        [HttpPost]
        public ActionResult ActualizarOrden(List<Produccion.ValidacionProd> ListaAct) {
            string msj = string.Empty;
            ClaimsPrincipal iden = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string usr = iden.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault() ;

            if (Ob.ActualizarDtsOrden(ref msj, ListaAct, usr))
                return Json(new { codigo = 0, mensaje = "Actualizacion realizada correctamente" }, JsonRequestBehavior.DenyGet);
            else
                return Json(new { codigo = -1, mensaje = msj }, JsonRequestBehavior.DenyGet);
        }
        [HttpPost]
        public ActionResult ObtenerBCode(string BCode, string Warehouse) {
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
            if (!Lote.StartsWith(ConfigurationManager.AppSettings["TRL"])) //Si la etiqueta es de Traspaleo T NO SE PROCESA - IUO 6/11/2020
            {
                //Si la etiqueta es diferente de T00000 se puede leer y procesar
                if (Ob.EstadoEtiqueta(ref valor, Lote.Split('&')[0]))
                {
                    if (Lote.StartsWith(ConfigurationManager.AppSettings["TRLU"])) //Si la etiqueta es de Traspaleo U se Procesa - Nuevo Cambio Traspaleo etiquetas U - IUO 6/11/2020
                    {
                        switch (valor)
                        {
                            case "L":
                                return Json(new { codigo = 0, mensaje = "" }, JsonRequestBehavior.DenyGet);
                            default:
                                return Json(new { codigo = -2, mensaje = "La etiqueta de traspaleo no tiene el estatus de Liberado" }, JsonRequestBehavior.DenyGet);
                        }
                    }
                    else
                    {
                        switch (valor)
                        {
                            case "R":
                                return Json(new { codigo = -2, mensaje = "La etiqueta Ya ha sido REGISTRADA" }, JsonRequestBehavior.DenyGet);
                            case "G":
                                return Json(new { codigo = 0, mensaje = "" }, JsonRequestBehavior.DenyGet);
                            case "L":
                                return Json(new { codigo = -2, mensaje = "La etiqueta YA FUE LIBERADA" }, JsonRequestBehavior.DenyGet);
                            case "E":
                                return Json(new { codigo = -2, mensaje = "Las etiquetas embarcadas NO estan PERMITIDAS" }, JsonRequestBehavior.DenyGet);
                            default:
                                return Json(new { codigo = -1, mensaje = "El estatus de la etiqueta no pudo ser verificada" }, JsonRequestBehavior.DenyGet);
                        }
                    }
                }
                else
                    return Json(new { codigo = -1, mensaje = "Error al obtener estado de la etiqueta" }, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { codigo = -2, mensaje = "La etiqueta NO es permitida" }, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public ActionResult CrearEmisionRecibo(List<Produccion.EmisionRecibo> Lineas, int Turno, string ToWhsCode, string FechaUsuario) {
            string result = string.Empty, msjSbo = string.Empty, resultT = string.Empty;
            List<MsjProduccion> mensajes = new List<MsjProduccion>();
            string lotesTT = string.Empty;
            Mensajes mensaje = new Mensajes();
            MsjRecibo msjTraspaleo = new MsjRecibo();
            Guid g;
            g = Guid.NewGuid();
            IDbConnection dbCon = null;
            Company sbo = null;
            try
            {
                dbCon = Ob.conSql(); //Conexion a la BD
                //Procesar Etiquetas - Debe haber etiquetas de PT
                if (SP.GuardarProduccion(Lineas.Where(x => x.DistNumber.StartsWith("L")).ToList(), g.ToString(), ref result, dbCon))
                {
                    sbo = Crear.sboConect(ref msjSbo); //Conexion a SAP
                    if (sbo != null && msjSbo.Length == 0)
                    {
                        //Emision y Recibo de Produccion
                        foreach (Produccion.DocumentoAProduccion produccion in Ob.ObtenerDocumentoAProduccion(dbCon, g.ToString(), "LOF", 0))
                        {
                            List<Produccion.ConsumoOrdenFabricacion> detalleOf = Ob.ObtenerConsumoPorOrden(dbCon, g.ToString(), produccion.DocNum, produccion.Quantity);
                            for (int d = 0; d < detalleOf.Count; d++)
                            {
                                if (detalleOf[d].ItemType == 4)
                                    detalleOf[d].Batch = Ob.ObtenerLotesAConsumir(dbCon, g.ToString(), detalleOf[d].ItemCode, detalleOf[d].Warehouse, produccion.Quantity, /*double.Parse(detalleOf[d].CantReq.ToString())*/ produccion.DocNum);
                                else
                                    detalleOf[d].Batch = new List<Produccion.LoteConsumo>();
                            }
                            Crear.Aproduccion(sbo, ref mensaje, ref mensajes, produccion, detalleOf, g.ToString(), Ob.ObtenerDocumentoAProduccion(dbCon, g.ToString(), "EPM", produccion.DocNum), Turno, Ob.ObtenerMaquina(produccion.DocNum, dbCon), FechaUsuario);
                        }
                        //SP.RemoverRegistros("[Produccion]", g.ToString());
                        //Traspaso de etiquetas tipo Traspaleo - para recibos de produccion solo procesar etiquetas tipo U - cambio IUO 6/11/2020
                        //Listar las etiquetas de Traspaleo U00000 
                        List<Produccion.EmisionRecibo> EtqsTraspaleo = Lineas.Where(t => t.DistNumber.StartsWith(ConfigurationManager.AppSettings["TRLU"])).ToList();
                        List<Produccion.EmisionRecibo> nuevLista = new List<Produccion.EmisionRecibo>();
                        if (EtqsTraspaleo.Count > 0)
                        {
                            int inserts = 0;
                            string msjSql = string.Empty;
                            if (EtqsTraspaleo.Count > 0) {
                                //Se registran las etiquetas de Traspaleo en tabla Movimiento
                                if (SP.GuardarRegistrosTraspaleo(dbCon, EtqsTraspaleo, ToWhsCode, g.ToString(), ref inserts, ref msjSql)) 
                                {
                                    //Generar el Traspaso en SAP
                                    if (!Crear.TraspasoTraspaleo(ref msjTraspaleo, ref lotesTT, sbo, g.ToString()))
                                        //No pudo generarse el traspaso, se registra en LOG
                                        Logs.Escribir(String.Join(",", EtqsTraspaleo.Select(q => q.DistNumber).FirstOrDefault()), g.ToString(), ToWhsCode, "TTSAP", msjTraspaleo.Mensaje);
                                }
                                else //No pudieron guardarse en la tabla de trabajo
                                    Logs.Escribir(String.Join(",", EtqsTraspaleo.Select(q => q.DistNumber).FirstOrDefault()), g.ToString(), ToWhsCode, "Movimiento", msjSql); 
                            }
                        }
                        //SP.BorradorControl(EtqsTraspaleo, FiltrarListaProd(Lineas.Where(x => x.DistNumber.StartsWith("L")).ToList(), mensajes), mensajes);
                        return Json(new { codigo = 0, mensaje, documentos = mensajes, traspaleo = lotesTT }, JsonRequestBehavior.DenyGet);
                    }
                    else
                        return Json(new { codigo = -1, mensaje = msjSbo }, JsonRequestBehavior.DenyGet);
                }
                //Traspaso SOLO de etiquetas tipo Traspaleo - U
                /*else if (SP.GuardarProduccion(Lineas.Where(x => x.DistNumber.StartsWith("U")).ToList(), g.ToString(), ref result, dbCon))
                {
                    sbo = Crear.sboConect(ref msjSbo);
                    if (sbo != null && msjSbo.Length == 0)
                    {
                        //SP.RemoverRegistros("[Produccion]", g.ToString());
                        //Traspaso de etiquetas tipo Traspaleo
                        List<Produccion.EmisionRecibo> EtqsTraspaleo = Lineas.Where(t => t.DistNumber.StartsWith(ConfigurationManager.AppSettings["TRLU"])).ToList();
                        List<Produccion.EmisionRecibo> nuevLista = new List<Produccion.EmisionRecibo>();
                        if (EtqsTraspaleo.Count > 0)
                        {
                            int inserts = 0;
                            string msjSql = string.Empty;
                            if (EtqsTraspaleo.Count > 0)
                            {
                                if (SP.GuardarRegistrosTraspaleo(dbCon, EtqsTraspaleo, ToWhsCode, g.ToString(), ref inserts, ref msjSql))
                                {
                                    if (!Crear.TraspasoTraspaleo(ref msjTraspaleo, ref lotesTT, sbo, g.ToString()))//No pudo generarse el traspaso
                                    {
                                        Logs.Escribir(String.Join(",", EtqsTraspaleo.Select(q => q.DistNumber).FirstOrDefault()), g.ToString(), ToWhsCode, "TTSAP", msjTraspaleo.Mensaje);
                                    }                                        
                                    else {
                                        Logs.Escribir(String.Join(",", EtqsTraspaleo.Select(q => q.DistNumber).FirstOrDefault()), g.ToString(), ToWhsCode, "TTSAP", msjTraspaleo.Mensaje);
                                        return Json(new { codigo = 0, mensaje, documentos = mensajes, traspaleo = lotesTT }, JsonRequestBehavior.DenyGet);
                                    }
                                }
                                else //No pudieron guardarse en la tabla de trabajo
                                    Logs.Escribir(String.Join(",", EtqsTraspaleo.Select(q => q.DistNumber).FirstOrDefault()), g.ToString(), ToWhsCode, "Movimiento", msjSql);
                            }
                        }
                        //SP.BorradorControl(EtqsTraspaleo, FiltrarListaProd(Lineas.Where(x => x.DistNumber.StartsWith("L")).ToList(), mensajes), mensajes);
                        return Json(new { codigo = 0, mensaje, documentos = mensajes, traspaleo = lotesTT }, JsonRequestBehavior.DenyGet);
                    }
                    else
                        return Json(new { codigo = -1, mensaje = msjSbo }, JsonRequestBehavior.DenyGet);
                }*/
                else
                    return Json(new { codigo = -1, mensaje = result }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return Json(new { codigo = -1, mensaje = ex.Message }, JsonRequestBehavior.DenyGet);
            }
            finally
            {
                if (dbCon != null)
                {
                    if (dbCon.State == ConnectionState.Open) {
                        dbCon.Close();
                        dbCon.Dispose();
                    }
                }
                if (sbo != null)
                {
                    if (sbo.Connected)
                        sbo.Disconnect();
                    Marshal.ReleaseComObject(sbo);
                }
            }

        }
        private List<Produccion.EmisionRecibo> FiltrarListaTraspaleo(List<Produccion.EmisionRecibo> lineas, List<MsjProduccion> mensajes) {
            try
            {
                List<Produccion.EmisionRecibo> nuevLista = new List<Produccion.EmisionRecibo>();
                foreach (Produccion.EmisionRecibo val in lineas) {
                    foreach (MsjProduccion men in mensajes) {
                        //if (val.OF == men.OF && men.Integrado == true)
                        if (men.Integrado == true)
                            nuevLista.Add(val);
                    }
                }
                return nuevLista;
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return null;
            }
        }
        private List<Produccion.EmisionRecibo> FiltrarListaProd(List<Produccion.EmisionRecibo> lineas, List<MsjProduccion> mensajes)
        {
            try
            {
                List<Produccion.EmisionRecibo> nuevLista = new List<Produccion.EmisionRecibo>();
                foreach (Produccion.EmisionRecibo val in lineas)
                {
                    foreach (MsjProduccion men in mensajes)
                    {
                        if (val.OF == men.OF && men.Integrado == true)
                            nuevLista.Add(val);
                    }
                }
                return nuevLista;
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, this.GetType().FullName);
                return null;
            }
        }
        [HttpGet]
        public ActionResult Operadores() {
            List<Produccion.Operadores> Operadores = new List<Produccion.Operadores>();
            switch (SP.Operadores(ref Operadores))
            {
                case true:
                    if (Operadores.Count > 0)
                        return Json(new { codigo = 0, mensaje = Operadores }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { codigo = -1, mensaje = "No se recuperaron registros de operadores"}, JsonRequestBehavior.AllowGet);
                case false:
                default:
                    return Json(new { codigo = -1, mensaje = "Error al recuperar los registros de operadores" }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult Turnos() {
            List<Produccion.Turnos> Turnos = new List<Produccion.Turnos>();
           
            switch (SP.Turnos(ref Turnos))
            {
                case true:
                    if (Turnos.Count > 0) {
                        return Json(new { codigo = 0, mensaje = Turnos, turno = int.Parse(Ob.ObtenerTurno()) /*turnoDefault(Turnos)*/ }, JsonRequestBehavior.AllowGet); 
                    }
                    else
                        return Json(new { codigo = -1, mensaje = "No se recuperaron registros de turnos" }, JsonRequestBehavior.AllowGet);
                case false:
                default:
                    return Json(new { codigo = -1, mensaje = "No se pudierón recuperar registros de los turnos" }, JsonRequestBehavior.AllowGet);
            }
        }
        public int turnoDefault(List<Produccion.Turnos> listTurnos) {
            // si t1 es menor que t2, el resultado es menor que cero
            // si t1 es igual a t2, entonces el resultado es cero
            // si t1 es mayor que t2, el resultado es mayor cero
            int Turno = 0;
            DateTime sistema = DateTime.Now;
            for (int d = 0; d < listTurnos.Count; d++) {
                DateTime inicio = Convert.ToDateTime(listTurnos[d].U_Inicio);
                DateTime fin = Convert.ToDateTime(listTurnos[d].U_Fin);
                int i = DateTime.Compare(sistema, inicio);
                int f = DateTime.Compare(sistema, fin);

                if (i > 0 && f < 0) {//En el rango es de inicio y fin
                    Turno = listTurnos[d].Code;
                    break;
                }
            }
            return Turno;
        }
        [HttpPost]
        public ActionResult Fifo(string Qr, List<Etiqueta> lineas) {
            //int i = -1;
            List<Fifo> listaF = new List<Fifo>();
            if (lineas == null) 
                lineas = new List<Etiqueta>();

            if (Ob.obtenerFIFOLote(ref listaF, Qr.Split('&')[0], Qr.Split('&')[1], lineas))
            {
                if (listaF.Count == 1 || listaF.Count == 0)
                    return Json(new { codigo = 0, mensaje = true }, JsonRequestBehavior.DenyGet);
                else 
                    return Json(new { codio = -1, mensaje = $"Existe el lote {listaF[0].DistNumber} con fecha: { listaF[0].InDate }"}, JsonRequestBehavior.DenyGet);
                //if (i == 1)
                //    return Json(new { codigo = 0, mensaje = true }, JsonRequestBehavior.DenyGet);
                //else
                //    return Json(new { codio = -1, mensaje = "Existe un lote con una fecha más antigua"}, JsonRequestBehavior.DenyGet);
            }
            else
                return Json(new { codigo = -1, mensaje ="Error al obtener lote por FIFO" }, JsonRequestBehavior.DenyGet);
        }
        [HttpGet]
        public ActionResult ArmarUbicacion(string WhsCode, string Type) {
            List<Produccion.SubAlmacen> ListaSL1 = new List<Produccion.SubAlmacen>();
            List<Produccion.Linea> ListaSL2 = new List<Produccion.Linea>();
            List<Produccion.Fila> ListaSL3 = new List<Produccion.Fila>();

            if (Ob.ObtenerNivelesUbicacion(ref ListaSL1, ref ListaSL2, ref ListaSL3, WhsCode, Type))
                return Json(new { codigo = 0, ListaSL1, ListaSL2, ListaSL3}, JsonRequestBehavior.AllowGet);
            else
                return Json(new { codigo = -1, mensaje = "" }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GuardarBorrador(List<Produccion.EmisionRecibo> Lineas, int Turno, string guid, string almacenSel) {
            string response = string.Empty;
            if (Lineas == null) 
                return Json(new { codigo = -1, mensaje = $"Para guardar un borrador debe leer al menos una etiqueta" }, JsonRequestBehavior.DenyGet);
            if (Lineas.Count == 0)
                return Json(new { codigo = -1, mensaje = $"Para guardar un borrador debe leer al menos una etiqueta" }, JsonRequestBehavior.DenyGet);

            if (SP.GuardarBorrador(Lineas, Turno, guid, almacenSel, ref response))
                return Json(new { codigo = 0, mensaje = "Borrador guardado" }, JsonRequestBehavior.DenyGet);
            else
                return Json(new { codigo = -1, mensaje = $"Error: {response}" }, JsonRequestBehavior.DenyGet);
        }
        [HttpGet]
        public ActionResult RecuperarBorrador(string type, int order, int turno) {
            switch (type)
            {
                case "L"://Listar Borradores del usuario
                    List<Produccion.Borrador> listaBorrador = new List<Produccion.Borrador>();
                    if (Ob.ListaBorradores(turno, ref listaBorrador))
                    {
                        if(listaBorrador.Count > 0)
                            return Json(new { codigo = 0, mensaje = listaBorrador }, JsonRequestBehavior.AllowGet);
                        else
                            return Json(new { codigo = -1, mensaje = $"No existen registros guardados" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                        return Json(new { codigo = -1, mensaje = $"Error al recuperar la lista de borredaros" }, JsonRequestBehavior.AllowGet);
                case "R"://Recuperar Borrador seleccionado
                    List<Produccion.EmisionRecibo> Lineas = new List<Produccion.EmisionRecibo>();
                    string almacenSel = string.Empty;
                    if (Ob.RecuperarBorrador(order, turno, ref Lineas, ref almacenSel))
                    {
                        if (Lineas.Count > 0)
                            return Json(new { codigo = 0, mensaje = Lineas, almacenSel }, JsonRequestBehavior.AllowGet);
                        else
                            return Json(new { codigo = -1, mensaje = "No se recuperarón etiquetas" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                        return Json(new { codigo = -2, mensaje = "Error al recuperar etiquetas guardadas" }, JsonRequestBehavior.AllowGet);
                default:
                    return Json(new { codigo = -2, mensaje = $"No se reconoce el tipo de acción indicado" }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult BorrarRBorrador(Produccion.EmisionRecibo registro) {
            SP.EliminarRegistroBorrador(registro);
            return Json("Eliminado");
        }
        [HttpPost]
        public ActionResult BorrarTBorrador(List<Produccion.EmisionRecibo> Lineas) {
            int response = SP.EliminarTBorrador(Lineas);
            if (response == 0)
                return Json(new { codigo = 0, mensaje = "Borrador eliminado" }, JsonRequestBehavior.DenyGet);
            else
                return Json(new { codigo = -1, mensaje = $"Surgio un error durante el proceso, intente una vez más. Registros eliminados: { response } de { Lineas.Count }"});
        }
    }
}