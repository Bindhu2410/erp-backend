using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ERP.API.Models;
using Microsoft.Extensions.Logging;

namespace ERP.API.Services
{
    public class SalesTermsAndConditionsService : ISalesTermsAndConditionsService
    {
        private readonly string _connectionString;
        private readonly ILogger<SalesTermsAndConditionsService> _logger;

        public SalesTermsAndConditionsService(
            IConfiguration configuration,
            ILogger<SalesTermsAndConditionsService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                throw new ArgumentNullException(nameof(configuration), "DefaultConnection string is not configured");
            _logger = logger;
        }

        public async Task<IEnumerable<SalesTermsAndConditions>> GetAllAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.QueryAsync<SalesTermsAndConditions>(
                    "SELECT * FROM sales_terms_and_conditions WHERE is_active = true");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all terms and conditions");
                throw;
            }
        }

        public async Task<SalesTermsAndConditions> GetByIdAsync(int id)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var result = await connection.QueryFirstOrDefaultAsync<SalesTermsAndConditions>(
                    "SELECT * FROM sales_terms_and_conditions WHERE id = @Id AND is_active = true", 
                    new { Id = id });
                
                if (result == null)
                    throw new KeyNotFoundException($"Terms and conditions with ID {id} not found");
                    
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting terms and conditions by ID {Id}", id);
                throw;
            }
        }

        public async Task<SalesTermsAndConditions> GetByQuotationIdAsync(int quotationId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var result = await connection.QueryFirstOrDefaultAsync<SalesTermsAndConditions>(
                    "SELECT * FROM sales_terms_and_conditions WHERE quotation_id = @QuotationId AND is_active = true", 
                    new { QuotationId = quotationId });
                
                if (result == null)
                    throw new KeyNotFoundException($"Terms and conditions for quotation ID {quotationId} not found");
                    
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting terms and conditions by quotation ID {QuotationId}", quotationId);
                throw;
            }
        }

        public async Task<IEnumerable<SalesTermsAndConditions>> GetTermsByQuotationIdAsync(int quotationId)
        {
            // Stub: Replace with actual DB logic
            return new List<SalesTermsAndConditions>();
        }

        public async Task<int> CreateAsync(SalesTermsAndConditions termsAndConditions)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    var sql = @"
                        INSERT INTO sales_terms_and_conditions 
                        (user_created, date_created, user_updated, date_updated, taxes, 
                        freight_charges, delivery, payment, warranty, template_name, 
                        is_default, is_active, quotation_id)
                        VALUES 
                        (@UserCreated, @DateCreated, @UserUpdated, @DateUpdated, @Taxes, 
                        @FreightCharges, @Delivery, @Payment, @Warranty, @TemplateName, 
                        @IsDefault, @IsActive, @QuotationId)
                        RETURNING id";

                    termsAndConditions.DateCreated = DateTime.UtcNow;
                    termsAndConditions.DateUpdated = DateTime.UtcNow;
                    termsAndConditions.IsActive = true;
                    
                    if (termsAndConditions.IsDefault == true)
                    {
                        // Reset other defaults within the transaction
                        await connection.ExecuteAsync(
                            "UPDATE sales_terms_and_conditions SET is_default = false WHERE is_default = true",
                            transaction: transaction);
                    }

                    var id = await connection.ExecuteScalarAsync<int>(sql, termsAndConditions, transaction);
                    await transaction.CommitAsync();
                    
                    _logger.LogInformation("Created terms and conditions with ID {Id}", id);
                    return id;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error creating terms and conditions. Rolling back transaction.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating terms and conditions");
                throw;
            }
        }

        public async Task UpdateAsync(SalesTermsAndConditions termsAndConditions)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    var sql = @"
                        UPDATE sales_terms_and_conditions 
                        SET user_updated = @UserUpdated, 
                            date_updated = @DateUpdated,
                            taxes = @Taxes,
                            freight_charges = @FreightCharges,
                            delivery = @Delivery,
                            payment = @Payment,
                            warranty = @Warranty,
                            template_name = @TemplateName,
                            is_default = @IsDefault,
                            is_active = @IsActive,
                            quotation_id = @QuotationId
                        WHERE id = @Id";

                    termsAndConditions.DateUpdated = DateTime.UtcNow;

                    if (termsAndConditions.IsDefault == true)
                    {
                        // Reset other defaults within the transaction
                        await connection.ExecuteAsync(
                            "UPDATE sales_terms_and_conditions SET is_default = false WHERE is_default = true AND id != @Id",
                            new { termsAndConditions.Id },
                            transaction);
                    }

                    var rowsAffected = await connection.ExecuteAsync(sql, termsAndConditions, transaction);
                    if (rowsAffected == 0)
                    {
                        throw new KeyNotFoundException($"Terms and conditions with ID {termsAndConditions.Id} not found");
                    }

                    await transaction.CommitAsync();
                    _logger.LogInformation("Updated terms and conditions with ID {Id}", termsAndConditions.Id);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error updating terms and conditions. Rolling back transaction.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating terms and conditions");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var rowsAffected = await connection.ExecuteAsync(
                    "UPDATE sales_terms_and_conditions SET is_active = false WHERE id = @Id",
                    new { Id = id });
                
                if (rowsAffected == 0)
                {
                    throw new KeyNotFoundException($"Terms and conditions with ID {id} not found");
                }
                
                _logger.LogInformation("Deleted terms and conditions with ID {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting terms and conditions with ID {Id}", id);
                throw;
            }
        }

        public async Task<SalesTermsAndConditions> GetDefaultTemplateAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var result = await connection.QueryFirstOrDefaultAsync<SalesTermsAndConditions>(
                    "SELECT * FROM sales_terms_and_conditions WHERE is_default = true AND is_active = true");
                
                if (result == null)
                    throw new KeyNotFoundException("No default terms and conditions template found");
                    
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default terms and conditions template");
                throw;
            }
        }
    }
}
