using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using CRMs.Utilerias;
using CRMs.Filters;

namespace CRMs.Controllers
{
    [Authorize]
    [NoCache]
    public class PedidoController : Controller
    {
        // GET: Pedido
        public ActionResult Pedidos()
        {
            try
            {
                var Verif = ((ClaimsIdentity)User.Identity).Claims.First().Value;
                if (Verif == "")
                {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Login");
                }
                else
                    return View();
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Pedido.Pedidos");
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Login");
            }
        }

        // GET: Consulta Facturas
        public ActionResult Facturas()
        {
            try
            {
                var Verif = ((ClaimsIdentity)User.Identity).Claims.First().Value;
                if (Verif == "")
                {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Login");
                }
                else
                    return View();
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Pedido.Facturas");
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Login");
            }
        }
    }
}