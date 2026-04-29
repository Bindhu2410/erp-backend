using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using System.Linq.Expressions;

namespace ERP.API.Services
{
    public abstract class BaseDataService<T> where T : class
    {
        protected readonly string _connectionString;
        protected readonly string _tableName;

        protected BaseDataService(string connectionString, string tableName)
        {
            _connectionString = connectionString;
            _tableName = tableName;
        }

        public NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        protected string GetColumnName(PropertyInfo property)
        {
            var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
            return columnAttr != null ? $"\"{columnAttr.Name}\"" : $"\"{property.Name}\"";
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            using var connection = CreateConnection();
            var result = await connection.QueryAsync<T>($"SELECT * FROM {_tableName}");
            return result ?? Enumerable.Empty<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(string whereClause, object parameters)
        {
            using var connection = CreateConnection();
            var result = await connection.QueryAsync<T>($"SELECT * FROM {_tableName} WHERE {whereClause}", parameters);
            return result ?? Enumerable.Empty<T>();
        }        public virtual async Task<T?> GetByIdAsync(int? id)
        {
            try
            {
                if (!id.HasValue || id.Value <= 0)
                {
                    throw new ArgumentException("ID must be a positive number", nameof(id));
                }

                using var connection = CreateConnection();
                var idProperty = typeof(T).GetProperties()
                    .FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
                
                string idColumnName = "id";
                if (idProperty != null)
                {
                    var columnAttr = idProperty.GetCustomAttribute<ColumnAttribute>();
                    idColumnName = columnAttr?.Name ?? "id";
                }

                var query = $@"
                    SELECT * 
                    FROM {_tableName} 
                    WHERE {idColumnName} = @Id";

                Console.WriteLine($"[DEBUG] GetByIdAsync [{typeof(T).Name}] Query: {query}");
                Console.WriteLine($"[DEBUG] GetByIdAsync [{typeof(T).Name}] Id: {id}");
                  var result = await connection.QueryFirstOrDefaultAsync<T>(
                    query,
                    new { Id = id.Value },
                    commandTimeout: 30
                );
                
                Console.WriteLine($"[DEBUG] GetByIdAsync [{typeof(T).Name}] Result: {(result == null ? "null" : "found")}");
                
                return result;
            }            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetByIdAsync for {typeof(T).Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex is ArgumentException)
                    throw;
                throw new Exception($"Error retrieving {typeof(T).Name} with ID {id}: {ex.Message}", ex);
            }
        }
        //public virtual async Task<int> CreateAsync(T entity)
        //{
        //    using var connection = CreateConnection();
        //    var insertQuery = GenerateInsertQuery();
        //    var parameters = entity.GetType().GetProperties()
        //        .Where(itm => itm.Name.ToLower() != "id" &&
        //            (itm.PropertyType.IsGenericType 
        //                ? itm.PropertyType.GetGenericTypeDefinition().Name
        //                : itm.PropertyType.Name).ToLower() != "list`1")
        //        .ToDictionary(p => p.Name, p => p.GetValue(entity));

        //    var id = await connection.QuerySingleAsync<int>(insertQuery, parameters);
        //    return id;
        //}
        public virtual async Task<int> CreateAsync(T entity)
        {
            using var connection = CreateConnection();
            var insertQuery = GenerateInsertQuery();

            // Get all properties except the excluded ones
            var excludedProperties = new HashSet<string> { "id", "assigneduser" };

            var parameters = entity.GetType().GetProperties()
                .Where(p => !excludedProperties.Contains(p.Name.ToLower()))
                .ToDictionary(p => p.Name, p => p.GetValue(entity));

            var id = await connection.QuerySingleAsync<int>(insertQuery, parameters);
            return id;
        }
        public virtual async Task<bool> DeleteAsync(int id)
        {
            using var connection = CreateConnection();
            var rowsAffected = await connection.ExecuteAsync($"DELETE FROM {_tableName} WHERE id = @Id", new { Id = id });
            return rowsAffected > 0;
        }        protected abstract string GenerateInsertQuery();
        protected abstract string GenerateUpdateQuery();        public virtual async Task<bool> UpdateAsync(T entity)
        {
            using var connection = CreateConnection();
            var updateQuery = GenerateUpdateQuery();
            
            // Filter out navigation properties and collections
            var parameters = entity.GetType().GetProperties()
                .Where(p => 
                    !p.PropertyType.IsClass || 
                    p.PropertyType == typeof(string) || 
                    p.PropertyType == typeof(DateTime) ||
                    p.PropertyType == typeof(DateTime?))
                .Where(p => 
                    !(p.PropertyType.IsGenericType && 
                      p.PropertyType.GetGenericTypeDefinition() == typeof(List<>)))
                .ToDictionary(p => p.Name, p => p.GetValue(entity));

            try 
            {
                var rowsAffected = await connection.ExecuteAsync(updateQuery, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                // Log the error details
                System.Diagnostics.Debug.WriteLine($"Error updating {typeof(T).Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Query: {updateQuery}");
                System.Diagnostics.Debug.WriteLine($"Parameters: {string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"))}");
                throw;
            }
        }
    }
}