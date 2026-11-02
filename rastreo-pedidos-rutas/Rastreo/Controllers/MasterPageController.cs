using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Rastreo.Utilerias;

namespace Rastreo.Controllers
{
    public class MasterPageController : Controller
    {
        // GET: MasterPage
        public ActionResult PaginaInicio()
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
                ExceptionUtility.LogException(ex, "MasterPage.PaginaInicio");
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Index", "Login");
            }
        }
    }
}