using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models.DTOs;
using Dapper;
using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ERP.API.Services
{
    public class AssignedToDropdownService : IAssignedToDropdownService
    {
        private readonly string _connectionString;
        public AssignedToDropdownService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

public async Task<IEnumerable<AssignedToDropdownDto>> GetAssignedToDropdownAsync(int userId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            // Define role groups
            var mdRole = "Managing Director";
            var adminRole = "Admin";
            var managerRoles = new[] { "Manager", "Marketing Coordinator", "Sales Coordinator" };
            var salesManagerRole = "Sales Manager";
            var salesManagerCanSee = new[] { "Territory Manager", "Area Manager", "Field Service Technician", "Sales Representative" };
            var allAuthorityRoles = new[] {
                mdRole, adminRole, "Manager", "Marketing Coordinator", "Sales Coordinator",
                salesManagerRole, "Territory Manager", "Area Manager", "Field Service Technician", "Sales Representative"
            };

            // Get all users in the same region with their roles (default)
            string sql = @"SELECT userid as UserId, username as Username, rolename as RoleName
                          FROM get_team_hierarchy_with_roles(@userId)";
            var allUsers = (await connection.QueryAsync<AssignedToDropdownDto>(sql, new { userId })).ToList();

            // Get current user's role
            var currentUser = allUsers.FirstOrDefault(u => u.UserId == userId);
            var currentRole = currentUser?.RoleName?.Trim() ?? string.Empty;

            IEnumerable<AssignedToDropdownDto> filtered;
            if (string.Equals(currentRole, mdRole, StringComparison.OrdinalIgnoreCase))
            {
                // MD sees all users with any authority role, across all regions
                string allUsersSql = @"SELECT u.userid as UserId, u.username as Username, r.rolename as RoleName
                                      FROM users u
                                      JOIN userroles ur ON u.userid = ur.userid
                                      JOIN roles r ON ur.roleid = r.roleid
                                      WHERE r.rolename IN ('Managing Director', 'Admin', 'Manager', 'Marketing Coordinator', 'Sales Coordinator', 'Sales Manager', 'Territory Manager', 'Area Manager', 'Field Service Technician', 'Sales Representative')";
                var allAuthorityUsers = await connection.QueryAsync<AssignedToDropdownDto>(allUsersSql);
                filtered = allAuthorityUsers;
            }
            else if (string.Equals(currentRole, adminRole, StringComparison.OrdinalIgnoreCase))
            {
                // Admin sees all except MD (in region)
                filtered = allUsers.Where(u => allAuthorityRoles.Any(r => string.Equals(u.RoleName, r, StringComparison.OrdinalIgnoreCase)) && !string.Equals(u.RoleName, mdRole, StringComparison.OrdinalIgnoreCase));
            }
            else if (managerRoles.Any(r => string.Equals(currentRole, r, StringComparison.OrdinalIgnoreCase)))
            {
                // Manager, Marketing Coordinator, Sales Coordinator see all except MD (in region)
                filtered = allUsers.Where(u => allAuthorityRoles.Any(r => string.Equals(u.RoleName, r, StringComparison.OrdinalIgnoreCase)) && !string.Equals(u.RoleName, mdRole, StringComparison.OrdinalIgnoreCase));
            }
            else if (string.Equals(currentRole, salesManagerRole, StringComparison.OrdinalIgnoreCase))
            {
                // Sales Manager sees only Territory Manager, Area Manager, Field Service Technician, Sales Representative (in region)
                filtered = allUsers.Where(u => salesManagerCanSee.Any(r => string.Equals(u.RoleName, r, StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                // Others: only themselves (or adjust as needed)
                filtered = allUsers.Where(u => u.UserId == userId);
            }

            return filtered.OrderBy(u => u.Username).ToList();
        }
    }
}
