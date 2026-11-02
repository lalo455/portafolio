using System;
using System.Configuration;
using System.IO;
using System.Text;

namespace Rastreo.Utilities
{
    public class Logs
    {
        private readonly string _rutaLog;

        public Logs()
        {
            // Obtener la ruta de logs desde el archivo de configuración
            _rutaLog = ConfigurationManager.AppSettings["RutaLog"] ?? @"C:\RastreoLog\Log.txt";

            // Crear el directorio de logs si no existe
            string directorioLog = Path.GetDirectoryName(_rutaLog);
            if (!Directory.Exists(directorioLog))
            {
                Directory.CreateDirectory(directorioLog);
            }
        }

        // Método para registrar mensajes de depuración
        public void Debug(string mensaje, string modulo, string metodo)
        {
            EscribirLog("DEBUG", mensaje, modulo, metodo);
        }

        // Método para registrar información
        public void Info(string mensaje, string modulo, string metodo)
        {
            EscribirLog("INFO", mensaje, modulo, metodo);
        }

        // Método para registrar advertencias
        public void Warning(string mensaje, string modulo, string metodo)
        {
            EscribirLog("WARNING", mensaje, modulo, metodo);
        }

        // Método para registrar errores
        public void Error(string mensaje, string modulo, string metodo)
        {
            EscribirLog("ERROR", mensaje, modulo, metodo);
        }

        // Método para registrar excepciones
        public void Error(Exception ex, string modulo, string metodo)
        {
            string mensaje = $"Excepción: {ex.Message}\nStack Trace: {ex.StackTrace}";
            EscribirLog("ERROR", mensaje, modulo, metodo);
        }

        // Método base para escribir en el archivo de log
        private void EscribirLog(string nivel, string mensaje, string modulo, string metodo)
        {
            try
            {
                // Crear la entrada de log
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{nivel}] {modulo}.{metodo}: {mensaje}";

                // Escribir en el archivo de log
                using (StreamWriter writer = new StreamWriter(_rutaLog, true))
                {
                    writer.WriteLine(logEntry);
                }

                // También imprimir en la consola (útil para depuración)
                Console.WriteLine(logEntry);
            }
            catch (Exception ex)
            {
                // Si falla el logging, imprimir en la consola
                Console.WriteLine($"Error al escribir en el log: {ex.Message}");
            }
        }
    }
}