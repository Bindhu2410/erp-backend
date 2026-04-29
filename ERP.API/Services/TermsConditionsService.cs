using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ERP.API.Models;
using Microsoft.Extensions.Logging;

namespace ERP.API.Services
{
    public class TermsConditionsService : ITermsConditionsService
    {
        private readonly string _connectionString;
        private readonly ILogger<TermsConditionsService> _logger;

        public TermsConditionsService(
            IConfiguration configuration,
            ILogger<TermsConditionsService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                throw new ArgumentNullException(nameof(configuration), "DefaultConnection string is not configured");
            _logger = logger;
        }

        public async Task<IEnumerable<TermsConditions>> GetAllAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var sql = @"
                    SELECT tc.*, tcd.id, tcd.tc_id, tcd.sno, tcd.type, tcd.terms_and_conditions
                    FROM terms_conditions tc
                    LEFT JOIN terms_conditions_details tcd ON tc.id = tcd.tc_id
                    ORDER BY tc.id, tcd.sno";

                var termsDict = new Dictionary<int, TermsConditions>();

                await connection.QueryAsync<TermsConditions, TermsConditionsDetail, TermsConditions>(
                    sql,
                    (tc, detail) =>
                    {
                        if (!termsDict.TryGetValue(tc.Id, out var termsConditions))
                        {
                            termsConditions = tc;
                            termsConditions.Details = new List<TermsConditionsDetail>();
                            termsDict.Add(tc.Id, termsConditions);
                        }

                        if (detail != null)
                            termsConditions.Details.Add(detail);

                        return termsConditions;
                    },
                    splitOn: "id");

                return termsDict.Values;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all terms and conditions");
                throw;
            }
        }

        public async Task<TermsConditions> GetByIdAsync(int id)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var sql = @"
                    SELECT tc.*, tcd.id, tcd.tc_id, tcd.sno, tcd.type, tcd.terms_and_conditions
                    FROM terms_conditions tc
                    LEFT JOIN terms_conditions_details tcd ON tc.id = tcd.tc_id
                    WHERE tc.id = @Id
                    ORDER BY tcd.sno";

                TermsConditions? termsConditions = null;

                await connection.QueryAsync<TermsConditions, TermsConditionsDetail, TermsConditions>(
                    sql,
                    (tc, detail) =>
                    {
                        if (termsConditions == null)
                        {
                            termsConditions = tc;
                            termsConditions.Details = new List<TermsConditionsDetail>();
                        }

                        if (detail != null)
                            termsConditions.Details.Add(detail);

                        return termsConditions;
                    },
                    new { Id = id },
                    splitOn: "id");

                if (termsConditions == null)
                    throw new KeyNotFoundException($"Terms and conditions with ID {id} not found");

                return termsConditions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting terms and conditions by ID {Id}", id);
                throw;
            }
        }

        public async Task<int> CreateAsync(TermsConditions termsConditions)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    var sql = @"
                        INSERT INTO terms_conditions 
                        (user_created, date_created, module_name, template_name, template_description)
                        VALUES 
                        (@UserCreated, @DateCreated, @ModuleName, @TemplateName, @TemplateDescription)
                        RETURNING id";

                    termsConditions.DateCreated = DateTime.UtcNow;

                    var id = await connection.ExecuteScalarAsync<int>(sql, termsConditions, transaction);

                    if (termsConditions.Details != null && termsConditions.Details.Any())
                    {
                        var detailSql = @"
                            INSERT INTO terms_conditions_details 
                            (tc_id, sno, type, terms_and_conditions)
                            VALUES 
                            (@TcId, @Sno, @Type, @TermsAndConditions)";

                        foreach (var detail in termsConditions.Details)
                        {
                            detail.TcId = id;
                            await connection.ExecuteAsync(detailSql, detail, transaction);
                        }
                    }

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

        public async Task UpdateAsync(TermsConditions termsConditions)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    var sql = @"
                        UPDATE terms_conditions 
                        SET user_updated = @UserUpdated, 
                            date_updated = @DateUpdated,
                            module_name = @ModuleName,
                            template_name = @TemplateName,
                            template_description = @TemplateDescription
                        WHERE id = @Id";

                    termsConditions.DateUpdated = DateTime.UtcNow;

                    var rowsAffected = await connection.ExecuteAsync(sql, termsConditions, transaction);
                    if (rowsAffected == 0)
                        throw new KeyNotFoundException($"Terms and conditions with ID {termsConditions.Id} not found");

                    await connection.ExecuteAsync(
                        "DELETE FROM terms_conditions_details WHERE tc_id = @Id",
                        new { termsConditions.Id },
                        transaction);

                    if (termsConditions.Details != null && termsConditions.Details.Any())
                    {
                        var detailSql = @"
                            INSERT INTO terms_conditions_details 
                            (tc_id, sno, type, terms_and_conditions)
                            VALUES 
                            (@TcId, @Sno, @Type, @TermsAndConditions)";

                        foreach (var detail in termsConditions.Details)
                        {
                            detail.TcId = termsConditions.Id;
                            await connection.ExecuteAsync(detailSql, detail, transaction);
                        }
                    }

                    await transaction.CommitAsync();
                    _logger.LogInformation("Updated terms and conditions with ID {Id}", termsConditions.Id);
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
                    "DELETE FROM terms_conditions WHERE id = @Id", new { Id = id });
                
                if (rowsAffected == 0)
                    throw new KeyNotFoundException($"Terms and conditions with ID {id} not found");
                
                _logger.LogInformation("Deleted terms and conditions with ID {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting terms and conditions with ID {Id}", id);
                throw;
            }
        }
    }
}
