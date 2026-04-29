using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models.DTOs;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ERP.API.Services.CompanySetup
{
    public class CurrencyExchangeRateService
    {
        private readonly string _connectionString;

        public CurrencyExchangeRateService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task CreateAsync(CurrencyExchangeRateCreateDto dto)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "SELECT sp_insert_currency_exchange_rate(@CompanyId, @FromCurrencyId, @ToCurrencyId, @RateDate::date, @ExchangeRate, @EffectiveFromDate::date, @CreatedBy, @RateType, @RateSource, @EffectiveToDate::date)",
                dto);
        }

        public async Task UpdateAsync(CurrencyExchangeRateUpdateDto dto)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "SELECT sp_update_currency_exchange_rate(@ExchangeRateId, @ExchangeRate, @RateType, @RateSource, @EffectiveFromDate::date, @EffectiveToDate::date, @ModifiedBy)",
                dto);
        }

        public async Task<CurrencyExchangeRateDto?> GetByIdAsync(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<CurrencyExchangeRateDto>(
                "SELECT * FROM sp_get_currency_exchange_rate_by_id(@Id)", new { Id = id });
        }

        public async Task<IEnumerable<CurrencyExchangeRateDto>> GetByCompanyIdAsync(int companyId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QueryAsync<CurrencyExchangeRateDto>(
                "SELECT * FROM sp_get_currency_exchange_rate_by_companyid(@CompanyId)", new { CompanyId = companyId });
        }

        public async Task<IEnumerable<CurrencyExchangeRateDto>> GetAllAsync(bool onlyActive = false)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            return await conn.QueryAsync<CurrencyExchangeRateDto>(
                "SELECT * FROM sp_get_all_currency_exchange_rates(@OnlyActive)", new { OnlyActive = onlyActive });
        }

        public async Task DeleteAsync(CurrencyExchangeRateDeleteDto dto)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "SELECT sp_delete_currency_exchange_rate(@ExchangeRateId, @Username)",
                new { dto.ExchangeRateId, dto.Username });
        }
    }
}
