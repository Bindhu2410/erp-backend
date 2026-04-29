using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ERP.API.Services.Implementation.CompanySetup
{
    public class CsIntercompanyAccountService : BaseDataService<CsIntercompanyAccount>, ICsIntercompanyAccountService
    {
        public CsIntercompanyAccountService(IConfiguration configuration) 
            : base(configuration.GetConnectionString("DefaultConnection"), "cs_intercompany_accounts")
        {
        }

        public async Task<CsIntercompanyAccount?> GetByIdAsync(int accountId)
        {
            using var connection = CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_account_id", accountId);

            return await connection.QueryFirstOrDefaultAsync<CsIntercompanyAccount>(
                "SELECT * FROM sp_get_cs_intercompany_account_by_id(@p_account_id)",
                parameters);
        }

        public async Task<(IEnumerable<CsIntercompanyAccount> Data, int TotalRecords, int FilteredRecords)> GetByRelationshipAsync(CsIntercompanyAccountSearchDto searchDto)
        {
            using var connection = CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_relationship_id", searchDto.RelationshipId);
            parameters.Add("p_search_text", searchDto.SearchText);

            var data = await connection.QueryAsync<CsIntercompanyAccount>(
                "SELECT * FROM sp_get_cs_intercompany_accounts_by_relationship(@p_relationship_id, @p_search_text)",
                parameters);

            return (data, parameters.Get<int>("total_records"), parameters.Get<int>("filtered_records"));
        }

        public override async Task<int> CreateAsync(CsIntercompanyAccount account)
        {
            using var connection = CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_relationship_id", account.RelationshipId);
            parameters.Add("p_transaction_type", account.TransactionType);
            parameters.Add("p_company1_receivable_account_id", account.Company1ReceivableAccountId);
            parameters.Add("p_company2_payable_account_id", account.Company2PayableAccountId);
            parameters.Add("p_company1_tax_treatment_rule", account.Company1TaxTreatmentRule);
            parameters.Add("p_company2_tax_treatment_rule", account.Company2TaxTreatmentRule);
            parameters.Add("p_intercompany_account_id", dbType: DbType.Int32, direction: ParameterDirection.InputOutput);

            await connection.ExecuteAsync(
                GenerateInsertQuery(),
                parameters);

            return parameters.Get<int>("p_intercompany_account_id");
        }

        public override async Task<bool> UpdateAsync(CsIntercompanyAccount account)
        {
            using var connection = CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_intercompany_account_id", account.IntercompanyAccountId);
            parameters.Add("p_relationship_id", account.RelationshipId);
            parameters.Add("p_transaction_type", account.TransactionType);
            parameters.Add("p_company1_receivable_account_id", account.Company1ReceivableAccountId);
            parameters.Add("p_company2_payable_account_id", account.Company2PayableAccountId);
            parameters.Add("p_company1_tax_treatment_rule", account.Company1TaxTreatmentRule);
            parameters.Add("p_company2_tax_treatment_rule", account.Company2TaxTreatmentRule);
            parameters.Add("p_success", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                GenerateUpdateQuery(),
                parameters);

            return parameters.Get<bool>("p_success");
        }

        public override async Task<bool> DeleteAsync(int accountId)
        {
            using var connection = CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_intercompany_account_id", accountId);
            parameters.Add("p_success", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "CALL sp_delete_cs_intercompany_account(@p_intercompany_account_id, @p_success)",
                parameters);

            return parameters.Get<bool>("p_success");
        }

        protected override string GenerateInsertQuery()
        {
            return "CALL sp_create_cs_intercompany_account(@p_relationship_id, @p_transaction_type, @p_company1_receivable_account_id, @p_company2_payable_account_id, @p_company1_tax_treatment_rule, @p_company2_tax_treatment_rule, @p_intercompany_account_id)";
        }

        protected override string GenerateUpdateQuery()
        {
            return "CALL sp_update_cs_intercompany_account(@p_intercompany_account_id, @p_relationship_id, @p_transaction_type, @p_company1_receivable_account_id, @p_company2_payable_account_id, @p_company1_tax_treatment_rule, @p_company2_tax_treatment_rule, @p_success)";
        }
    }
}
