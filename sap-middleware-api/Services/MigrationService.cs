// Services/MigrationService.cs
using Dapper;
using System.Data;

namespace SapMiddleware.Services;

/// <summary>
/// Servicio que se ejecuta al arrancar la aplicación y aplica migraciones
/// seguras (IF NOT EXISTS) sobre la tabla IntegrationTickets.
/// No requiere Entity Framework ni herramientas externas.
/// </summary>
public class MigrationService
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<MigrationService> _logger;

    public MigrationService(IDbConnection dbConnection, ILogger<MigrationService> logger)
    {
        _dbConnection = dbConnection;
        _logger       = logger;
    }

    public async Task ApplyMigrationsAsync()
    {
        _logger.LogInformation("[Migration] Verificando esquema de IntegrationTickets...");

        // 1. Crear tabla si no existe (para entornos nuevos / primera instalación)
        await _dbConnection.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                           WHERE TABLE_NAME = 'IntegrationTickets')
            BEGIN
                CREATE TABLE IntegrationTickets (
                    Id                  UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
                    TipoDocumento       NVARCHAR(20)        NULL,
                    ClaveDeduplicacion  NVARCHAR(200)       NULL,
                    Payload             NVARCHAR(MAX)       NOT NULL,
                    Status              NVARCHAR(50)        NOT NULL DEFAULT 'En Cola',
                    SapDocNum           INT                 NULL,
                    ErrorMessage        NVARCHAR(MAX)       NULL,
                    Intentos            INT                 NOT NULL DEFAULT 0,
                    CreatedAt           DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
                    LastUpdatedAt       DATETIME2           NOT NULL DEFAULT GETUTCDATE()
                );
                PRINT 'Tabla IntegrationTickets creada.';
            END");

        // 2. Agregar columna TipoDocumento si no existe
        await _dbConnection.ExecuteAsync(@"
            IF NOT EXISTS (
                SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'IntegrationTickets' AND COLUMN_NAME = 'TipoDocumento'
            )
            BEGIN
                ALTER TABLE IntegrationTickets
                    ADD TipoDocumento NVARCHAR(20) NULL;
                PRINT 'Columna TipoDocumento agregada.';
            END");

        // 3. Agregar columna ClaveDeduplicacion si no existe
        await _dbConnection.ExecuteAsync(@"
            IF NOT EXISTS (
                SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'IntegrationTickets' AND COLUMN_NAME = 'ClaveDeduplicacion'
            )
            BEGIN
                ALTER TABLE IntegrationTickets
                    ADD ClaveDeduplicacion NVARCHAR(200) NULL;
                PRINT 'Columna ClaveDeduplicacion agregada.';
            END");

        // 4. Agregar columna Intentos si no existe
        await _dbConnection.ExecuteAsync(@"
            IF NOT EXISTS (
                SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'IntegrationTickets' AND COLUMN_NAME = 'Intentos'
            )
            BEGIN
                ALTER TABLE IntegrationTickets
                    ADD Intentos INT NOT NULL DEFAULT 0;
                PRINT 'Columna Intentos agregada.';
            END");

        // 5. Agregar columna SapDocEntry si no existe (Necesario para Crystal Reports)
        await _dbConnection.ExecuteAsync(@"
            IF NOT EXISTS (
                SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'IntegrationTickets' AND COLUMN_NAME = 'SapDocEntry'
            )
            BEGIN
                ALTER TABLE IntegrationTickets
                    ADD SapDocEntry INT NULL;
                PRINT 'Columna SapDocEntry agregada.';
            END");

        // 6. Crear índice de performance si no existe
        // Acelera las consultas de deduplicación y el endpoint /health
        await _dbConnection.ExecuteAsync(@"
            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE name = 'IX_IntegrationTickets_Status_CreatedAt'
                  AND object_id = OBJECT_ID('IntegrationTickets')
            )
            BEGIN
                CREATE NONCLUSTERED INDEX IX_IntegrationTickets_Status_CreatedAt
                ON IntegrationTickets (Status, CreatedAt DESC)
                INCLUDE (TipoDocumento, ClaveDeduplicacion, SapDocNum);
                PRINT 'Índice IX_IntegrationTickets_Status_CreatedAt creado.';
            END");

        _logger.LogInformation("[Migration] Esquema verificado y actualizado correctamente.");
    }
}
