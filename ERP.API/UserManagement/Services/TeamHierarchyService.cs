using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using ERP.API.UserManagement.DTOs;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ERP.API.UserManagement.Services
{
    public class TeamHierarchyService : ITeamHierarchyService
    {
        private readonly string _connectionString;
        private readonly ILogger<TeamHierarchyService> _logger;

        public TeamHierarchyService(IConfiguration configuration, ILogger<TeamHierarchyService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Default connection string not found");
            _logger = logger;
        }

        public async Task<string> AddOrUpdateTeamHierarchyAsync(AddOrUpdateTeamHierarchyDto dto)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                await using var command = new NpgsqlCommand(
                    "SELECT public.sp_um_add_or_update_team_hierarchy(@p_userid, @p_parent_userid, @p_roleid, @p_region, @p_assignedby)", connection);

                command.Parameters.AddWithValue("p_userid", dto.UserId);
                command.Parameters.AddWithValue("p_parent_userid", dto.ParentUserId);
                command.Parameters.AddWithValue("p_roleid", dto.RoleId);
                command.Parameters.AddWithValue("p_region", dto.Region ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_assignedby", dto.AssignedBy);

                var result = await command.ExecuteScalarAsync();
                return result?.ToString() ?? "No response from database.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding/updating team hierarchy");
                return "Error adding/updating team hierarchy.";
            }
        }

        public async Task<string> DeleteTeamHierarchyAsync(int userId)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                await using var command = new NpgsqlCommand(
                    "SELECT public.sp_um_delete_team_hierarchy(@p_userid)", connection);

                command.Parameters.AddWithValue("p_userid", userId);

                var result = await command.ExecuteScalarAsync();
                return result?.ToString() ?? "No response from database.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting team hierarchy");
                return "Error deleting team hierarchy.";
            }
        }

        public async Task<List<TeamHierarchyDto>> GetTeamHierarchyAsync()
        {
            var list = new List<TeamHierarchyDto>();
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                await using var command = new NpgsqlCommand("SELECT * FROM sp_um_get_team_hierarchy()", connection);
                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new TeamHierarchyDto
                    {
                        UserId = reader.GetInt32(reader.GetOrdinal("userid")),
                        Username = reader.GetString(reader.GetOrdinal("username")),
                        RoleName = reader.IsDBNull(reader.GetOrdinal("rolename")) ? null : reader.GetString(reader.GetOrdinal("rolename")),
                        Region = reader.IsDBNull(reader.GetOrdinal("region")) ? null : reader.GetString(reader.GetOrdinal("region")),
                        ParentUserId = reader.IsDBNull(reader.GetOrdinal("parent_userid")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("parent_userid")),
                        ParentUsername = reader.IsDBNull(reader.GetOrdinal("parent_username")) ? null : reader.GetString(reader.GetOrdinal("parent_username")),
                        ParentRoleName = reader.IsDBNull(reader.GetOrdinal("parent_rolename")) ? null : reader.GetString(reader.GetOrdinal("parent_rolename"))
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team hierarchy");
            }
            return list;
        }

        public async Task<TeamHierarchyDto?> GetTeamHierarchyByUserIdAsync(int userId)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                await using var command = new NpgsqlCommand("SELECT * FROM sp_um_get_team_hierarchy_by_userid(@p_userid)", connection);
                command.Parameters.AddWithValue("p_userid", userId);

                await using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new TeamHierarchyDto
                    {
                        UserId = reader.GetInt32(reader.GetOrdinal("userid")),
                        Username = reader.GetString(reader.GetOrdinal("username")),
                        RoleName = reader.IsDBNull(reader.GetOrdinal("rolename")) ? null : reader.GetString(reader.GetOrdinal("rolename")),
                        Region = reader.IsDBNull(reader.GetOrdinal("region")) ? null : reader.GetString(reader.GetOrdinal("region")),
                        ParentUserId = reader.IsDBNull(reader.GetOrdinal("parent_userid")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("parent_userid")),
                        ParentUsername = reader.IsDBNull(reader.GetOrdinal("parent_username")) ? null : reader.GetString(reader.GetOrdinal("parent_username")),
                        ParentRoleName = reader.IsDBNull(reader.GetOrdinal("parent_rolename")) ? null : reader.GetString(reader.GetOrdinal("parent_rolename"))
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team hierarchy by user id");
            }
            return null;
        }
    }
}