using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Dapper;
using ERP.API.Models;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;
using System.Data;
using Npgsql;

namespace ERP.API.Services.Implementation.CompanySetup
{
    public class CsTdsRateService : ICsTdsRateService
    {
        private readonly string _connectionString;

        public CsTdsRateService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<CsTdsRateDto> GetByIdAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            
            try
            {
                await connection.OpenAsync();
                Console.WriteLine($"[TDS] Connected to database, executing sp_cs_tds_rates_get_by_id for ID: {id}");
                
                // Try using direct approach without explicit type casting
                var result = await connection.QueryFirstOrDefaultAsync<CsTdsRateDto>(
                    "SELECT * FROM sp_cs_tds_rates_get_by_id(@p_tds_rate_id)",
                    new { p_tds_rate_id = id }
                );
                
                if (result != null)
                {
                    Console.WriteLine($"[TDS] Result found for ID {id}: TdsRateId={result.TdsRateId}, CompanyId={result.CompanyId}");
                }
                else 
                {
                    Console.WriteLine($"[TDS] No result found for ID {id}");
                }
                
                return result;
            }
            catch (PostgresException pgEx)
            {
                // Handle PostgreSQL specific errors
                Console.WriteLine($"[TDS] PostgresException in GetByIdAsync: {pgEx.Message}");
                Console.WriteLine($"[TDS] PostgresException Detail: {pgEx.Detail}");
                Console.WriteLine($"[TDS] PostgresException Hint: {pgEx.Hint}");
                Console.WriteLine($"[TDS] PostgresException SQLState: {pgEx.SqlState}");
                
                // Try an alternative approach with a different syntax
                try
                {
                    var result = await connection.QueryFirstOrDefaultAsync<CsTdsRateDto>(
                        "SELECT * FROM public.sp_cs_tds_rates_get_by_id(@p_tds_rate_id)",
                        new { p_tds_rate_id = id }
                    );
                    return result;
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine($"[TDS] Fallback approach also failed: {fallbackEx.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                // Better exception logging with details
                Console.WriteLine($"[TDS] Error in CsTdsRateService.GetByIdAsync: {ex.Message}");
                Console.WriteLine($"[TDS] Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[TDS] Inner Exception: {ex.InnerException.Message}");
                }
                // Re-throw to propagate to the caller
                throw;
            }
        }

        public async Task<PagedResponse<CsTdsRateDto>> SearchAsync(CsTdsRateSearchDto searchDto)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"SearchAsync called with CompanyId: {searchDto.CompanyId}, SectionType: {searchDto.SectionType}, PageSize: {searchDto.PageSize}, PageNumber: {searchDto.PageNumber}");
                
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                // Use dynamic to capture all columns including total_records
                var result = await connection.QueryAsync<dynamic>(
                    "SELECT * FROM public.sp_cs_tds_rates_search(@p_company_id, @p_section_type, @p_page_size, @p_page_number)",
                    new
                    {
                        p_company_id = searchDto.CompanyId,
                        p_section_type = searchDto.SectionType ?? string.Empty,
                        p_page_size = searchDto.PageSize,
                        p_page_number = searchDto.PageNumber
                    }
                );

                // Log the result for debugging
                var resultList = result.ToList();
                System.Diagnostics.Debug.WriteLine($"Search returned {resultList.Count} rows");
                
                // Get the first row to check for total_records
                var firstRow = resultList.FirstOrDefault();
                
                int totalRecords = 0;
                // Safely try to extract total_records
                if (firstRow != null)
                {
                    try
                    {
                        // Check if the property exists using IDictionary for dynamic objects
                        var dictionary = (IDictionary<string, object>)firstRow;
                        if (dictionary.ContainsKey("total_records"))
                        {
                            totalRecords = Convert.ToInt32(dictionary["total_records"]);
                            System.Diagnostics.Debug.WriteLine($"Total records found: {totalRecords}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("total_records property not found in result");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error extracting total_records: {ex.Message}");
                        // Continue with totalRecords = 0
                    }
                }

                // Convert dynamic to CsTdsRateDto
                var tdsRates = new List<CsTdsRateDto>();
                foreach (var row in resultList)
                {
                    try
                    {
                        var dictionary = (IDictionary<string, object>)row;
                        var dto = new CsTdsRateDto
                        {
                            TdsRateId = dictionary.ContainsKey("tds_rate_id") ? Convert.ToInt32(dictionary["tds_rate_id"]) : 0,
                            CompanyId = dictionary.ContainsKey("company_id") ? Convert.ToInt32(dictionary["company_id"]) : 0,
                            SectionType = dictionary.ContainsKey("section_type") ? dictionary["section_type"]?.ToString() : string.Empty,
                            ThresholdAmount = dictionary.ContainsKey("threshold_amount") ? Convert.ToDecimal(dictionary["threshold_amount"]) : 0,
                            Rate = dictionary.ContainsKey("rate") ? Convert.ToDecimal(dictionary["rate"]) : 0,
                            EffectiveDate = dictionary.ContainsKey("effective_date") ? Convert.ToDateTime(dictionary["effective_date"]) : DateTime.Now
                        };
                        tdsRates.Add(dto);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error converting row to DTO: {ex.Message}");
                        // Skip this row
                    }
                }

                return new PagedResponse<CsTdsRateDto>
                {
                    Data = tdsRates,
                    TotalRecords = totalRecords,
                    PageNumber = searchDto.PageNumber,
                    PageSize = searchDto.PageSize
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in SearchAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                if (ex is Npgsql.PostgresException pgEx)
                {
                    System.Diagnostics.Debug.WriteLine($"PostgreSQL Error: {pgEx.MessageText}");
                    System.Diagnostics.Debug.WriteLine($"Detail: {pgEx.Detail}, Hint: {pgEx.Hint}, Position: {pgEx.Position}");
                }
                
                throw; // Rethrow the exception after logging
            }
        }

        public async Task<PagedResponse<CsTdsRateDto>> GetByCompanyAsync(int companyId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetByCompanyAsync called with CompanyId: {companyId}, PageSize: {pageSize}, PageNumber: {pageNumber}");
                
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                // Use dynamic to capture all columns including total_count
                var result = await connection.QueryAsync<dynamic>(
                    "SELECT * FROM public.sp_cs_tds_rates_get_by_company(@p_company_id, @p_page_size, @p_page_number)",
                    new
                    {
                        p_company_id = companyId,
                        p_page_size = pageSize,
                        p_page_number = pageNumber
                    }
                );

                // Log the result for debugging
                var resultList = result.ToList();
                System.Diagnostics.Debug.WriteLine($"GetByCompanyAsync returned {resultList.Count} rows");
                
                // Get the first row to check for total_count
                var firstRow = resultList.FirstOrDefault();
                
                int totalRecords = 0;
                // Safely try to extract total_count
                if (firstRow != null)
                {
                    try
                    {
                        // Check if the property exists using IDictionary for dynamic objects
                        var dictionary = (IDictionary<string, object>)firstRow;
                        if (dictionary.ContainsKey("total_count"))
                        {
                            totalRecords = Convert.ToInt32(dictionary["total_count"]);
                            System.Diagnostics.Debug.WriteLine($"Total records found: {totalRecords}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("total_count property not found in result");
                            // Try looking for other possible property names
                            foreach (var key in dictionary.Keys)
                            {
                                System.Diagnostics.Debug.WriteLine($"Available property: {key}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error extracting total_count: {ex.Message}");
                        // Continue with totalRecords = 0
                    }
                }

                // Convert dynamic to CsTdsRateDto
                var tdsRates = new List<CsTdsRateDto>();
                foreach (var row in resultList)
                {
                    try
                    {
                        var dictionary = (IDictionary<string, object>)row;
                        var dto = new CsTdsRateDto
                        {
                            TdsRateId = dictionary.ContainsKey("tds_rate_id") ? Convert.ToInt32(dictionary["tds_rate_id"]) : 0,
                            CompanyId = dictionary.ContainsKey("company_id") ? Convert.ToInt32(dictionary["company_id"]) : 0,
                            SectionType = dictionary.ContainsKey("section_type") ? dictionary["section_type"]?.ToString() : string.Empty,
                            ThresholdAmount = dictionary.ContainsKey("threshold_amount") ? Convert.ToDecimal(dictionary["threshold_amount"]) : 0,
                            Rate = dictionary.ContainsKey("rate") ? Convert.ToDecimal(dictionary["rate"]) : 0,
                            EffectiveDate = dictionary.ContainsKey("effective_date") ? Convert.ToDateTime(dictionary["effective_date"]) : DateTime.Now
                        };
                        tdsRates.Add(dto);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error converting row to DTO: {ex.Message}");
                        // Skip this row
                    }
                }

                return new PagedResponse<CsTdsRateDto>
                {
                    Data = tdsRates,
                    TotalRecords = totalRecords,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in GetByCompanyAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                if (ex is Npgsql.PostgresException pgEx)
                {
                    System.Diagnostics.Debug.WriteLine($"PostgreSQL Error: {pgEx.MessageText}");
                    System.Diagnostics.Debug.WriteLine($"Detail: {pgEx.Detail}, Hint: {pgEx.Hint}, Position: {pgEx.Position}");
                }
                
                throw; // Rethrow the exception after logging
            }
        }

        public async Task<int> CreateAsync(CsTdsRateDto tdsRate)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var result = await connection.QuerySingleAsync<int>(
                "SELECT public.sp_cs_tds_rates_create(@p_company_id::integer, @p_section_type::varchar, @p_threshold_amount::numeric, @p_rate::numeric, @p_effective_date::date)",
                new
                {
                    p_company_id = tdsRate.CompanyId,
                    p_section_type = tdsRate.SectionType,
                    p_threshold_amount = tdsRate.ThresholdAmount,
                    p_rate = tdsRate.Rate,
                    p_effective_date = tdsRate.EffectiveDate
                }
            );

            return result;
        }

        public async Task<bool> UpdateAsync(CsTdsRateDto tdsRate)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var result = await connection.ExecuteScalarAsync<bool>(
                "SELECT public.sp_cs_tds_rates_update(@p_tds_rate_id::integer, @p_company_id::integer, @p_section_type::varchar, @p_threshold_amount::numeric, @p_rate::numeric, @p_effective_date::date)",
                new
                {
                    p_tds_rate_id = tdsRate.TdsRateId,
                    p_company_id = tdsRate.CompanyId,
                    p_section_type = tdsRate.SectionType,
                    p_threshold_amount = tdsRate.ThresholdAmount,
                    p_rate = tdsRate.Rate,
                    p_effective_date = tdsRate.EffectiveDate
                }
            );

            return result;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var result = await connection.ExecuteScalarAsync<bool>(
                "SELECT public.sp_cs_tds_rates_delete(@p_tds_rate_id::integer)",
                new { p_tds_rate_id = id }
            );

            return result;
        }

        public async Task<List<CsTdsRateDto>> GetAllItemsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetAllItemsAsync called");
                
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                // Call the stored procedure with no parameters
                var result = await connection.QueryAsync<dynamic>(
                    "SELECT * FROM public.sp_cs_tds_rates_get_all_items()"
                );

                // Log the result for debugging
                var resultList = result.ToList();
                System.Diagnostics.Debug.WriteLine($"GetAllItemsAsync returned {resultList.Count} rows");

                // Convert dynamic to CsTdsRateDto
                var tdsRates = new List<CsTdsRateDto>();
                foreach (var row in resultList)
                {
                    try
                    {
                        var dictionary = (IDictionary<string, object>)row;
                        var dto = new CsTdsRateDto
                        {
                            TdsRateId = dictionary.ContainsKey("tds_rate_id") ? Convert.ToInt32(dictionary["tds_rate_id"]) : 0,
                            CompanyId = dictionary.ContainsKey("company_id") ? Convert.ToInt32(dictionary["company_id"]) : 0,
                            SectionType = dictionary.ContainsKey("section_type") ? dictionary["section_type"]?.ToString() : string.Empty,
                            ThresholdAmount = dictionary.ContainsKey("threshold_amount") ? Convert.ToDecimal(dictionary["threshold_amount"]) : 0,
                            Rate = dictionary.ContainsKey("rate") ? Convert.ToDecimal(dictionary["rate"]) : 0,
                            EffectiveDate = dictionary.ContainsKey("effective_date") ? Convert.ToDateTime(dictionary["effective_date"]) : DateTime.Now
                        };
                        tdsRates.Add(dto);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error converting row to DTO: {ex.Message}");
                        // Skip this row
                    }
                }

                return tdsRates;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in GetAllItemsAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                if (ex is Npgsql.PostgresException pgEx)
                {
                    System.Diagnostics.Debug.WriteLine($"PostgreSQL Error: {pgEx.MessageText}");
                    System.Diagnostics.Debug.WriteLine($"Detail: {pgEx.Detail}, Hint: {pgEx.Hint}, Position: {pgEx.Position}");
                }
                
                throw; // Rethrow the exception after logging
            }
        }
    }
}
