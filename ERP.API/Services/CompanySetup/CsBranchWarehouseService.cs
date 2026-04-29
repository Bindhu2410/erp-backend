using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models.DTOs.CompanySetup;
using Microsoft.Extensions.Logging;

namespace ERP.API.Services.CompanySetup
{
    public class CsBranchWarehouseService : ICsBranchWarehouseService
    {
        private readonly string _connectionString;
        private readonly ILogger<CsBranchWarehouseService> _logger;

        public CsBranchWarehouseService(string connectionString, ILogger<CsBranchWarehouseService> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<int> CreateBranchWarehouseAsync(CsBranchWarehouseDto createDto)
        {
            try
            {
                using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                var warehouseId = await connection.QuerySingleAsync<int>(
                    "SELECT sp_create_cs_branch_warehouse(@BranchId, @WarehouseCode, @Name, @Description, @AddressLine1, @AddressLine2, @City, @State, @Pincode, @ContactPerson, @ContactNumber, @Email, @IsActive)",
                    new
                    {
                        createDto.BranchId,
                        createDto.WarehouseCode,
                        createDto.Name,
                        createDto.Description,
                        createDto.AddressLine1,
                        createDto.AddressLine2,
                        createDto.City,
                        createDto.State,
                        createDto.Pincode,
                        createDto.ContactPerson,
                        createDto.ContactNumber,
                        createDto.Email,
                        createDto.IsActive
                    });
                return warehouseId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating branch warehouse");
                throw;
            }
        }

        public async Task<bool> UpdateBranchWarehouseAsync(CsBranchWarehouseDto updateDto)
        {
            try
            {
                using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                var result = await connection.QuerySingleAsync<bool>(
                    "SELECT sp_update_cs_branch_warehouse(@WarehouseId, @BranchId, @WarehouseCode, @Name, @Description, @AddressLine1, @AddressLine2, @City, @State, @Pincode, @ContactPerson, @ContactNumber, @Email, @IsActive)",
                    new
                    {
                        updateDto.WarehouseId,
                        updateDto.BranchId,
                        updateDto.WarehouseCode,
                        updateDto.Name,
                        updateDto.Description,
                        updateDto.AddressLine1,
                        updateDto.AddressLine2,
                        updateDto.City,
                        updateDto.State,
                        updateDto.Pincode,
                        updateDto.ContactPerson,
                        updateDto.ContactNumber,
                        updateDto.Email,
                        updateDto.IsActive
                    });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating branch warehouse");
                throw;
            }
        }

        public async Task<bool> DeleteBranchWarehouseAsync(int warehouseId)
        {
            try
            {
                using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                var result = await connection.QuerySingleAsync<bool>(
                    "SELECT sp_delete_cs_branch_warehouse(@WarehouseId)",
                    new { WarehouseId = warehouseId });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting branch warehouse");
                throw;
            }
        }

        public async Task<CsBranchWarehouseDto?> GetBranchWarehouseByIdAsync(int warehouseId)
        {
            try
            {
                using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                var warehouse = await connection.QueryFirstOrDefaultAsync<CsBranchWarehouseDto>(
                    "SELECT * FROM sp_get_cs_branch_warehouse_by_id(@WarehouseId)",
                    new { WarehouseId = warehouseId });
                return warehouse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branch warehouse by ID");
                throw;
            }
        }

        public async Task<IEnumerable<CsBranchWarehouseDto>> GetBranchWarehousesByBranchAsync(int branchId)
        {
            try
            {
                using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                var warehouses = await connection.QueryAsync<CsBranchWarehouseDto>(
                    @"SELECT 
                        warehouse_id AS WarehouseId, 
                        branch_id AS BranchId, 
                        warehouse_code AS WarehouseCode, 
                        name AS Name, 
                        description AS Description, 
                        address_line1 AS AddressLine1, 
                        address_line2 AS AddressLine2, 
                        city AS City, 
                        state AS State, 
                        pincode AS Pincode, 
                        contact_person AS ContactPerson, 
                        contact_number AS ContactNumber, 
                        email AS Email, 
                        is_active AS IsActive,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt
                    FROM cs_branch_warehouses 
                    WHERE branch_id = @BranchId",
                    new { BranchId = branchId });
                return warehouses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branch warehouses by branch");
                throw;
            }
        }

        public async Task<IEnumerable<CsBranchWarehouseDto>> GetBranchWarehousesDropdownAsync(int branchId)
        {
            try
            {
                using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                var warehouses = await connection.QueryAsync<CsBranchWarehouseDto>(
                    @"SELECT 
                        warehouse_id AS WarehouseId, 
                        branch_id AS BranchId, 
                        warehouse_code AS WarehouseCode, 
                        name AS Name, 
                        description AS Description, 
                        address_line1 AS AddressLine1, 
                        address_line2 AS AddressLine2, 
                        city AS City, 
                        state AS State, 
                        pincode AS Pincode, 
                        contact_person AS ContactPerson, 
                        contact_number AS ContactNumber, 
                        email AS Email, 
                        is_active AS IsActive,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt
                    FROM cs_branch_warehouses 
                    WHERE branch_id = @BranchId AND is_active = true",
                    new { BranchId = branchId });
                return warehouses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branch warehouses dropdown");
                throw;
            }
        }
    }
}
