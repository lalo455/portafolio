
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rastreo.Models;
using System.Security.Claims;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using System.Threading;
using System.Web.Security;
using Rastreo.Utilerias;
using System.Web.UI.WebControls;
using Rastreo.Filters;

namespace Rastreo.Controllers
{
    [AuthorizeRole("A", "R")]  // Solo usuarios con rol A (Administrador) o R (Repartidor)
    public class MenuController : Controller
    {
     

        // GET: Menu
        public ActionResult Menu()
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