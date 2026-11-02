using System;
using System.Runtime.Caching;

namespace CRMs.Utilerias
{
    public static class SesionManager
    {
        private static ObjectCache cache = MemoryCache.Default;

        public static void GuardarTokenUsuario(string usuarioId, string token)
        {
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(6)
            };
            cache.Set("session_" + usuarioId, token, policy);
        }

        public static string ObtenerTokenUsuario(string usuarioId)
        {
            return cache.Get("session_" + usuarioId) as string;
        }

        public static void EliminarTokenUsuario(string usuarioId)
        {
            cache.Remove("session_" + usuarioId);
        }
    }
}
