using System;
using System.Net;
using System.Net.NetworkInformation;

namespace Utilities
{
    public static class PingClass
    {
        public static string Host { get; set; } 
        public static bool Enabled { get; set; } 
        public static int TimeoutMs { get; set; } = 2000;
        public static string LastError { get; private set; }

        public static bool IsValid()
        {
            LastError = string.Empty;
            if (!Enabled) return true;
            if (string.IsNullOrWhiteSpace(Host))
            {
                LastError = "Host no configurado.";
                return false;
            }

            try
            {
                string host = GetHostFromString(Host);
                using var ping = new Ping();
                var reply = ping.Send(host, TimeoutMs);

                if (reply == null)
                {
                    LastError = "No se obtuvo respuesta.";
                    return false;
                }

                if (reply.Status == IPStatus.Success) { return true; }

                LastError = $"Ping falló: {reply.Status}";
                return false;
            }
            catch (Exception ex)
            {
                LastError = $"Error: {ex.Message}";
                return false;
            }
        }

        public static string GetError() => LastError;

        private static string GetHostFromString(string value)
        {
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
                return uri.Host;
            return value;
        }
    }
}
