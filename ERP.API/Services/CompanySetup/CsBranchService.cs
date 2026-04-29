using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ERP.API.Services.CompanySetup
{
    public class CsBranchService : BaseDataService<CsBranch>, ICsBranchService
    {
        private readonly ILogger<CsBranchService> _logger;

        public CsBranchService(string connectionString, ILogger<CsBranchService> logger)
            : base(connectionString, "cs_branches")
        {
            _logger = logger;
        }

        public async Task<CsBranchCreateResponseDto> CreateBranchAsync(CreateCsBranchDto createDto)
        {
            try
            {
                using var connection = CreateConnection();
                var result = await connection.QueryFirstAsync<CsBranchCreateResponseDto>(
                    "SELECT * FROM sp_create_cs_branch(@CompanyId, @BranchName, @BranchCode, " +
                    "@AddressLine1, @AddressLine2, @City, @State, @Pincode, " +
                    "@PhoneNumber, @EmailAddress, @Gstin, @IsHeadOffice, @IsActive)",
                    new
                    {
                        createDto.CompanyId,
                        createDto.BranchName,
                        createDto.BranchCode,
                        createDto.AddressLine1,
                        createDto.AddressLine2,
                        createDto.City,
                        createDto.State,
                        createDto.Pincode,
                        createDto.PhoneNumber,
                        createDto.EmailAddress,
                        createDto.Gstin,
                        createDto.IsHeadOffice,
                        createDto.IsActive
                    });

                if (result.Success)
                {
                    _logger.LogInformation("Branch created successfully with ID: {BranchId}", result.OutBranchId);
                }
                else
                {
                    _logger.LogWarning("Branch creation failed: {Message}", result.OutMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating branch: {BranchName}", createDto.BranchName);
                return new CsBranchCreateResponseDto 
                { 
                    OutMessage = $"Error creating branch: {ex.Message}" 
                };
            }
        }

        public async Task<(bool Success, string Message)> UpdateBranchAsync(UpdateCsBranchDto updateDto)
        {
            try
            {
                using var connection = CreateConnection();
                
                // Create JSON object for the stored procedure
                var branchData = new
                {
                    branch_id = updateDto.BranchId,
                    branch_name = updateDto.BranchName,
                    branch_code = updateDto.BranchCode,
                    address_line1 = updateDto.AddressLine1,
                    address_line2 = updateDto.AddressLine2,
                    city = updateDto.City,
                    state = updateDto.State,
                    pincode = updateDto.Pincode,
                    phone_number = updateDto.PhoneNumber,
                    email_address = updateDto.EmailAddress,
                    gstin = updateDto.Gstin,
                    is_head_office = updateDto.IsHeadOffice,
                    is_active = updateDto.IsActive
                };

                var jsonData = JsonConvert.SerializeObject(branchData);

                var result = await connection.QueryFirstAsync<(bool Success, string Message)>(
                    "SELECT * FROM sp_update_cs_branch(@BranchData::jsonb)",
                    new { BranchData = jsonData });

                if (result.Success)
                {
                    _logger.LogInformation("Branch updated successfully: {BranchId}", updateDto.BranchId);
                }
                else
                {
                    _logger.LogWarning("Branch update failed: {Message}", result.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating branch: {BranchId}", updateDto.BranchId);
                return (false, $"Error updating branch: {ex.Message}");
            }
        }

        public async Task<bool> ValidateBranchCompanyAsync(int branchId, int companyId)
        {
            try
            {
                using var connection = CreateConnection();
                var result = await connection.QuerySingleAsync<bool>(
                    "SELECT sp_validate_cs_branch_company(@BranchId, @CompanyId)",
                    new { BranchId = branchId, CompanyId = companyId });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating branch company: BranchId={BranchId}, CompanyId={CompanyId}", branchId, companyId);
                return false;
            }
        }

        public async Task<IEnumerable<CsBranchDto>> GetBranchesByCompanyAsync(int companyId, bool includeInactive = false)
        {
            try
            {
                using var connection = CreateConnection();
                var branches = await connection.QueryAsync<CsBranchDto>(
                    "SELECT " +
                    "branch_id as BranchId, " +
                    "company_id as CompanyId, " +
                    "branch_code as BranchCode, " +
                    "branch_name as BranchName, " +
                    "address_line1 as BranchAddressLine1, " +
                    "address_line2 as BranchAddressLine2, " +
                    "city as City, " +
                    "state as State, " +
                    "pincode as Pincode, " +
                    "phone_number as BranchPhoneNumber, " +
                    "email_address as BranchEmailAddress, " +
                    "gstin as BranchGstin, " +
                    "is_active as IsActive, " +
                    "is_head_office as IsHeadOffice, " +
                    "created_at as CreatedAt, " +
                    "updated_at as UpdatedAt " +
                    "FROM sp_get_cs_branches_by_company(@CompanyId, @IncludeInactive)",
                    new { CompanyId = companyId, IncludeInactive = includeInactive });

                return branches ?? Enumerable.Empty<CsBranchDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branches by company: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<IEnumerable<CsBranchDropdownDto>> GetBranchesDropdownAsync(int companyId, bool activeOnly = true)
        {
            try
            {
                using var connection = CreateConnection();
                var branches = await connection.QueryAsync<CsBranchDropdownDto>(
                    "SELECT " +
                    "branch_id as BranchId, " +
                    "branch_code as BranchCode, " +
                    "branch_name as BranchName, " +
                    "is_head_office as IsHeadOffice, " +
                    "full_address as FullAddress " +
                    "FROM sp_get_cs_branches_dropdown(@CompanyId, @ActiveOnly)",
                    new { CompanyId = companyId, ActiveOnly = activeOnly });

                return branches ?? Enumerable.Empty<CsBranchDropdownDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branches dropdown: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<CsBranchPagedResponseDto> GetAllBranchesAsync(CsBranchPagedRequestDto request)
        {
            try
            {
                using var connection = CreateConnection();
                var branches = await connection.QueryAsync<CsBranchDto>(
                    "SELECT " +
                    "branch_id as BranchId, " +
                    "company_id as CompanyId, " +
                    "branch_name as BranchName, " +
                    "branch_code as BranchCode, " +
                    "address_line1 as BranchAddressLine1, " +
                    "address_line2 as BranchAddressLine2, " +
                    "city as City, " +
                    "state as State, " +
                    "pincode as Pincode, " +
                    "phone_number as BranchPhoneNumber, " +
                    "email_address as BranchEmailAddress, " +
                    "gstin as BranchGstin, " +
                    "is_head_office as IsHeadOffice, " +
                    "is_active as IsActive, " +
                    "created_at as CreatedAt, " +
                    "updated_at as UpdatedAt, " +
                    "company_name as CompanyName, " +
                    "company_code as CompanyCode, " +
                    "total_count as TotalCount " +
                    "FROM sp_get_all_cs_branches(@PageNumber, @PageSize, @CompanyId, @IsActive)",
                    new 
                    { 
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize,
                        CompanyId = request.CompanyId,
                        IsActive = request.IsActive
                    });

                var branchList = branches.ToList();
                var totalCount = branchList.FirstOrDefault()?.TotalCount ?? 0;

                return new CsBranchPagedResponseDto
                {
                    Branches = branchList,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all branches");
                throw;
            }
        }

        public async Task<(bool Success, string Message)> DeleteBranchAsync(int branchId, int companyId)
        {
            try
            {
                using var connection = CreateConnection();
                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_delete_cs_branch(@BranchId, @CompanyId)",
                    new
                    {
                        BranchId = branchId,
                        CompanyId = companyId
                    });

                return (result.success, result.message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting branch: {BranchId}, {CompanyId}", branchId, companyId);
                throw;
            }
        }

        protected override string GenerateInsertQuery()
        {
            // This method is required by BaseDataService but we're using stored procedures
            throw new NotImplementedException("Use CreateBranchAsync method instead");
        }

        protected override string GenerateUpdateQuery()
        {
            // This method is required by BaseDataService but we're using stored procedures
            throw new NotImplementedException("Use UpdateBranchAsync method instead");
        }
    }
}
