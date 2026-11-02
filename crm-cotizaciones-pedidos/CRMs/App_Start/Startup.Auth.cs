using System;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace CRMs
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configura autenticación con cookies
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Login/Index"),
                LogoutPath = new PathString("/Login/Logout"),

                // ⏳ Expira si el usuario está inactivo 15 minutos
                ExpireTimeSpan = TimeSpan.FromMinutes(5), // o lo que necesites

                // 🔁 Se renueva con cada acción que el usuario haga
                SlidingExpiration = true,

                CookieHttpOnly = true,
                CookieSecure = CookieSecureOption.SameAsRequest // o Always si usas HTTPS
            });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
        }
    }
}
