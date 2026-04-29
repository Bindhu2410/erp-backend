using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;
using Npgsql;

namespace ERP.API.Services.Implementation.CompanySetup
{
    public class CsInventoryLocationService : ICsInventoryLocationService
    {
        private readonly string _connectionString;

        public CsInventoryLocationService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");
        }

        public async Task<IEnumerable<CsInventoryLocationDto>> GetAllAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var results = await connection.QueryAsync<CsInventoryLocationDto>(
                "SELECT * FROM sp_getall_cs_inventory_locations()");

            return results;
        }

        public async Task<PagedResponse<CsInventoryLocationDto>> SearchAsync(CsInventoryLocationSearchDto searchDto)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            string searchText = !string.IsNullOrEmpty(searchDto.SearchText) ? $"%{searchDto.SearchText}%" : null;
            int offset = (searchDto.PageNumber - 1) * searchDto.PageSize;

            var results = await connection.QueryAsync<CsInventoryLocationDto>(@"
                SELECT 
                    l.location_id AS LocationId,
                    l.warehouse_id AS WarehouseId,
                    l.location_code AS LocationCode,
                    l.location_name AS LocationName,
                    l.location_category AS LocationCategory,
                    l.capacity_weight AS CapacityWeight,
                    l.capacity_weight_uom AS CapacityWeightUom,
                    l.capacity_volume AS CapacityVolume,
                    l.capacity_volume_uom AS CapacityVolumeUom,
                    l.capacity_item_count AS CapacityItemCount,
                    l.created_at AS CreatedAt,
                    l.updated_at AS UpdatedAt,
                    COUNT(*) OVER() AS TotalRecords
                FROM public.cs_inventory_locations l
                WHERE (@warehouseId IS NULL OR l.warehouse_id = @warehouseId)
                  AND (@searchText IS NULL OR (l.location_code ILIKE @searchText OR l.location_name ILIKE @searchText))
                ORDER BY l.location_code
                LIMIT @pageSize OFFSET @offset",
                new 
                {
                    warehouseId = searchDto.WarehouseId,
                    searchText = searchText,
                    pageSize = searchDto.PageSize,
                    offset = offset
                });

            var resultsList = results.ToList();
            var totalRecords = resultsList.Any() ? resultsList.First().TotalRecords : 0;

            return new PagedResponse<CsInventoryLocationDto>
            {
                Data = resultsList,
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize,
                TotalRecords = totalRecords,
                FilteredRecords = totalRecords
            };
        }

        public async Task<CsInventoryLocationDto> GetByIdAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var result = await connection.QueryFirstOrDefaultAsync<CsInventoryLocationDto>(
                "SELECT * FROM sp_get_cs_inventory_location_by_id(@p_location_id)",
                new { p_location_id = id });

            return result ?? throw new KeyNotFoundException($"Inventory location with ID {id} not found");
        }

        public async Task<CsInventoryLocationDto> CreateAsync(CsInventoryLocationDto locationDto)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var locationId = await connection.QueryFirstAsync<int>(
                "SELECT sp_create_cs_inventory_location(@p_warehouse_id, @p_location_code, @p_location_name, @p_location_category, @p_capacity_weight, @p_capacity_weight_uom, @p_capacity_volume, @p_capacity_volume_uom, @p_capacity_item_count)",
                new
                {
                    p_warehouse_id = locationDto.WarehouseId,
                    p_location_code = locationDto.LocationCode,
                    p_location_name = locationDto.LocationName,
                    p_location_category = locationDto.LocationCategory,
                    p_capacity_weight = locationDto.CapacityWeight,
                    p_capacity_weight_uom = locationDto.CapacityWeightUom,
                    p_capacity_volume = locationDto.CapacityVolume,
                    p_capacity_volume_uom = locationDto.CapacityVolumeUom,
                    p_capacity_item_count = locationDto.CapacityItemCount
                });

            // Return the created record
            return await GetByIdAsync(locationId);
        }

        public async Task UpdateAsync(CsInventoryLocationDto locationDto)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await connection.ExecuteAsync(
                "SELECT sp_update_cs_inventory_location(@p_location_id, @p_warehouse_id, @p_location_code, @p_location_name, @p_location_category, @p_capacity_weight, @p_capacity_weight_uom, @p_capacity_volume, @p_capacity_volume_uom, @p_capacity_item_count)",
                new
                {
                    p_location_id = locationDto.LocationId,
                    p_warehouse_id = locationDto.WarehouseId,
                    p_location_code = locationDto.LocationCode,
                    p_location_name = locationDto.LocationName,
                    p_location_category = locationDto.LocationCategory,
                    p_capacity_weight = locationDto.CapacityWeight,
                    p_capacity_weight_uom = locationDto.CapacityWeightUom,
                    p_capacity_volume = locationDto.CapacityVolume,
                    p_capacity_volume_uom = locationDto.CapacityVolumeUom,
                    p_capacity_item_count = locationDto.CapacityItemCount
                });
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await connection.ExecuteAsync(
                "SELECT sp_delete_cs_inventory_location(@p_location_id)",
                new { p_location_id = id });
        }
    }
}
