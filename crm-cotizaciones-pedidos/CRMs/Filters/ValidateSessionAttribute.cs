using System;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using System.Web.Routing;
using CRMs.Utilerias;

namespace CRMs.Filters
{
    public class ValidateSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var action = filterContext.ActionDescriptor.ActionName;

            // Excluir LoginController y sus acciones públicas
            if (controller.Equals("Login", StringComparison.OrdinalIgnoreCase))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            var context = filterContext.HttpContext;
            var user = context.User;

            if (user.Identity.IsAuthenticated)
            {
                var claimsIdentity = user.Identity as ClaimsIdentity;
                var usuarioId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var tokenSesion = context.Session["AuthToken"]?.ToString();
                var tokenEnServidor = SesionManager.ObtenerTokenUsuario(usuarioId);

                if (tokenSesion == null || tokenSesion != tokenEnServidor)
                {
                    context.GetOwinContext().Authentication.SignOut();
                    context.Session.Clear();
                    context.Session.Abandon();

                    var urlHelper = new UrlHelper(filterContext.RequestContext);
                    var loginUrl = urlHelper.Action("Index", "Login");
                    filterContext.Result = new RedirectResult(loginUrl);
                    return;
                }
            }
            else
            {
                // Usuario no autenticado
                var urlHelper = new UrlHelper(filterContext.RequestContext);
                var loginUrl = urlHelper.Action("Index", "Login");
                filterContext.Result = new RedirectResult(loginUrl);
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
