using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WMS.Logic.Stores;
using WMS.Models;
using WMS.Logic.Comun;
using System.Security.Claims;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Data;
using SAPbobsCOM;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;

namespace WMS.Controllers
{
    public class TraspasoManualController : Controller
    {
        StoreProcedure SP;
        Obtener Lt;
        Integrar IntSAP;
        Obtener Ob;
        Utilities.Log Logs;

        public TraspasoManualController() { 
            SP = new StoreProcedure();
            Lt = new Obtener();
            IntSAP = new Integrar();
            Ob = new Obtener();
            Logs = new Utilities.Log();
        }
        
        public ActionResult TrsManual()
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
        
        [HttpGet]
        public ActionResult ClientesConAlmacen() 
        {
            List<TrsManual.ClienteAlmacen> ListaCA = new List<TrsManual.ClienteAlmacen>();
            if (Ob.ObtenerClienteAlmacen(ref ListaCA))
            {
                if (ListaCA.Count > 0)
                {
                    return Json(new { codigo = 0, mensaje = ListaCA }, JsonRequestBehavior.AllowGet);
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

        public ActionResult ObtenerAlmacenes(string TipoWhs, string WhsCode)
        {
            List<TrsManual.Almacen> ListAlm = new List<TrsManual.Almacen>();

            if (Ob.ObtenerAlm(ref ListAlm, WhsCode, TipoWhs))
            {
                if (ListAlm.Count > 0)
                {
                    return Json(new { codigo = 0, mensaje = ListAlm }, JsonRequestBehavior.AllowGet);
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

        public ActionResult VerificarEtq(string QR)
        {
            string StEti = string.Empty;
            List<InicialEtiqueta> Lista = new List<InicialEtiqueta>();

            //Si la etiqueta es diferente de T00000 se puede leer y procesar
            if (Lt.EstadoEtiqueta(ref Lista, QR.Split('&')[0]))
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
                                    return Json(new { codigo = 0, mensaje = "" });
                                case "X":
                                    return Json(new { codigo = -2, mensaje = "La etiqueta leída esta BLOQUEADA" });
                                case "T": //Estatus de estiqueta 
                                    return Json(new { codigo = -2, mensaje = "La etiqueta leída FUE USADA PARA TRASPALEO" });
                                case null:
                                case "":
                                    //return Json(new { codigo = -2, mensaje = "La etiqueta leída NO EXISTE"});
                                    List<Etiqueta> Lotes = new List<Etiqueta>();
                                    if (Lt.Lotes(QR, ref Lotes))
                                    {
                                        if (Lotes.Count > 0)
                                            return Json(new { codigo = 0, mensaje = "" });
                                        else
                                            return Json(new { codigo = -2, mensaje = "La etiqueta leída NO EXISTE" });
                                    }
                                    else
                                        return Json(new { codigo = -1, mensaje = "Error al obtener información de la etiqueta" }, JsonRequestBehavior.DenyGet);
                                default:
                                    return Json(new { codigo = -1, mensaje = "La etiqueta existe pero NO HA SIDO LIBERADA" });
                            }
                        }
                    }
                    return Json(new { codigo = 0, mensaje = Lista }, JsonRequestBehavior.DenyGet);
                }
            }
            else
            {
                return Json(new { codigo = -1, mensaje = StEti });
            }
        }

        public ActionResult ObtenerLote(string QR) {
            List<Etiqueta> Lotes = new List<Etiqueta>();
            if (Lt.Lotes(QR, ref Lotes))
            {
                switch (Lotes.Count)
                {
                    case 0:
                        return Json(new { codigo = -1, mensaje = "No se recuperó informacion de la etiqueta" }, JsonRequestBehavior.DenyGet);
                    default:
                        return Json(new { codigo = 0, mensaje = Lotes }, JsonRequestBehavior.DenyGet);
                }
            }
            else
                return Json(new { codigo = -1, mensaje = "Error al obtener información de la etiqueta"}, JsonRequestBehavior.DenyGet);
        }
        
        public ActionResult Crear(TrsManual.Traspaso Documento, string CodigoGuia, List<Detalle> RegMaster) 
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();
            string CodeGuia = CodigoGuia + Usuario;

            Logs.FullInfo("Modulo: Traspaso Manual - Inicio", "", Usuario, 1, "", ""); //Log Informativo

            Logs.FullInfo("Información a Procesar", "", Usuario, 3, "", ""); //Log Informativo
                Logs.FullInfo("",   "\r\n Codigo Guia del Proceso: " + CodeGuia + 
                                    "\r\n Cliente: " + Documento.ClienteSel +
                                    "\r\n Almacen Destino : " + Documento.AlmacenSel
                                    , Usuario, 0, modulo, metodo); //Log Informativo
            try
            {
                //Antes de Realizar el proceso, se verificará que el Codigo de Guia en Transferencia de Stock (OWTR.U_WMS_CodeGuid) de SAP ---- IUrizar 10-06-2021
                string DocNum = string.Empty;
                if (Lt.GetDocNum(ref DocNum, CodeGuia, "Traspaso"))
                {
                    switch (DocNum)
                    {
                        case null: //No existe - Se puede procesar la información
                        case "": //No existe - Se puede procesar la información

                            //**************************************************************************
                            //**************************************************************************
                            List<Detalle> regParciales = new List<Detalle>();
                            //Validar si hay registros master a procesar
                            if (RegMaster != null && RegMaster.Count > 0) {
                                Logs.FullInfo("", "Proceso de registros Individuales y de Master", Usuario, 0, modulo, metodo); //Log Informativo
                                //Obtener solo registros leidos tipo I: Individual
                                foreach (var reg in Documento.Lineas) {
                                    if (reg.TipoLectura == "I") {
                                        regParciales.Add(new Detalle() 
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
                                Documento.Lineas.Clear();

                                //Agregar registros Individuales y registros de Master authors.AddRange(duplicateAuthors);
                                Documento.Lineas.AddRange(regParciales);
                                Documento.Lineas.AddRange(RegMaster);
                            }
                            //**************************************************************************
                            //**************************************************************************

                            foreach (var etq in Documento.Lineas)
                            {
                                Logs.FullInfo("", "Etiqueta Leida a procesar: " + etq.DistNumber, Usuario, 0, modulo, metodo); //Log Informativo
                            }
                            Logs.FullInfo("Información a Procesar", "", Usuario, 4, "", ""); //Log Informativo
                            
                            StringBuilder st = new StringBuilder(); //Lista de Lotes sin contenedor 
                            StringBuilder LotesSC = new StringBuilder(); //Lotes Sin Contenedor Original a Guardar en el archivo por numero de Parte
                            string Archivos = "";

                            //Actualización: Se agrega Condicion -- 14-01-2021
                            //SI el proceso lleva CLIENTE, se evalua Contenedor Original
                            //SI el proceso NO lleva Cliente, NO se avalua Contenedor Original

                            int result = 0; //Valor Inicial: SIN Cliente = 0, de lo contrario Valor sera igual a Respuesta de Validación de Contenedor Original
                            if (Documento.ClienteSel != null)
                            {
                                Logs.FullInfo("", "Traspaso con Cliente => Se evalua Contenedor Original: " + st.ToString(), Usuario, 0, modulo, metodo); //Log Informativo
                                result = Ob.ContenedorOriginal(ref st, ref LotesSC, ref Archivos, Documento.Lineas); //Evaluacion de Contenedor Original de los Lotes Leidos
                            }
                            else
                            {
                                Logs.FullInfo("", "Traspaso SIN Cliente => NO se evalua Contenedor Original: " + st.ToString(), Usuario, 0, modulo, metodo); //Log Informativo
                                result = 0; //NO se evalua Contenedor Original de los Lotes
                            }

                            //De acuerdo al valor de result, se evaluan las opciones a seguir
                            //SI result = 0 , indica SIN Cliente o Lotes completos con Contenedor Original => Se procede con la Transferencia
                            //De lo contrario, se evalua la opción 
                            switch (result)
                            {
                                case -1:
                                    Logs.FullInfo("", "No se pudo validar el contenedor: " + st.ToString(), Usuario, 0, modulo, metodo); //Log Informativo
                                    return Json(new { codigo = -1, mensaje = $"No se pudo validar el contenedor: {st.ToString()}" }, JsonRequestBehavior.DenyGet);

                                case 0:
                                    Mensajes msj = new Mensajes();
                                    Guid g;
                                    g = Guid.NewGuid();
                                    if (SP.GuardarRegistrosTraspaso(Documento, g.ToString())) //Guarda Registro en Tabla Movimiento
                                    {
                                        //Integración en SAP de la Transferencia de Stock                            
                                        if (IntSAP.Documento(ref msj, Documento, g.ToString(), CodeGuia))
                                        {
                                            Logs.FullInfo("", "Documento de Traspaso Creado: " + msj.DocNum, Usuario, 0, modulo, metodo); //Log Informativo
                                            return Json(new { codigo = 0, mensaje = "Transferencia de Stock realizada \r\n Num. Doc. : " + msj.DocNum }, JsonRequestBehavior.DenyGet); //Muestra Documento Creado
                                        }
                                        else
                                        {
                                            Logs.Registrar("Documento de Traspaso NO Creado", msj.Mensaje, modulo, metodo);
                                            Logs.FullInfo("", "Documento de Traspaso NO Creado: " + msj.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo
                                            return Json(new { codigo = -1, mensaje = msj.Mensaje }, JsonRequestBehavior.DenyGet); //No se genero Documento
                                        }
                                    }
                                    else
                                    {
                                        Logs.FullInfo("", "Error al guardar los registros para el traspaso", Usuario, 0, modulo, metodo); //Log Informativo
                                        return Json(new { codigo = -1, mensaje = "Error al guardar los registros para el traspaso" }, JsonRequestBehavior.DenyGet);
                                    }

                                case 1:
                                    Logs.FullInfo("", "Se encontraron Lotes que NO corresponde a contenedor original: " + st.ToString() + "\r\n --- Se Crearon archivos: " + Archivos, Usuario, 0, modulo, metodo); //Log Informativo
                                    return Json(new { codigo = -1, mensaje = "Los siguientes Lotes NO corresponde a contenedor original: " + st.ToString() + "\r\n --- " + Archivos }, JsonRequestBehavior.DenyGet);

                                default:
                                    Logs.FullInfo("", "Opcion desconocida" + st.ToString(), Usuario, 0, modulo, metodo); //Log Informativo
                                    return Json(new { codigo = -1, mensaje = "Opcion desconocida" }, JsonRequestBehavior.DenyGet);
                            }


                        default: //Existe - NO se procesa, se sale del proceso y se muestra el Documento
                            Logs.FullInfo("", "Obtenido por Verificacion de CodeGuia: " + CodeGuia + " --- Documento de Traspaso Creado: " + DocNum, Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = 0, mensaje = "Transferencia de Stock \r\n Num. Doc. : " + DocNum }, JsonRequestBehavior.DenyGet); //Muestra Documento Creado
                    }
                }
                else
                {
                    Logs.FullInfo("", "Error Verificación CodeGuia => Mensaje ::: " + DocNum, Usuario, 0, modulo, metodo); //Log Informativo
                    return Json(new { codigo = -1, mensaje = "Error Verificación CodeGuia => Mensaje ::: " + DocNum });
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar("Documento de Traspaso NO Creado", ex.Message, modulo, metodo);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = "Error en el Traspaso" }, JsonRequestBehavior.DenyGet); 
            }
            finally
            {
                Logs.FullInfo("Modulo: Traspaso Manual - Fin", "", Usuario, 2, "", ""); //Log Informativo
            }
        }

    }
}