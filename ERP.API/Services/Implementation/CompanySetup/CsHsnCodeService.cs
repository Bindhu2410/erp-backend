using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ERP.API.Services.CompanySetup
{
    public class CsHsnCodeService : BaseDataService<CsHsnCode>, ICsHsnCodeService
    {
        public CsHsnCodeService(IConfiguration configuration) 
            : base(configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection is not configured"), "cs_hsn_codes")
        {
        }

        public async Task<CsHsnCode?> GetByIdAsync(int hsnCodeId)
        {
            using var connection = CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_hsn_code_id", hsnCodeId);

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT * FROM sp_get_cs_hsn_code_by_id(@p_hsn_code_id)",
                parameters);

            if (result == null) return null;

            // Defensive: handle nulls for all value types
            return new CsHsnCode
            {
                HsnCodeId = result.hsn_code_id != null ? (int)result.hsn_code_id : 0,
                CompanyId = result.company_id != null ? (int)result.company_id : 0,
                Code = result.hsn_code != null ? (string)result.hsn_code : string.Empty,
                Description = result.description != null ? (string)result.description : string.Empty,
                IsActive = result.is_active != null ? (bool)result.is_active : false,
                DefaultGstRate = result.default_gst_rate != null ? (decimal)result.default_gst_rate : 0
            };
        }

        public async Task<(IEnumerable<CsHsnCode> Data, int TotalRecords, int FilteredRecords)> GetByCompanyAsync(CsHsnCodeSearchDto searchDto)
        {
            using var connection = CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_company_id", searchDto.CompanyId);
            parameters.Add("p_search_text", searchDto.SearchText ?? string.Empty);

            var result = await connection.QueryAsync<dynamic>(
                "SELECT * FROM sp_get_cs_hsn_codes_by_company(@p_company_id, @p_search_text)",
                parameters);

            var resultList = result.ToList();
            int totalRecords = 0;
            int filteredRecords = 0;
            
            if (resultList.Any())
            {
                var firstItem = resultList.FirstOrDefault();
                if (firstItem != null)
                {
                    totalRecords = Convert.ToInt32(firstItem.total_records);
                    filteredRecords = Convert.ToInt32(firstItem.filtered_records);
                }
            }
            
            // Convert dynamic result to CsHsnCode objects
            var data = resultList.Select(item => new CsHsnCode
            {
                HsnCodeId = item.hsn_code_id,
                CompanyId = item.company_id,
                Code = item.hsn_code,
                Description = item.description,
                IsActive = true, // Assuming this is true for retrieved records
                DefaultGstRate = item.default_gst_rate != null ? (decimal)item.default_gst_rate : 0
            });

            return (data, totalRecords, filteredRecords);
        }

        public override async Task<int> CreateAsync(CsHsnCode hsnCode)
        {
            using var connection = CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_company_id", hsnCode.CompanyId);
            parameters.Add("p_hsn_code", hsnCode.Code);
            parameters.Add("p_description", hsnCode.Description);
            parameters.Add("p_default_gst_rate", hsnCode.DefaultGstRate);
            parameters.Add("p_hsn_code_id", dbType: DbType.Int32, direction: ParameterDirection.InputOutput);

            await connection.ExecuteAsync(
                "CALL sp_create_cs_hsn_code(@p_company_id, @p_hsn_code, @p_description, @p_default_gst_rate, @p_hsn_code_id)",
                parameters);

            return parameters.Get<int>("p_hsn_code_id");
        }

        public override async Task<bool> UpdateAsync(CsHsnCode hsnCode)
        {
            try
            {
                using var connection = CreateConnection();
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                var parameters = new DynamicParameters();
                parameters.Add("p_hsn_code_id", hsnCode.HsnCodeId, DbType.Int32, ParameterDirection.Input);
                parameters.Add("p_company_id", hsnCode.CompanyId, DbType.Int32, ParameterDirection.Input);
                parameters.Add("p_hsn_code", hsnCode.Code, DbType.String, ParameterDirection.Input);
                parameters.Add("p_description", hsnCode.Description, DbType.String, ParameterDirection.Input);
                parameters.Add("p_default_gst_rate", hsnCode.DefaultGstRate, DbType.Decimal, ParameterDirection.Input);
                parameters.Add("p_success", false, DbType.Boolean, ParameterDirection.InputOutput);

                try
                {
                    await connection.ExecuteAsync(
                        "CALL sp_update_cs_hsn_code(@p_hsn_code_id, @p_company_id, @p_hsn_code, @p_description, @p_default_gst_rate, @p_success)",
                        parameters,
                        commandTimeout: 60);

                    var success = parameters.Get<bool>("p_success");
                    Console.WriteLine($"SP update result for HSN code ID {hsnCode.HsnCodeId}: success = {success}");
                    return success;
                }
                catch (Npgsql.PostgresException pgEx)
                {
                    Console.WriteLine($"PostgreSQL Error updating HSN code ID {hsnCode.HsnCodeId}: {pgEx.MessageText}, Code: {pgEx.SqlState}");
                    Console.WriteLine($"Detail: {pgEx.Detail}, Hint: {pgEx.Hint}, Position: {pgEx.Position}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating HSN code ID {hsnCode.HsnCodeId}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    var nested = ex.InnerException.InnerException;
                    if (nested != null)
                    {
                        Console.WriteLine($"Nested Exception: {nested.Message}");
                    }
                }
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        public override async Task<bool> DeleteAsync(int hsnCodeId)
        {
            try
            {
                Console.WriteLine($"Received request to delete HSN code with ID: {hsnCodeId}");
                // First check if the record exists
                var existingRecord = await GetByIdAsync(hsnCodeId);
                if (existingRecord == null)
                {
                    Console.WriteLine($"HSN code with ID {hsnCodeId} not found. Cannot delete.");
                    return false;
                }
                Console.WriteLine($"HSN code with ID {hsnCodeId} found. Proceeding with deletion via SP.");

                using var connection = CreateConnection();
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                var parameters = new DynamicParameters();
                parameters.Add("p_hsn_code_id", hsnCodeId, DbType.Int32, ParameterDirection.Input);
                parameters.Add("p_success", false, DbType.Boolean, ParameterDirection.InputOutput);

                try
                {
                    await connection.ExecuteAsync(
                        "CALL sp_delete_cs_hsn_code(@p_hsn_code_id, @p_success)",
                        parameters,
                        commandTimeout: 60);

                    var success = parameters.Get<bool>("p_success");
                    Console.WriteLine($"SP delete result for HSN code ID {hsnCodeId}: success = {success}");
                    return success;
                }
                catch (Npgsql.PostgresException pgEx)
                {
                    Console.WriteLine($"PostgreSQL Error deleting HSN code ID {hsnCodeId}: {pgEx.MessageText}, Code: {pgEx.SqlState}");
                    Console.WriteLine($"Detail: {pgEx.Detail}, Hint: {pgEx.Hint}, Position: {pgEx.Position}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting HSN code ID {hsnCodeId}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    var nested = ex.InnerException.InnerException;
                    if (nested != null)
                    {
                        Console.WriteLine($"Nested Exception: {nested.Message}");
                    }
                }
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }



        protected override string GenerateInsertQuery()
        {
            return "CALL sp_create_cs_hsn_code(@p_company_id, @p_hsn_code, @p_description, @p_default_gst_rate, @p_hsn_code_id)";
        }

        protected override string GenerateUpdateQuery()
        {
            return "CALL sp_update_cs_hsn_code(@p_hsn_code_id, @p_company_id, @p_hsn_code, @p_description, @p_default_gst_rate, @p_success)";
        }
    }
}
