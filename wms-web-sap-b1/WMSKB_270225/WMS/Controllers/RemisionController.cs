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

namespace WMS.Controllers
{
    public class RemisionController : Controller
    {
        StoreProcedure SP = new StoreProcedure();
        Obtener Lt = new Obtener();
        Integrar IntSAP = new Integrar();
        Utilities.Log Logs = new Utilities.Log();
        Obtener Ob = new Obtener();
        
        // GET: Remision
        public ActionResult Remision()
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
            catch (Exception)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Login", "Login");
            }
        }

        [HttpPost]
        public ActionResult ObtenerClientes(string Tipo, string Param) {
            List<TrsManual.Cliente> Clts = new List<TrsManual.Cliente>();
            if (SP.ObtenerDtsRem(ref Clts, Tipo, Param))
                return Json(new { codigo = 0, mensaje = Clts }, JsonRequestBehavior.DenyGet);
            else
                return Json(new { codigo = -1, mensaje = "Error al recuperar la lista de clientes" }, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public ActionResult ObtenerPedidos(string Tipo, string Param) {
            List<Remision.Pedido> Peds = new List<Remision.Pedido>();
            if (SP.ObtenerDtsRem(ref Peds, Tipo, Param))
                return Json(new { codigo = 0, mensaje = Peds }, JsonRequestBehavior.DenyGet);
            else
                return Json(new { codigo = -1, mensaje = "Error al recuperar la lista de Pedidos" }, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public ActionResult ObtenerDetallePed(string Tipo, string Param) {
            List<Remision.DetallePedido> DetPeds = new List<Remision.DetallePedido>();
            if (SP.ObtenerDtsRem(ref DetPeds, Tipo, Param))
                return Json(new { codigo = 0, mensaje = DetPeds }, JsonRequestBehavior.DenyGet);
            else
                return Json(new { codigo= -1, mensaje = "Error al recuperar el detalle del pedid"}, JsonRequestBehavior.DenyGet);
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
        public ActionResult Crear(Remision.Preliminar Documento, string CodigoGuia, List<DetalleR> RegMaster) 
        {
            string Usuario = ((ClaimsIdentity)User.Identity).Claims.First().Value;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();
            string CodeGuia = CodigoGuia + Usuario;

            Logs.FullInfo("Modulo: Remision - Inicio", "", Usuario, 1, "", ""); //Log Informativo
            Logs.FullInfo("Información a Procesar", "", Usuario, 3, "", ""); //Log Informativo
            Logs.FullInfo("", "\r\n Codigo Guia del Proceso: " + CodeGuia +
                                "\r\n Cliente: " + Documento.ClienteSel +
                                "\r\n Pedido : " + Documento.Pedido +
                                "\r\n Referencia : " + Documento.Referencia
                                , Usuario, 0, modulo, metodo); //Log Informativo

            try
            {
                //Antes de Realizar el proceso, se verificará que el Codigo de Guia en Transferencia de Stock (OWTR.U_WMS_CodeGuid) de SAP ---- IUrizar 10-06-2021
                string DocNum = string.Empty;
                if (Lt.GetDocNum(ref DocNum, CodeGuia, "Remision"))
                {
                    switch (DocNum)
                    {
                        case null: //No existe - Se puede procesar la información
                        case "": //No existe - Se puede procesar la información

                            //**************************************************************************
                            //**************************************************************************
                            List<DetalleR> regParciales = new List<DetalleR>();
                            //Validar si hay registros master a procesar
                            if (RegMaster != null && RegMaster.Count > 0)
                            {
                                Logs.FullInfo("", "Proceso de registros Individuales y de Master", Usuario, 0, modulo, metodo); //Log Informativo
                                //Obtener solo registros leidos tipo I: Individual
                                foreach (var reg in Documento.Lineas)
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
                                            Tarima = reg.Tarima,
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

                            //*********************************************************************/
                            //Validar Contenedor -- 19-01-2021
                            //Si tiene Contenedor Original o Contenedor Original Desechable se sigue el procedimiento
                            //De lo contrario, se crea el archivo txt con los Lotes 
                            StringBuilder st = new StringBuilder(); //Lista de Lotes sin contenedor 
                            StringBuilder LotesSC = new StringBuilder(); //Lotes Sin Contenedor Original a Guardar en el archivo por numero de Parte
                            string Archivos = "";

                            Logs.FullInfo("", "Se evalua Contenedor Original", Usuario, 0, modulo, metodo); //Log Informativo
                            int result = Ob.ContenedorOriginalRemision(ref st, ref LotesSC, ref Archivos, Documento.Lineas); //Evaluacion de Contenedor Original de los Lotes Leidos
                            //**********************************************************/

                            switch (result)
                            {
                                case -1:
                                    Logs.FullInfo("", "No se pudo validar el contenedor: " + st.ToString(), Usuario, 0, modulo, metodo); //Log Informativo
                                    return Json(new { codigo = -1, mensaje = $"No se pudo validar el contenedor: {st.ToString()}" }, JsonRequestBehavior.DenyGet);

                                case 0:
                                    Mensajes RespInt = new Mensajes();
                                    Guid g;
                                    g = Guid.NewGuid();

                                    if (SP.GuardarRegistros(Documento, g.ToString()))
                                    {
                                        if (IntSAP.Remision(ref RespInt, Documento, g.ToString(), CodeGuia)) //Crear Documento
                                        {
                                            Logs.FullInfo("", "Documento Preliminar de Remision Creado: DocNum => " + RespInt.DocNum + "--- DocEntry => " + RespInt.DocEntry, Usuario, 0, modulo, metodo); //Log Informativo
                                            return Json(new { codigo = 0, mensaje = "Documento Preliminar de Entrega realizada : " + RespInt.DocEntry }, JsonRequestBehavior.DenyGet); //Regresa Documento creado
                                        }
                                        else
                                        {
                                            Logs.FullInfo("", "Documento Preliminar de Remision NO Creado: " + RespInt.Mensaje, Usuario, 0, modulo, metodo); //Log Informativo
                                            return Json(new { codigo = -1, mensaje = RespInt.Mensaje }, JsonRequestBehavior.DenyGet); //Error en la creacion del documento
                                        }
                                    }
                                    else
                                    {
                                        Logs.FullInfo("", "Error al guardar los registros", Usuario, 0, modulo, metodo); //Log Informativo
                                        return Json(new { codigo = -1, mensaje = "Error al guardar los registros" });
                                    }

                                case 1:
                                    Logs.FullInfo("", "Se encontraron Lotes que NO corresponde a contenedor original: " + st.ToString() + "\r\n --- Se Crearon archivos: " + Archivos, Usuario, 0, modulo, metodo); //Log Informativo
                                    return Json(new { codigo = -1, mensaje = "Los siguientes Lotes NO corresponde a contenedor original: " + st.ToString() + "\r\n --- " + Archivos }, JsonRequestBehavior.DenyGet);

                                default:
                                    Logs.FullInfo("", "Opcion desconocida" + st.ToString(), Usuario, 0, modulo, metodo); //Log Informativo
                                    return Json(new { codigo = -1, mensaje = "Opcion desconocida" }, JsonRequestBehavior.DenyGet);
                            }

                        default: //Existe - NO se procesa, se sale del proceso y se muestra el Documento
                            Logs.FullInfo("", "Obtenido por Verificacion de CodeGuia: " + CodeGuia + " --- Documento de Preliminar de Entrega Creado: " + DocNum, Usuario, 0, modulo, metodo); //Log Informativo
                            return Json(new { codigo = 0, mensaje = "Documento Preliminar de Entrega : " + DocNum }, JsonRequestBehavior.DenyGet); //Muestra Documento Creado
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
                Logs.Registrar("Documento Preliminar de Remisión NO Realizado", ex.Message, modulo, metodo);
                Logs.FullInfo("", "ERROR: Mensaje: " + ex.Message, Usuario, 0, modulo, metodo); //Log Informativo
                return Json(new { codigo = -1, mensaje = ex.Message });
            }
            finally
            {
                Logs.FullInfo("Modulo: Remision - Fin", "", Usuario, 2, "", ""); //Log Informativo
            }            
        }
        
        [HttpGet]
        public ActionResult NumerosDeCatalogos(string CardCode) {
            List<string> Catalogo = new List<string>();
            if (SP.CatalogoSocioDeNegocios(CardCode, ref Catalogo))
            {
                if (Catalogo.Count > 0)
                    return Json(new { codigo = 0, mensaje = Catalogo }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { codigo = -1, mensaje = $"El cliente {CardCode} no tiene un catálogo asignado"});
            }
            else
                return Json(new { codigo = -1, mensaje = $"Error al recuperar catálogo de artículos para el cliente {CardCode}"});

        }

        [HttpPost]
        public ActionResult ValidarPedido(string ordenVentaSAP, string numeroLineaPedido,
                                  string descripcionArticulo, string numeroPiezas,
                                  string area, string huacal)
        {
            try
            {
                var sp = new StoreProcedure();
                List<PedidoArquitectura> datosArquitectura;
                string mensaje;

                if (sp.ValidarPedido(ordenVentaSAP, numeroLineaPedido, descripcionArticulo,
                                    numeroPiezas, area, out datosArquitectura, out mensaje))
                {
                    if (area == "L")
                    {
                        var primerResultado = datosArquitectura.FirstOrDefault();
                        return Json(new
                        {
                            codigo = 0,
                            mensaje = "Pedido de Laminado validado correctamente",
                            area = "L",
                            ordenVenta = ordenVentaSAP,
                            huacal = huacal,
                            descripcion = descripcionArticulo,
                            piezas = int.Parse(numeroPiezas),
                            cliente = primerResultado?.Cliente,
                            articulo = primerResultado?.Articulo
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            codigo = 0,
                            mensaje = "Pedido de Arquitectura validado correctamente",
                            area = "A",
                            datos = datosArquitectura,
                            huacalPrincipal = huacal,
                            cliente = datosArquitectura.FirstOrDefault()?.Cliente,
                            articulo = datosArquitectura.FirstOrDefault()?.Articulo
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        codigo = -1,
                        mensaje = mensaje
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    codigo = -1,
                    mensaje = $"Error al validar pedido: {ex.Message}"
                });
            }
        }
    }
}