using Dapper;
using ERP.API.Models.DTOs.CompanySetup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ERP.API.Services.CompanySetup
{
    public class CsAccountingPeriodService : ICsAccountingPeriodService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CsAccountingPeriodService> _logger;
        private readonly string _connectionString;

        public CsAccountingPeriodService(IConfiguration configuration, ILogger<CsAccountingPeriodService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");
        }

        public async Task<CsAccountingPeriodResponse> CreateAccountingPeriodAsync(CsAccountingPeriodDto createDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var result = await connection.QuerySingleAsync<CsAccountingPeriodResponse>(
                    "SELECT * FROM sp_create_cs_accounting_period(@p_company_id::INTEGER, @p_period_name::VARCHAR, @p_start_date::DATE, @p_end_date::DATE, @p_status::VARCHAR, @p_is_current_active::BOOLEAN)",
                    new
                    {
                        p_company_id = createDto.CompanyId,
                        p_period_name = createDto.PeriodName,
                        p_start_date = createDto.StartDate.Date, // Convert to date only
                        p_end_date = createDto.EndDate.Date, // Convert to date only
                        p_status = createDto.Status,
                        p_is_current_active = createDto.IsCurrentActive
                    });
                return result;
            }
            catch (PostgresException pgEx) when (pgEx.SqlState == "23505" && pgEx.ConstraintName == "cs_accounting_periods_company_id_period_name_key")
            {
                _logger.LogWarning("Attempt to create duplicate accounting period: {PeriodName} for company ID: {CompanyId}", 
                    createDto.PeriodName, createDto.CompanyId);
                throw new InvalidOperationException($"An accounting period with the name '{createDto.PeriodName}' already exists for this company.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating accounting period");
                throw;
            }
        }

        public async Task<CsAccountingPeriodResponse> UpdateAccountingPeriodAsync(int periodId, CsAccountingPeriodDto updateDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var result = await connection.QuerySingleAsync<CsAccountingPeriodResponse>(
                    "SELECT * FROM sp_update_cs_accounting_period(@p_period_id::INTEGER, @p_period_name::VARCHAR, @p_start_date::DATE, @p_end_date::DATE, @p_status::VARCHAR, @p_is_current_active::BOOLEAN)",
                    new
                    {
                        p_period_id = periodId,
                        p_period_name = updateDto.PeriodName,
                        p_start_date = updateDto.StartDate.Date, // Convert to date only
                        p_end_date = updateDto.EndDate.Date, // Convert to date only
                        p_status = updateDto.Status,
                        p_is_current_active = updateDto.IsCurrentActive
                    });
                return result;
            }
            catch (PostgresException pgEx) when (pgEx.SqlState == "23505" && pgEx.ConstraintName == "cs_accounting_periods_company_id_period_name_key")
            {
                _logger.LogWarning("Attempt to update to duplicate period name: {PeriodName} for period ID: {PeriodId}", 
                    updateDto.PeriodName, periodId);
                throw new InvalidOperationException($"An accounting period with the name '{updateDto.PeriodName}' already exists for this company.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating accounting period");
                throw;
            }
        }

        public async Task<bool> DeleteAccountingPeriodAsync(int periodId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                return await connection.QuerySingleAsync<bool>(
                    "SELECT sp_delete_cs_accounting_period(@p_period_id::INTEGER)",
                    new { p_period_id = periodId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting accounting period");
                throw;
            }
        }

        public async Task<CsAccountingPeriodResponse?> GetAccountingPeriodByIdAsync(int periodId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                return await connection.QuerySingleOrDefaultAsync<CsAccountingPeriodResponse>(
                    "SELECT * FROM sp_get_cs_accounting_period_by_id(@p_period_id::INTEGER)",
                    new { p_period_id = periodId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accounting period by ID");
                throw;
            }
        }

        public async Task<CsAccountingPeriodPagedResponse> GetAccountingPeriodsByCompanyAsync(int companyId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var result = await connection.QueryAsync<dynamic>(
                    "SELECT * FROM sp_get_cs_accounting_periods_by_company(@p_company_id::INTEGER, @p_page_number::INTEGER, @p_page_size::INTEGER)",
                    new { p_company_id = companyId, p_page_number = pageNumber, p_page_size = pageSize });

                var resultList = result.ToList();
                if (!resultList.Any())
                {
                    return new CsAccountingPeriodPagedResponse
                    {
                        Items = new List<CsAccountingPeriodResponse>(),
                        TotalCount = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }

                var items = resultList.Select(x => new CsAccountingPeriodResponse
                {
                    PeriodId = x.period_id,
                    CompanyId = x.company_id,
                    PeriodName = x.period_name,
                    StartDate = x.start_date,
                    EndDate = x.end_date,
                    Status = x.status,
                    IsCurrentActive = x.is_current_active,
                    CreatedAt = x.created_at,
                    UpdatedAt = x.updated_at
                }).ToList();

                return new CsAccountingPeriodPagedResponse
                {
                    Items = items,
                    TotalCount = (int)resultList[0].total_count,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accounting periods by company");
                throw;
            }
        }

        public async Task<CsAccountingPeriodPagedResponse> SearchAccountingPeriodsAsync(int companyId, CsAccountingPeriodSearchRequest searchRequest)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var result = await connection.QueryAsync<dynamic>(
                    "SELECT * FROM sp_search_cs_accounting_periods(@p_company_id::INTEGER, @p_search_text::VARCHAR, @p_status::VARCHAR, @p_date::DATE, @p_page_number::INTEGER, @p_page_size::INTEGER)",
                    new
                    {
                        p_company_id = companyId,
                        p_search_text = searchRequest.SearchText,
                        p_status = searchRequest.Status,
                        p_date = searchRequest.Date?.Date, // Convert to date only if not null
                        p_page_number = searchRequest.PageNumber,
                        p_page_size = searchRequest.PageSize
                    });

                var resultList = result.ToList();
                if (!resultList.Any())
                {
                    return new CsAccountingPeriodPagedResponse
                    {
                        Items = new List<CsAccountingPeriodResponse>(),
                        TotalCount = 0,
                        PageNumber = searchRequest.PageNumber,
                        PageSize = searchRequest.PageSize
                    };
                }

                var items = resultList.Select(x => new CsAccountingPeriodResponse
                {
                    PeriodId = x.period_id,
                    CompanyId = x.company_id,
                    PeriodName = x.period_name,
                    StartDate = x.start_date,
                    EndDate = x.end_date,
                    Status = x.status,
                    IsCurrentActive = x.is_current_active,
                    CreatedAt = x.created_at,
                    UpdatedAt = x.updated_at
                }).ToList();

                return new CsAccountingPeriodPagedResponse
                {
                    Items = items,
                    TotalCount = (int)resultList[0].total_count,
                    PageNumber = searchRequest.PageNumber,
                    PageSize = searchRequest.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching accounting periods");
                throw;
            }
        }

        public async Task<List<CsAccountingPeriodResponse>> GetAllAccountingPeriodsAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var result = await connection.QueryAsync<dynamic>(
                    "SELECT * FROM sp_getall_cs_accounting_periods()");

                var items = result.Select(x => new CsAccountingPeriodResponse
                {
                    PeriodId = x.period_id,
                    CompanyId = x.company_id,
                    PeriodName = x.period_name,
                    StartDate = x.start_date,
                    EndDate = x.end_date,
                    Status = x.status,
                    IsCurrentActive = x.is_current_active,
                    CreatedAt = x.created_at,
                    UpdatedAt = x.updated_at
                }).ToList();

                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all accounting periods");
                throw;
            }
        }
    }
}
