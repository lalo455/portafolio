using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Rastreo.Controllers
{
    public class CotizacionController : Controller
    {
        // GET: Cotizacion
        public ActionResult Cotizacion()
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
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Login");
            }
        }
    }
}