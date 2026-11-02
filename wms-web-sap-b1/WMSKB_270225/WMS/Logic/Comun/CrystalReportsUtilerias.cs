using System;
using System.Collections.Generic;
using System.Linq;
using CrystalDecisions.CrystalReports.Engine;
using System.Web;

namespace WMS.Logic.Comun
{
    public class CrystalReportsUtilerias
    {
        public static void RecurseAndRemap(ReportDocument CR, string servidor, string baseDeDatos, string usuario, string contrasenia)
        {
            Utilities.Log Logs = new Utilities.Log();

            try
            {
                for (int i = 0; i < CR.DataSourceConnections.Count; i++)
                {
                    CR.DataSourceConnections[i].SetLogon(usuario, contrasenia);
                    CR.DataSourceConnections[i].SetConnection(servidor, baseDeDatos, false);
                }

                CR.SetDatabaseLogon(usuario, contrasenia);

                for (int i = 0; i < CR.Database.Tables.Count; i++)
                {
                    CR.Database.Tables[i].LogOnInfo.ConnectionInfo.UserID = usuario;
                    CR.Database.Tables[i].LogOnInfo.ConnectionInfo.Password = contrasenia;

                }

                if (CR.IsSubreport)
                {
                    for (int i = 0; i < CR.Subreports.Count; i++)
                    {
                        RecurseAndRemap(CR.Subreports[i], servidor, baseDeDatos, usuario, contrasenia);
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.Registrar(ex.InnerException == null ? "---" : ex.InnerException.ToString(), ex.Message, ex.StackTrace, "CrystalReportsUtilerias.RecurseAndRemap");
            }

        }

    }
}