using SAPbobsCOM;
using System;
using System.Configuration;
using System.Reflection;
using System.Security.Claims;
using System.Web.Mvc;
using Rastreo.Utilities;
using Rastreo.Models;
using System.Linq;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Rastreo.Utilerias;
using System.Runtime.InteropServices;
using System.Data.Odbc;
using System.IO;
using WMS.Models;
using Rastreo.Logic.Comun;
using System.Collections.Generic;

namespace Rastreo.Controllers
{
    [Authorize]
    public class IntegracionSAPController : Controller
    {
        // Candado estático para evitar choques si 2+ usuarios integran al mismo tiempo
        private static readonly object _lockDIAPI = new object();

        private Company _company;
        private Logs log = new Logs();

        public IntegracionSAPController()
        {
            // EL CONSTRUCTOR DEBE ESTAR VACÍO O SOLO INICIALIZAR VARIABLES BÁSICAS.
            // NUNCA te conectes a SAP aquí.
        }

        // Método privado para conectarse dinámicamente leyendo el Web.config
        private void ConectarSAP()
        {
            try
            {
                // Si ya está conectado, no hacemos nada
                if (_company != null && _company.Connected) return;

                string cadenaConexionSAP = ConfigurationManager.ConnectionStrings["ConexionSAPDI"].ConnectionString;

                // Limpiamos los saltos de línea y separamos
                var parametros = cadenaConexionSAP
                    .Replace("\r", "").Replace("\n", "")
                    .Split(';')
                    .Select(part => part.Split('='))
                    .Where(part => part.Length == 2)
                    .ToDictionary(sp => sp[0].Trim(), sp => sp[1].Trim());

                _company = new Company();
                _company.Server = parametros.ContainsKey("Server") ? parametros["Server"] : "";
                _company.CompanyDB = parametros.ContainsKey("CompanyDB") ? parametros["CompanyDB"] : "";

                // Corrección de la llave a DbServerType
                string serverType = parametros.ContainsKey("DbServerType") ? parametros["DbServerType"] : "";
                _company.DbServerType = serverType == "MSSQL2016" ? BoDataServerTypes.dst_MSSQL2016 :
                                        serverType == "MSSQL2017" ? BoDataServerTypes.dst_MSSQL2017 :
                                        BoDataServerTypes.dst_MSSQL2019; // Default

                // CORRECCIÓN PRINCIPAL: Mapeo exacto a las llaves de tu Web.config
                _company.UserName = parametros.ContainsKey("UserName") ? parametros["UserName"] : "";
                _company.Password = parametros.ContainsKey("Password") ? parametros["Password"] : "";
                _company.DbUserName = parametros.ContainsKey("DbUserName") ? parametros["DbUserName"] : "";
                _company.DbPassword = parametros.ContainsKey("DbPassword") ? parametros["DbPassword"] : "";

                string license = parametros.ContainsKey("LicenseServer") ? parametros["LicenseServer"] : "";
                _company.LicenseServer = license;
                _company.SLDServer = license;

                _company.language = BoSuppLangs.ln_Spanish_La;
                _company.UseTrusted = false;

                int result = _company.Connect();
                if (result != 0)
                {
                    string errorMessage = "Error DI-API: " + _company.GetLastErrorDescription();
                    log.Error(errorMessage, "IntegracionSapController", "ConectarSAP");
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "IntegracionSapController", "ConectarSAP");
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_company != null)
            {
                if (_company.Connected)
                {
                    _company.Disconnect();
                }
                Marshal.ReleaseComObject(_company);
                _company = null;
            }
            base.Dispose(disposing);
        }
      

   
        private int ObtenerDocNumOrdenEntrega(int docEntryOrdenEntrega)
        {
            Documents ordenEntrega = null;
            try
            {
                ordenEntrega = (Documents)_company.GetBusinessObject(BoObjectTypes.oDeliveryNotes);
                if (ordenEntrega.GetByKey(docEntryOrdenEntrega))
                {
                    return ordenEntrega.DocNum;
                }
                return 0;
            }
            finally
            {
                if (ordenEntrega != null)
                {
                    Marshal.ReleaseComObject(ordenEntrega);
                    ordenEntrega = null;
                }
            }
        }

        public Rastreo.Models.ResultadoSAP GenerarOrdenDeEntrega(int docEntryPedido, List<DetallePedido> detalles,string idRepartidor)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
          
            // Declarar variables COM afuera para poder destruirlas al final
            Documents pedido = null;
            Documents ordenEntrega = null;
            log.Info($"Inicio de GenerarOrdenDeEntrega. DocEntryPedido: {docEntryPedido}", modulo, metodo);
            lock (_lockDIAPI)
            {
                try
            {
                    ConectarSAP();
                // Validar conexión a SAP
                if (_company == null || !_company.Connected)
                {
                    log.Error("Conexión a SAP no está activa.", modulo, metodo);
                    return new Rastreo.Models.ResultadoSAP { success = false, docEntry = 0, message = "Conexión a SAP no está activa." };
                }

                // Validar detalles del pedido
                if (detalles == null || detalles.Count == 0)
                {
                    log.Error("La lista de detalles del pedido está vacía o es nula.", modulo, metodo);
                    return new Rastreo.Models.ResultadoSAP { success = false, docEntry = 0, message = "La lista de detalles del pedido está vacía o es nula." };
                }

                    // Obtener el pedido en SAP
                    pedido = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);
                    if (!pedido.GetByKey(docEntryPedido))
                    {
                        log.Error($"No se encontró el pedido con DocEntry: {docEntryPedido}", modulo, metodo);
                        return new Rastreo.Models.ResultadoSAP { success = false, docEntry = 0, message = "Pedido no encontrado" };
                    }

                log.Info($"Pedido encontrado. Cliente: {pedido.CardCode}, Total de líneas: {pedido.Lines.Count}", modulo, metodo);

                // Iniciar transacción en SAP
                _company.StartTransaction();

                    // Crear la Orden de Entrega
                    ordenEntrega = (Documents)_company.GetBusinessObject(BoObjectTypes.oDeliveryNotes);
                    ordenEntrega.CardCode = pedido.CardCode;
                    ordenEntrega.DocDate = DateTime.Now;

                bool primeraLinea = true;

                foreach (var detalle in detalles)
                {
                    if (detalle.Entregado && detalle.CantidadEditada > 0)
                    {
                        if (!primeraLinea)
                            ordenEntrega.Lines.Add();

                        ordenEntrega.Lines.BaseType = (int)BoObjectTypes.oOrders;
                        ordenEntrega.Lines.BaseEntry = docEntryPedido;
                        ordenEntrega.Lines.BaseLine = detalle.Linea;
                        ordenEntrega.Lines.Quantity = (double)detalle.CantidadEditada;
                        ordenEntrega.Lines.ItemCode = detalle.ItemCode;
                        ordenEntrega.UserFields.Fields.Item("U_UsuarioEntrego").Value = idRepartidor;
                            primeraLinea = false;
                    }
                }

                // Guardar Orden de Entrega en SAP
                if (ordenEntrega.Add() == 0)
                {
                    int docEntry = int.Parse(_company.GetNewObjectKey());
                    log.Info($"Orden de entrega generada con éxito. DocEntry: {docEntry}", modulo, metodo);

                    // Llamar a `CerrarPedidoEnSAP` para gestionar cierre o actualización del pedido
                    bool pedidoCerrado = CerrarPedidoEnSAP(docEntryPedido, detalles, idRepartidor);
                   
                        if (pedidoCerrado)
                    {
                        log.Info($"Pedido cerrado correctamente en SAP. DocEntryPedido: {docEntryPedido}", modulo, metodo);
                    }
                    else
                    {
                        log.Info($"Pedido actualizado en SAP pero sigue abierto. DocEntryPedido: {docEntryPedido}", modulo, metodo);
                    }

                    // Confirmar transacción
                    _company.EndTransaction(BoWfTransOpt.wf_Commit);
                    log.Info("Transacción finalizada con Commit.", modulo, metodo);

                    return new Rastreo.Models.ResultadoSAP { success = true, docEntry = docEntry, message = "Orden de entrega creada correctamente." };
                }
                else
                {
                    _company.EndTransaction(BoWfTransOpt.wf_RollBack);
                    log.Info("Transacción finalizada con Rollback debido a un error en la generación de la orden de entrega.", modulo, metodo);

                    log.Error($"Error al generar la orden de entrega: {_company.GetLastErrorDescription()}", modulo, metodo);
                    return new Rastreo.Models.ResultadoSAP { success = false, docEntry = 0, message = $"Error {_company.GetLastErrorCode()}: {_company.GetLastErrorDescription()}" };
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (_company.InTransaction)
                        _company.EndTransaction(BoWfTransOpt.wf_RollBack);
                        log.Error(ex, modulo, metodo);
                    }
                catch (Exception rollbackEx)
                {
                    log.Error($"Error al hacer Rollback: {rollbackEx.Message}", modulo, metodo);
                }

                log.Error(ex, modulo, metodo);
                return new Rastreo.Models.ResultadoSAP { success = false, docEntry = 0, message = ex.Message };
            }
                finally
                {
                    // ESTO SALVA LA MEMORIA DE TU SERVIDOR
                    if (pedido != null)
                    {
                        Marshal.ReleaseComObject(pedido);
                        pedido = null;
                    }
                    if (ordenEntrega != null)
                    {
                        Marshal.ReleaseComObject(ordenEntrega);
                        ordenEntrega = null;
                    }
                }
            }
        }

        private bool CerrarPedidoEnSAP(int docEntryPedido, List<DetallePedido> detalles, string idRepartidor)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            Documents pedido = null;

            log.Debug($"Inicio de CerrarPedidoEnSAP. docEntryPedido: {docEntryPedido}", modulo, metodo);

            try
            {
                pedido = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);
                if (!pedido.GetByKey(docEntryPedido))
                {
                    log.Error($"No se encontró el pedido con DocEntry: {docEntryPedido}", modulo, metodo);
                    return false;
                }

                // 1. ASIGNAR EL CAMPO PERSONALIZADO AL PEDIDO
                pedido.UserFields.Fields.Item("U_UsuarioEntrego").Value = idRepartidor;

                // 2. GUARDAR EL UDF EN SAP
                // Nota: Ya no modificamos las cantidades a mano porque SAP lo hace automáticamente.
                // Al quitar esa modificación manual, este Update() será exitoso.
                if (pedido.Update() == 0)
                {
                    log.Info($"Pedido actualizado correctamente con el usuario {idRepartidor}. docEntryPedido: {docEntryPedido}", modulo, metodo);

                    // Verificar si todas las cantidades fueron entregadas (evitando errores por nulos)
                    bool pedidoCompleto = detalles.All(item => item != null && item.CantidadPendiente != null && Convert.ToDouble(item.CantidadPendiente) == 0);

                    if (pedidoCompleto)
                    {
                        // SAP cierra el pedido automáticamente al entregar todo, 
                        // pero validamos por si acaso quedó abierto y lo forzamos a cerrar.
                        if (pedido.DocumentStatus != BoStatus.bost_Close)
                        {
                            if (pedido.Close() == 0)
                            {
                                log.Info($"Pedido cerrado correctamente. docEntryPedido: {docEntryPedido}", modulo, metodo);
                                return true;
                            }
                            else
                            {
                                log.Error($"Error al cerrar el pedido: {_company.GetLastErrorCode()} - {_company.GetLastErrorDescription()}", modulo, metodo);
                                return false;
                            }
                        }
                        return true; // Ya estaba cerrado por SAP
                    }

                    return false; // Retorna false porque sigue abierto (entrega parcial)
                }
                else
                {
                    log.Error($"Error al actualizar el pedido: {_company.GetLastErrorCode()} - {_company.GetLastErrorDescription()}", modulo, metodo);
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, modulo, metodo);
                return false;
            }
            finally
            {
                if (pedido != null)
                {
                    Marshal.ReleaseComObject(pedido);
                    pedido = null;
                }
                log.Debug($"Fin de CerrarPedidoEnSAP. docEntryPedido: {docEntryPedido}", modulo, metodo);
            }
        }

        private void HacerRollBack(int docEntryPedido)
        {
            string modulo = MethodBase.GetCurrentMethod().ReflectedType.Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            Documents pedido = null;

            log.Debug($"Inicio de HacerRollBack. docEntryPedido: {docEntryPedido}", modulo, metodo);

            try
            {
                pedido = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);
                if (pedido.GetByKey(docEntryPedido))
                {
                    // Intentar reabrir el pedido
                    if (pedido.Reopen() == 0)
                    {
                        log.Info($"Pedido reabierto correctamente. docEntryPedido: {docEntryPedido}", modulo, metodo);
                    }
                    else
                    {
                        // Si falla, obtener el error
                        int errorCode = _company.GetLastErrorCode();
                        string errorMessage = _company.GetLastErrorDescription();
                        log.Error($"Error al reabrir el pedido: {errorCode} - {errorMessage}", modulo, metodo);

                        // Notificar al usuario que el pedido no se puede reabrir
                        throw new Exception($"No se pudo reabrir el pedido. Error: {errorMessage}");
                    }
                }
                else
                {
                    log.Error($"No se encontró el pedido con DocEntry: {docEntryPedido}", modulo, metodo);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, modulo, metodo);
                throw; // Relanzar la excepción para que se maneje en un nivel superior
            }
            finally
            {
                if (pedido != null)
                {
                    Marshal.ReleaseComObject(pedido);
                    pedido = null;
                }
                log.Debug($"Fin de HacerRollBack. docEntryPedido: {docEntryPedido}", modulo, metodo);
            }
        }



        private Stream GenerarImpresionOrdenEntrega(int docEntry)
        {
            string modulo = this.GetType().Name;
            string metodo = MethodBase.GetCurrentMethod().Name;
            Logs log = new Logs();

            try
            {
                // Obtener los parámetros de conexión a SAP
                SAP param = modeloConexion();
                if (param == null)
                {
                    throw new Exception("No se pudo obtener la conexión a SAP.");
                }

                string server = "10.0.0.1";
                string database = param.Database;
                string userDB = param.UserId;
                string password = param.Password;

                // Ruta del reporte de Crystal Reports

                string rutaReporte = Server.MapPath("~/Reportes/FormatoTicket.rpt");
                if (string.IsNullOrEmpty(rutaReporte))
                {
                    throw new Exception("La ruta del reporte no está configurada.");
                }

                // Cargar el reporte
                ReportDocument reporte = new ReportDocument();
                reporte.Load(rutaReporte);
                reporte.Refresh();

                // Verificar si el parámetro DocKey@ existe en el reporte
                if (reporte.ParameterFields["DocKey@"] == null)
                {
                    throw new Exception("El parámetro DocKey@ no existe en el reporte.");
                }

                // Establecer el parámetro del reporte (DocKey@ es de tipo numérico)
                reporte.SetParameterValue("DocKey", docEntry);

                // Configurar la conexión a la base de datos
                OdbcConnectionStringBuilder odbcConnection = new OdbcConnectionStringBuilder
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["DBSap"].ConnectionString
                };

                // Mapear la conexión del reporte
                CrystalReportsUtilerias.RecurseAndRemap(reporte, server, database, userDB, password);

                // Exportar el reporte a un stream en formato PDF
                return reporte.ExportToStream(ExportFormatType.PortableDocFormat);
            }
            catch (Exception ex)
            {
                log.Error(ex, modulo, metodo);
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes("Se generó un error al momento de procesar el documento PDF.");
                return new MemoryStream(bytes);
            }
        }


        public SAP modeloConexion()
        {
            // 1. Declaramos modeloSAp e inicializamos en nulo aquí adentro
            SAP modeloSAp = null;

            try
            {
                // 2. Le agregamos "string" para declarar CadenaSAP
                string CadenaSAP = ConfigurationManager.ConnectionStrings["ConexionSAPDI"].ConnectionString;

                String[] substrings = CadenaSAP.Split(';');
                modeloSAp = new SAP()
                {
                    Server = substrings[0].Split('=')[0] == "Server" ? substrings[0].Split('=')[1] : "",
                    Database = substrings[1].Split('=')[0] == "Database" ? substrings[1].Split('=')[1] : "",
                    UserId = substrings[2].Split('=')[0] == "UserId" ? substrings[2].Split('=')[1] : "",
                    Password = substrings[3].Split('=')[0] == "Password" ? substrings[3].Split('=')[1] : "",
                    UserIdSAP = substrings[4].Split('=')[0] == "UserIdSAP" ? substrings[4].Split('=')[1] : "",
                    PasswordSAP = substrings[5].Split('=')[0] == "PasswordSAP" ? substrings[5].Split('=')[1] : "",
                    ServerType = substrings[6].Split('=')[0] == "ServerType" ? substrings[6].Split('=')[1] : "",
                    LicenseServer = substrings[7].Split('=')[0] == "LicenseServer" ? substrings[7].Split('=')[1] : ""
                };
            }
            catch (Exception ex)
            {
                log.Error(ex, this.GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

            return modeloSAp;
        }
    }
}