using Dapper;
using ERP.API.Models.CompanySetup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ERP.API.Services.CompanySetup;

namespace ERP.API.Services.Implementation.CompanySetup
{
    public class CsChartOfAccountService : ICsChartOfAccountService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CsChartOfAccountService> _logger;
        private readonly string _connectionString;

        public CsChartOfAccountService(IConfiguration configuration, ILogger<CsChartOfAccountService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");
        }

        public async Task<IEnumerable<CsChartOfAccount>> GetAllAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var results = await connection.QueryAsync<CsChartOfAccount>(
                    "SELECT * FROM sp_getall_cs_chart_of_accounts()");

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all chart of accounts");
                throw;
            }
        }

        public async Task<CsChartOfAccount> CreateChartOfAccountAsync(CsChartOfAccount chartOfAccount)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                int? parentAccountId = chartOfAccount.ParentAccountId;
                if (parentAccountId.HasValue)
                {
                    var parentExists = await connection.ExecuteScalarAsync<bool>(
                        "SELECT EXISTS (SELECT 1 FROM cs_chart_of_accounts WHERE account_id = @ParentAccountId)",
                        new { ParentAccountId = parentAccountId.Value });
                    if (!parentExists)
                    {
                        // Parent does not exist, set to null
                        parentAccountId = null;
                    }
                }

                var parameters = new
                {
                    p_company_id = chartOfAccount.CompanyId,
                    p_parent_account_id = parentAccountId,
                    p_account_code = chartOfAccount.AccountCode,
                    p_account_name = chartOfAccount.AccountName,
                    p_account_type = chartOfAccount.AccountType,
                    p_is_active = chartOfAccount.IsActive,
                    p_cost_centre_allocation_required = chartOfAccount.CostCentreAllocationRequired
                };

                // Call the function that returns the full record
                var result = await connection.QuerySingleAsync<CsChartOfAccount>(
                    "SELECT * FROM sp_create_cs_chart_of_account(@p_company_id, @p_parent_account_id, @p_account_code, @p_account_name, @p_account_type, @p_is_active, @p_cost_centre_allocation_required)",
                    parameters);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chart of account");
                throw;
            }
        }

        public async Task<CsChartOfAccount> UpdateChartOfAccountAsync(int accountId, CsChartOfAccount chartOfAccount)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_account_id = accountId,
                    p_company_id = chartOfAccount.CompanyId,
                    p_parent_account_id = chartOfAccount.ParentAccountId,
                    p_account_code = chartOfAccount.AccountCode,
                    p_account_name = chartOfAccount.AccountName,
                    p_account_type = chartOfAccount.AccountType,
                    p_is_active = chartOfAccount.IsActive,
                    p_cost_centre_allocation_required = chartOfAccount.CostCentreAllocationRequired
                };

                var result = await connection.QuerySingleOrDefaultAsync<CsChartOfAccount>(
                    "SELECT * FROM sp_update_cs_chart_of_account(@p_account_id, @p_company_id, @p_parent_account_id, @p_account_code, @p_account_name, @p_account_type, @p_is_active, @p_cost_centre_allocation_required)",
                    parameters);

                return result ?? throw new KeyNotFoundException($"Chart of account with ID {accountId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating chart of account with ID {AccountId}", accountId);
                throw;
            }
        }

        public async Task<bool> DeleteChartOfAccountAsync(int accountId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Use the safe delete stored procedure
                var results = await connection.QueryAsync<dynamic>(
                    "SELECT * FROM sp_delete_cs_chart_of_account(@p_account_id)",
                    new { p_account_id = accountId });

                var result = results.FirstOrDefault();
                
                if (result == null)
                {
                    throw new InvalidOperationException("Unexpected error: No result from delete operation");
                }

                bool success = result.success;
                string message = result.message;
                int childCount = result.child_count;

                if (!success)
                {
                    if (message.Contains("not found"))
                    {
                        return false; // Account doesn't exist
                    }
                    else
                    {
                        // Business rule violation (child accounts exist, foreign key constraints, etc.)
                        throw new InvalidOperationException(message);
                    }
                }

                return true; // Successfully deleted
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw our custom validation exceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting chart of account with ID {AccountId}", accountId);
                throw;
            }
        }

        public async Task<CsChartOfAccount> GetChartOfAccountByIdAsync(int accountId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new { p_account_id = accountId };

                var result = await connection.QuerySingleOrDefaultAsync<CsChartOfAccount>(
                    "SELECT * FROM sp_get_cs_chart_of_account_by_id(@p_account_id)",
                    parameters);

                return result ?? throw new KeyNotFoundException($"Chart of account with ID {accountId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chart of account by ID {AccountId}", accountId);
                throw;
            }
        }

        public async Task<CsChartOfAccountPagedResponse> GetChartOfAccountsByCompanyAsync(CsChartOfAccountSearchRequest searchRequest)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_company_id = searchRequest.CompanyId,
                    p_search_text = searchRequest.SearchText,
                    p_account_type = searchRequest.AccountType,
                    p_is_active = searchRequest.IsActive,
                    p_page_number = searchRequest.PageNumber,
                    p_page_size = searchRequest.PageSize
                };

                var results = await connection.QueryAsync<dynamic>(
                    "SELECT * FROM sp_get_cs_chart_of_accounts_by_company(@p_company_id, @p_search_text, @p_account_type, @p_is_active, @p_page_number, @p_page_size)",
                    parameters);

                var resultList = results.ToList();
                
                if (!resultList.Any())
                {
                    return new CsChartOfAccountPagedResponse
                    {
                        Items = new List<CsChartOfAccount>(),
                        TotalCount = 0,
                        PageNumber = searchRequest.PageNumber,
                        PageSize = searchRequest.PageSize
                    };
                }

                return new CsChartOfAccountPagedResponse
                {
                    Items = resultList.Select(r => new CsChartOfAccount
                    {
                        AccountId = r.account_id,
                        CompanyId = r.company_id,
                        ParentAccountId = r.parent_account_id,
                        AccountCode = r.account_code,
                        AccountName = r.account_name,
                        AccountType = r.account_type,
                        IsActive = r.is_active,
                        CostCentreAllocationRequired = r.cost_centre_allocation_required,
                        CreatedAt = r.created_at,
                        UpdatedAt = r.updated_at
                    }),
                    TotalCount = Convert.ToInt32(((dynamic)resultList.First()).total_count),
                    PageNumber = searchRequest.PageNumber,
                    PageSize = searchRequest.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chart of accounts by company {CompanyId}", searchRequest.CompanyId);
                throw;
            }
        }

        public async Task<IEnumerable<CsChartOfAccountDto>> GetChartOfAccountsHierarchyAsync(int companyId, bool includeInactive = false)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_company_id = companyId,
                    p_include_inactive = includeInactive
                };

                var results = await connection.QueryAsync<CsChartOfAccountDto>(
                    "SELECT * FROM sp_get_cs_chart_of_accounts_hierarchy(@p_company_id, @p_include_inactive)",
                    parameters);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chart of accounts hierarchy for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<IEnumerable<CsChartOfAccountDropdownItem>> GetChartOfAccountsDropdownAsync(int companyId, string? accountType = null)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_company_id = companyId,
                    p_account_type = accountType
                };

                var results = await connection.QueryAsync<CsChartOfAccountDropdownItem>(
                    "SELECT * FROM sp_get_cs_chart_of_accounts_dropdown(@p_company_id, @p_account_type)",
                    parameters);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chart of accounts dropdown for company {CompanyId}", companyId);
                throw;
            }
        }
    }
}
