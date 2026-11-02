using System;
using System.IO;
using System.Web;
using System.Configuration;

namespace Rastreo.Utilerias
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
                LogException(ex, "ExceptionUtility.LogMessageDIAPI");
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
                sw.Close();
            }

        }

        // POR SI ES NECESARIO REGRESAR EL ERROR AL USUARIO
        public static void NotifySystemOps(Exception exc)
        {
            // 
        }
    }
}