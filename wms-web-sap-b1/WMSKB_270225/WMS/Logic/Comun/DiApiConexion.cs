using SAPbobsCOM;
using System;
using System.Configuration;
using System.Runtime.InteropServices;
using static Dapper.SqlMapper;
using System.Security.Claims;
using System.Threading;
using System.Linq;

namespace WMS.Logic.Comun
{
    public static class DiApiConexion
    {
        public static Company company = new Company();
        private static ClaimsPrincipal Identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
        public static Company GetCompanyConnection { get; private set; } = new Company();
        public static readonly Utilities.Log Logs;

        public static int Conectar(ref string mensaje)
        {
            int connectionResult = 0;
            int errorCode;
            string errorMessage;
            if (company != null && company.Connected)
            {
                GetCompanyConnection = GetCompany();
                return connectionResult;
            }
            else
            {
                company = new Company
                {
                    Server = ConfigurationManager.AppSettings["ServerSAP"],
                    CompanyDB = ConfigurationManager.AppSettings["CompanyDB"],
                    UserName = Identity.Claims.Where(c => c.Type == ClaimTypes.Actor).Select(c => c.Value).SingleOrDefault(),
                    Password = Identity.Claims.Where(p => p.Type == ClaimTypes.Anonymous).Select(p => p.Value).SingleOrDefault(),
                    DbUserName = ConfigurationManager.AppSettings["UserDBSAP"],
                    DbPassword = ConfigurationManager.AppSettings["PassDBSAP"]
                };

                switch (ConfigurationManager.AppSettings["VersionSQL"].ToString())
                {
                    case "2012":
                        company.DbServerType = BoDataServerTypes.dst_MSSQL2012;
                        break;
                    case "2014":
                        company.DbServerType = BoDataServerTypes.dst_MSSQL2014;
                        break;
                    case "2016":
                        company.DbServerType = BoDataServerTypes.dst_MSSQL2016;
                        break;
                    case "2017":
                        company.DbServerType = BoDataServerTypes.dst_MSSQL2017;
                        break;
                    case "2019":
                        company.DbServerType = BoDataServerTypes.dst_MSSQL2019;
                        break;
                }

                company.language = BoSuppLangs.ln_Spanish_La;
                company.UseTrusted = false;
                connectionResult = company.Connect();

                if (connectionResult != 0)
                {
                    company.GetLastError(out errorCode, out errorMessage);
                    mensaje = errorMessage;
                    Logs.RegistrarEx($"Error:{errorCode.ToString() + " --> " + errorMessage}", new Exception());
                }

                else
                    GetCompanyConnection = GetCompany();
                return connectionResult;
            }
        }

        public static Company GetCompany()
        {
            return company;
        }

        public static void Desconectar()
        {
            if (company != null)
            {
                if (company != null && company.Connected)
                    company.Disconnect();

                Marshal.ReleaseComObject(company);                
                company = null;
            }
            GC.Collect();
        }

        public static void LimpiarObjeto(object obj)
        {
            if (obj != null)
            {
                Marshal.ReleaseComObject(obj);
                obj = null;
            }
            GC.Collect();
        }

    }
}