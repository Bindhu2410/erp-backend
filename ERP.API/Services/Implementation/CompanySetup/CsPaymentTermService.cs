using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Dapper;
using ERP.API.Models;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;

namespace ERP.API.Services.Implementation.CompanySetup
{
    public class CsPaymentTermService : ICsPaymentTermService
    {
        private readonly IDbConnection _db;

        public CsPaymentTermService(IDbConnection db)
        {
            _db = db;
        }

        public async Task<PagedResponse<CsPaymentTermDto>> SearchAsync(CsPaymentTermSearchDto searchDto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_company_id", searchDto.CompanyId);
            parameters.Add("@p_term_name", searchDto.TermName);
            parameters.Add("@p_calculation_type", searchDto.CalculationType);
            parameters.Add("@p_page_size", searchDto.PageSize);
            parameters.Add("@p_page_number", searchDto.PageNumber);

            var result = await _db.QueryAsync<CsPaymentTermDto>(
                "SELECT * FROM sp_cs_payment_terms_search(@p_company_id, @p_term_name, @p_calculation_type, @p_page_size, @p_page_number)",
                parameters
            );

            var totalRecords = result.FirstOrDefault()?.TotalRecords ?? 0;

            return new PagedResponse<CsPaymentTermDto>
            {
                Data = result,
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize,
                TotalRecords = totalRecords
            };
        }

        public async Task<CsPaymentTermDto> GetByIdAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_term_id", id);

            return await _db.QueryFirstOrDefaultAsync<CsPaymentTermDto>(
                "SELECT * FROM sp_cs_payment_terms_get_by_id(@p_term_id)",
                parameters
            );
        }

        public async Task<int> CreateAsync(CsPaymentTermDto dto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_company_id", dto.CompanyId);
            parameters.Add("@p_term_name", dto.TermName);
            parameters.Add("@p_calculation_type", dto.CalculationType);
            parameters.Add("@p_due_days", dto.DueDays);
            parameters.Add("@p_discount_percentage", dto.DiscountPercentage);
            parameters.Add("@p_discount_days", dto.DiscountDays);

            return await _db.ExecuteScalarAsync<int>(
                "SELECT sp_cs_payment_terms_create(@p_company_id, @p_term_name, @p_calculation_type, @p_due_days, @p_discount_percentage, @p_discount_days)",
                parameters
            );
        }

        public async Task<bool> UpdateAsync(CsPaymentTermDto dto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_term_id", dto.TermId);
            parameters.Add("@p_company_id", dto.CompanyId);
            parameters.Add("@p_term_name", dto.TermName);
            parameters.Add("@p_calculation_type", dto.CalculationType);
            parameters.Add("@p_due_days", dto.DueDays);
            parameters.Add("@p_discount_percentage", dto.DiscountPercentage);
            parameters.Add("@p_discount_days", dto.DiscountDays);

            return await _db.ExecuteScalarAsync<bool>(
                "SELECT sp_cs_payment_terms_update(@p_term_id, @p_company_id, @p_term_name, @p_calculation_type, @p_due_days, @p_discount_percentage, @p_discount_days)",
                parameters
            );
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_term_id", id);

            return await _db.ExecuteScalarAsync<bool>(
                "SELECT sp_cs_payment_terms_delete(@p_term_id)",
                parameters
            );
        }

        public async Task<PagedResponse<CsPaymentTermDto>> GetByCompanyAsync(int companyId, int pageNumber = 1, int pageSize = 10)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_company_id", companyId);
            parameters.Add("@p_page_number", pageNumber);
            parameters.Add("@p_page_size", pageSize);

            var result = await _db.QueryAsync<CsPaymentTermDto>(
                "SELECT * FROM sp_cs_payment_terms_get_by_company(@p_company_id, @p_page_size, @p_page_number)",
                parameters
            );

            var totalRecords = result.FirstOrDefault()?.TotalRecords ?? 0;

            return new PagedResponse<CsPaymentTermDto>
            {
                Data = result,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        public async Task<IEnumerable<CsPaymentTermDto>> GetAllPaymentTermsAsync()
        {
            return await _db.QueryAsync<CsPaymentTermDto>(
                "SELECT * FROM sp_cs_getall_payment_terms()"
            );
        }
    }
}
