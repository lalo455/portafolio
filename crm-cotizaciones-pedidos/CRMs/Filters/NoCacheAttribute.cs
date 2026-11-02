using System;
using System.Web;
using System.Web.Mvc;

namespace CRMs.Filters
{
    public class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var response = filterContext.HttpContext.Response;
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            response.AppendHeader("Pragma", "no-cache");

            base.OnResultExecuting(filterContext);
        }
    }
}
