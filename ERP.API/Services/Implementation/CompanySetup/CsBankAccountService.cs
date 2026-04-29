using Dapper;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.API.Services.Implementation.CompanySetup
{
    public class CsBankAccountService : ICsBankAccountService
    {
        private readonly string _connectionString;
        private readonly ILogger<CsBankAccountService> _logger;

        public CsBankAccountService(string connectionString, ILogger<CsBankAccountService> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<CsBankAccount> CreateBankAccountAsync(CsBankAccount bankAccount)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var result = await connection.QuerySingleAsync<CsBankAccount>(
                    "SELECT * FROM sp_create_cs_bank_account(@CompanyId, @BankName, @BankBranchName, " +
                    "@AccountNumber, @IFSCCode, @SwiftCode, @Purpose, @Currency)",
                    new { 
                        CompanyId = bankAccount.CompanyId,
                        BankName = bankAccount.BankName,
                        BankBranchName = bankAccount.BankBranchName,
                        AccountNumber = bankAccount.AccountNumber,
                        IFSCCode = bankAccount.IFSCCode,
                        SwiftCode = bankAccount.SwiftCode,
                        Purpose = bankAccount.Purpose,
                        Currency = bankAccount.Currency
                    });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bank account");
                throw;
            }
        }

        public async Task<CsBankAccount> UpdateBankAccountAsync(CsBankAccount bankAccount)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var result = await connection.QuerySingleAsync<CsBankAccount>(
                    "SELECT * FROM sp_update_cs_bank_account(@BankAccountId, @BankName, @BankBranchName, " + 
                    "@AccountNumber, @IFSCCode, @SwiftCode, @Purpose, @Currency)",
                    new { 
                        BankAccountId = bankAccount.BankAccountId,
                        BankName = bankAccount.BankName,
                        BankBranchName = bankAccount.BankBranchName,
                        AccountNumber = bankAccount.AccountNumber,
                        IFSCCode = bankAccount.IFSCCode,
                        SwiftCode = bankAccount.SwiftCode,
                        Purpose = bankAccount.Purpose,
                        Currency = bankAccount.Currency
                    });
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bank account");
                throw;
            }
        }

        public async Task<bool> DeleteBankAccountAsync(int bankAccountId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.QuerySingleAsync<bool>(
                    "SELECT sp_delete_cs_bank_account(@BankAccountId)",
                    new { BankAccountId = bankAccountId });
            }
            catch (PostgresException ex) when (ex.SqlState == "P0001")
            {
                // Handle business rule violations gracefully
                _logger.LogWarning("Cannot delete bank account {BankAccountId}: {Message}", bankAccountId, ex.MessageText);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting bank account");
                throw;
            }
        }

        public async Task<CsBankAccount> GetBankAccountByIdAsync(int bankAccountId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.QuerySingleOrDefaultAsync<CsBankAccount>(
                    "SELECT * FROM sp_get_cs_bank_account_by_id(@BankAccountId)",
                    new { BankAccountId = bankAccountId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bank account by ID");
                throw;
            }
        }

        public async Task<(IEnumerable<CsBankAccount> bankAccounts, long totalCount)> GetBankAccountsByCompanyAsync(int companyId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var results = await connection.QueryAsync<CsBankAccount>(
                    "SELECT * FROM sp_get_cs_bank_accounts_by_company(@CompanyId, @PageNumber, @PageSize)",
                    new {
                        CompanyId = companyId,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    });

                var totalCount = results.Any() ? results.First().TotalCount : 0;
                return (results, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bank accounts by company");
                throw;
            }
        }

        public async Task<(IEnumerable<CsBankAccount> bankAccounts, long totalCount)> SearchBankAccountsAsync(int? companyId, string? searchText = null, string? purpose = null, string? currency = null, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                if (companyId.HasValue && companyId.Value > 0)
                {
                    var results = await connection.QueryAsync<CsBankAccount>(
                        "SELECT * FROM sp_search_cs_bank_accounts(@CompanyId, @SearchText, @Purpose, @Currency, @PageNumber, @PageSize)",
                        new {
                            CompanyId = companyId.Value,
                            SearchText = searchText,
                            Purpose = purpose,
                            Currency = currency,
                            PageNumber = pageNumber,
                            PageSize = pageSize
                        });

                    var totalCount = results.Any() ? results.First().TotalCount : 0;
                    return (results, totalCount);
                }

                // companyId not provided -> search across all companies
                var whereClauses = new List<string>();
                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    whereClauses.Add("(ba.bank_name ILIKE @SearchPattern OR ba.bank_branch_name ILIKE @SearchPattern OR ba.account_number ILIKE @SearchPattern OR ba.ifsc_code ILIKE @SearchPattern)");
                    parameters.Add("SearchPattern", $"%{searchText}%");
                }

                if (!string.IsNullOrWhiteSpace(purpose))
                {
                    whereClauses.Add("ba.purpose = @Purpose");
                    parameters.Add("Purpose", purpose);
                }

                if (!string.IsNullOrWhiteSpace(currency))
                {
                    whereClauses.Add("ba.currency = @Currency");
                    parameters.Add("Currency", currency);
                }

                parameters.Add("PageSize", pageSize);
                parameters.Add("Offset", (pageNumber - 1) * pageSize);

                var whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : string.Empty;

                var countSql = $@"SELECT COUNT(*) FROM cs_bank_accounts ba {whereSql}";
                var total = await connection.ExecuteScalarAsync<long>(countSql, parameters);

                var dataSql = $@"
SELECT
    ba.bank_account_id AS BankAccountId,
    ba.company_id AS CompanyId,
    ba.bank_name AS BankName,
    ba.bank_branch_name AS BankBranchName,
    ba.account_number AS AccountNumber,
    ba.ifsc_code AS IFSCCode,
    ba.swift_code AS SwiftCode,
    ba.purpose AS Purpose,
    ba.currency AS Currency,
    ba.created_at AS CreatedAt,
    ba.updated_at AS UpdatedAt
FROM cs_bank_accounts ba
{whereSql}
ORDER BY ba.created_at DESC
LIMIT @PageSize OFFSET @Offset";

                var rows = (await connection.QueryAsync<CsBankAccount>(dataSql, parameters)).ToList();
                foreach (var r in rows) r.TotalCount = total;
                return (rows, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching bank accounts");
                throw;
            }
        }

        // DTO-based methods implementation
        public async Task<CsBankAccount> CreateBankAccountAsync(CsBankAccountDto bankAccountDto)
        {
            try
            {
                _logger.LogInformation("Creating bank account for company {CompanyId} with data: {@BankAccountDto}", 
                    bankAccountDto.CompanyId, bankAccountDto);

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                _logger.LogInformation("Database connection opened successfully");

                var result = await connection.QuerySingleAsync<CsBankAccount>(
                    "SELECT * FROM sp_create_cs_bank_account(@CompanyId, @BankName, @BankBranchName, " +
                    "@AccountNumber, @IFSCCode, @SwiftCode, @Purpose, @Currency)",
                    new { 
                        CompanyId = bankAccountDto.CompanyId,
                        BankName = bankAccountDto.BankName,
                        BankBranchName = bankAccountDto.BankBranchName,
                        AccountNumber = bankAccountDto.AccountNumber,
                        IFSCCode = bankAccountDto.IFSCCode,
                        SwiftCode = bankAccountDto.SwiftCode,
                        Purpose = bankAccountDto.Purpose,
                        Currency = bankAccountDto.Currency
                    });

                _logger.LogInformation("Bank account created successfully with ID: {BankAccountId}", result.BankAccountId);
                return result;
            }
            catch (PostgresException pgEx)
            {
                _logger.LogError(pgEx, "PostgreSQL error creating bank account. SqlState: {SqlState}, Message: {Message}", 
                    pgEx.SqlState, pgEx.MessageText);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General error creating bank account from DTO for company {CompanyId}", 
                    bankAccountDto?.CompanyId);
                throw;
            }
        }

        public async Task<CsBankAccount> UpdateBankAccountAsync(int bankAccountId, CsBankAccountDto bankAccountDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var result = await connection.QuerySingleAsync<CsBankAccount>(
                    "SELECT * FROM sp_update_cs_bank_account(@BankAccountId, @BankName, @BankBranchName, " + 
                    "@AccountNumber, @IFSCCode, @SwiftCode, @Purpose, @Currency)",
                    new { 
                        BankAccountId = bankAccountId,
                        BankName = bankAccountDto.BankName,
                        BankBranchName = bankAccountDto.BankBranchName,
                        AccountNumber = bankAccountDto.AccountNumber,
                        IFSCCode = bankAccountDto.IFSCCode,
                        SwiftCode = bankAccountDto.SwiftCode,
                        Purpose = bankAccountDto.Purpose,
                        Currency = bankAccountDto.Currency
                    });
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bank account from DTO");
                throw;
            }
        }
    }
}
