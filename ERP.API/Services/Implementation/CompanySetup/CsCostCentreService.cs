using Dapper;
using ERP.API.Models.CompanySetup;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using ERP.API.Services.CompanySetup;

namespace ERP.API.Services.Implementation.CompanySetup
{
    public class CsCostCentreService : ICsCostCentreService
    {
        private readonly string _connectionString;
        private readonly ILogger<CsCostCentreService> _logger;

        public CsCostCentreService(string connectionString, ILogger<CsCostCentreService> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<CsCostCentre> CreateCostCentreAsync(int companyId, CsCostCentreDto createDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var result = await connection.QuerySingleAsync<CsCostCentre>(
                    "SELECT * FROM sp_create_cs_cost_centre(@CompanyId, @ParentCostCentreId, @CostCentreCode, @CostCentreName, @IsActive)",
                    new
                    {
                        CompanyId = companyId,
                        createDto.ParentCostCentreId,
                        createDto.CostCentreCode,
                        createDto.CostCentreName,
                        createDto.IsActive
                    });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cost centre");
                throw;
            }
        }

        public async Task<CsCostCentre> UpdateCostCentreAsync(int costCentreId, CsCostCentreDto updateDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var result = await connection.QuerySingleAsync<CsCostCentre>(@"
                    UPDATE public.cs_cost_centres 
                    SET 
                        parent_cost_centre_id = @ParentCostCentreId, 
                        cost_centre_code = @CostCentreCode, 
                        cost_centre_name = @CostCentreName, 
                        is_active = @IsActive, 
                        updated_at = CURRENT_TIMESTAMP 
                    WHERE cost_centre_id = @CostCentreId 
                    RETURNING cost_centre_id AS CostCentreId, company_id AS CompanyId, parent_cost_centre_id AS ParentCostCentreId, cost_centre_code AS CostCentreCode, cost_centre_name AS CostCentreName, is_active AS IsActive, created_at AS CreatedAt, updated_at AS UpdatedAt",
                    new
                    {
                        CostCentreId = costCentreId,
                        updateDto.ParentCostCentreId,
                        updateDto.CostCentreCode,
                        updateDto.CostCentreName,
                        updateDto.IsActive
                    });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cost centre");
                throw;
            }
        }

        // public async Task<bool> DeleteCostCentreAsync(int costCentreId)
        // {
        //     try
        //     {
        //         using var connection = new NpgsqlConnection(_connectionString);
        //         return await connection.QuerySingleAsync<bool>(
        //             "SELECT sp_delete_cs_cost_centre(@CostCentreId)",
        //             new { CostCentreId = costCentreId });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error deleting cost centre");
        //         throw;
        //     }
        // }

        public async Task<bool> DeleteCostCentreAsync(int costCentreId)
{
    try
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleAsync<bool>(
            "SELECT sp_delete_cs_cost_centre(@CostCentreId)",
            new { CostCentreId = costCentreId });
    }
    catch (PostgresException ex) when (ex.SqlState == "P0001")
    {
        // Handle business rule violations gracefully
        _logger.LogWarning("Cannot delete cost centre {CostCentreId}: {Message}", costCentreId, ex.MessageText);
        return false; // Or throw a custom business exception
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error deleting cost centre");
        throw;
    }
}

        public async Task<CsCostCentre?> GetCostCentreByIdAsync(int costCentreId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.QuerySingleOrDefaultAsync<CsCostCentre>(
                    "SELECT * FROM sp_get_cs_cost_centre_by_id(@CostCentreId)",
                    new { CostCentreId = costCentreId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cost centre by ID");
                throw;
            }
        }

        public async Task<CsCostCentrePagedResponse> GetCostCentresByCompanyAsync(int companyId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var results = await connection.QueryAsync<CsCostCentre>(
                    "SELECT * FROM sp_get_cs_cost_centres_by_company(@CompanyId, @PageNumber, @PageSize)",
                    new { CompanyId = companyId, PageNumber = pageNumber, PageSize = pageSize });            var resultList = results.ToList();
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
            return new CsCostCentrePagedResponse
            {
                Items = resultList,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cost centres by company");
                throw;
            }
        }

        public async Task<CsCostCentrePagedResponse> SearchCostCentresAsync(int companyId, CsCostCentreSearchRequest searchRequest)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var results = await connection.QueryAsync<CsCostCentre>(
                    "SELECT * FROM sp_search_cs_cost_centres(@CompanyId, @SearchText, @IsActive, @ParentCostCentreId, @PageNumber, @PageSize)",
                    new
                    {
                        CompanyId = companyId,
                        searchRequest.SearchText,
                        searchRequest.IsActive,
                        searchRequest.ParentCostCentreId,
                        searchRequest.PageNumber,
                        searchRequest.PageSize
                    });

                var resultList = results.ToList();
                return new CsCostCentrePagedResponse
                {
                    Items = resultList,
                    TotalCount = resultList.Any() ? Convert.ToInt32(((dynamic)resultList.First()).total_count) : 0,
                    PageNumber = searchRequest.PageNumber,
                    PageSize = searchRequest.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching cost centres");
                throw;
            }
        }

        public async Task<List<CsCostCentreHierarchyItem>> GetCostCentreHierarchyAsync(int companyId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var results = await connection.QueryAsync<CsCostCentreHierarchyItem>(
                    "SELECT * FROM sp_get_cs_cost_centre_hierarchy(@CompanyId)",
                    new { CompanyId = companyId });
                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cost centre hierarchy");
                throw;
            }
        }

        public async Task<List<CsCostCentreDropdownItem>> GetCostCentresDropdownAsync(int companyId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var results = await connection.QueryAsync<CsCostCentreDropdownItem>(
                    "CALL sp_cs_cost_centres_get_dropdown(@CompanyId)",
                    new { CompanyId = companyId });
                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cost centres dropdown");
                throw;
            }
        }

        public async Task<List<CsCostCentre>> GetAllCostCentresAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var costCentres = await connection.QueryAsync<CsCostCentre>("SELECT * FROM sp_getall_cs_cost_centres_with_details()");
                return costCentres.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all cost centres");
                throw;
            }
        }
    }
}
