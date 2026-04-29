using Dapper;
using ERP.API.Models;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP.API.Services
{
    public class DesignationService
    {
        private readonly string _connectionString;

        public DesignationService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> CreateAsync(Designation designation, int userId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"INSERT INTO designation (user_created, date_created, code, name) 
                       VALUES (@userId, NOW(), @Code, @Name) RETURNING id";
            return await connection.QuerySingleAsync<int>(sql, new { userId, designation.Code, designation.Name });
        }

        public async Task<IEnumerable<Designation>> GetAllAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = "SELECT * FROM designation ORDER BY id";
            return await connection.QueryAsync<Designation>(sql);
        }

        public async Task<Designation> GetByIdAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = "SELECT * FROM designation WHERE id = @id";
            return await connection.QuerySingleOrDefaultAsync<Designation>(sql, new { id });
        }

        public async Task<bool> UpdateAsync(int id, Designation designation, int userId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"UPDATE designation SET user_updated = @userId, date_updated = NOW(), 
                       code = @Code, name = @Name WHERE id = @id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { id, userId, designation.Code, designation.Name });
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = "DELETE FROM designation WHERE id = @id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { id });
            return rowsAffected > 0;
        }
    }
}