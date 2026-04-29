using Dapper;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.API.Services.CompanySetup
{
    public class CsWarehouseService : ICsWarehouseService
    {
        private readonly string _connectionString;
        private readonly ILogger<CsWarehouseService> _logger;

        public CsWarehouseService(IConfiguration configuration, ILogger<CsWarehouseService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<CsWarehouseCreateResponseDto> CreateWarehouseAsync(CreateCsWarehouseDto createDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_company_id = createDto.CompanyId,
                    p_branch_id = createDto.BranchId,
                    p_warehouse_code = createDto.WarehouseCode,
                    p_warehouse_name = createDto.WarehouseName,
                    p_warehouse_address_line1 = createDto.WarehouseAddressLine1,
                    p_warehouse_address_line2 = createDto.WarehouseAddressLine2,
                    p_city = createDto.City,
                    p_state = createDto.State,
                    p_pincode = createDto.Pincode,
                    p_default_inventory_location_id = createDto.DefaultInventoryLocationId
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_create_cs_warehouse(@p_company_id, @p_branch_id, @p_warehouse_code, @p_warehouse_name, @p_warehouse_address_line1, @p_warehouse_address_line2, @p_city, @p_state, @p_pincode, @p_default_inventory_location_id)",
                    parameters);

                return new CsWarehouseCreateResponseDto
                {
                    Success = result.warehouse_id != null,
                    Message = result.message,
                    WarehouseId = result.warehouse_id,
                    WarehouseCode = result.warehouse_code
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating warehouse: {WarehouseName}", createDto.WarehouseName);
                return new CsWarehouseCreateResponseDto
                {
                    Success = false,
                    Message = $"Error creating warehouse: {ex.Message}"
                };
            }
        }

        public async Task<(bool Success, string Message)> UpdateWarehouseAsync(UpdateCsWarehouseDto updateDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_warehouse_id = updateDto.WarehouseId,
                    p_warehouse_name = updateDto.WarehouseName,
                    p_warehouse_code = updateDto.WarehouseCode,
                    p_warehouse_address_line1 = updateDto.WarehouseAddressLine1,
                    p_warehouse_address_line2 = updateDto.WarehouseAddressLine2,
                    p_city = updateDto.City,
                    p_state = updateDto.State,
                    p_pincode = updateDto.Pincode,
                    p_default_inventory_location_id = updateDto.DefaultInventoryLocationId
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_cs_warehouses_update(@p_warehouse_id, @p_warehouse_name, @p_warehouse_code, @p_warehouse_address_line1, @p_warehouse_address_line2, @p_city, @p_state, @p_pincode, @p_default_inventory_location_id)",
                    parameters);

                return (result.success, result.message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating warehouse: {WarehouseId}", updateDto.WarehouseId);
                return (false, $"Error updating warehouse: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteWarehouseAsync(int warehouseId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await connection.ExecuteAsync(
                    "CALL sp_cs_warehouse_delete(@p_warehouse_id)",
                    new { p_warehouse_id = warehouseId });

                return (true, "Warehouse deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting warehouse: {WarehouseId}", warehouseId);
                return (false, $"Error deleting warehouse: {ex.Message}");
            }
        }

        public async Task<CsWarehouseDto?> GetWarehouseByIdAsync(int warehouseId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var warehouse = await connection.QueryFirstOrDefaultAsync<CsWarehouseDto>(
                    "SELECT * FROM sp_cs_warehouse_get_by_id(@p_warehouse_id)",
                    new { p_warehouse_id = warehouseId });

                return warehouse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouse by ID: {WarehouseId}", warehouseId);
                return null;
            }
        }

        public async Task<IEnumerable<CsWarehouseDto>> GetWarehousesByCompanyAsync(int companyId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var warehouses = await connection.QueryAsync<CsWarehouseDto>(
                    "SELECT * FROM sp_cs_warehouses_get_by_company(@p_company_id)",
                    new { p_company_id = companyId });

                return warehouses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouses by company: {CompanyId}", companyId);
                return Enumerable.Empty<CsWarehouseDto>();
            }
        }

        public async Task<IEnumerable<CsWarehouseDto>> GetWarehousesByBranchAsync(int branchId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var warehouses = await connection.QueryAsync<CsWarehouseDto>(
                    "SELECT * FROM sp_cs_warehouses_get_by_branch(@p_branch_id)",
                    new { p_branch_id = branchId });

                return warehouses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouses by branch: {BranchId}", branchId);
                return Enumerable.Empty<CsWarehouseDto>();
            }
        }

        public async Task<IEnumerable<CsWarehouseDropdownDto>> GetWarehousesDropdownByCompanyAsync(int companyId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var warehouses = await connection.QueryAsync<CsWarehouseDropdownDto>(
                    "SELECT * FROM sp_cs_warehouses_get_dropdown_by_company(@p_company_id)",
                    new { p_company_id = companyId });

                return warehouses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouses dropdown by company: {CompanyId}", companyId);
                return Enumerable.Empty<CsWarehouseDropdownDto>();
            }
        }

        public async Task<IEnumerable<CsWarehouseDropdownDto>> GetWarehousesDropdownByBranchAsync(int branchId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var warehouses = await connection.QueryAsync<CsWarehouseDropdownDto>(
                    "SELECT * FROM sp_cs_warehouses_get_dropdown_by_branch(@p_branch_id)",
                    new { p_branch_id = branchId });

                return warehouses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouses dropdown by branch: {BranchId}", branchId);
                return Enumerable.Empty<CsWarehouseDropdownDto>();
            }
        }

        public async Task<IEnumerable<CsWarehouseDto>> GetAllWarehousesAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var warehouses = await connection.QueryAsync<CsWarehouseDto>(
                    "SELECT * FROM sp_getall_cs_warehouse()");

                return warehouses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all warehouses");
                return Enumerable.Empty<CsWarehouseDto>();
            }
        }
    }
}
