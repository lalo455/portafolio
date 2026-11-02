using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CRMs.Models;
using System.Security.Claims;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using System.Threading;
using System.Web.Security;
using CRMs.Utilerias;

namespace CRMs.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        // GET: Login
        //Log incidencias = new Log();
        Consultas ConsultasBD = new Consultas();

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Acceso(DatosAcceso Credenciales)
        {
            Consultas Consulta = new Consultas();
            string ReturnURl = string.Empty;
            DatosAcceso.UsuarioLogeado loginDeUsuario = new DatosAcceso.UsuarioLogeado();
            bool AutenticacionCorrecta = false;
            int CodigoLogin = 0;
            string MsgLogin = "";
            try
            {
                //if (Credenciales.Usuario == null || Credenciales.Password == null)
                //{
                //    CodigoLogin = -1;
                //    return Json(new { codigo = CodigoLogin, mensajes = "Asegures de introducir Usuario y Contraseña", urlRetorno = ReturnURl }, JsonRequestBehavior.DenyGet); ;
                //}

                var respuestaValidacion = ValidacionCredenciales(Credenciales);

                if (respuestaValidacion.Count > 0)
                {
                    loginDeUsuario.AutenticacionCorrecta = false;
                    CodigoLogin = -1;
                    ExceptionUtility.LogMessageDIAPI(Credenciales.Usuario, "Error", "No se pudo conectar, datos incompletos.", "");
                    return Json(new { codigo = CodigoLogin, mensajes = respuestaValidacion[0].PropertyName, urlRetorno = "" }, JsonRequestBehavior.DenyGet);
                }
                else
                {
                    var Respues = Consulta.LoginUsuario(Credenciales);
                    if (Respues.AutenticacionCorrecta)
                    {

                        ReturnURl = Url.Action("PaginaInicio", "MasterPage");

                        string tokenSesion = Guid.NewGuid().ToString();
                        Session["AuthToken"] = tokenSesion;
                        SesionManager.GuardarTokenUsuario(Respues.UsuarioWeb, tokenSesion);

                        var ClaimsIdentidad = new List<Claim>();
                        //Agregamos al Claims los valores de usuario y id
                        ClaimsIdentidad.Add(new Claim(ClaimTypes.NameIdentifier, Respues.UsuarioWeb.ToString()));
                        ClaimsIdentidad.Add(new Claim(ClaimTypes.Name, Respues.NombreEmpleado.ToString()));
                        ClaimsIdentidad.Add(new Claim(ClaimTypes.Actor, Respues.IdEmpleadoSAP.ToString()));
                        ClaimsIdentidad.Add(new Claim(ClaimTypes.Role, Respues.RolWeb.ToString()));
                        var identity = new ClaimsIdentity(ClaimsIdentidad, DefaultAuthenticationTypes.ApplicationCookie);
                        
                        //caracterisitcas para cookies
                        HttpContext.GetOwinContext().Authentication.SignIn(new AuthenticationProperties()
                        {
                            AllowRefresh = true,
                            IsPersistent = true,
                            ExpiresUtc = DateTime.UtcNow.AddHours(6)
                        }, identity);

                        ExceptionUtility.LogMessageDIAPI(Respues.UsuarioWeb.ToString(), "", "Login de Usuario exitoso.", ReturnURl);
                        AutenticacionCorrecta = Respues.AutenticacionCorrecta;
                    }
                    else if(Respues.AutenticacionCorrecta == false)
                    {
                        MsgLogin = "Asegurese de ingresar credenciales válidas.";
                        CodigoLogin = -1;
                    }                    
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Error durante la autenticación");
                MsgLogin = "Se presento un error durante la autenticación";
                CodigoLogin = -1;
                
            }
        
            return Json(new { codigo = CodigoLogin, mensajes = MsgLogin, urlRetorno = ReturnURl }, JsonRequestBehavior.DenyGet);

        }
      
        public ActionResult Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                string usuarioId = User.Identity.Name;
                SesionManager.EliminarTokenUsuario(usuarioId);
            }

            HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Response.Cookies.Clear();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Login");
        }

        public dynamic ValidacionCredenciales(DatosAcceso DatosAcceso) {

            DatosAcceso.Validador validador = new DatosAcceso.Validador();
            var resultadoValidacion = validador.Validate(DatosAcceso);

            if (!resultadoValidacion.IsValid)
            {
                return resultadoValidacion.Errors;
            }
            return resultadoValidacion.Errors;
        }

        public bool CompararCredenciales(DatosAcceso DatosIngresado) {

            var respuestaCon = ConsultasBD.LoginUsuario(DatosIngresado);
            if (respuestaCon == null)
                return respuestaCon.AutenticacionCorrecta;
            else 
                return false;
        }
    }
}