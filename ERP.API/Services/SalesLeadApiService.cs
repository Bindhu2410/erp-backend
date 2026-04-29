using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models;
using ERP.API.Models.DTOs;
using Npgsql;

namespace ERP.API.Services
{      public class SalesLeadApiService : ISalesLeadApiService
    {
        private readonly IDbConnection _connection;

        public SalesLeadApiService(IDbConnection connection)
        {
            _connection = connection;
        }
        
        public IDbConnection GetConnection()
        {
            return _connection;
        }

        public async Task<IEnumerable<SalesLeadDto>> GetAllSalesLeadsAsync()
        {
            try
            {
                var salesLeads = await _connection.QueryAsync<SalesLeadEntity>(
                    "SELECT * FROM public.sp_sales_lead_read()");

                return salesLeads.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve sales leads: {ex.Message}", ex);
            }
        }

        public async Task<SalesLeadDto?> GetSalesLeadByIdAsync(int id)
        {
            try
            {
                var salesLead = await _connection.QuerySingleOrDefaultAsync<SalesLeadEntity>(
                    "SELECT * FROM public.sp_sales_lead_read(@p_id)",
                    new { p_id = id });

                if (salesLead == null)
                {
                    return null;
                }

                return MapToDto(salesLead);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve sales lead {id}: {ex.Message}", ex);
            }
        }

        public async Task<SalesLeadDto> CreateSalesLeadAsync(CreateSalesLeadDto createSalesLeadDto)
        {
            try
            {
                // Convert DTO to JSON format
                var jsonData = ConvertToJson(createSalesLeadDto);

                // Call the stored procedure
                var salesLead = await _connection.QuerySingleAsync<SalesLeadEntity>(
                    "SELECT * FROM public.sp_sales_lead_create(@p_data)",
                    new { p_data = jsonData });

                return MapToDto(salesLead);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create sales lead: {ex.Message}", ex);
            }
        }        
        
        public async Task<SalesLeadDto?> UpdateSalesLeadAsync(int id, UpdateSalesLeadDto updateSalesLeadDto)
        {
            try
            {
                // First verify that the record exists
                var existingRecord = await _connection.QuerySingleOrDefaultAsync<SalesLeadEntity>(
                    "SELECT * FROM public.sales_lead WHERE id = @id", 
                    new { id });

                if (existingRecord == null)
                {
                    return null;
                }
                
                // Convert DTO to JSON format
                var jsonData = ConvertToJson(updateSalesLeadDto);

                try
                {
                    // Call the stored procedure with explicitly named parameters
                    var salesLead = await _connection.QuerySingleOrDefaultAsync<SalesLeadEntity>(
                        "SELECT * FROM public.sp_sales_lead_update(@p_id, @p_data)",
                        new { p_id = id, p_data = jsonData });

                    if (salesLead == null)
                    {
                        return null;
                    }

                    return MapToDto(salesLead);
                }
                catch (Npgsql.PostgresException pgEx)
                {
                    // Specific handling for PostgreSQL errors
                    if (pgEx.SqlState == "42883") // function does not exist
                    {
                        // Fall back to direct SQL update
                        return await FallbackUpdateUsingDirectSql(id, updateSalesLeadDto);
                    }
                    
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update sales lead {id}: {ex.Message}", ex);
            }
        }
        
        // Fallback method if the stored procedure fails
        private async Task<SalesLeadDto?> FallbackUpdateUsingDirectSql(int id, UpdateSalesLeadDto updateDto)
        {
            try
            {
                // Build a direct SQL update statement
                var updateFields = new List<string>();
                var parameters = new DynamicParameters();
                parameters.Add("Id", id);
                
                // Add each property from the DTO
                foreach (var prop in typeof(UpdateSalesLeadDto).GetProperties())
                {
                    var value = prop.GetValue(updateDto);
                    if (value != null)
                    {
                        var columnName = GetSnakeCaseName(prop.Name);
                        updateFields.Add($"{columnName} = @{prop.Name}");
                        parameters.Add(prop.Name, value);
                    }
                }
                
                // Always update date_updated
                updateFields.Add("date_updated = CURRENT_TIMESTAMP");
                
                if (updateFields.Count == 0)
                {
                    return null;
                }
                
                // Construct and execute the SQL
                var sql = $"UPDATE public.sales_lead SET {string.Join(", ", updateFields)} " +
                          "WHERE id = @Id RETURNING *";
                
                var updated = await _connection.QuerySingleOrDefaultAsync<SalesLeadEntity>(sql, parameters);
                
                if (updated == null)
                {
                    return null;
                }
                
                return MapToDto(updated);
            }
            catch
            {
                throw;
            }
        }
        
        private string GetSnakeCaseName(string camelCaseName)
        {
            // Convert camelCase to snake_case (e.g., customerName -> customer_name)
            return string.Concat(camelCaseName.Select((x, i) => 
                i > 0 && char.IsUpper(x) ? "_" + char.ToLower(x) : x.ToString())).ToLower();
        }

        public async Task<bool> DeleteSalesLeadAsync(int id, int userUpdated)
        {
            try
            {
                var salesLead = await _connection.QuerySingleOrDefaultAsync<SalesLeadEntity>(
                    "SELECT * FROM public.sp_sales_lead_soft_delete(@p_id, @p_user_updated)",
                    new { p_id = id, p_user_updated = userUpdated });

                return salesLead != null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete sales lead {id}: {ex.Message}", ex);
            }
        }        
        
        // Helper method to convert DTOs to JSON
        private string ConvertToJson(object dto)
        {
            try
            {
                // Configure Newtonsoft.Json to use camelCase, which is what the stored procedure expects
                var settings = new Newtonsoft.Json.JsonSerializerSettings
                {
                    Formatting = Newtonsoft.Json.Formatting.None,
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Include,
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                };

                // Explicitly use camelCase without converting to snake_case
                // This matches the camelCase property names used in C# models
                return Newtonsoft.Json.JsonConvert.SerializeObject(dto, settings);
            }
            catch (Exception ex)
            {
                // Log or handle serialization error
                throw new Exception($"JSON serialization error: {ex.Message}", ex);
            }
        }

        // Helper method to map from entity to DTO
        // Explicit mapping for type safety and to avoid runtime cast exceptions        
        private SalesLeadDto MapToDto(SalesLeadEntity entity)
        {
            if (entity == null) return new SalesLeadDto();
            
            return new SalesLeadDto
            {
                Id = entity.Id,
                UserCreated = entity.UserCreated,
                DateCreated = entity.DateCreated,
                UserUpdated = entity.UserUpdated,
                DateUpdated = entity.DateUpdated,
                CustomerName = entity.CustomerName,
                LeadSource = entity.LeadSource,
                ReferralSourceName = entity.ReferralSourceName,
                HospitalOfReferral = entity.HospitalOfReferral,
                DepartmentOfReferral = entity.DepartmentOfReferral,
                SocialMedia = entity.SocialMedia,
                EventDate = entity.EventDate,
                EventName = entity.EventName,
                LeadId = entity.LeadId,
                Status = entity.Status,
                Score = entity.Score,
                IsActive = entity.IsActive,
                Comments = entity.Comments,
                LeadType = entity.LeadType,
                ContactName = entity.ContactName,
                Salutation = entity.Salutation,
                ContactMobileNo = entity.ContactMobileNo,
                LandLineNo = entity.LandLineNo,
                Email = entity.Email,
                Fax = entity.Fax,
                DoorNo = entity.DoorNo,
                Street = entity.Street,
                Landmark = entity.Landmark,
                Website = entity.Website,
                GeographicalDivisionsId = entity.GeographicalDivisionsId,
                Territory = entity.Territory,
                AreaId = entity.AreaId,
                Area = entity.Area,
                City = entity.City,
                PincodeId = entity.PincodeId,
                Pincode = entity.Pincode,
                District = entity.District,
                State = entity.State,
                Country = entity.Country,
                ConvertedCustomerId = entity.ConvertedCustomerId,
                UserId = entity.UserId
            };
        }

        // New Dashboard and Filtering Methods
        public async Task<SalesLeadSummaryCards> GetSummaryCardsAsync()
        {
            try
            {
                var summaryData = await _connection.QuerySingleOrDefaultAsync<SalesLeadSummaryCards>(
                    "SELECT total_leads as \"TotalLeads\", new_this_week as \"NewThisWeek\", qualified_leads as \"QualifiedLeads\" FROM public.fn_get_saleslead_card_status()"
                );
                return summaryData ?? new SalesLeadSummaryCards();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve summary cards: {ex.Message}", ex);
            }
        }        public async Task<SalesLeadFilterResponse> FilterLeadsAsync(SalesLeadFilterRequest request)
        {
            try
            {
                // Handle null or empty sorting values
                if (string.IsNullOrEmpty(request.SortField))
                {
                    request.SortField = "id";
                }
                else if (request.SortField.ToLower() == "date_created")
                {
                    // Convert for backward compatibility
                    request.SortField = "created_date";
                }

                if (string.IsNullOrEmpty(request.SortDirection))
                {
                    request.SortDirection = "ASC";
                }

                var leads = await _connection.QueryAsync<SalesLeadFilterResult>(
                    "SELECT * FROM public.sp_sales_lead_filter(@p_territory, @p_customer_name, @p_status, @p_score, " +
                    "@p_lead_type, @p_sort_field, @p_sort_direction, @p_page_number, @p_page_size)",
                    new { 
                        p_territory = request.Territory,
                        p_customer_name = request.CustomerName,
                        p_status = request.Status,
                        p_score = request.Score,
                        p_lead_type = request.LeadType,
                        p_sort_field = request.SortField,
                        p_sort_direction = request.SortDirection,
                        p_page_number = request.PageNumber,
                        p_page_size = request.PageSize
                    });

                var leadsList = leads.ToList();
                var response = new SalesLeadFilterResponse
                {
                    Leads = leadsList,
                    TotalCount = leadsList.Any() ? leadsList.First().TotalCount : 0
                };

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to filter leads: {ex.Message}", ex);
            }
        }        public async Task<SalesLeadFilterResponse> GetMyLeadsAsync(int userId, SalesLeadFilterRequest request)
        {
            try
            {
                // Handle null or empty sorting values
                if (string.IsNullOrEmpty(request.SortField))
                {
                    request.SortField = "id";
                }
                else if (request.SortField.ToLower() == "date_created")
                {
                    // Convert for backward compatibility
                    request.SortField = "created_date";
                }

                if (string.IsNullOrEmpty(request.SortDirection))
                {
                    request.SortDirection = "ASC";
                }
                
                var leads = await _connection.QueryAsync<SalesLeadFilterResult>(
                    "SELECT * FROM public.sp_sales_lead_my_leads(@p_user_id, @p_sort_field, @p_sort_direction, " +
                    "@p_page_number, @p_page_size)",
                    new {
                        p_user_id = userId,
                        p_sort_field = request.SortField,
                        p_sort_direction = request.SortDirection,
                        p_page_number = request.PageNumber,
                        p_page_size = request.PageSize
                    });

                var leadsList = leads.ToList();
                var response = new SalesLeadFilterResponse
                {
                    Leads = leadsList,
                    TotalCount = leadsList.Any() ? leadsList.First().TotalCount : 0
                };

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve my leads: {ex.Message}", ex);
            }
        }

        public async Task<SalesLeadDropdownOptions> GetFilterDropdownOptionsAsync()
        {
            try
            {
                var options = await _connection.QuerySingleOrDefaultAsync<SalesLeadDropdownOptions>(
                    "SELECT territories, customers, statuses, scores, lead_types " +
                    "FROM public.sp_sales_lead_dropdown_options()");
                
                return options ?? new SalesLeadDropdownOptions();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve filter options: {ex.Message}", ex);
            }
        }

        public async Task<(IEnumerable<SalesLeadGridResult> Results, int TotalRecords)> GetSalesLeadsGridAsync(
            string? searchText = null,
            string[]? customerNames = null,
            string[]? statuses = null,
            string[]? scores = null,
            string[]? leadTypes = null,
            int? pageNumber = 1,
            int? pageSize = 10,            string? orderBy = "created_date",
            string? orderDirection = "DESC")
        {
            try
            {
            var parameters = new DynamicParameters();
            parameters.Add("p_search_text", searchText);
            parameters.Add("p_customer_names", customerNames?.Length > 0 ? customerNames : null, DbType.Object);
            parameters.Add("p_statuses", statuses?.Length > 0 ? statuses : null, DbType.Object);
            parameters.Add("p_scores", scores?.Length > 0 ? scores : null, DbType.Object);
            parameters.Add("p_lead_types", leadTypes?.Length > 0 ? leadTypes : null, DbType.Object);
            parameters.Add("p_page_number", pageNumber);
            parameters.Add("p_page_size", pageSize);                parameters.Add("p_order_by", $"{(orderBy ?? "created_date")}");
            parameters.Add("p_order_direction", orderDirection);

            var results = await _connection.QueryAsync<SalesLeadGridResult>(
                "SELECT * FROM sales_lead_grid(" +
                "p_search_text => @p_search_text, " +
                "p_customer_names => @p_customer_names, " +
                "p_statuses => @p_statuses, " +
                "p_scores => @p_scores, " +
                "p_lead_types => @p_lead_types, " +
                "p_page_number => @p_page_number, " +
                "p_page_size => @p_page_size, " +
                "p_order_by => @p_order_by, " +
                "p_order_direction => @p_order_direction)",
                parameters
            );

                var resultsList = results.ToList();
                var totalRecords = resultsList.Any() ? resultsList.First().TotalRecords : 0;

                return (resultsList, totalRecords);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve sales leads grid: {ex.Message}", ex);
            }
        }

        public async Task<(IEnumerable<LeadsDropdownResult> Results, int TotalRecords)> GetLeadsDropdownAsync(
            string? searchText = null,
            int? pageNumber = 1,
            int? pageSize = 10)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("p_search_text", string.IsNullOrWhiteSpace(searchText) ? null : searchText);
                parameters.Add("p_page_number", pageNumber ?? 1);
                parameters.Add("p_page_size", pageSize ?? 10);

                var results = await _connection.QueryAsync<LeadsDropdownResult>(
                    @"SELECT * FROM get_leads_dropdown(
                        p_search_text := @p_search_text,
                        p_page_number := @p_page_number,
                        p_page_size := @p_page_size)",
                    parameters
                );

                var list = results.ToList();
                return (list, list.FirstOrDefault()?.TotalRecords ?? 0);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve leads dropdown: {ex.Message}", ex);
            }
        }
    }
}
