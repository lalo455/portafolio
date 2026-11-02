using System.Data;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using SAPbobsCOM;
using SapMiddleware.DTOs;
using SapMiddleware.Interfaces;
using Serilog.Context;

namespace SapMiddleware.Services;

public class SapIntegrationService : ISapIntegrationService
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private readonly ISapConnectionManager _sapConnectionManager;
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<SapIntegrationService> _logger;

    public SapIntegrationService(
        ISapConnectionManager sapConnectionManager,
        IDbConnection dbConnection,
        ILogger<SapIntegrationService> logger)
    {
        _sapConnectionManager = sapConnectionManager;
        _dbConnection = dbConnection;
        _logger = logger;
    }

    #region Procesamiento de Pedidos (CRM)

    public async Task ProcessPedidoAsync(Guid ticketId)
    {
        // 1. Obtener el payload de la base de datos
        const string sqlSelect = "SELECT Payload FROM IntegrationTickets WHERE Id = @Id";
        var jsonPayload = await _dbConnection.QuerySingleOrDefaultAsync<string>(sqlSelect, new { Id = ticketId });

        if (string.IsNullOrEmpty(jsonPayload))
        {
            _logger.LogError("No se encontró el payload para el ticket {TicketId}", ticketId);
            return;
        }

        var request = JsonSerializer.Deserialize<PedidoIntegracionDto>(jsonPayload, _jsonOptions);
        if (request == null) return;

        using var propSociedad = LogContext.PushProperty("Sociedad", request.Sociedad);
        using var propUsuario  = LogContext.PushProperty("Usuario",  request.Usuario);

        _logger.LogInformation("Iniciando procesamiento de Pedido. Ticket: {TicketId}", ticketId);

        // 2. Marcar como "Procesando"
        await UpdateTicketStatusAsync(ticketId, "Procesando", null, null);

        Company?        company = null;
        Documents?      pedido  = null;
        Document_Lines? lineas  = null;

        try
        {
            // 3. Conexión efímera a SAP
            company = _sapConnectionManager.ConnectCompany();
            _logger.LogInformation("Conectado a SAP — Server: {Server} DB: {DB}", company.Server, company.CompanyDB);

            // 4. Objeto Pedido (oOrders)
            pedido = (Documents)company.GetBusinessObject(BoObjectTypes.oOrders);

            // 5. Cabecera
            pedido.CardCode   = request.CardCode;
            pedido.DocDate    = DateTime.Now;
            pedido.DocDueDate = request.DocDueDate;
            pedido.Comments   = request.Comments;

            SetUserField(pedido.UserFields, "U_OrderNo",  request.U_NumPed);
            SetUserField(pedido.UserFields, "U_PortalUser", request.CardCode);
            SetUserField(pedido.UserFields, "U_Route",       request.Uruta);

            // 6. Líneas
            lineas = pedido.Lines;

            for (int i = 0; i < request.DocumentLines.Count; i++)
            {
                var lineaDto = request.DocumentLines[i];

                if (i > 0) lineas.Add();
                lineas.SetCurrentLine(i);

                lineas.ItemCode  = lineaDto.ItemCode;
                lineas.Price     = lineaDto.PrecioUnitario;
                lineas.Quantity  = lineaDto.Quantity;
                lineas.VatGroup  = lineaDto.VatGroup;
                lineas.Currency  = lineaDto.Currency == "$" ? "MXP" : lineaDto.Currency;

                // Descuento combinado: cabecera + línea
                double desc1         = 100 - request.Discount;
                double desc2         = 100 - lineaDto.PrtgDiscount;
                double promDesc      = (desc1 * desc2) / 100;
                double totalDescuento = 100 - promDesc;

                lineas.DiscountPercent = totalDescuento;

                SetUserField(lineas.UserFields, "U_Disc1", request.Discount);
                SetUserField(lineas.UserFields, "U_Disc2", lineaDto.PrtgDiscount);
            }

            // 7. Guardar en SAP
            int result = pedido.Add();

            if (result != 0)
            {
                company.GetLastError(out int errorCode, out string errorMsg);
                string fullError = $"Error SAP [{errorCode}]: {errorMsg}";
                _logger.LogError("Fallo al crear pedido en SAP. Ticket: {TicketId}. Error: {Error}", ticketId, fullError);
                await UpdateTicketStatusAsync(ticketId, "Error", null, fullError);
            }
            else
            {
                string docEntryStr = company.GetNewObjectKey();
                int    docNum      = GetDocNumFromDocEntry(company, docEntryStr, BoObjectTypes.oOrders);
                _logger.LogInformation("Pedido creado en SAP. DocNum: {DocNum}. Ticket: {TicketId}", docNum, ticketId);
                await UpdateTicketStatusAsync(ticketId, "Completado", docNum, null, int.Parse(docEntryStr));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción crítica procesando el Pedido. Ticket: {TicketId}", ticketId);
            await UpdateTicketStatusAsync(ticketId, "Error", null, ex.Message);
            throw; // Re-throw para que Hangfire registre el fallo correctamente
        }
        finally
        {
            // Liberar objetos COM en orden inverso
            if (lineas  != null) Marshal.ReleaseComObject(lineas);
            if (pedido  != null) Marshal.ReleaseComObject(pedido);
            _sapConnectionManager.DisconnectCompany(company);
        }
    }

    #endregion

    #region Procesamiento de Entregas (Rastreo)

    public async Task ProcessEntregaAsync(Guid ticketId)
    {
        var jsonPayload = await _dbConnection.QuerySingleOrDefaultAsync<string>(
            "SELECT Payload FROM IntegrationTickets WHERE Id = @Id", new { Id = ticketId });

        if (string.IsNullOrEmpty(jsonPayload)) return;

        var request = JsonSerializer.Deserialize<OrdenEntregaRequest>(jsonPayload, _jsonOptions);
        if (request == null) return;

        using var propSociedad = LogContext.PushProperty("Sociedad", request.Sociedad);
        using var propUsuario  = LogContext.PushProperty("Usuario",  request.Usuario);

        _logger.LogInformation("Iniciando procesamiento de Entrega. Ticket: {TicketId}. DocEntryPedido: {DocEntry}",
            ticketId, request.DocEntryPedido);

        // Validar que haya al menos una línea marcada como entregada
        var lineasAEntregar = request.Detalles
            .Where(d => d.Entregado && d.CantidadEditada > 0)
            .ToList();

        if (lineasAEntregar.Count == 0)
        {
            const string msgVacio = "No hay líneas marcadas como entregadas con cantidad > 0. No se generó entrega en SAP.";
            _logger.LogWarning(msgVacio + " Ticket: {TicketId}", ticketId);
            await UpdateTicketStatusAsync(ticketId, "Error", null, msgVacio);
            return;
        }

        await UpdateTicketStatusAsync(ticketId, "Procesando", null, null);

        Company?   company      = null;
        Documents? pedido       = null;
        Documents? ordenEntrega = null;

        try
        {
            company = _sapConnectionManager.ConnectCompany();
            _logger.LogInformation("[SAP] Conectado — Server: {Server}, DB: {DB}, License: {License}",
                company.Server, company.CompanyDB, company.LicenseServer);

            // 1. Validar que el pedido exista en SAP y obtener el CardCode
            pedido = (Documents)company.GetBusinessObject(BoObjectTypes.oOrders);
            if (!pedido.GetByKey(request.DocEntryPedido))
            {
                throw new Exception($"No se encontró el pedido con DocEntry: {request.DocEntryPedido}");
            }

            string cardCode = pedido.CardCode;
            _logger.LogInformation("[SAP] Pedido encontrado. Cliente: {CardCode}, Líneas totales: {Lineas}",
                cardCode, pedido.Lines.Count);

            // Liberar el objeto pedido — se vuelve a instanciar fresco después del Add()
            Marshal.ReleaseComObject(pedido);
            pedido = null;

            // 2. Iniciar transacción SAP
            company.StartTransaction();

            // 3. Crear la Orden de Entrega
            ordenEntrega          = (Documents)company.GetBusinessObject(BoObjectTypes.oDeliveryNotes);
            ordenEntrega.CardCode = cardCode;
            ordenEntrega.DocDate  = DateTime.Now;

            // UDF en el header de la entrega (igual que el original)
            SetUserField(ordenEntrega.UserFields, "U_UsuarioEntrego", request.NombreRepartidor);

            bool primeraLinea = true;

            foreach (var detalle in lineasAEntregar)
            {
                if (!primeraLinea)
                    ordenEntrega.Lines.Add();

                // Mapeo exacto al IntegracionSAPController original (líneas 186-190)
                ordenEntrega.Lines.BaseType  = (int)BoObjectTypes.oOrders; // 17
                ordenEntrega.Lines.BaseEntry = request.DocEntryPedido;
                ordenEntrega.Lines.BaseLine  = detalle.Linea;
                ordenEntrega.Lines.Quantity  = detalle.CantidadEditada;
                ordenEntrega.Lines.ItemCode  = detalle.ItemCode;

                primeraLinea = false;
            }

            // 4. Guardar Orden de Entrega
            if (ordenEntrega.Add() != 0)
            {
                int    sapCode = company.GetLastErrorCode();
                string sapDesc = company.GetLastErrorDescription();
                throw new Exception($"Error SAP al generar la orden de entrega [{sapCode}]: {sapDesc}");
            }

            string docEntryEntregaStr = company.GetNewObjectKey();
            int    docEntryEntrega    = int.Parse(docEntryEntregaStr);
            int    docNumEntrega      = GetDocNumFromDocEntry(company, docEntryEntregaStr, BoObjectTypes.oDeliveryNotes);
            _logger.LogInformation("[SAP] Orden de entrega generada. DocEntry: {DocEntry}, DocNum: {DocNum}",
                docEntryEntrega, docNumEntrega);

            // 5. Cerrar/actualizar el pedido original (método separado, fiel al original)
            bool pedidoCerrado = CerrarPedidoEnSAP(company, request.DocEntryPedido, request.Detalles, request.NombreRepartidor);

            if (pedidoCerrado)
                _logger.LogInformation("[SAP] Pedido cerrado correctamente. DocEntry: {DocEntry}", request.DocEntryPedido);
            else
                _logger.LogInformation("[SAP] Pedido actualizado pero sigue abierto (entrega parcial). DocEntry: {DocEntry}", request.DocEntryPedido);

            // 6. Confirmar transacción
            company.EndTransaction(BoWfTransOpt.wf_Commit);
            _logger.LogInformation("[SAP] Transacción confirmada (Commit). Ticket: {TicketId}", ticketId);

            await UpdateTicketStatusAsync(ticketId, "Completado", docNumEntrega, null, docEntryEntrega);
        }
        catch (Exception ex)
        {
            // Revertir transacción SAP si está activa
            try
            {
                if (company != null && company.InTransaction)
                {
                    company.EndTransaction(BoWfTransOpt.wf_RollBack);
                    _logger.LogWarning("[SAP] Transacción revertida (Rollback). Ticket: {TicketId}", ticketId);
                }
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "[SAP] Error al ejecutar Rollback. Ticket: {TicketId}", ticketId);
            }

            _logger.LogError(ex, "Error crítico procesando la Entrega. Ticket: {TicketId}", ticketId);
            await UpdateTicketStatusAsync(ticketId, "Error", null, ex.Message);
            throw; // Re-throw para que Hangfire registre el fallo y aplique reintentos
        }
        finally
        {
            // Liberación estricta de objetos COM en orden inverso
            if (pedido       != null) Marshal.ReleaseComObject(pedido);
            if (ordenEntrega != null) Marshal.ReleaseComObject(ordenEntrega);
            _sapConnectionManager.DisconnectCompany(company);
        }
    }

    /// <summary>
    /// Actualiza el UDF U_UsuarioEntrego en el pedido y lo cierra si todas las cantidades fueron entregadas.
    /// Lógica fiel al método CerrarPedidoEnSAP del IntegracionSAPController original (Rastreo).
    /// </summary>
    private bool CerrarPedidoEnSAP(Company company, int docEntryPedido,
        List<DetalleEntregaRequest> detalles, string nombreRepartidor)
    {
        Documents? pedido = null;
        try
        {
            pedido = (Documents)company.GetBusinessObject(BoObjectTypes.oOrders);
            if (!pedido.GetByKey(docEntryPedido))
            {
                _logger.LogError("[SAP] CerrarPedidoEnSAP: No se encontró el pedido DocEntry: {DocEntry}", docEntryPedido);
                return false;
            }

            // 1. Asignar UDF al pedido
            SetUserField(pedido.UserFields, "U_UsuarioEntrego", nombreRepartidor);

            // 2. Guardar UDF
            if (pedido.Update() != 0)
            {
                _logger.LogError("[SAP] Error al actualizar el pedido [{Code}]: {Desc}",
                    company.GetLastErrorCode(), company.GetLastErrorDescription());
                return false;
            }

            _logger.LogInformation("[SAP] Pedido actualizado con repartidor {Repartidor}. DocEntry: {DocEntry}",
                nombreRepartidor, docEntryPedido);

            // 3. Verificar si es entrega completa (CantidadPendiente == 0 en TODOS los detalles)
            bool pedidoCompleto = detalles.All(d => d.CantidadPendiente == 0);

            if (pedidoCompleto && pedido.DocumentStatus != BoStatus.bost_Close)
            {
                if (pedido.Close() == 0)
                {
                    _logger.LogInformation("[SAP] Pedido cerrado correctamente. DocEntry: {DocEntry}", docEntryPedido);
                    return true;
                }
                else
                {
                    _logger.LogError("[SAP] Error al cerrar el pedido [{Code}]: {Desc}",
                        company.GetLastErrorCode(), company.GetLastErrorDescription());
                    return false;
                }
            }

            // Ya estaba cerrado por SAP automáticamente
            if (pedido.DocumentStatus == BoStatus.bost_Close)
                return true;

            // Entrega parcial — pedido sigue abierto
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SAP] Excepción en CerrarPedidoEnSAP. DocEntry: {DocEntry}", docEntryPedido);
            return false;
        }
        finally
        {
            if (pedido != null) Marshal.ReleaseComObject(pedido);
        }
    }

    #endregion

    #region Métodos Auxiliares (Helpers)

    /// <summary>
    /// Actualiza el estado del ticket en SQL Server usando Dapper.
    /// </summary>
    private async Task UpdateTicketStatusAsync(Guid ticketId, string status, int? sapDocNum, string? errorMessage, int? sapDocEntry = null)
    {
        const string sqlUpdate = @"
            UPDATE IntegrationTickets 
            SET Status        = @Status, 
                SapDocNum     = @SapDocNum, 
                SapDocEntry   = @SapDocEntry,
                ErrorMessage  = @ErrorMessage, 
                LastUpdatedAt = @LastUpdatedAt 
            WHERE Id = @Id";

        await _dbConnection.ExecuteAsync(sqlUpdate, new
        {
            Id            = ticketId,
            Status        = status,
            SapDocNum     = sapDocNum,
            SapDocEntry   = sapDocEntry,
            ErrorMessage  = errorMessage,
            LastUpdatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Asigna un Campo Definido por el Usuario (UDF) y destruye los objetos COM intermedios
    /// para evitar memory leaks en la DI-API.
    /// </summary>
    private void SetUserField(UserFields userFields, string fieldName, object value)
    {
        Fields? fields = null;
        Field?  field  = null;
        try
        {
            fields      = userFields.Fields;
            field       = fields.Item(fieldName);
            field.Value = value;
        }
        catch (Exception ex)
        {
            // Un UDF que no existe no debe tirar todo el proceso
            _logger.LogWarning("No se pudo asignar UDF '{FieldName}': {Error}", fieldName, ex.Message);
        }
        finally
        {
            if (field  != null) Marshal.ReleaseComObject(field);
            if (fields != null) Marshal.ReleaseComObject(fields);
        }
    }

    /// <summary>
    /// Obtiene el DocNum a partir del DocEntry sin usar Recordset (más seguro y rápido).
    /// </summary>
    private int GetDocNumFromDocEntry(Company company, string docEntry, BoObjectTypes objectType)
    {
        Documents? tempDoc = null;
        try
        {
            tempDoc = (Documents)company.GetBusinessObject(objectType);
            if (tempDoc.GetByKey(Convert.ToInt32(docEntry)))
            {
                return tempDoc.DocNum;
            }
            return 0;
        }
        finally
        {
            if (tempDoc != null) Marshal.ReleaseComObject(tempDoc);
        }
    }

    #endregion
}
