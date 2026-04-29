using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models.DTOs;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ERP.API.Services.CompanySetup
{
    public class JournalEntryTemplateService
    {
        private readonly string _connectionString;

        public JournalEntryTemplateService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<int> CreateAsync(JournalEntryTemplateCreateDto dto)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(
                "SELECT sp_create_journal_entry_template(@CompanyId, @TemplateCode, @TemplateName, @TemplateDescription, @TemplateCategoryId, @Frequency, @IsActive, @AutoReverse, @AutoReverseDays, @ApprovalRequired, @ApprovalWorkflowId, @AutoGenerate, @NextGenerationDate, @LastGeneratedDate, @GenerationCount, @Tags, @CreatedBy)",
                dto);
        }

        public async Task UpdateAsync(JournalEntryTemplateUpdateDto dto)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
    "CALL sp_update_journal_entry_template(@TemplateId, @TemplateCode, @TemplateName, @TemplateDescription, @TemplateCategoryId, @Frequency, @IsActive, @AutoReverse, @AutoReverseDays, @ApprovalRequired, @ApprovalWorkflowId, @AutoGenerate, @NextGenerationDate, @LastGeneratedDate, @GenerationCount, @Tags, @ModifiedBy)",
    dto);
        }

        public async Task<JournalEntryTemplateDto?> GetByIdAsync(int templateId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<JournalEntryTemplateDto>(
                "SELECT * FROM sp_get_journal_entry_template_by_id(@TemplateId)", new { TemplateId = templateId });
        }

        public async Task<IEnumerable<JournalEntryTemplateDto>> GetAllAsync()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QueryAsync<JournalEntryTemplateDto>(
                "SELECT * FROM sp_get_all_journal_entry_templates()", null);
        }

        public async Task DeleteAsync(JournalEntryTemplateDeleteDto dto)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "CALL sp_delete_journal_entry_template(@TemplateId)",
                dto);
        }
    }
}
