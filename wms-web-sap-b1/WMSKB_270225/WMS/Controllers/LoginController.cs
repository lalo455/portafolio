using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WMS.Logic.Comun;
using WMS.Models;
using WMS.Logic.Stores;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Configuration;
using SAPbobsCOM;
using System.Runtime.InteropServices;

namespace WMS.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        Company oCompany;
        StoreProcedure SP = new StoreProcedure();
        Integrar IntSAP = new Integrar();
        Utilities.Log Logs = new Utilities.Log();

        public ActionResult Login()
        {
            return View();
        }

        [Authorize]
        public ActionResult LogOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut();
            DiApiConexion.Desconectar();
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        public ActionResult Ingresar(Acceso.Credenciales Usuario)
        {
            string Usr = Usuario.Usuario;
            string modulo = this.ControllerContext.RouteData.Values["controller"].ToString();
            string metodo = this.ControllerContext.RouteData.Values["action"].ToString();

            Logs.FullInfo("Modulo: Login - Inicio", "", Usr, 1, "", ""); //Log Informativo

            Acceso.UsuarioLogeado UsrLogin = null;
            string UrlReturn = string.Empty;

            if (SP.Login(ref UsrLogin, Usuario))
            {
                if (UsrLogin != null)
                {
                    Logs.FullInfo("", "Acceso correcto del Usuario: " + Usr, Usr, 0, modulo, metodo); //Log Informativo

                    // Si la autenticación es correcta, se procede a entrar en la aplicación
                    UrlReturn = Url.Action("Remision", "Remision");
                    List<Claim> ClaimsIdentidad = new List<Claim>();
                    Acceso.Datos.UserId = UsrLogin.UserId;  // Asignamos el UserId a la clase Datos

                    // Agregamos al Claims los valores de usuario y id
                    ClaimsIdentidad.Add(new Claim(ClaimTypes.NameIdentifier, UsrLogin.UserId.ToString()));
                    ClaimsIdentidad.Add(new Claim(ClaimTypes.Name, UsrLogin.UserId));

                    ClaimsIdentity identity = new ClaimsIdentity(ClaimsIdentidad, DefaultAuthenticationTypes.ApplicationCookie);
                    // Características para cookies
                    HttpContext.GetOwinContext().Authentication.SignIn(new AuthenticationProperties()
                    {
                        AllowRefresh = true,
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddHours(double.Parse(ConfigurationManager.AppSettings["Sesion"]))
                    }, identity);

                    Logs.FullInfo("Modulo: Login - Fin", "", Usr, 2, "", ""); //Log Informativo
                    return Json(new { codigo = "0", mensaje = UrlReturn });
                }
                else
                {
                    Logs.FullInfo("", "Datos incorrectos, verificar configuración de usuario", Usr, 0, modulo, metodo); //Log Informativo
                    Logs.FullInfo("Modulo: Login - Fin", "", Usr, 2, "", ""); //Log Informativo
                    return Json(new { codigo = "-1", mensaje = "Datos incorrectos, verificar configuración de usuario" });
                }
            }
            else
            {
                Logs.FullInfo("", "El usuario no existe", Usr, 0, modulo, metodo); //Log Informativo
                Logs.FullInfo("Modulo: Login - Fin", "", Usr, 2, "", ""); //Log Informativo
                return Json(new { codigo = "-1", mensaje = "El usuario no existe" });
            }
        }

    }
}