using Rastreo.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Rastreo.Controllers
{
    [AuthorizeRole("R")]
    public class CorteDeCajaController : Controller
    {
        // GET: CorteDeCaja
        public ActionResult CorteCaja()
        {
            try
            {
                var Verificar = ((ClaimsIdentity)User.Identity).Claims.First().Value;
                if (String.IsNullOrEmpty(Verificar))
                {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Login");
            }
        }
    }
}