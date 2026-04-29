using System.Threading.Tasks;
using Dapper;
using Npgsql;

namespace ERP.API.Services.Common
{
    public class DbService : IDbService
    {
        private readonly string _connectionString;

        public DbService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object parameters = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
        }

        public async Task<T[]> QueryAsync<T>(string sql, object parameters = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var result = await connection.QueryAsync<T>(sql, parameters);
            return result.ToArray();
        }

        public async Task<int> ExecuteAsync(string sql, object parameters = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.ExecuteAsync(sql, parameters);
        }
    }
}
