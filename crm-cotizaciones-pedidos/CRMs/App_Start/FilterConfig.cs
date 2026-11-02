using System.Web;
using System.Web.Mvc;
using CRMs.Filters;

namespace CRMs
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new NoCacheAttribute()); // ← Añade esta línea
            filters.Add(new ValidateSessionAttribute());
        }
    }
}
