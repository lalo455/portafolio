using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;
using Encuesta.Core.Entities;
using Encuesta.Core.Interfaces;

namespace Encuesta.Infrastructure.Data
{
    public class SurveyRepository : ISurveyRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public SurveyRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") 
                ?? throw new System.ArgumentNullException("DefaultConnection");
        }

        public async Task<IEnumerable<Survey>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Survey>("SELECT * FROM Survey ORDER BY CreatedAt DESC");
        }

        public async Task<Survey?> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Survey>(
                "SELECT * FROM Survey WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> AddAsync(Survey survey)
        {
            var sql = @"
                INSERT INTO Survey (Title, Description, IsActive, CreatedAt)
                VALUES (@Title, @Description, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<int>(sql, survey);
        }

        public async Task UpdateAsync(Survey survey)
        {
            var sql = @"
                UPDATE Survey 
                SET Title = @Title, Description = @Description, IsActive = @IsActive 
                WHERE Id = @Id";

            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, survey);
        }

        public async Task DeactivateAsync(int id)
        {
            var sql = "UPDATE Survey SET IsActive = 0 WHERE Id = @Id";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
