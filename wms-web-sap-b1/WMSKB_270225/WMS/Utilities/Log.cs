using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using WMS.Models;

namespace WMS.Utilities
{
    public class Log
    {
        private readonly string _rutaBaseLog;
        private readonly string _rutaContenedor;
        private readonly string _rutaFullLog;

        public Log()
        {
            _rutaBaseLog = ConfigurationManager.AppSettings["RutaLog"] ?? @"C:\Logs\AppLogs";
            _rutaContenedor = ConfigurationManager.AppSettings["RutaContenedor"] ?? @"C:\Logs\Contenedores";
            _rutaFullLog = ConfigurationManager.AppSettings["RutaFullLog"] ?? @"C:\Logs\FullLogs";

            // Crear directorios si no existen
            Directory.CreateDirectory(_rutaBaseLog);
            Directory.CreateDirectory(_rutaContenedor);
            Directory.CreateDirectory(_rutaFullLog);
        }

        public void RegistrarError(Exception ex, string contextoAdicional = "")
        {
            try
            {
                var logEntry = new StringBuilder();
                logEntry.AppendLine(":::::::::::::::::::::::::::::::::::::::");
                logEntry.AppendLine($"Fecha: {DateTime.Now}");
                logEntry.AppendLine($"Mensaje: {ex.Message}");
                logEntry.AppendLine($"Contexto: {contextoAdicional}");
                logEntry.AppendLine($"StackTrace: {ex.StackTrace}");
                logEntry.AppendLine($"Tipo: {ex.GetType().FullName}");

                if (ex.InnerException != null)
                {
                    logEntry.AppendLine("--- Inner Exception ---");
                    logEntry.AppendLine($"Mensaje: {ex.InnerException.Message}");
                    logEntry.AppendLine($"StackTrace: {ex.InnerException.StackTrace}");
                }

                EscribirArchivo(Path.Combine(_rutaBaseLog, $"Error_{DateTime.Now:yyyyMMdd}.log"), logEntry.ToString());
            }
            catch (Exception logEx)
            {
                EventLog.WriteEntry("Application", $"Error al registrar log: {logEx.Message}", EventLogEntryType.Error);
            }
        }

        public void RegistrarEvento(string titulo, string mensaje, string usuario, int severidad)
        {
            var logEntry = new StringBuilder();
            logEntry.AppendLine($"[{DateTime.Now}] [{usuario}] [{severidad}]");
            logEntry.AppendLine($"Título: {titulo}");
            logEntry.AppendLine($"Mensaje: {mensaje}");

            EscribirArchivo(Path.Combine(_rutaFullLog, $"Evento_{DateTime.Now:yyyyMMdd}.log"), logEntry.ToString());
        }

        private void EscribirArchivo(string rutaCompleta, string contenido)
        {
            try
            {
                // Implementación thread-safe
                lock (this)
                {
                    File.AppendAllText(rutaCompleta, contenido);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Application", $"Error al escribir log: {ex.Message}", EventLogEntryType.Error);
            }
        }

        public void Registrar(string exception, string mensaje, string StackTrace, string nameSpace)
        {
            StreamWriter Wr = null;
            string rutaLog = ConfigurationManager.AppSettings["RutaLog"];
            try
            {
                if (!Directory.Exists(@"C:\WMSKBLog"))
                    Directory.CreateDirectory(@"C:\WMSKBLog");
                if (!File.Exists(rutaLog == "" ? @"C:\WMSKBLog\Log.txt" : rutaLog))
                    File.CreateText(rutaLog).Dispose();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Format("{0}", ":::::::::::::::::::::::::::::::::::::::"));
                sb.AppendLine(string.Format("Fecha > {0}", DateTime.Now));
                sb.AppendLine(string.Format("Clase/Método > {0}", StackTrace));
                sb.AppendLine(string.Format("Proyecto > {0}", nameSpace));
                sb.AppendLine(string.Format("Excepción > {0}", exception == null ? "" : exception));
                sb.AppendLine(string.Format("Mensaje > {0}", mensaje == null ? "" : mensaje));

                Wr = File.AppendText(rutaLog);
                Wr.Write(sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} Exception caught. Log de Errores: ", ex);
            }
            finally
            {
                if (Wr != null)
                {
                    Wr.Flush();
                    Wr.Close();
                }
            }
        }

        public void RegistrarEx(string mensaje, Exception exception)
        {
            StreamWriter Wr = null;
            string rutaLog = ConfigurationManager.AppSettings["RutaLog"];
            try
            {
                if (!Directory.Exists(@"C:\WMSKBLog"))
                    Directory.CreateDirectory(@"C:\WMSKBLog");
                if (!File.Exists(rutaLog == "" ? @"C:\WMSKBLog\Log.txt" : rutaLog))
                    File.CreateText(rutaLog).Dispose();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Format("{0}", ":::::::::::::::::::::::::::::::::::::::"));
                sb.AppendLine(string.Format("Fecha > {0}", DateTime.Now));
                sb.AppendLine(string.Format("Excepción > {0}", exception.StackTrace ?? ""));
                sb.AppendLine(string.Format("Mensaje > {0}", mensaje ?? ""));

                Wr = File.AppendText(rutaLog);
                Wr.Write(sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} Exception caught. Log de Errores: ", ex);
            }
            finally
            {
                if (Wr != null)
                {
                    Wr.Flush();
                    Wr.Close();
                }
            }
        }

        public void Escribir(string lotes, string guid, string destino, string grabado, string msj)
        {
            StreamWriter Wr = null;
            string rutaLog = ConfigurationManager.AppSettings["RutaLog"];
            try
            {
                if (!Directory.Exists(@"C:\WMSKBLog"))
                    Directory.CreateDirectory(@"C:\WMSKBLog");
                if (!File.Exists(rutaLog == "" ? @"C:\WMSKBLog\Log.txt" : rutaLog))
                    File.CreateText(rutaLog).Dispose();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Format("{0}", ":::::::::::::::::::::::::::::::::::::::"));
                sb.AppendLine(string.Format("Fecha : {0}", DateTime.Now));
                sb.AppendLine(string.Format("Lotes : {0}", lotes));
                sb.AppendLine(string.Format("Guid  : {0}", guid));
                sb.AppendLine(string.Format("Destino : {0}", destino));
                sb.AppendLine(string.Format("Grabado en : {0}", grabado));
                sb.AppendLine(string.Format("Mensaje SAP : {0}", msj));
                Wr = File.AppendText(rutaLog);
                Wr.Write(sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} Exception caught. Log de Errores: ", ex);
            }
            finally
            {
                if (Wr != null)
                {
                    Wr.Flush();
                    Wr.Close();
                }
            }
        }
        
        //Escribir Contenedor Alterno de Traspaso Manual
        public bool EscribirContenedor(string[] stream, string User, string NumParte, ref string Archivo) 
        {
            StreamWriter Wr = null;
            string rutaContenedor = ConfigurationManager.AppSettings["RutaContenedor"];
            //string rutaArchivo = $"{rutaContenedor}\\{DateTime.Now.ToString("MM-dd-yyyy")}_{User}_{ArchivoConsecutivo(rutaContenedor, User)}.txt";

            FullInfo("I n i c i o   P r o c e s a m i e n t o   d e   L o t e s    S i n    C o n t e n e d o r    O r i g i n a l", "", User, 3, "", ""); //Log Informativo
            FullInfo("", "Creación de Archivo de Lotes Sin Contenedor Original", User, 0, "Log", "EscribirContenedor"); //Log Informativo
            FullInfo("", "Ruta de Archivo: " + rutaContenedor, User, 0, "Log", "EscribirContenedor"); //Log Informativo

            try
            {
                if (!Directory.Exists(Path.Combine(rutaContenedor)))
                {
                    Directory.CreateDirectory(@"C:\Transferencias");
                    FullInfo("", "Se crea Directorio " + rutaContenedor, User, 0, "Log", "EscribirContenedor"); //Log Informativo
                }

                if (!Directory.Exists(Path.Combine(rutaContenedor, User)))
                {
                    Directory.CreateDirectory(Path.Combine(rutaContenedor, User));
                    FullInfo("", "Se crea Carpeta de Usuario: " + User + " en Directorio: " + rutaContenedor, User, 0, "Log", "EscribirContenedor"); //Log Informativo
                }

                //Por cada Número de Parte se Creara un Archivo con las Etiquetas que correspondan
                //Se verifica si ya existe un archivo igual, de ser así se creara un consecutivo
                string Pat = Path.Combine(rutaContenedor, User, DateTime.Now.ToString("dd") + "-" + DateTime.Now.ToString("MM") + "-" + DateTime.Now.ToString("yyyy") + "_" + User + "_" + NumParte + ".txt");
                string PatFile = getNextFileName(Pat);
                FullInfo("", "Archivo a Crear: " + PatFile, User, 0, "Log", "EscribirContenedor"); //Log Informativo

                /*if (!Directory.Exists($"{rutaContenedor}"))
                    Directory.CreateDirectory($"{rutaContenedor}");
                
                if (!File.Exists(rutaArchivo))
                    File.CreateText(rutaArchivo).Dispose();*/

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < stream.Length; i++)
                {
                    sb.AppendLine(stream[i].Trim());

                    FullInfo("", "Lote a registrar en archivo: " + stream[i].Trim(), User, 0, "Log", "EscribirContenedor"); //Log Informativo
                }

                Wr = File.AppendText(PatFile);
                Wr.Write(sb.ToString());

                Archivo = PatFile;
                return true;
            }
            catch(Exception ex)
            {
                Archivo = ex.Message;
                FullInfo("", "ERROR: Mensaje: " + ex.Message, User, 0, "Log", "EscribirContenedor"); //Log Informativo

                return false;
            }
            finally
            {
                if (Wr != null)
                {
                    Wr.Flush();
                    Wr.Close();
                }

                FullInfo("F i n   P r o c e s a m i e n t o   d e   L o t e s    S i n    C o n t e n e d o r    O r i g i n a l", "", User, 4, "", ""); //Log Informativo
            }
        }


        //Escribir Contenedor Alterno de Remision
        public bool EscribirContenedorRemision(string[] stream, string User, string NumParte, ref string Archivo)
        {
            StreamWriter Wr = null;
            string rutaContenedor = ConfigurationManager.AppSettings["RutaContenedorR"];
            //string rutaArchivo = $"{rutaContenedor}\\{DateTime.Now.ToString("MM-dd-yyyy")}_{User}_{ArchivoConsecutivo(rutaContenedor, User)}.txt";

            FullInfo("I n i c i o   P r o c e s a m i e n t o   d e   L o t e s    S i n    C o n t e n e d o r    O r i g i n a l", "", User, 3, "", ""); //Log Informativo
            FullInfo("", "Remisión -- Creación de Archivo de Lotes Sin Contenedor Original", User, 0, "Log", "EscribirContenedorRemision"); //Log Informativo
            FullInfo("", "Ruta de Archivo: " + rutaContenedor, User, 0, "Log", "EscribirContenedorRemision"); //Log Informativo

            try
            {
                if (!Directory.Exists(Path.Combine(rutaContenedor)))
                {
                    Directory.CreateDirectory(@"C:\Entregas");
                    FullInfo("", "Se crea Directorio " + rutaContenedor, User, 0, "Log", "EscribirContenedorRemision"); //Log Informativo
                }

                if (!Directory.Exists(Path.Combine(rutaContenedor, User)))
                {
                    Directory.CreateDirectory(Path.Combine(rutaContenedor, User));
                    FullInfo("", "Se crea Carpeta de Usuario: " + User + " en Directorio: " + rutaContenedor, User, 0, "Log", "EscribirContenedorRemision"); //Log Informativo
                }

                //Por cada Número de Parte se Creara un Archivo con las Etiquetas que correspondan
                //Se verifica si ya existe un archivo igual, de ser así se creara un consecutivo
                string Pat = Path.Combine(rutaContenedor, User, DateTime.Now.ToString("dd") + "-" + DateTime.Now.ToString("MM") + "-" + DateTime.Now.ToString("yyyy") + "_" + User + "_" + NumParte + ".txt");
                string PatFile = getNextFileName(Pat);
                FullInfo("", "Archivo a Crear: " + PatFile, User, 0, "Log", "EscribirContenedorRemision"); //Log Informativo

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < stream.Length; i++)
                {
                    sb.AppendLine(stream[i].Trim());

                    FullInfo("", "Lote a registrar en archivo: " + stream[i].Trim(), User, 0, "Log", "EscribirContenedorRemision"); //Log Informativo
                }

                Wr = File.AppendText(PatFile);
                Wr.Write(sb.ToString());

                Archivo = PatFile;
                return true;
            }
            catch (Exception ex)
            {
                Archivo = ex.Message;
                FullInfo("", "ERROR: Mensaje: " + ex.Message, User, 0, "Log", "EscribirContenedorRemision"); //Log Informativo

                return false;
            }
            finally
            {
                if (Wr != null)
                {
                    Wr.Flush();
                    Wr.Close();
                }

                FullInfo("F i n   P r o c e s a m i e n t o   d e   L o t e s    S i n    C o n t e n e d o r    O r i g i n a l", "", User, 4, "", ""); //Log Informativo
            }
        }


        private int ArchivoConsecutivo(string path, string user) 
        {
            if (Directory.Exists(path))
            {
                string[] filesEntries = Directory.GetFiles(path, $"*{user}*", SearchOption.TopDirectoryOnly);
                return filesEntries.Length > 0 ? int.Parse(Path.GetFileNameWithoutExtension(filesEntries[filesEntries.Length - 1]).Split('_')[2]) + 1 : 0;
            }
            else
                return 0;
        }

        private string getNextFileName(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            int i = 0;
            while (File.Exists(fileName))
            {
                if (i == 0)
                    //fileName = fileName.Replace(extension, "(" + ++i + ")" + extension);
                    fileName = fileName.Replace(extension, "_" + ++i + extension);
                else
                    //fileName = fileName.Replace("(" + i + ")" + extension, "(" + ++i + ")" + extension);
                    fileName = fileName.Replace("_" + i + extension, "_" + ++i + extension);
            }

            return fileName;
        }



        //Registro de Log Full, seguimiento de procesos
        //Archivo de Log, por dia y por usuario
        public void FullInfo(string titulo, string informacion, string User, int tipo, string modulo, string metodo)  //string exception, string mensaje, string StackTrace, string nameSpace
        {
            StreamWriter Wr = null;
            string rutaFullLog = ConfigurationManager.AppSettings["RutaFullLog"]; //Carpeta
            string rutaLogUsuario = $"{rutaFullLog}\\{User}"; //Carpeta de Usuario
            string rutaArchivo = $"{rutaLogUsuario}\\RegistroInformativo_{DateTime.Now.ToString("MM-dd-yyyy")}.txt"; //Archivo de Log
            
            try
            {
                #region CreacionCarpetaArchivoLog
                if (!Directory.Exists($"{rutaFullLog}")) //Verificar si la carpeta Existe
                    Directory.CreateDirectory($"{rutaFullLog}"); //Crear la carpeta si no existe
                
                if (!Directory.Exists($"{rutaLogUsuario}")) //Verificar si la carpeta Existe
                    Directory.CreateDirectory($"{rutaLogUsuario}"); //Crear la carpeta si no existe

                if (!File.Exists(rutaArchivo)) //Verificar si existe el archivo
                    File.CreateText(rutaArchivo).Dispose(); //Crear el archivo si no existe el archivo
                #endregion

                StringBuilder sb = new StringBuilder();

                #region LogInformativo
                //Prncipal
                if (tipo == 1)
                {
                    sb.AppendLine(string.Format("{0}", "Inicio Seguimiento: " + titulo + "***********************************************************"));
                    //sb.AppendLine(string.Format("{0}", titulo));
                }
                
                if (tipo == 0)
                {
                    sb.AppendLine(string.Format("{0}", DateTime.Now) + " -> " + modulo + " -> " + metodo + " -> " + informacion);

                }
                
                if (tipo == 2)
                {
                    //sb.AppendLine(string.Format("{0}", titulo));
                    sb.AppendLine(string.Format("{0}", "Fin de Seguimiento: " + titulo + "*****************************************************"));
                    sb.Append(Environment.NewLine);
                    sb.AppendLine(string.Format("{0}", "========================================================================================================"));
                    sb.AppendLine(string.Format("{0}", ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>"));
                    sb.AppendLine(string.Format("{0}", "========================================================================================================"));
                    sb.Append(Environment.NewLine);
                }

                //Secundario
                if (tipo == 3)
                {
                    sb.Append(Environment.NewLine);
                    sb.AppendLine(string.Format("{0}", "Inicio Proceso: " + titulo + ":::::::::::::::::::::::::::::::::::::::::::::::::::::::::::"));
                    //sb.AppendLine(string.Format("{0}", titulo));
                }

                if (tipo == 4)
                {
                    //sb.AppendLine(string.Format("{0}", titulo));
                    sb.AppendLine(string.Format("{0}", "Fin de Proceso: " + titulo + ":::::::::::::::::::::::::::::::::::::::::::::::::::::::::::"));
                    sb.Append(Environment.NewLine);
                }
                #endregion

                #region LogError
                /*
                sb.AppendLine(string.Format("Fecha > {0}", DateTime.Now));
                sb.AppendLine(string.Format("Clase/Método > {0}", StackTrace));
                sb.AppendLine(string.Format("Proyecto > {0}", nameSpace));
                sb.AppendLine(string.Format("Excepción > {0}", exception == null ? "" : exception));
                sb.AppendLine(string.Format("Mensaje > {0}", mensaje == null ? "" : mensaje));
                */
                #endregion


                Wr = File.AppendText(rutaArchivo);
                Wr.Write(sb.ToString());
            }
            catch (Exception ex) 
            {
                Console.WriteLine("{0} Exception caught. Log de Errores: ", ex);
            }
            finally
            {
                if (Wr != null)
                {
                    Wr.Flush();
                    Wr.Close();
                }
            }
        }




    }
}