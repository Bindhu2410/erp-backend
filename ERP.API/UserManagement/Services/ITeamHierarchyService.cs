using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.UserManagement.DTOs;


namespace ERP.API.UserManagement.Services
{
    public interface ITeamHierarchyService
    {
        Task<string> AddOrUpdateTeamHierarchyAsync(AddOrUpdateTeamHierarchyDto dto);
        Task<string> DeleteTeamHierarchyAsync(int userId);
        Task<List<TeamHierarchyDto>> GetTeamHierarchyAsync();
        Task<TeamHierarchyDto?> GetTeamHierarchyByUserIdAsync(int userId);
    }
}