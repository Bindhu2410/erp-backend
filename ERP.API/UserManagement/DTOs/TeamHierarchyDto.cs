using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.UserManagement.DTOs
{
    public class TeamHierarchyDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string? RoleName { get; set; }
        public string? Region { get; set; }
        public int? ParentUserId { get; set; }
        public string? ParentUsername { get; set; }
        public string? ParentRoleName { get; set; }
    }

    public class AddOrUpdateTeamHierarchyDto
    {
        public int UserId { get; set; }
        public int ParentUserId { get; set; }
        public int RoleId { get; set; }
        public string Region { get; set; }
        public int AssignedBy { get; set; }
    }
}