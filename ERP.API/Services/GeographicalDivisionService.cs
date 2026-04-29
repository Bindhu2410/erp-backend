using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models;
using ERP.API.Models.DTOs;
using System.Linq;

namespace ERP.API.Services
{
    public class GeographicalDivisionService : IGeographicalDivisionService
    {
        private readonly IDbConnection _connection;

        public GeographicalDivisionService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<GeographicalDivision> GetByIdAsync(long divisionId)
        {
            var parameters = new { p_division_id = divisionId };
            var result = await _connection.QueryAsync<GeographicalDivision>(
                "SELECT * FROM sp_get_geographical_division_by_id(@p_division_id)",
                parameters);

            return result.FirstOrDefault() ?? new GeographicalDivision();
        }

        public async Task<IEnumerable<GeographicalDivision>> GetAllAsync()
        {
            return await _connection.QueryAsync<GeographicalDivision>(
                "SELECT division_id as DivisionId, division_name as DivisionName, " +
                "division_type as DivisionType, parent_division_id as ParentDivisionId, " +
                "created_at as CreatedAt, updated_at as UpdatedAt, " +
                "created_by as CreatedBy, updated_by as UpdatedBy " +
                "FROM public.geographical_divisions");
        }

        public async Task<long> CreateAsync(GeographicalDivision division)
        {
            var parameters = new
            {
                p_division_name = division.DivisionName,
                p_division_type = division.DivisionType,
                p_parent_division_id = division.ParentDivisionId,
                p_created_by = division.CreatedBy
            };

            return await _connection.ExecuteScalarAsync<long>(
                "SELECT sp_create_geographical_division(@p_division_name, @p_division_type, @p_parent_division_id, @p_created_by)",
                parameters);
        }

        public async Task<bool> UpdateAsync(GeographicalDivision division)
        {
            var parameters = new
            {
                p_division_id = division.DivisionId,
                p_division_name = division.DivisionName,
                p_division_type = division.DivisionType,
                p_parent_division_id = division.ParentDivisionId,
                p_updated_by = division.UpdatedBy
            };

            return await _connection.ExecuteScalarAsync<bool>(
                "SELECT sp_update_geographical_division(@p_division_id, @p_division_name, @p_division_type, @p_parent_division_id, @p_updated_by)",
                parameters);
        }

        public async Task<bool> DeleteAsync(long divisionId)
        {
            var parameters = new { p_division_id = divisionId };
            return await _connection.ExecuteScalarAsync<bool>(
                "SELECT sp_delete_geographical_division(@p_division_id)",
                parameters);
        }        public async Task<IEnumerable<GeographicalHierarchyDto>> GetHierarchyByPincodeAsync(string pincode)
        {
            try
            {
                var parameters = new { p_pincode = pincode };
                var sql = @"SELECT 
                    h.division_id as DivisionId, 
                    h.parent_division_id as ParentDivisionId, 
                    h.division_name as DivisionName, 
                    h.division_type as DivisionType, 
                    h.level as Level 
                FROM sp_get_geographical_hierarchy_by_pincode(@p_pincode) h";
                
                return await _connection.QueryAsync<GeographicalHierarchyDto>(sql, parameters);
            }
            catch (System.Exception ex)
            {
                // Add exception handling to get more details
                System.Console.WriteLine($"Error in GetHierarchyByPincodeAsync: {ex.Message}");
                throw;
            }
        }
    }
}
