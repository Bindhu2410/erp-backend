using Dapper;
using ERP.API.Models.CompanySetup;
using ERP.API.Services.CompanySetup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace ERP.API.Services.Implementation.CompanySetup
{
    public class CsDefaultAccountMappingService : ICsDefaultAccountMappingService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CsDefaultAccountMappingService> _logger;

        public CsDefaultAccountMappingService(IConfiguration configuration, ILogger<CsDefaultAccountMappingService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<CsDefaultAccountMapping> CreateDefaultAccountMappingAsync(CsDefaultAccountMappingRequest request)
        {
            try
            {
                using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("p_company_id", request.CompanyId);
                parameters.Add("p_transaction_type", request.TransactionType);
                parameters.Add("p_default_debit_account_id", request.DefaultDebitAccountId);
                parameters.Add("p_default_credit_account_id", request.DefaultCreditAccountId);
                parameters.Add("p_mapping_id", dbType: DbType.Int32, direction: ParameterDirection.InputOutput);

                await connection.ExecuteAsync(
                    "CALL sp_create_cs_default_account_mapping(@p_company_id, @p_transaction_type, @p_default_debit_account_id, @p_default_credit_account_id, @p_mapping_id)",
                    parameters);

                var mappingId = parameters.Get<int>("p_mapping_id");
                return await GetDefaultAccountMappingByIdAsync(mappingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default account mapping");
                throw;
            }
        }

        public async Task<CsDefaultAccountMapping> UpdateDefaultAccountMappingAsync(int mappingId, CsDefaultAccountMappingRequest request)
        {
            try
            {
                using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("p_mapping_id", mappingId);
                parameters.Add("p_company_id", request.CompanyId);
                parameters.Add("p_transaction_type", request.TransactionType);
                parameters.Add("p_default_debit_account_id", request.DefaultDebitAccountId);
                parameters.Add("p_default_credit_account_id", request.DefaultCreditAccountId);
                parameters.Add("p_success", dbType: DbType.Boolean, direction: ParameterDirection.InputOutput);

                await connection.ExecuteAsync(
                    "CALL sp_update_cs_default_account_mapping(@p_mapping_id, @p_company_id, @p_transaction_type, @p_default_debit_account_id, @p_default_credit_account_id, @p_success)",
                    parameters);

                var success = parameters.Get<bool>("p_success");
                if (!success)
                {
                    throw new Exception("Failed to update default account mapping");
                }

                return await GetDefaultAccountMappingByIdAsync(mappingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating default account mapping");
                throw;
            }
        }

        public async Task<bool> DeleteDefaultAccountMappingAsync(int mappingId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("p_mapping_id", mappingId);
                parameters.Add("p_success", dbType: DbType.Boolean, direction: ParameterDirection.InputOutput);

                await connection.ExecuteAsync(
                    "CALL sp_delete_cs_default_account_mapping(@p_mapping_id, @p_success)",
                    parameters);

                return parameters.Get<bool>("p_success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting default account mapping");
                throw;
            }
        }

        public async Task<CsDefaultAccountMapping> GetDefaultAccountMappingByIdAsync(int mappingId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                var mapping = await connection.QuerySingleOrDefaultAsync<CsDefaultAccountMapping>(
                    "SELECT * FROM sp_get_cs_default_account_mapping_by_id(@p_mapping_id)",
                    new { p_mapping_id = mappingId });

                return mapping;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default account mapping by ID");
                throw;
            }
        }

        public async Task<CsDefaultAccountMappingResponse> GetDefaultAccountMappingsByCompanyAsync(CsDefaultAccountMappingSearchRequest request)
        {
            try
            {
                using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                var results = await connection.QueryMultipleAsync(
                    "SELECT * FROM sp_get_cs_default_account_mappings_by_company(@p_company_id, @p_search_text)",
                    new { 
                        p_company_id = request.CompanyId, 
                        p_search_text = request.SearchText 
                    });

                var mappings = results.Read<CsDefaultAccountMapping>().ToList();

                return new CsDefaultAccountMappingResponse(
                    mappings, 
                    request.PageNumber, 
                    request.PageSize, 
                    mappings.Count, // Total records
                    mappings.Count  // Filtered records
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default account mappings by company");
                throw;
            }
        }

        public async Task<List<CsDefaultAccountMapping>> GetAllDefaultAccountMappingsAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                var mappings = await connection.QueryAsync<CsDefaultAccountMapping>(
                    "SELECT * FROM sp_getall_cs_default_account_mapping()");
                
                return mappings.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all default account mappings");
                throw;
            }
        }
    }
}
