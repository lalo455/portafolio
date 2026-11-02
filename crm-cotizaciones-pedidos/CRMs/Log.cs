using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace CRMs
{
    public class Log
    {
        public void Registrar(string excepcion, string mensaje, string Clase_Metodo, string espacioNombre)
        {

            StreamWriter Wr = null;
            string rutaLog = ConfigurationManager.AppSettings["RutaLog"];
            try
            {
                if (rutaLog.Length == 0)
                    rutaLog = @"C:\PortalWebLog\Incidencias.txt";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Format("{0}",":::::::::::::::::::::::::::::::::::::::"));
                sb.AppendLine(string.Format("Fecha > {0}", DateTime.Now.ToString("dd/mm/yyyy")));
                sb.AppendLine(string.Format("Clase/Método > {0}", Clase_Metodo));
                sb.AppendLine(string.Format("Proyecto > {0}", espacioNombre));
                sb.AppendLine(string.Format("Excepción > {0}", excepcion == null ? "" : excepcion));
                sb.AppendLine(string.Format("Mensaje > {0}", mensaje == null ? "" : mensaje));

                Wr = File.AppendText(rutaLog);
                Wr.Write(sb.ToString());
            }
            catch (Exception ex)
            {

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