using System.Threading.Tasks;
using System.Linq;
using System.Data;
using Dapper;
using ERP.API.Models;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;

namespace ERP.API.Services.Implementation.CompanySetup
{
    public class CsOpeningBalanceService : ICsOpeningBalanceService
    {
        private readonly IDbConnection _db;

        public CsOpeningBalanceService(IDbConnection db)
        {
            _db = db;
        }

        public async Task<PagedResponse<CsOpeningBalanceDto>> SearchAsync(CsOpeningBalanceSearchDto searchDto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_company_id", searchDto.CompanyId);
            parameters.Add("@p_account_id", searchDto.AccountId);
            parameters.Add("@p_period_id", searchDto.PeriodId);
            parameters.Add("@p_page_size", searchDto.PageSize);
            parameters.Add("@p_page_number", searchDto.PageNumber);

            var result = await _db.QueryAsync<CsOpeningBalanceDto>(
                "SELECT * FROM sp_cs_opening_balances_search(@p_company_id, @p_account_id, @p_period_id, @p_page_size, @p_page_number)",
                parameters
            );

            var totalRecords = result.FirstOrDefault()?.TotalRecords ?? 0;

            return new PagedResponse<CsOpeningBalanceDto>
            {
                Data = result,
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize,
                TotalRecords = totalRecords
            };
        }

        public async Task<CsOpeningBalanceDto> GetByIdAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_balance_id", id);

            var result = await _db.QueryFirstOrDefaultAsync<CsOpeningBalanceDto>(
                "SELECT * FROM sp_cs_opening_balances_get_by_id(@p_balance_id)",
                parameters
            );

            return result;
        }

        public async Task<int> CreateAsync(CsOpeningBalanceDto balanceDto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_company_id", balanceDto.CompanyId);
            parameters.Add("@p_account_id", balanceDto.AccountId);
            parameters.Add("@p_period_id", balanceDto.PeriodId);
            parameters.Add("@p_balance_amount", balanceDto.BalanceAmount);
            parameters.Add("@p_balance_type", balanceDto.BalanceType);

            var balanceId = await _db.ExecuteScalarAsync<int>(
                "SELECT sp_cs_opening_balances_create(@p_company_id, @p_account_id, @p_period_id, @p_balance_amount, @p_balance_type)",
                parameters
            );

            return balanceId;
        }

        public async Task<bool> UpdateAsync(CsOpeningBalanceDto balanceDto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_balance_id", balanceDto.BalanceId);
            parameters.Add("@p_company_id", balanceDto.CompanyId);
            parameters.Add("@p_account_id", balanceDto.AccountId);
            parameters.Add("@p_period_id", balanceDto.PeriodId);
            parameters.Add("@p_balance_amount", balanceDto.BalanceAmount);
            parameters.Add("@p_balance_type", balanceDto.BalanceType);

            var result = await _db.ExecuteScalarAsync<bool>(
                "SELECT sp_cs_opening_balances_update(@p_balance_id, @p_company_id, @p_account_id, @p_period_id, @p_balance_amount, @p_balance_type)",
                parameters
            );

            return result;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_balance_id", id);

            var result = await _db.ExecuteScalarAsync<bool>(
                "SELECT sp_cs_opening_balances_delete(@p_balance_id)",
                parameters
            );

            return result;
        }

        public async Task<PagedResponse<CsOpeningBalanceDto>> GetByCompanyPeriodAsync(int companyId, int periodId, int pageNumber = 1, int pageSize = 10)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_company_id", companyId);
            parameters.Add("@p_period_id", periodId);
            parameters.Add("@p_page_number", pageNumber);
            parameters.Add("@p_page_size", pageSize);

            var result = await _db.QueryAsync<CsOpeningBalanceDto>(
                "SELECT * FROM sp_get_cs_opening_balances_by_company_period(@p_company_id, @p_period_id, @p_page_number, @p_page_size)",
                parameters
            );

            var totalRecords = result.FirstOrDefault()?.TotalRecords ?? 0;

            return new PagedResponse<CsOpeningBalanceDto>
            {
                Data = result,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }
    }
}
