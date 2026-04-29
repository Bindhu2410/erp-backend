using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.UserManagement.DTOs
{
    public class RoleDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public bool IsSystemRole { get; set; }
        public DateTime DateCreated { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedByUsername { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateRoleDto
    {
        [Required]
        [StringLength(50)]
        public string RoleName { get; set; }
        // Now expects permission names, not IDs
       public IEnumerable<string> PermissionNames { get; set; } = new List<string>();
        public string Description { get; set; }
        public bool IsSystemRole { get; set; } = false;
        public int? CreatedBy { get; set; }
    }

    public class UpdateRoleDto
    {
        [StringLength(50)]
        public string RoleName { get; set; }
        
        public string Description { get; set; }
        public bool? IsSystemRole { get; set; }
        public bool? IsActive { get; set; }
    }

    public class RoleListDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public bool IsSystemRole { get; set; }
        public DateTime DateCreated { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedByUsername { get; set; }
        public bool IsActive { get; set; }
        public long TotalCount { get; set; }
    }

    public class RoleStatisticsDto
    {
        public long TotalRoles { get; set; }
        public long ActiveRoles { get; set; }
        public long InactiveRoles { get; set; }
        public long SystemRoles { get; set; }
        public long CustomRoles { get; set; }
    }

    public class PaginatedRolesDto
    {
        public IEnumerable<RoleListDto> Roles { get; set; } = new List<RoleListDto>();
        public long TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
