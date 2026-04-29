using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ERP.API.Services.CompanySetup
{
    public class CsGstRateService : ICsGstRateService
    {
        private readonly string _connectionString;
        private readonly ILogger<CsGstRateService> _logger;

        public CsGstRateService(IConfiguration configuration, ILogger<CsGstRateService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
            _logger = logger;
        }

        public async Task<CsGstRate?> GetByIdAsync(int gstRateId)
        {
            try
            {
                if (gstRateId <= 0)
                {
                    return null;
                }

                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.QuerySingleOrDefaultAsync<CsGstRate>(
                    "SELECT * FROM sp_get_cs_gst_rate_by_id(@p_gst_rate_id)",
                    new { p_gst_rate_id = gstRateId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting GST rate by ID");
                throw;
            }
        }

        public async Task<(IEnumerable<CsGstRate> Data, int TotalRecords, int FilteredRecords)> GetByCompanyAsync(CsGstRateSearchDto searchDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                
                var results = await connection.QueryAsync<dynamic>(
                    "SELECT * FROM sp_get_cs_gst_rates_by_company(@p_company_id, @p_search_text, @p_page_number, @p_page_size)",
                    new 
                    { 
                        p_company_id = searchDto.CompanyId,
                        p_search_text = searchDto.SearchText,
                        p_page_number = searchDto.PageNumber,
                        p_page_size = searchDto.PageSize
                    });

                var resultList = results.ToList();
                
                // Extract total and filtered records from the first row (if any)
                int totalRecords = 0;
                int filteredRecords = 0;
                
                if (resultList.Any())
                {
                    var firstRow = resultList.First();
                    totalRecords = (int)(long)firstRow.total_records;
                    filteredRecords = (int)(long)firstRow.filtered_records;
                }

                // Map to CsGstRate objects
                var data = resultList.Select(r => new CsGstRate
                {
                    GstRateId = r.gst_rate_id,
                    CompanyId = r.company_id,
                    HsnSacCode = r.hsn_sac_code,
                    IsHsn = r.is_hsn,
                    GstRate = r.gst_rate,
                    EffectiveDate = r.effective_date,
                    CreatedAt = r.created_at,
                    UpdatedAt = r.updated_at
                });

                return (data, totalRecords, filteredRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting GST rates by company");
                throw;
            }
        }

        public async Task<IEnumerable<CsGstRateWithCompany>> GetAllAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var results = await connection.QueryAsync<CsGstRateWithCompany>(
                    "SELECT * FROM sp_getall_cs_gst_rates()");

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all GST rates");
                throw;
            }
        }

        public async Task<CsGstRate?> GetByHsnSacAsync(int companyId, string hsnSacCode, bool isHsn, DateTime effectiveDate)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("p_company_id", companyId);
                parameters.Add("p_hsn_sac_code", hsnSacCode);
                parameters.Add("p_is_hsn", isHsn);
                parameters.Add("p_effective_date", effectiveDate.Date, DbType.Date);

                return await connection.QueryFirstOrDefaultAsync<CsGstRate>(
                    "SELECT * FROM sp_get_cs_gst_rate_by_hsn_sac(@p_company_id, @p_hsn_sac_code, @p_is_hsn, @p_effective_date)",
                    parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting GST rate by HSN/SAC");
                throw;
            }
        }

        public async Task<int> CreateAsync(CsGstRate gstRate)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("p_company_id", gstRate.CompanyId);
                parameters.Add("p_hsn_sac_code", gstRate.HsnSacCode);
                parameters.Add("p_is_hsn", gstRate.IsHsn);
                parameters.Add("p_gst_rate", gstRate.GstRate);
                parameters.Add("p_effective_date", gstRate.EffectiveDate.Date, DbType.Date);
                parameters.Add("p_gst_rate_id", value: 0, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);

                await connection.ExecuteAsync(
                    "CALL sp_create_cs_gst_rate(@p_company_id, @p_hsn_sac_code, @p_is_hsn, @p_gst_rate, @p_effective_date, @p_gst_rate_id)",
                    parameters);

                return parameters.Get<int>("p_gst_rate_id");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating GST rate");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(CsGstRate gstRate)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("p_gst_rate_id", gstRate.GstRateId);
                parameters.Add("p_company_id", gstRate.CompanyId);
                parameters.Add("p_hsn_sac_code", gstRate.HsnSacCode);
                parameters.Add("p_is_hsn", gstRate.IsHsn);
                parameters.Add("p_gst_rate", gstRate.GstRate);
                parameters.Add("p_effective_date", gstRate.EffectiveDate.Date, DbType.Date);
                parameters.Add("p_success", value: false, dbType: DbType.Boolean, direction: ParameterDirection.InputOutput);

                await connection.ExecuteAsync(
                    "CALL sp_update_cs_gst_rate(@p_gst_rate_id, @p_company_id, @p_hsn_sac_code, @p_is_hsn, @p_gst_rate, @p_effective_date, @p_success)",
                    parameters);

                return parameters.Get<bool>("p_success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating GST rate");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int gstRateId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("p_gst_rate_id", gstRateId);
                parameters.Add("p_success", value: false, dbType: DbType.Boolean, direction: ParameterDirection.InputOutput);

                await connection.ExecuteAsync(
                    "CALL sp_delete_cs_gst_rate(@p_gst_rate_id, @p_success)",
                    parameters);

                return parameters.Get<bool>("p_success");
            }
            catch (PostgresException ex) when (ex.SqlState == "P0001")
            {
                // Handle business rule violations gracefully
                _logger.LogWarning("Cannot delete GST rate {GstRateId}: {Message}", gstRateId, ex.MessageText);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting GST rate");
                throw;
            }
        }
    }
}
