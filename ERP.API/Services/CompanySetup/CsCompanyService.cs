using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services;
using Microsoft.Extensions.Logging;

namespace ERP.API.Services.CompanySetup
{
    public class CsCompanyService : BaseDataService<CsCompany>, ICsCompanyService
    {
        private readonly ILogger<CsCompanyService> _logger;

        public CsCompanyService(string connectionString, ILogger<CsCompanyService> logger)
            : base(connectionString, "cs_companies")
        {
            _logger = logger;
        }

        public async Task<int> CreateCompanyAsync(CreateCsCompanyDto createDto)
        {
            try
            {
                using var connection = CreateConnection();
                var companyId = await connection.QuerySingleAsync<int>(
                    "SELECT sp_create_cs_company(@LegalCompanyName, @ParentCompanyId, @RegisteredAddressLine1, " +
                    "@RegisteredAddressLine2, @City, @State, @Pincode, @PhoneNumber, @EmailAddress, " +
                    "@WebsiteUrl, @CompanyLogoPath, @BaseCurrency, @FinancialYearStartDate, " +
                    "@FinancialYearEndDate, @Pan, @Tan, @Gstin, @LegalEntityType, @LegalNameAsPerPanTan)",
                    new
                    {
                        createDto.LegalCompanyName,
                        createDto.ParentCompanyId,
                        createDto.RegisteredAddressLine1,
                        createDto.RegisteredAddressLine2,
                        createDto.City,
                        createDto.State,
                        createDto.Pincode,
                        createDto.PhoneNumber,
                        createDto.EmailAddress,
                        createDto.WebsiteUrl,
                        createDto.CompanyLogoPath,
                        createDto.BaseCurrency,
                        createDto.FinancialYearStartDate,
                        createDto.FinancialYearEndDate,
                        createDto.Pan,
                        createDto.Tan,
                        createDto.Gstin,
                        createDto.LegalEntityType,
                        createDto.LegalNameAsPerPanTan
                    });

                _logger.LogInformation("Company created successfully with ID: {CompanyId}", companyId);
                return companyId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating company: {LegalCompanyName}", createDto.LegalCompanyName);
                throw;
            }
        }

        public async Task<bool> UpdateCompanyAsync(UpdateCsCompanyDto updateDto)
        {
            try
            {
                using var connection = CreateConnection();
                var result = await connection.QuerySingleAsync<bool>(
                    "SELECT sp_update_cs_company(@CompanyId, @ParentCompanyId, @LegalCompanyName, " +
                    "@RegisteredAddressLine1, @RegisteredAddressLine2, @City, @State, @Pincode, " +
                    "@PhoneNumber, @EmailAddress, @WebsiteUrl, @CompanyLogoPath, @BaseCurrency, " +
                    "@FinancialYearStartDate, @FinancialYearEndDate, @Pan, @Tan, @Gstin, " +
                    "@LegalEntityType, @LegalNameAsPerPanTan)",
                    new
                    {
                        updateDto.CompanyId,
                        updateDto.ParentCompanyId,
                        updateDto.LegalCompanyName,
                        updateDto.RegisteredAddressLine1,
                        updateDto.RegisteredAddressLine2,
                        updateDto.City,
                        updateDto.State,
                        updateDto.Pincode,
                        updateDto.PhoneNumber,
                        updateDto.EmailAddress,
                        updateDto.WebsiteUrl,
                        updateDto.CompanyLogoPath,
                        updateDto.BaseCurrency,
                        updateDto.FinancialYearStartDate,
                        updateDto.FinancialYearEndDate,
                        updateDto.Pan,
                        updateDto.Tan,
                        updateDto.Gstin,
                        updateDto.LegalEntityType,
                        updateDto.LegalNameAsPerPanTan
                    });

                _logger.LogInformation("Company updated successfully: {CompanyId}", updateDto.CompanyId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company: {CompanyId}", updateDto.CompanyId);
                throw;
            }
        }

        public async Task<bool> DeleteCompanyAsync(int companyId, bool forceDelete = false)
        {
            try
            {
                using var connection = CreateConnection();
                var result = await connection.QuerySingleAsync<bool>(
                    "SELECT sp_delete_cs_company(@CompanyId, @ForceDelete)",
                    new { CompanyId = companyId, ForceDelete = forceDelete });

                _logger.LogInformation("Company deleted successfully: {CompanyId}", companyId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting company: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<CsCompanyDto?> GetCompanyByIdAsync(int companyId)
        {
            try
            {
                using var connection = CreateConnection();
                var company = await connection.QueryFirstOrDefaultAsync<CsCompanyDto>(
                    "SELECT * FROM sp_get_cs_company_by_id(@CompanyId)",
                    new { CompanyId = companyId });

                return company;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company by ID: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<IEnumerable<CsCompanyDto>> GetAllCompaniesAsync()
        {
            try
            {
                using var connection = CreateConnection();
                var companies = await connection.QueryAsync<CsCompanyDto>(
                    "SELECT * FROM sp_get_all_cs_companies()");

                return companies ?? Enumerable.Empty<CsCompanyDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all companies");
                throw;
            }
        }

        public async Task<IEnumerable<CsCompanyDto>> SearchCompaniesAsync(CsCompanySearchDto searchDto)
        {
            try
            {
                using var connection = CreateConnection();
                
                // If only search term is provided, use the single-parameter overload
                if (!string.IsNullOrEmpty(searchDto.SearchTerm) && 
                    searchDto.ParentCompanyId == null && 
                    string.IsNullOrEmpty(searchDto.LegalEntityType))
                {
                    var companies = await connection.QueryAsync<CsCompanyDto>(
                        "SELECT * FROM sp_search_cs_companies(@SearchTerm)",
                        new { SearchTerm = searchDto.SearchTerm });
                    
                    return companies ?? Enumerable.Empty<CsCompanyDto>();
                }
                else
                {
                    // Use the three-parameter overload
                    var companies = await connection.QueryAsync<CsCompanyDto>(
                        "SELECT * FROM sp_search_cs_companies(@SearchTerm, @ParentCompanyId, @LegalEntityType)",
                        new 
                        { 
                            SearchTerm = searchDto.SearchTerm,
                            ParentCompanyId = searchDto.ParentCompanyId,
                            LegalEntityType = searchDto.LegalEntityType
                        });
                    
                    return companies ?? Enumerable.Empty<CsCompanyDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching companies");
                throw;
            }
        }

        public async Task<IEnumerable<CsCompanyHierarchyDto>> GetCompanyHierarchyAsync()
        {
            try
            {
                using var connection = CreateConnection();
                var hierarchy = await connection.QueryAsync<CsCompanyHierarchyDto>(
                    "SELECT * FROM sp_get_cs_company_hierarchy()");

                return hierarchy ?? Enumerable.Empty<CsCompanyHierarchyDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company hierarchy");
                throw;
            }
        }

        protected override string GenerateInsertQuery()
        {
            // This method is required by BaseDataService but we're using stored procedures
            // so we don't need to implement it for this service
            throw new NotImplementedException("Use CreateCompanyAsync method instead");
        }

        protected override string GenerateUpdateQuery()
        {
            // This method is required by BaseDataService but we're using stored procedures
            // so we don't need to implement it for this service
            throw new NotImplementedException("Use UpdateCompanyAsync method instead");
        }
    }
}
