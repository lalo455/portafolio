using System;
using System.IO;
using System.Web;
using System.Configuration;
using System.Text;

namespace CRMs.Utilerias
{

    public sealed class ExceptionUtility
    {
        //  
        private ExceptionUtility()
        { }

        //PARA REGISTRAR LOS MENSAJES DEL DIAPI
        public static void LogMessageDIAPI(string User, string CodeError, string Mensaje, string folio)
        {
            StreamWriter sw1 = null;// = new StreamWriter(logPath, true);
            try
            {

                string logPath = ConfigurationManager.AppSettings["RutaLog"];
                sw1 = new StreamWriter(logPath, true);

                sw1.WriteLine("********** {0} **********", DateTime.Now);
                sw1.Write("USR SAP: ");
                sw1.WriteLine(User);
                sw1.Write("CODIGO SAP: ");
                sw1.WriteLine(CodeError);
                sw1.Write("MENSAJE SAP: ");
                sw1.WriteLine(Mensaje);
                sw1.Write("FOLIO_PEDIDO: ");
                sw1.WriteLine(folio);
                sw1.Close();
            }
            catch (Exception ex)
            {
                //LogException(ex, "ExceptionUtility.LogMessageDIAPI");
                Console.WriteLine(ex.ToString());
                sw1.Close();
            }

        }
        // Registrar la Excepcion
        public static void LogException(Exception exc, string source)
        {
            StreamWriter sw = null;// new StreamWriter(logFile, true);
            try
            {

                string logFile = ConfigurationManager.AppSettings["RutaLog"];

                sw = new StreamWriter(logFile, true);
                sw.WriteLine("********** {0} **********", DateTime.Now);
                if (exc.InnerException != null)
                {
                    sw.Write("TIPO DE EXCEPCION INTERNA: ");
                    sw.WriteLine(exc.InnerException.GetType().ToString());
                    sw.Write("EXCEPCION INTERNA: ");
                    sw.WriteLine(exc.InnerException.Message);
                    sw.Write("ORIGEN DE LA EXCEPCION: ");
                    sw.WriteLine(exc.InnerException.Source);
                    if (exc.InnerException.StackTrace != null)
                    {
                        sw.WriteLine("SEGUIMIENTO DE PILA: ");
                        sw.WriteLine(exc.InnerException.StackTrace);
                    }
                }
                sw.Write("TIPO DE EXCEPCION: ");
                sw.WriteLine(exc.GetType().ToString());
                sw.WriteLine("EXCEPCION: " + exc.Message);
                sw.WriteLine("ORIGEN: " + source);
                sw.WriteLine("SEGUIMIENTO DE PILA: ");
                if (exc.StackTrace != null)
                {
                    sw.WriteLine(exc.StackTrace);
                    sw.WriteLine();
                }
                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                sw.Close();
            }
        }

        public static void FormatExcep(FormatException exc, string source)
        {
            StreamWriter sw = null;// new StreamWriter(logFile, true);
            try
            {

                string logFile = ConfigurationManager.AppSettings["RutaLog"];

                sw = new StreamWriter(logFile, true);
                sw.WriteLine("********** {0} **********", DateTime.Now);
                if (exc.InnerException != null)
                {
                    sw.Write("TIPO DE EXCEPCION INTERNA: ");
                    sw.WriteLine(exc.InnerException.GetType().ToString());
                    sw.Write("EXCEPCION INTERNA: ");
                    sw.WriteLine(exc.InnerException.Message);
                    sw.Write("ORIGEN DE LA EXCEPCION: ");
                    sw.WriteLine(exc.InnerException.Source);
                    if (exc.InnerException.StackTrace != null)
                    {
                        sw.WriteLine("SEGUIMIENTO DE PILA: ");
                        sw.WriteLine(exc.InnerException.StackTrace);
                    }
                }
                sw.Write("TIPO DE EXCEPCION: ");
                sw.WriteLine(exc.GetType().ToString());
                sw.WriteLine("EXCEPCION: " + exc.Message);
                sw.WriteLine("ORIGEN: " + source);
                sw.WriteLine("SEGUIMIENTO DE PILA: ");
                if (exc.StackTrace != null)
                {
                    sw.WriteLine(exc.StackTrace);
                    sw.WriteLine();
                }
                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                sw.Close();
            }

        }

        public static void FullInfo(string titulo, string informacion, string User, int tipo, string modulo, string metodo)  //string exception, string mensaje, string StackTrace, string nameSpace
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
                Console.WriteLine("{0} Exception caught. Log Seguimiento: ", ex);
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

        // POR SI ES NECESARIO REGRESAR EL ERROR AL USUARIO
        public static void NotifySystemOps(Exception exc)
        {
            // 
        }
    }
}