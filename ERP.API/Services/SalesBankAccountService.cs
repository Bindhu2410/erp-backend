using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ERP.API.Models;

namespace ERP.API.Services
{
    public class SalesBankAccountService : ISalesBankAccountService
    {
        private readonly string _connectionString;

        public SalesBankAccountService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                throw new ArgumentNullException(nameof(configuration), "DefaultConnection string is not configured");
        }        public async Task<IEnumerable<SalesBankAccount>> GetAllAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                return await connection.QueryAsync<SalesBankAccount>(@"
                    SELECT id, user_created, date_created, user_updated, date_updated,
                           branch, registered_company, name_of_the_bank, account_no, 
                           ifsc_code, account_holder_name, COALESCE(isactive, true) as isactive
                    FROM sales_bank_account 
                    WHERE COALESCE(isactive, true) = true 
                    ORDER BY id");
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving bank accounts", ex);
            }
        }

        public async Task<SalesBankAccount> GetByIdAsync(int id)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                var result = await connection.QueryFirstOrDefaultAsync<SalesBankAccount>(@"
                    SELECT id, user_created, date_created, user_updated, date_updated,
                           branch, registered_company, name_of_the_bank, account_no, 
                           ifsc_code, account_holder_name, COALESCE(isactive, true) as isactive
                    FROM sales_bank_account 
                    WHERE id = @Id AND COALESCE(isactive, true) = true", 
                    new { Id = id });
                
                if (result == null)
                    throw new KeyNotFoundException($"Bank account with ID {id} not found");
                    
                return result;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving bank account with ID {id}", ex);
            }
        }

        public async Task<int> CreateAsync(SalesBankAccount bankAccount)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"
                INSERT INTO sales_bank_account 
                (user_created, date_created, user_updated, date_updated, 
                branch, registered_company, name_of_the_bank, account_no, 
                ifsc_code, account_holder_name)
                VALUES 
                (@UserCreated, @DateCreated, @UserUpdated, @DateUpdated, 
                @Branch, @RegisteredCompany, @NameOfTheBank, @AccountNo, 
                @IFSCCode, @AccountHolderName)
                RETURNING id";

            bankAccount.DateCreated = DateTime.UtcNow;
            bankAccount.DateUpdated = DateTime.UtcNow;

            return await connection.ExecuteScalarAsync<int>(sql, bankAccount);
        }

        public async Task UpdateAsync(SalesBankAccount bankAccount)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"
                UPDATE sales_bank_account 
                SET user_updated = @UserUpdated, 
                    date_updated = @DateUpdated,
                    branch = @Branch,
                    registered_company = @RegisteredCompany,
                    name_of_the_bank = @NameOfTheBank,
                    account_no = @AccountNo,
                    ifsc_code = @IFSCCode,
                    account_holder_name = @AccountHolderName
                WHERE id = @Id";

            bankAccount.DateUpdated = DateTime.UtcNow;
            await connection.ExecuteAsync(sql, bankAccount);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE sales_bank_account SET isactive = false WHERE id = @Id",
                new { Id = id });
        }
    }
}
