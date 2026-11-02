using Microsoft.Extensions.Configuration;
using SAPbobsCOM;
using SapMiddleware.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace SapMiddleware.Services;

public class SapConnectionManager : ISapConnectionManager
{
    private readonly IConfiguration _configuration;

    public SapConnectionManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Company ConnectCompany()
    {
        Company company = new Company();

        company.Server = _configuration["SapSettings:Server"];
        company.CompanyDB = _configuration["SapSettings:CompanyDB"];
        company.DbUserName = _configuration["SapSettings:DbUserName"];
        company.DbPassword = _configuration["SapSettings:DbPassword"];
        company.UserName = _configuration["SapSettings:UserName"];
        company.Password = _configuration["SapSettings:Password"];

        // 1. Manejo dinámico del driver SQL (Solución al Error -119)
        string dbServerTypeStr = _configuration["SapSettings:DbServerType"] ?? "dst_MSSQL2017";
        if (Enum.TryParse<BoDataServerTypes>(dbServerTypeStr, true, out var dbServerType))
        {
            company.DbServerType = dbServerType;
        }
        else
        {
            company.DbServerType = BoDataServerTypes.dst_MSSQL2017;
        }

        company.UseTrusted = _configuration.GetValue<bool>("SapSettings:UseTrusted");
        company.language = BoSuppLangs.ln_Spanish_La;
        
        // 2. Servidor de Licencias (Solución al Error -111)
        // IMPORTANTE: NO asignar SLDServer ya que causa el rechazo de autenticación (Error -132) en SAP 9.3/10.0
        string licenseServer = _configuration["SapSettings:LicenseServer"];
        if (!string.IsNullOrWhiteSpace(licenseServer))
        {
            company.LicenseServer = licenseServer;
        }

        int result = company.Connect();
        if (result != 0)
        {
            company.GetLastError(out int errorCode, out string errorMsg);
            
            // Liberar objeto COM si falló la conexión
            Marshal.ReleaseComObject(company);

            throw new Exception($"Error de Autenticación/Conexión SAP DI-API. Código [{errorCode}]: {errorMsg}");
        }

        return company;
    }

    public void DisconnectCompany(Company? company)
    {
        if (company != null)
        {
            try
            {
                if (company.Connected)
                {
                    company.Disconnect();
                }
            }
            catch
            {
                // Ignorar errores al desconectar si el canal ya estaba roto
            }
            finally
            {
                Marshal.ReleaseComObject(company);
                // NOTA: No llamamos GC.Collect() aquí intencionalmente.
                // Hacerlo de forma síncrona bloqueaba el worker de Hangfire mientras
                // el GC corría. El runtime de .NET recoge los objetos COM liberados
                // automáticamente en el próximo ciclo de GC sin latencia adicional.
            }
        }
    }
}
