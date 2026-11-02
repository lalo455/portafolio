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

namespace Rastreo.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        //Log incidencias = new Log();
        Consultas ConsultasBD = new Consultas();
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
            //bool AutenticacionCorrecta = false;
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
                    //ExceptionUtility.LogMessageDIAPI(Credenciales.Usuario, "Error", "No se pudo conectar, datos incompletos.", "");
                    return Json(new { codigo = CodigoLogin, mensajes = respuestaValidacion[0].PropertyName, urlRetorno = "" }, JsonRequestBehavior.DenyGet);
                }
                else
                {
                    ModelosConsultas.UsuarioAutenticado Respues = Consulta.LoginUsuario(Credenciales);
                    if (Respues.AutenticacionCorrecta)
                        //pedir a miguel que agregue el campo validFor
                    {
                        //if (!Respues.validFor)
                        //{
                        //    CodigoLogin = -1;
                        //    MsgLogin = "El usuario esta inactivo";
                        //    return Json(new { codigo = CodigoLogin, mensajes = MsgLogin, urlRetorno = ReturnURl }, JsonRequestBehavior.DenyGet);
                        //}
                        //else
                        //{
                            ReturnURl = Url.Action("Menu", "Menu");
                            List<Claim> ClaimsIdentidad = new List<Claim>();
                            //Agregamos al Claims los valores de usuario y id
                            ClaimsIdentidad.Add(new Claim(ClaimTypes.NameIdentifier, Respues.UsuarioWeb.ToString()));
                            ClaimsIdentidad.Add(new Claim(ClaimTypes.Name, Respues.NombreEmpleado.ToString()));
                            ClaimsIdentidad.Add(new Claim(ClaimTypes.PrimarySid, Respues.IdEmpleadoSAP.ToString()));
                            ClaimsIdentidad.Add(new Claim(ClaimTypes.Role, Respues.RolWeb.ToString()));//guardamos el rol loggiado
                            ClaimsIdentity identity = new ClaimsIdentity(ClaimsIdentidad, DefaultAuthenticationTypes.ApplicationCookie);
                            
                            //caracterisitcas para cookies
                            HttpContext.GetOwinContext().Authentication.SignIn(new AuthenticationProperties()
                            {
                                AllowRefresh = true,
                                IsPersistent = true,
                                ExpiresUtc = DateTime.UtcNow.AddHours(1)
                            }, identity);

                            Datos.Id = Respues.UsuarioWeb;
                            Datos.Name = Respues.NombreEmpleado;
                            Datos.CardCode = Respues.IdEmpleadoSAP;
                        //}
                        //ExceptionUtility.LogMessageDIAPI(Respues.UsuarioWeb.ToString(), "", "Login de Usuario exitoso.", "");
                        //AutenticacionCorrecta = Respues.AutenticacionCorrecta;
                    }
                    else//if(!Respues.AutenticacionCorrecta)
                    {
                        MsgLogin = "El usuario o contraseña son incorrectos";
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
            HttpContext.GetOwinContext().Authentication.SignOut();
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