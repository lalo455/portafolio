using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Encuesta.Core.Entities;
using Encuesta.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Encuesta.Infrastructure.Data
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly string _connectionString;

        public QuestionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<Question>> GetBySurveyIdAsync(int surveyId)
        {
            using var connection = CreateConnection();
            var sql = "SELECT * FROM Question WHERE SurveyId = @SurveyId ORDER BY [Order] ASC, Id ASC";
            return await connection.QueryAsync<Question>(sql, new { SurveyId = surveyId });
        }

        public async Task<Question?> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();
            var sql = "SELECT * FROM Question WHERE Id = @Id";
            return await connection.QuerySingleOrDefaultAsync<Question>(sql, new { Id = id });
        }

        public async Task<int> AddAsync(Question question)
        {
            using var connection = CreateConnection();
            var sql = @"
                INSERT INTO Question (SurveyId, Text, Type, [Order]) 
                VALUES (@SurveyId, @Text, @Type, @Order);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            return await connection.ExecuteScalarAsync<int>(sql, question);
        }

        public async Task UpdateAsync(Question question)
        {
            using var connection = CreateConnection();
            var sql = "UPDATE Question SET Text = @Text, Type = @Type, [Order] = @Order WHERE Id = @Id";
            await connection.ExecuteAsync(sql, question);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = CreateConnection();
            var sql = "DELETE FROM Question WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<int> AddOptionAsync(QuestionOption option)
        {
            using var connection = CreateConnection();
            var sql = @"
                INSERT INTO QuestionOption (QuestionId, Value, Label, [Order]) 
                VALUES (@QuestionId, @Value, @Label, @Order);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            return await connection.ExecuteScalarAsync<int>(sql, option);
        }

        public async Task<IEnumerable<QuestionOption>> GetOptionsByQuestionIdAsync(int questionId)
        {
            using var connection = CreateConnection();
            var sql = "SELECT * FROM QuestionOption WHERE QuestionId = @QuestionId ORDER BY [Order] ASC, Id ASC";
            return await connection.QueryAsync<QuestionOption>(sql, new { QuestionId = questionId });
        }

        public async Task DeleteOptionsByQuestionIdAsync(int questionId)
        {
            using var connection = CreateConnection();
            var sql = "DELETE FROM QuestionOption WHERE QuestionId = @QuestionId";
            await connection.ExecuteAsync(sql, new { QuestionId = questionId });
        }
    }
}
