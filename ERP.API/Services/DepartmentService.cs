using Dapper;
using ERP.API.Models;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP.API.Services
{
    public class DepartmentService
    {
        private readonly string _connectionString;

        public DepartmentService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> CreateAsync(Department department, int userId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"INSERT INTO departments (user_created, date_created, name, head_of_department) 
                       VALUES (@userId, NOW(), @Name, @HeadOfDepartment) RETURNING id";
            return await connection.QuerySingleAsync<int>(sql, new { userId, department.Name, department.HeadOfDepartment });
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = "SELECT * FROM departments ORDER BY id";
            return await connection.QueryAsync<Department>(sql);
        }

        public async Task<Department> GetByIdAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = "SELECT * FROM departments WHERE id = @id";
            return await connection.QuerySingleOrDefaultAsync<Department>(sql, new { id });
        }

        public async Task<bool> UpdateAsync(int id, Department department, int userId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"UPDATE departments SET user_updated = @userId, date_updated = NOW(), 
                       name = @Name, head_of_department = @HeadOfDepartment WHERE id = @id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { id, userId, department.Name, department.HeadOfDepartment });
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = "DELETE FROM departments WHERE id = @id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { id });
            return rowsAffected > 0;
        }
    }
}