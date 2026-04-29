using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models.DTOs.CompanySetup;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ERP.API.Services.CompanySetup
{
    public class FinancialStatementTemplateService
    {
        private readonly string _connectionString;

        public FinancialStatementTemplateService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<int> CreateAsync(FinancialStatementTemplateCreateDto dto)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(
                "SELECT sp_create_financial_statement_template(@TemplateCode, @TemplateName, @TemplateType, @TemplateDescription, @CreatedBy, @AccountingStandard, @IsDefault, @IsActive)",
                dto);
        }

        public async Task UpdateAsync(FinancialStatementTemplateUpdateDto dto)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "CALL sp_update_financial_statement_template(@TemplateId, @TemplateName, @TemplateDescription, @AccountingStandard, @TemplateType, @IsDefault, @IsActive, @ModifiedBy)",
                dto);
        }

        public async Task<FinancialStatementTemplateDto?> GetByIdAsync(int templateId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<FinancialStatementTemplateDto>(
                "SELECT * FROM sp_get_financial_statement_template_by_id(@TemplateId)", new { TemplateId = templateId });
        }

        public async Task<IEnumerable<FinancialStatementTemplateDto>> GetAllAsync()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QueryAsync<FinancialStatementTemplateDto>(
                "SELECT * FROM sp_get_all_financial_statement_templates()", null);
        }

        public async Task DeleteAsync(FinancialStatementTemplateDeleteDto dto)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "CALL sp_delete_financial_statement_template(@TemplateId)",
                dto);
        }
    }
}
