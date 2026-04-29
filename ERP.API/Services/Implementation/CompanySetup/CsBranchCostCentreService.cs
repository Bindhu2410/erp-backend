using Dapper;
using ERP.API.Models.CompanySetup;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using ERP.API.Services.CompanySetup;

namespace ERP.API.Services.Implementation.CompanySetup
{
    public class CsBranchCostCentreService : ICsBranchCostCentreService
    {
        private readonly string _connectionString;
        private readonly ILogger<CsBranchCostCentreService> _logger;

        public CsBranchCostCentreService(string connectionString, ILogger<CsBranchCostCentreService> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<CsBranchCostCentre> CreateBranchCostCentreAsync(int branchId, int costCentreId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var result = await connection.QuerySingleAsync<CsBranchCostCentre>(
                    "SELECT * FROM sp_create_cs_branch_cost_centre(@BranchId, @CostCentreId)",
                    new { BranchId = branchId, CostCentreId = costCentreId });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating branch cost centre mapping");
                throw;
            }
        }

        public async Task<bool> DeleteBranchCostCentreAsync(int branchId, int costCentreId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.QuerySingleAsync<bool>(
                    "SELECT * FROM sp_delete_cs_branch_cost_centre(@BranchId, @CostCentreId)",
                    new { BranchId = branchId, CostCentreId = costCentreId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting branch cost centre mapping");
                throw;
            }
        }

        public async Task<IEnumerable<CsBranchCostCentreDetail>> GetCostCentresByBranchAsync(int branchId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.QueryAsync<CsBranchCostCentreDetail>(
                    "SELECT * FROM sp_get_cs_cost_centres_by_branch(@BranchId)",
                    new { BranchId = branchId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cost centres by branch");
                throw;
            }
        }

        public async Task<CsBranchCostCentrePagedResponse> GetBranchesByCostCentreAsync(
            int costCentreId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var results = await connection.QueryAsync<CsBranchDetail>(
                    "SELECT * FROM sp_get_cs_branches_by_cost_centre(@CostCentreId, @PageNumber, @PageSize)",
                    new 
                    { 
                        CostCentreId = costCentreId, 
                        PageNumber = pageNumber, 
                        PageSize = pageSize 
                    });            var resultList = results.ToList();
            long totalCount = 0;
            if (resultList.Any())
            {
                var firstItem = resultList.First();
                var itemType = firstItem.GetType();
                var totalCountProp = itemType.GetProperty("total_count");
                if (totalCountProp != null)
                {
                    totalCount = (long)totalCountProp.GetValue(firstItem, null);
                }
            }
            return new CsBranchCostCentrePagedResponse
            {
                Items = resultList,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branches by cost centre");
                throw;
            }
        }

        public async Task<IEnumerable<CsBranchCostCentreDropdownItem>> GetBranchCostCentresDropdownAsync(int branchId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.QueryAsync<CsBranchCostCentreDropdownItem>(
                    "SELECT * FROM sp_get_cs_branch_cost_centres_dropdown(@BranchId)",
                    new { BranchId = branchId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branch cost centres dropdown");
                throw;
            }
        }
    }
}
