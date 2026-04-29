using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.Linq;
using Dapper;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ERP.API.Services.CompanySetup;

namespace ERP.API.Services.Implementation.CompanySetup
{
    public class CsIntercompanyRelationshipService : ICsIntercompanyRelationshipService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public CsIntercompanyRelationshipService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<CsIntercompanyRelationship?> GetByIdAsync(int relationshipId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<CsIntercompanyRelationship>(
                "CALL cs_get_intercompany_relationship_by_id(@RelationshipId);",
                new { RelationshipId = relationshipId });
        }

        public async Task<(IEnumerable<CsIntercompanyRelationship> Data, int TotalRecords, int FilteredRecords)> SearchAsync(CsIntercompanyRelationshipSearchDto searchDto)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", searchDto.CompanyId);
            parameters.Add("@SearchText", searchDto.SearchText);
            parameters.Add("@RelationshipType", searchDto.RelationshipType);
            parameters.Add("@EffectiveDate", searchDto.EffectiveDate);
            parameters.Add("@ActiveOnly", searchDto.ActiveOnly);

            using var multi = await connection.QueryMultipleAsync(
                "CALL cs_search_intercompany_relationships(@CompanyId, @SearchText, @RelationshipType, @EffectiveDate, @ActiveOnly);",
                parameters);

            var results = await multi.ReadAsync<CsIntercompanyRelationship>();
            var counts = await multi.ReadFirstAsync<(int TotalRecords, int FilteredRecords)>();
            
            return (results, counts.TotalRecords, counts.FilteredRecords);
        }

        public async Task<IEnumerable<CsIntercompanyRelationship>> GetByCompanyAsync(int companyId, bool activeOnly = true)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryAsync<CsIntercompanyRelationship>(
                "CALL cs_get_intercompany_relationships_by_company(@CompanyId, @ActiveOnly);",
                new { CompanyId = companyId, ActiveOnly = activeOnly });
        }

        public async Task<int> CreateAsync(CsIntercompanyRelationship relationship)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId1", relationship.CompanyId1);
            parameters.Add("@CompanyId2", relationship.CompanyId2);
            parameters.Add("@RelationshipType", relationship.RelationshipType);
            parameters.Add("@EffectiveDate", relationship.EffectiveDate);
            parameters.Add("@EndDate", relationship.EndDate);
            parameters.Add("@Notes", relationship.Notes);

            return await connection.ExecuteScalarAsync<int>(
                "CALL cs_create_intercompany_relationship(@CompanyId1, @CompanyId2, @RelationshipType, @EffectiveDate, @EndDate, @Notes);",
                parameters);
        }

        public async Task<bool> UpdateAsync(CsIntercompanyRelationship relationship)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@RelationshipId", relationship.RelationshipId);
            parameters.Add("@CompanyId1", relationship.CompanyId1);
            parameters.Add("@CompanyId2", relationship.CompanyId2);
            parameters.Add("@RelationshipType", relationship.RelationshipType);
            parameters.Add("@EffectiveDate", relationship.EffectiveDate);
            parameters.Add("@EndDate", relationship.EndDate);
            parameters.Add("@Notes", relationship.Notes);

            var rowsAffected = await connection.ExecuteAsync(
                "CALL cs_update_intercompany_relationship(@RelationshipId, @CompanyId1, @CompanyId2, @RelationshipType, @EffectiveDate, @EndDate, @Notes);",
                parameters);

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int relationshipId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(
                "CALL cs_delete_intercompany_relationship(@RelationshipId);",
                new { RelationshipId = relationshipId });

            return rowsAffected > 0;
        }
    }
}
