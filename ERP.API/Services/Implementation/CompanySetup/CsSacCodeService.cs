        
using System.Threading.Tasks;
using System.Linq;
using System.Data;
using Dapper;
using ERP.API.Models;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;

namespace ERP.API.Services.Implementation.CompanySetup
{
    public class CsSacCodeService : ICsSacCodeService
    {
        private readonly IDbConnection _db;

        public CsSacCodeService(IDbConnection db)
        {
            _db = db;
        }

        public async Task<PagedResponse<CsSacCodeDto>> SearchAsync(CsSacCodeSearchDto searchDto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_company_id", searchDto.CompanyId);
            parameters.Add("@p_sac_code", searchDto.SacCode);
            parameters.Add("@p_description", searchDto.Description);
            parameters.Add("@p_page_size", searchDto.PageSize);
            parameters.Add("@p_page_number", searchDto.PageNumber);

            var result = await _db.QueryAsync<CsSacCodeDto>(
                "SELECT * FROM sp_cs_sac_codes_search(@p_company_id, @p_sac_code, @p_description, @p_page_size, @p_page_number)",
                parameters
            );

            var totalRecords = result.FirstOrDefault()?.TotalRecords ?? 0;

            return new PagedResponse<CsSacCodeDto>
            {
                Data = result,
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize,
                TotalRecords = totalRecords
            };
        }

        public async Task<CsSacCodeDto> GetByIdAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_sac_code_id", id);

            return await _db.QueryFirstOrDefaultAsync<CsSacCodeDto>(
                "SELECT * FROM sp_cs_sac_codes_get_by_id(@p_sac_code_id)",
                parameters
            );
        }
       public async Task<IEnumerable<CsSacCodeDto>> GetAllAsync()
        {
            var result = await _db.QueryAsync<CsSacCodeDto>(
                "SELECT * FROM sp_get_all_cs_sac_codes()"
            );
            return result;
        }
        public async Task<int> CreateAsync(CsSacCodeDto dto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_company_id", dto.CompanyId);
            parameters.Add("@p_sac_code", dto.SacCode);
            parameters.Add("@p_description", dto.Description);
            parameters.Add("@p_default_gst_rate", dto.DefaultGstRate);

            return await _db.ExecuteScalarAsync<int>(
                "SELECT sp_cs_sac_codes_create(@p_company_id, @p_sac_code, @p_description, @p_default_gst_rate)",
                parameters
            );
        }

        public async Task<bool> UpdateAsync(CsSacCodeDto dto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_sac_code_id", dto.SacCodeId);
            parameters.Add("@p_company_id", dto.CompanyId);
            parameters.Add("@p_sac_code", dto.SacCode);
            parameters.Add("@p_description", dto.Description);
            parameters.Add("@p_default_gst_rate", dto.DefaultGstRate);

            return await _db.ExecuteScalarAsync<bool>(
                "SELECT sp_cs_sac_codes_update(@p_sac_code_id, @p_company_id, @p_sac_code, @p_description, @p_default_gst_rate)",
                parameters
            );
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_sac_code_id", id);

            return await _db.ExecuteScalarAsync<bool>(
                "SELECT sp_cs_sac_codes_delete(@p_sac_code_id)",
                parameters
            );
        }

        public async Task<PagedResponse<CsSacCodeDto>> GetByCompanyAsync(int companyId, int pageNumber = 1, int pageSize = 10)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@p_company_id", companyId);
            parameters.Add("@p_page_number", pageNumber);
            parameters.Add("@p_page_size", pageSize);

            var result = await _db.QueryAsync<CsSacCodeDto>(
                "SELECT * FROM sp_cs_sac_codes_get_by_company(@p_company_id, @p_page_size, @p_page_number)",
                parameters
            );

            var totalRecords = result.FirstOrDefault()?.TotalRecords ?? 0;

            return new PagedResponse<CsSacCodeDto>
            {
                Data = result,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }
    }
}
