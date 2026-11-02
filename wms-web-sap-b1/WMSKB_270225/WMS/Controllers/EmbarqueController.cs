using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using WMS.Logic.Comun;
using WMS.Logic.Stores;
using WMS.Models;

namespace WMS.Controllers
{
    public class EmbarqueController : Controller
    {
        StoreProcedure SP = new StoreProcedure();
        Utilities.Log Logs = new Utilities.Log();

        // GET: Embarque
        public ActionResult ValidacionEmbarque()
        {
            try
            {
                if (String.IsNullOrEmpty(((ClaimsIdentity)User.Identity).Claims.First().Value))
                    return RedirectToAction("Login", "Login");
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
        [HttpPost]
        public ActionResult CargarDocumento(int Doc)
        {
            List<Embarque.ASN> ListaEmbarques = new List<Embarque.ASN>();
            if (SP.ObtenerLists(ref ListaEmbarques, "EM", Doc))
                return Json(new { codigo = ListaEmbarques.Count > 0 ? 1 : 0, mensaje = ListaEmbarques }, JsonRequestBehavior.DenyGet);
            else
                return Json(new { codigo = -1, mensaje = "Error al recuperar información del documento: " + Doc }, JsonRequestBehavior.DenyGet);
        }
        [HttpPost]
        public ActionResult ObtenerParteSN(string Etiqueta, string CodigoSN)
        {
            List<Embarque.NParte> Enc = new List<Embarque.NParte>();
            if (SP.ParteSN(ref Enc, Etiqueta, CodigoSN))
            {
                //if (Enc.Count > 0)
                    return Json(new { codigo = 0, mensaje = Enc }, JsonRequestBehavior.DenyGet);
                //else
                //{
                //    Enc[0].Encontrado = 0;
                //    return Json(new { codigo = 0, mensaje = Enc }, JsonRequestBehavior.DenyGet);
                //}
            }
            else
                return Json(new { codigo = -1, mensaje = "No recuperó información" }, JsonRequestBehavior.DenyGet);
        }
        [HttpPost]
        
        public ActionResult ValidarDocumento(int NumEmbarque, List<Embarque.ASN> LineasDoc) {
            if (SP.ValidarEmbarque(NumEmbarque, LineasDoc))
                return Json(new { codigo = 0, mensaje = "Embarque actualizado" }, JsonRequestBehavior.DenyGet);
            else
                return Json(new { codigo = -1, mensaje = "No se pudo actualizar el embarque: " + NumEmbarque }, JsonRequestBehavior.DenyGet);
        }

        [HttpGet]
        public ActionResult SociosEmbarques() {
            List<Embarque.Socios> Socios = new List<Embarque.Socios>();
            if (SP.SociosEmbarques(ref Socios)) {
                if(Socios.Count > 0)
                    return Json(new { codigo = 0, mensaje = Socios }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { codigo = -1, mensaje = "No se recupero la lista de clientes" }, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { codigo = -1, mensaje = "Error al recuperar la lista de clientes" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult NoParte(string Serial, string Documento) {
            string Item = string.Empty;
            if (SP.ExisteNumeroSerie(ref Item, Serial, Documento))
            {
                if (String.IsNullOrEmpty(Item))
                    return Json(new { codigo = -1, mensaje = "La etiqueta no existe en el embarque " + Documento }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { codigo = 0, mensaje = int.Parse(Item) }, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { codigo = -1, mensaje = "Error : " + Item });
        }
    }
}