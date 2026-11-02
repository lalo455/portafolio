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
    public class ResponseRepository : IResponseRepository
    {
        private readonly string _connectionString;

        public ResponseRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<int> AddResponseAsync(SurveyResponse response)
        {
            using var connection = CreateConnection();
            var sql = @"
                INSERT INTO SurveyResponse (SurveyId, SubmittedAt) 
                VALUES (@SurveyId, @SubmittedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            return await connection.ExecuteScalarAsync<int>(sql, response);
        }

        public async Task AddAnswerAsync(Answer answer)
        {
            using var connection = CreateConnection();
            var sql = @"
                INSERT INTO Answer (SurveyResponseId, QuestionId, SelectedOptionId, TextValue) 
                VALUES (@SurveyResponseId, @QuestionId, @SelectedOptionId, @TextValue);";
            await connection.ExecuteAsync(sql, answer);
        }

        public async Task<IEnumerable<SurveyResponse>> GetResponsesBySurveyIdAsync(int surveyId)
        {
            using var connection = CreateConnection();
            var sql = "SELECT * FROM SurveyResponse WHERE SurveyId = @SurveyId ORDER BY SubmittedAt DESC";
            return await connection.QueryAsync<SurveyResponse>(sql, new { SurveyId = surveyId });
        }

        public async Task<IEnumerable<Answer>> GetAnswersBySurveyIdAsync(int surveyId)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT a.* 
                FROM Answer a
                INNER JOIN SurveyResponse sr ON a.SurveyResponseId = sr.Id
                WHERE sr.SurveyId = @SurveyId";
            return await connection.QueryAsync<Answer>(sql, new { SurveyId = surveyId });
        }
    }
}
