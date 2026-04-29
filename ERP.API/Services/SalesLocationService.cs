using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models;
using System.Data;

namespace ERP.API.Services
{
    public class SalesLocationService : BaseDataService<SalesLocation>
    {
        public SalesLocationService(string connectionString)
            : base(connectionString, "sales_locations") // Table name is not directly used but required for BaseDataService
        {
        }

        protected override string GenerateInsertQuery()
        {
            var properties = typeof(SalesLocation).GetProperties()
                .Where(p => p.Name.ToLower() != "rowid" && !p.PropertyType.IsClass);

            var columns = string.Join(", ", properties.Select(p => GetColumnName(p)));
            var parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));

            return $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters}) RETURNING rowid";
        }

        protected override string GenerateUpdateQuery()
        {
            var properties = typeof(SalesLocation).GetProperties()
                .Where(p => p.Name.ToLower() != "rowid" && !p.PropertyType.IsClass);

            var setClauses = string.Join(", ", properties.Select(p => $"{GetColumnName(p)} = @{p.Name}"));

            return $"UPDATE {_tableName} SET {setClauses} WHERE rowid = @RowId";
        }

        public async Task<IEnumerable<SalesLocation>> GetAllLocationsAsync()
        {
            using var connection = CreateConnection();

            var query = @"
                SELECT 
                    ROW_NUMBER() OVER () AS RowId,
                    sc.id AS CountryId, sc.name AS Country,
                    ss.id AS StateId, ss.name AS State,
                    st.id AS TerritoryId, st.name AS Territory, st.alias AS TerritoryAlias,
                    sd.id AS DistrictId, sd.name AS District,
                    sct.id AS CityId, sct.name AS City,
                    sa.id AS AreaId, sa.name AS Area,
                    p.id AS PincodeId, p.pincode AS Pincode
                FROM sales_countries sc
                LEFT JOIN sales_states ss ON ss.sales_countries_id = sc.id                
                LEFT JOIN sales_districts sd ON sd.sales_states_id = ss.id
                LEFT JOIN sales_cities sct ON sct.sales_districts_id = sd.id
                LEFT JOIN sales_areas sa ON sa.sales_cities_id = sct.id
                LEFT JOIN pincodes p ON p.sales_areas_id = sa.id
                LEFT JOIN sales_territories st ON st.id = sd.sales_territories_id";

            var locations = await connection.QueryAsync<SalesLocation>(query);
            return locations;
        }

        public async Task<SalesLocation> GetLocationByIdAsync(int id)
        {
            using var connection = CreateConnection();

            var query = @"
                SELECT 
                    ROW_NUMBER() OVER () AS RowId,
                    sc.id AS CountryId, sc.name AS Country,
                    ss.id AS StateId, ss.name AS State,
                    st.id AS TerritoryId, st.name AS Territory, st.alias AS TerritoryAlias,
                    sd.id AS DistrictId, sd.name AS District,
                    sct.id AS CityId, sct.name AS City,
                    sa.id AS AreaId, sa.name AS Area,
                    p.id AS PincodeId, p.pincode AS Pincode
                FROM sales_countries sc
                LEFT JOIN sales_states ss ON ss.sales_countries_id = sc.id                
                LEFT JOIN sales_districts sd ON sd.sales_states_id = ss.id
                LEFT JOIN sales_cities sct ON sct.sales_districts_id = sd.id
                LEFT JOIN sales_areas sa ON sa.sales_cities_id = sct.id
                LEFT JOIN pincodes p ON p.sales_areas_id = sa.id
                LEFT JOIN sales_territories st ON st.id = sd.sales_territories_id
                WHERE sc.id = @Id OR ss.id = @Id OR st.id = @Id OR sd.id = @Id OR sct.id = @Id OR sa.id = @Id OR p.id = @Id";            var location = await connection.QueryFirstOrDefaultAsync<SalesLocation>(query, new { Id = id });
            return location ?? new SalesLocation();
        }

        public async Task<IEnumerable<SalesLocation>> SearchLocationsAsync(string country,string state, string territory, string district, string city, string area, string pincode)
        {
            using var connection = CreateConnection();

            var query = @"
                SELECT 
    ROW_NUMBER() OVER () AS RowId,
    sc.id AS CountryId, sc.name AS Country,
    ss.id AS StateId, ss.name AS State,
    st.id AS TerritoryId, st.name AS Territory, st.alias AS TerritoryAlias,
    sd.id AS DistrictId, sd.name AS District,
    sct.id AS CityId, sct.name AS City,
    sa.id AS AreaId, sa.name AS Area,
    p.id AS PincodeId, p.pincode AS Pincode
FROM sales_countries sc
LEFT JOIN sales_states ss ON ss.sales_countries_id = sc.id                
LEFT JOIN sales_districts sd ON sd.sales_states_id = ss.id
LEFT JOIN sales_cities sct ON sct.sales_districts_id = sd.id
LEFT JOIN sales_areas sa ON sa.sales_cities_id = sct.id
LEFT JOIN pincodes p ON sa.pincodes_id = p.id
LEFT JOIN sales_territories st ON st.id = sd.sales_territories_id
WHERE
    (
        @Country IS NULL OR TRIM(@Country) = '' OR LOWER(sc.name) LIKE LOWER(@Country)
    )
    AND (
        @State IS NULL OR TRIM(@State) = '' OR LOWER(ss.name) LIKE LOWER(@State)
    )
    AND (
        @Territory IS NULL OR TRIM(@Territory) = '' OR LOWER(st.name) LIKE LOWER(@Territory)
    )
    AND (
        @District IS NULL OR TRIM(@District) = '' OR LOWER(sd.name) LIKE LOWER(@District)
    )
    AND (
        @City IS NULL OR TRIM(@City) = '' OR LOWER(sct.name) LIKE LOWER(@City)
    )
    AND (
        @Area IS NULL OR TRIM(@Area) = '' OR LOWER(sa.name) LIKE LOWER(@Area)
    )
    AND (
        @Pincode IS NULL OR TRIM(@Pincode) = '' OR LOWER(p.pincode) LIKE LOWER(@Pincode)
    )";

            var locations = await connection.QueryAsync<SalesLocation>(query, new { Country = $"%{country}%", State = $"%{state}%", Territory = $"%{territory}%", District = $"%{district}%", City = $"%{city}%", Area = $"%{area}%", Pincode = $"%{pincode}%" });
            return locations;
        }
        
    }
}
